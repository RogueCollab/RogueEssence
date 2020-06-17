using System;
using System.Drawing;
using System.Windows.Forms;

namespace RogueEssence.Dev
{
    public partial class ElementForm : Form
    {

        public TableLayoutPanel ControlPanel
        {
            get { return panel; }
        }

        public event EventHandler OnOK
        {
            add { btnOK.Click += value; }
            remove { btnOK.Click -= value; }
        }

        public event EventHandler OnCancel
        {
            add { btnCancel.Click += value; }
            remove { btnCancel.Click -= value; }
        }

        public ElementForm()
        {
            InitializeComponent();

            Screen myScreen = Screen.FromControl(this);
            ControlPanel.MaximumSize = new Size(0, myScreen.WorkingArea.Height - 200);
        }
    }
}
