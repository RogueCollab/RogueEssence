using System;
using System.Windows.Forms;

namespace RogueEssence.Dev
{
    public partial class ClassBox : UserControl
    {
        public object Object { get; private set; }

        public delegate void EditElementOp(object element);
        public delegate void ElementOp(object element, EditElementOp op);

        public ElementOp OnEditItem;

        public ClassBox()
        {
            InitializeComponent();
        }

        public void LoadFromSource(object source)
        {
            Object = source;
            lblName.Text = Object.ToString();
        }


        private void btnEdit_Click(object sender, EventArgs e)
        {
            object element = Object;
            OnEditItem(element, LoadFromSource);
        }
    }
}
