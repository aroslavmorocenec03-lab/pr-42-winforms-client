using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using PostService.CommonTypes;
using PostService.Dtos;

namespace PostService.App;

public partial class PostingList : Form
{
    private readonly HttpClient _httpClient = new()
    {
        BaseAddress = new Uri("http://localhost:5211/"),
        Timeout = TimeSpan.FromSeconds(10)
    };
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };
    private readonly CancellationTokenSource _lifetime = new();
    private bool _busy;
    private bool _resourcesDisposed;

    public PostingList()
    {
        InitializeComponent();
        gridPostings.AutoGenerateColumns = false;
        UpdateSelection();
    }

    private async void PostingList_Load(object? sender, EventArgs e) => await LoadPostingsAsync();

    private async void btnRefresh_Click(object? sender, EventArgs e)
    {
        if (!_busy) await LoadPostingsAsync();
    }

    private async Task LoadPostingsAsync()
    {
        int? selectedId = (gridPostings.CurrentRow?.DataBoundItem as PostingGetDto)?.Id;
        SetBusy(true);
        lblStatus.Text = "Завантаження відправлень…";
        try
        {
            var postings = await _httpClient.GetFromJsonAsync<PostingGetDto[]>("postings", _jsonOptions, _lifetime.Token);
            if (IsDisposed || Disposing) return;
            gridPostings.DataSource = postings ?? Array.Empty<PostingGetDto>();
            gridPostings.ClearSelection();
            gridPostings.CurrentCell = null;
            if (selectedId is int id)
            {
                foreach (DataGridViewRow row in gridPostings.Rows)
                    if (row.DataBoundItem is PostingGetDto p && p.Id == id)
                    {
                        gridPostings.CurrentCell = row.Cells["colFrom"];
                        row.Selected = true;
                        break;
                    }
            }
            lblStatus.Text = $"Відправлень: {postings?.Length ?? 0} · Оновлено о {DateTime.Now:HH:mm:ss}";
        }
        catch (OperationCanceledException) when (_lifetime.IsCancellationRequested) { }
        catch (OperationCanceledException)
        {
            ShowLoadError("Сервер не відповів вчасно. Спробуйте натиснути «Оновити».");
        }
        catch (HttpRequestException)
        {
            ShowLoadError("Не вдалося отримати дані з сервера. Перевірте, чи запущено PostService на http://localhost:5211.");
        }
        catch (JsonException)
        {
            ShowLoadError("Сервер повернув дані невідомого формату. Перевірте сумісність версій клієнта й сервера.");
        }
        finally
        {
            if (!IsDisposed && !Disposing) SetBusy(false);
        }
    }

    private async void btnDelete_Click(object? sender, EventArgs e)
    {
        if (_busy) return;
        if (gridPostings.CurrentRow?.DataBoundItem is not PostingGetDto selectedPosting)
        {
            MessageBox.Show(this, "Виберіть відправлення, яке потрібно видалити.", "PostService", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        if (MessageBox.Show(this,
            $"Видалити відправлення № {selectedPosting.Id}?\n{selectedPosting.From} → {selectedPosting.To}\n\nЦю дію неможливо скасувати.",
            "Підтвердження видалення", MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
            MessageBoxDefaultButton.Button2) != DialogResult.Yes) return;

        SetBusy(true);
        lblStatus.Text = "Видалення відправлення…";
        try
        {
            using var response = await _httpClient.DeleteAsync($"postings/{selectedPosting.Id}", _lifetime.Token);
            if (IsDisposed || Disposing) return;
            if (!response.IsSuccessStatusCode)
            {
                var message = response.StatusCode == HttpStatusCode.NotFound
                    ? "Це відправлення вже відсутнє на сервері. Список буде оновлено."
                    : $"Не вдалося видалити відправлення. Сервер повернув статус {(int)response.StatusCode}.";
                MessageBox.Show(this, message, "PostService", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            // Джерело істини — сервер. Повторний GET потрібен і після відповіді 404.
            await LoadPostingsAsync();
        }
        catch (OperationCanceledException) when (_lifetime.IsCancellationRequested) { }
        catch (OperationCanceledException)
        {
            ShowLoadError("Сервер не відповів вчасно. Оновіть список, щоб перевірити результат видалення.");
        }
        catch (HttpRequestException)
        {
            ShowLoadError("Не вдалося зв’язатися із сервером. Оновіть список після відновлення з’єднання.");
        }
        finally
        {
            if (!IsDisposed && !Disposing) SetBusy(false);
        }
    }

    private void ShowLoadError(string message)
    {
        if (IsDisposed || Disposing || _lifetime.IsCancellationRequested) return;
        lblStatus.Text = "Дані не оновлено. Перевірте з’єднання й натисніть «Оновити».";
        MessageBox.Show(this, message, "Помилка з’єднання", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }

    private void SetBusy(bool busy)
    {
        _busy = busy;
        btnRefresh.Enabled = !busy;
        gridPostings.Enabled = !busy;
        UseWaitCursor = busy;
        UpdateSelection();
    }

    private void gridPostings_SelectionChanged(object? sender, EventArgs e) => UpdateSelection();

    private void UpdateSelection()
    {
        var selected = gridPostings.CurrentRow?.DataBoundItem as PostingGetDto;
        btnDelete.Enabled = !_busy && selected is not null;
        lblSelection.Text = selected is null ? "Виберіть рядок для видалення" : $"Вибрано відправлення № {selected.Id}";
    }

    private void gridPostings_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (gridPostings.Columns[e.ColumnIndex].DataPropertyName == nameof(PostingGetDto.DeliveryType) && e.Value is DeliveryType type)
        {
            e.Value = type switch
            {
                DeliveryType.Department => "У відділення",
                DeliveryType.Courier => "Кур’єр",
                DeliveryType.ExpressCourier => "Експрес-кур’єр",
                _ => "Невідомий тип"
            };
            e.FormattingApplied = true;
        }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        base.OnFormClosing(e);
        if (!e.Cancel) _lifetime.Cancel();
    }
}