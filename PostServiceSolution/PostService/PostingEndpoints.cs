using PostService.BusinessLogic;
using PostService.Dtos;
using PostService.Mappings;

namespace PostService;

/// <summary>Документовані обробники REST API поштових відправлень.</summary>
internal static class PostingEndpoints
{
    /// <summary>Перевірити доступність PostService</summary>
    /// <remarks>Повертає текстове повідомлення про доступність API. Вхідних параметрів немає.</remarks>
    /// <response code="200">Сервіс доступний.</response>
    internal static IResult Status() => Results.Text("Це API поштового клієнта.");

    /// <summary>Отримати список поштових відправлень</summary>
    /// <remarks>Повертає всі збережені відправлення за зростанням Id. Вхідних параметрів немає. Порожня база повертає порожній масив JSON.</remarks>
    /// <param name="service">Сервіс бізнес логіки, надається контейнером залежностей.</param>
    /// <response code="200">Список поштових відправлень.</response>
    internal static IResult GetAll(IPostingService service) =>
        Results.Ok(service.GetAll().Select(PostingMapper.ToPostingGetDto).ToArray());

    /// <summary>Отримати відправлення за номером</summary>
    /// <remarks>Завантажує повні дані одного відправлення. Тіло запиту не потрібне.</remarks>
    /// <param name="id" example="1">Унікальний цілий номер поштового відправлення в адресі запиту.</param>
    /// <param name="service">Сервіс бізнес логіки, надається контейнером залежностей.</param>
    /// <response code="200">Повні дані знайденого відправлення.</response>
    /// <response code="404">Відправлення з указаним номером не існує.</response>
    internal static IResult GetById(int id, IPostingService service)
    {
        var posting = service.Find(id);
        return posting is null ? Results.NotFound() : Results.Ok(PostingMapper.ToPostingGetDto(posting));
    }

    /// <summary>Створити поштове відправлення</summary>
    /// <remarks>Приймає PostingPostDto у JSON. Сервер призначає номер, дату створення в UTC та розраховує ціну. Повертає створений запис і заголовок Location.</remarks>
    /// <param name="dto">Тіло JSON з відправником, отримувачем, вмістом, доставкою, вагою, габаритами та необов’язковими цінністю й описом.</param>
    /// <param name="service">Сервіс бізнес логіки, надається контейнером залежностей.</param>
    /// <response code="201">Відправлення створено; тіло містить PostingGetDto.</response>
    /// <response code="400">Некоректний JSON або порушення правил полів; при бізнес помилці повертається ValidationProblem.</response>
    internal static IResult Create(PostingPostDto dto, IPostingService service)
    {
        var saved = service.Create(PostingMapper.ToPosting(dto));
        var result = PostingMapper.ToPostingGetDto(saved);
        return Results.Created($"/postings/{result.Id}", result);
    }

    /// <summary>Оновити поштове відправлення</summary>
    /// <remarks>Повністю замінює змінювані поля запису даними PostingPutDto. Id у JSON має збігатися з id у маршруті. Дата створення зберігається. Ціну передає клієнт, автоматичний перерахунок під час PUT не виконується.</remarks>
    /// <param name="id" example="1">Цілий номер існуючого відправлення в адресі запиту.</param>
    /// <param name="dto">Повний набір полів для оновлення, включно з Id і невід’ємною ціною у гривнях.</param>
    /// <param name="service">Сервіс бізнес логіки, надається контейнером залежностей.</param>
    /// <response code="200">Оновлене відправлення у форматі PostingGetDto.</response>
    /// <response code="400">Некоректні поля, JSON або невідповідність Id у маршруті й тілі.</response>
    /// <response code="404">Відправлення не існує.</response>
    internal static IResult Update(int id, PostingPutDto dto, IPostingService service)
    {
        var updated = service.Update(id, PostingMapper.ToPosting(dto));
        return updated is null ? Results.NotFound() : Results.Ok(PostingMapper.ToPostingGetDto(updated));
    }

    /// <summary>Видалити поштове відправлення</summary>
    /// <remarks>Видаляє відправлення з бази. Тіло запиту не потрібне. Успішна відповідь 204 не містить тіла.</remarks>
    /// <param name="id" example="1">Цілий номер відправлення, яке потрібно видалити.</param>
    /// <param name="service">Сервіс бізнес логіки, надається контейнером залежностей.</param>
    /// <response code="204">Відправлення видалено.</response>
    /// <response code="404">Відправлення відсутнє або вже видалене.</response>
    internal static IResult Delete(int id, IPostingService service) =>
        service.Delete(id) == 0 ? Results.NotFound() : Results.NoContent();
}
