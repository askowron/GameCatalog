namespace GameCatalog.UI.Frames
{
    partial class GameExtrasFrame
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
            this.lvExtras = new System.Windows.Forms.ListView();
            this.tsExtras = new System.Windows.Forms.ToolStrip();
            this.lblExtrasTitle = new System.Windows.Forms.Label();
            this.colExtraTitle = new System.Windows.Forms.ColumnHeader();
            this.colExtraCategory = new System.Windows.Forms.ColumnHeader();
            this.btnExtraAdd = new System.Windows.Forms.ToolStripButton();
            this.btnExtraEdit = new System.Windows.Forms.ToolStripButton();
            this.btnExtraDelete = new System.Windows.Forms.ToolStripButton();
            this.localizer = new GameCatalog.UI.Components.Localizer(this.components);
            this.tsExtras.SuspendLayout();
            this.SuspendLayout();
            //
            // lvExtras
            //
            this.lvExtras.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvExtras.FullRowSelect = true;
            this.lvExtras.HideSelection = false;
            this.lvExtras.MultiSelect = false;
            this.lvExtras.View = System.Windows.Forms.View.Details;
            this.lvExtras.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colExtraTitle,
            this.colExtraCategory});
            this.lvExtras.Name = "lvExtras";
            this.lvExtras.TabIndex = 0;
            //
            // tsExtras
            //
            this.tsExtras.Dock = System.Windows.Forms.DockStyle.Top;
            this.tsExtras.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.tsExtras.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnExtraAdd,
            this.btnExtraEdit,
            this.btnExtraDelete});
            this.tsExtras.Name = "tsExtras";
            this.tsExtras.TabIndex = 1;
            //
            // lblExtrasTitle
            //
            this.lblExtrasTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblExtrasTitle.Height = 22;
            this.lblExtrasTitle.Text = "Dodatki";
            this.lblExtrasTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblExtrasTitle.Name = "lblExtrasTitle";
            this.lblExtrasTitle.TabIndex = 2;
            //
            // localizer
            //
            this.localizer.ContainerControl = this;
            this.colExtraTitle.Name = "colExtraTitle";
            this.colExtraTitle.Text = "Tytuł";
            this.colExtraTitle.Width = 220;
            this.colExtraCategory.Name = "colExtraCategory";
            this.colExtraCategory.Text = "Kategoria";
            this.colExtraCategory.Width = 120;
            this.btnExtraAdd.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnExtraAdd.Name = "btnExtraAdd";
            this.btnExtraEdit.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnExtraEdit.Name = "btnExtraEdit";
            this.btnExtraDelete.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnExtraDelete.Name = "btnExtraDelete";
            //
            // GameExtrasFrame
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lvExtras);
            this.Controls.Add(this.tsExtras);
            this.Controls.Add(this.lblExtrasTitle);
            this.Name = "GameExtrasFrame";
            this.Size = new System.Drawing.Size(400, 300);
            this.tsExtras.ResumeLayout(false);
            this.tsExtras.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView lvExtras;
        private System.Windows.Forms.ToolStrip tsExtras;
        private System.Windows.Forms.Label lblExtrasTitle;
        private System.Windows.Forms.ColumnHeader colExtraTitle;
        private System.Windows.Forms.ColumnHeader colExtraCategory;
        private System.Windows.Forms.ToolStripButton btnExtraAdd;
        private System.Windows.Forms.ToolStripButton btnExtraEdit;
        private System.Windows.Forms.ToolStripButton btnExtraDelete;
        private GameCatalog.UI.Components.Localizer localizer;
    }
}
