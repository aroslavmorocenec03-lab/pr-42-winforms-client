using PostService.BusinessLogic;

namespace PostService.Infrastructure;

// HTTP-рівень лише перетворює відхилення сервісу на відповідь 400.
/// <summary>Перетворює помилки бізнес перевірки на HTTP 400 ValidationProblem.</summary>
/// <param name="next">Наступний обробник HTTP запиту.</param>
public class BusinessExceptionMiddleware(RequestDelegate next)
{
    /// <summary>Виконує наступний обробник і формує відповідь при помилці валідації.</summary>
    /// <param name="context">Контекст поточного HTTP запиту.</param>
    /// <returns>Завдання завершення обробки запиту.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        try { await next(context); }
        catch (PostingValidationException exception)
        {
            await Results.ValidationProblem(exception.Errors).ExecuteAsync(context);
        }
    }
}
