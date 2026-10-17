namespace GameCatalog.App.Forms
{
    partial class LanguageSelectForm
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
            this.lblChoose = new System.Windows.Forms.Label();
            this.lvLanguages = new System.Windows.Forms.ListView();
            this.colLanguageName = new System.Windows.Forms.ColumnHeader();
            this.colLanguageAuthor = new System.Windows.Forms.ColumnHeader();
            this.pnlButtons = new System.Windows.Forms.FlowLayoutPanel();
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.localizer = new GameCatalog.UI.Components.Localizer(this.components);
            this.pnlButtons.SuspendLayout();
            this.SuspendLayout();
            //
            // lblChoose
            //
            this.lblChoose.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblChoose.Location = new System.Drawing.Point(8, 8);
            this.lblChoose.Name = "lblChoose";
            this.lblChoose.Size = new System.Drawing.Size(328, 24);
            this.lblChoose.TabIndex = 0;
            this.lblChoose.Text = "Wybierz język interfejsu:";
            this.lblChoose.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // lvLanguages
            //
            this.lvLanguages.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colLanguageName,
            this.colLanguageAuthor});
            this.lvLanguages.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvLanguages.FullRowSelect = true;
            this.lvLanguages.HideSelection = false;
            this.lvLanguages.Location = new System.Drawing.Point(8, 32);
            this.lvLanguages.MultiSelect = false;
            this.lvLanguages.Name = "lvLanguages";
            this.lvLanguages.Size = new System.Drawing.Size(328, 177);
            this.lvLanguages.TabIndex = 1;
            this.lvLanguages.UseCompatibleStateImageBehavior = false;
            this.lvLanguages.View = System.Windows.Forms.View.Details;
            //
            // colLanguageName
            //
            this.colLanguageName.Name = "colLanguageName";
            this.colLanguageName.Text = "Język";
            this.colLanguageName.Width = 170;
            //
            // colLanguageAuthor
            //
            this.colLanguageAuthor.Name = "colLanguageAuthor";
            this.colLanguageAuthor.Text = "Autor";
            this.colLanguageAuthor.Width = 130;
            //
            // pnlButtons
            //
            this.pnlButtons.Controls.Add(this.btnOk);
            this.pnlButtons.Controls.Add(this.btnCancel);
            this.pnlButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlButtons.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.pnlButtons.Location = new System.Drawing.Point(8, 209);
            this.pnlButtons.Name = "pnlButtons";
            this.pnlButtons.Padding = new System.Windows.Forms.Padding(0, 8, 0, 0);
            this.pnlButtons.Size = new System.Drawing.Size(328, 44);
            this.pnlButtons.TabIndex = 2;
            //
            // btnOk
            //
            this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOk.Location = new System.Drawing.Point(245, 11);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(80, 23);
            this.btnOk.TabIndex = 0;
            this.btnOk.Text = "OK";
            this.btnOk.UseVisualStyleBackColor = true;
            //
            // btnCancel
            //
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(159, 11);
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
            // LanguageSelectForm
            //
            this.AcceptButton = this.btnOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(344, 261);
            this.Controls.Add(this.lvLanguages);
            this.Controls.Add(this.lblChoose);
            this.Controls.Add(this.pnlButtons);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "LanguageSelectForm";
            this.Padding = new System.Windows.Forms.Padding(8);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Język";
            this.pnlButtons.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lblChoose;
        private System.Windows.Forms.ListView lvLanguages;
        private System.Windows.Forms.ColumnHeader colLanguageName;
        private System.Windows.Forms.ColumnHeader colLanguageAuthor;
        private System.Windows.Forms.FlowLayoutPanel pnlButtons;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
        private GameCatalog.UI.Components.Localizer localizer;
    }
}
