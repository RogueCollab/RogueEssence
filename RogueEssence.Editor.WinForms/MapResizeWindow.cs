using System;
using RogueElements;
using System.Windows.Forms;

namespace RogueEssence.Dev
{
    public partial class MapResizeWindow : Form
    {
        public int MapWidth { get; set; }
        public int MapHeight { get; set; }

        public Dir8 ResizeDir { get; set; }

        public MapResizeWindow(int width, int height)
        {
            InitializeComponent();

            MapWidth = width;
            MapHeight = height;

            nudWidth.Value = width;
            nudHeight.Value = height;

            ResizeDir = Dir8.None;

            RefreshResizeDir();
        }

        void RefreshResizeDir()
        {
            
            btnCenter.Text = "";
            btnBottom.Text = "";
            btnLeft.Text = "";
            btnTop.Text = "";
            btnRight.Text = "";
            btnBottomLeft.Text = "";
            btnTopLeft.Text = "";
            btnTopRight.Text = "";
            btnBottomRight.Text = "";

            switch (ResizeDir)
            {
                case Dir8.None:
                    btnCenter.Text = "X";
                    break;
                case Dir8.Down:
                    btnBottom.Text = "X";
                    break;
                case Dir8.Left:
                    btnLeft.Text = "X";
                    break;
                case Dir8.Up:
                    btnTop.Text = "X";
                    break;
                case Dir8.Right:
                    btnRight.Text = "X";
                    break;
                case Dir8.DownLeft:
                    btnBottomLeft.Text = "X";
                    break;
                case Dir8.UpLeft:
                    btnTopLeft.Text = "X";
                    break;
                case Dir8.UpRight:
                    btnTopRight.Text = "X";
                    break;
                case Dir8.DownRight:
                    btnBottomRight.Text = "X";
                    break;
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            MapWidth = (int)nudWidth.Value;
            MapHeight = (int)nudHeight.Value;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnTopLeft_Click(object sender, EventArgs e)
        {
            ResizeDir = Dir8.UpLeft;
            RefreshResizeDir();
        }

        private void btnTop_Click(object sender, EventArgs e)
        {
            ResizeDir = Dir8.Up;
            RefreshResizeDir();
        }

        private void btnTopRight_Click(object sender, EventArgs e)
        {
            ResizeDir = Dir8.UpRight;
            RefreshResizeDir();
        }

        private void btnLeft_Click(object sender, EventArgs e)
        {
            ResizeDir = Dir8.Left;
            RefreshResizeDir();
        }

        private void btnCenter_Click(object sender, EventArgs e)
        {
            ResizeDir = Dir8.None;
            RefreshResizeDir();
        }

        private void btnRight_Click(object sender, EventArgs e)
        {
            ResizeDir = Dir8.Right;
            RefreshResizeDir();
        }

        private void btnBottomLeft_Click(object sender, EventArgs e)
        {
            ResizeDir = Dir8.DownLeft;
            RefreshResizeDir();
        }

        private void btnBottom_Click(object sender, EventArgs e)
        {
            ResizeDir = Dir8.Down;
            RefreshResizeDir();
        }

        private void btnBottomRight_Click(object sender, EventArgs e)
        {
            ResizeDir = Dir8.DownRight;
            RefreshResizeDir();
        }
    }
}
