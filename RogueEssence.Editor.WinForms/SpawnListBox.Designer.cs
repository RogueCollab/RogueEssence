namespace RogueEssence.Dev
{
    partial class SpawnListBox
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
            this.btnUp = new System.Windows.Forms.Button();
            this.btnDown = new System.Windows.Forms.Button();
            this.sidePanel = new System.Windows.Forms.TableLayoutPanel();
            this.lbxCollection = new RogueEssence.Dev.SelectNoneListBox();
            this.lowerPanel = new System.Windows.Forms.TableLayoutPanel();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnAdd = new System.Windows.Forms.Button();
            this.bottomPanel = new System.Windows.Forms.TableLayoutPanel();
            this.spawnRateTrackBar = new System.Windows.Forms.TrackBar();
            this.upperPanel = new System.Windows.Forms.TableLayoutPanel();
            this.lblWeight = new System.Windows.Forms.Label();
            this.sidePanel.SuspendLayout();
            this.lowerPanel.SuspendLayout();
            this.bottomPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.spawnRateTrackBar)).BeginInit();
            this.upperPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnUp
            // 
            this.btnUp.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnUp.Location = new System.Drawing.Point(4, 5);
            this.btnUp.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnUp.Name = "btnUp";
            this.btnUp.Size = new System.Drawing.Size(30, 82);
            this.btnUp.TabIndex = 6;
            this.btnUp.Text = "^";
            this.btnUp.UseVisualStyleBackColor = true;
            this.btnUp.Click += new System.EventHandler(this.btnUp_Click);
            // 
            // btnDown
            // 
            this.btnDown.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnDown.Location = new System.Drawing.Point(4, 97);
            this.btnDown.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnDown.Name = "btnDown";
            this.btnDown.Size = new System.Drawing.Size(30, 82);
            this.btnDown.TabIndex = 7;
            this.btnDown.Text = "v";
            this.btnDown.UseVisualStyleBackColor = true;
            this.btnDown.Click += new System.EventHandler(this.btnDown_Click);
            // 
            // sidePanel
            // 
            this.sidePanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.sidePanel.ColumnCount = 1;
            this.sidePanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.sidePanel.Controls.Add(this.btnUp, 0, 0);
            this.sidePanel.Controls.Add(this.btnDown, 0, 1);
            this.sidePanel.Location = new System.Drawing.Point(188, 0);
            this.sidePanel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.sidePanel.Name = "sidePanel";
            this.sidePanel.RowCount = 2;
            this.sidePanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.sidePanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.sidePanel.Size = new System.Drawing.Size(38, 184);
            this.sidePanel.TabIndex = 8;
            // 
            // lbxCollection
            // 
            this.lbxCollection.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbxCollection.FormattingEnabled = true;
            this.lbxCollection.ItemHeight = 20;
            this.lbxCollection.Location = new System.Drawing.Point(0, 0);
            this.lbxCollection.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.lbxCollection.Name = "lbxCollection";
            this.lbxCollection.SelectNone = true;
            this.lbxCollection.Size = new System.Drawing.Size(186, 184);
            this.lbxCollection.TabIndex = 4;
            this.lbxCollection.SelectedIndexChanged += new System.EventHandler(this.lbxCollection_SelectedIndexChanged);
            this.lbxCollection.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lbxCollection_MouseDoubleClick);
            // 
            // lowerPanel
            // 
            this.lowerPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lowerPanel.ColumnCount = 1;
            this.lowerPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.lowerPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.lowerPanel.Controls.Add(this.upperPanel, 0, 0);
            this.lowerPanel.Controls.Add(this.bottomPanel, 0, 1);
            this.lowerPanel.Location = new System.Drawing.Point(0, 184);
            this.lowerPanel.Margin = new System.Windows.Forms.Padding(0);
            this.lowerPanel.Name = "lowerPanel";
            this.lowerPanel.RowCount = 2;
            this.lowerPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.lowerPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.lowerPanel.Size = new System.Drawing.Size(226, 90);
            this.lowerPanel.TabIndex = 9;
            // 
            // btnDelete
            // 
            this.btnDelete.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnDelete.Location = new System.Drawing.Point(117, 5);
            this.btnDelete.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(105, 35);
            this.btnDelete.TabIndex = 2;
            this.btnDelete.Text = "Delete";
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // btnAdd
            // 
            this.btnAdd.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnAdd.Location = new System.Drawing.Point(4, 5);
            this.btnAdd.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(105, 35);
            this.btnAdd.TabIndex = 1;
            this.btnAdd.Text = "Add";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // bottomPanel
            // 
            this.bottomPanel.ColumnCount = 2;
            this.bottomPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.bottomPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.bottomPanel.Controls.Add(this.btnAdd, 0, 0);
            this.bottomPanel.Controls.Add(this.btnDelete, 1, 0);
            this.bottomPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bottomPanel.Location = new System.Drawing.Point(0, 45);
            this.bottomPanel.Margin = new System.Windows.Forms.Padding(0);
            this.bottomPanel.Name = "bottomPanel";
            this.bottomPanel.RowCount = 1;
            this.bottomPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.bottomPanel.Size = new System.Drawing.Size(226, 45);
            this.bottomPanel.TabIndex = 5;
            // 
            // spawnRateTrackBar
            // 
            this.spawnRateTrackBar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.spawnRateTrackBar.Enabled = false;
            this.spawnRateTrackBar.LargeChange = 8;
            this.spawnRateTrackBar.Location = new System.Drawing.Point(3, 3);
            this.spawnRateTrackBar.Maximum = 255;
            this.spawnRateTrackBar.Minimum = 1;
            this.spawnRateTrackBar.Name = "spawnRateTrackBar";
            this.spawnRateTrackBar.Size = new System.Drawing.Size(150, 39);
            this.spawnRateTrackBar.TabIndex = 6;
            this.spawnRateTrackBar.TickFrequency = 20;
            this.spawnRateTrackBar.Value = 1;
            this.spawnRateTrackBar.Scroll += new System.EventHandler(this.spawnRateTrackBar_Scroll);
            this.spawnRateTrackBar.MouseCaptureChanged += new System.EventHandler(this.spawnRateTrackBar_MouseCaptureChanged);
            this.spawnRateTrackBar.MouseDown += new System.Windows.Forms.MouseEventHandler(this.spawnRateTrackBar_MouseDown);
            // 
            // upperPanel
            // 
            this.upperPanel.ColumnCount = 2;
            this.upperPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.upperPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 70F));
            this.upperPanel.Controls.Add(this.spawnRateTrackBar, 0, 0);
            this.upperPanel.Controls.Add(this.lblWeight, 1, 0);
            this.upperPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.upperPanel.Location = new System.Drawing.Point(0, 0);
            this.upperPanel.Margin = new System.Windows.Forms.Padding(0);
            this.upperPanel.Name = "upperPanel";
            this.upperPanel.RowCount = 1;
            this.upperPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.upperPanel.Size = new System.Drawing.Size(226, 45);
            this.upperPanel.TabIndex = 10;
            // 
            // lblWeight
            // 
            this.lblWeight.AutoSize = true;
            this.lblWeight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblWeight.Location = new System.Drawing.Point(156, 0);
            this.lblWeight.Margin = new System.Windows.Forms.Padding(0);
            this.lblWeight.Name = "lblWeight";
            this.lblWeight.Size = new System.Drawing.Size(70, 45);
            this.lblWeight.TabIndex = 7;
            this.lblWeight.Text = "Weight:\r\n---";
            this.lblWeight.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // SpawnListBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lowerPanel);
            this.Controls.Add(this.sidePanel);
            this.Controls.Add(this.lbxCollection);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "SpawnListBox";
            this.Size = new System.Drawing.Size(225, 274);
            this.sidePanel.ResumeLayout(false);
            this.lowerPanel.ResumeLayout(false);
            this.bottomPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spawnRateTrackBar)).EndInit();
            this.upperPanel.ResumeLayout(false);
            this.upperPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private SelectNoneListBox lbxCollection;
        private System.Windows.Forms.Button btnUp;
        private System.Windows.Forms.Button btnDown;
        private System.Windows.Forms.TableLayoutPanel sidePanel;
        private System.Windows.Forms.TableLayoutPanel lowerPanel;
        private System.Windows.Forms.TableLayoutPanel bottomPanel;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.TrackBar spawnRateTrackBar;
        private System.Windows.Forms.TableLayoutPanel upperPanel;
        private System.Windows.Forms.Label lblWeight;
    }
}
