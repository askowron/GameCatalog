namespace GameCatalog.UI.Frames
{
    partial class GameAllFieldsFrame
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
            this.gridFields = new System.Windows.Forms.DataGridView();
            this.colField = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colValue = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.localizer = new GameCatalog.UI.Components.Localizer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.gridFields)).BeginInit();
            this.SuspendLayout();
            //
            // gridFields
            //
            this.gridFields.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridFields.AllowUserToAddRows = false;
            this.gridFields.AllowUserToDeleteRows = false;
            this.gridFields.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.gridFields.ReadOnly = true;
            this.gridFields.RowHeadersVisible = false;
            this.gridFields.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colField,
            this.colValue});
            this.gridFields.Name = "gridFields";
            this.gridFields.TabIndex = 0;
            //
            // localizer
            //
            this.localizer.ContainerControl = this;
            this.colField.HeaderText = "Pole";
            this.colField.Name = "colField";
            this.colField.ReadOnly = true;
            this.colValue.HeaderText = "Wartość";
            this.colValue.Name = "colValue";
            this.colValue.ReadOnly = true;
            //
            // GameAllFieldsFrame
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.gridFields);
            this.Name = "GameAllFieldsFrame";
            this.Size = new System.Drawing.Size(400, 300);
            ((System.ComponentModel.ISupportInitialize)(this.gridFields)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView gridFields;
        private System.Windows.Forms.DataGridViewTextBoxColumn colField;
        private System.Windows.Forms.DataGridViewTextBoxColumn colValue;
        private GameCatalog.UI.Components.Localizer localizer;
    }
}
