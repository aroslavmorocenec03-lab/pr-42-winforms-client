using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using PostService.Dtos;

namespace PostService.App;

public sealed class PostServiceApi : IDisposable
{
    private readonly HttpClient _http;
    private readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };
    public PostServiceApi(string baseUrl = "http://localhost:5211/") =>
        _http = new HttpClient { BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/"), Timeout = TimeSpan.FromSeconds(10) };

    public async Task<PostingGetDto[]> GetAllAsync(CancellationToken token = default)
    {
        using var response = await _http.GetAsync("postings", token);
        await EnsureSuccessAsync(response, token);
        return await response.Content.ReadFromJsonAsync<PostingGetDto[]>(_json, token) ?? throw new JsonException("Порожня відповідь.");
    }
    public async Task<PostingGetDto> GetAsync(int id, CancellationToken token = default)
    {
        using var response = await _http.GetAsync($"postings/{id}", token);
        return await ReadPostingAsync(response, token);
    }
    public async Task<PostingGetDto> CreateAsync(PostingPostDto posting, CancellationToken token = default)
    {
        using var response = await _http.PostAsJsonAsync("postings", posting, _json, token);
        return await ReadPostingAsync(response, token);
    }
    public async Task<PostingGetDto> UpdateAsync(PostingPutDto posting, CancellationToken token = default)
    {
        using var response = await _http.PutAsJsonAsync($"postings/{posting.Id}", posting, _json, token);
        return await ReadPostingAsync(response, token);
    }
    public async Task DeleteAsync(int id, CancellationToken token = default)
    {
        using var response = await _http.DeleteAsync($"postings/{id}", token);
        await EnsureSuccessAsync(response, token);
    }
    private async Task<PostingGetDto> ReadPostingAsync(HttpResponseMessage response, CancellationToken token)
    {
        await EnsureSuccessAsync(response, token);
        return await response.Content.ReadFromJsonAsync<PostingGetDto>(_json, token) ?? throw new JsonException("Порожня відповідь.");
    }
    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken token)
    {
        if (response.IsSuccessStatusCode) return;
        var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            try
            {
                using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync(token));
                if (doc.RootElement.TryGetProperty("errors", out var fields))
                    foreach (var field in fields.EnumerateObject())
                        errors[field.Name] = field.Value.EnumerateArray().Select(x => x.GetString() ?? "Некоректне значення.").ToArray();
            }
            catch (JsonException) { }
        }
        var message = response.StatusCode switch
        {
            HttpStatusCode.NotFound => "Відправлення вже відсутнє на сервері. Закрийте діалог і оновіть список.",
            HttpStatusCode.BadRequest => "Сервер відхилив дані. Перевірте значення полів.",
            _ => $"Не вдалося виконати операцію. Відповідь сервера: {(int)response.StatusCode}."
        };
        throw new PostServiceApiException(response.StatusCode, message, errors);
    }
    public void Dispose() => _http.Dispose();
}

public sealed class PostServiceApiException(HttpStatusCode statusCode, string message, Dictionary<string, string[]> errors) : Exception(message)
{
    public HttpStatusCode StatusCode { get; } = statusCode;
    public IReadOnlyDictionary<string, string[]> Errors { get; } = errors;
}

internal static class OperationError
{
    public static string Describe(Exception error, bool saving = false) => error switch
    {
        PostServiceApiException api => api.Message,
        OperationCanceledException => saving
            ? "Сервер не відповів вчасно. Збереження могло відбутися. Закрийте діалог і перевірте список перед повторною спробою."
            : "Сервер не відповів вчасно. Натисніть «Оновити», щоб перевірити актуальні дані.",
        HttpRequestException => saving
            ? "З’єднання перервано. Перевірте список перед повторним збереженням, щоб не створити дублікат."
            : "Немає з’єднання з PostService. Перевірте запуск сервера і натисніть «Оновити».",
        JsonException => "Невідомий формат відповіді. Перевірте сумісність клієнта й PostService та оновіть список.",
        _ => "Не вдалося виконати операцію. Спробуйте оновити список."
    };
}
