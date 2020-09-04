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
            this.pnlTextures = new System.Windows.Forms.Panel();
            this.pnlAnimation = new System.Windows.Forms.Panel();
            this.nudFrameLength = new RogueEssence.Dev.IntNumericUpDown();
            this.lbxFrames = new RogueEssence.Dev.SelectNoneListBox();
            this.tblModes = new System.Windows.Forms.TableLayoutPanel();
            this.rbDraw = new System.Windows.Forms.RadioButton();
            this.rbRectangle = new System.Windows.Forms.RadioButton();
            this.rbFill = new System.Windows.Forms.RadioButton();
            this.rbEyedrop = new System.Windows.Forms.RadioButton();
            this.lblMode = new System.Windows.Forms.Label();
            this.tilePreview = new RogueEssence.Dev.TilePreview();
            this.slbTilesets = new RogueEssence.Dev.SearchListBox();
            ((System.ComponentModel.ISupportInitialize)(this.picTileset)).BeginInit();
            this.pnlTextures.SuspendLayout();
            this.pnlAnimation.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudFrameLength)).BeginInit();
            this.tblModes.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblTileInfo
            // 
            this.lblTileInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblTileInfo.AutoSize = true;
            this.lblTileInfo.Location = new System.Drawing.Point(495, 51);
            this.lblTileInfo.Name = "lblTileInfo";
            this.lblTileInfo.Size = new System.Drawing.Size(45, 13);
            this.lblTileInfo.TabIndex = 56;
            this.lblTileInfo.Text = "Tile Info";
            // 
            // btnRemoveFrame
            // 
            this.btnRemoveFrame.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRemoveFrame.Location = new System.Drawing.Point(97, 159);
            this.btnRemoveFrame.Name = "btnRemoveFrame";
            this.btnRemoveFrame.Size = new System.Drawing.Size(60, 23);
            this.btnRemoveFrame.TabIndex = 48;
            this.btnRemoveFrame.Text = "Remove";
            this.btnRemoveFrame.UseVisualStyleBackColor = true;
            this.btnRemoveFrame.Click += new System.EventHandler(this.btnRemoveFrame_Click);
            // 
            // btnAddFrame
            // 
            this.btnAddFrame.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnAddFrame.Location = new System.Drawing.Point(3, 159);
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
            this.chkAnimationMode.Location = new System.Drawing.Point(494, 23);
            this.chkAnimationMode.Name = "chkAnimationMode";
            this.chkAnimationMode.Size = new System.Drawing.Size(77, 17);
            this.chkAnimationMode.TabIndex = 43;
            this.chkAnimationMode.Text = "Animations";
            this.chkAnimationMode.UseVisualStyleBackColor = true;
            this.chkAnimationMode.CheckedChanged += new System.EventHandler(this.chkAnimationMode_CheckedChanged);
            // 
            // picTileset
            // 
            this.picTileset.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.picTileset.Location = new System.Drawing.Point(0, 0);
            this.picTileset.Name = "picTileset";
            this.picTileset.Size = new System.Drawing.Size(474, 440);
            this.picTileset.TabIndex = 41;
            this.picTileset.TabStop = false;
            this.picTileset.Click += new System.EventHandler(this.picTileset_Click);
            // 
            // hScroll
            // 
            this.hScroll.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.hScroll.Location = new System.Drawing.Point(0, 435);
            this.hScroll.Name = "hScroll";
            this.hScroll.Size = new System.Drawing.Size(488, 17);
            this.hScroll.TabIndex = 42;
            this.hScroll.Scroll += new System.Windows.Forms.ScrollEventHandler(this.hScroll_Scroll);
            // 
            // vScroll
            // 
            this.vScroll.Dock = System.Windows.Forms.DockStyle.Right;
            this.vScroll.Location = new System.Drawing.Point(471, 0);
            this.vScroll.Name = "vScroll";
            this.vScroll.Size = new System.Drawing.Size(17, 435);
            this.vScroll.TabIndex = 40;
            this.vScroll.Scroll += new System.Windows.Forms.ScrollEventHandler(this.vScroll_Scroll);
            // 
            // lblFrames
            // 
            this.lblFrames.AutoSize = true;
            this.lblFrames.Location = new System.Drawing.Point(4, 39);
            this.lblFrames.Name = "lblFrames";
            this.lblFrames.Size = new System.Drawing.Size(41, 13);
            this.lblFrames.TabIndex = 46;
            this.lblFrames.Text = "Frames";
            // 
            // lblFrameLength
            // 
            this.lblFrameLength.AutoSize = true;
            this.lblFrameLength.Location = new System.Drawing.Point(3, 0);
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
            // pnlTextures
            // 
            this.pnlTextures.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlTextures.Controls.Add(this.picTileset);
            this.pnlTextures.Controls.Add(this.vScroll);
            this.pnlTextures.Controls.Add(this.hScroll);
            this.pnlTextures.Location = new System.Drawing.Point(2, 45);
            this.pnlTextures.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.pnlTextures.Name = "pnlTextures";
            this.pnlTextures.Size = new System.Drawing.Size(488, 452);
            this.pnlTextures.TabIndex = 57;
            // 
            // pnlAnimation
            // 
            this.pnlAnimation.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlAnimation.Controls.Add(this.lblFrameLength);
            this.pnlAnimation.Controls.Add(this.nudFrameLength);
            this.pnlAnimation.Controls.Add(this.lblFrames);
            this.pnlAnimation.Controls.Add(this.lbxFrames);
            this.pnlAnimation.Controls.Add(this.btnAddFrame);
            this.pnlAnimation.Controls.Add(this.btnRemoveFrame);
            this.pnlAnimation.Location = new System.Drawing.Point(491, 44);
            this.pnlAnimation.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.pnlAnimation.Name = "pnlAnimation";
            this.pnlAnimation.Size = new System.Drawing.Size(159, 185);
            this.pnlAnimation.TabIndex = 58;
            // 
            // nudFrameLength
            // 
            this.nudFrameLength.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.nudFrameLength.Location = new System.Drawing.Point(5, 16);
            this.nudFrameLength.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudFrameLength.Name = "nudFrameLength";
            this.nudFrameLength.Size = new System.Drawing.Size(151, 20);
            this.nudFrameLength.TabIndex = 44;
            this.nudFrameLength.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudFrameLength.TextChanged += new System.EventHandler(this.nudFrameLength_TextChanged);
            // 
            // lbxFrames
            // 
            this.lbxFrames.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbxFrames.FormattingEnabled = true;
            this.lbxFrames.Location = new System.Drawing.Point(4, 58);
            this.lbxFrames.Name = "lbxFrames";
            this.lbxFrames.SelectNone = true;
            this.lbxFrames.Size = new System.Drawing.Size(153, 95);
            this.lbxFrames.TabIndex = 49;
            this.lbxFrames.SelectedIndexChanged += new System.EventHandler(this.lbxFrames_SelectedIndexChanged);
            // 
            // tblModes
            // 
            this.tblModes.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tblModes.ColumnCount = 4;
            this.tblModes.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tblModes.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tblModes.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tblModes.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tblModes.Controls.Add(this.rbDraw, 0, 0);
            this.tblModes.Controls.Add(this.rbRectangle, 1, 0);
            this.tblModes.Controls.Add(this.rbFill, 2, 0);
            this.tblModes.Controls.Add(this.rbEyedrop, 3, 0);
            this.tblModes.Location = new System.Drawing.Point(4, 21);
            this.tblModes.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tblModes.Name = "tblModes";
            this.tblModes.RowCount = 1;
            this.tblModes.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblModes.Size = new System.Drawing.Size(472, 20);
            this.tblModes.TabIndex = 59;
            // 
            // rbDraw
            // 
            this.rbDraw.AutoSize = true;
            this.rbDraw.Checked = true;
            this.rbDraw.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rbDraw.Location = new System.Drawing.Point(2, 2);
            this.rbDraw.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.rbDraw.Name = "rbDraw";
            this.rbDraw.Size = new System.Drawing.Size(114, 16);
            this.rbDraw.TabIndex = 0;
            this.rbDraw.TabStop = true;
            this.rbDraw.Text = "Draw";
            this.rbDraw.UseVisualStyleBackColor = true;
            this.rbDraw.CheckedChanged += new System.EventHandler(this.rbDraw_CheckedChanged);
            // 
            // rbRectangle
            // 
            this.rbRectangle.AutoSize = true;
            this.rbRectangle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rbRectangle.Location = new System.Drawing.Point(120, 2);
            this.rbRectangle.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.rbRectangle.Name = "rbRectangle";
            this.rbRectangle.Size = new System.Drawing.Size(114, 16);
            this.rbRectangle.TabIndex = 1;
            this.rbRectangle.Text = "Rectangle";
            this.rbRectangle.UseVisualStyleBackColor = true;
            this.rbRectangle.CheckedChanged += new System.EventHandler(this.rbRectangle_CheckedChanged);
            // 
            // rbFill
            // 
            this.rbFill.AutoSize = true;
            this.rbFill.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rbFill.Location = new System.Drawing.Point(238, 2);
            this.rbFill.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.rbFill.Name = "rbFill";
            this.rbFill.Size = new System.Drawing.Size(114, 16);
            this.rbFill.TabIndex = 2;
            this.rbFill.Text = "Fill";
            this.rbFill.UseVisualStyleBackColor = true;
            this.rbFill.CheckedChanged += new System.EventHandler(this.rbFill_CheckedChanged);
            // 
            // rbEyedrop
            // 
            this.rbEyedrop.AutoSize = true;
            this.rbEyedrop.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rbEyedrop.Location = new System.Drawing.Point(356, 2);
            this.rbEyedrop.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.rbEyedrop.Name = "rbEyedrop";
            this.rbEyedrop.Size = new System.Drawing.Size(114, 16);
            this.rbEyedrop.TabIndex = 3;
            this.rbEyedrop.Text = "Eyedrop";
            this.rbEyedrop.UseVisualStyleBackColor = true;
            this.rbEyedrop.CheckedChanged += new System.EventHandler(this.rbEyedrop_CheckedChanged);
            // 
            // lblMode
            // 
            this.lblMode.AutoSize = true;
            this.lblMode.Location = new System.Drawing.Point(3, 6);
            this.lblMode.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblMode.Name = "lblMode";
            this.lblMode.Size = new System.Drawing.Size(37, 13);
            this.lblMode.TabIndex = 60;
            this.lblMode.Text = "Mode:";
            // 
            // tilePreview
            // 
            this.tilePreview.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.tilePreview.Location = new System.Drawing.Point(614, 6);
            this.tilePreview.Name = "tilePreview";
            this.tilePreview.Size = new System.Drawing.Size(32, 32);
            this.tilePreview.TabIndex = 39;
            // 
            // slbTilesets
            // 
            this.slbTilesets.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.slbTilesets.Location = new System.Drawing.Point(491, 235);
            this.slbTilesets.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.slbTilesets.Name = "slbTilesets";
            this.slbTilesets.SearchText = "";
            this.slbTilesets.SelectedIndex = -1;
            this.slbTilesets.SelectNone = false;
            this.slbTilesets.Size = new System.Drawing.Size(159, 262);
            this.slbTilesets.TabIndex = 55;
            this.slbTilesets.SelectedIndexChanged += new System.EventHandler(this.slbTilesets_SelectedIndexChanged);
            // 
            // TileBrowser
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lblMode);
            this.Controls.Add(this.tblModes);
            this.Controls.Add(this.pnlAnimation);
            this.Controls.Add(this.pnlTextures);
            this.Controls.Add(this.tilePreview);
            this.Controls.Add(this.lblTileInfo);
            this.Controls.Add(this.slbTilesets);
            this.Controls.Add(this.chkAnimationMode);
            this.Name = "TileBrowser";
            this.Size = new System.Drawing.Size(654, 502);
            this.Load += new System.EventHandler(this.TileBrowser_Load);
            ((System.ComponentModel.ISupportInitialize)(this.picTileset)).EndInit();
            this.pnlTextures.ResumeLayout(false);
            this.pnlAnimation.ResumeLayout(false);
            this.pnlAnimation.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudFrameLength)).EndInit();
            this.tblModes.ResumeLayout(false);
            this.tblModes.PerformLayout();
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
        private System.Windows.Forms.Panel pnlTextures;
        private System.Windows.Forms.Panel pnlAnimation;
        private System.Windows.Forms.TableLayoutPanel tblModes;
        private System.Windows.Forms.RadioButton rbDraw;
        private System.Windows.Forms.RadioButton rbRectangle;
        private System.Windows.Forms.RadioButton rbFill;
        private System.Windows.Forms.RadioButton rbEyedrop;
        private System.Windows.Forms.Label lblMode;
    }
}
