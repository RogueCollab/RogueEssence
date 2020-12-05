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
    public class PriorityListBox : UserControl
    {
        private ObservableCollection<(Priority, object)> collection;

        public int SelectedIndex
        {
            get { return lbxCollection.SelectedIndex; }
            set { lbxCollection.SelectedIndex = value; }
        }


        public delegate void EditElementOp(Priority priority, int index, object element);
        public delegate void ElementOp(Priority priority, int index, object element, EditElementOp op);

        public delegate void EditPriorityOp(Priority priority, int index, Priority newPriority);
        public delegate void PriorityOp(Priority priority, int index, EditPriorityOp op);

        public ElementOp OnEditItem;
        public PriorityOp OnEditPriority;

        private ListBox lbxCollection;

        public PriorityListBox()
        {
            this.InitializeComponent();
            lbxCollection = this.FindControl<ListBox>("lbxItems");
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public IPriorityList GetList(Type type)
        {
            IPriorityList result = (IPriorityList)Activator.CreateInstance(type);
            foreach ((Priority, object) item in collection)
                result.Add(item.Item1, item.Item2);
            return result;
        }

        public void LoadFromList(IPriorityList source)
        {
            collection = new ObservableCollection<(Priority, object)>();
            foreach (Priority priority in source.GetPriorities())
            {
                //keys.Add(priority);
                foreach (object item in source.GetItems(priority))
                    collection.Add((priority, item));
            }

            //bind the collection
            var subject = new Subject<ObservableCollection<(Priority, object)>>();
            lbxCollection.Bind(ComboBox.ItemsProperty, subject);
            subject.OnNext(collection);
        }


        private void editItem(Priority priority, int index, object element)
        {
            int boxIndex = findBoxIndex(priority, index);
            collection[boxIndex] = (priority, element);
        }

        private void insertItem(Priority priority, int index, object element)
        {
            int boxIndex = findBoxIndex(priority, index);
            collection.Insert(boxIndex, (priority, element));
        }

        private int findBoxIndex(Priority priority, int index)
        {
            if (collection.Count == 0)
                return 0;

            Priority prevPriority = collection[0].Item1;
            int runningIndex = 0;
            for (int ii = 1; ii < collection.Count; ii++)
            {
                if (collection[ii].Item1 == prevPriority)
                    runningIndex++;
                else
                    runningIndex = 0;

                if (collection[ii].Item1 > priority)
                    return ii;
                else if (collection[ii].Item1 == priority && runningIndex == index)
                    return ii;

                prevPriority = collection[ii].Item1;
            }
            return collection.Count;
        }

        private int getPriorityIndex(int boxIndex)
        {
            int index = 0;
            while (boxIndex > 0 && collection[boxIndex].Item1 == collection[boxIndex-1].Item1)
            {
                index++;
                boxIndex--;
            }
            return index;
        }


        public void lbxCollection_DoubleClick(object sender, RoutedEventArgs e)
        {
            //int boxIndex = lbxCollection.IndexFromPoint(e.X, e.Y);
            int boxIndex = lbxCollection.SelectedIndex;
            if (boxIndex > -1)
            {
                Priority priority = collection[boxIndex].Item1;
                int index = getPriorityIndex(boxIndex);
                object element = collection[boxIndex].Item2;
                OnEditItem(priority, index, element, editItem);
            }
        }

        public void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            int boxIndex = lbxCollection.SelectedIndex;
            Priority priority = new Priority(0);
            int index = 0;
            if (boxIndex >= 0)
            {
                priority = collection[boxIndex].Item1;
                index = getPriorityIndex(boxIndex);
            }
            object element = null;
            OnEditItem(priority, index, element, insertItem);
        }

        public void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (lbxCollection.SelectedIndex > -1)
                collection.RemoveAt(lbxCollection.SelectedIndex);
        }



        private void Switch(int a, int b)
        {
            (Priority, object) obj = collection[a];
            collection[a] = collection[b];
            collection[b] = obj;
        }


        public void btnUp_Click(object sender, RoutedEventArgs e)
        {
            if (lbxCollection.SelectedIndex < 0)
                return;

            int boxIndex = lbxCollection.SelectedIndex;
            Priority priority = collection[boxIndex].Item1;
            Priority currentPriority = priority;
            if (boxIndex == 0 || collection[boxIndex].Item1 != collection[boxIndex - 1].Item1)
            {
                Priority newPriority = (boxIndex > 0) ? collection[boxIndex - 1].Item1 : new Priority(currentPriority[0] - 1);

                Priority nextPriority = getNextPriority(currentPriority, newPriority);
                object obj = collection[boxIndex].Item2;
                collection[boxIndex] = (nextPriority, obj);
            }
            else
            {
                //switch
                int selectedIndex = lbxCollection.SelectedIndex;
                Switch(boxIndex, boxIndex - 1);
                lbxCollection.SelectedIndex = selectedIndex - 1;
            }
        }

        public void btnDown_Click(object sender, RoutedEventArgs e)
        {
            if (lbxCollection.SelectedIndex < 0)
                return;

            int boxIndex = lbxCollection.SelectedIndex;
            Priority priority = collection[boxIndex].Item1;
            Priority currentPriority = priority;
            if (boxIndex == collection.Count - 1 || collection[boxIndex].Item1 != collection[boxIndex + 1].Item1)
            {
                Priority newPriority = (boxIndex < collection.Count-1) ? collection[boxIndex + 1].Item1 : new Priority(currentPriority[0] + 1);

                Priority nextPriority = getNextPriority(currentPriority, newPriority);
                object obj = collection[boxIndex].Item2;
                collection[boxIndex] = (nextPriority, obj);
            }
            else
            {
                int selectedIndex = lbxCollection.SelectedIndex;
                Switch(boxIndex, boxIndex + 1);
                lbxCollection.SelectedIndex = selectedIndex + 1;
            }
        }

        private void changePriority(Priority priority, int index, Priority newPriority)
        {
            int boxIndex = findBoxIndex(priority, index);
            (Priority, object) item = collection[boxIndex];
            collection.RemoveAt(boxIndex);

            int newBoxIndex = findBoxIndex(priority, 0);
            collection.Insert(newBoxIndex, (newPriority, item.Item2));
        }

        public void btnEditKey_Click(object sender, RoutedEventArgs e)
        {
            if (lbxCollection.SelectedIndex > -1)
            {
                int boxIndex = lbxCollection.SelectedIndex;
                Priority priority = collection[boxIndex].Item1;
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



    }
}
