using PostService.CommonTypes;

namespace PostService.Dtos;

/// <summary>Повні дані збереженого поштового відправлення у відповіді сервера.</summary>
public class PostingGetDto
{
    /// <summary>Унікальний номер відправлення, призначений сервером.</summary>
    /// <example>1</example>
    public int Id { get; set; }
    /// <summary>Відправник або адреса відправлення. Обов’язковий непорожній рядок.</summary>
    /// <example>Київ, відділення № 3</example>
    public string From { get; set; } = string.Empty;
    /// <summary>Отримувач або адреса доставки. Обов’язковий непорожній рядок.</summary>
    /// <example>Львів, відділення № 7</example>
    public string To { get; set; } = string.Empty;
    /// <summary>Вміст відправлення. Обов’язковий непорожній рядок.</summary>
    /// <example>Книги</example>
    public string Content { get; set; } = string.Empty;
    /// <summary>Спосіб доставки: Department — у відділення; Courier — кур’єром; ExpressCourier — експрес кур’єром.</summary>
    /// <example>Department</example>
    public DeliveryType DeliveryType { get; set; }
    /// <summary>Вага в кілограмах. Скінченне число більше нуля та не більше 1000000.</summary>
    /// <example>2.5</example>
    public float Weight { get; set; }
    /// <summary>Ширина в сантиметрах. Скінченне число більше нуля.</summary>
    /// <example>20.5</example>
    public float Width { get; set; }
    /// <summary>Висота в сантиметрах. Скінченне число більше нуля.</summary>
    /// <example>15.25</example>
    public float Height { get; set; }
    /// <summary>Глибина в сантиметрах. Скінченне число більше нуля.</summary>
    /// <example>3</example>
    public float Depth { get; set; }
    /// <summary>Оголошена цінність у гривнях. Необов’язкове невід’ємне скінченне число; null — не вказана.</summary>
    /// <example>100.5</example>
    public float? Value { get; set; }
    /// <summary>Додатковий опис або вказівки щодо обробки. Необов’язковий рядок, допускає null.</summary>
    /// <example>Не згинати</example>
    public string? Description { get; set; }
    /// <summary>Вартість доставки у гривнях. При створенні розраховується сервером; у PUT задається невід’ємним скінченним числом.</summary>
    /// <example>65</example>
    public float Price { get; set; }
    /// <summary>Дата та час створення в UTC у форматі ISO 8601. Визначає сервер; під час редагування не змінюється.</summary>
    /// <example>2026-11-12T12:00:00Z</example>
    public DateTime CreatedAt { get; set; }
}
