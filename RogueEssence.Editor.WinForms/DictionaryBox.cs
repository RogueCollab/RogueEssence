using System;
using System.Collections;
using System.Windows.Forms;

namespace RogueEssence.Dev
{
    public partial class DictionaryBox : UserControl
    {
        public IDictionary Dictionary { get; private set; }

        public delegate void EditElementOp(object key, object element);
        public delegate void ElementOp(object key, object element, EditElementOp op);

        public ElementOp OnEditKey;
        public ElementOp OnEditItem;
        public ReflectionExt.TypeStringConv StringConv;

        public DictionaryBox()
        {
            InitializeComponent();
            StringConv = DefaultStringConv;
        }

        private string DefaultStringConv(object obj)
        {
            return obj.ToString();
        }

        public void LoadFromDictionary(Type type, IDictionary source)
        {
            Dictionary = (IDictionary)Activator.CreateInstance(type);
            foreach (object obj in source.Keys)
                Dictionary.Add(obj, source[obj]);


            foreach (object obj in Dictionary.Keys)
                lbxDictionary.Items.Add("[" + obj.ToString() + "] " + StringConv(Dictionary[obj]));
        }



        private void editItem(object key, object element)
        {
            int index = getIndexFromKey(key);
            Dictionary[key] = element;
            lbxDictionary.Items[index] = "[" + key.ToString() + "] " + StringConv(Dictionary[key]);
        }

        private void insertKey(object key, object element)
        {
            if (Dictionary.Contains(key))
            {
                MessageBox.Show("Dictionary already contains this key!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            OnEditItem(key, element, insertItem);
        }

        private void insertItem(object key, object element)
        {
            Dictionary.Add(key, element);
            lbxDictionary.Items.Add("[" + key.ToString() + "] " + StringConv(element));
        }


        private object getKeyFromIndex(int index)
        {
            object key = null;
            int curIndex = 0;
            foreach (object curKey in Dictionary.Keys)
            {
                if (curIndex == index)
                {
                    key = curKey;
                    break;
                }
                curIndex++;
            }
            return key;
        }

        private int getIndexFromKey(object key)
        {
            int curIndex = 0;
            foreach (object curKey in Dictionary.Keys)
            {
                if (curKey == key)
                {
                    key = curKey;
                    break;
                }
                curIndex++;
            }
            return curIndex;
        }


        private void lbxDictionary_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            int index = lbxDictionary.IndexFromPoint(e.X, e.Y);
            if (index > -1)
            {
                object key = getKeyFromIndex(index);
                object element = Dictionary[key];
                OnEditItem(key, element, editItem);
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            object newKey = null;
            object element = null;
            OnEditKey(newKey, element, insertKey);
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (lbxDictionary.SelectedIndex > -1)
            {
                object key = null;
                int curIndex = 0;
                foreach (object curKey in Dictionary.Keys)
                {
                    if (curIndex == lbxDictionary.SelectedIndex)
                    {
                        key = curKey;
                        break;
                    }
                    curIndex++;
                }
                Dictionary.Remove(key);
                lbxDictionary.Items.RemoveAt(lbxDictionary.SelectedIndex);
            }
        }
    }
}
