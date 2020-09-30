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
            /// <summary>
            /// Index in the keys list
            /// </summary>
            public int keyIndex;

            /// <summary>
            /// Index within the priority
            /// </summary>
            public int index;

            public PriorityListIndex(int p, int i)
            {
                keyIndex = p;
                index = i;
            }
        }

        public IPriorityList Collection { get; private set; }
        private List<Priority> keys;

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


        public delegate void EditElementOp(Priority priority, int index, object element);
        public delegate void ElementOp(Priority priority, int index, object element, EditElementOp op);

        public delegate void EditPriorityOp(Priority priority, int index, Priority newPriority);
        public delegate void PriorityOp(Priority priority, int index, EditPriorityOp op);

        public ElementOp OnEditItem;
        public PriorityOp OnEditPriority;
        public ReflectionExt.TypeStringConv StringConv;

        public PriorityListBox()
        {
            InitializeComponent();
            StringConv = DefaultStringConv;
        }

        private string DefaultStringConv(object obj)
        {
            return obj.ToString();
        }

        public void LoadFromList(Type type, IPriorityList source)
        {
            Collection = (IPriorityList)Activator.CreateInstance(type);
            keys = new List<Priority>();
            foreach (Priority priority in source.GetPriorities())
            {
                keys.Add(priority);
                foreach (object item in source.GetItems(priority))
                    Collection.Add(priority, item);
            }

            keys.Sort();
            for(int ii = 0; ii < keys.Count; ii++)
            {
                Priority priority = keys[ii];
                foreach(object item in Collection.GetItems(priority))
                    lbxCollection.Items.Add(getEntryString(priority, item));
            }
        }

        private void editItem(Priority priority, int index, object element)
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

            lbxCollection.Items[index] = getEntryString(priority, element);
        }

        private void insertItem(Priority priority, int index, object element)
        {
            index = Math.Min(Math.Max(0, index), Collection.GetCountAtPriority(priority)+1);
            bool addKey = Collection.GetCountAtPriority(priority) == 0;

            Collection.Insert(priority, index, element);

            int boxIndex = findBoxIndex(priority, index);
            if (addKey)
            {
                for (int ii = 0; ii < keys.Count; ii++)
                {
                    if (priority < keys[ii])
                    {
                        keys.Insert(ii, priority);
                        break;
                    }
                }
            }

            lbxCollection.Items.Insert(boxIndex, getEntryString(priority, element));
        }

        private void lbxCollection_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            int boxIndex = lbxCollection.IndexFromPoint(e.X, e.Y);
            if (boxIndex > -1)
            {
                PriorityListIndex priorityIndex = getListBoxIndex();
                Priority priority = keys[priorityIndex.keyIndex];
                int index = priorityIndex.index;
                object element = Collection.Get(priority, index);
                OnEditItem(priority, index, element, editItem);
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            int boxIndex = lbxCollection.SelectedIndex;
            Priority priority = new Priority(0);
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
                Priority priority = keys[index.keyIndex];
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
                Priority priority = keys[ii];
                int count = Collection.GetCountAtPriority(priority);
                if (lbxCollection.SelectedIndex < runningIndex + count)
                    break;
                runningIndex += count;
            }
            return new PriorityListIndex(ii, lbxCollection.SelectedIndex - runningIndex);
        }

        private void Switch(Priority priority, int a, int b, int ad, int bd)
        {
            object obj = Collection.Get(priority, a);
            Collection.Set(priority, a, Collection.Get(priority, b));
            Collection.Set(priority, b, obj);
            lbxCollection.Items[ad] = getEntryString(priority, Collection.Get(priority, a));
            lbxCollection.Items[bd] = getEntryString(priority, Collection.Get(priority, b));

        }

        private string getEntryString(Priority priority, object obj)
        {
            return priority.ToString() + ": " + StringConv(obj);
        }

        /// <summary>
        /// Gets the lowest tier in which the two priorities differ.
        /// If two priorities are the same up to the last tier of one of them, that tier is selected instead.
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        private int getDiffPosition(Priority p1, Priority p2)
        {
            int ii = 0;
            while (true)
            {
                if (ii > p1.Length || ii > p2.Length)
                    return ii - 1;
                if (p1[ii] != p2[ii])
                    return ii;
                ii++;
            }
        }

        private Priority getTruncation(Priority input, int newLength)
        {
            int[] newArgs = new int[newLength];
            for (int ii = 0; ii < newLength; ii++)
                newArgs[ii] = input[ii];

            return new Priority(newArgs);
        }

        private Priority getNextPriority(Priority start, Priority end)
        {
            int diffPos = getDiffPosition(start, end);
            int tierDiff = end[diffPos] - start[diffPos];
            if (Math.Abs(tierDiff) <= 1)
                return end;
            else
            {
                {
                    //get a "mid" priority up to the diffpos digit
                    //essentially the start priority, but truncated
                    Priority midPriority = getTruncation(start, diffPos + 1);
                    //if the mid priority stands between the start and the end
                    //the mid priority will be the next priority
                    if (tierDiff > 0 && midPriority > start)
                        return midPriority;
                    if (tierDiff < 0 && midPriority < start)
                        return midPriority;
                }
                //if we don't hit the midpos, increment/decrement the digit
                {
                    int[] newArgs = new int[start.Length];
                    for (int ii = 0; ii < start.Length; ii++)
                        newArgs[ii] = start[ii];
                    newArgs[diffPos] = newArgs[diffPos] + Math.Sign(tierDiff);
                    Priority newPriority = new Priority(newArgs);
                    return newPriority;
                }
            }
        }
        
        private void btnUp_Click(object sender, EventArgs e)
        {
            if (lbxCollection.SelectedIndex < 0)
                return;

            PriorityListIndex index = getListBoxIndex();
            Priority currentPriority = keys[index.keyIndex];
            if (index.index == 0)
            {
                Priority newPriority = (index.keyIndex > 0) ? keys[index.keyIndex - 1] : new Priority(currentPriority[0]-1);

                //remove from current
                object obj = Collection.Get(currentPriority, index.index);
                Collection.RemoveAt(currentPriority, index.index);

                //send it to the higher tier
                Priority nextPriority = getNextPriority(currentPriority, newPriority);
                Collection.Add(nextPriority, obj);

                //synchronize key list
                if (Collection.GetCountAtPriority(currentPriority) == 0)
                    keys.RemoveAt(index.keyIndex);

                if (index.keyIndex == 0 || keys[index.keyIndex - 1] != nextPriority)
                    keys.Insert(index.keyIndex, nextPriority);

                //regardless, just change the name of the selected index
                lbxCollection.Items[lbxCollection.SelectedIndex] = getEntryString(nextPriority, obj);
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
            Priority currentPriority = keys[index.keyIndex];
            if (index.index == Collection.GetCountAtPriority(currentPriority) - 1)
            {
                Priority newPriority = (index.keyIndex < keys.Count - 1) ? keys[index.keyIndex + 1] : new Priority(currentPriority[0] + 1);

                //remove from current
                object obj = Collection.Get(currentPriority, index.index);
                Collection.RemoveAt(currentPriority, index.index);

                //send it to the lower tier
                Priority nextPriority = getNextPriority(currentPriority, newPriority);
                Collection.Add(nextPriority, obj);

                //synchronize key list
                if (index.keyIndex == keys.Count - 1 || keys[index.keyIndex + 1] != nextPriority)
                    keys.Insert(index.keyIndex + 1, nextPriority);

                if (Collection.GetCountAtPriority(currentPriority) == 0)
                    keys.RemoveAt(index.keyIndex);

                //regardless, just change the name of the selected index
                lbxCollection.Items[lbxCollection.SelectedIndex] = getEntryString(nextPriority, obj);
            }
            else
            {
                int selectedIndex = lbxCollection.SelectedIndex;
                Switch(currentPriority, index.index, index.index + 1, selectedIndex, selectedIndex + 1);
                lbxCollection.SelectedIndex = selectedIndex + 1;
            }
        }

        private int findBoxIndex(Priority priority, int index)
        {
            int boxIndex = 0;
            for (int ii = 0; ii < keys.Count; ii++)
            {
                if (priority <= keys[ii])
                    break;
                boxIndex += Collection.GetCountAtPriority(keys[ii]);
            }
            boxIndex += index;
            return boxIndex;
        }

        private void changePriority(Priority priority, int index, Priority newPriority)
        {
            //remove old
            object element = Collection.Get(priority, index);
            int origKeyIndex = keys.IndexOf(priority);
            int origBoxIndex = findBoxIndex(priority, index);

            Collection.RemoveAt(priority, index);
            if (Collection.GetCountAtPriority(priority) == 0)
                keys.RemoveAt(origKeyIndex);

            lbxCollection.Items.RemoveAt(origBoxIndex);


            //add new
            bool addKey = Collection.GetCountAtPriority(newPriority) == 0;
            Collection.Insert(newPriority, 0, element);

            int boxIndex = findBoxIndex(newPriority, 0);
            if (addKey)
            {
                for (int ii = 0; ii < keys.Count; ii++)
                {
                    if (newPriority < keys[ii])
                    {
                        keys.Insert(ii, newPriority);
                        break;
                    }
                }
            }

            lbxCollection.Items.Insert(boxIndex, getEntryString(newPriority, element));
        }

        private void btnEditKey_Click(object sender, EventArgs e)
        {
            if (lbxCollection.SelectedIndex > -1)
            {
                PriorityListIndex index = getListBoxIndex();
                Priority priority = keys[index.keyIndex];
                OnEditPriority(priority, index.index, changePriority);
            }
        }
    }
}
