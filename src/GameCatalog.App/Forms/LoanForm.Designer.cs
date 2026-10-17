namespace GameCatalog.App.Forms
{
    partial class LoanForm
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
            this.grdLoans = new System.Windows.Forms.DataGridView();
            this.colGame = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colBorrower = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colLoanDate = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colReturnDate = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colState = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pnlTop = new System.Windows.Forms.FlowLayoutPanel();
            this.lblGame = new System.Windows.Forms.Label();
            this.cbxGame = new System.Windows.Forms.ComboBox();
            this.lblBorrower = new System.Windows.Forms.Label();
            this.txtBorrower = new System.Windows.Forms.TextBox();
            this.btnBorrow = new System.Windows.Forms.Button();
            this.btnReturn = new System.Windows.Forms.Button();
            this.pnlButtons = new System.Windows.Forms.FlowLayoutPanel();
            this.btnClose = new System.Windows.Forms.Button();
            this.localizer = new GameCatalog.UI.Components.Localizer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.grdLoans)).BeginInit();
            this.pnlTop.SuspendLayout();
            this.pnlButtons.SuspendLayout();
            this.SuspendLayout();
            //
            // grdLoans
            //
            this.grdLoans.AllowUserToAddRows = false;
            this.grdLoans.AutoGenerateColumns = false;
            this.grdLoans.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colGame,
            this.colBorrower,
            this.colLoanDate,
            this.colReturnDate,
            this.colState});
            this.grdLoans.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grdLoans.Location = new System.Drawing.Point(0, 40);
            this.grdLoans.Name = "grdLoans";
            this.grdLoans.ReadOnly = true;
            this.grdLoans.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.grdLoans.Size = new System.Drawing.Size(704, 337);
            this.grdLoans.TabIndex = 1;
            //
            // colGame
            //
            this.colGame.DataPropertyName = "GameTitle";
            this.colGame.HeaderText = "Gra";
            this.colGame.Name = "colGame";
            this.colGame.ReadOnly = true;
            this.colGame.Width = 220;
            //
            // colBorrower
            //
            this.colBorrower.DataPropertyName = "Borrower";
            this.colBorrower.HeaderText = "Pożyczono komu";
            this.colBorrower.Name = "colBorrower";
            this.colBorrower.ReadOnly = true;
            this.colBorrower.Width = 150;
            //
            // colLoanDate
            //
            this.colLoanDate.DataPropertyName = "LoanDate";
            this.colLoanDate.HeaderText = "Data wypoż.";
            this.colLoanDate.Name = "colLoanDate";
            this.colLoanDate.ReadOnly = true;
            this.colLoanDate.Width = 100;
            //
            // colReturnDate
            //
            this.colReturnDate.DataPropertyName = "ReturnDate";
            this.colReturnDate.HeaderText = "Data zwrotu";
            this.colReturnDate.Name = "colReturnDate";
            this.colReturnDate.ReadOnly = true;
            this.colReturnDate.Width = 100;
            //
            // colState
            //
            this.colState.DataPropertyName = "State";
            this.colState.HeaderText = "Stan";
            this.colState.Name = "colState";
            this.colState.ReadOnly = true;
            this.colState.Width = 100;
            //
            // pnlTop
            //
            this.pnlTop.Controls.Add(this.lblGame);
            this.pnlTop.Controls.Add(this.cbxGame);
            this.pnlTop.Controls.Add(this.lblBorrower);
            this.pnlTop.Controls.Add(this.txtBorrower);
            this.pnlTop.Controls.Add(this.btnBorrow);
            this.pnlTop.Controls.Add(this.btnReturn);
            this.pnlTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTop.Location = new System.Drawing.Point(0, 0);
            this.pnlTop.Name = "pnlTop";
            this.pnlTop.Padding = new System.Windows.Forms.Padding(8);
            this.pnlTop.Size = new System.Drawing.Size(704, 40);
            this.pnlTop.TabIndex = 0;
            //
            // lblGame
            //
            this.lblGame.AutoSize = true;
            this.lblGame.Name = "lblGame";
            this.lblGame.TabIndex = 0;
            this.lblGame.Text = "Gra:";
            //
            // cbxGame
            //
            this.cbxGame.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxGame.Name = "cbxGame";
            this.cbxGame.Size = new System.Drawing.Size(260, 23);
            this.cbxGame.TabIndex = 1;
            //
            // lblBorrower
            //
            this.lblBorrower.AutoSize = true;
            this.lblBorrower.Name = "lblBorrower";
            this.lblBorrower.TabIndex = 2;
            this.lblBorrower.Text = "Komu:";
            //
            // txtBorrower
            //
            this.txtBorrower.Name = "txtBorrower";
            this.txtBorrower.Size = new System.Drawing.Size(160, 23);
            this.txtBorrower.TabIndex = 3;
            //
            // btnBorrow
            //
            this.btnBorrow.AutoSize = true;
            this.btnBorrow.Name = "btnBorrow";
            this.btnBorrow.TabIndex = 4;
            this.btnBorrow.Text = "Pożycz";
            this.btnBorrow.UseVisualStyleBackColor = true;
            //
            // btnReturn
            //
            this.btnReturn.AutoSize = true;
            this.btnReturn.Name = "btnReturn";
            this.btnReturn.TabIndex = 5;
            this.btnReturn.Text = "Zwróć zaznaczone";
            this.btnReturn.UseVisualStyleBackColor = true;
            //
            // pnlButtons
            //
            this.pnlButtons.Controls.Add(this.btnClose);
            this.pnlButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlButtons.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.pnlButtons.Location = new System.Drawing.Point(0, 377);
            this.pnlButtons.Name = "pnlButtons";
            this.pnlButtons.Size = new System.Drawing.Size(704, 44);
            this.pnlButtons.TabIndex = 2;
            //
            // btnClose
            //
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.OK;
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
            // LoanForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(704, 421);
            this.Controls.Add(this.grdLoans);
            this.Controls.Add(this.pnlTop);
            this.Controls.Add(this.pnlButtons);
            this.Name = "LoanForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Wypożyczenia";
            ((System.ComponentModel.ISupportInitialize)(this.grdLoans)).EndInit();
            this.pnlTop.ResumeLayout(false);
            this.pnlTop.PerformLayout();
            this.pnlButtons.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView grdLoans;
        private System.Windows.Forms.DataGridViewTextBoxColumn colGame;
        private System.Windows.Forms.DataGridViewTextBoxColumn colBorrower;
        private System.Windows.Forms.DataGridViewTextBoxColumn colLoanDate;
        private System.Windows.Forms.DataGridViewTextBoxColumn colReturnDate;
        private System.Windows.Forms.DataGridViewTextBoxColumn colState;
        private System.Windows.Forms.FlowLayoutPanel pnlTop;
        private System.Windows.Forms.Label lblGame;
        private System.Windows.Forms.ComboBox cbxGame;
        private System.Windows.Forms.Label lblBorrower;
        private System.Windows.Forms.TextBox txtBorrower;
        private System.Windows.Forms.Button btnBorrow;
        private System.Windows.Forms.Button btnReturn;
        private System.Windows.Forms.FlowLayoutPanel pnlButtons;
        private System.Windows.Forms.Button btnClose;
        private GameCatalog.UI.Components.Localizer localizer;
    }
}
