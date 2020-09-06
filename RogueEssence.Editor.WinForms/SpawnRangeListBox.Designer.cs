namespace RogueEssence.Dev
{
    partial class SpawnRangeListBox
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
            this.lowerPanel = new System.Windows.Forms.TableLayoutPanel();
            this.upperPanel = new System.Windows.Forms.TableLayoutPanel();
            this.spawnRateTrackBar = new System.Windows.Forms.TrackBar();
            this.lblWeight = new System.Windows.Forms.Label();
            this.bottomPanel = new System.Windows.Forms.TableLayoutPanel();
            this.btnAdd = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.lblTo = new System.Windows.Forms.Label();
            this.nudStart = new RogueEssence.Dev.IntNumericUpDown();
            this.nudEnd = new RogueEssence.Dev.IntNumericUpDown();
            this.lbxCollection = new RogueEssence.Dev.SelectNoneListBox();
            this.sidePanel.SuspendLayout();
            this.lowerPanel.SuspendLayout();
            this.upperPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.spawnRateTrackBar)).BeginInit();
            this.bottomPanel.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudStart)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudEnd)).BeginInit();
            this.SuspendLayout();
            // 
            // btnUp
            // 
            this.btnUp.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnUp.Location = new System.Drawing.Point(3, 3);
            this.btnUp.Name = "btnUp";
            this.btnUp.Size = new System.Drawing.Size(19, 67);
            this.btnUp.TabIndex = 6;
            this.btnUp.Text = "^";
            this.btnUp.UseVisualStyleBackColor = true;
            this.btnUp.Click += new System.EventHandler(this.btnUp_Click);
            // 
            // btnDown
            // 
            this.btnDown.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnDown.Location = new System.Drawing.Point(3, 76);
            this.btnDown.Name = "btnDown";
            this.btnDown.Size = new System.Drawing.Size(19, 68);
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
            this.sidePanel.Location = new System.Drawing.Point(125, 0);
            this.sidePanel.Name = "sidePanel";
            this.sidePanel.RowCount = 2;
            this.sidePanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.sidePanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.sidePanel.Size = new System.Drawing.Size(25, 147);
            this.sidePanel.TabIndex = 8;
            // 
            // lowerPanel
            // 
            this.lowerPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lowerPanel.ColumnCount = 1;
            this.lowerPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.lowerPanel.Controls.Add(this.upperPanel, 0, 0);
            this.lowerPanel.Controls.Add(this.bottomPanel, 0, 2);
            this.lowerPanel.Controls.Add(this.tableLayoutPanel1, 0, 1);
            this.lowerPanel.Location = new System.Drawing.Point(0, 150);
            this.lowerPanel.Margin = new System.Windows.Forms.Padding(0);
            this.lowerPanel.Name = "lowerPanel";
            this.lowerPanel.RowCount = 3;
            this.lowerPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.lowerPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.lowerPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.lowerPanel.Size = new System.Drawing.Size(151, 85);
            this.lowerPanel.TabIndex = 9;
            // 
            // upperPanel
            // 
            this.upperPanel.ColumnCount = 2;
            this.upperPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.upperPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 47F));
            this.upperPanel.Controls.Add(this.spawnRateTrackBar, 0, 0);
            this.upperPanel.Controls.Add(this.lblWeight, 1, 0);
            this.upperPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.upperPanel.Location = new System.Drawing.Point(0, 0);
            this.upperPanel.Margin = new System.Windows.Forms.Padding(0);
            this.upperPanel.Name = "upperPanel";
            this.upperPanel.RowCount = 1;
            this.upperPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.upperPanel.Size = new System.Drawing.Size(151, 28);
            this.upperPanel.TabIndex = 10;
            // 
            // spawnRateTrackBar
            // 
            this.spawnRateTrackBar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.spawnRateTrackBar.Enabled = false;
            this.spawnRateTrackBar.LargeChange = 8;
            this.spawnRateTrackBar.Location = new System.Drawing.Point(2, 2);
            this.spawnRateTrackBar.Margin = new System.Windows.Forms.Padding(2);
            this.spawnRateTrackBar.Maximum = 255;
            this.spawnRateTrackBar.Minimum = 1;
            this.spawnRateTrackBar.Name = "spawnRateTrackBar";
            this.spawnRateTrackBar.Size = new System.Drawing.Size(100, 24);
            this.spawnRateTrackBar.TabIndex = 6;
            this.spawnRateTrackBar.TickFrequency = 20;
            this.spawnRateTrackBar.Value = 1;
            this.spawnRateTrackBar.Scroll += new System.EventHandler(this.spawnRateTrackBar_Scroll);
            this.spawnRateTrackBar.MouseCaptureChanged += new System.EventHandler(this.spawnRateTrackBar_MouseCaptureChanged);
            this.spawnRateTrackBar.MouseDown += new System.Windows.Forms.MouseEventHandler(this.spawnRateTrackBar_MouseDown);
            // 
            // lblWeight
            // 
            this.lblWeight.AutoSize = true;
            this.lblWeight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblWeight.Location = new System.Drawing.Point(104, 0);
            this.lblWeight.Margin = new System.Windows.Forms.Padding(0);
            this.lblWeight.Name = "lblWeight";
            this.lblWeight.Size = new System.Drawing.Size(47, 28);
            this.lblWeight.TabIndex = 7;
            this.lblWeight.Text = "Weight:\r\n---";
            this.lblWeight.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // bottomPanel
            // 
            this.bottomPanel.ColumnCount = 2;
            this.bottomPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.bottomPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.bottomPanel.Controls.Add(this.btnAdd, 0, 0);
            this.bottomPanel.Controls.Add(this.btnDelete, 1, 0);
            this.bottomPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bottomPanel.Location = new System.Drawing.Point(0, 56);
            this.bottomPanel.Margin = new System.Windows.Forms.Padding(0);
            this.bottomPanel.Name = "bottomPanel";
            this.bottomPanel.RowCount = 1;
            this.bottomPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.bottomPanel.Size = new System.Drawing.Size(151, 29);
            this.bottomPanel.TabIndex = 5;
            // 
            // btnAdd
            // 
            this.btnAdd.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnAdd.Location = new System.Drawing.Point(3, 3);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(69, 23);
            this.btnAdd.TabIndex = 1;
            this.btnAdd.Text = "Add";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // btnDelete
            // 
            this.btnDelete.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnDelete.Location = new System.Drawing.Point(78, 3);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(70, 23);
            this.btnDelete.TabIndex = 2;
            this.btnDelete.Text = "Delete";
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.lblTo, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.nudStart, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.nudEnd, 2, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 31);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(145, 22);
            this.tableLayoutPanel1.TabIndex = 11;
            // 
            // lblTo
            // 
            this.lblTo.AutoSize = true;
            this.lblTo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTo.Location = new System.Drawing.Point(62, 0);
            this.lblTo.Margin = new System.Windows.Forms.Padding(0);
            this.lblTo.Name = "lblTo";
            this.lblTo.Size = new System.Drawing.Size(20, 22);
            this.lblTo.TabIndex = 0;
            this.lblTo.Text = "to";
            this.lblTo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // nudStart
            // 
            this.nudStart.Dock = System.Windows.Forms.DockStyle.Fill;
            this.nudStart.Enabled = false;
            this.nudStart.Location = new System.Drawing.Point(0, 0);
            this.nudStart.Margin = new System.Windows.Forms.Padding(0);
            this.nudStart.Name = "nudStart";
            this.nudStart.Size = new System.Drawing.Size(62, 20);
            this.nudStart.TabIndex = 1;
            this.nudStart.ValueChanged += new System.EventHandler(this.nudStart_ValueChanged);
            // 
            // nudEnd
            // 
            this.nudEnd.Dock = System.Windows.Forms.DockStyle.Fill;
            this.nudEnd.Enabled = false;
            this.nudEnd.Location = new System.Drawing.Point(82, 0);
            this.nudEnd.Margin = new System.Windows.Forms.Padding(0);
            this.nudEnd.Name = "nudEnd";
            this.nudEnd.Size = new System.Drawing.Size(63, 20);
            this.nudEnd.TabIndex = 2;
            this.nudEnd.ValueChanged += new System.EventHandler(this.nudEnd_ValueChanged);
            // 
            // lbxCollection
            // 
            this.lbxCollection.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbxCollection.FormattingEnabled = true;
            this.lbxCollection.Location = new System.Drawing.Point(0, 0);
            this.lbxCollection.Name = "lbxCollection";
            this.lbxCollection.SelectNone = true;
            this.lbxCollection.Size = new System.Drawing.Size(125, 147);
            this.lbxCollection.TabIndex = 4;
            this.lbxCollection.SelectedIndexChanged += new System.EventHandler(this.lbxCollection_SelectedIndexChanged);
            this.lbxCollection.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lbxCollection_MouseDoubleClick);
            // 
            // SpawnListBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lowerPanel);
            this.Controls.Add(this.sidePanel);
            this.Controls.Add(this.lbxCollection);
            this.Name = "SpawnListBox";
            this.Size = new System.Drawing.Size(150, 235);
            this.sidePanel.ResumeLayout(false);
            this.lowerPanel.ResumeLayout(false);
            this.upperPanel.ResumeLayout(false);
            this.upperPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.spawnRateTrackBar)).EndInit();
            this.bottomPanel.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudStart)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudEnd)).EndInit();
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
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label lblTo;
        private IntNumericUpDown nudStart;
        private IntNumericUpDown nudEnd;
    }
}
