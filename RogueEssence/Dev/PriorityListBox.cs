using System;
using System.Windows.Forms;
using RogueElements;
using System.Collections.Generic;

namespace RogueEssence.Dev
{
    public partial class PriorityListBox : UserControl
    {
        private class PriorityListIndex
        {
            public int keyIndex;
            public int index;

            public PriorityListIndex(int p, int i)
            {
                keyIndex = p;
                index = i;
            }
        }

        public IPriorityList Collection { get; private set; }
        private List<int> keys;

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


        public delegate void EditElementOp(int priority, int index, object element);
        public delegate void ElementOp(int priority, int index, object element, EditElementOp op);

        public ElementOp OnEditItem;

        public PriorityListBox()
        {
            InitializeComponent();
        }

        public void LoadFromList(Type type, IPriorityList source)
        {
            Collection = (IPriorityList)Activator.CreateInstance(type);
            keys = new List<int>();
            foreach (int priority in source.GetPriorities())
            {
                keys.Add(priority);
                foreach (object item in source.GetItems(priority))
                    Collection.Add(priority, item);
            }

            keys.Sort();
            for(int ii = 0; ii < keys.Count; ii++)
            {
                int priority = keys[ii];
                foreach(object item in Collection.GetItems(priority))
                    lbxCollection.Items.Add(getEntryString(priority, item));
            }
        }

        private void editItem(int priority, int index, object element)
        {
            if (Collection.GetCountAtPriority(priority) == 0)
                return;

            index = Math.Min(Math.Max(0, index), Collection.GetCountAtPriority(priority));
            Collection.Set(priority, index, element);

            int boxIndex = 0;
            for (int ii = 0; ii < keys.Count; ii++)
            {
                if (priority <= keys[ii])
                    break;
                boxIndex += Collection.GetCountAtPriority(keys[ii]);
            }
            boxIndex += index;

            lbxCollection.Items[index] = getEntryString(priority, element.ToString());
        }

        private void insertItem(int priority, int index, object element)
        {
            index = Math.Min(Math.Max(0, index), Collection.GetCountAtPriority(priority)+1);
            bool addKey = Collection.GetCountAtPriority(priority) == 0;

            Collection.Insert(priority, index, element);

            int boxIndex = 0;
            for (int ii = 0; ii < keys.Count; ii++)
            {
                if (priority < keys[ii])
                {
                    if (addKey)
                    {
                        addKey = false;
                        keys.Insert(ii, priority);
                    }
                    break;
                }
                else if (priority == keys[ii])
                    break;
                boxIndex += Collection.GetCountAtPriority(keys[ii]);
            }
            if (addKey)
                keys.Add(priority);
            boxIndex += index;

            lbxCollection.Items.Insert(boxIndex, getEntryString(priority, element.ToString()));
        }

        private void lbxCollection_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            int boxIndex = lbxCollection.IndexFromPoint(e.X, e.Y);
            if (boxIndex > -1)
            {
                PriorityListIndex priorityIndex = getListBoxIndex();
                int priority = keys[priorityIndex.keyIndex];
                int index = priorityIndex.index;
                object element = Collection.Get(priority, index);
                OnEditItem(priority, index, element, editItem);
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            int boxIndex = lbxCollection.SelectedIndex;
            int priority = 0;
            int index = 0;
            if (boxIndex >= 0)
            {
                PriorityListIndex priorityIndex = getListBoxIndex();
                priority = keys[priorityIndex.keyIndex];
                index = priorityIndex.index;
            }
            object element = null;
            OnEditItem(priority, index, element, insertItem);
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (lbxCollection.SelectedIndex > -1)
            {
                PriorityListIndex index = getListBoxIndex();
                int priority = keys[index.keyIndex];
                Collection.RemoveAt(priority, index.index);
                if (Collection.GetCountAtPriority(priority) == 0)
                    keys.RemoveAt(index.keyIndex);

                lbxCollection.Items.RemoveAt(lbxCollection.SelectedIndex);
            }
        }

        private PriorityListIndex getListBoxIndex()
        {
            int runningIndex = 0;
            int ii = 0;
            for (; ii < keys.Count; ii++)
            {
                int priority = keys[ii];
                int count = Collection.GetCountAtPriority(priority);
                if (lbxCollection.SelectedIndex < runningIndex + count)
                    break;
                runningIndex += count;
            }
            return new PriorityListIndex(ii, lbxCollection.SelectedIndex - runningIndex);
        }

        private void Switch(int priority, int a, int b, int ad, int bd)
        {
            object obj = Collection.Get(priority, a);
            Collection.Set(priority, a, Collection.Get(priority, b));
            Collection.Set(priority, b, obj);
            lbxCollection.Items[ad] = getEntryString(priority, Collection.Get(priority, a));
            lbxCollection.Items[bd] = getEntryString(priority, Collection.Get(priority, b));

        }

        private string getEntryString(int priority, object obj)
        {
            return priority + ": " + obj.ToString();
        }

        private void btnUp_Click(object sender, EventArgs e)
        {
            if (lbxCollection.SelectedIndex < 0)
                return;

            PriorityListIndex index = getListBoxIndex();
            int currentPriority = keys[index.keyIndex];
            if (index.index == 0)
            {

                //remove from current
                object obj = Collection.Get(currentPriority, index.index);
                Collection.RemoveAt(currentPriority, index.index);

                //send it to the higher tier
                int newPriority = currentPriority - 1;
                Collection.Add(newPriority, obj);

                //synchronize key list
                if (Collection.GetCountAtPriority(currentPriority) == 0)
                    keys.RemoveAt(index.keyIndex);

                if (index.keyIndex == 0 || keys[index.keyIndex - 1] < currentPriority - 1)
                    keys.Insert(index.keyIndex, newPriority);

                //regardless, just change the name of the selected index
                lbxCollection.Items[lbxCollection.SelectedIndex] = getEntryString(newPriority, obj);
            }
            else
            {
                //switch
                int selectedIndex = lbxCollection.SelectedIndex;
                Switch(currentPriority, index.index, index.index - 1, selectedIndex, selectedIndex - 1);
                lbxCollection.SelectedIndex = selectedIndex - 1;
            }
        }

        private void btnDown_Click(object sender, EventArgs e)
        {
            if (lbxCollection.SelectedIndex < 0)
                return;

            PriorityListIndex index = getListBoxIndex();
            int currentPriority = keys[index.keyIndex];
            if (index.index == Collection.GetCountAtPriority(currentPriority) - 1)
            {
                //remove from current
                object obj = Collection.Get(currentPriority, index.index);
                Collection.RemoveAt(currentPriority, index.index);

                //send it to the lower tier
                int newPriority = currentPriority + 1;
                Collection.Add(newPriority, obj);

                //synchronize key list
                if (index.keyIndex == keys.Count-1 || keys[index.keyIndex + 1] > currentPriority + 1)
                    keys.Insert(index.keyIndex+1, newPriority);

                if (Collection.GetCountAtPriority(currentPriority) == 0)
                    keys.RemoveAt(index.keyIndex);

                //regardless, just change the name of the selected index
                lbxCollection.Items[lbxCollection.SelectedIndex] = getEntryString(newPriority, obj);
            }
            else
            {
                int selectedIndex = lbxCollection.SelectedIndex;
                Switch(currentPriority, index.index, index.index + 1, selectedIndex, selectedIndex + 1);
                lbxCollection.SelectedIndex = selectedIndex + 1;
            }
        }
    }
}
