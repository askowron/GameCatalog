namespace GameCatalog.UI.Controls
{
    partial class HtmlPreview
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
            this.webView = new Microsoft.Web.WebView2.WinForms.WebView2();
            this.pnlMissing = new System.Windows.Forms.Panel();
            this.lnkInstall = new System.Windows.Forms.LinkLabel();
            this.lblMissing = new System.Windows.Forms.Label();
            this.localizer = new GameCatalog.UI.Components.Localizer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.webView)).BeginInit();
            this.pnlMissing.SuspendLayout();
            this.SuspendLayout();
            //
            // webView
            //
            this.webView.AllowExternalDrop = true;
            this.webView.CreationProperties = null;
            this.webView.DefaultBackgroundColor = System.Drawing.Color.White;
            this.webView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webView.Location = new System.Drawing.Point(0, 0);
            this.webView.Name = "webView";
            this.webView.Size = new System.Drawing.Size(400, 300);
            this.webView.TabIndex = 0;
            this.webView.ZoomFactor = 1D;
            //
            // pnlMissing
            //
            this.pnlMissing.Controls.Add(this.lnkInstall);
            this.pnlMissing.Controls.Add(this.lblMissing);
            this.pnlMissing.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMissing.Location = new System.Drawing.Point(0, 0);
            this.pnlMissing.Name = "pnlMissing";
            this.pnlMissing.Padding = new System.Windows.Forms.Padding(12);
            this.pnlMissing.Size = new System.Drawing.Size(400, 300);
            this.pnlMissing.TabIndex = 1;
            this.pnlMissing.Visible = false;
            //
            // lnkInstall
            //
            this.lnkInstall.Dock = System.Windows.Forms.DockStyle.Top;
            this.lnkInstall.Location = new System.Drawing.Point(12, 52);
            this.lnkInstall.Name = "lnkInstall";
            this.lnkInstall.Size = new System.Drawing.Size(376, 23);
            this.lnkInstall.TabIndex = 1;
            this.lnkInstall.TabStop = true;
            this.lnkInstall.Text = "Pobierz WebView2 Runtime";
            //
            // lblMissing
            //
            this.lblMissing.AutoSize = false;
            this.lblMissing.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblMissing.Location = new System.Drawing.Point(12, 12);
            this.lblMissing.Name = "lblMissing";
            this.lblMissing.Size = new System.Drawing.Size(376, 40);
            this.lblMissing.TabIndex = 0;
            this.lblMissing.Text = "Podgląd HTML wymaga składnika Microsoft Edge WebView2 Runtime, którego nie znaleziono.";
            //
            // localizer
            //
            this.localizer.ContainerControl = this;
            //
            // HtmlPreview
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pnlMissing);
            this.Controls.Add(this.webView);
            this.Name = "HtmlPreview";
            this.Size = new System.Drawing.Size(400, 300);
            ((System.ComponentModel.ISupportInitialize)(this.webView)).EndInit();
            this.pnlMissing.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Microsoft.Web.WebView2.WinForms.WebView2 webView;
        private System.Windows.Forms.Panel pnlMissing;
        private System.Windows.Forms.LinkLabel lnkInstall;
        private System.Windows.Forms.Label lblMissing;
        private GameCatalog.UI.Components.Localizer localizer;
    }
}
