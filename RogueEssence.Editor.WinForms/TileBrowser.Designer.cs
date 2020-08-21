namespace RogueEssence.Dev
{
    partial class TileBrowser
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
            this.lblTileInfo = new System.Windows.Forms.Label();
            this.btnRemoveFrame = new System.Windows.Forms.Button();
            this.btnAddFrame = new System.Windows.Forms.Button();
            this.chkAnimationMode = new System.Windows.Forms.CheckBox();
            this.picTileset = new System.Windows.Forms.PictureBox();
            this.hScroll = new System.Windows.Forms.HScrollBar();
            this.vScroll = new System.Windows.Forms.VScrollBar();
            this.lblFrames = new System.Windows.Forms.Label();
            this.lblFrameLength = new System.Windows.Forms.Label();
            this.openTileFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.saveTileFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.tilePreview = new RogueEssence.Dev.TilePreview();
            this.slbTilesets = new RogueEssence.Dev.SearchListBox();
            this.lbxFrames = new RogueEssence.Dev.SelectNoneListBox();
            this.nudFrameLength = new RogueEssence.Dev.IntNumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.picTileset)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudFrameLength)).BeginInit();
            this.SuspendLayout();
            // 
            // lblTileInfo
            // 
            this.lblTileInfo.AutoSize = true;
            this.lblTileInfo.Location = new System.Drawing.Point(513, 51);
            this.lblTileInfo.Name = "lblTileInfo";
            this.lblTileInfo.Size = new System.Drawing.Size(45, 13);
            this.lblTileInfo.TabIndex = 56;
            this.lblTileInfo.Text = "Tile Info";
            // 
            // btnRemoveFrame
            // 
            this.btnRemoveFrame.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRemoveFrame.Location = new System.Drawing.Point(586, 193);
            this.btnRemoveFrame.Name = "btnRemoveFrame";
            this.btnRemoveFrame.Size = new System.Drawing.Size(60, 23);
            this.btnRemoveFrame.TabIndex = 48;
            this.btnRemoveFrame.Text = "Remove";
            this.btnRemoveFrame.UseVisualStyleBackColor = true;
            this.btnRemoveFrame.Click += new System.EventHandler(this.btnRemoveFrame_Click);
            // 
            // btnAddFrame
            // 
            this.btnAddFrame.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAddFrame.Location = new System.Drawing.Point(516, 193);
            this.btnAddFrame.Name = "btnAddFrame";
            this.btnAddFrame.Size = new System.Drawing.Size(64, 23);
            this.btnAddFrame.TabIndex = 47;
            this.btnAddFrame.Text = "Add";
            this.btnAddFrame.UseVisualStyleBackColor = true;
            this.btnAddFrame.Click += new System.EventHandler(this.btnAddFrame_Click);
            // 
            // chkAnimationMode
            // 
            this.chkAnimationMode.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chkAnimationMode.AutoSize = true;
            this.chkAnimationMode.Location = new System.Drawing.Point(516, 9);
            this.chkAnimationMode.Name = "chkAnimationMode";
            this.chkAnimationMode.Size = new System.Drawing.Size(77, 17);
            this.chkAnimationMode.TabIndex = 43;
            this.chkAnimationMode.Text = "Animations";
            this.chkAnimationMode.UseVisualStyleBackColor = true;
            this.chkAnimationMode.CheckedChanged += new System.EventHandler(this.chkAnimationMode_CheckedChanged);
            // 
            // picTileset
            // 
            this.picTileset.Location = new System.Drawing.Point(9, 51);
            this.picTileset.Name = "picTileset";
            this.picTileset.Size = new System.Drawing.Size(480, 416);
            this.picTileset.TabIndex = 41;
            this.picTileset.TabStop = false;
            this.picTileset.Click += new System.EventHandler(this.picTileset_Click);
            // 
            // hScroll
            // 
            this.hScroll.Location = new System.Drawing.Point(9, 467);
            this.hScroll.Name = "hScroll";
            this.hScroll.Size = new System.Drawing.Size(480, 17);
            this.hScroll.TabIndex = 42;
            this.hScroll.Scroll += new System.Windows.Forms.ScrollEventHandler(this.hScroll_Scroll);
            // 
            // vScroll
            // 
            this.vScroll.Location = new System.Drawing.Point(488, 51);
            this.vScroll.Name = "vScroll";
            this.vScroll.Size = new System.Drawing.Size(17, 416);
            this.vScroll.TabIndex = 40;
            this.vScroll.Scroll += new System.Windows.Forms.ScrollEventHandler(this.vScroll_Scroll);
            // 
            // lblFrames
            // 
            this.lblFrames.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblFrames.AutoSize = true;
            this.lblFrames.Location = new System.Drawing.Point(513, 76);
            this.lblFrames.Name = "lblFrames";
            this.lblFrames.Size = new System.Drawing.Size(41, 13);
            this.lblFrames.TabIndex = 46;
            this.lblFrames.Text = "Frames";
            // 
            // lblFrameLength
            // 
            this.lblFrameLength.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblFrameLength.AutoSize = true;
            this.lblFrameLength.Location = new System.Drawing.Point(513, 35);
            this.lblFrameLength.Name = "lblFrameLength";
            this.lblFrameLength.Size = new System.Drawing.Size(72, 13);
            this.lblFrameLength.TabIndex = 45;
            this.lblFrameLength.Text = "Frame Length";
            // 
            // openTileFileDialog
            // 
            this.openTileFileDialog.Filter = "PNG files (*.png)|*.png";
            // 
            // saveTileFileDialog
            // 
            this.saveTileFileDialog.Filter = "PNG files (*.png)|*.png";
            // 
            // tilePreview
            // 
            this.tilePreview.Location = new System.Drawing.Point(614, 6);
            this.tilePreview.Name = "tilePreview";
            this.tilePreview.Size = new System.Drawing.Size(32, 32);
            this.tilePreview.TabIndex = 39;
            // 
            // slbTilesets
            // 
            this.slbTilesets.Location = new System.Drawing.Point(516, 235);
            this.slbTilesets.Name = "slbTilesets";
            this.slbTilesets.SearchText = "";
            this.slbTilesets.SelectedIndex = -1;
            this.slbTilesets.SelectNone = false;
            this.slbTilesets.Size = new System.Drawing.Size(130, 264);
            this.slbTilesets.TabIndex = 55;
            this.slbTilesets.SelectedIndexChanged += new System.EventHandler(this.slbTilesets_SelectedIndexChanged);
            // 
            // lbxFrames
            // 
            this.lbxFrames.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lbxFrames.FormattingEnabled = true;
            this.lbxFrames.Location = new System.Drawing.Point(516, 92);
            this.lbxFrames.Name = "lbxFrames";
            this.lbxFrames.SelectNone = true;
            this.lbxFrames.Size = new System.Drawing.Size(130, 95);
            this.lbxFrames.TabIndex = 49;
            this.lbxFrames.SelectedIndexChanged += new System.EventHandler(this.lbxFrames_SelectedIndexChanged);
            // 
            // nudFrameLength
            // 
            this.nudFrameLength.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.nudFrameLength.Location = new System.Drawing.Point(516, 51);
            this.nudFrameLength.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudFrameLength.Name = "nudFrameLength";
            this.nudFrameLength.Size = new System.Drawing.Size(130, 20);
            this.nudFrameLength.TabIndex = 44;
            this.nudFrameLength.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudFrameLength.TextChanged += new System.EventHandler(this.nudFrameLength_TextChanged);
            // 
            // TileBrowser
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tilePreview);
            this.Controls.Add(this.lblTileInfo);
            this.Controls.Add(this.slbTilesets);
            this.Controls.Add(this.lbxFrames);
            this.Controls.Add(this.btnRemoveFrame);
            this.Controls.Add(this.btnAddFrame);
            this.Controls.Add(this.nudFrameLength);
            this.Controls.Add(this.chkAnimationMode);
            this.Controls.Add(this.picTileset);
            this.Controls.Add(this.hScroll);
            this.Controls.Add(this.vScroll);
            this.Controls.Add(this.lblFrames);
            this.Controls.Add(this.lblFrameLength);
            this.Name = "TileBrowser";
            this.Size = new System.Drawing.Size(654, 502);
            this.Load += new System.EventHandler(this.TileBrowser_Load);
            ((System.ComponentModel.ISupportInitialize)(this.picTileset)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudFrameLength)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private TilePreview tilePreview;
        private System.Windows.Forms.Label lblTileInfo;
        private SearchListBox slbTilesets;
        private SelectNoneListBox lbxFrames;
        private System.Windows.Forms.Button btnRemoveFrame;
        private System.Windows.Forms.Button btnAddFrame;
        private IntNumericUpDown nudFrameLength;
        private System.Windows.Forms.CheckBox chkAnimationMode;
        private System.Windows.Forms.PictureBox picTileset;
        private System.Windows.Forms.HScrollBar hScroll;
        private System.Windows.Forms.VScrollBar vScroll;
        private System.Windows.Forms.Label lblFrames;
        private System.Windows.Forms.Label lblFrameLength;
        private System.Windows.Forms.OpenFileDialog openTileFileDialog;
        private System.Windows.Forms.SaveFileDialog saveTileFileDialog;
    }
}
