namespace GameCatalog.App.Forms
{
    partial class ScriptResultForm
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
            this.clbFields = new System.Windows.Forms.CheckedListBox();
            this.lblFields = new System.Windows.Forms.Label();
            this.lstExtras = new System.Windows.Forms.ListBox();
            this.lblExtras = new System.Windows.Forms.Label();
            this.chkCover = new System.Windows.Forms.CheckBox();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.pnlButtons = new System.Windows.Forms.FlowLayoutPanel();
            this.btnApply = new System.Windows.Forms.Button();
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
            this.splitMain.Orientation = System.Windows.Forms.Orientation.Horizontal;
            //
            // splitMain.Panel1
            //
            this.splitMain.Panel1.Controls.Add(this.clbFields);
            this.splitMain.Panel1.Controls.Add(this.lblFields);
            //
            // splitMain.Panel2
            //
            this.splitMain.Panel2.Controls.Add(this.lstExtras);
            this.splitMain.Panel2.Controls.Add(this.lblExtras);
            this.splitMain.Panel2.Controls.Add(this.chkCover);
            this.splitMain.Size = new System.Drawing.Size(544, 307);
            this.splitMain.SplitterDistance = 220;
            this.splitMain.TabIndex = 0;
            //
            // clbFields
            //
            this.clbFields.CheckOnClick = true;
            this.clbFields.Dock = System.Windows.Forms.DockStyle.Fill;
            this.clbFields.IntegralHeight = false;
            this.clbFields.Location = new System.Drawing.Point(0, 30);
            this.clbFields.Name = "clbFields";
            this.clbFields.Size = new System.Drawing.Size(544, 190);
            this.clbFields.TabIndex = 1;
            //
            // lblFields
            //
            this.lblFields.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblFields.Location = new System.Drawing.Point(0, 0);
            this.lblFields.Name = "lblFields";
            this.lblFields.Size = new System.Drawing.Size(544, 30);
            this.lblFields.TabIndex = 0;
            this.lblFields.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // lstExtras
            //
            this.lstExtras.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstExtras.IntegralHeight = false;
            this.lstExtras.ItemHeight = 15;
            this.lstExtras.Location = new System.Drawing.Point(0, 41);
            this.lstExtras.Name = "lstExtras";
            this.lstExtras.Size = new System.Drawing.Size(544, 42);
            this.lstExtras.TabIndex = 2;
            //
            // lblExtras
            //
            this.lblExtras.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblExtras.Location = new System.Drawing.Point(0, 19);
            this.lblExtras.Name = "lblExtras";
            this.lblExtras.Size = new System.Drawing.Size(544, 22);
            this.lblExtras.TabIndex = 1;
            //
            // chkCover
            //
            this.chkCover.AutoSize = true;
            this.chkCover.Dock = System.Windows.Forms.DockStyle.Top;
            this.chkCover.Location = new System.Drawing.Point(0, 0);
            this.chkCover.Name = "chkCover";
            this.chkCover.Size = new System.Drawing.Size(544, 19);
            this.chkCover.TabIndex = 0;
            this.chkCover.UseVisualStyleBackColor = true;
            //
            // txtLog
            //
            this.txtLog.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.txtLog.Location = new System.Drawing.Point(0, 307);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ReadOnly = true;
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLog.Size = new System.Drawing.Size(544, 90);
            this.txtLog.TabIndex = 1;
            //
            // pnlButtons
            //
            this.pnlButtons.Controls.Add(this.btnApply);
            this.pnlButtons.Controls.Add(this.btnCancel);
            this.pnlButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlButtons.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.pnlButtons.Location = new System.Drawing.Point(0, 397);
            this.pnlButtons.Name = "pnlButtons";
            this.pnlButtons.Padding = new System.Windows.Forms.Padding(8);
            this.pnlButtons.Size = new System.Drawing.Size(544, 44);
            this.pnlButtons.TabIndex = 2;
            //
            // btnApply
            //
            this.btnApply.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnApply.Location = new System.Drawing.Point(435, 11);
            this.btnApply.Name = "btnApply";
            this.btnApply.Size = new System.Drawing.Size(90, 23);
            this.btnApply.TabIndex = 0;
            this.btnApply.Text = "Zastosuj";
            this.btnApply.UseVisualStyleBackColor = true;
            //
            // btnCancel
            //
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(339, 11);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(90, 23);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "Anuluj";
            this.btnCancel.UseVisualStyleBackColor = true;
            //
            // localizer
            //
            this.localizer.ContainerControl = this;
            //
            // ScriptResultForm
            //
            this.AcceptButton = this.btnApply;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(544, 441);
            this.Controls.Add(this.splitMain);
            this.Controls.Add(this.txtLog);
            this.Controls.Add(this.pnlButtons);
            this.Name = "ScriptResultForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Wyniki skryptu";
            this.splitMain.Panel1.ResumeLayout(false);
            this.splitMain.Panel2.ResumeLayout(false);
            this.splitMain.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).EndInit();
            this.splitMain.ResumeLayout(false);
            this.pnlButtons.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitMain;
        private System.Windows.Forms.CheckedListBox clbFields;
        private System.Windows.Forms.Label lblFields;
        private System.Windows.Forms.ListBox lstExtras;
        private System.Windows.Forms.Label lblExtras;
        private System.Windows.Forms.CheckBox chkCover;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.FlowLayoutPanel pnlButtons;
        private System.Windows.Forms.Button btnApply;
        private System.Windows.Forms.Button btnCancel;
        private GameCatalog.UI.Components.Localizer localizer;
    }
}
