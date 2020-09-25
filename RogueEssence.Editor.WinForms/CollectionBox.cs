using System;
using System.Collections;
using System.Windows.Forms;

namespace RogueEssence.Dev
{
    public partial class CollectionBox : UserControl
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


        public delegate void EditElementOp(int index, object element);
        public delegate void ElementOp(int index, object element, EditElementOp op);

        public ElementOp OnEditItem;
        public ReflectionExt.TypeStringConv StringConv;

        public CollectionBox()
        {
            InitializeComponent();

            StringConv = DefaultStringConv;
        }

        private string DefaultStringConv(object obj)
        {
            return obj.ToString();
        }

        public void LoadFromList(Type type, IList source)
        {
            Collection = (IList)Activator.CreateInstance(type);
            foreach (object obj in source)
                Collection.Add(obj);

            foreach (object obj in Collection)
                lbxCollection.Items.Add(StringConv(obj));
        }

        private void editItem(int index, object element)
        {
            index = Math.Min(Math.Max(0, index), Collection.Count);
            Collection[index] = element;
            lbxCollection.Items[index] = StringConv(element);
        }

        private void insertItem(int index, object element)
        {
            index = Math.Min(Math.Max(0, index), Collection.Count+1);
            Collection.Insert(index, element);
            lbxCollection.Items.Insert(index, StringConv(element));
        }

        private void lbxCollection_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            int index = lbxCollection.IndexFromPoint(e.X, e.Y);
            if (index > -1)
            {
                object element = Collection[index];
                OnEditItem(index, element, editItem);
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            int index = lbxCollection.SelectedIndex;
            if (index < 0)
                index = lbxCollection.Items.Count;
            object element = null;
            OnEditItem(index, element, insertItem);
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (lbxCollection.SelectedIndex > -1)
            {
                Collection.RemoveAt(lbxCollection.SelectedIndex);
                lbxCollection.Items.RemoveAt(lbxCollection.SelectedIndex);
            }
        }

        private void Switch(int a, int b)
        {
            object obj = Collection[a];
            Collection[a] = Collection[b];
            Collection[b] = obj;
            lbxCollection.Items[a] = StringConv(Collection[a]);
            lbxCollection.Items[b] = StringConv(Collection[b]);

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
            if (lbxCollection.SelectedIndex > -1 && lbxCollection.SelectedIndex < lbxCollection.Items.Count - 1)
            {
                int index = lbxCollection.SelectedIndex;
                Switch(lbxCollection.SelectedIndex, lbxCollection.SelectedIndex + 1);
                lbxCollection.SelectedIndex = index + 1;
            }
        }
    }
}
