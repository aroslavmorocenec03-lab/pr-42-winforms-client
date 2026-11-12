#nullable enable
namespace PostService.App;

partial class PostingList
{
    private DataGridView gridPostings = null!;
    private Button btnNew = null!, btnEdit = null!, btnDelete = null!, btnRefresh = null!;
    private Label lblStatus = null!, lblSelection = null!;
    protected override void Dispose(bool disposing)
    {
        if (disposing && !_resourcesDisposed) { _resourcesDisposed = true; _lifetime.Cancel(); _api.Dispose(); _lifetime.Dispose(); }
        base.Dispose(disposing);
    }
    private void InitializeComponent()
    {
        SuspendLayout();
        AutoScaleDimensions = new SizeF(96, 96); AutoScaleMode = AutoScaleMode.Dpi;
        Font = new Font("Segoe UI", 10); ClientSize = new Size(1160, 650); MinimumSize = new Size(980, 540);
        StartPosition = FormStartPosition.CenterScreen; Text = "Облік поштових відправлень — PostService";
        BackColor = Color.FromArgb(243, 246, 250); ForeColor = Color.FromArgb(23, 43, 66);
        var title = new Label { Text = "Поштові відправлення", Font = new Font("Segoe UI", 22, FontStyle.Bold), AutoSize = true, Location = new Point(22, 18) };
        var subtitle = new Label { Text = "Облік, створення та редагування поштових відправлень", AutoSize = true, Location = new Point(26, 70), ForeColor = Color.FromArgb(90, 109, 128) };
        btnNew = MakeButton("btnNew", "Нове відправлення", 24, 178, Color.FromArgb(36, 107, 168), 0);
        btnEdit = MakeButton("btnEdit", "Редагувати", 214, 135, Color.FromArgb(36, 107, 168), 1);
        btnDelete = MakeButton("btnDelete", "Видалити", 361, 120, Color.FromArgb(180, 35, 53), 2);
        btnRefresh = MakeButton("btnRefresh", "Оновити", 493, 120, Color.FromArgb(73, 93, 113), 3);
        btnNew.Click += btnNew_Click; btnEdit.Click += btnEdit_Click; btnDelete.Click += btnDelete_Click; btnRefresh.Click += btnRefresh_Click;
        lblSelection = new Label { Location = new Point(24, 158), Size = new Size(1100, 26), Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top };
        gridPostings = new DataGridView
        {
            Name = "gridPostings", Location = new Point(24, 190), Size = new Size(1112, 360),
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
            ReadOnly = true, AllowUserToAddRows = false, AllowUserToDeleteRows = false, AllowUserToOrderColumns = true,
            MultiSelect = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AutoGenerateColumns = false,
            RowHeadersVisible = false, BackgroundColor = Color.White, BorderStyle = BorderStyle.FixedSingle,
            ColumnHeadersHeight = 46, ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
            EnableHeadersVisualStyles = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None, TabIndex = 4
        };
        gridPostings.RowTemplate.Height = 40;
        gridPostings.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.FromArgb(232, 237, 244), Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = ForeColor };
        gridPostings.DefaultCellStyle.SelectionBackColor = Color.FromArgb(214, 234, 255); gridPostings.DefaultCellStyle.SelectionForeColor = ForeColor;
        gridPostings.DefaultCellStyle.NullValue = "—"; gridPostings.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
        AddColumn("Id", "№", 55); AddColumn("From", "Відправник", 180); AddColumn("To", "Отримувач", 180);
        AddColumn("Content", "Вміст", 150); AddColumn("DeliveryType", "Тип доставки", 160); AddColumn("Weight", "Вага, кг", 90);
        AddColumn("Price", "Вартість, грн", 130, "N2"); AddColumn("Width", "Ширина, см", 110);
        AddColumn("Height", "Висота, см", 110); AddColumn("Depth", "Глибина, см", 110); AddColumn("Value", "Цінність, грн", 130, "N2");
        AddColumn("Description", "Опис", 210); AddColumn("CreatedAt", "Створено", 160);
        gridPostings.SelectionChanged += (_, _) => UpdateSelection(); gridPostings.CellFormatting += gridPostings_CellFormatting;
        gridPostings.CellDoubleClick += (_, e) => { if (e.RowIndex >= 0 && !_busy) btnEdit.PerformClick(); };
        lblStatus = new Label { Name = "lblStatus", Text = "Очікування завантаження…", Location = new Point(24, 568), Size = new Size(1112, 62), Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom };
        Controls.AddRange([title, subtitle, btnNew, btnEdit, btnDelete, btnRefresh, lblSelection, gridPostings, lblStatus]);
        Load += PostingList_Load; ResumeLayout(false); PerformLayout();
    }
    private static Button MakeButton(string name, string text, int left, int width, Color color, int tab)
        => new() { Name = name, Text = text, Location = new Point(left, 104), Size = new Size(width, 42), BackColor = color, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, TabIndex = tab };
    private void AddColumn(string property, string title, int width, string? format = null)
    {
        var col = new DataGridViewTextBoxColumn { Name = "col" + property, DataPropertyName = property, HeaderText = title, Width = width, SortMode = DataGridViewColumnSortMode.NotSortable };
        if (format is not null) col.DefaultCellStyle.Format = format;
        gridPostings.Columns.Add(col);
    }
}
