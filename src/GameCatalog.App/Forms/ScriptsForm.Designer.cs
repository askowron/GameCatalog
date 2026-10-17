namespace GameCatalog.App.Forms
{
    partial class ScriptsForm
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
            this.lstScripts = new System.Windows.Forms.ListBox();
            this.pnlLeftButtons = new System.Windows.Forms.FlowLayoutPanel();
            this.btnNew = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.pnlRight = new System.Windows.Forms.Panel();
            this.splitMain = new System.Windows.Forms.SplitContainer();
            this.txtEditor = new System.Windows.Forms.TextBox();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.lblLog = new System.Windows.Forms.Label();
            this.pnlProperties = new System.Windows.Forms.Panel();
            this.lblProperties = new System.Windows.Forms.Label();
            this.pnlButtons = new System.Windows.Forms.FlowLayoutPanel();
            this.btnRun = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.localizer = new GameCatalog.UI.Components.Localizer(this.components);
            this.pnlLeft.SuspendLayout();
            this.pnlLeftButtons.SuspendLayout();
            this.pnlRight.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).BeginInit();
            this.splitMain.Panel1.SuspendLayout();
            this.splitMain.Panel2.SuspendLayout();
            this.splitMain.SuspendLayout();
            this.pnlProperties.SuspendLayout();
            this.pnlButtons.SuspendLayout();
            this.SuspendLayout();
            //
            // pnlLeft
            //
            this.pnlLeft.Controls.Add(this.lstScripts);
            this.pnlLeft.Controls.Add(this.pnlLeftButtons);
            this.pnlLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlLeft.Location = new System.Drawing.Point(0, 0);
            this.pnlLeft.Name = "pnlLeft";
            this.pnlLeft.Size = new System.Drawing.Size(220, 561);
            this.pnlLeft.TabIndex = 1;
            //
            // lstScripts
            //
            this.lstScripts.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstScripts.IntegralHeight = false;
            this.lstScripts.ItemHeight = 15;
            this.lstScripts.Location = new System.Drawing.Point(0, 0);
            this.lstScripts.Name = "lstScripts";
            this.lstScripts.Size = new System.Drawing.Size(220, 521);
            this.lstScripts.TabIndex = 0;
            //
            // pnlLeftButtons
            //
            this.pnlLeftButtons.Controls.Add(this.btnNew);
            this.pnlLeftButtons.Controls.Add(this.btnDelete);
            this.pnlLeftButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlLeftButtons.Location = new System.Drawing.Point(0, 521);
            this.pnlLeftButtons.Name = "pnlLeftButtons";
            this.pnlLeftButtons.Padding = new System.Windows.Forms.Padding(4);
            this.pnlLeftButtons.Size = new System.Drawing.Size(220, 40);
            this.pnlLeftButtons.TabIndex = 1;
            //
            // btnNew
            //
            this.btnNew.AutoSize = true;
            this.btnNew.Name = "btnNew";
            this.btnNew.TabIndex = 0;
            this.btnNew.Text = "Nowy";
            this.btnNew.UseVisualStyleBackColor = true;
            //
            // btnDelete
            //
            this.btnDelete.AutoSize = true;
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.TabIndex = 1;
            this.btnDelete.Text = "Usuń";
            this.btnDelete.UseVisualStyleBackColor = true;
            //
            // pnlRight
            //
            this.pnlRight.Controls.Add(this.splitMain);
            this.pnlRight.Controls.Add(this.pnlProperties);
            this.pnlRight.Controls.Add(this.pnlButtons);
            this.pnlRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlRight.Location = new System.Drawing.Point(220, 0);
            this.pnlRight.Name = "pnlRight";
            this.pnlRight.Size = new System.Drawing.Size(664, 561);
            this.pnlRight.TabIndex = 0;
            //
            // splitMain
            //
            this.splitMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitMain.Location = new System.Drawing.Point(0, 88);
            this.splitMain.Name = "splitMain";
            this.splitMain.Orientation = System.Windows.Forms.Orientation.Horizontal;
            //
            // splitMain.Panel1
            //
            this.splitMain.Panel1.Controls.Add(this.txtEditor);
            //
            // splitMain.Panel2
            //
            this.splitMain.Panel2.Controls.Add(this.txtLog);
            this.splitMain.Panel2.Controls.Add(this.lblLog);
            this.splitMain.Size = new System.Drawing.Size(664, 429);
            this.splitMain.SplitterDistance = 350;
            this.splitMain.TabIndex = 0;
            //
            // txtEditor
            //
            this.txtEditor.AcceptsTab = true;
            this.txtEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtEditor.Font = new System.Drawing.Font("Consolas", 10F);
            this.txtEditor.Multiline = true;
            this.txtEditor.Name = "txtEditor";
            this.txtEditor.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtEditor.TabIndex = 0;
            this.txtEditor.WordWrap = false;
            //
            // txtLog
            //
            this.txtLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ReadOnly = true;
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLog.TabIndex = 1;
            //
            // lblLog
            //
            this.lblLog.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblLog.Name = "lblLog";
            this.lblLog.Size = new System.Drawing.Size(664, 20);
            this.lblLog.TabIndex = 0;
            this.lblLog.Text = "Dziennik:";
            //
            // pnlProperties
            //
            this.pnlProperties.Controls.Add(this.lblProperties);
            this.pnlProperties.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlProperties.Location = new System.Drawing.Point(0, 0);
            this.pnlProperties.Name = "pnlProperties";
            this.pnlProperties.Size = new System.Drawing.Size(664, 88);
            this.pnlProperties.TabIndex = 1;
            //
            // lblProperties
            //
            this.lblProperties.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblProperties.Name = "lblProperties";
            this.lblProperties.Padding = new System.Windows.Forms.Padding(6);
            this.lblProperties.TabIndex = 0;
            //
            // pnlButtons
            //
            this.pnlButtons.Controls.Add(this.btnRun);
            this.pnlButtons.Controls.Add(this.btnSave);
            this.pnlButtons.Controls.Add(this.btnClose);
            this.pnlButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlButtons.Location = new System.Drawing.Point(0, 517);
            this.pnlButtons.Name = "pnlButtons";
            this.pnlButtons.Padding = new System.Windows.Forms.Padding(8);
            this.pnlButtons.Size = new System.Drawing.Size(664, 44);
            this.pnlButtons.TabIndex = 2;
            //
            // btnRun
            //
            this.btnRun.AutoSize = true;
            this.btnRun.Name = "btnRun";
            this.btnRun.TabIndex = 0;
            this.btnRun.Text = "Uruchom testowo";
            this.btnRun.UseVisualStyleBackColor = true;
            //
            // btnSave
            //
            this.btnSave.AutoSize = true;
            this.btnSave.Name = "btnSave";
            this.btnSave.TabIndex = 1;
            this.btnSave.Text = "Zapisz";
            this.btnSave.UseVisualStyleBackColor = true;
            //
            // btnClose
            //
            this.btnClose.AutoSize = true;
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.Name = "btnClose";
            this.btnClose.TabIndex = 2;
            this.btnClose.Text = "Zamknij";
            this.btnClose.UseVisualStyleBackColor = true;
            //
            // localizer
            //
            this.localizer.ContainerControl = this;
            //
            // ScriptsForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(884, 561);
            this.Controls.Add(this.pnlRight);
            this.Controls.Add(this.pnlLeft);
            this.Name = "ScriptsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Skrypty";
            this.pnlLeft.ResumeLayout(false);
            this.pnlLeftButtons.ResumeLayout(false);
            this.pnlLeftButtons.PerformLayout();
            this.pnlRight.ResumeLayout(false);
            this.splitMain.Panel1.ResumeLayout(false);
            this.splitMain.Panel1.PerformLayout();
            this.splitMain.Panel2.ResumeLayout(false);
            this.splitMain.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).EndInit();
            this.splitMain.ResumeLayout(false);
            this.pnlProperties.ResumeLayout(false);
            this.pnlButtons.ResumeLayout(false);
            this.pnlButtons.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlLeft;
        private System.Windows.Forms.ListBox lstScripts;
        private System.Windows.Forms.FlowLayoutPanel pnlLeftButtons;
        private System.Windows.Forms.Button btnNew;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Panel pnlRight;
        private System.Windows.Forms.SplitContainer splitMain;
        private System.Windows.Forms.TextBox txtEditor;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.Label lblLog;
        private System.Windows.Forms.Panel pnlProperties;
        private System.Windows.Forms.Label lblProperties;
        private System.Windows.Forms.FlowLayoutPanel pnlButtons;
        private System.Windows.Forms.Button btnRun;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnClose;
        private GameCatalog.UI.Components.Localizer localizer;
    }
}
