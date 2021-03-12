using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using RogueElements;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Reactive.Subjects;

namespace RogueEssence.Dev.Views
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

        public PriorityElement(Priority priority, object value)
        {
            this.priority = priority;
            this.value = value;
        }
    }

    public class PriorityListBox : UserControl
    {
        private ObservableCollection<PriorityElement> collection;

        public int SelectedIndex
        {
            get { return gridCollection.SelectedIndex; }
            set { gridCollection.SelectedIndex = value; }
        }


        public delegate void EditElementOp(Priority priority, int index, object element);
        public delegate void ElementOp(Priority priority, int index, object element, EditElementOp op);

        public delegate void EditPriorityOp(Priority priority, int index, Priority newPriority);
        public delegate void PriorityOp(Priority priority, int index, EditPriorityOp op);

        public ElementOp OnEditItem;
        public PriorityOp OnEditPriority;

        private DataGrid gridCollection;

        public PriorityListBox()
        {
            this.InitializeComponent();
            gridCollection = this.FindControl<DataGrid>("gridItems");
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public IPriorityList GetList(Type type)
        {
            IPriorityList result = (IPriorityList)Activator.CreateInstance(type);
            foreach (PriorityElement item in collection)
                result.Add(item.Priority, item.Value);
            return result;
        }

        public void LoadFromList(IPriorityList source)
        {
            collection = new ObservableCollection<PriorityElement>();
            foreach (Priority priority in source.GetPriorities())
            {
                //keys.Add(priority);
                foreach (object item in source.GetItems(priority))
                    collection.Add(new PriorityElement(priority, item));
            }

            //bind the collection
            var subject = new Subject<ObservableCollection<PriorityElement>>();
            gridCollection.Bind(DataGrid.ItemsProperty, subject);
            subject.OnNext(collection);
        }


        private void editItem(Priority priority, int index, object element)
        {
            collection[index] = new PriorityElement(priority, element);
            SelectedIndex = index;
        }

        private void insertItem(Priority priority, int index, object element)
        {
            int boxIndex = findBoxIndex(priority, index);
            collection.Insert(boxIndex, new PriorityElement(priority, element));
            SelectedIndex = boxIndex;
        }

        private int findBoxIndex(Priority priority, int index)
        {
            if (collection.Count == 0)
                return 0;

            Priority prevPriority = collection[0].Priority;
            int runningIndex = 0;
            for (int ii = 1; ii < collection.Count; ii++)
            {
                if (collection[ii].Priority == prevPriority)
                    runningIndex++;
                else
                    runningIndex = 0;

                if (collection[ii].Priority > priority)
                    return ii;
                else if (collection[ii].Priority == priority && runningIndex == index)
                    return ii;

                prevPriority = collection[ii].Priority;
            }
            return collection.Count;
        }

        private int getPriorityIndex(int boxIndex)
        {
            int index = 0;
            while (boxIndex > 0 && collection[boxIndex].Priority == collection[boxIndex-1].Priority)
            {
                index++;
                boxIndex--;
            }
            return index;
        }


        public void lbxCollection_DoubleClick(object sender, RoutedEventArgs e)
        {
            //int boxIndex = lbxCollection.IndexFromPoint(e.X, e.Y);
            int boxIndex = SelectedIndex;
            if (boxIndex > -1)
            {
                Priority priority = collection[boxIndex].Priority;
                int index = getPriorityIndex(boxIndex);
                object element = collection[boxIndex].Value;
                OnEditItem(priority, index, element, editItem);
            }
        }

        public void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            int boxIndex = SelectedIndex;
            Priority priority = new Priority(0);
            int index = 0;
            if (boxIndex >= 0)
            {
                priority = collection[boxIndex].Priority;
                index = getPriorityIndex(boxIndex);
            }
            object element = null;
            OnEditItem(priority, index, element, insertItem);
        }

        public void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedIndex > -1)
                collection.RemoveAt(SelectedIndex);
        }



        private void Switch(int a, int b)
        {
            PriorityElement obj = collection[a];
            collection[a] = collection[b];
            collection[b] = obj;
        }


        public void btnUp_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedIndex < 0)
                return;

            int boxIndex = SelectedIndex;
            Priority priority = collection[boxIndex].Priority;
            Priority currentPriority = priority;
            if (boxIndex == 0 || collection[boxIndex].Priority != collection[boxIndex - 1].Priority)
            {
                Priority newPriority = (boxIndex > 0) ? collection[boxIndex - 1].Priority : new Priority(currentPriority[0] - 1);

                Priority nextPriority = getNextPriority(currentPriority, newPriority);
                object obj = collection[boxIndex].Value;
                collection[boxIndex] = new PriorityElement(nextPriority, obj);
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

        public void btnDown_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedIndex < 0)
                return;

            int boxIndex = SelectedIndex;
            Priority priority = collection[boxIndex].Priority;
            Priority currentPriority = priority;
            if (boxIndex == collection.Count - 1 || collection[boxIndex].Priority != collection[boxIndex + 1].Priority)
            {
                Priority newPriority = (boxIndex < collection.Count-1) ? collection[boxIndex + 1].Priority : new Priority(currentPriority[0] + 1);

                Priority nextPriority = getNextPriority(currentPriority, newPriority);
                object obj = collection[boxIndex].Value;
                collection[boxIndex] = new PriorityElement(nextPriority, obj);
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
            PriorityElement item = collection[index];
            collection.RemoveAt(index);

            int newBoxIndex = findBoxIndex(newPriority, 0);
            collection.Insert(newBoxIndex, new PriorityElement(newPriority, item.Value));
            SelectedIndex = newBoxIndex;
        }

        public void btnEditKey_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedIndex > -1)
            {
                int boxIndex = SelectedIndex;
                Priority priority = collection[boxIndex].Priority;
                int index = getPriorityIndex(boxIndex);
                OnEditPriority(priority, index, changePriority);
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
