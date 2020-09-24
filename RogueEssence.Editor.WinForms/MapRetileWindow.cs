using System;
using System.Windows.Forms;

namespace RogueEssence.Dev
{
    public partial class MapRetileWindow : Form
    {
        public int TileSize { get; set; }

        public MapRetileWindow(int tileSize)
        {
            InitializeComponent();

            TileSize = tileSize;
            nudSize.Value = tileSize;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            TileSize = (int)nudSize.Value;
            if (TileSize % 8 != 0)
            {
                MessageBox.Show(this, "Choose a size that is divisible by 8.", "Size must be divisible by 8!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            this.DialogResult = DialogResult.OK;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

    }
}
