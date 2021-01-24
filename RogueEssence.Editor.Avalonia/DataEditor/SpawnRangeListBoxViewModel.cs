using System;
using System.Collections.Generic;
using System.Text;
using ReactiveUI;
using System.Collections.ObjectModel;
using Avalonia.Interactivity;
using Avalonia.Controls;
using RogueElements;

namespace RogueEssence.Dev.ViewModels
{
    public class SpawnRangeListElement : ViewModelBase
    {
        private int start;
        public int Start
        {
            get { return start; }
            set { this.SetIfChanged(ref start, value); }
        }
        private int end;
        public int End
        {
            get { return end; }
            set { this.SetIfChanged(ref end, value); }
        }
        private int weight;
        public int Weight
        {
            get { return weight; }
            set { this.SetIfChanged(ref weight, value); }
        }
        private object val;
        public object Value
        {
            get { return val; }
            set { this.SetIfChanged(ref val, value); }
        }

        public SpawnRangeListElement(int start, int end, int weight, object val)
        {
            this.start = start;
            this.end = end;
            this.weight = weight;
            this.val = val;
        }
    }

    public class SpawnRangeListBoxViewModel : ViewModelBase
    {
        public delegate void EditElementOp(int index, object element);
        public delegate void ElementOp(int index, object element, EditElementOp op);

        public event ElementOp OnEditItem;


        public SpawnRangeListBoxViewModel()
        {
            Collection = new ObservableCollection<SpawnRangeListElement>();
        }

        public ObservableCollection<SpawnRangeListElement> Collection { get; }

        private int currentElement;
        public int CurrentElement
        {
            get { return currentElement; }
            set
            {
                this.SetIfChanged(ref currentElement, value);
                if (currentElement > -1)
                {
                    CurrentWeight = Collection[currentElement].Weight;
                    CurrentStart = Collection[currentElement].Start;
                    CurrentEnd = Collection[currentElement].End;
                }
                else
                {
                    CurrentWeight = 1;
                    CurrentStart = 0;
                    CurrentEnd = 1;
                }
            }
        }

        private int currentWeight;
        public int CurrentWeight
        {
            get { return currentWeight; }
            set
            {
                this.SetIfChanged(ref currentWeight, value);
                if (currentElement > -1)
                    Collection[currentElement].Weight = currentWeight;
            }
        }

        private int currentStart;
        public int CurrentStart
        {
            get { return currentStart; }
            set
            {
                this.SetIfChanged(ref currentStart, value);
                if (currentElement > -1)
                    Collection[currentElement].Start = currentStart;
            }
        }

        private int currentEnd;
        public int CurrentEnd
        {
            get { return currentEnd; }
            set
            {
                this.SetIfChanged(ref currentEnd, value);
                if (currentElement > -1)
                    Collection[currentElement].End = currentEnd;
            }
        }

        public ISpawnRangeList GetList(Type type)
        {
            ISpawnRangeList result = (ISpawnRangeList)Activator.CreateInstance(type);
            foreach (SpawnRangeListElement item in Collection)
                result.Add(item.Value, new IntRange(item.Start, item.End), item.Weight);
            return result;
        }

        public void LoadFromList(ISpawnRangeList source)
        {
            Collection.Clear();
            for (int ii = 0; ii < source.Count; ii++)
            {
                object obj = source.GetSpawn(ii);
                IntRange range = source.GetSpawnRange(ii);
                int rate = source.GetSpawnRate(ii);
                Collection.Add(new SpawnRangeListElement(range.Min, range.Max, rate, obj));
            }
        }


        private void editItem(int index, object element)
        {
            index = Math.Min(Math.Max(0, index), Collection.Count);
            Collection[index] = new SpawnRangeListElement(Collection[index].Start, Collection[index].End, Collection[index].Weight, element);
        }

        private void insertItem(int index, object element)
        {
            index = Math.Min(Math.Max(0, index), Collection.Count + 1);
            Collection.Insert(index, new SpawnRangeListElement(0, 1, 10, element));
        }

        public void gridCollection_DoubleClick(object sender, RoutedEventArgs e)
        {
            //int index = lbxCollection.IndexFromPoint(e.X, e.Y);
            int index = CurrentElement;
            if (index > -1)
            {
                object element = Collection[index];
                OnEditItem?.Invoke(index, element, editItem);
            }
        }


        private void btnAdd_Click()
        {
            int index = CurrentElement;
            if (index < 0)
                index = Collection.Count;
            object element = null;
            OnEditItem(index, element, insertItem);
        }

        private void btnDelete_Click()
        {
            if (CurrentElement > -1)
                Collection.RemoveAt(CurrentElement);
        }

        private void Switch(int a, int b)
        {
            SpawnRangeListElement obj = Collection[a];
            Collection[a] = Collection[b];
            Collection[b] = obj;
        }

        private void btnUp_Click()
        {
            if (CurrentElement > 0)
            {
                int index = CurrentElement;
                Switch(CurrentElement, CurrentElement - 1);
                CurrentElement = index - 1;
            }
        }

        private void btnDown_Click()
        {
            if (CurrentElement > -1 && CurrentElement < Collection.Count - 1)
            {
                int index = CurrentElement;
                Switch(CurrentElement, CurrentElement + 1);
                CurrentElement = index + 1;
            }
        }

    }
}
