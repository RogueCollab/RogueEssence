using System;
using System.Windows.Forms;

namespace RogueEssence.Dev
{
    public partial class MapLayerWindow : Form
    {
        public string LayerName { get; set; }
        public bool Front { get; set; }

        public MapLayerWindow(string name, bool front)
        {
            InitializeComponent();

            LayerName = name;
            Front = front;

            txtName.Text = LayerName;
            chkFront.Checked = Front;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            LayerName = txtName.Text;
            Front = chkFront.Checked;
        }

    }
}
