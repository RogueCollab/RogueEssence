namespace RogueEssence.Dev
{
    partial class GroundEditor
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
            this.mnuMenu = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importFromTilesetToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importFromPngToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resizeMapToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.retileMapToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.undoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.redoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.saveMapFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.tabMapOptions = new System.Windows.Forms.TabControl();
            this.tabTextures = new System.Windows.Forms.TabPage();
            this.tileBrowser = new RogueEssence.Dev.TileBrowser();
            this.tabBlock = new System.Windows.Forms.TabPage();
            this.tblBlockModes = new System.Windows.Forms.TableLayoutPanel();
            this.rbBlockDraw = new System.Windows.Forms.RadioButton();
            this.rbBlockRectangle = new System.Windows.Forms.RadioButton();
            this.rbBlockFill = new System.Windows.Forms.RadioButton();
            this.lblBlockMode = new System.Windows.Forms.Label();
            this.tabEntities = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel8 = new System.Windows.Forms.TableLayoutPanel();
            this.rbEntSelect = new System.Windows.Forms.RadioButton();
            this.rbEntPlace = new System.Windows.Forms.RadioButton();
            this.rbEntMove = new System.Windows.Forms.RadioButton();
            this.label22 = new System.Windows.Forms.Label();
            this.tableLayoutPanel7 = new System.Windows.Forms.TableLayoutPanel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.cmbEntityDir = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtEntityName = new System.Windows.Forms.TextBox();
            this.cmbEntityType = new System.Windows.Forms.ComboBox();
            this.tabctrlEntData = new System.Windows.Forms.TabControl();
            this.tabEntObjDisplay = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
            this.label6 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.listView1 = new System.Windows.Forms.ListView();
            this.numEntObjStartFrame = new System.Windows.Forms.NumericUpDown();
            this.label17 = new System.Windows.Forms.Label();
            this.numEntObjEndFrame = new System.Windows.Forms.NumericUpDown();
            this.label18 = new System.Windows.Forms.Label();
            this.numEntObjFrameTime = new System.Windows.Forms.NumericUpDown();
            this.numEntObjAlpha = new System.Windows.Forms.NumericUpDown();
            this.label19 = new System.Windows.Forms.Label();
            this.cbEntObjSpriteID = new System.Windows.Forms.ComboBox();
            this.tabEntCharDisplay = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.picEntPreview = new System.Windows.Forms.PictureBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.cmbEntKind = new System.Windows.Forms.ComboBox();
            this.label9 = new System.Windows.Forms.Label();
            this.chkEntRare = new System.Windows.Forms.CheckBox();
            this.label10 = new System.Windows.Forms.Label();
            this.cmbEntForm = new System.Windows.Forms.ComboBox();
            this.tabEntScript = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.label11 = new System.Windows.Forms.Label();
            this.cmbEntTriggerType = new System.Windows.Forms.ComboBox();
            this.label12 = new System.Windows.Forms.Label();
            this.chklstEntScriptCallbacks = new System.Windows.Forms.CheckedListBox();
            this.lblScriptSecondaryCallbacks = new System.Windows.Forms.Label();
            this.chklstScriptSecondaryCallbacks = new System.Windows.Forms.CheckedListBox();
            this.tabEntCharData = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.label4 = new System.Windows.Forms.Label();
            this.txtEntCharNickname = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.numEntCharLevel = new System.Windows.Forms.NumericUpDown();
            this.label14 = new System.Windows.Forms.Label();
            this.cmbEntCharGender = new System.Windows.Forms.ComboBox();
            this.tabEntSpawner = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel6 = new System.Windows.Forms.TableLayoutPanel();
            this.label20 = new System.Windows.Forms.Label();
            this.txtSpawnedEntName = new System.Windows.Forms.TextBox();
            this.label21 = new System.Windows.Forms.Label();
            this.cmbSpawnerType = new System.Windows.Forms.ComboBox();
            this.label15 = new System.Windows.Forms.Label();
            this.numEntWidth = new System.Windows.Forms.NumericUpDown();
            this.numEntHeight = new System.Windows.Forms.NumericUpDown();
            this.btnAddToTemplates = new System.Windows.Forms.Button();
            this.chkEntEnabled = new System.Windows.Forms.CheckBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel9 = new System.Windows.Forms.TableLayoutPanel();
            this.btnLoadFromTemplate = new System.Windows.Forms.Button();
            this.btnRemTemplate = new System.Windows.Forms.Button();
            this.lstTemplates = new System.Windows.Forms.ListBox();
            this.cmbTemplateType = new System.Windows.Forms.ComboBox();
            this.tabProperties = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel10 = new System.Windows.Forms.TableLayoutPanel();
            this.lblMapName = new System.Windows.Forms.Label();
            this.txtMapName = new System.Windows.Forms.TextBox();
            this.label23 = new System.Windows.Forms.Label();
            this.cbScrollEdge = new System.Windows.Forms.ComboBox();
            this.btnReloadSongs = new System.Windows.Forms.Button();
            this.lbxMusic = new System.Windows.Forms.ListBox();
            this.lblMusic = new System.Windows.Forms.Label();
            this.tabScript = new System.Windows.Forms.TabPage();
            this.flowLayoutPanel5 = new System.Windows.Forms.FlowLayoutPanel();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.btnMapReloadScripts = new System.Windows.Forms.Button();
            this.btnOpenScriptDir = new System.Windows.Forms.Button();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.chklstScriptMapCallbacks = new System.Windows.Forms.CheckedListBox();
            this.tabStrings = new System.Windows.Forms.TabPage();
            this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            this.label3 = new System.Windows.Forms.Label();
            this.gvStrings = new System.Windows.Forms.DataGridView();
            this.flowLayoutPanel3 = new System.Windows.Forms.FlowLayoutPanel();
            this.btnCommitStrings = new System.Windows.Forms.Button();
            this.btnReloadStrings = new System.Windows.Forms.Button();
            this.btnStringAdd = new System.Windows.Forms.Button();
            this.btnStringRem = new System.Windows.Forms.Button();
            this.tableLayoutPanel11 = new System.Windows.Forms.TableLayoutPanel();
            this.lbxLayers = new RogueEssence.Dev.LayerBox();
            this.tableLayoutPanel12 = new System.Windows.Forms.TableLayoutPanel();
            this.label24 = new System.Windows.Forms.Label();
            this.mnuMenu.SuspendLayout();
            this.tabMapOptions.SuspendLayout();
            this.tabTextures.SuspendLayout();
            this.tabBlock.SuspendLayout();
            this.tblBlockModes.SuspendLayout();
            this.tabEntities.SuspendLayout();
            this.tableLayoutPanel8.SuspendLayout();
            this.tableLayoutPanel7.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.tabctrlEntData.SuspendLayout();
            this.tabEntObjDisplay.SuspendLayout();
            this.tableLayoutPanel5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numEntObjStartFrame)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numEntObjEndFrame)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numEntObjFrameTime)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numEntObjAlpha)).BeginInit();
            this.tabEntCharDisplay.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picEntPreview)).BeginInit();
            this.tabEntScript.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.tabEntCharData.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numEntCharLevel)).BeginInit();
            this.tabEntSpawner.SuspendLayout();
            this.tableLayoutPanel6.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numEntWidth)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numEntHeight)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.tableLayoutPanel9.SuspendLayout();
            this.tabProperties.SuspendLayout();
            this.tableLayoutPanel10.SuspendLayout();
            this.tabScript.SuspendLayout();
            this.flowLayoutPanel5.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.tabStrings.SuspendLayout();
            this.flowLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gvStrings)).BeginInit();
            this.flowLayoutPanel3.SuspendLayout();
            this.tableLayoutPanel11.SuspendLayout();
            this.tableLayoutPanel12.SuspendLayout();
            this.SuspendLayout();
            // 
            // mnuMenu
            // 
            this.mnuMenu.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.mnuMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.editToolStripMenuItem});
            this.mnuMenu.Location = new System.Drawing.Point(0, 0);
            this.mnuMenu.Name = "mnuMenu";
            this.mnuMenu.Padding = new System.Windows.Forms.Padding(4, 1, 0, 1);
            this.mnuMenu.Size = new System.Drawing.Size(850, 24);
            this.mnuMenu.TabIndex = 0;
            this.mnuMenu.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newToolStripMenuItem,
            this.openToolStripMenuItem,
            this.saveToolStripMenuItem,
            this.saveAsToolStripMenuItem,
            this.importFromTilesetToolStripMenuItem,
            this.importFromPngToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 22);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // newToolStripMenuItem
            // 
            this.newToolStripMenuItem.Name = "newToolStripMenuItem";
            this.newToolStripMenuItem.Size = new System.Drawing.Size(165, 22);
            this.newToolStripMenuItem.Text = "New";
            this.newToolStripMenuItem.Click += new System.EventHandler(this.newToolStripMenuItem_Click);
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(165, 22);
            this.openToolStripMenuItem.Text = "Open";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(165, 22);
            this.saveToolStripMenuItem.Text = "Save";
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
            // 
            // saveAsToolStripMenuItem
            // 
            this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(165, 22);
            this.saveAsToolStripMenuItem.Text = "Save As";
            this.saveAsToolStripMenuItem.Click += new System.EventHandler(this.saveAsToolStripMenuItem_Click);
            // 
            // importFromPngToolStripMenuItem
            // 
            this.importFromTilesetToolStripMenuItem.Name = "importFromTilesetToolStripMenuItem";
            this.importFromTilesetToolStripMenuItem.Size = new System.Drawing.Size(165, 22);
            this.importFromTilesetToolStripMenuItem.Text = "Load Tileset to Map";
            this.importFromTilesetToolStripMenuItem.Click += new System.EventHandler(this.importFromTilesetToolStripMenuItem_Click);
            // 
            // importFromPngToolStripMenuItem
            // 
            this.importFromPngToolStripMenuItem.Name = "importFromPngToolStripMenuItem";
            this.importFromPngToolStripMenuItem.Size = new System.Drawing.Size(165, 22);
            this.importFromPngToolStripMenuItem.Text = "Import  Tileset from Png";
            this.importFromPngToolStripMenuItem.Click += new System.EventHandler(this.importFromPngToolStripMenuItem_Click);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.resizeMapToolStripMenuItem,
            this.retileMapToolStripMenuItem,
            this.undoToolStripMenuItem,
            this.redoToolStripMenuItem});
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(39, 22);
            this.editToolStripMenuItem.Text = "Edit";
            // 
            // resizeMapToolStripMenuItem
            // 
            this.resizeMapToolStripMenuItem.Name = "resizeMapToolStripMenuItem";
            this.resizeMapToolStripMenuItem.Size = new System.Drawing.Size(137, 22);
            this.resizeMapToolStripMenuItem.Text = "Resize Map";
            this.resizeMapToolStripMenuItem.Click += new System.EventHandler(this.resizeMapToolStripMenuItem_Click);
            // 
            // retileMapToolStripMenuItem
            // 
            this.retileMapToolStripMenuItem.Name = "retileMapToolStripMenuItem";
            this.retileMapToolStripMenuItem.Size = new System.Drawing.Size(137, 22);
            this.retileMapToolStripMenuItem.Text = "Re-Tile Map";
            this.retileMapToolStripMenuItem.Click += new System.EventHandler(this.retileMapToolStripMenuItem_Click);
            // 
            // undoToolStripMenuItem
            // 
            this.undoToolStripMenuItem.Enabled = false;
            this.undoToolStripMenuItem.Name = "undoToolStripMenuItem";
            this.undoToolStripMenuItem.Size = new System.Drawing.Size(137, 22);
            this.undoToolStripMenuItem.Text = "Undo";
            // 
            // redoToolStripMenuItem
            // 
            this.redoToolStripMenuItem.Enabled = false;
            this.redoToolStripMenuItem.Name = "redoToolStripMenuItem";
            this.redoToolStripMenuItem.Size = new System.Drawing.Size(137, 22);
            this.redoToolStripMenuItem.Text = "Redo";
            // 
            // openFileDialog
            // 
            this.openFileDialog.RestoreDirectory = true;
            // 
            // saveMapFileDialog
            // 
            this.saveMapFileDialog.RestoreDirectory = true;
            // 
            // tabMapOptions
            // 
            this.tabMapOptions.Controls.Add(this.tabTextures);
            this.tabMapOptions.Controls.Add(this.tabBlock);
            this.tabMapOptions.Controls.Add(this.tabEntities);
            this.tabMapOptions.Controls.Add(this.tabProperties);
            this.tabMapOptions.Controls.Add(this.tabScript);
            this.tabMapOptions.Controls.Add(this.tabStrings);
            this.tabMapOptions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabMapOptions.Location = new System.Drawing.Point(0, 24);
            this.tabMapOptions.Name = "tabMapOptions";
            this.tabMapOptions.SelectedIndex = 0;
            this.tabMapOptions.Size = new System.Drawing.Size(850, 475);
            this.tabMapOptions.TabIndex = 16;
            // 
            // tabTextures
            // 
            this.tabTextures.Controls.Add(this.tableLayoutPanel11);
            this.tabTextures.Location = new System.Drawing.Point(4, 22);
            this.tabTextures.Name = "tabTextures";
            this.tabTextures.Padding = new System.Windows.Forms.Padding(3);
            this.tabTextures.Size = new System.Drawing.Size(842, 449);
            this.tabTextures.TabIndex = 0;
            this.tabTextures.Text = "Textures";
            this.tabTextures.UseVisualStyleBackColor = true;
            // 
            // tileBrowser
            // 
            this.tileBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tileBrowser.Location = new System.Drawing.Point(150, 0);
            this.tileBrowser.Margin = new System.Windows.Forms.Padding(0);
            this.tileBrowser.Name = "tileBrowser";
            this.tileBrowser.Size = new System.Drawing.Size(686, 443);
            this.tileBrowser.TabIndex = 20;
            // 
            // tabBlock
            // 
            this.tabBlock.Controls.Add(this.tblBlockModes);
            this.tabBlock.Controls.Add(this.lblBlockMode);
            this.tabBlock.Location = new System.Drawing.Point(4, 22);
            this.tabBlock.Margin = new System.Windows.Forms.Padding(2);
            this.tabBlock.Name = "tabBlock";
            this.tabBlock.Padding = new System.Windows.Forms.Padding(2);
            this.tabBlock.Size = new System.Drawing.Size(842, 449);
            this.tabBlock.TabIndex = 5;
            this.tabBlock.Text = "Walls";
            this.tabBlock.UseVisualStyleBackColor = true;
            // 
            // tblBlockModes
            // 
            this.tblBlockModes.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tblBlockModes.ColumnCount = 3;
            this.tblBlockModes.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tblBlockModes.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tblBlockModes.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tblBlockModes.Controls.Add(this.rbBlockDraw, 0, 0);
            this.tblBlockModes.Controls.Add(this.rbBlockRectangle, 1, 0);
            this.tblBlockModes.Controls.Add(this.rbBlockFill, 2, 0);
            this.tblBlockModes.Location = new System.Drawing.Point(5, 17);
            this.tblBlockModes.Margin = new System.Windows.Forms.Padding(2);
            this.tblBlockModes.Name = "tblBlockModes";
            this.tblBlockModes.RowCount = 1;
            this.tblBlockModes.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblBlockModes.Size = new System.Drawing.Size(570, 20);
            this.tblBlockModes.TabIndex = 60;
            // 
            // rbBlockDraw
            // 
            this.rbBlockDraw.AutoSize = true;
            this.rbBlockDraw.Checked = true;
            this.rbBlockDraw.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rbBlockDraw.Location = new System.Drawing.Point(2, 2);
            this.rbBlockDraw.Margin = new System.Windows.Forms.Padding(2);
            this.rbBlockDraw.Name = "rbBlockDraw";
            this.rbBlockDraw.Size = new System.Drawing.Size(186, 16);
            this.rbBlockDraw.TabIndex = 0;
            this.rbBlockDraw.TabStop = true;
            this.rbBlockDraw.Text = "Draw";
            this.rbBlockDraw.UseVisualStyleBackColor = true;
            this.rbBlockDraw.CheckedChanged += new System.EventHandler(this.rbBlockDraw_CheckedChanged);
            // 
            // rbBlockRectangle
            // 
            this.rbBlockRectangle.AutoSize = true;
            this.rbBlockRectangle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rbBlockRectangle.Location = new System.Drawing.Point(192, 2);
            this.rbBlockRectangle.Margin = new System.Windows.Forms.Padding(2);
            this.rbBlockRectangle.Name = "rbBlockRectangle";
            this.rbBlockRectangle.Size = new System.Drawing.Size(186, 16);
            this.rbBlockRectangle.TabIndex = 1;
            this.rbBlockRectangle.Text = "Rectangle";
            this.rbBlockRectangle.UseVisualStyleBackColor = true;
            this.rbBlockRectangle.CheckedChanged += new System.EventHandler(this.rbBlockRectangle_CheckedChanged);
            // 
            // rbBlockFill
            // 
            this.rbBlockFill.AutoSize = true;
            this.rbBlockFill.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rbBlockFill.Location = new System.Drawing.Point(382, 2);
            this.rbBlockFill.Margin = new System.Windows.Forms.Padding(2);
            this.rbBlockFill.Name = "rbBlockFill";
            this.rbBlockFill.Size = new System.Drawing.Size(186, 16);
            this.rbBlockFill.TabIndex = 2;
            this.rbBlockFill.Text = "Fill";
            this.rbBlockFill.UseVisualStyleBackColor = true;
            this.rbBlockFill.CheckedChanged += new System.EventHandler(this.rbBlockFill_CheckedChanged);
            // 
            // lblBlockMode
            // 
            this.lblBlockMode.AutoSize = true;
            this.lblBlockMode.Location = new System.Drawing.Point(5, 2);
            this.lblBlockMode.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblBlockMode.Name = "lblBlockMode";
            this.lblBlockMode.Size = new System.Drawing.Size(37, 13);
            this.lblBlockMode.TabIndex = 0;
            this.lblBlockMode.Text = "Mode:";
            // 
            // tabEntities
            // 
            this.tabEntities.Controls.Add(this.tableLayoutPanel8);
            this.tabEntities.Controls.Add(this.label22);
            this.tabEntities.Controls.Add(this.tableLayoutPanel7);
            this.tabEntities.Location = new System.Drawing.Point(4, 22);
            this.tabEntities.Name = "tabEntities";
            this.tabEntities.Padding = new System.Windows.Forms.Padding(3);
            this.tabEntities.Size = new System.Drawing.Size(842, 449);
            this.tabEntities.TabIndex = 2;
            this.tabEntities.Text = "Entities";
            this.tabEntities.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel8
            // 
            this.tableLayoutPanel8.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel8.ColumnCount = 3;
            this.tableLayoutPanel8.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel8.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel8.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel8.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 13F));
            this.tableLayoutPanel8.Controls.Add(this.rbEntSelect, 0, 0);
            this.tableLayoutPanel8.Controls.Add(this.rbEntPlace, 1, 0);
            this.tableLayoutPanel8.Controls.Add(this.rbEntMove, 2, 0);
            this.tableLayoutPanel8.Location = new System.Drawing.Point(5, 16);
            this.tableLayoutPanel8.Margin = new System.Windows.Forms.Padding(2);
            this.tableLayoutPanel8.Name = "tableLayoutPanel8";
            this.tableLayoutPanel8.RowCount = 1;
            this.tableLayoutPanel8.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel8.Size = new System.Drawing.Size(631, 20);
            this.tableLayoutPanel8.TabIndex = 62;
            // 
            // rbEntSelect
            // 
            this.rbEntSelect.AutoSize = true;
            this.rbEntSelect.Checked = true;
            this.rbEntSelect.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rbEntSelect.Location = new System.Drawing.Point(2, 2);
            this.rbEntSelect.Margin = new System.Windows.Forms.Padding(2);
            this.rbEntSelect.Name = "rbEntSelect";
            this.rbEntSelect.Size = new System.Drawing.Size(206, 16);
            this.rbEntSelect.TabIndex = 0;
            this.rbEntSelect.TabStop = true;
            this.rbEntSelect.Text = "Select";
            this.rbEntSelect.UseVisualStyleBackColor = true;
            this.rbEntSelect.CheckedChanged += new System.EventHandler(this.rbEntSelect_CheckedChanged);
            // 
            // rbEntPlace
            // 
            this.rbEntPlace.AutoSize = true;
            this.rbEntPlace.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rbEntPlace.Location = new System.Drawing.Point(212, 2);
            this.rbEntPlace.Margin = new System.Windows.Forms.Padding(2);
            this.rbEntPlace.Name = "rbEntPlace";
            this.rbEntPlace.Size = new System.Drawing.Size(206, 16);
            this.rbEntPlace.TabIndex = 1;
            this.rbEntPlace.Text = "Place";
            this.rbEntPlace.UseVisualStyleBackColor = true;
            this.rbEntPlace.CheckedChanged += new System.EventHandler(this.rbEntPlace_CheckedChanged);
            // 
            // rbEntMove
            // 
            this.rbEntMove.AutoSize = true;
            this.rbEntMove.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rbEntMove.Location = new System.Drawing.Point(422, 2);
            this.rbEntMove.Margin = new System.Windows.Forms.Padding(2);
            this.rbEntMove.Name = "rbEntMove";
            this.rbEntMove.Size = new System.Drawing.Size(207, 16);
            this.rbEntMove.TabIndex = 3;
            this.rbEntMove.Text = "Move";
            this.rbEntMove.UseVisualStyleBackColor = true;
            this.rbEntMove.CheckedChanged += new System.EventHandler(this.rbEntMove_CheckedChanged);
            // 
            // label22
            // 
            this.label22.AutoSize = true;
            this.label22.Location = new System.Drawing.Point(5, 1);
            this.label22.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label22.Name = "label22";
            this.label22.Size = new System.Drawing.Size(37, 13);
            this.label22.TabIndex = 61;
            this.label22.Text = "Mode:";
            // 
            // tableLayoutPanel7
            // 
            this.tableLayoutPanel7.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel7.ColumnCount = 2;
            this.tableLayoutPanel7.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel7.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel7.Controls.Add(this.groupBox1, 0, 0);
            this.tableLayoutPanel7.Controls.Add(this.groupBox2, 1, 0);
            this.tableLayoutPanel7.Location = new System.Drawing.Point(0, 38);
            this.tableLayoutPanel7.Margin = new System.Windows.Forms.Padding(2);
            this.tableLayoutPanel7.Name = "tableLayoutPanel7";
            this.tableLayoutPanel7.RowCount = 1;
            this.tableLayoutPanel7.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel7.Size = new System.Drawing.Size(863, 560);
            this.tableLayoutPanel7.TabIndex = 1;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.tableLayoutPanel1);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(425, 554);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "New Entity";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 29.87552F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 70.12448F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 176F));
            this.tableLayoutPanel1.Controls.Add(this.cmbEntityDir, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.label5, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.label2, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.txtEntityName, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.cmbEntityType, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.tabctrlEntData, 0, 5);
            this.tableLayoutPanel1.Controls.Add(this.label15, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.numEntWidth, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.numEntHeight, 2, 3);
            this.tableLayoutPanel1.Controls.Add(this.btnAddToTemplates, 2, 6);
            this.tableLayoutPanel1.Controls.Add(this.chkEntEnabled, 1, 4);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 16);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 7;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 77F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 27F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(419, 535);
            this.tableLayoutPanel1.TabIndex = 12;
            // 
            // cmbEntityDir
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.cmbEntityDir, 2);
            this.cmbEntityDir.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmbEntityDir.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbEntityDir.FormattingEnabled = true;
            this.cmbEntityDir.Location = new System.Drawing.Point(75, 53);
            this.cmbEntityDir.Name = "cmbEntityDir";
            this.cmbEntityDir.Size = new System.Drawing.Size(341, 21);
            this.cmbEntityDir.TabIndex = 10;
            this.cmbEntityDir.SelectedIndexChanged += new System.EventHandler(this.cmbEntityDir_SelectedIndexChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label5.Location = new System.Drawing.Point(3, 50);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(66, 25);
            this.label5.TabIndex = 9;
            this.label5.Text = "Direction";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(66, 25);
            this.label1.TabIndex = 1;
            this.label1.Text = "Type";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label2.Location = new System.Drawing.Point(3, 25);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(66, 25);
            this.label2.TabIndex = 2;
            this.label2.Text = "Name";
            // 
            // txtEntityName
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.txtEntityName, 2);
            this.txtEntityName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtEntityName.Location = new System.Drawing.Point(75, 28);
            this.txtEntityName.Name = "txtEntityName";
            this.txtEntityName.Size = new System.Drawing.Size(341, 20);
            this.txtEntityName.TabIndex = 3;
            this.txtEntityName.Leave += new System.EventHandler(this.txtEntityName_Leave);
            // 
            // cmbEntityType
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.cmbEntityType, 2);
            this.cmbEntityType.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmbEntityType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbEntityType.FormattingEnabled = true;
            this.cmbEntityType.Location = new System.Drawing.Point(75, 3);
            this.cmbEntityType.Name = "cmbEntityType";
            this.cmbEntityType.Size = new System.Drawing.Size(341, 21);
            this.cmbEntityType.TabIndex = 0;
            this.cmbEntityType.SelectedIndexChanged += new System.EventHandler(this.cmbEntityType_SelectedIndexChanged);
            // 
            // tabctrlEntData
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.tabctrlEntData, 3);
            this.tabctrlEntData.Controls.Add(this.tabEntObjDisplay);
            this.tabctrlEntData.Controls.Add(this.tabEntCharDisplay);
            this.tabctrlEntData.Controls.Add(this.tabEntScript);
            this.tabctrlEntData.Controls.Add(this.tabEntCharData);
            this.tabctrlEntData.Controls.Add(this.tabEntSpawner);
            this.tabctrlEntData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabctrlEntData.Location = new System.Drawing.Point(0, 177);
            this.tabctrlEntData.Margin = new System.Windows.Forms.Padding(0);
            this.tabctrlEntData.Name = "tabctrlEntData";
            this.tabctrlEntData.SelectedIndex = 0;
            this.tabctrlEntData.Size = new System.Drawing.Size(419, 331);
            this.tabctrlEntData.TabIndex = 11;
            // 
            // tabEntObjDisplay
            // 
            this.tabEntObjDisplay.Controls.Add(this.tableLayoutPanel5);
            this.tabEntObjDisplay.Location = new System.Drawing.Point(4, 22);
            this.tabEntObjDisplay.Name = "tabEntObjDisplay";
            this.tabEntObjDisplay.Padding = new System.Windows.Forms.Padding(3);
            this.tabEntObjDisplay.Size = new System.Drawing.Size(411, 305);
            this.tabEntObjDisplay.TabIndex = 0;
            this.tabEntObjDisplay.Text = "Display[Object]";
            this.tabEntObjDisplay.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel5
            // 
            this.tableLayoutPanel5.ColumnCount = 2;
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel5.Controls.Add(this.label6, 0, 0);
            this.tableLayoutPanel5.Controls.Add(this.label16, 0, 2);
            this.tableLayoutPanel5.Controls.Add(this.listView1, 0, 1);
            this.tableLayoutPanel5.Controls.Add(this.numEntObjStartFrame, 1, 2);
            this.tableLayoutPanel5.Controls.Add(this.label17, 0, 3);
            this.tableLayoutPanel5.Controls.Add(this.numEntObjEndFrame, 1, 3);
            this.tableLayoutPanel5.Controls.Add(this.label18, 0, 4);
            this.tableLayoutPanel5.Controls.Add(this.numEntObjFrameTime, 1, 4);
            this.tableLayoutPanel5.Controls.Add(this.numEntObjAlpha, 1, 5);
            this.tableLayoutPanel5.Controls.Add(this.label19, 0, 5);
            this.tableLayoutPanel5.Controls.Add(this.cbEntObjSpriteID, 1, 0);
            this.tableLayoutPanel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel5.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel5.Name = "tableLayoutPanel5";
            this.tableLayoutPanel5.RowCount = 6;
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 172F));
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 49.12281F));
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50.87719F));
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 29F));
            this.tableLayoutPanel5.Size = new System.Drawing.Size(405, 299);
            this.tableLayoutPanel5.TabIndex = 3;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label6.Location = new System.Drawing.Point(3, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(196, 20);
            this.label6.TabIndex = 1;
            this.label6.Text = "Sprite";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label16.Location = new System.Drawing.Point(3, 192);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(196, 25);
            this.label16.TabIndex = 0;
            this.label16.Text = "Start Frame";
            // 
            // listView1
            // 
            this.tableLayoutPanel5.SetColumnSpan(this.listView1, 2);
            this.listView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView1.HideSelection = false;
            this.listView1.Location = new System.Drawing.Point(3, 23);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(399, 166);
            this.listView1.TabIndex = 0;
            this.listView1.UseCompatibleStateImageBehavior = false;
            // 
            // numEntObjStartFrame
            // 
            this.numEntObjStartFrame.Dock = System.Windows.Forms.DockStyle.Fill;
            this.numEntObjStartFrame.Location = new System.Drawing.Point(205, 195);
            this.numEntObjStartFrame.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
            this.numEntObjStartFrame.Name = "numEntObjStartFrame";
            this.numEntObjStartFrame.Size = new System.Drawing.Size(197, 20);
            this.numEntObjStartFrame.TabIndex = 1;
            this.numEntObjStartFrame.Value = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label17.Location = new System.Drawing.Point(3, 217);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(196, 26);
            this.label17.TabIndex = 2;
            this.label17.Text = "End Frame";
            // 
            // numEntObjEndFrame
            // 
            this.numEntObjEndFrame.Dock = System.Windows.Forms.DockStyle.Fill;
            this.numEntObjEndFrame.Location = new System.Drawing.Point(205, 220);
            this.numEntObjEndFrame.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
            this.numEntObjEndFrame.Name = "numEntObjEndFrame";
            this.numEntObjEndFrame.Size = new System.Drawing.Size(197, 20);
            this.numEntObjEndFrame.TabIndex = 3;
            this.numEntObjEndFrame.Value = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label18.Location = new System.Drawing.Point(3, 243);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(196, 26);
            this.label18.TabIndex = 4;
            this.label18.Text = "Frame Time";
            // 
            // numEntObjFrameTime
            // 
            this.numEntObjFrameTime.Dock = System.Windows.Forms.DockStyle.Fill;
            this.numEntObjFrameTime.Location = new System.Drawing.Point(205, 246);
            this.numEntObjFrameTime.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numEntObjFrameTime.Name = "numEntObjFrameTime";
            this.numEntObjFrameTime.Size = new System.Drawing.Size(197, 20);
            this.numEntObjFrameTime.TabIndex = 5;
            this.numEntObjFrameTime.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // numEntObjAlpha
            // 
            this.numEntObjAlpha.Dock = System.Windows.Forms.DockStyle.Fill;
            this.numEntObjAlpha.Location = new System.Drawing.Point(205, 272);
            this.numEntObjAlpha.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.numEntObjAlpha.Name = "numEntObjAlpha";
            this.numEntObjAlpha.Size = new System.Drawing.Size(197, 20);
            this.numEntObjAlpha.TabIndex = 6;
            this.numEntObjAlpha.Value = new decimal(new int[] {
            255,
            0,
            0,
            0});
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label19.Location = new System.Drawing.Point(3, 269);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(196, 30);
            this.label19.TabIndex = 7;
            this.label19.Text = "Alpha";
            // 
            // cbEntObjSpriteID
            // 
            this.cbEntObjSpriteID.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cbEntObjSpriteID.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbEntObjSpriteID.FormattingEnabled = true;
            this.cbEntObjSpriteID.Location = new System.Drawing.Point(204, 2);
            this.cbEntObjSpriteID.Margin = new System.Windows.Forms.Padding(2);
            this.cbEntObjSpriteID.Name = "cbEntObjSpriteID";
            this.cbEntObjSpriteID.Size = new System.Drawing.Size(199, 21);
            this.cbEntObjSpriteID.TabIndex = 8;
            this.cbEntObjSpriteID.SelectedIndexChanged += new System.EventHandler(this.cbEntObjSpriteID_SelectedIndexChanged);
            // 
            // tabEntCharDisplay
            // 
            this.tabEntCharDisplay.Controls.Add(this.tableLayoutPanel2);
            this.tabEntCharDisplay.Location = new System.Drawing.Point(4, 22);
            this.tabEntCharDisplay.Name = "tabEntCharDisplay";
            this.tabEntCharDisplay.Size = new System.Drawing.Size(411, 305);
            this.tabEntCharDisplay.TabIndex = 2;
            this.tabEntCharDisplay.Text = "Display[Character]";
            this.tabEntCharDisplay.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20.45455F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 79.54546F));
            this.tableLayoutPanel2.Controls.Add(this.picEntPreview, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.label7, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.label8, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.cmbEntKind, 1, 1);
            this.tableLayoutPanel2.Controls.Add(this.label9, 0, 2);
            this.tableLayoutPanel2.Controls.Add(this.chkEntRare, 1, 2);
            this.tableLayoutPanel2.Controls.Add(this.label10, 0, 3);
            this.tableLayoutPanel2.Controls.Add(this.cmbEntForm, 1, 3);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 5;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 85.39326F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 14.60674F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 67F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(411, 305);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // picEntPreview
            // 
            this.picEntPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.picEntPreview.Location = new System.Drawing.Point(87, 3);
            this.picEntPreview.Name = "picEntPreview";
            this.picEntPreview.Size = new System.Drawing.Size(321, 154);
            this.picEntPreview.TabIndex = 0;
            this.picEntPreview.TabStop = false;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label7.Location = new System.Drawing.Point(3, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(78, 160);
            this.label7.TabIndex = 1;
            this.label7.Text = "Preview";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label8.Location = new System.Drawing.Point(3, 160);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(78, 27);
            this.label8.TabIndex = 2;
            this.label8.Text = "Kind";
            // 
            // cmbEntKind
            // 
            this.cmbEntKind.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.cmbEntKind.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cmbEntKind.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmbEntKind.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbEntKind.FormattingEnabled = true;
            this.cmbEntKind.Location = new System.Drawing.Point(87, 163);
            this.cmbEntKind.Name = "cmbEntKind";
            this.cmbEntKind.Size = new System.Drawing.Size(321, 21);
            this.cmbEntKind.TabIndex = 3;
            this.cmbEntKind.SelectedIndexChanged += new System.EventHandler(this.cmbEntKind_SelectedIndexChanged);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label9.Location = new System.Drawing.Point(3, 187);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(78, 22);
            this.label9.TabIndex = 4;
            this.label9.Text = "Settings";
            // 
            // chkEntRare
            // 
            this.chkEntRare.AutoSize = true;
            this.chkEntRare.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkEntRare.Location = new System.Drawing.Point(87, 190);
            this.chkEntRare.Name = "chkEntRare";
            this.chkEntRare.Size = new System.Drawing.Size(321, 16);
            this.chkEntRare.TabIndex = 5;
            this.chkEntRare.Text = "Rare";
            this.chkEntRare.UseVisualStyleBackColor = true;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label10.Location = new System.Drawing.Point(3, 209);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(78, 28);
            this.label10.TabIndex = 6;
            this.label10.Text = "Form";
            // 
            // cmbEntForm
            // 
            this.cmbEntForm.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmbEntForm.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbEntForm.FormattingEnabled = true;
            this.cmbEntForm.Location = new System.Drawing.Point(87, 212);
            this.cmbEntForm.Name = "cmbEntForm";
            this.cmbEntForm.Size = new System.Drawing.Size(321, 21);
            this.cmbEntForm.TabIndex = 7;
            this.cmbEntForm.SelectedIndexChanged += new System.EventHandler(this.cmbEntForm_SelectedIndexChanged);
            // 
            // tabEntScript
            // 
            this.tabEntScript.Controls.Add(this.tableLayoutPanel3);
            this.tabEntScript.Location = new System.Drawing.Point(4, 22);
            this.tabEntScript.Name = "tabEntScript";
            this.tabEntScript.Size = new System.Drawing.Size(411, 305);
            this.tabEntScript.TabIndex = 3;
            this.tabEntScript.Text = "Script";
            this.tabEntScript.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 2;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 21.6763F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 78.3237F));
            this.tableLayoutPanel3.Controls.Add(this.label11, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.cmbEntTriggerType, 1, 0);
            this.tableLayoutPanel3.Controls.Add(this.label12, 0, 1);
            this.tableLayoutPanel3.Controls.Add(this.chklstEntScriptCallbacks, 1, 1);
            this.tableLayoutPanel3.Controls.Add(this.lblScriptSecondaryCallbacks, 0, 2);
            this.tableLayoutPanel3.Controls.Add(this.chklstScriptSecondaryCallbacks, 1, 2);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 3;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 21.42857F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 78.57143F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 141F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(411, 305);
            this.tableLayoutPanel3.TabIndex = 1;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label11.Location = new System.Drawing.Point(3, 0);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(83, 35);
            this.label11.TabIndex = 0;
            this.label11.Text = "Trigger Type";
            // 
            // cmbEntTriggerType
            // 
            this.cmbEntTriggerType.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmbEntTriggerType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbEntTriggerType.FormattingEnabled = true;
            this.cmbEntTriggerType.Location = new System.Drawing.Point(92, 3);
            this.cmbEntTriggerType.Name = "cmbEntTriggerType";
            this.cmbEntTriggerType.Size = new System.Drawing.Size(316, 21);
            this.cmbEntTriggerType.TabIndex = 1;
            this.cmbEntTriggerType.SelectedIndexChanged += new System.EventHandler(this.cmbEntTriggerType_SelectedIndexChanged);
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label12.Location = new System.Drawing.Point(3, 35);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(83, 128);
            this.label12.TabIndex = 2;
            this.label12.Text = "Callbacks";
            // 
            // chklstEntScriptCallbacks
            // 
            this.chklstEntScriptCallbacks.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chklstEntScriptCallbacks.FormattingEnabled = true;
            this.chklstEntScriptCallbacks.Location = new System.Drawing.Point(92, 38);
            this.chklstEntScriptCallbacks.Name = "chklstEntScriptCallbacks";
            this.chklstEntScriptCallbacks.Size = new System.Drawing.Size(316, 122);
            this.chklstEntScriptCallbacks.TabIndex = 3;
            this.chklstEntScriptCallbacks.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.chklstEntScriptCallbacks_ItemCheck);
            // 
            // lblScriptSecondaryCallbacks
            // 
            this.lblScriptSecondaryCallbacks.AutoSize = true;
            this.lblScriptSecondaryCallbacks.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblScriptSecondaryCallbacks.Location = new System.Drawing.Point(3, 163);
            this.lblScriptSecondaryCallbacks.Name = "lblScriptSecondaryCallbacks";
            this.lblScriptSecondaryCallbacks.Size = new System.Drawing.Size(83, 142);
            this.lblScriptSecondaryCallbacks.TabIndex = 4;
            this.lblScriptSecondaryCallbacks.Text = "Spawnee Callbacks:";
            // 
            // chklstScriptSecondaryCallbacks
            // 
            this.chklstScriptSecondaryCallbacks.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chklstScriptSecondaryCallbacks.FormattingEnabled = true;
            this.chklstScriptSecondaryCallbacks.Location = new System.Drawing.Point(92, 166);
            this.chklstScriptSecondaryCallbacks.Name = "chklstScriptSecondaryCallbacks";
            this.chklstScriptSecondaryCallbacks.Size = new System.Drawing.Size(316, 136);
            this.chklstScriptSecondaryCallbacks.TabIndex = 5;
            this.chklstScriptSecondaryCallbacks.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.chklstScriptSecondaryCallbacks_ItemCheck);
            // 
            // tabEntCharData
            // 
            this.tabEntCharData.Controls.Add(this.tableLayoutPanel4);
            this.tabEntCharData.Location = new System.Drawing.Point(4, 22);
            this.tabEntCharData.Name = "tabEntCharData";
            this.tabEntCharData.Padding = new System.Windows.Forms.Padding(3);
            this.tabEntCharData.Size = new System.Drawing.Size(411, 305);
            this.tabEntCharData.TabIndex = 4;
            this.tabEntCharData.Text = "Data[Character]";
            this.tabEntCharData.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.ColumnCount = 2;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 18.49711F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 81.50289F));
            this.tableLayoutPanel4.Controls.Add(this.label4, 0, 0);
            this.tableLayoutPanel4.Controls.Add(this.txtEntCharNickname, 1, 0);
            this.tableLayoutPanel4.Controls.Add(this.label13, 0, 1);
            this.tableLayoutPanel4.Controls.Add(this.numEntCharLevel, 1, 1);
            this.tableLayoutPanel4.Controls.Add(this.label14, 0, 2);
            this.tableLayoutPanel4.Controls.Add(this.cmbEntCharGender, 1, 2);
            this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel4.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 3;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 49.20635F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50.79365F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 250F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(405, 299);
            this.tableLayoutPanel4.TabIndex = 0;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label4.Location = new System.Drawing.Point(3, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(68, 24);
            this.label4.TabIndex = 0;
            this.label4.Text = "Nickname";
            // 
            // txtEntCharNickname
            // 
            this.txtEntCharNickname.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtEntCharNickname.Location = new System.Drawing.Point(77, 3);
            this.txtEntCharNickname.Name = "txtEntCharNickname";
            this.txtEntCharNickname.Size = new System.Drawing.Size(325, 20);
            this.txtEntCharNickname.TabIndex = 1;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label13.Location = new System.Drawing.Point(3, 24);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(68, 24);
            this.label13.TabIndex = 2;
            this.label13.Text = "Level";
            // 
            // numEntCharLevel
            // 
            this.numEntCharLevel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.numEntCharLevel.Location = new System.Drawing.Point(77, 27);
            this.numEntCharLevel.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numEntCharLevel.Name = "numEntCharLevel";
            this.numEntCharLevel.Size = new System.Drawing.Size(325, 20);
            this.numEntCharLevel.TabIndex = 3;
            this.numEntCharLevel.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label14.Location = new System.Drawing.Point(3, 48);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(68, 251);
            this.label14.TabIndex = 4;
            this.label14.Text = "Gender";
            // 
            // cmbEntCharGender
            // 
            this.cmbEntCharGender.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmbEntCharGender.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbEntCharGender.FormattingEnabled = true;
            this.cmbEntCharGender.Location = new System.Drawing.Point(77, 51);
            this.cmbEntCharGender.Name = "cmbEntCharGender";
            this.cmbEntCharGender.Size = new System.Drawing.Size(325, 21);
            this.cmbEntCharGender.TabIndex = 5;
            // 
            // tabEntSpawner
            // 
            this.tabEntSpawner.Controls.Add(this.tableLayoutPanel6);
            this.tabEntSpawner.Location = new System.Drawing.Point(4, 22);
            this.tabEntSpawner.Name = "tabEntSpawner";
            this.tabEntSpawner.Padding = new System.Windows.Forms.Padding(3);
            this.tabEntSpawner.Size = new System.Drawing.Size(411, 305);
            this.tabEntSpawner.TabIndex = 5;
            this.tabEntSpawner.Text = "Spawner Data";
            this.tabEntSpawner.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel6
            // 
            this.tableLayoutPanel6.ColumnCount = 2;
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 41.04046F));
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 58.95954F));
            this.tableLayoutPanel6.Controls.Add(this.label20, 0, 0);
            this.tableLayoutPanel6.Controls.Add(this.txtSpawnedEntName, 1, 0);
            this.tableLayoutPanel6.Controls.Add(this.label21, 0, 1);
            this.tableLayoutPanel6.Controls.Add(this.cmbSpawnerType, 1, 1);
            this.tableLayoutPanel6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel6.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel6.Name = "tableLayoutPanel6";
            this.tableLayoutPanel6.RowCount = 3;
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel6.Size = new System.Drawing.Size(405, 299);
            this.tableLayoutPanel6.TabIndex = 0;
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label20.Location = new System.Drawing.Point(3, 0);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(160, 26);
            this.label20.TabIndex = 0;
            this.label20.Text = "Spawned Entity Name:";
            // 
            // txtSpawnedEntName
            // 
            this.txtSpawnedEntName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtSpawnedEntName.Location = new System.Drawing.Point(169, 3);
            this.txtSpawnedEntName.Name = "txtSpawnedEntName";
            this.txtSpawnedEntName.Size = new System.Drawing.Size(233, 20);
            this.txtSpawnedEntName.TabIndex = 1;
            this.txtSpawnedEntName.TextChanged += new System.EventHandler(this.txtSpawnedEntName_TextChanged);
            // 
            // label21
            // 
            this.label21.AutoSize = true;
            this.label21.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label21.Location = new System.Drawing.Point(3, 26);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(160, 27);
            this.label21.TabIndex = 2;
            this.label21.Text = "Spawner Type:";
            // 
            // cmbSpawnerType
            // 
            this.cmbSpawnerType.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmbSpawnerType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbSpawnerType.Enabled = false;
            this.cmbSpawnerType.Items.AddRange(new object[] {
            "Random Team Member"});
            this.cmbSpawnerType.Location = new System.Drawing.Point(169, 29);
            this.cmbSpawnerType.Name = "cmbSpawnerType";
            this.cmbSpawnerType.Size = new System.Drawing.Size(233, 21);
            this.cmbSpawnerType.TabIndex = 3;
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label15.Location = new System.Drawing.Point(3, 75);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(66, 25);
            this.label15.TabIndex = 13;
            this.label15.Text = "Bounds";
            // 
            // numEntWidth
            // 
            this.numEntWidth.Dock = System.Windows.Forms.DockStyle.Fill;
            this.numEntWidth.Location = new System.Drawing.Point(75, 78);
            this.numEntWidth.Maximum = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            this.numEntWidth.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numEntWidth.Name = "numEntWidth";
            this.numEntWidth.Size = new System.Drawing.Size(164, 20);
            this.numEntWidth.TabIndex = 14;
            this.numEntWidth.Value = new decimal(new int[] {
            8,
            0,
            0,
            0});
            this.numEntWidth.ValueChanged += new System.EventHandler(this.numEntWidth_ValueChanged);
            // 
            // numEntHeight
            // 
            this.numEntHeight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.numEntHeight.Location = new System.Drawing.Point(245, 78);
            this.numEntHeight.Maximum = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            this.numEntHeight.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numEntHeight.Name = "numEntHeight";
            this.numEntHeight.Size = new System.Drawing.Size(171, 20);
            this.numEntHeight.TabIndex = 15;
            this.numEntHeight.Value = new decimal(new int[] {
            8,
            0,
            0,
            0});
            this.numEntHeight.ValueChanged += new System.EventHandler(this.numEntHeight_ValueChanged);
            // 
            // btnAddToTemplates
            // 
            this.btnAddToTemplates.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnAddToTemplates.Location = new System.Drawing.Point(245, 511);
            this.btnAddToTemplates.Name = "btnAddToTemplates";
            this.btnAddToTemplates.Size = new System.Drawing.Size(171, 21);
            this.btnAddToTemplates.TabIndex = 5;
            this.btnAddToTemplates.Text = "Add to templates";
            this.btnAddToTemplates.UseVisualStyleBackColor = true;
            this.btnAddToTemplates.Click += new System.EventHandler(this.btnAddToTemplates_Click);
            // 
            // chkEntEnabled
            // 
            this.chkEntEnabled.AutoSize = true;
            this.chkEntEnabled.Checked = true;
            this.chkEntEnabled.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkEntEnabled.Location = new System.Drawing.Point(75, 103);
            this.chkEntEnabled.Name = "chkEntEnabled";
            this.chkEntEnabled.Size = new System.Drawing.Size(65, 17);
            this.chkEntEnabled.TabIndex = 18;
            this.chkEntEnabled.Text = "Enabled";
            this.chkEntEnabled.UseVisualStyleBackColor = true;
            this.chkEntEnabled.CheckedChanged += new System.EventHandler(this.chkEntEnabled_CheckedChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.tableLayoutPanel9);
            this.groupBox2.Controls.Add(this.lstTemplates);
            this.groupBox2.Controls.Add(this.cmbTemplateType);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox2.Location = new System.Drawing.Point(434, 3);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(426, 554);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Templates";
            // 
            // tableLayoutPanel9
            // 
            this.tableLayoutPanel9.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel9.ColumnCount = 2;
            this.tableLayoutPanel9.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel9.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel9.Controls.Add(this.btnLoadFromTemplate, 0, 0);
            this.tableLayoutPanel9.Controls.Add(this.btnRemTemplate, 1, 0);
            this.tableLayoutPanel9.Location = new System.Drawing.Point(7, 522);
            this.tableLayoutPanel9.Margin = new System.Windows.Forms.Padding(2);
            this.tableLayoutPanel9.Name = "tableLayoutPanel9";
            this.tableLayoutPanel9.RowCount = 1;
            this.tableLayoutPanel9.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel9.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 29F));
            this.tableLayoutPanel9.Size = new System.Drawing.Size(415, 29);
            this.tableLayoutPanel9.TabIndex = 5;
            // 
            // btnLoadFromTemplate
            // 
            this.btnLoadFromTemplate.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnLoadFromTemplate.Enabled = false;
            this.btnLoadFromTemplate.Location = new System.Drawing.Point(3, 3);
            this.btnLoadFromTemplate.Name = "btnLoadFromTemplate";
            this.btnLoadFromTemplate.Size = new System.Drawing.Size(201, 23);
            this.btnLoadFromTemplate.TabIndex = 4;
            this.btnLoadFromTemplate.Text = "Load From Template";
            this.btnLoadFromTemplate.UseVisualStyleBackColor = true;
            // 
            // btnRemTemplate
            // 
            this.btnRemTemplate.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnRemTemplate.Enabled = false;
            this.btnRemTemplate.Location = new System.Drawing.Point(210, 3);
            this.btnRemTemplate.Name = "btnRemTemplate";
            this.btnRemTemplate.Size = new System.Drawing.Size(202, 23);
            this.btnRemTemplate.TabIndex = 3;
            this.btnRemTemplate.Text = "Remove Template";
            this.btnRemTemplate.UseVisualStyleBackColor = true;
            // 
            // lstTemplates
            // 
            this.lstTemplates.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstTemplates.FormattingEnabled = true;
            this.lstTemplates.Location = new System.Drawing.Point(6, 47);
            this.lstTemplates.Name = "lstTemplates";
            this.lstTemplates.ScrollAlwaysVisible = true;
            this.lstTemplates.Size = new System.Drawing.Size(416, 472);
            this.lstTemplates.Sorted = true;
            this.lstTemplates.TabIndex = 1;
            // 
            // cmbTemplateType
            // 
            this.cmbTemplateType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbTemplateType.FormattingEnabled = true;
            this.cmbTemplateType.Location = new System.Drawing.Point(6, 20);
            this.cmbTemplateType.Name = "cmbTemplateType";
            this.cmbTemplateType.Size = new System.Drawing.Size(416, 21);
            this.cmbTemplateType.TabIndex = 0;
            this.cmbTemplateType.SelectedIndexChanged += new System.EventHandler(this.cmbTemplateType_SelectedIndexChanged);
            // 
            // tabProperties
            // 
            this.tabProperties.Controls.Add(this.tableLayoutPanel10);
            this.tabProperties.Controls.Add(this.btnReloadSongs);
            this.tabProperties.Controls.Add(this.lbxMusic);
            this.tabProperties.Controls.Add(this.lblMusic);
            this.tabProperties.Location = new System.Drawing.Point(4, 22);
            this.tabProperties.Name = "tabProperties";
            this.tabProperties.Padding = new System.Windows.Forms.Padding(3);
            this.tabProperties.Size = new System.Drawing.Size(842, 449);
            this.tabProperties.TabIndex = 1;
            this.tabProperties.Text = "Properties";
            this.tabProperties.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel10
            // 
            this.tableLayoutPanel10.ColumnCount = 1;
            this.tableLayoutPanel10.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel10.Controls.Add(this.lblMapName, 0, 0);
            this.tableLayoutPanel10.Controls.Add(this.txtMapName, 0, 1);
            this.tableLayoutPanel10.Controls.Add(this.label23, 0, 2);
            this.tableLayoutPanel10.Controls.Add(this.cbScrollEdge, 0, 3);
            this.tableLayoutPanel10.Location = new System.Drawing.Point(8, 12);
            this.tableLayoutPanel10.Name = "tableLayoutPanel10";
            this.tableLayoutPanel10.RowCount = 4;
            this.tableLayoutPanel10.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 15.84158F));
            this.tableLayoutPanel10.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 27.47253F));
            this.tableLayoutPanel10.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 29.67033F));
            this.tableLayoutPanel10.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 28.57143F));
            this.tableLayoutPanel10.Size = new System.Drawing.Size(347, 102);
            this.tableLayoutPanel10.TabIndex = 7;
            // 
            // lblMapName
            // 
            this.lblMapName.AutoSize = true;
            this.lblMapName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblMapName.Location = new System.Drawing.Point(3, 0);
            this.lblMapName.Name = "lblMapName";
            this.lblMapName.Size = new System.Drawing.Size(341, 15);
            this.lblMapName.TabIndex = 1;
            this.lblMapName.Text = "Map Name";
            this.lblMapName.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // txtMapName
            // 
            this.txtMapName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtMapName.Location = new System.Drawing.Point(3, 18);
            this.txtMapName.Name = "txtMapName";
            this.txtMapName.Size = new System.Drawing.Size(341, 20);
            this.txtMapName.TabIndex = 6;
            this.txtMapName.TextChanged += new System.EventHandler(this.txtMapName_TextChanged);
            // 
            // label23
            // 
            this.label23.AutoSize = true;
            this.label23.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label23.Location = new System.Drawing.Point(3, 42);
            this.label23.Name = "label23";
            this.label23.Size = new System.Drawing.Size(341, 29);
            this.label23.TabIndex = 7;
            this.label23.Text = "Scroll Edge";
            this.label23.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // cbScrollEdge
            // 
            this.cbScrollEdge.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cbScrollEdge.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbScrollEdge.FormattingEnabled = true;
            this.cbScrollEdge.Location = new System.Drawing.Point(3, 74);
            this.cbScrollEdge.Name = "cbScrollEdge";
            this.cbScrollEdge.Size = new System.Drawing.Size(341, 21);
            this.cbScrollEdge.TabIndex = 8;
            this.cbScrollEdge.SelectedIndexChanged += new System.EventHandler(this.cbScrollEdge_SelectedIndexChanged);
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
            // tabScript
            // 
            this.tabScript.Controls.Add(this.flowLayoutPanel5);
            this.tabScript.Location = new System.Drawing.Point(4, 22);
            this.tabScript.Name = "tabScript";
            this.tabScript.Padding = new System.Windows.Forms.Padding(3);
            this.tabScript.Size = new System.Drawing.Size(842, 449);
            this.tabScript.TabIndex = 3;
            this.tabScript.Text = "Script";
            this.tabScript.UseVisualStyleBackColor = true;
            // 
            // flowLayoutPanel5
            // 
            this.flowLayoutPanel5.Controls.Add(this.groupBox3);
            this.flowLayoutPanel5.Controls.Add(this.groupBox4);
            this.flowLayoutPanel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel5.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel5.Location = new System.Drawing.Point(3, 3);
            this.flowLayoutPanel5.Name = "flowLayoutPanel5";
            this.flowLayoutPanel5.Size = new System.Drawing.Size(836, 443);
            this.flowLayoutPanel5.TabIndex = 2;
            this.flowLayoutPanel5.WrapContents = false;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.btnMapReloadScripts);
            this.groupBox3.Controls.Add(this.btnOpenScriptDir);
            this.groupBox3.Location = new System.Drawing.Point(3, 3);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(663, 53);
            this.groupBox3.TabIndex = 0;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Script Data";
            // 
            // btnMapReloadScripts
            // 
            this.btnMapReloadScripts.Location = new System.Drawing.Point(140, 18);
            this.btnMapReloadScripts.Name = "btnMapReloadScripts";
            this.btnMapReloadScripts.Size = new System.Drawing.Size(100, 23);
            this.btnMapReloadScripts.TabIndex = 3;
            this.btnMapReloadScripts.Text = "Reload Scripts";
            this.btnMapReloadScripts.UseVisualStyleBackColor = true;
            this.btnMapReloadScripts.Click += new System.EventHandler(this.btnMapReloadScripts_Click);
            // 
            // btnOpenScriptDir
            // 
            this.btnOpenScriptDir.Location = new System.Drawing.Point(6, 19);
            this.btnOpenScriptDir.Name = "btnOpenScriptDir";
            this.btnOpenScriptDir.Size = new System.Drawing.Size(127, 23);
            this.btnOpenScriptDir.TabIndex = 2;
            this.btnOpenScriptDir.Text = "Open script directory";
            this.btnOpenScriptDir.UseVisualStyleBackColor = true;
            this.btnOpenScriptDir.Click += new System.EventHandler(this.btnOpenScriptDir_Click);
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.chklstScriptMapCallbacks);
            this.groupBox4.Location = new System.Drawing.Point(3, 62);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(222, 108);
            this.groupBox4.TabIndex = 1;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Map Callbacks";
            // 
            // chklstScriptMapCallbacks
            // 
            this.chklstScriptMapCallbacks.CheckOnClick = true;
            this.chklstScriptMapCallbacks.FormattingEnabled = true;
            this.chklstScriptMapCallbacks.Location = new System.Drawing.Point(6, 19);
            this.chklstScriptMapCallbacks.Name = "chklstScriptMapCallbacks";
            this.chklstScriptMapCallbacks.Size = new System.Drawing.Size(206, 79);
            this.chklstScriptMapCallbacks.TabIndex = 0;
            this.chklstScriptMapCallbacks.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.chklstScriptMapCallbacks_ItemCheck);
            // 
            // tabStrings
            // 
            this.tabStrings.Controls.Add(this.flowLayoutPanel2);
            this.tabStrings.Location = new System.Drawing.Point(4, 22);
            this.tabStrings.Name = "tabStrings";
            this.tabStrings.Padding = new System.Windows.Forms.Padding(3);
            this.tabStrings.Size = new System.Drawing.Size(842, 449);
            this.tabStrings.TabIndex = 4;
            this.tabStrings.Text = "Strings";
            this.tabStrings.UseVisualStyleBackColor = true;
            // 
            // flowLayoutPanel2
            // 
            this.flowLayoutPanel2.Controls.Add(this.label3);
            this.flowLayoutPanel2.Controls.Add(this.gvStrings);
            this.flowLayoutPanel2.Controls.Add(this.flowLayoutPanel3);
            this.flowLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel2.Location = new System.Drawing.Point(3, 3);
            this.flowLayoutPanel2.Margin = new System.Windows.Forms.Padding(0);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            this.flowLayoutPanel2.Size = new System.Drawing.Size(836, 443);
            this.flowLayoutPanel2.TabIndex = 4;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(368, 13);
            this.label3.TabIndex = 1;
            this.label3.Text = "Add, edit or remove localized text strings to be used by the script on the map.";
            // 
            // gvStrings
            // 
            this.gvStrings.AllowUserToAddRows = false;
            this.gvStrings.AllowUserToDeleteRows = false;
            this.gvStrings.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.gvStrings.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.gvStrings.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gvStrings.Location = new System.Drawing.Point(3, 16);
            this.gvStrings.Name = "gvStrings";
            this.gvStrings.RowHeadersWidth = 62;
            this.gvStrings.RowTemplate.DefaultCellStyle.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.gvStrings.RowTemplate.Height = 32;
            this.gvStrings.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.gvStrings.Size = new System.Drawing.Size(640, 382);
            this.gvStrings.TabIndex = 0;
            this.gvStrings.CellValidating += new System.Windows.Forms.DataGridViewCellValidatingEventHandler(this.gvStrings_CellValidating);
            // 
            // flowLayoutPanel3
            // 
            this.flowLayoutPanel3.AutoSize = true;
            this.flowLayoutPanel3.Controls.Add(this.btnCommitStrings);
            this.flowLayoutPanel3.Controls.Add(this.btnReloadStrings);
            this.flowLayoutPanel3.Controls.Add(this.btnStringAdd);
            this.flowLayoutPanel3.Controls.Add(this.btnStringRem);
            this.flowLayoutPanel3.Location = new System.Drawing.Point(3, 404);
            this.flowLayoutPanel3.Name = "flowLayoutPanel3";
            this.flowLayoutPanel3.Size = new System.Drawing.Size(369, 29);
            this.flowLayoutPanel3.TabIndex = 6;
            // 
            // btnCommitStrings
            // 
            this.btnCommitStrings.Enabled = false;
            this.btnCommitStrings.Location = new System.Drawing.Point(3, 3);
            this.btnCommitStrings.Name = "btnCommitStrings";
            this.btnCommitStrings.Size = new System.Drawing.Size(75, 23);
            this.btnCommitStrings.TabIndex = 2;
            this.btnCommitStrings.Text = "Commit";
            this.btnCommitStrings.UseVisualStyleBackColor = true;
            this.btnCommitStrings.Click += new System.EventHandler(this.btnCommitStrings_Click);
            // 
            // btnReloadStrings
            // 
            this.btnReloadStrings.Enabled = false;
            this.btnReloadStrings.Location = new System.Drawing.Point(84, 3);
            this.btnReloadStrings.Name = "btnReloadStrings";
            this.btnReloadStrings.Size = new System.Drawing.Size(75, 23);
            this.btnReloadStrings.TabIndex = 3;
            this.btnReloadStrings.Text = "Reload";
            this.btnReloadStrings.UseVisualStyleBackColor = true;
            this.btnReloadStrings.Click += new System.EventHandler(this.btnReloadStrings_Click);
            // 
            // btnStringAdd
            // 
            this.btnStringAdd.Location = new System.Drawing.Point(165, 3);
            this.btnStringAdd.Name = "btnStringAdd";
            this.btnStringAdd.Size = new System.Drawing.Size(75, 23);
            this.btnStringAdd.TabIndex = 4;
            this.btnStringAdd.Text = "Add String";
            this.btnStringAdd.UseVisualStyleBackColor = true;
            this.btnStringAdd.Click += new System.EventHandler(this.btnStringAdd_Click);
            // 
            // btnStringRem
            // 
            this.btnStringRem.Location = new System.Drawing.Point(246, 3);
            this.btnStringRem.Name = "btnStringRem";
            this.btnStringRem.Size = new System.Drawing.Size(120, 23);
            this.btnStringRem.TabIndex = 5;
            this.btnStringRem.Text = "Remove String";
            this.btnStringRem.UseVisualStyleBackColor = true;
            this.btnStringRem.Click += new System.EventHandler(this.btnStringRem_Click);
            // 
            // tableLayoutPanel11
            // 
            this.tableLayoutPanel11.ColumnCount = 2;
            this.tableLayoutPanel11.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 150F));
            this.tableLayoutPanel11.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel11.Controls.Add(this.tableLayoutPanel12, 0, 0);
            this.tableLayoutPanel11.Controls.Add(this.tileBrowser, 1, 0);
            this.tableLayoutPanel11.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel11.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel11.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel11.Name = "tableLayoutPanel11";
            this.tableLayoutPanel11.RowCount = 1;
            this.tableLayoutPanel11.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel11.Size = new System.Drawing.Size(836, 443);
            this.tableLayoutPanel11.TabIndex = 21;
            // 
            // lbxLayers
            // 
            this.lbxLayers.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbxLayers.Location = new System.Drawing.Point(0, 20);
            this.lbxLayers.Margin = new System.Windows.Forms.Padding(0);
            this.lbxLayers.Name = "lbxLayers";
            this.lbxLayers.SelectedIndex = -1;
            this.lbxLayers.Size = new System.Drawing.Size(150, 423);
            this.lbxLayers.TabIndex = 21;
            this.lbxLayers.CheckChanged += new System.Windows.Forms.ItemCheckEventHandler(this.lbxLayers_CheckChanged);
            this.lbxLayers.OnAddItem += new RogueEssence.Dev.LayerBox.CheckElementOp(this.lbxLayers_OnAddItem);
            this.lbxLayers.OnEditItem += new RogueEssence.Dev.LayerBox.ElementOp(this.lbxLayers_OnEditItem);
            this.lbxLayers.OnDuplicateItem += new RogueEssence.Dev.LayerBox.CheckElementOp(this.lbxLayers_OnDuplicateItem);
            // 
            // tableLayoutPanel12
            // 
            this.tableLayoutPanel12.ColumnCount = 1;
            this.tableLayoutPanel12.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel12.Controls.Add(this.lbxLayers, 0, 1);
            this.tableLayoutPanel12.Controls.Add(this.label24, 0, 0);
            this.tableLayoutPanel12.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel12.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel12.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel12.Name = "tableLayoutPanel12";
            this.tableLayoutPanel12.RowCount = 2;
            this.tableLayoutPanel12.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel12.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel12.Size = new System.Drawing.Size(150, 443);
            this.tableLayoutPanel12.TabIndex = 22;
            // 
            // label24
            // 
            this.label24.AutoSize = true;
            this.label24.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label24.Location = new System.Drawing.Point(3, 0);
            this.label24.Name = "label24";
            this.label24.Size = new System.Drawing.Size(144, 20);
            this.label24.TabIndex = 22;
            this.label24.Text = "Layers:";
            this.label24.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // GroundEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(850, 499);
            this.Controls.Add(this.tabMapOptions);
            this.Controls.Add(this.mnuMenu);
            this.DoubleBuffered = true;
            this.MainMenuStrip = this.mnuMenu;
            this.Name = "GroundEditor";
            this.Text = "Ground Editor";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.GroundEditor_FormClosed);
            this.Load += new System.EventHandler(this.GroundEditor_Load);
            this.mnuMenu.ResumeLayout(false);
            this.mnuMenu.PerformLayout();
            this.tabMapOptions.ResumeLayout(false);
            this.tabTextures.ResumeLayout(false);
            this.tabBlock.ResumeLayout(false);
            this.tabBlock.PerformLayout();
            this.tblBlockModes.ResumeLayout(false);
            this.tblBlockModes.PerformLayout();
            this.tabEntities.ResumeLayout(false);
            this.tabEntities.PerformLayout();
            this.tableLayoutPanel8.ResumeLayout(false);
            this.tableLayoutPanel8.PerformLayout();
            this.tableLayoutPanel7.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.tabctrlEntData.ResumeLayout(false);
            this.tabEntObjDisplay.ResumeLayout(false);
            this.tableLayoutPanel5.ResumeLayout(false);
            this.tableLayoutPanel5.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numEntObjStartFrame)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numEntObjEndFrame)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numEntObjFrameTime)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numEntObjAlpha)).EndInit();
            this.tabEntCharDisplay.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picEntPreview)).EndInit();
            this.tabEntScript.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            this.tabEntCharData.ResumeLayout(false);
            this.tableLayoutPanel4.ResumeLayout(false);
            this.tableLayoutPanel4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numEntCharLevel)).EndInit();
            this.tabEntSpawner.ResumeLayout(false);
            this.tableLayoutPanel6.ResumeLayout(false);
            this.tableLayoutPanel6.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numEntWidth)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numEntHeight)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.tableLayoutPanel9.ResumeLayout(false);
            this.tabProperties.ResumeLayout(false);
            this.tabProperties.PerformLayout();
            this.tableLayoutPanel10.ResumeLayout(false);
            this.tableLayoutPanel10.PerformLayout();
            this.tabScript.ResumeLayout(false);
            this.flowLayoutPanel5.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.tabStrings.ResumeLayout(false);
            this.flowLayoutPanel2.ResumeLayout(false);
            this.flowLayoutPanel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gvStrings)).EndInit();
            this.flowLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel11.ResumeLayout(false);
            this.tableLayoutPanel12.ResumeLayout(false);
            this.tableLayoutPanel12.PerformLayout();
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
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.SaveFileDialog saveMapFileDialog;
        private System.Windows.Forms.TabControl tabMapOptions;
        private System.Windows.Forms.TabPage tabTextures;
        private System.Windows.Forms.TabPage tabProperties;
        private System.Windows.Forms.Button btnReloadSongs;
        private System.Windows.Forms.Label lblMusic;
        private System.Windows.Forms.Label lblMapName;
        private System.Windows.Forms.ListBox lbxMusic;
        private System.Windows.Forms.TextBox txtMapName;
        private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem resizeMapToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem retileMapToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem redoToolStripMenuItem;
        private TileBrowser tileBrowser;
        private System.Windows.Forms.TabPage tabEntities;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.ListBox lstTemplates;
        private System.Windows.Forms.ComboBox cmbTemplateType;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cmbEntityType;
        private System.Windows.Forms.TextBox txtEntityName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnAddToTemplates;
        private System.Windows.Forms.Button btnRemTemplate;
        private System.Windows.Forms.TabPage tabScript;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Button btnOpenScriptDir;
        private System.Windows.Forms.TabPage tabStrings;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.DataGridView gvStrings;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel3;
        private System.Windows.Forms.Button btnCommitStrings;
        private System.Windows.Forms.Button btnReloadStrings;
        private System.Windows.Forms.Button btnStringAdd;
        private System.Windows.Forms.Button btnStringRem;
        private System.Windows.Forms.ComboBox cmbEntityDir;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TabControl tabctrlEntData;
        private System.Windows.Forms.TabPage tabEntObjDisplay;
        private System.Windows.Forms.TabPage tabEntCharDisplay;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.PictureBox picEntPreview;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.ComboBox cmbEntKind;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.CheckBox chkEntRare;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.ComboBox cmbEntForm;
        private System.Windows.Forms.TabPage tabEntScript;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel5;
        private System.Windows.Forms.CheckedListBox chklstScriptMapCallbacks;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.ComboBox cmbEntTriggerType;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.CheckedListBox chklstEntScriptCallbacks;
        private System.Windows.Forms.TabPage tabEntCharData;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtEntCharNickname;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.NumericUpDown numEntCharLevel;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.ComboBox cmbEntCharGender;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.NumericUpDown numEntWidth;
        private System.Windows.Forms.NumericUpDown numEntHeight;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.NumericUpDown numEntObjStartFrame;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.NumericUpDown numEntObjEndFrame;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.NumericUpDown numEntObjFrameTime;
        private System.Windows.Forms.NumericUpDown numEntObjAlpha;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.Button btnMapReloadScripts;
        private System.Windows.Forms.CheckBox chkEntEnabled;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.TabPage tabEntSpawner;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel6;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.TextBox txtSpawnedEntName;
        private System.Windows.Forms.Label label21;
        private System.Windows.Forms.ComboBox cmbSpawnerType;
        private System.Windows.Forms.Label lblScriptSecondaryCallbacks;
        private System.Windows.Forms.CheckedListBox chklstScriptSecondaryCallbacks;
        private System.Windows.Forms.ToolStripMenuItem importFromPngToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importFromTilesetToolStripMenuItem;
        private System.Windows.Forms.TabPage tabBlock;
        private System.Windows.Forms.Label lblBlockMode;
        private System.Windows.Forms.TableLayoutPanel tblBlockModes;
        private System.Windows.Forms.RadioButton rbBlockDraw;
        private System.Windows.Forms.RadioButton rbBlockRectangle;
        private System.Windows.Forms.RadioButton rbBlockFill;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel7;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel8;
        private System.Windows.Forms.RadioButton rbEntSelect;
        private System.Windows.Forms.RadioButton rbEntPlace;
        private System.Windows.Forms.RadioButton rbEntMove;
        private System.Windows.Forms.Label label22;
        private System.Windows.Forms.ComboBox cbEntObjSpriteID;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel9;
        private System.Windows.Forms.Button btnLoadFromTemplate;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel10;
        private System.Windows.Forms.Label label23;
        private System.Windows.Forms.ComboBox cbScrollEdge;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel11;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel12;
        private LayerBox lbxLayers;
        private System.Windows.Forms.Label label24;
    }
}