#nullable enable
namespace PostService.App;

partial class PostingEditor
{
    private readonly Dictionary<string, TextBox> inputs = new();
    private TableLayoutPanel fieldsPanel = null!;
    private ComboBox cboDelivery = null!;
    private Button btnSave = null!, btnCancel = null!;
    private Label lblHeading = null!, lblInfo = null!, lblMessage = null!;
    private ErrorProvider errorProvider = null!;
    protected override void Dispose(bool disposing)
    {
        if (disposing && !_resourcesDisposed) { _resourcesDisposed = true; _lifetime.Cancel(); _lifetime.Dispose(); errorProvider.Dispose(); }
        base.Dispose(disposing);
    }
    private void InitializeComponent()
    {
        SuspendLayout();
        AutoScaleDimensions = new SizeF(96, 96); AutoScaleMode = AutoScaleMode.Dpi;
        ClientSize = new Size(790, 660); MinimumSize = new Size(730, 640);
        Font = new Font("Segoe UI", 10); BackColor = Color.FromArgb(243, 246, 250); ForeColor = Color.FromArgb(23, 43, 66);
        StartPosition = FormStartPosition.CenterParent; ShowInTaskbar = false;
        MaximizeBox = false; MinimizeBox = false;
        lblHeading = new Label { Location = new Point(24, 16), Size = new Size(742, 42), Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top, Font = new Font("Segoe UI", 18, FontStyle.Bold) };
        lblInfo = new Label { Location = new Point(26, 62), Size = new Size(740, 28), Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top };
        fieldsPanel = new TableLayoutPanel
        {
            Location = new Point(24, 90), Size = new Size(742, 400), Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
            ColumnCount = 2, RowCount = 11, AutoScroll = true, Padding = new Padding(0, 0, 24, 0), TabIndex = 0
        };
        fieldsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 220));
        fieldsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        string[] keys = ["From", "To", "Content", "DeliveryType", "Weight", "Width", "Height", "Depth", "Value", "Price", "Description"];
        string[] labels = ["Відправник *", "Отримувач *", "Вміст *", "Тип доставки *", "Вага, кг *", "Ширина, см *", "Висота, см *", "Глибина, см *", "Оголошена цінність, грн", "Вартість, грн", "Опис"];
        for (int row = 0; row < keys.Length; row++)
        {
            fieldsPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            var label = new Label { Text = labels[row], AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(0, 0, 8, 0) };
            Control input;
            if (keys[row] == "DeliveryType")
            {
                cboDelivery = new ComboBox { Name = "DeliveryType", DropDownStyle = ComboBoxStyle.DropDownList };
                cboDelivery.Items.AddRange(["У відділення", "Кур’єр", "Експрес-кур’єр"]); input = cboDelivery;
            }
            else
            {
                var textbox = new TextBox { Name = keys[row] };
                inputs.Add(keys[row], textbox); input = textbox;
            }
            input.Dock = DockStyle.Fill; input.Margin = new Padding(0, 4, 8, 4); input.TabIndex = row;
            input.AccessibleName = labels[row];
            fieldsPanel.Controls.Add(label, 0, row); fieldsPanel.Controls.Add(input, 1, row);
        }
        var note = new Label { Text = "Числа можна вводити через кому або крапку. Нову вартість розраховує сервер.", Location = new Point(24, 500), Size = new Size(742, 28), Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right, ForeColor = Color.FromArgb(90, 109, 128) };
        lblMessage = new Label { Name = "lblMessage", Location = new Point(24, 532), Size = new Size(742, 66), Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right, ForeColor = Color.FromArgb(168, 30, 48), AccessibleRole = AccessibleRole.Alert };
        btnSave = new Button { Name = "btnSave", Text = "Зберегти", Location = new Point(24, 608), Size = new Size(150, 40), Anchor = AnchorStyles.Bottom | AnchorStyles.Left, TabIndex = 1, BackColor = Color.FromArgb(36, 107, 168), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
        btnCancel = new Button { Name = "btnCancel", Text = "Скасувати", Location = new Point(188, 608), Size = new Size(150, 40), Anchor = AnchorStyles.Bottom | AnchorStyles.Left, TabIndex = 2, DialogResult = DialogResult.Cancel };
        btnSave.Click += btnSave_Click;
        AcceptButton = btnSave; CancelButton = btnCancel;
        errorProvider = new ErrorProvider { ContainerControl = this, BlinkStyle = ErrorBlinkStyle.NeverBlink };
        Controls.AddRange([lblHeading, lblInfo, fieldsPanel, note, lblMessage, btnSave, btnCancel]);
        ResumeLayout(false);
    }
}
