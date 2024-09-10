using System;
using System.Collections.Generic;
using System.Text;
using ReactiveUI;
using System.Collections.ObjectModel;
using Avalonia.Interactivity;
using Avalonia.Controls;
using Avalonia.Input;
using RogueElements;
using RogueEssence.Dev.Views;

namespace RogueEssence.Dev.ViewModels
{
    public class SpawnRangeListElement : ViewModelBase
    {
        private int start;
        public int Start
        {
            get { return start; }
            set
            {
                start = value;
                DisplayStart = DisplayStart;
            }
        }
        private int end;
        public int End
        {
            get { return end; }
            set
            {
                end = value;
                DisplayEnd = DisplayEnd;
            }
        }


        public int DisplayStart
        {
            get { return start + addMin; }
            set { this.RaisePropertyChanged(); }
        }
        public int DisplayEnd
        {
            get { return end + addMax; }
            set { this.RaisePropertyChanged(); }
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
        }

        private int addMin;
        private int addMax;

        public string DisplayValue
        {
            get { return conv.GetString(val); }
        }

        private StringConv conv;

        public SpawnRangeListElement(StringConv conv, int addMin, int addMax, int start, int end, int weight, object val)
        {
            this.conv = conv;
            this.addMin = addMin;
            this.addMax = addMax;
            this.start = start;
            this.end = end;
            this.weight = weight;
            this.val = val;
        }
    }

    public class SpawnRangeListBoxViewModel : ViewModelBase
    {
        public delegate void EditElementOp(int index, object element);
        public delegate void ElementOp(int index, object element, bool advancedEdit, EditElementOp op);

        public StringConv StringConv;

        private Window parent;

        public event ElementOp OnEditItem;

        public bool Index1;
        public bool Inclusive;

        public int AddMin
        {
            get { return Index1 ? 1 : 0; }
        }
        public int AddMax
        {
            get
            {
                int result = Index1 ? 1 : 0;
                if (Inclusive)
                    result -= 1;
                return result;
            }
        }

        public bool ConfirmDelete;

        public SpawnRangeListBoxViewModel(Window parent, StringConv conv)
        {
            StringConv = conv;
            this.parent = parent;
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
                    settingRange = true;
                    CurrentStart = Collection[currentElement].DisplayStart;
                    CurrentEnd = Collection[currentElement].DisplayEnd;
                    settingRange = false;
                }
                else
                {
                    CurrentWeight = 1;
                    settingRange = true;
                    CurrentStart = 0 + AddMin;
                    CurrentEnd = 1 + AddMax;
                    settingRange = false;
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
                    Collection[currentElement].Start = currentStart - AddMin;
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
                    Collection[currentElement].End = currentEnd - AddMax;
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
                Collection.Add(new SpawnRangeListElement(StringConv, AddMin, AddMax, range.Min, range.Max, rate, obj));
            }
        }


        private void editItem(int index, object element)
        {
            index = Math.Min(Math.Max(0, index), Collection.Count);
            Collection[index] = new SpawnRangeListElement(StringConv, AddMin, AddMax, Collection[index].Start, Collection[index].End, Collection[index].Weight, element);
            CurrentElement = index;
        }

        private void insertItem(int index, object element)
        {
            index = Math.Min(Math.Max(0, index), Collection.Count + 1);
            Collection.Insert(index, new SpawnRangeListElement(StringConv, AddMin, AddMax, 0, 1, 10, element));
            CurrentElement = index;
        }

        public void InsertOnKey(int index, object element)
        {
            int start = 0;
            int end = 1;
            int rate = 10;
            if (0 <= index && index < Collection.Count)
            {
                start = Collection[index].Start;
                end = Collection[index].End;
                rate = Collection[index].Weight;
            }
            index = Math.Min(Math.Max(0, index), Collection.Count + 1);
            Collection.Insert(index, new SpawnRangeListElement(StringConv, AddMin, AddMax, start, end, rate, element));
            CurrentElement = index;
        }

        public void gridCollection_DoubleClick(object sender, PointerReleasedEventArgs e)
        {
            //int index = lbxCollection.IndexFromPoint(e.X, e.Y);
            int index = CurrentElement;
            if (index > -1)
            {
                SpawnRangeListElement element = Collection[index];
                KeyModifiers modifiers = e.KeyModifiers;
                bool advancedEdit = modifiers.HasFlag(KeyModifiers.Shift);
                OnEditItem?.Invoke(index, element.Value, advancedEdit, editItem);
            }
        }


        public void btnAdd_Click(bool advancedEdit)
        {
            int index = CurrentElement;
            if (index < 0)
                index = Collection.Count;
            object element = null;
            OnEditItem?.Invoke(index, element, advancedEdit, insertItem);
        }

        private async void btnDelete_Click()
        {
            if (CurrentElement > -1 && CurrentElement < Collection.Count)
            {
                if (ConfirmDelete)
                {
                    MessageBox.MessageBoxResult result = await MessageBox.Show(parent, "Are you sure you want to delete this item:\n" + Collection[currentElement].DisplayValue, "Confirm Delete",
                        MessageBox.MessageBoxButtons.YesNo);
                    if (result == MessageBox.MessageBoxResult.No)
                        return;
                }

                Collection.RemoveAt(CurrentElement);
            }
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

        bool settingRange;
        public void AdjustOtherLimit(int newVal, bool changeEnd)
        {
            int newStart = CurrentStart;
            int newEnd = CurrentEnd;

            if (changeEnd)
                newEnd = newVal;
            else
                newStart = newVal;

            if (!settingRange && newEnd < newStart)
            {
                if (changeEnd)
                    CurrentStart = newEnd;
                else
                    CurrentEnd = newStart;
            }
        }

    }
}
