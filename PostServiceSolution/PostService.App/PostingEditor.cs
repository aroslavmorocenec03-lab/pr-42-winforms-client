using System.Globalization;
using PostService.CommonTypes;
using PostService.Dtos;

namespace PostService.App;

public partial class PostingEditor : Form
{
    private readonly PostServiceApi _api;
    private readonly PostingGetDto? _posting;
    private readonly CancellationTokenSource _lifetime = new();
    private bool _saving;
    private bool _resourcesDisposed;
    public int? SavedPostingId { get; private set; }

    public PostingEditor(PostServiceApi api, PostingGetDto? posting = null)
    {
        _api = api; _posting = posting;
        InitializeComponent();
        Text = posting is null ? "Нове відправлення" : $"Редагування відправлення № {posting.Id}";
        lblHeading.Text = Text;
        lblInfo.Text = posting is null ? "Заповніть дані відправлення. * Обов’язкові поля."
            : $"Створено {posting.CreatedAt.ToLocalTime():dd.MM.yyyy HH:mm}. * Обов’язкові поля.";
        cboDelivery.SelectedIndex = 0;
        inputs["Price"].ReadOnly = posting is null;
        inputs["Price"].BackColor = posting is null ? Color.FromArgb(232, 237, 244) : Color.White;
        inputs["Price"].Text = posting is null ? "Розрахує PostService" : Format(posting.Price);
        if (posting is not null)
        {
            inputs["From"].Text = posting.From; inputs["To"].Text = posting.To;
            inputs["Content"].Text = posting.Content; cboDelivery.SelectedIndex = (int)posting.DeliveryType;
            inputs["Weight"].Text = Format(posting.Weight); inputs["Width"].Text = Format(posting.Width);
            inputs["Height"].Text = Format(posting.Height); inputs["Depth"].Text = Format(posting.Depth);
            inputs["Value"].Text = posting.Value is float value ? Format(value) : "";
            inputs["Description"].Text = posting.Description;
        }
    }
    private static string Format(float value) => value.ToString("G9", CultureInfo.GetCultureInfo("uk-UA"));

    private async void btnSave_Click(object? sender, EventArgs e)
    {
        if (_saving) return;
        errorProvider.Clear();
        var fields = inputs.ToDictionary(x => x.Key, x => x.Value.Text);
        DeliveryType? delivery = cboDelivery.SelectedIndex < 0 ? null : (DeliveryType)cboDelivery.SelectedIndex;
        var errors = PostingInput.Validate(fields, delivery, _posting is not null, out var create, out var update);
        if (errors.Count > 0)
        {
            ShowFieldErrors(errors);
            lblMessage.Text = "Виправте позначені поля. " + string.Join(" ", errors.Values.Distinct());
            return;
        }
        SetSaving(true); lblMessage.Text = "Збереження відправлення…";
        try
        {
            update.Id = _posting?.Id ?? 0;
            var saved = _posting is null ? await _api.CreateAsync(create, _lifetime.Token) : await _api.UpdateAsync(update, _lifetime.Token);
            SavedPostingId = saved.Id;
            SetSaving(false);
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception error) when (error is HttpRequestException or OperationCanceledException or System.Text.Json.JsonException or PostServiceApiException)
        {
            lblMessage.Text = OperationError.Describe(error, saving: true);
            if (error is PostServiceApiException api && api.Errors.Count > 0)
                ShowFieldErrors(api.Errors.ToDictionary(x => x.Key, x => string.Join(" ", x.Value)));
        }
        finally { if (!IsDisposed) SetSaving(false); }
    }
    private void ShowFieldErrors(IReadOnlyDictionary<string, string> errors)
    {
        Control? first = null;
        foreach (var (key, message) in errors)
        {
            Control? control = key.Equals("DeliveryType", StringComparison.OrdinalIgnoreCase) ? cboDelivery
                : inputs.FirstOrDefault(x => x.Key.Equals(key, StringComparison.OrdinalIgnoreCase)).Value;
            if (control is null) continue;
            errorProvider.SetError(control, message); first ??= control;
        }
        first?.Focus();
    }
    private void SetSaving(bool saving)
    {
        _saving = saving; fieldsPanel.Enabled = btnSave.Enabled = btnCancel.Enabled = !saving;
        btnSave.Text = saving ? "Збереження…" : "Зберегти";
    }
    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (_saving) { e.Cancel = true; return; }
        base.OnFormClosing(e);
        if (!e.Cancel) _lifetime.Cancel();
    }
}
