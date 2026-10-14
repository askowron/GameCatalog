namespace GameCatalog.UI.Frames
{
    partial class GameCustomFrame
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
            this.pnlScroll = new System.Windows.Forms.Panel();
            this.tlpFields = new System.Windows.Forms.TableLayoutPanel();
            this.localizer = new GameCatalog.UI.Components.Localizer(this.components);
            this.pnlScroll.SuspendLayout();
            this.tlpFields.SuspendLayout();
            this.SuspendLayout();
            //
            // pnlScroll
            //
            this.pnlScroll.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlScroll.AutoScroll = true;
            this.pnlScroll.BackColor = System.Drawing.SystemColors.Window;
            this.pnlScroll.Padding = new System.Windows.Forms.Padding(8);
            this.pnlScroll.Controls.Add(this.tlpFields);
            this.pnlScroll.Name = "pnlScroll";
            this.pnlScroll.TabIndex = 0;
            //
            // tlpFields
            //
            this.tlpFields.ColumnCount = 2;
            this.tlpFields.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 130F));
            this.tlpFields.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpFields.RowCount = 0;
            this.tlpFields.Dock = System.Windows.Forms.DockStyle.Top;
            this.tlpFields.AutoSize = true;
            this.tlpFields.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpFields.Name = "tlpFields";
            this.tlpFields.TabIndex = 1;
            //
            // localizer
            //
            this.localizer.ContainerControl = this;
            //
            // GameCustomFrame
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pnlScroll);
            this.Name = "GameCustomFrame";
            this.Size = new System.Drawing.Size(400, 300);
            this.tlpFields.ResumeLayout(false);
            this.tlpFields.PerformLayout();
            this.pnlScroll.ResumeLayout(false);
            this.pnlScroll.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlScroll;
        private System.Windows.Forms.TableLayoutPanel tlpFields;
        private GameCatalog.UI.Components.Localizer localizer;
    }
}
