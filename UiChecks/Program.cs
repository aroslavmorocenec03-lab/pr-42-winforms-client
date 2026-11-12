using System.Reflection;
using PostService.App;
using PostService.Dtos;

// Інтеграційний тест самих WinForms-класів у потоці STA.
// Окремий тестовий проєкт не входить до робочого застосунку.
internal static class Program
{
    private static int _checks;
    private static int _exitCode;
    private static string _output = "";
    [STAThread]
    private static int Main(string[] args)
    {
        if (args.Length != 2) { Console.WriteLine("Usage: UiChecks <isolated API URL> <screenshots directory>"); return 2; }
        Environment.SetEnvironmentVariable("POST_SERVICE_URL", args[0]);
        _output = Path.GetFullPath(args[1]); Directory.CreateDirectory(_output);
        ApplicationConfiguration.Initialize();
        using var list = new PostingList();
        list.Shown += async (_, _) =>
        {
            try { await RunAsync(list, args[0]); Console.WriteLine($"All {_checks} UI checks passed."); }
            catch (Exception error) { Console.WriteLine(error); _exitCode = 1; }
            finally
            {
                foreach (var editor in Application.OpenForms.OfType<PostingEditor>().ToArray()) editor.Close();
                list.Close();
            }
        };
        Application.Run(list);
        return _exitCode;
    }
    private static T Field<T>(object target, string name) => (T)target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(target)!;
    private static void Check(bool condition, string text)
    {
        if (!condition) throw new Exception("FAIL: " + text);
        Console.WriteLine($"PASS UI {++_checks:00}: {text}");
    }
    private static async Task WaitAsync(Func<bool> condition)
    {
        var limit = DateTime.UtcNow.AddSeconds(15);
        while (!condition()) { if (DateTime.UtcNow > limit) throw new TimeoutException("UI condition timed out."); await Task.Delay(50); }
        await Task.Delay(150);
    }
    private static async Task<PostingEditor> OpenAsync(PostingList list, string button)
    {
        list.BeginInvoke(() => Field<Button>(list, button).PerformClick());
        await WaitAsync(() => Application.OpenForms.OfType<PostingEditor>().Any());
        return Application.OpenForms.OfType<PostingEditor>().Single();
    }
    private static void Snapshot(Form form, string filename)
    {
        using var bitmap = new Bitmap(form.Width, form.Height);
        form.DrawToBitmap(bitmap, new Rectangle(Point.Empty, bitmap.Size));
        bitmap.Save(Path.Combine(_output, filename), System.Drawing.Imaging.ImageFormat.Png);
    }
    private static Dictionary<string, TextBox> Inputs(PostingEditor editor) => Field<Dictionary<string, TextBox>>(editor, "inputs");
    private static async Task ReadyAsync(PostingList list) => await WaitAsync(() => Field<Button>(list, "btnNew").Enabled && Field<Label>(list, "lblStatus").Text.Contains("Відправлень:"));
    private static async Task RunAsync(PostingList list, string baseUrl)
    {
        using var api = new PostServiceApi(baseUrl);
        Check((await api.GetAllAsync()).Length == 0, "Isolated database starts empty");
        var grid = Field<DataGridView>(list, "gridPostings");
        await ReadyAsync(list);
        Check(grid.Rows.Count == 0 && !Field<Button>(list, "btnEdit").Enabled && !Field<Button>(list, "btnDelete").Enabled, "Empty list and selection buttons");
        Snapshot(list, "01-empty.png");
        var editor = await OpenAsync(list, "btnNew");
        Field<Button>(editor, "btnSave").PerformClick();
        var errors = Field<ErrorProvider>(editor, "errorProvider");
        Check(errors.GetError(Inputs(editor)["From"]).Length > 0 && errors.GetError(Inputs(editor)["Weight"]).Length > 0, "Required fields show ErrorProvider messages");
        Check((await api.GetAllAsync()).Length == 0, "Invalid form sends no new record");
        Snapshot(editor, "02-validation.png");
        var values = new Dictionary<string, string>
        {
            ["From"] = "Київ, відділення № 3", ["To"] = "Львів, відділення № 7", ["Content"] = "Книги",
            ["Weight"] = "2,5", ["Width"] = "20.5", ["Height"] = "15,25", ["Depth"] = "3", ["Value"] = "100,50", ["Description"] = "Не згинати"
        };
        foreach (var (key, value) in values) Inputs(editor)[key].Text = value;
        Check(Inputs(editor)["Price"].ReadOnly, "Create price is read only");
        Field<Button>(editor, "btnSave").PerformClick();
        await WaitAsync(() => !Application.OpenForms.OfType<PostingEditor>().Any()); await ReadyAsync(list);
        var saved = (await api.GetAllAsync()).Single();
        Check(grid.Rows.Count == 1 && saved.Content == "Книги" && saved.Price == 65 && saved.Weight == 2.5f, "Dialog POST and automatic list refresh");
        Check((grid.CurrentRow?.DataBoundItem as PostingGetDto)?.Id == saved.Id, "Saved row selected");
        Snapshot(list, "03-created.png");
        editor = await OpenAsync(list, "btnEdit");
        Check(Inputs(editor)["From"].Text == saved.From && Inputs(editor)["Content"].Text == "Книги" && !Inputs(editor)["Price"].ReadOnly, "Common edit dialog loads selected record");
        Inputs(editor)["Content"].Text = "Книги та зошити";
        Inputs(editor)["Price"].Text = "72,50";
        Snapshot(editor, "04-edit.png");
        Field<Button>(editor, "btnSave").PerformClick();
        await WaitAsync(() => !Application.OpenForms.OfType<PostingEditor>().Any()); await ReadyAsync(list);
        var updated = await api.GetAsync(saved.Id);
        Check(updated.Content == "Книги та зошити" && updated.Price == 72.5f && updated.CreatedAt == saved.CreatedAt, "Dialog PUT updates fields and keeps timestamp");
        Check((grid.Rows[0].DataBoundItem as PostingGetDto)?.Content == updated.Content, "List shows saved edits without manual refresh");
        Snapshot(list, "05-updated.png");
        editor = await OpenAsync(list, "btnEdit");
        Inputs(editor)["Content"].Text = "ЦЕ НЕ ЗБЕРІГАТИ";
        Field<Button>(editor, "btnCancel").PerformClick();
        await WaitAsync(() => !Application.OpenForms.OfType<PostingEditor>().Any()); await ReadyAsync(list);
        Check((await api.GetAsync(saved.Id)).Content == "Книги та зошити", "Cancel edit does not persist changes");
        editor = await OpenAsync(list, "btnNew");
        Inputs(editor)["From"].Text = "Скасований запис";
        Field<Button>(editor, "btnCancel").PerformClick();
        await WaitAsync(() => !Application.OpenForms.OfType<PostingEditor>().Any()); await ReadyAsync(list);
        Check((await api.GetAllAsync()).Length == 1, "Cancel creation sends no POST");
        await api.DeleteAsync(saved.Id);
        Field<Button>(list, "btnRefresh").PerformClick(); await ReadyAsync(list);
        Check(grid.Rows.Count == 0 && !Field<Button>(list, "btnEdit").Enabled, "List refresh after server deletion");
        Snapshot(list, "06-empty-after-delete.png");
        using var offlineApi = new PostServiceApi("http://localhost:1/");
        using var offlineEditor = new PostingEditor(offlineApi);
        offlineEditor.Show(list);
        foreach (var (key, value) in values) Inputs(offlineEditor)[key].Text = value;
        Field<Button>(offlineEditor, "btnSave").PerformClick();
        await WaitAsync(() => Field<Button>(offlineEditor, "btnSave").Enabled);
        Check(offlineEditor.DialogResult != DialogResult.OK && Field<Label>(offlineEditor, "lblMessage").Text.Contains("З’єднання"), "Failed save keeps editor open and explains connection error");
        Check(Inputs(offlineEditor)["Content"].Text == "Книги", "Failed save preserves entered values");
        Snapshot(offlineEditor, "07-connection-error.png");
        offlineEditor.Close();
    }
}
