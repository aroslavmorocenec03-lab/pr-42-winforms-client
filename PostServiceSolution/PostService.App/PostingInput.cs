using System.Globalization;
using PostService.CommonTypes;
using PostService.Dtos;

namespace PostService.App;

// Обмеження відповідають PostingService.Validate. Сервер перевіряє дані повторно.
public static class PostingInput
{
    public static Dictionary<string, string> Validate(IReadOnlyDictionary<string, string> fields,
        DeliveryType? deliveryType, bool editing, out PostingPostDto create, out PostingPutDto update)
    {
        var errors = new Dictionary<string, string>();
        string Text(string key) => fields.GetValueOrDefault(key, "").Trim();
        foreach (var key in new[] { "From", "To", "Content" })
            if (Text(key).Length == 0) errors[key] = "Заповніть обов’язкове поле.";
        if (deliveryType is null || !Enum.IsDefined(deliveryType.Value)) errors["DeliveryType"] = "Виберіть тип доставки.";
        float Number(string key, bool positive, bool optional = false)
        {
            var text = Text(key);
            if (optional && text.Length == 0) return 0;
            if (!float.TryParse(text.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out var value) || !float.IsFinite(value))
            {
                errors[key] = "Введіть скінченне число через кому або крапку.";
                return 0;
            }
            if (positive ? value <= 0 : value < 0)
                errors[key] = positive ? "Значення має бути більшим за нуль." : "Значення не може бути від’ємним.";
            if (key == "Weight" && value > 1_000_000) errors[key] = "Вага не може перевищувати 1 000 000 кг.";
            return value;
        }
        var weight = Number("Weight", true); var width = Number("Width", true);
        var height = Number("Height", true); var depth = Number("Depth", true);
        var value = Number("Value", false, true); var price = editing ? Number("Price", false) : 0;
        create = new PostingPostDto
        {
            From = Text("From"), To = Text("To"), Content = Text("Content"), DeliveryType = deliveryType ?? DeliveryType.Department,
            Weight = weight, Width = width, Height = height, Depth = depth,
            Value = Text("Value").Length == 0 ? null : value,
            Description = Text("Description").Length == 0 ? null : Text("Description")
        };
        update = new PostingPutDto
        {
            From = create.From, To = create.To, Content = create.Content, DeliveryType = create.DeliveryType,
            Weight = weight, Width = width, Height = height, Depth = depth, Value = create.Value, Description = create.Description, Price = price
        };
        return errors;
    }
}
