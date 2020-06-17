namespace RogueEssence.Dev
{
    partial class MapResizeWindow
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MapResizeWindow));
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.nudWidth = new RogueEssence.Dev.IntNumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.nudHeight = new RogueEssence.Dev.IntNumericUpDown();
            this.btnTopLeft = new System.Windows.Forms.Button();
            this.btnTopRight = new System.Windows.Forms.Button();
            this.btnRight = new System.Windows.Forms.Button();
            this.btnLeft = new System.Windows.Forms.Button();
            this.btnTop = new System.Windows.Forms.Button();
            this.btnCenter = new System.Windows.Forms.Button();
            this.btnBottomLeft = new System.Windows.Forms.Button();
            this.btnBottomRight = new System.Windows.Forms.Button();
            this.btnBottom = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.nudWidth)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudHeight)).BeginInit();
            this.SuspendLayout();
            // 
            // btnOK
            // 
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(12, 308);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 0;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(93, 308);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Width";
            // 
            // nudWidth
            // 
            this.nudWidth.Location = new System.Drawing.Point(12, 24);
            this.nudWidth.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.nudWidth.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudWidth.Name = "nudWidth";
            this.nudWidth.Size = new System.Drawing.Size(156, 20);
            this.nudWidth.TabIndex = 3;
            this.nudWidth.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 54);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(38, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Height";
            // 
            // nudHeight
            // 
            this.nudHeight.Location = new System.Drawing.Point(12, 70);
            this.nudHeight.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.nudHeight.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudHeight.Name = "nudHeight";
            this.nudHeight.Size = new System.Drawing.Size(156, 20);
            this.nudHeight.TabIndex = 5;
            this.nudHeight.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // btnTopLeft
            // 
            this.btnTopLeft.Location = new System.Drawing.Point(12, 124);
            this.btnTopLeft.Name = "btnTopLeft";
            this.btnTopLeft.Size = new System.Drawing.Size(32, 32);
            this.btnTopLeft.TabIndex = 6;
            this.btnTopLeft.UseVisualStyleBackColor = true;
            this.btnTopLeft.Click += new System.EventHandler(this.btnTopLeft_Click);
            // 
            // btnTopRight
            // 
            this.btnTopRight.Location = new System.Drawing.Point(136, 124);
            this.btnTopRight.Name = "btnTopRight";
            this.btnTopRight.Size = new System.Drawing.Size(32, 32);
            this.btnTopRight.TabIndex = 7;
            this.btnTopRight.UseVisualStyleBackColor = true;
            this.btnTopRight.Click += new System.EventHandler(this.btnTopRight_Click);
            // 
            // btnRight
            // 
            this.btnRight.Location = new System.Drawing.Point(136, 162);
            this.btnRight.Name = "btnRight";
            this.btnRight.Size = new System.Drawing.Size(32, 80);
            this.btnRight.TabIndex = 9;
            this.btnRight.UseVisualStyleBackColor = true;
            this.btnRight.Click += new System.EventHandler(this.btnRight_Click);
            // 
            // btnLeft
            // 
            this.btnLeft.Location = new System.Drawing.Point(12, 162);
            this.btnLeft.Name = "btnLeft";
            this.btnLeft.Size = new System.Drawing.Size(32, 80);
            this.btnLeft.TabIndex = 8;
            this.btnLeft.UseVisualStyleBackColor = true;
            this.btnLeft.Click += new System.EventHandler(this.btnLeft_Click);
            // 
            // btnTop
            // 
            this.btnTop.Location = new System.Drawing.Point(50, 124);
            this.btnTop.Name = "btnTop";
            this.btnTop.Size = new System.Drawing.Size(80, 32);
            this.btnTop.TabIndex = 10;
            this.btnTop.UseVisualStyleBackColor = true;
            this.btnTop.Click += new System.EventHandler(this.btnTop_Click);
            // 
            // btnCenter
            // 
            this.btnCenter.Location = new System.Drawing.Point(50, 162);
            this.btnCenter.Name = "btnCenter";
            this.btnCenter.Size = new System.Drawing.Size(80, 80);
            this.btnCenter.TabIndex = 11;
            this.btnCenter.UseVisualStyleBackColor = true;
            this.btnCenter.Click += new System.EventHandler(this.btnCenter_Click);
            // 
            // btnBottomLeft
            // 
            this.btnBottomLeft.Location = new System.Drawing.Point(12, 248);
            this.btnBottomLeft.Name = "btnBottomLeft";
            this.btnBottomLeft.Size = new System.Drawing.Size(32, 32);
            this.btnBottomLeft.TabIndex = 12;
            this.btnBottomLeft.UseVisualStyleBackColor = true;
            this.btnBottomLeft.Click += new System.EventHandler(this.btnBottomLeft_Click);
            // 
            // btnBottomRight
            // 
            this.btnBottomRight.Location = new System.Drawing.Point(136, 248);
            this.btnBottomRight.Name = "btnBottomRight";
            this.btnBottomRight.Size = new System.Drawing.Size(32, 32);
            this.btnBottomRight.TabIndex = 13;
            this.btnBottomRight.UseVisualStyleBackColor = true;
            this.btnBottomRight.Click += new System.EventHandler(this.btnBottomRight_Click);
            // 
            // btnBottom
            // 
            this.btnBottom.Location = new System.Drawing.Point(50, 248);
            this.btnBottom.Name = "btnBottom";
            this.btnBottom.Size = new System.Drawing.Size(80, 32);
            this.btnBottom.TabIndex = 14;
            this.btnBottom.UseVisualStyleBackColor = true;
            this.btnBottom.Click += new System.EventHandler(this.btnBottom_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 108);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(41, 13);
            this.label3.TabIndex = 15;
            this.label3.Text = "Anchor";
            // 
            // MapResizeWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(180, 343);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.btnBottom);
            this.Controls.Add(this.btnBottomRight);
            this.Controls.Add(this.btnBottomLeft);
            this.Controls.Add(this.btnCenter);
            this.Controls.Add(this.btnTop);
            this.Controls.Add(this.btnRight);
            this.Controls.Add(this.btnLeft);
            this.Controls.Add(this.btnTopRight);
            this.Controls.Add(this.btnTopLeft);
            this.Controls.Add(this.nudHeight);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.nudWidth);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MapResizeWindow";
            this.Text = "Resize Map";
            ((System.ComponentModel.ISupportInitialize)(this.nudWidth)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudHeight)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label label1;
        private IntNumericUpDown nudWidth;
        private System.Windows.Forms.Label label2;
        private IntNumericUpDown nudHeight;
        private System.Windows.Forms.Button btnTopLeft;
        private System.Windows.Forms.Button btnTopRight;
        private System.Windows.Forms.Button btnRight;
        private System.Windows.Forms.Button btnLeft;
        private System.Windows.Forms.Button btnTop;
        private System.Windows.Forms.Button btnCenter;
        private System.Windows.Forms.Button btnBottomLeft;
        private System.Windows.Forms.Button btnBottomRight;
        private System.Windows.Forms.Button btnBottom;
        private System.Windows.Forms.Label label3;
    }
}