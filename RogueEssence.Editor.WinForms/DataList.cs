using System;
using System.Windows.Forms;

namespace RogueEssence.Dev
{
    public partial class DataList : Form
    {

        public int ChosenEntry;
        public delegate void ItemSelectedEvent();
        public ItemSelectedEvent SelectedOKEvent;
        public ItemSelectedEvent SelectedAddEvent;

        public DataList()
        {
            InitializeComponent();

            slbEntries.SetName("Select Item");

            ChosenEntry = -1;
        }

        public void AddEntries(string[] entries)
        {
            for (int ii = 0; ii < entries.Length; ii++)
            {
                slbEntries.AddItem(entries[ii]);
            }
        }

        public void ModifyEntry(int index, string entry)
        {
            slbEntries.SetInternalEntry(index, entry);
        }

        public void AddEntry(string entry)
        {
            slbEntries.AddItem(entry);
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            SelectedAddEvent();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {

        }

        private void slbEntries_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            int index = slbEntries.IndexFromPoint(e.Location);

            if (index != ListBox.NoMatches)
            {
                ChosenEntry = slbEntries.GetInternalIndex(index);
                SelectedOKEvent();
            }
        }
    }
}
