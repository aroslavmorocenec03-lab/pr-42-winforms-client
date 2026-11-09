#nullable enable

namespace PostService.App;

partial class PostingList
{
    private System.ComponentModel.IContainer? components;
    private DataGridView gridPostings = null!;
    private Button btnDelete = null!;
    private Button btnRefresh = null!;
    private Label lblTitle = null!;
    private Label lblSubtitle = null!;
    private Label lblStatus = null!;
    private Label lblSelection = null!;
    private DataGridViewTextBoxColumn colId = null!;
    private DataGridViewTextBoxColumn colFrom = null!;
    private DataGridViewTextBoxColumn colTo = null!;
    private DataGridViewTextBoxColumn colContent = null!;
    private DataGridViewTextBoxColumn colDeliveryType = null!;
    private DataGridViewTextBoxColumn colWeight = null!;
    private DataGridViewTextBoxColumn colWidth = null!;
    private DataGridViewTextBoxColumn colHeight = null!;
    private DataGridViewTextBoxColumn colDepth = null!;
    private DataGridViewTextBoxColumn colValue = null!;
    private DataGridViewTextBoxColumn colPrice = null!;
    private DataGridViewTextBoxColumn colDescription = null!;
    private DataGridViewTextBoxColumn colCreatedAt = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && !_resourcesDisposed)
        {
            _resourcesDisposed = true;
            _lifetime.Cancel();
            _httpClient.Dispose();
            _lifetime.Dispose();
            components?.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        gridPostings = new DataGridView();
        btnDelete = new Button();
        btnRefresh = new Button();
        lblTitle = new Label();
        lblSubtitle = new Label();
        lblStatus = new Label();
        lblSelection = new Label();
        colId = new DataGridViewTextBoxColumn();
        colFrom = new DataGridViewTextBoxColumn();
        colTo = new DataGridViewTextBoxColumn();
        colContent = new DataGridViewTextBoxColumn();
        colDeliveryType = new DataGridViewTextBoxColumn();
        colWeight = new DataGridViewTextBoxColumn();
        colWidth = new DataGridViewTextBoxColumn();
        colHeight = new DataGridViewTextBoxColumn();
        colDepth = new DataGridViewTextBoxColumn();
        colValue = new DataGridViewTextBoxColumn();
        colPrice = new DataGridViewTextBoxColumn();
        colDescription = new DataGridViewTextBoxColumn();
        colCreatedAt = new DataGridViewTextBoxColumn();
        ((System.ComponentModel.ISupportInitialize)gridPostings).BeginInit();
        SuspendLayout();
        // gridPostings
        gridPostings.Name = "gridPostings";
        gridPostings.Location = new Point(24, 130);
        gridPostings.Size = new Size(1112, 430);
        gridPostings.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        gridPostings.TabIndex = 0;
        gridPostings.ReadOnly = true;
        gridPostings.AllowUserToAddRows = false;
        gridPostings.AllowUserToDeleteRows = false;
        gridPostings.MultiSelect = false;
        gridPostings.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        gridPostings.AutoGenerateColumns = false;
        gridPostings.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
        gridPostings.BackgroundColor = Color.White;
        gridPostings.BorderStyle = BorderStyle.FixedSingle;
        gridPostings.RowHeadersVisible = false;
        gridPostings.RowTemplate.Height = 42;
        gridPostings.ColumnHeadersHeight = 44;
        gridPostings.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        gridPostings.EnableHeadersVisualStyles = false;
        gridPostings.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(232, 237, 244);
        gridPostings.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(36, 59, 83);
        gridPostings.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        gridPostings.DefaultCellStyle.SelectionBackColor = Color.FromArgb(214, 234, 255);
        gridPostings.DefaultCellStyle.SelectionForeColor = Color.FromArgb(23, 43, 66);
        gridPostings.DefaultCellStyle.NullValue = "—";
        gridPostings.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
        gridPostings.Columns.AddRange(colId, colFrom, colTo, colContent, colDeliveryType,
            colWeight, colWidth, colHeight, colDepth, colValue, colPrice, colDescription, colCreatedAt);
        gridPostings.SelectionChanged += gridPostings_SelectionChanged;
        gridPostings.CellFormatting += gridPostings_CellFormatting;
        // Explicit DTO bindings, including hidden technical columns.
        colId.Name = "colId"; colId.HeaderText = "Номер"; colId.DataPropertyName = "Id"; colId.Visible = false;
        colFrom.Name = "colFrom"; colFrom.HeaderText = "Відправник"; colFrom.DataPropertyName = "From"; colFrom.Width = 170;
        colTo.Name = "colTo"; colTo.HeaderText = "Отримувач"; colTo.DataPropertyName = "To"; colTo.Width = 170;
        colContent.Name = "colContent"; colContent.HeaderText = "Вміст"; colContent.DataPropertyName = "Content"; colContent.Width = 145;
        colDeliveryType.Name = "colDeliveryType"; colDeliveryType.HeaderText = "Тип доставки"; colDeliveryType.DataPropertyName = "DeliveryType"; colDeliveryType.Width = 145;
        colWeight.Name = "colWeight"; colWeight.HeaderText = "Вага, кг"; colWeight.DataPropertyName = "Weight"; colWeight.Width = 90;
        colWidth.Name = "colWidth"; colWidth.HeaderText = "Ширина, см"; colWidth.DataPropertyName = "Width"; colWidth.Width = 105;
        colHeight.Name = "colHeight"; colHeight.HeaderText = "Висота, см"; colHeight.DataPropertyName = "Height"; colHeight.Width = 105;
        colDepth.Name = "colDepth"; colDepth.HeaderText = "Глибина, см"; colDepth.DataPropertyName = "Depth"; colDepth.Width = 105;
        colValue.Name = "colValue"; colValue.HeaderText = "Цінність, грн"; colValue.DataPropertyName = "Value"; colValue.Width = 125; colValue.DefaultCellStyle.Format = "N2";
        colPrice.Name = "colPrice"; colPrice.HeaderText = "Вартість, грн"; colPrice.DataPropertyName = "Price"; colPrice.Width = 125; colPrice.DefaultCellStyle.Format = "N2";
        colDescription.Name = "colDescription"; colDescription.HeaderText = "Опис"; colDescription.DataPropertyName = "Description"; colDescription.Width = 200;
        colCreatedAt.Name = "colCreatedAt"; colCreatedAt.HeaderText = "Створено"; colCreatedAt.DataPropertyName = "CreatedAt"; colCreatedAt.Visible = false;
        // btnDelete
        btnDelete.Name = "btnDelete";
        btnDelete.Text = "Видалити";
        btnDelete.Location = new Point(900, 30);
        btnDelete.Size = new Size(112, 42);
        btnDelete.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnDelete.TabIndex = 2;
        btnDelete.BackColor = Color.FromArgb(180, 35, 53);
        btnDelete.ForeColor = Color.White;
        btnDelete.FlatStyle = FlatStyle.Flat;
        btnDelete.FlatAppearance.BorderSize = 0;
        btnDelete.Enabled = false;
        btnDelete.Click += btnDelete_Click;
        // btnRefresh
        btnRefresh.Name = "btnRefresh";
        btnRefresh.Text = "Оновити";
        btnRefresh.Location = new Point(1028, 30);
        btnRefresh.Size = new Size(108, 42);
        btnRefresh.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnRefresh.TabIndex = 1;
        btnRefresh.BackColor = Color.FromArgb(36, 107, 168);
        btnRefresh.ForeColor = Color.White;
        btnRefresh.FlatStyle = FlatStyle.Flat;
        btnRefresh.FlatAppearance.BorderSize = 0;
        btnRefresh.Click += btnRefresh_Click;
        // Labels
        lblTitle.Name = "lblTitle"; lblTitle.Text = "Поштові відправлення";
        lblTitle.Font = new Font("Segoe UI", 22F, FontStyle.Bold);
        lblTitle.Location = new Point(20, 20); lblTitle.Size = new Size(800, 50);
        lblSubtitle.Name = "lblSubtitle"; lblSubtitle.Text = "Перегляд та видалення відправлень";
        lblSubtitle.Location = new Point(24, 72); lblSubtitle.Size = new Size(800, 26);
        lblSubtitle.ForeColor = Color.FromArgb(90, 109, 128);
        lblSelection.Name = "lblSelection"; lblSelection.Text = "Виберіть рядок для видалення";
        lblSelection.Location = new Point(24, 102); lblSelection.Size = new Size(900, 25);
        lblSelection.ForeColor = Color.FromArgb(26, 93, 150);
        lblStatus.Name = "lblStatus"; lblStatus.Text = "Очікування завантаження…";
        lblStatus.Location = new Point(24, 577); lblStatus.Size = new Size(1112, 26);
        lblStatus.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        lblStatus.ForeColor = Color.FromArgb(90, 109, 128);
        // PostingList
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        ClientSize = new Size(1160, 620);
        MinimumSize = new Size(1000, 540);
        Font = new Font("Segoe UI", 10F);
        BackColor = Color.FromArgb(243, 246, 250);
        ForeColor = Color.FromArgb(23, 43, 66);
        StartPosition = FormStartPosition.CenterScreen;
        Name = "PostingList";
        Text = "PostService — список відправлень";
        Controls.Add(gridPostings);
        Controls.Add(btnDelete);
        Controls.Add(btnRefresh);
        Controls.Add(lblTitle);
        Controls.Add(lblSubtitle);
        Controls.Add(lblStatus);
        Controls.Add(lblSelection);
        Load += PostingList_Load;
        ((System.ComponentModel.ISupportInitialize)gridPostings).EndInit();
        ResumeLayout(false);
    }
}
