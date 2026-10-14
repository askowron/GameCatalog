namespace GameCatalog.UI.Controls
{
    partial class PictureToolWindow
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
            this.tsPicture = new System.Windows.Forms.ToolStrip();
            this.btnPicLoad = new System.Windows.Forms.ToolStripButton();
            this.btnPicSaveAs = new System.Windows.Forms.ToolStripButton();
            this.btnPicCopy = new System.Windows.Forms.ToolStripButton();
            this.btnPicPaste = new System.Windows.Forms.ToolStripButton();
            this.btnPicDelete = new System.Windows.Forms.ToolStripButton();
            this.picCover = new System.Windows.Forms.PictureBox();
            this.localizer = new GameCatalog.UI.Components.Localizer(this.components);
            this.tsPicture.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picCover)).BeginInit();
            this.SuspendLayout();
            //
            // tsPicture
            //
            this.tsPicture.Dock = System.Windows.Forms.DockStyle.Top;
            this.tsPicture.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.tsPicture.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnPicLoad,
            this.btnPicSaveAs,
            this.btnPicCopy,
            this.btnPicPaste,
            this.btnPicDelete});
            this.tsPicture.Location = new System.Drawing.Point(0, 0);
            this.tsPicture.Name = "tsPicture";
            this.tsPicture.Size = new System.Drawing.Size(200, 25);
            this.tsPicture.TabIndex = 0;
            //
            // btnPicLoad
            //
            this.btnPicLoad.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnPicLoad.Name = "btnPicLoad";
            this.btnPicLoad.Size = new System.Drawing.Size(23, 22);
            //
            // btnPicSaveAs
            //
            this.btnPicSaveAs.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnPicSaveAs.Name = "btnPicSaveAs";
            this.btnPicSaveAs.Size = new System.Drawing.Size(23, 22);
            //
            // btnPicCopy
            //
            this.btnPicCopy.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnPicCopy.Name = "btnPicCopy";
            this.btnPicCopy.Size = new System.Drawing.Size(23, 22);
            //
            // btnPicPaste
            //
            this.btnPicPaste.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnPicPaste.Name = "btnPicPaste";
            this.btnPicPaste.Size = new System.Drawing.Size(23, 22);
            //
            // btnPicDelete
            //
            this.btnPicDelete.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnPicDelete.Name = "btnPicDelete";
            this.btnPicDelete.Size = new System.Drawing.Size(23, 22);
            //
            // picCover
            //
            this.picCover.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picCover.Dock = System.Windows.Forms.DockStyle.Fill;
            this.picCover.Location = new System.Drawing.Point(0, 25);
            this.picCover.Name = "picCover";
            this.picCover.Size = new System.Drawing.Size(200, 175);
            this.picCover.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picCover.TabIndex = 1;
            this.picCover.TabStop = false;
            //
            // localizer
            //
            this.localizer.ContainerControl = this;
            //
            // PictureToolWindow
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.picCover);
            this.Controls.Add(this.tsPicture);
            this.Name = "PictureToolWindow";
            this.Size = new System.Drawing.Size(200, 200);
            this.tsPicture.ResumeLayout(false);
            this.tsPicture.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picCover)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip tsPicture;
        private System.Windows.Forms.ToolStripButton btnPicLoad;
        private System.Windows.Forms.ToolStripButton btnPicSaveAs;
        private System.Windows.Forms.ToolStripButton btnPicCopy;
        private System.Windows.Forms.ToolStripButton btnPicPaste;
        private System.Windows.Forms.ToolStripButton btnPicDelete;
        private System.Windows.Forms.PictureBox picCover;
        private GameCatalog.UI.Components.Localizer localizer;
    }
}
