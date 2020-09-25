using System;
using System.Collections;
using System.Windows.Forms;

namespace RogueEssence.Dev
{
    public partial class LayerBox : UserControl
    {
        public IList Collection { get; private set; }

        public int SelectedIndex
        {
            get { return lbxCollection.SelectedIndex; }
            set { lbxCollection.SelectedIndex = value; }
        }

        public event EventHandler SelectedIndexChanged
        {
            add { lbxCollection.SelectedIndexChanged += value; }
            remove { lbxCollection.SelectedIndexChanged -= value; }
        }

        public event ItemCheckEventHandler CheckChanged
        {
            add { lbxCollection.ItemCheck += value; }
            remove { lbxCollection.ItemCheck -= value; }
        }

        public event CheckElementOp OnAddItem;

        public event ElementOp OnEditItem;

        public event CheckElementOp OnDuplicateItem;


        public delegate void EditElementOp(int index, object element);
        public delegate void EditCheckElementOp(int index, object element, bool isChecked);
        public delegate void ElementOp(int index, object element, EditElementOp op);
        public delegate void CheckElementOp(int index, object element, EditCheckElementOp op);
        public delegate bool ElementCheck(object element);

        public LayerBox()
        {
            InitializeComponent();
        }

        public void LoadFromList(IList source, ElementCheck isChecked)
        {
            Collection = source;

            lbxCollection.Items.Clear();
            foreach (object obj in Collection)
                lbxCollection.Items.Add(obj.ToString(), isChecked(obj));

            lbxCollection.SelectedIndex = 0;
        }

        private void editItem(int index, object element)
        {
            index = Math.Min(Math.Max(0, index), Collection.Count);
            Collection[index] = element;
            lbxCollection.Items[index] = element.ToString();
        }

        private void insertItem(int index, object element, bool isChecked)
        {
            index = Math.Min(Math.Max(0, index), Collection.Count+1);
            Collection.Insert(index, element);
            lbxCollection.Items.Insert(index, element.ToString());
            lbxCollection.SetItemChecked(index, isChecked);
        }

        private void lbxCollection_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            int index = lbxCollection.IndexFromPoint(e.X, e.Y);
            if (index > -1)
            {
                object element = Collection[index];
                OnEditItem?.Invoke(index, element, editItem);
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            int index = lbxCollection.SelectedIndex;
            if (index < 0)
                index = lbxCollection.Items.Count;
            object element = null;
            OnAddItem?.Invoke(index, element, insertItem);
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (lbxCollection.SelectedIndex > -1)
            {
                int oldIndex = lbxCollection.SelectedIndex;
                Collection.RemoveAt(lbxCollection.SelectedIndex);
                lbxCollection.Items.RemoveAt(lbxCollection.SelectedIndex);
                lbxCollection.SelectedIndex = Math.Max(oldIndex, Collection.Count-1);
                if (Collection.Count == 1)
                    btnDelete.Enabled = false;
            }
        }

        private void Switch(int a, int b)
        {
            object obj = Collection[a];
            Collection[a] = Collection[b];
            Collection[b] = obj;

            bool isChecked = lbxCollection.GetItemChecked(a);
            lbxCollection.SetItemChecked(a, lbxCollection.GetItemChecked(b));
            lbxCollection.SetItemChecked(b, isChecked);
            lbxCollection.Items[a] = Collection[a].ToString();
            lbxCollection.Items[b] = Collection[b].ToString();

        }

        private void btnUp_Click(object sender, EventArgs e)
        {
            if (lbxCollection.SelectedIndex > 0)
            {
                int index = lbxCollection.SelectedIndex;
                Switch(lbxCollection.SelectedIndex, lbxCollection.SelectedIndex - 1);
                lbxCollection.SelectedIndex = index - 1;
            }
        }

        private void btnDown_Click(object sender, EventArgs e)
        {
            if (lbxCollection.SelectedIndex < lbxCollection.Items.Count - 1)
            {
                int index = lbxCollection.SelectedIndex;
                Switch(lbxCollection.SelectedIndex, lbxCollection.SelectedIndex + 1);
                lbxCollection.SelectedIndex = index + 1;
            }
        }

        private void btnDuplicate_Click(object sender, EventArgs e)
        {
            int index = lbxCollection.SelectedIndex;
            if (index < 0)
                index = lbxCollection.Items.Count;
            object element = Collection[index];
            OnDuplicateItem?.Invoke(index, element, insertItem);
        }

        private void btnMerge_Click(object sender, EventArgs e)
        {

        }
        private void lbxCollection_MouseClick(object sender, MouseEventArgs e)
        {
            if ((e.Button == MouseButtons.Left) & (e.X > 13))
            {
                lbxCollection.SetItemChecked(this.lbxCollection.SelectedIndex, !this.lbxCollection.GetItemChecked(this.lbxCollection.SelectedIndex));
            }
        }
    }
}
