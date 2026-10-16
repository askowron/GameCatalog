namespace GameCatalog.App.Forms
{
    partial class HtmlEditorForm
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
            this.splitMain = new System.Windows.Forms.SplitContainer();
            this.txtEditor = new System.Windows.Forms.TextBox();
            this.lstTags = new System.Windows.Forms.ListBox();
            this.lblTags = new System.Windows.Forms.Label();
            this.pnlButtons = new System.Windows.Forms.FlowLayoutPanel();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnReset = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.localizer = new GameCatalog.UI.Components.Localizer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).BeginInit();
            this.splitMain.Panel1.SuspendLayout();
            this.splitMain.Panel2.SuspendLayout();
            this.splitMain.SuspendLayout();
            this.pnlButtons.SuspendLayout();
            this.SuspendLayout();
            //
            // splitMain
            //
            this.splitMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitMain.Location = new System.Drawing.Point(0, 0);
            this.splitMain.Name = "splitMain";
            //
            // splitMain.Panel1
            //
            this.splitMain.Panel1.Controls.Add(this.txtEditor);
            //
            // splitMain.Panel2
            //
            this.splitMain.Panel2.Controls.Add(this.lstTags);
            this.splitMain.Panel2.Controls.Add(this.lblTags);
            this.splitMain.Size = new System.Drawing.Size(744, 457);
            this.splitMain.SplitterDistance = 560;
            this.splitMain.TabIndex = 0;
            //
            // txtEditor
            //
            this.txtEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtEditor.Font = new System.Drawing.Font("Consolas", 9F);
            this.txtEditor.Location = new System.Drawing.Point(0, 0);
            this.txtEditor.Multiline = true;
            this.txtEditor.Name = "txtEditor";
            this.txtEditor.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtEditor.Size = new System.Drawing.Size(560, 457);
            this.txtEditor.TabIndex = 0;
            this.txtEditor.WordWrap = false;
            //
            // lstTags
            //
            this.lstTags.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstTags.IntegralHeight = false;
            this.lstTags.ItemHeight = 15;
            this.lstTags.Items.AddRange(new object[] {
            "{Title}",
            "{LocalizedTitle}",
            "{Developer}",
            "{Publisher}",
            "{Designer}",
            "{Composer}",
            "{VoiceActors}",
            "{Country}",
            "{ReleaseYear}",
            "{PlaytimeHours}",
            "{Genre}",
            "{Pegi}",
            "{Platform}",
            "{Store}",
            "{Engine}",
            "{GameMode}",
            "{Status}",
            "{UserRating}",
            "{CriticRating}",
            "{Url}",
            "{Description}",
            "{Comments}",
            "{Number}",
            "{Cover}",
            "{Extras}"});
            this.lstTags.Location = new System.Drawing.Point(0, 30);
            this.lstTags.Name = "lstTags";
            this.lstTags.Size = new System.Drawing.Size(180, 427);
            this.lstTags.TabIndex = 1;
            //
            // lblTags
            //
            this.lblTags.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblTags.Location = new System.Drawing.Point(0, 0);
            this.lblTags.Name = "lblTags";
            this.lblTags.Size = new System.Drawing.Size(180, 30);
            this.lblTags.TabIndex = 0;
            this.lblTags.Text = "Znaczniki (2×klik wstawia):";
            //
            // pnlButtons
            //
            this.pnlButtons.Controls.Add(this.btnSave);
            this.pnlButtons.Controls.Add(this.btnReset);
            this.pnlButtons.Controls.Add(this.btnCancel);
            this.pnlButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlButtons.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.pnlButtons.Location = new System.Drawing.Point(0, 457);
            this.pnlButtons.Name = "pnlButtons";
            this.pnlButtons.Padding = new System.Windows.Forms.Padding(8);
            this.pnlButtons.Size = new System.Drawing.Size(744, 44);
            this.pnlButtons.TabIndex = 1;
            //
            // btnSave
            //
            this.btnSave.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnSave.Location = new System.Drawing.Point(635, 11);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(90, 23);
            this.btnSave.TabIndex = 0;
            this.btnSave.Text = "Zapisz";
            this.btnSave.UseVisualStyleBackColor = true;
            //
            // btnReset
            //
            this.btnReset.AutoSize = true;
            this.btnReset.Location = new System.Drawing.Point(509, 11);
            this.btnReset.Name = "btnReset";
            this.btnReset.Size = new System.Drawing.Size(120, 25);
            this.btnReset.TabIndex = 1;
            this.btnReset.Text = "Przywróć domyślny";
            this.btnReset.UseVisualStyleBackColor = true;
            //
            // btnCancel
            //
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(413, 11);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(90, 23);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "Anuluj";
            this.btnCancel.UseVisualStyleBackColor = true;
            //
            // localizer
            //
            this.localizer.ContainerControl = this;
            //
            // HtmlEditorForm
            //
            this.AcceptButton = this.btnSave;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(744, 501);
            this.Controls.Add(this.splitMain);
            this.Controls.Add(this.pnlButtons);
            this.Name = "HtmlEditorForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Edytor HTML";
            this.splitMain.Panel1.ResumeLayout(false);
            this.splitMain.Panel1.PerformLayout();
            this.splitMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).EndInit();
            this.splitMain.ResumeLayout(false);
            this.pnlButtons.ResumeLayout(false);
            this.pnlButtons.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitMain;
        private System.Windows.Forms.TextBox txtEditor;
        private System.Windows.Forms.ListBox lstTags;
        private System.Windows.Forms.Label lblTags;
        private System.Windows.Forms.FlowLayoutPanel pnlButtons;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnReset;
        private System.Windows.Forms.Button btnCancel;
        private GameCatalog.UI.Components.Localizer localizer;
    }
}
