namespace RogueEssence.Dev
{
    partial class SpriteBrowser
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.vsItemScroll = new System.Windows.Forms.VScrollBar();
            this.picSprite = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.picSprite)).BeginInit();
            this.SuspendLayout();
            // 
            // vsItemScroll
            // 
            this.vsItemScroll.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.vsItemScroll.LargeChange = 1;
            this.vsItemScroll.Location = new System.Drawing.Point(193, 3);
            this.vsItemScroll.Name = "vsItemScroll";
            this.vsItemScroll.Size = new System.Drawing.Size(17, 253);
            this.vsItemScroll.TabIndex = 5;
            this.vsItemScroll.ValueChanged += new System.EventHandler(this.vsItemScroll_ValueChanged);
            // 
            // picSprite
            // 
            this.picSprite.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.picSprite.Location = new System.Drawing.Point(0, 0);
            this.picSprite.Name = "picSprite";
            this.picSprite.Size = new System.Drawing.Size(192, 256);
            this.picSprite.TabIndex = 4;
            this.picSprite.TabStop = false;
            this.picSprite.Click += new System.EventHandler(this.picSprite_Click);
            // 
            // ItemBrowser
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.vsItemScroll);
            this.Controls.Add(this.picSprite);
            this.Name = "ItemBrowser";
            this.Size = new System.Drawing.Size(210, 256);
            this.Load += new System.EventHandler(this.ItemBrowser_Load);
            ((System.ComponentModel.ISupportInitialize)(this.picSprite)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.VScrollBar vsItemScroll;
        private System.Windows.Forms.PictureBox picSprite;
    }
}
