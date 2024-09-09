using System;
using System.Collections.Generic;
using System.Text;
using ReactiveUI;
using System.Collections.ObjectModel;
using Avalonia.Interactivity;
using Avalonia.Controls;
using RogueElements;
using System.Collections;
using Avalonia.Input;
using RogueEssence.Dev.Views;

namespace RogueEssence.Dev.ViewModels
{
    public class PriorityElement
    {
        private Priority priority;
        public Priority Priority
        {
            get { return priority; }
        }
        //PriorityObj is solely used for DataGrid's binding
        //for some reason, if priority was bound to the datagrid as a type,
        //it will appear as blank
        public object PriorityObj
        {
            get { return priority; }
        }
        private object value;
        public object Value
        {
            get { return value; }
        }
        public string DisplayValue
        {
            get { return conv.GetString(value); }
        }

        private StringConv conv;


        public PriorityElement(StringConv conv, Priority priority, object value)
        {
            this.conv = conv;
            this.priority = priority;
            this.value = value;
        }
    }

    public class PriorityListBoxViewModel : ViewModelBase
    {
        public ObservableCollection<PriorityElement> Collection { get; }

        private int selectedIndex;
        public int SelectedIndex
        {
            get { return selectedIndex; }
            set { this.SetIfChanged(ref selectedIndex, value); }
        }


        public delegate void EditElementOp(Priority priority, int index, object element);
        public delegate void ElementOp(Priority priority, int index, object element, bool advancedEdit, EditElementOp op);

        public delegate void EditPriorityOp(Priority priority, int index, Priority newPriority);
        public delegate void PriorityOp(Priority priority, int index, bool advancedEdit, EditPriorityOp op);

        public ElementOp OnEditItem;
        public PriorityOp OnEditPriority;

        public StringConv StringConv;

        private Window parent;

        public bool ConfirmDelete;

        public PriorityListBoxViewModel(Window parent, StringConv conv)
        {
            StringConv = conv;
            this.parent = parent;
            Collection = new ObservableCollection<PriorityElement>();
        }

        public IPriorityList GetList(Type type)
        {
            IPriorityList result = (IPriorityList)Activator.CreateInstance(type);
            foreach (PriorityElement item in Collection)
                result.Add(item.Priority, item.Value);
            return result;
        }

        public void LoadFromList(IPriorityList source)
        {
            Collection.Clear();
            foreach (Priority priority in source.GetPriorities())
            {
                //keys.Add(priority);
                foreach (object item in source.GetItems(priority))
                    Collection.Add(new PriorityElement(StringConv, priority, item));
            }
        }


        private void editItem(Priority priority, int index, object element)
        {
            Collection[index] = new PriorityElement(StringConv, priority, element);
            SelectedIndex = index;
        }

        private void insertItem(Priority priority, int index, object element)
        {
            int boxIndex = findBoxIndex(priority, index);
            Collection.Insert(boxIndex, new PriorityElement(StringConv, priority, element));
            SelectedIndex = boxIndex;
        }

        public void InsertOnKey(int boxIndex, object element)
        {
            Priority priority = Priority.Zero;
            if (boxIndex >= Collection.Count)
            {
                if (boxIndex > 0)
                    priority = Collection[boxIndex-1].Priority;
            }
            else if (boxIndex >= 0)
                priority = Collection[boxIndex].Priority;

            Collection.Insert(boxIndex, new PriorityElement(StringConv, priority, element));
            SelectedIndex = boxIndex;
        }

        private int findBoxIndex(Priority priority, int index)
        {
            if (Collection.Count == 0)
                return 0;

            Priority prevPriority = Collection[0].Priority;
            int runningIndex = 0;
            for (int ii = 0; ii < Collection.Count; ii++)
            {
                if (ii > 0 && Collection[ii].Priority == prevPriority)
                    runningIndex++;
                else
                    runningIndex = 0;

                if (Collection[ii].Priority > priority)
                    return ii;
                else if (Collection[ii].Priority == priority && runningIndex == index)
                    return ii;

                prevPriority = Collection[ii].Priority;
            }
            return Collection.Count;
        }

        public void lbxCollection_DoubleClick(object sender, PointerReleasedEventArgs e)
        {
            //int boxIndex = lbxCollection.IndexFromPoint(e.X, e.Y);
            int boxIndex = SelectedIndex;
            KeyModifiers modifiers = e.KeyModifiers;
            bool advancedEdit = modifiers.HasFlag(KeyModifiers.Shift);
            if (boxIndex > -1)
            {
                Priority priority = Collection[boxIndex].Priority;
                object element = Collection[boxIndex].Value;
                OnEditItem?.Invoke(priority, boxIndex, element, advancedEdit, editItem);
            }
        }

        public void btnAdd_Click(bool advancedEdit)
        {
            Priority priority = new Priority(0);
            int index = 0;
            if (SelectedIndex >= 0)
            {
                priority = Collection[SelectedIndex].Priority;
                index = SelectedIndex + 1;
            }
            object element = null;
            OnEditItem(priority, index, element, advancedEdit, insertItem);
        }

        public async void btnDelete_Click()
        {
            if (SelectedIndex > -1 && SelectedIndex < Collection.Count)
            {
                if (ConfirmDelete)
                {
                    MessageBox.MessageBoxResult result = await MessageBox.Show(parent, "Are you sure you want to delete this item:\n" + Collection[SelectedIndex].DisplayValue, "Confirm Delete",
                    MessageBox.MessageBoxButtons.YesNo);
                    if (result == MessageBox.MessageBoxResult.No)
                        return;
                }

                Collection.RemoveAt(SelectedIndex);
            }
        }



        private void Switch(int a, int b)
        {
            PriorityElement obj = Collection[a];
            Collection[a] = Collection[b];
            Collection[b] = obj;
        }


        public void btnUp_Click()
        {
            if (SelectedIndex < 0)
                return;

            int boxIndex = SelectedIndex;
            Priority priority = Collection[boxIndex].Priority;
            Priority currentPriority = priority;
            if (boxIndex == 0 || Collection[boxIndex].Priority != Collection[boxIndex - 1].Priority)
            {
                Priority newPriority = (boxIndex > 0) ? Collection[boxIndex - 1].Priority : new Priority(currentPriority[0] - 1);

                Priority nextPriority = getNextPriority(currentPriority, newPriority);
                object obj = Collection[boxIndex].Value;
                Collection[boxIndex] = new PriorityElement(StringConv, nextPriority, obj);
                SelectedIndex = boxIndex;
            }
            else
            {
                //switch
                int selectedIndex = SelectedIndex;
                Switch(boxIndex, boxIndex - 1);
                SelectedIndex = selectedIndex - 1;
            }
        }

        public void btnDown_Click()
        {
            if (SelectedIndex < 0)
                return;

            int boxIndex = SelectedIndex;
            Priority priority = Collection[boxIndex].Priority;
            Priority currentPriority = priority;
            if (boxIndex == Collection.Count - 1 || Collection[boxIndex].Priority != Collection[boxIndex + 1].Priority)
            {
                Priority newPriority = (boxIndex < Collection.Count - 1) ? Collection[boxIndex + 1].Priority : new Priority(currentPriority[0] + 1);

                Priority nextPriority = getNextPriority(currentPriority, newPriority);
                object obj = Collection[boxIndex].Value;
                Collection[boxIndex] = new PriorityElement(StringConv, nextPriority, obj);
                SelectedIndex = boxIndex;
            }
            else
            {
                int selectedIndex = SelectedIndex;
                Switch(boxIndex, boxIndex + 1);
                SelectedIndex = selectedIndex + 1;
            }
        }

        private void changePriority(Priority priority, int index, Priority newPriority)
        {
            PriorityElement item = Collection[index];
            Collection.RemoveAt(index);

            int newBoxIndex = findBoxIndex(newPriority, 0);
            Collection.Insert(newBoxIndex, new PriorityElement(StringConv, newPriority, item.Value));
            SelectedIndex = newBoxIndex;
        }

        public void btnEditKey_Click(bool advancedEdit)
        {
            if (SelectedIndex > -1)
            {
                Priority priority = Collection[SelectedIndex].Priority;
                OnEditPriority(priority, SelectedIndex, advancedEdit, changePriority);
            }
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
                if (ii >= p1.Length || ii >= p2.Length)
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
    }
}
