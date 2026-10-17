namespace GameCatalog.App.Forms
{
    partial class PluginsForm
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
            this.lvPlugins = new System.Windows.Forms.ListView();
            this.colPluginName = new System.Windows.Forms.ColumnHeader();
            this.colPluginVersion = new System.Windows.Forms.ColumnHeader();
            this.colPluginAuthor = new System.Windows.Forms.ColumnHeader();
            this.colPluginState = new System.Windows.Forms.ColumnHeader();
            this.colPluginError = new System.Windows.Forms.ColumnHeader();
            this.lblRestart = new System.Windows.Forms.Label();
            this.pnlButtons = new System.Windows.Forms.FlowLayoutPanel();
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.localizer = new GameCatalog.UI.Components.Localizer(this.components);
            this.pnlButtons.SuspendLayout();
            this.SuspendLayout();
            //
            // lvPlugins
            //
            this.lvPlugins.CheckBoxes = true;
            this.lvPlugins.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colPluginName,
            this.colPluginVersion,
            this.colPluginAuthor,
            this.colPluginState,
            this.colPluginError});
            this.lvPlugins.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvPlugins.FullRowSelect = true;
            this.lvPlugins.Location = new System.Drawing.Point(0, 0);
            this.lvPlugins.Name = "lvPlugins";
            this.lvPlugins.Size = new System.Drawing.Size(624, 293);
            this.lvPlugins.TabIndex = 0;
            this.lvPlugins.UseCompatibleStateImageBehavior = false;
            this.lvPlugins.View = System.Windows.Forms.View.Details;
            //
            // colPluginName
            //
            this.colPluginName.Name = "colPluginName";
            this.colPluginName.Text = "Nazwa";
            this.colPluginName.Width = 180;
            //
            // colPluginVersion
            //
            this.colPluginVersion.Name = "colPluginVersion";
            this.colPluginVersion.Text = "Wersja";
            this.colPluginVersion.Width = 70;
            //
            // colPluginAuthor
            //
            this.colPluginAuthor.Name = "colPluginAuthor";
            this.colPluginAuthor.Text = "Autor";
            this.colPluginAuthor.Width = 110;
            //
            // colPluginState
            //
            this.colPluginState.Name = "colPluginState";
            this.colPluginState.Text = "Stan";
            this.colPluginState.Width = 110;
            //
            // colPluginError
            //
            this.colPluginError.Name = "colPluginError";
            this.colPluginError.Text = "Błąd";
            this.colPluginError.Width = 240;
            //
            // lblRestart
            //
            this.lblRestart.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblRestart.Location = new System.Drawing.Point(0, 293);
            this.lblRestart.Name = "lblRestart";
            this.lblRestart.Padding = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.lblRestart.Size = new System.Drawing.Size(624, 24);
            this.lblRestart.TabIndex = 1;
            this.lblRestart.Text = "Zmiany zostaną zastosowane po ponownym uruchomieniu programu.";
            this.lblRestart.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // pnlButtons
            //
            this.pnlButtons.Controls.Add(this.btnOk);
            this.pnlButtons.Controls.Add(this.btnCancel);
            this.pnlButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlButtons.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.pnlButtons.Location = new System.Drawing.Point(0, 317);
            this.pnlButtons.Name = "pnlButtons";
            this.pnlButtons.Padding = new System.Windows.Forms.Padding(8);
            this.pnlButtons.Size = new System.Drawing.Size(624, 44);
            this.pnlButtons.TabIndex = 2;
            //
            // btnOk
            //
            this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOk.Location = new System.Drawing.Point(525, 11);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(80, 23);
            this.btnOk.TabIndex = 0;
            this.btnOk.Text = "OK";
            this.btnOk.UseVisualStyleBackColor = true;
            //
            // btnCancel
            //
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(439, 11);
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
            // PluginsForm
            //
            this.AcceptButton = this.btnOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(624, 361);
            this.Controls.Add(this.lvPlugins);
            this.Controls.Add(this.lblRestart);
            this.Controls.Add(this.pnlButtons);
            this.MinimizeBox = false;
            this.Name = "PluginsForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Pluginy";
            this.pnlButtons.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView lvPlugins;
        private System.Windows.Forms.ColumnHeader colPluginName;
        private System.Windows.Forms.ColumnHeader colPluginVersion;
        private System.Windows.Forms.ColumnHeader colPluginAuthor;
        private System.Windows.Forms.ColumnHeader colPluginState;
        private System.Windows.Forms.ColumnHeader colPluginError;
        private System.Windows.Forms.Label lblRestart;
        private System.Windows.Forms.FlowLayoutPanel pnlButtons;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
        private GameCatalog.UI.Components.Localizer localizer;
    }
}
