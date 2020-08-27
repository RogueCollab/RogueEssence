namespace RogueEssence.Dev
{
    partial class ElementForm
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
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.panel = new System.Windows.Forms.TableLayoutPanel();
            this.outerPanel = new System.Windows.Forms.TableLayoutPanel();
            this.bottomPanel = new System.Windows.Forms.TableLayoutPanel();
            this.copyPasteStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pasteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.outerPanel.SuspendLayout();
            this.bottomPanel.SuspendLayout();
            this.copyPasteStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnOK
            // 
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnOK.Location = new System.Drawing.Point(4, 5);
            this.btnOK.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(201, 38);
            this.btnOK.TabIndex = 4;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnCancel.Location = new System.Drawing.Point(213, 5);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(201, 38);
            this.btnCancel.TabIndex = 5;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // panel
            // 
            this.panel.AutoScroll = true;
            this.panel.AutoSize = true;
            this.panel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panel.ColumnCount = 1;
            this.panel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.panel.ContextMenuStrip = this.copyPasteStrip;
            this.panel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel.Location = new System.Drawing.Point(0, 0);
            this.panel.Margin = new System.Windows.Forms.Padding(0);
            this.panel.MinimumSize = new System.Drawing.Size(450, 0);
            this.panel.Name = "panel";
            this.panel.RowCount = 1;
            this.panel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.panel.Size = new System.Drawing.Size(450, 267);
            this.panel.TabIndex = 3;
            // 
            // outerPanel
            // 
            this.outerPanel.AutoSize = true;
            this.outerPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.outerPanel.ColumnCount = 1;
            this.outerPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.outerPanel.Controls.Add(this.bottomPanel, 0, 1);
            this.outerPanel.Controls.Add(this.panel, 0, 0);
            this.outerPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.outerPanel.Location = new System.Drawing.Point(0, 0);
            this.outerPanel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.outerPanel.Name = "outerPanel";
            this.outerPanel.RowCount = 2;
            this.outerPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.outerPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 58F));
            this.outerPanel.Size = new System.Drawing.Size(426, 325);
            this.outerPanel.TabIndex = 6;
            // 
            // bottomPanel
            // 
            this.bottomPanel.ColumnCount = 2;
            this.bottomPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.bottomPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.bottomPanel.Controls.Add(this.btnOK, 0, 0);
            this.bottomPanel.Controls.Add(this.btnCancel, 1, 0);
            this.bottomPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bottomPanel.Location = new System.Drawing.Point(4, 272);
            this.bottomPanel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.bottomPanel.Name = "bottomPanel";
            this.bottomPanel.RowCount = 1;
            this.bottomPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.bottomPanel.Size = new System.Drawing.Size(418, 48);
            this.bottomPanel.TabIndex = 0;
            // 
            // copyPasteStrip
            // 
            this.copyPasteStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.copyPasteStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.copyToolStripMenuItem,
            this.pasteToolStripMenuItem});
            this.copyPasteStrip.Name = "copyPasteStrip";
            this.copyPasteStrip.Size = new System.Drawing.Size(241, 101);
            // 
            // copyToolStripMenuItem
            // 
            this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            this.copyToolStripMenuItem.Size = new System.Drawing.Size(240, 32);
            this.copyToolStripMenuItem.Text = "Copy";
            // 
            // pasteToolStripMenuItem
            // 
            this.pasteToolStripMenuItem.Name = "pasteToolStripMenuItem";
            this.pasteToolStripMenuItem.Size = new System.Drawing.Size(240, 32);
            this.pasteToolStripMenuItem.Text = "Paste";
            // 
            // ElementForm
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(426, 325);
            this.Controls.Add(this.outerPanel);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ElementForm";
            this.Text = "ElementForm";
            this.outerPanel.ResumeLayout(false);
            this.outerPanel.PerformLayout();
            this.bottomPanel.ResumeLayout(false);
            this.copyPasteStrip.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.TableLayoutPanel panel;
        private System.Windows.Forms.TableLayoutPanel outerPanel;
        private System.Windows.Forms.TableLayoutPanel bottomPanel;
        private System.Windows.Forms.ContextMenuStrip copyPasteStrip;
        private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pasteToolStripMenuItem;
    }
}