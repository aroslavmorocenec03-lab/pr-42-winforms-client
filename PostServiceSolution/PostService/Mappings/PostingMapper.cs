using PostService.Dtos;
using PostService.Models;

namespace PostService.Mappings;

/// <summary>Перетворення між моделлю відправлення та контрактами REST API.</summary>
public static class PostingMapper
{
    /// <summary>Створює модель із контракту POST.</summary>
    /// <param name="dto">Вхідні дані для перетворення.</param>
    /// <returns>Нова модель без серверних реквізитів.</returns>
    public static Posting ToPosting(PostingPostDto dto) => new()
    {
        From = dto.From, To = dto.To, Content = dto.Content, DeliveryType = dto.DeliveryType,
        Weight = dto.Weight, Width = dto.Width, Height = dto.Height, Depth = dto.Depth, Value = dto.Value, Description = dto.Description
    };

    /// <summary>Створює модель із контракту PUT.</summary>
    /// <param name="dto">Вхідні дані для перетворення.</param>
    /// <returns>Модель із полями для оновлення.</returns>
    public static Posting ToPosting(PostingPutDto dto) => new()
    {
        Id = dto.Id, From = dto.From, To = dto.To, Content = dto.Content, DeliveryType = dto.DeliveryType,
        Weight = dto.Weight, Width = dto.Width, Height = dto.Height, Depth = dto.Depth, Value = dto.Value, Description = dto.Description, Price = dto.Price
    };

    /// <summary>Створює контракт відповіді зі збереженої моделі.</summary>
    /// <param name="posting">Вхідні дані для перетворення.</param>
    /// <returns>Повні дані відправлення для клієнта.</returns>
    public static PostingGetDto ToPostingGetDto(Posting posting) => new()
    {
        Id = posting.Id, From = posting.From, To = posting.To, Content = posting.Content,
        DeliveryType = posting.DeliveryType, Weight = posting.Weight,
        Width = posting.Width, Height = posting.Height, Depth = posting.Depth,
        Value = posting.Value, Description = posting.Description, Price = posting.Price, CreatedAt = posting.CreatedAt
    };
}
