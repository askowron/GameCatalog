namespace GameCatalog.App.Forms
{
    partial class CustomFieldEditForm
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
            this.tlpFields = new System.Windows.Forms.TableLayoutPanel();
            this.lblTag = new System.Windows.Forms.Label();
            this.txtTag = new System.Windows.Forms.TextBox();
            this.lblName = new System.Windows.Forms.Label();
            this.txtName = new System.Windows.Forms.TextBox();
            this.lblType = new System.Windows.Forms.Label();
            this.cbxType = new System.Windows.Forms.ComboBox();
            this.lblListValues = new System.Windows.Forms.Label();
            this.txtListValues = new System.Windows.Forms.TextBox();
            this.pnlButtons = new System.Windows.Forms.FlowLayoutPanel();
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.localizer = new GameCatalog.UI.Components.Localizer(this.components);
            this.tlpFields.SuspendLayout();
            this.pnlButtons.SuspendLayout();
            this.SuspendLayout();
            //
            // tlpFields
            //
            this.tlpFields.ColumnCount = 2;
            this.tlpFields.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            this.tlpFields.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpFields.Controls.Add(this.lblTag, 0, 0);
            this.tlpFields.Controls.Add(this.txtTag, 1, 0);
            this.tlpFields.Controls.Add(this.lblName, 0, 1);
            this.tlpFields.Controls.Add(this.txtName, 1, 1);
            this.tlpFields.Controls.Add(this.lblType, 0, 2);
            this.tlpFields.Controls.Add(this.cbxType, 1, 2);
            this.tlpFields.Controls.Add(this.lblListValues, 0, 3);
            this.tlpFields.Controls.Add(this.txtListValues, 1, 3);
            this.tlpFields.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpFields.Location = new System.Drawing.Point(0, 0);
            this.tlpFields.Name = "tlpFields";
            this.tlpFields.Padding = new System.Windows.Forms.Padding(10);
            this.tlpFields.RowCount = 5;
            this.tlpFields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.tlpFields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.tlpFields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.tlpFields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.tlpFields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpFields.Size = new System.Drawing.Size(384, 197);
            this.tlpFields.TabIndex = 0;
            //
            // lblTag
            //
            this.lblTag.AutoSize = true;
            this.lblTag.Location = new System.Drawing.Point(13, 10);
            this.lblTag.Name = "lblTag";
            this.lblTag.Size = new System.Drawing.Size(28, 15);
            this.lblTag.TabIndex = 0;
            this.lblTag.Text = "Tag:";
            //
            // txtTag
            //
            this.txtTag.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtTag.Location = new System.Drawing.Point(122, 13);
            this.txtTag.Name = "txtTag";
            this.txtTag.Size = new System.Drawing.Size(249, 23);
            this.txtTag.TabIndex = 1;
            //
            // lblName
            //
            this.lblName.AutoSize = true;
            this.lblName.Location = new System.Drawing.Point(13, 39);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(45, 15);
            this.lblName.TabIndex = 2;
            this.lblName.Text = "Nazwa:";
            //
            // txtName
            //
            this.txtName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtName.Location = new System.Drawing.Point(122, 42);
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(249, 23);
            this.txtName.TabIndex = 3;
            //
            // lblType
            //
            this.lblType.AutoSize = true;
            this.lblType.Location = new System.Drawing.Point(13, 68);
            this.lblType.Name = "lblType";
            this.lblType.Size = new System.Drawing.Size(29, 15);
            this.lblType.TabIndex = 4;
            this.lblType.Text = "Typ:";
            //
            // cbxType
            //
            this.cbxType.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cbxType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxType.Items.AddRange(new object[] {
            "Text",
            "Number",
            "Date",
            "List",
            "Url",
            "YesNo"});
            this.cbxType.Location = new System.Drawing.Point(122, 71);
            this.cbxType.Name = "cbxType";
            this.cbxType.Size = new System.Drawing.Size(249, 23);
            this.cbxType.TabIndex = 5;
            //
            // lblListValues
            //
            this.lblListValues.AutoSize = true;
            this.lblListValues.Location = new System.Drawing.Point(13, 97);
            this.lblListValues.Name = "lblListValues";
            this.lblListValues.Size = new System.Drawing.Size(103, 15);
            this.lblListValues.TabIndex = 6;
            this.lblListValues.Text = "Wartości listy (;):";
            //
            // txtListValues
            //
            this.txtListValues.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtListValues.Location = new System.Drawing.Point(122, 100);
            this.txtListValues.Name = "txtListValues";
            this.txtListValues.Size = new System.Drawing.Size(249, 23);
            this.txtListValues.TabIndex = 7;
            //
            // pnlButtons
            //
            this.pnlButtons.Controls.Add(this.btnOk);
            this.pnlButtons.Controls.Add(this.btnCancel);
            this.pnlButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlButtons.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.pnlButtons.Location = new System.Drawing.Point(0, 197);
            this.pnlButtons.Name = "pnlButtons";
            this.pnlButtons.Padding = new System.Windows.Forms.Padding(8);
            this.pnlButtons.Size = new System.Drawing.Size(384, 44);
            this.pnlButtons.TabIndex = 1;
            //
            // btnOk
            //
            this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOk.Location = new System.Drawing.Point(285, 11);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(80, 23);
            this.btnOk.TabIndex = 0;
            this.btnOk.Text = "OK";
            this.btnOk.UseVisualStyleBackColor = true;
            //
            // btnCancel
            //
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(199, 11);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(80, 23);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "Anuluj";
            this.btnCancel.UseVisualStyleBackColor = true;
            //
            // localizer
            //
            this.localizer.ContainerControl = this;
            //
            // CustomFieldEditForm
            //
            this.AcceptButton = this.btnOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(384, 241);
            this.Controls.Add(this.tlpFields);
            this.Controls.Add(this.pnlButtons);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CustomFieldEditForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Pole własne";
            this.tlpFields.ResumeLayout(false);
            this.tlpFields.PerformLayout();
            this.pnlButtons.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tlpFields;
        private System.Windows.Forms.Label lblTag;
        private System.Windows.Forms.TextBox txtTag;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.Label lblType;
        private System.Windows.Forms.ComboBox cbxType;
        private System.Windows.Forms.Label lblListValues;
        private System.Windows.Forms.TextBox txtListValues;
        private System.Windows.Forms.FlowLayoutPanel pnlButtons;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
        private GameCatalog.UI.Components.Localizer localizer;
    }
}
