using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;


namespace RogueEssence.Dev
{
    public partial class SearchListBox : UserControl
    {
        private List<string> entries;
        private List<int> entryMap;

        public bool SelectNone
        {
            get { return lbxItems.SelectNone; }
            set { lbxItems.SelectNone = value; }
        }

        public int SelectedIndex
        {
            get { return lbxItems.SelectedIndex; }
            set { lbxItems.SelectedIndex = value; }
        }

        public int InternalIndex
        {
            get { return GetInternalIndex(lbxItems.SelectedIndex); }
        }

        public string SearchText
        {
            get { return txtSearch.Text; }
            set { txtSearch.Text = value; }
        }

        public int Count { get { return lbxItems.Items.Count; } }

        public int InternalCount { get { return entries.Count; } }

        //selected index changed
        public event EventHandler SelectedIndexChanged
        {
            add { lbxItems.SelectedIndexChanged += value; }
            remove { lbxItems.SelectedIndexChanged -= value; }
        }

        //right clicked
        public event MouseEventHandler ListBoxMouseDoubleClick
        {
            add { lbxItems.MouseDoubleClick += value; }
            remove { lbxItems.MouseDoubleClick -= value; }
        }

        public SearchListBox()
        {
            InitializeComponent();

            entries = new List<string>();
            entryMap = new List<int>();

            RefreshFilter();
        }

        public void SetName(string name)
        {
            lblName.Text = name + ":";
        }

        public void AddItem(string item)
        {
            entries.Add(item);

            if (txtSearch.Text == "" || entries[entries.Count-1].IndexOf(txtSearch.Text, StringComparison.CurrentCultureIgnoreCase) > -1)
            {
                lbxItems.Items.Add((entries.Count - 1) + ": " + entries[entries.Count - 1]);
                entryMap.Add(entries.Count - 1);
            }
        }
        public void Clear()
        {
            entries.Clear();

            lbxItems.Items.Clear();
            entryMap.Clear();
            txtSearch.Text = "";
        }

        public void RemoveAt(int index)
        {
            entries.RemoveAt(entryMap[index]);

            lbxItems.Items.RemoveAt(index);
            entryMap.RemoveAt(index);
        }

        public string GetItem(int index)
        {
            return entries[entryMap[index]];
        }

        public void SetItem(int index, string entry)
        {
            entries[entryMap[index]] = entry;

            if (txtSearch.Text == "" || entries[entryMap[index]].IndexOf(txtSearch.Text, StringComparison.CurrentCultureIgnoreCase) > -1)
            {
                lbxItems.Items[index] = entryMap[index] + ": " + entry;
            }
            else
            {
                lbxItems.Items.RemoveAt(index);
                entryMap.RemoveAt(index);
            }
        }

        public int GetInternalIndex(int index)
        {
            return entryMap[index];
        }

        public string GetInternalEntry(int index)
        {
            return entries[index];
        }

        public void SetInternalEntry(int index, string entry)
        {
            bool oldAppears = (txtSearch.Text == "" || entries[index].IndexOf(txtSearch.Text, StringComparison.CurrentCultureIgnoreCase) > -1);
            bool newAppears = (txtSearch.Text == "" || entry.IndexOf(txtSearch.Text, StringComparison.CurrentCultureIgnoreCase) > -1);
            entries[index] = entry;

            int shownIndex = entryMap.IndexOf(index);
            
            if (oldAppears && newAppears)
            {
                //change
                lbxItems.Items[shownIndex] = index + ": " + entry;
            }
            else if (oldAppears)
            {
                //remove
                lbxItems.Items.RemoveAt(shownIndex);
                entryMap.RemoveAt(shownIndex);
            }
            else if (newAppears)
            {
                //add
                for (int ii = 0; ii < entryMap.Count; ii++)
                {
                    if (entryMap[ii] < index)
                    {
                        lbxItems.Items.Insert(ii, index + ": " + entry);
                        entryMap.Insert(ii, index);
                        break;
                    }
                }
            }
        }

        private void RefreshFilter()
        {
            int internalIndex = -1;
            if (SelectedIndex > -1)
                internalIndex = InternalIndex;
            lbxItems.Items.Clear();
            entryMap.Clear();

            int index = -1;
            for (int ii = 0; ii < entries.Count; ii++)
            {
                if (txtSearch.Text == "" || entries[ii].IndexOf(txtSearch.Text, StringComparison.CurrentCultureIgnoreCase) > -1)
                {
                    lbxItems.Items.Add(ii + ": " + entries[ii]);
                    entryMap.Add(ii);
                    if (ii == internalIndex)
                        index = entryMap.Count-1;
                }
            }
            if (index > -1)
                SelectedIndex = index;
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            RefreshFilter();
        }

        public int IndexFromPoint(Point p)
        {
            return lbxItems.IndexFromPoint(p);
        }
    }
}
