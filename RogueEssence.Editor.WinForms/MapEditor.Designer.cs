namespace RogueEssence.Dev
{
    partial class MapEditor
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MapEditor));
            this.mnuMenu = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resizeMapToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.undoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.redoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.chkTexEyeDropper = new System.Windows.Forms.CheckBox();
            this.openMapFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.saveMapFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.tabMapOptions = new System.Windows.Forms.TabControl();
            this.tabTextures = new System.Windows.Forms.TabPage();
            this.chkFill = new System.Windows.Forms.CheckBox();
            this.tileBrowser = new RogueEssence.Dev.TileBrowser();
            this.tabProperties = new System.Windows.Forms.TabPage();
            this.cbCharSight = new System.Windows.Forms.ComboBox();
            this.lblTime = new System.Windows.Forms.Label();
            this.cbTileSight = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtMapName = new System.Windows.Forms.TextBox();
            this.btnReloadSongs = new System.Windows.Forms.Button();
            this.lblMapName = new System.Windows.Forms.Label();
            this.nudTimeLimit = new RogueEssence.Dev.IntNumericUpDown();
            this.lbxMusic = new System.Windows.Forms.ListBox();
            this.lblMusic = new System.Windows.Forms.Label();
            this.lblSight = new System.Windows.Forms.Label();
            this.mnuMenu.SuspendLayout();
            this.tabMapOptions.SuspendLayout();
            this.tabTextures.SuspendLayout();
            this.tabProperties.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudTimeLimit)).BeginInit();
            this.SuspendLayout();
            // 
            // mnuMenu
            // 
            this.mnuMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.editToolStripMenuItem});
            this.mnuMenu.Location = new System.Drawing.Point(0, 0);
            this.mnuMenu.Name = "mnuMenu";
            this.mnuMenu.Size = new System.Drawing.Size(685, 24);
            this.mnuMenu.TabIndex = 0;
            this.mnuMenu.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newToolStripMenuItem,
            this.openToolStripMenuItem,
            this.saveToolStripMenuItem,
            this.saveAsToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // newToolStripMenuItem
            // 
            this.newToolStripMenuItem.Name = "newToolStripMenuItem";
            this.newToolStripMenuItem.Size = new System.Drawing.Size(114, 22);
            this.newToolStripMenuItem.Text = "New";
            this.newToolStripMenuItem.Click += new System.EventHandler(this.newToolStripMenuItem_Click);
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(114, 22);
            this.openToolStripMenuItem.Text = "Open";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(114, 22);
            this.saveToolStripMenuItem.Text = "Save";
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
            // 
            // saveAsToolStripMenuItem
            // 
            this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(114, 22);
            this.saveAsToolStripMenuItem.Text = "Save As";
            this.saveAsToolStripMenuItem.Click += new System.EventHandler(this.saveAsToolStripMenuItem_Click);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.resizeMapToolStripMenuItem,
            this.undoToolStripMenuItem,
            this.redoToolStripMenuItem});
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(39, 20);
            this.editToolStripMenuItem.Text = "Edit";
            // 
            // resizeMapToolStripMenuItem
            // 
            this.resizeMapToolStripMenuItem.Name = "resizeMapToolStripMenuItem";
            this.resizeMapToolStripMenuItem.Size = new System.Drawing.Size(133, 22);
            this.resizeMapToolStripMenuItem.Text = "Resize Map";
            this.resizeMapToolStripMenuItem.Click += new System.EventHandler(this.resizeMapToolStripMenuItem_Click);
            // 
            // undoToolStripMenuItem
            // 
            this.undoToolStripMenuItem.Enabled = false;
            this.undoToolStripMenuItem.Name = "undoToolStripMenuItem";
            this.undoToolStripMenuItem.Size = new System.Drawing.Size(133, 22);
            this.undoToolStripMenuItem.Text = "Undo";
            // 
            // redoToolStripMenuItem
            // 
            this.redoToolStripMenuItem.Enabled = false;
            this.redoToolStripMenuItem.Name = "redoToolStripMenuItem";
            this.redoToolStripMenuItem.Size = new System.Drawing.Size(133, 22);
            this.redoToolStripMenuItem.Text = "Redo";
            // 
            // chkTexEyeDropper
            // 
            this.chkTexEyeDropper.AutoSize = true;
            this.chkTexEyeDropper.Location = new System.Drawing.Point(62, 9);
            this.chkTexEyeDropper.Name = "chkTexEyeDropper";
            this.chkTexEyeDropper.Size = new System.Drawing.Size(85, 17);
            this.chkTexEyeDropper.TabIndex = 4;
            this.chkTexEyeDropper.Text = "Eye Dropper";
            this.chkTexEyeDropper.UseVisualStyleBackColor = true;
            this.chkTexEyeDropper.CheckedChanged += new System.EventHandler(this.chkTexEyeDropper_CheckedChanged);
            // 
            // openMapFileDialog
            // 
            this.openMapFileDialog.Filter = "map files (*.rsmap)|*.rsmap";
            this.openMapFileDialog.RestoreDirectory = true;
            // 
            // saveMapFileDialog
            // 
            this.saveMapFileDialog.Filter = "map files (*.rsmap)|*.rsmap";
            this.saveMapFileDialog.RestoreDirectory = true;
            // 
            // tabMapOptions
            // 
            this.tabMapOptions.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabMapOptions.Controls.Add(this.tabTextures);
            this.tabMapOptions.Controls.Add(this.tabProperties);
            this.tabMapOptions.Location = new System.Drawing.Point(12, 27);
            this.tabMapOptions.Name = "tabMapOptions";
            this.tabMapOptions.SelectedIndex = 0;
            this.tabMapOptions.Size = new System.Drawing.Size(661, 528);
            this.tabMapOptions.TabIndex = 16;
            // 
            // tabTextures
            // 
            this.tabTextures.Controls.Add(this.chkFill);
            this.tabTextures.Controls.Add(this.chkTexEyeDropper);
            this.tabTextures.Controls.Add(this.tileBrowser);
            this.tabTextures.Location = new System.Drawing.Point(4, 22);
            this.tabTextures.Name = "tabTextures";
            this.tabTextures.Padding = new System.Windows.Forms.Padding(3);
            this.tabTextures.Size = new System.Drawing.Size(653, 502);
            this.tabTextures.TabIndex = 0;
            this.tabTextures.Text = "Textures";
            this.tabTextures.UseVisualStyleBackColor = true;
            // 
            // chkFill
            // 
            this.chkFill.AutoSize = true;
            this.chkFill.Location = new System.Drawing.Point(18, 9);
            this.chkFill.Name = "chkFill";
            this.chkFill.Size = new System.Drawing.Size(38, 17);
            this.chkFill.TabIndex = 19;
            this.chkFill.Text = "Fill";
            this.chkFill.UseVisualStyleBackColor = true;
            this.chkFill.CheckedChanged += new System.EventHandler(this.chkFill_CheckedChanged);
            // 
            // tileBrowser
            // 
            this.tileBrowser.Location = new System.Drawing.Point(0, 0);
            this.tileBrowser.Name = "tileBrowser";
            this.tileBrowser.Size = new System.Drawing.Size(654, 502);
            this.tileBrowser.TabIndex = 20;
            // 
            // tabProperties
            // 
            this.tabProperties.Controls.Add(this.cbCharSight);
            this.tabProperties.Controls.Add(this.lblTime);
            this.tabProperties.Controls.Add(this.cbTileSight);
            this.tabProperties.Controls.Add(this.label1);
            this.tabProperties.Controls.Add(this.txtMapName);
            this.tabProperties.Controls.Add(this.btnReloadSongs);
            this.tabProperties.Controls.Add(this.lblMapName);
            this.tabProperties.Controls.Add(this.nudTimeLimit);
            this.tabProperties.Controls.Add(this.lbxMusic);
            this.tabProperties.Controls.Add(this.lblMusic);
            this.tabProperties.Controls.Add(this.lblSight);
            this.tabProperties.Location = new System.Drawing.Point(4, 22);
            this.tabProperties.Name = "tabProperties";
            this.tabProperties.Padding = new System.Windows.Forms.Padding(3);
            this.tabProperties.Size = new System.Drawing.Size(653, 502);
            this.tabProperties.TabIndex = 1;
            this.tabProperties.Text = "Properties";
            this.tabProperties.UseVisualStyleBackColor = true;
            // 
            // cbCharSight
            // 
            this.cbCharSight.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbCharSight.FormattingEnabled = true;
            this.cbCharSight.Location = new System.Drawing.Point(9, 160);
            this.cbCharSight.Name = "cbCharSight";
            this.cbCharSight.Size = new System.Drawing.Size(324, 21);
            this.cbCharSight.TabIndex = 13;
            this.cbCharSight.SelectedIndexChanged += new System.EventHandler(this.cbCharSight_SelectedIndexChanged);
            // 
            // lblTime
            // 
            this.lblTime.AutoSize = true;
            this.lblTime.Location = new System.Drawing.Point(6, 56);
            this.lblTime.Name = "lblTime";
            this.lblTime.Size = new System.Drawing.Size(54, 13);
            this.lblTime.TabIndex = 12;
            this.lblTime.Text = "Time Limit";
            // 
            // cbTileSight
            // 
            this.cbTileSight.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbTileSight.FormattingEnabled = true;
            this.cbTileSight.Location = new System.Drawing.Point(9, 116);
            this.cbTileSight.Name = "cbTileSight";
            this.cbTileSight.Size = new System.Drawing.Size(324, 21);
            this.cbTileSight.TabIndex = 8;
            this.cbTileSight.SelectedIndexChanged += new System.EventHandler(this.cbTileSight_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 100);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(48, 13);
            this.label1.TabIndex = 7;
            this.label1.Text = "TileSight";
            // 
            // txtMapName
            // 
            this.txtMapName.Location = new System.Drawing.Point(9, 30);
            this.txtMapName.Name = "txtMapName";
            this.txtMapName.Size = new System.Drawing.Size(324, 20);
            this.txtMapName.TabIndex = 6;
            this.txtMapName.TextChanged += new System.EventHandler(this.txtMapName_TextChanged);
            // 
            // btnReloadSongs
            // 
            this.btnReloadSongs.Location = new System.Drawing.Point(361, 285);
            this.btnReloadSongs.Name = "btnReloadSongs";
            this.btnReloadSongs.Size = new System.Drawing.Size(237, 23);
            this.btnReloadSongs.TabIndex = 5;
            this.btnReloadSongs.Text = "Reload Music Folder";
            this.btnReloadSongs.UseVisualStyleBackColor = true;
            this.btnReloadSongs.Click += new System.EventHandler(this.btnReloadSongs_Click);
            // 
            // lblMapName
            // 
            this.lblMapName.AutoSize = true;
            this.lblMapName.Location = new System.Drawing.Point(6, 12);
            this.lblMapName.Name = "lblMapName";
            this.lblMapName.Size = new System.Drawing.Size(59, 13);
            this.lblMapName.TabIndex = 1;
            this.lblMapName.Text = "Map Name";
            // 
            // nudTimeLimit
            // 
            this.nudTimeLimit.Location = new System.Drawing.Point(9, 74);
            this.nudTimeLimit.Maximum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudTimeLimit.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
            this.nudTimeLimit.Name = "nudTimeLimit";
            this.nudTimeLimit.Size = new System.Drawing.Size(324, 20);
            this.nudTimeLimit.TabIndex = 11;
            // 
            // lbxMusic
            // 
            this.lbxMusic.FormattingEnabled = true;
            this.lbxMusic.Location = new System.Drawing.Point(361, 28);
            this.lbxMusic.Name = "lbxMusic";
            this.lbxMusic.Size = new System.Drawing.Size(237, 251);
            this.lbxMusic.TabIndex = 0;
            this.lbxMusic.SelectedIndexChanged += new System.EventHandler(this.lbxMusic_SelectedIndexChanged);
            // 
            // lblMusic
            // 
            this.lblMusic.AutoSize = true;
            this.lblMusic.Location = new System.Drawing.Point(358, 12);
            this.lblMusic.Name = "lblMusic";
            this.lblMusic.Size = new System.Drawing.Size(35, 13);
            this.lblMusic.TabIndex = 2;
            this.lblMusic.Text = "Music";
            // 
            // lblSight
            // 
            this.lblSight.AutoSize = true;
            this.lblSight.Location = new System.Drawing.Point(6, 144);
            this.lblSight.Name = "lblSight";
            this.lblSight.Size = new System.Drawing.Size(53, 13);
            this.lblSight.TabIndex = 9;
            this.lblSight.Text = "CharSight";
            // 
            // MapEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(685, 565);
            this.Controls.Add(this.tabMapOptions);
            this.Controls.Add(this.mnuMenu);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.mnuMenu;
            this.Name = "MapEditor";
            this.Text = "Map Editor";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MapEditor_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MapEditor_FormClosed);
            this.Load += new System.EventHandler(this.MapEditor_Load);
            this.mnuMenu.ResumeLayout(false);
            this.mnuMenu.PerformLayout();
            this.tabMapOptions.ResumeLayout(false);
            this.tabTextures.ResumeLayout(false);
            this.tabTextures.PerformLayout();
            this.tabProperties.ResumeLayout(false);
            this.tabProperties.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudTimeLimit)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip mnuMenu;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem undoToolStripMenuItem;
        private System.Windows.Forms.CheckBox chkTexEyeDropper;
        private System.Windows.Forms.OpenFileDialog openMapFileDialog;
        private System.Windows.Forms.SaveFileDialog saveMapFileDialog;
        private System.Windows.Forms.TabControl tabMapOptions;
        private System.Windows.Forms.TabPage tabTextures;
        private System.Windows.Forms.TabPage tabProperties;
        private System.Windows.Forms.Button btnReloadSongs;
        private System.Windows.Forms.Label lblMusic;
        private System.Windows.Forms.Label lblMapName;
        private System.Windows.Forms.ListBox lbxMusic;
        private System.Windows.Forms.ComboBox cbTileSight;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtMapName;
        private System.Windows.Forms.Label lblTime;
        private IntNumericUpDown nudTimeLimit;
        private System.Windows.Forms.Label lblSight;
        private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem resizeMapToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem redoToolStripMenuItem;
        private System.Windows.Forms.CheckBox chkFill;
        private TileBrowser tileBrowser;
        private System.Windows.Forms.ComboBox cbCharSight;
    }
}