namespace GameCatalog.App.Forms
{
    partial class StatsForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.pnlLeft = new System.Windows.Forms.Panel();
            this.lstCategories = new System.Windows.Forms.ListBox();
            this.grpScope = new System.Windows.Forms.GroupBox();
            this.pnlScope = new System.Windows.Forms.FlowLayoutPanel();
            this.rbAll = new System.Windows.Forms.RadioButton();
            this.rbSelected = new System.Windows.Forms.RadioButton();
            this.rbChecked = new System.Windows.Forms.RadioButton();
            this.rbVisible = new System.Windows.Forms.RadioButton();
            this.pnlRight = new System.Windows.Forms.Panel();
            this.pnlBody = new System.Windows.Forms.Panel();
            this.chartCanvas = new GameCatalog.UI.Controls.StatsCanvas();
            this.scrChart = new System.Windows.Forms.VScrollBar();
            this.lvGeneral = new System.Windows.Forms.ListView();
            this.colGeneralName = new System.Windows.Forms.ColumnHeader();
            this.colGeneralValue = new System.Windows.Forms.ColumnHeader();
            this.tsChart = new System.Windows.Forms.ToolStrip();
            this.btnSaveChart = new System.Windows.Forms.ToolStripButton();
            this.btnCopyChart = new System.Windows.Forms.ToolStripButton();
            this.sepChart = new System.Windows.Forms.ToolStripSeparator();
            this.btnOptions = new System.Windows.Forms.ToolStripDropDownButton();
            this.mnuOptLegend = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuOptLabels = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuOptEmptyMonths = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuOptGroup = new System.Windows.Forms.ToolStripMenuItem();
            this.pnlButtons = new System.Windows.Forms.Panel();
            this.btnClose = new System.Windows.Forms.Button();
            this.localizer = new GameCatalog.UI.Components.Localizer(this.components);
            this.pnlLeft.SuspendLayout();
            this.grpScope.SuspendLayout();
            this.pnlScope.SuspendLayout();
            this.pnlRight.SuspendLayout();
            this.pnlBody.SuspendLayout();
            this.tsChart.SuspendLayout();
            this.pnlButtons.SuspendLayout();
            this.SuspendLayout();
            //
            // pnlLeft
            //
            this.pnlLeft.Controls.Add(this.lstCategories);
            this.pnlLeft.Controls.Add(this.grpScope);
            this.pnlLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlLeft.Location = new System.Drawing.Point(0, 0);
            this.pnlLeft.Name = "pnlLeft";
            this.pnlLeft.Size = new System.Drawing.Size(160, 479);
            this.pnlLeft.TabIndex = 1;
            //
            // lstCategories
            //
            this.lstCategories.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstCategories.IntegralHeight = false;
            this.lstCategories.ItemHeight = 15;
            this.lstCategories.Location = new System.Drawing.Point(0, 0);
            this.lstCategories.Name = "lstCategories";
            this.lstCategories.Size = new System.Drawing.Size(160, 359);
            this.lstCategories.TabIndex = 0;
            //
            // grpScope
            //
            this.grpScope.Controls.Add(this.pnlScope);
            this.grpScope.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.grpScope.Location = new System.Drawing.Point(0, 359);
            this.grpScope.Name = "grpScope";
            this.grpScope.Size = new System.Drawing.Size(160, 120);
            this.grpScope.TabIndex = 1;
            this.grpScope.TabStop = false;
            this.grpScope.Text = "Uwzględnij gry";
            //
            // pnlScope
            //
            this.pnlScope.Controls.Add(this.rbAll);
            this.pnlScope.Controls.Add(this.rbSelected);
            this.pnlScope.Controls.Add(this.rbChecked);
            this.pnlScope.Controls.Add(this.rbVisible);
            this.pnlScope.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlScope.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.pnlScope.Name = "pnlScope";
            this.pnlScope.TabIndex = 0;
            //
            // rbAll
            //
            this.rbAll.AutoSize = true;
            this.rbAll.Checked = true;
            this.rbAll.Name = "rbAll";
            this.rbAll.TabIndex = 0;
            this.rbAll.TabStop = true;
            this.rbAll.Text = "Wszystkie";
            this.rbAll.UseVisualStyleBackColor = true;
            //
            // rbSelected
            //
            this.rbSelected.AutoSize = true;
            this.rbSelected.Name = "rbSelected";
            this.rbSelected.TabIndex = 1;
            this.rbSelected.Text = "Wybrane";
            this.rbSelected.UseVisualStyleBackColor = true;
            //
            // rbChecked
            //
            this.rbChecked.AutoSize = true;
            this.rbChecked.Name = "rbChecked";
            this.rbChecked.TabIndex = 2;
            this.rbChecked.Text = "Zaznaczone";
            this.rbChecked.UseVisualStyleBackColor = true;
            //
            // rbVisible
            //
            this.rbVisible.AutoSize = true;
            this.rbVisible.Name = "rbVisible";
            this.rbVisible.TabIndex = 3;
            this.rbVisible.Text = "Widoczne";
            this.rbVisible.UseVisualStyleBackColor = true;
            //
            // pnlRight
            //
            this.pnlRight.Controls.Add(this.pnlBody);
            this.pnlRight.Controls.Add(this.tsChart);
            this.pnlRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlRight.Location = new System.Drawing.Point(160, 0);
            this.pnlRight.Name = "pnlRight";
            this.pnlRight.Size = new System.Drawing.Size(504, 479);
            this.pnlRight.TabIndex = 0;
            //
            // pnlBody
            //
            this.pnlBody.Controls.Add(this.chartCanvas);
            this.pnlBody.Controls.Add(this.scrChart);
            this.pnlBody.Controls.Add(this.lvGeneral);
            this.pnlBody.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlBody.Location = new System.Drawing.Point(0, 25);
            this.pnlBody.Name = "pnlBody";
            this.pnlBody.Size = new System.Drawing.Size(504, 454);
            this.pnlBody.TabIndex = 1;
            //
            // chartCanvas
            //
            this.chartCanvas.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chartCanvas.Name = "chartCanvas";
            this.chartCanvas.TabIndex = 0;
            //
            // scrChart
            //
            this.scrChart.Dock = System.Windows.Forms.DockStyle.Right;
            this.scrChart.Name = "scrChart";
            this.scrChart.TabIndex = 1;
            this.scrChart.Visible = false;
            //
            // lvGeneral
            //
            this.lvGeneral.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colGeneralName,
            this.colGeneralValue});
            this.lvGeneral.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvGeneral.FullRowSelect = true;
            this.lvGeneral.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.lvGeneral.Name = "lvGeneral";
            this.lvGeneral.TabIndex = 2;
            this.lvGeneral.UseCompatibleStateImageBehavior = false;
            this.lvGeneral.View = System.Windows.Forms.View.Details;
            //
            // colGeneralName
            //
            this.colGeneralName.Width = 220;
            //
            // colGeneralValue
            //
            this.colGeneralValue.Width = 200;
            //
            // tsChart
            //
            this.tsChart.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnSaveChart,
            this.btnCopyChart,
            this.sepChart,
            this.btnOptions});
            this.tsChart.Location = new System.Drawing.Point(0, 0);
            this.tsChart.Name = "tsChart";
            this.tsChart.Size = new System.Drawing.Size(504, 25);
            this.tsChart.TabIndex = 0;
            //
            // btnSaveChart
            //
            this.btnSaveChart.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnSaveChart.Name = "btnSaveChart";
            this.btnSaveChart.ToolTipText = "Zapisz wykres…";
            //
            // btnCopyChart
            //
            this.btnCopyChart.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnCopyChart.Name = "btnCopyChart";
            this.btnCopyChart.ToolTipText = "Kopiuj wykres do schowka";
            //
            // sepChart
            //
            this.sepChart.Name = "sepChart";
            //
            // btnOptions
            //
            this.btnOptions.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuOptLegend,
            this.mnuOptLabels,
            this.mnuOptEmptyMonths,
            this.mnuOptGroup});
            this.btnOptions.Name = "btnOptions";
            this.btnOptions.Text = "Opcje";
            this.btnOptions.ToolTipText = "Opcje wykresu";
            //
            // mnuOptLegend
            //
            this.mnuOptLegend.Checked = true;
            this.mnuOptLegend.CheckOnClick = true;
            this.mnuOptLegend.CheckState = System.Windows.Forms.CheckState.Checked;
            this.mnuOptLegend.Name = "mnuOptLegend";
            this.mnuOptLegend.Text = "Legenda";
            //
            // mnuOptLabels
            //
            this.mnuOptLabels.Checked = true;
            this.mnuOptLabels.CheckOnClick = true;
            this.mnuOptLabels.CheckState = System.Windows.Forms.CheckState.Checked;
            this.mnuOptLabels.Name = "mnuOptLabels";
            this.mnuOptLabels.Text = "Etykiety";
            //
            // mnuOptEmptyMonths
            //
            this.mnuOptEmptyMonths.Checked = true;
            this.mnuOptEmptyMonths.CheckOnClick = true;
            this.mnuOptEmptyMonths.CheckState = System.Windows.Forms.CheckState.Checked;
            this.mnuOptEmptyMonths.Name = "mnuOptEmptyMonths";
            this.mnuOptEmptyMonths.Text = "Puste miesiące";
            //
            // mnuOptGroup
            //
            this.mnuOptGroup.Checked = true;
            this.mnuOptGroup.CheckOnClick = true;
            this.mnuOptGroup.CheckState = System.Windows.Forms.CheckState.Checked;
            this.mnuOptGroup.Name = "mnuOptGroup";
            this.mnuOptGroup.Text = "Grupuj wartości";
            //
            // pnlButtons
            //
            this.pnlButtons.Controls.Add(this.btnClose);
            this.pnlButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlButtons.Location = new System.Drawing.Point(0, 479);
            this.pnlButtons.Name = "pnlButtons";
            this.pnlButtons.Size = new System.Drawing.Size(664, 42);
            this.pnlButtons.TabIndex = 2;
            //
            // btnClose
            //
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.Location = new System.Drawing.Point(564, 8);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(90, 23);
            this.btnClose.TabIndex = 0;
            this.btnClose.Text = "Zamknij";
            this.btnClose.UseVisualStyleBackColor = true;
            //
            // localizer
            //
            this.localizer.ContainerControl = this;
            //
            // StatsForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(664, 521);
            this.Controls.Add(this.pnlRight);
            this.Controls.Add(this.pnlLeft);
            this.Controls.Add(this.pnlButtons);
            this.MinimizeBox = false;
            this.Name = "StatsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Statystyki";
            this.pnlLeft.ResumeLayout(false);
            this.grpScope.ResumeLayout(false);
            this.pnlScope.ResumeLayout(false);
            this.pnlScope.PerformLayout();
            this.pnlRight.ResumeLayout(false);
            this.pnlRight.PerformLayout();
            this.pnlBody.ResumeLayout(false);
            this.tsChart.ResumeLayout(false);
            this.tsChart.PerformLayout();
            this.pnlButtons.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlLeft;
        private System.Windows.Forms.ListBox lstCategories;
        private System.Windows.Forms.GroupBox grpScope;
        private System.Windows.Forms.FlowLayoutPanel pnlScope;
        private System.Windows.Forms.RadioButton rbAll;
        private System.Windows.Forms.RadioButton rbSelected;
        private System.Windows.Forms.RadioButton rbChecked;
        private System.Windows.Forms.RadioButton rbVisible;
        private System.Windows.Forms.Panel pnlRight;
        private System.Windows.Forms.Panel pnlBody;
        private GameCatalog.UI.Controls.StatsCanvas chartCanvas;
        private System.Windows.Forms.VScrollBar scrChart;
        private System.Windows.Forms.ListView lvGeneral;
        private System.Windows.Forms.ColumnHeader colGeneralName;
        private System.Windows.Forms.ColumnHeader colGeneralValue;
        private System.Windows.Forms.ToolStrip tsChart;
        private System.Windows.Forms.ToolStripButton btnSaveChart;
        private System.Windows.Forms.ToolStripButton btnCopyChart;
        private System.Windows.Forms.ToolStripSeparator sepChart;
        private System.Windows.Forms.ToolStripDropDownButton btnOptions;
        private System.Windows.Forms.ToolStripMenuItem mnuOptLegend;
        private System.Windows.Forms.ToolStripMenuItem mnuOptLabels;
        private System.Windows.Forms.ToolStripMenuItem mnuOptEmptyMonths;
        private System.Windows.Forms.ToolStripMenuItem mnuOptGroup;
        private System.Windows.Forms.Panel pnlButtons;
        private System.Windows.Forms.Button btnClose;
        private GameCatalog.UI.Components.Localizer localizer;
    }
}
