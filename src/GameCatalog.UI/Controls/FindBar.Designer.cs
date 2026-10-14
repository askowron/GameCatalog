namespace GameCatalog.UI.Controls
{
    partial class FindBar
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
            this.tlpFind = new System.Windows.Forms.TableLayoutPanel();
            this.lblField = new System.Windows.Forms.Label();
            this.cbxField = new System.Windows.Forms.ComboBox();
            this.chkWhole = new System.Windows.Forms.CheckBox();
            this.chkShow = new System.Windows.Forms.CheckBox();
            this.chkReverse = new System.Windows.Forms.CheckBox();
            this.lblValue = new System.Windows.Forms.Label();
            this.txtValue = new System.Windows.Forms.TextBox();
            this.btnNext = new System.Windows.Forms.Button();
            this.btnHide = new System.Windows.Forms.Button();
            this.localizer = new GameCatalog.UI.Components.Localizer(this.components);
            this.tlpFind.SuspendLayout();
            this.SuspendLayout();
            //
            // tlpFind
            //
            this.tlpFind.ColumnCount = 6;
            this.tlpFind.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpFind.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpFind.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpFind.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpFind.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpFind.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpFind.Controls.Add(this.lblField, 0, 0);
            this.tlpFind.Controls.Add(this.cbxField, 1, 0);
            this.tlpFind.Controls.Add(this.chkWhole, 2, 0);
            this.tlpFind.Controls.Add(this.chkShow, 3, 0);
            this.tlpFind.Controls.Add(this.chkReverse, 4, 0);
            this.tlpFind.Controls.Add(this.lblValue, 0, 1);
            this.tlpFind.Controls.Add(this.txtValue, 1, 1);
            this.tlpFind.Controls.Add(this.btnNext, 2, 1);
            this.tlpFind.Controls.Add(this.btnHide, 4, 1);
            this.tlpFind.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpFind.Location = new System.Drawing.Point(0, 0);
            this.tlpFind.Name = "tlpFind";
            this.tlpFind.RowCount = 2;
            this.tlpFind.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpFind.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpFind.Size = new System.Drawing.Size(556, 56);
            this.tlpFind.TabIndex = 0;
            //
            // lblField
            //
            this.lblField.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblField.AutoSize = true;
            this.lblField.Location = new System.Drawing.Point(3, 6);
            this.lblField.Name = "lblField";
            this.lblField.Size = new System.Drawing.Size(86, 15);
            this.lblField.TabIndex = 0;
            this.lblField.Text = "Szukaj w &polu:";
            //
            // cbxField
            //
            this.cbxField.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.cbxField.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxField.FormattingEnabled = true;
            this.cbxField.Location = new System.Drawing.Point(120, 3);
            this.cbxField.Name = "cbxField";
            this.cbxField.Size = new System.Drawing.Size(150, 23);
            this.cbxField.TabIndex = 1;
            //
            // chkWhole
            //
            this.chkWhole.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkWhole.AutoSize = true;
            this.chkWhole.Location = new System.Drawing.Point(276, 4);
            this.chkWhole.Name = "chkWhole";
            this.chkWhole.Size = new System.Drawing.Size(80, 19);
            this.chkWhole.TabIndex = 2;
            this.chkWhole.Text = "Całe pole";
            this.chkWhole.UseVisualStyleBackColor = true;
            //
            // chkShow
            //
            this.chkShow.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkShow.AutoSize = true;
            this.chkShow.Location = new System.Drawing.Point(362, 4);
            this.chkShow.Name = "chkShow";
            this.chkShow.Size = new System.Drawing.Size(92, 19);
            this.chkShow.TabIndex = 3;
            this.chkShow.Text = "Pokaż wyniki";
            this.chkShow.UseVisualStyleBackColor = true;
            //
            // chkReverse
            //
            this.chkReverse.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkReverse.AutoSize = true;
            this.chkReverse.Location = new System.Drawing.Point(460, 4);
            this.chkReverse.Name = "chkReverse";
            this.chkReverse.Size = new System.Drawing.Size(93, 19);
            this.chkReverse.TabIndex = 4;
            this.chkReverse.Text = "Odwróć wyniki";
            this.chkReverse.UseVisualStyleBackColor = true;
            //
            // lblValue
            //
            this.lblValue.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblValue.AutoSize = true;
            this.lblValue.Location = new System.Drawing.Point(3, 34);
            this.lblValue.Name = "lblValue";
            this.lblValue.Size = new System.Drawing.Size(111, 15);
            this.lblValue.TabIndex = 5;
            this.lblValue.Text = "Szukana &wartość:";
            //
            // txtValue
            //
            this.txtValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtValue.Location = new System.Drawing.Point(120, 31);
            this.txtValue.Name = "txtValue";
            this.txtValue.Size = new System.Drawing.Size(150, 23);
            this.txtValue.TabIndex = 6;
            //
            // btnNext
            //
            this.btnNext.AutoSize = true;
            this.tlpFind.SetColumnSpan(this.btnNext, 2);
            this.btnNext.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnNext.Location = new System.Drawing.Point(276, 31);
            this.btnNext.Name = "btnNext";
            this.btnNext.Size = new System.Drawing.Size(178, 22);
            this.btnNext.TabIndex = 7;
            this.btnNext.Text = "Znajdź następny";
            this.btnNext.UseVisualStyleBackColor = true;
            //
            // btnHide
            //
            this.btnHide.AutoSize = true;
            this.btnHide.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnHide.Location = new System.Drawing.Point(460, 31);
            this.btnHide.Name = "btnHide";
            this.btnHide.Size = new System.Drawing.Size(93, 22);
            this.btnHide.TabIndex = 8;
            this.btnHide.Text = "Ukryj";
            this.btnHide.UseVisualStyleBackColor = true;
            //
            // localizer
            //
            this.localizer.ContainerControl = this;
            //
            // FindBar
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.tlpFind);
            this.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.Name = "FindBar";
            this.Size = new System.Drawing.Size(558, 58);
            this.tlpFind.ResumeLayout(false);
            this.tlpFind.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tlpFind;
        private System.Windows.Forms.Label lblField;
        private System.Windows.Forms.ComboBox cbxField;
        private System.Windows.Forms.CheckBox chkWhole;
        private System.Windows.Forms.CheckBox chkShow;
        private System.Windows.Forms.CheckBox chkReverse;
        private System.Windows.Forms.Label lblValue;
        private System.Windows.Forms.TextBox txtValue;
        private System.Windows.Forms.Button btnNext;
        private System.Windows.Forms.Button btnHide;
        private GameCatalog.UI.Components.Localizer localizer;
    }
}
