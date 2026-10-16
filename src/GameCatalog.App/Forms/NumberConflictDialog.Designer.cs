namespace GameCatalog.App.Forms
{
    partial class NumberConflictDialog
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
            this.lblNumber = new System.Windows.Forms.Label();
            this.numNumber = new System.Windows.Forms.NumericUpDown();
            this.btnFind = new System.Windows.Forms.Button();
            this.lblTakenBy = new System.Windows.Forms.Label();
            this.grpResolution = new System.Windows.Forms.GroupBox();
            this.rbAcceptDuplicate = new System.Windows.Forms.RadioButton();
            this.rbRenumberExisting = new System.Windows.Forms.RadioButton();
            this.rbShiftNew = new System.Windows.Forms.RadioButton();
            this.chkDontAskAgain = new System.Windows.Forms.CheckBox();
            this.pnlButtons = new System.Windows.Forms.Panel();
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.localizer = new GameCatalog.UI.Components.Localizer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.numNumber)).BeginInit();
            this.grpResolution.SuspendLayout();
            this.pnlButtons.SuspendLayout();
            this.SuspendLayout();
            //
            // lblNumber
            //
            this.lblNumber.AutoSize = true;
            this.lblNumber.Location = new System.Drawing.Point(12, 14);
            this.lblNumber.Name = "lblNumber";
            this.lblNumber.Size = new System.Drawing.Size(140, 15);
            this.lblNumber.TabIndex = 0;
            this.lblNumber.Text = "Wprowadź numer dla gry:";
            //
            // numNumber
            //
            this.numNumber.Location = new System.Drawing.Point(170, 11);
            this.numNumber.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            this.numNumber.Name = "numNumber";
            this.numNumber.Size = new System.Drawing.Size(130, 23);
            this.numNumber.TabIndex = 1;
            this.numNumber.Value = new decimal(new int[] { 1, 0, 0, 0 });
            //
            // btnFind
            //
            this.btnFind.Location = new System.Drawing.Point(306, 10);
            this.btnFind.Name = "btnFind";
            this.btnFind.Size = new System.Drawing.Size(26, 24);
            this.btnFind.TabIndex = 2;
            this.toolTip.SetToolTip(this.btnFind, "Znajdź pierwszy wolny numer");
            this.btnFind.UseVisualStyleBackColor = true;
            //
            // lblTakenBy
            //
            this.lblTakenBy.AutoEllipsis = true;
            this.lblTakenBy.Location = new System.Drawing.Point(12, 40);
            this.lblTakenBy.Name = "lblTakenBy";
            this.lblTakenBy.Size = new System.Drawing.Size(345, 18);
            this.lblTakenBy.TabIndex = 3;
            //
            // grpResolution
            //
            this.grpResolution.Controls.Add(this.rbAcceptDuplicate);
            this.grpResolution.Controls.Add(this.rbRenumberExisting);
            this.grpResolution.Controls.Add(this.rbShiftNew);
            this.grpResolution.Location = new System.Drawing.Point(12, 62);
            this.grpResolution.Name = "grpResolution";
            this.grpResolution.Size = new System.Drawing.Size(345, 108);
            this.grpResolution.TabIndex = 4;
            this.grpResolution.TabStop = false;
            //
            // rbAcceptDuplicate
            //
            this.rbAcceptDuplicate.AutoSize = true;
            this.rbAcceptDuplicate.Location = new System.Drawing.Point(10, 14);
            this.rbAcceptDuplicate.Name = "rbAcceptDuplicate";
            this.rbAcceptDuplicate.Size = new System.Drawing.Size(119, 19);
            this.rbAcceptDuplicate.TabIndex = 0;
            this.rbAcceptDuplicate.Text = "Akceptuj duplikat";
            this.rbAcceptDuplicate.UseVisualStyleBackColor = true;
            //
            // rbRenumberExisting
            //
            this.rbRenumberExisting.AutoSize = true;
            this.rbRenumberExisting.Location = new System.Drawing.Point(10, 40);
            this.rbRenumberExisting.Name = "rbRenumberExisting";
            this.rbRenumberExisting.Size = new System.Drawing.Size(211, 19);
            this.rbRenumberExisting.TabIndex = 1;
            this.rbRenumberExisting.Text = "Nadaj inny numer istniejącej grze";
            this.rbRenumberExisting.UseVisualStyleBackColor = true;
            //
            // rbShiftNew
            //
            this.rbShiftNew.AutoSize = true;
            this.rbShiftNew.Checked = true;
            this.rbShiftNew.Location = new System.Drawing.Point(10, 66);
            this.rbShiftNew.Name = "rbShiftNew";
            this.rbShiftNew.Size = new System.Drawing.Size(320, 19);
            this.rbShiftNew.TabIndex = 2;
            this.rbShiftNew.TabStop = true;
            this.rbShiftNew.Text = "Przesuwaj numer gry do czasu, aż nie będzie zduplikowany";
            this.rbShiftNew.UseVisualStyleBackColor = true;
            //
            // chkDontAskAgain
            //
            this.chkDontAskAgain.AutoSize = true;
            this.chkDontAskAgain.Location = new System.Drawing.Point(14, 178);
            this.chkDontAskAgain.Name = "chkDontAskAgain";
            this.chkDontAskAgain.Size = new System.Drawing.Size(316, 19);
            this.chkDontAskAgain.TabIndex = 5;
            this.chkDontAskAgain.Text = "Nie pytaj ponownie; użyj automatycznego numerowania";
            this.chkDontAskAgain.UseVisualStyleBackColor = true;
            //
            // pnlButtons
            //
            this.pnlButtons.Controls.Add(this.btnOk);
            this.pnlButtons.Controls.Add(this.btnCancel);
            this.pnlButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlButtons.Location = new System.Drawing.Point(0, 201);
            this.pnlButtons.Name = "pnlButtons";
            this.pnlButtons.Size = new System.Drawing.Size(369, 44);
            this.pnlButtons.TabIndex = 6;
            //
            // btnOk
            //
            this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOk.Location = new System.Drawing.Point(100, 10);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(80, 24);
            this.btnOk.TabIndex = 0;
            this.btnOk.Text = "OK";
            this.btnOk.UseVisualStyleBackColor = true;
            //
            // btnCancel
            //
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(190, 10);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(80, 24);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "Anuluj";
            this.btnCancel.UseVisualStyleBackColor = true;
            //
            // localizer
            //
            this.localizer.ContainerControl = this;
            //
            // NumberConflictDialog
            //
            this.AcceptButton = this.btnOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(369, 245);
            this.Controls.Add(this.lblNumber);
            this.Controls.Add(this.numNumber);
            this.Controls.Add(this.btnFind);
            this.Controls.Add(this.lblTakenBy);
            this.Controls.Add(this.grpResolution);
            this.Controls.Add(this.chkDontAskAgain);
            this.Controls.Add(this.pnlButtons);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "NumberConflictDialog";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Wybór numeru gry";
            ((System.ComponentModel.ISupportInitialize)(this.numNumber)).EndInit();
            this.grpResolution.ResumeLayout(false);
            this.grpResolution.PerformLayout();
            this.pnlButtons.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblNumber;
        private System.Windows.Forms.NumericUpDown numNumber;
        private System.Windows.Forms.Button btnFind;
        private System.Windows.Forms.Label lblTakenBy;
        private System.Windows.Forms.GroupBox grpResolution;
        private System.Windows.Forms.RadioButton rbAcceptDuplicate;
        private System.Windows.Forms.RadioButton rbRenumberExisting;
        private System.Windows.Forms.RadioButton rbShiftNew;
        private System.Windows.Forms.CheckBox chkDontAskAgain;
        private System.Windows.Forms.Panel pnlButtons;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.ToolTip toolTip;
        private GameCatalog.UI.Components.Localizer localizer;
    }
}
