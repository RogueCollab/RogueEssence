using System.Windows.Forms;

namespace RogueEssence.Dev
{
    public class SelectNoneListBox : ListBox
    {
        public bool SelectNone { get; set; }



        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (SelectNone)
            {
                if (e.Button == System.Windows.Forms.MouseButtons.Right)
                {
                    int index = IndexFromPoint(e.X, e.Y);
                    if (index == SelectedIndex)
                        SelectedIndex = -1;
                }
                else if (e.Button == System.Windows.Forms.MouseButtons.Left)
                {
                    int index = IndexFromPoint(e.X, e.Y);
                    if (index == -1)
                        SelectedIndex = -1;
                } 
            }
            base.OnMouseDown(e);
        }

    }
}
