using PostService.CommonTypes;
using PostService.Dtos;

namespace PostService.App;

public partial class PostingList : Form
{
    private readonly PostServiceApi _api;
    private readonly CancellationTokenSource _lifetime = new();
    private bool _busy;
    private bool _resourcesDisposed;
    private PostingGetDto? Selected => gridPostings.CurrentRow?.DataBoundItem as PostingGetDto;
    public PostingList()
    {
        _api = new PostServiceApi(Environment.GetEnvironmentVariable("POST_SERVICE_URL") ?? "http://localhost:5211/");
        InitializeComponent(); UpdateSelection();
    }
    private async void PostingList_Load(object? sender, EventArgs e) => await LoadPostingsAsync();
    private async Task LoadPostingsAsync(int? preferredId = null, string? success = null)
    {
        int? id = preferredId ?? Selected?.Id;
        SetBusy(true); lblStatus.Text = "Завантаження відправлень…";
        try
        {
            var postings = await _api.GetAllAsync(_lifetime.Token);
            if (IsDisposed) return;
            gridPostings.DataSource = postings; gridPostings.ClearSelection(); gridPostings.CurrentCell = null;
            foreach (DataGridViewRow row in gridPostings.Rows)
                if (row.DataBoundItem is PostingGetDto posting && posting.Id == id)
                {
                    gridPostings.CurrentCell = row.Cells["colFrom"]; row.Selected = true; break;
                }
            lblStatus.ForeColor = Color.FromArgb(60, 89, 113);
            lblStatus.Text = $"{success}Відправлень: {postings.Length}. Оновлено о {DateTime.Now:HH:mm:ss}."
                + (postings.Length == 0 ? " Натисніть «Нове відправлення», щоб додати запис." : "");
        }
        catch (OperationCanceledException) when (_lifetime.IsCancellationRequested) { }
        catch (Exception error) when (IsOperationError(error))
        { ShowError((success is null ? "" : success + "Список не оновлено. ") + OperationError.Describe(error)); }
        finally { if (!IsDisposed) SetBusy(false); }
    }
    private async void btnRefresh_Click(object? sender, EventArgs e) { if (!_busy) await LoadPostingsAsync(); }
    private async void btnNew_Click(object? sender, EventArgs e)
    {
        if (_busy) return;
        SetBusy(true);
        try
        {
            using var dialog = new PostingEditor(_api);
            var result = dialog.ShowDialog(this);
            // Перевіряємо сервер і після Cancel: запит міг завершитися після обриву зв’язку.
            await LoadPostingsAsync(dialog.SavedPostingId, result == DialogResult.OK ? "Відправлення створено. " : null);
        }
        finally { if (!IsDisposed) SetBusy(false); }
    }
    private async void btnEdit_Click(object? sender, EventArgs e)
    {
        if (_busy || Selected is not PostingGetDto selected) return;
        SetBusy(true);
        try
        {
            var current = await _api.GetAsync(selected.Id, _lifetime.Token);
            if (IsDisposed) return;
            using var dialog = new PostingEditor(_api, current);
            var result = dialog.ShowDialog(this);
            await LoadPostingsAsync(dialog.SavedPostingId ?? current.Id, result == DialogResult.OK ? "Зміни збережено. " : null);
        }
        catch (OperationCanceledException) when (_lifetime.IsCancellationRequested) { }
        catch (Exception error) when (IsOperationError(error))
        {
            ShowError(OperationError.Describe(error));
            if (error is PostServiceApiException { StatusCode: System.Net.HttpStatusCode.NotFound }) await LoadPostingsAsync();
        }
        finally { if (!IsDisposed) SetBusy(false); }
    }
    private async void btnDelete_Click(object? sender, EventArgs e)
    {
        if (_busy || Selected is not PostingGetDto selected) return;
        if (MessageBox.Show(this, $"Видалити відправлення № {selected.Id}?\n{selected.From} → {selected.To}\n\nЦю дію неможливо скасувати.",
            "Підтвердження видалення", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) != DialogResult.Yes) return;
        SetBusy(true);
        try { await _api.DeleteAsync(selected.Id, _lifetime.Token); await LoadPostingsAsync(success: "Відправлення видалено. "); }
        catch (OperationCanceledException) when (_lifetime.IsCancellationRequested) { }
        catch (Exception error) when (IsOperationError(error))
        {
            ShowError(OperationError.Describe(error));
            if (error is PostServiceApiException { StatusCode: System.Net.HttpStatusCode.NotFound }) await LoadPostingsAsync();
        }
        finally { if (!IsDisposed) SetBusy(false); }
    }
    private static bool IsOperationError(Exception e) => e is HttpRequestException or OperationCanceledException or System.Text.Json.JsonException or PostServiceApiException;
    private void ShowError(string message)
    {
        if (IsDisposed || _lifetime.IsCancellationRequested) return;
        lblStatus.ForeColor = Color.FromArgb(168, 30, 48); lblStatus.Text = message;
        MessageBox.Show(this, message, "Помилка операції", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }
    private void SetBusy(bool busy)
    {
        _busy = busy; btnNew.Enabled = btnRefresh.Enabled = gridPostings.Enabled = !busy; UpdateSelection();
    }
    private void UpdateSelection()
    {
        btnEdit.Enabled = btnDelete.Enabled = !_busy && Selected is not null;
        lblSelection.Text = Selected is null ? "Виберіть відправлення для редагування або видалення" : $"Вибрано відправлення № {Selected.Id}";
    }
    private void gridPostings_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (gridPostings.Columns[e.ColumnIndex].DataPropertyName == nameof(PostingGetDto.DeliveryType) && e.Value is DeliveryType type)
        {
            e.Value = type switch { DeliveryType.Department => "У відділення", DeliveryType.Courier => "Кур’єр", _ => "Експрес-кур’єр" };
            e.FormattingApplied = true;
        }
        if (gridPostings.Columns[e.ColumnIndex].DataPropertyName == nameof(PostingGetDto.CreatedAt) && e.Value is DateTime date)
        { e.Value = date.ToLocalTime().ToString("dd.MM.yyyy HH:mm"); e.FormattingApplied = true; }
    }
    protected override void OnFormClosing(FormClosingEventArgs e)
    { base.OnFormClosing(e); if (!e.Cancel) _lifetime.Cancel(); }
}
