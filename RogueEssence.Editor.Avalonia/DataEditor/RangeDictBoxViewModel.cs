using System;
using System.Collections.Generic;
using System.Text;
using ReactiveUI;
using System.Collections.ObjectModel;
using Avalonia.Interactivity;
using Avalonia.Controls;
using RogueElements;
using System.Collections;
using RogueEssence.Dev.Views;
using RogueEssence.LevelGen;
using System.Linq;

namespace RogueEssence.Dev.ViewModels
{
    public class RangeDictElement : ViewModelBase
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

        //TODO: the separation of display vs. internal value can be offloaded
        //to the already existing converter system
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

        private object value;
        public object Value
        {
            get { return value; }
        }

        private int addMin;
        private int addMax;

        public string DisplayValue
        {
            get { return conv.GetString(value); }
        }

        private StringConv conv;

        public RangeDictElement(StringConv conv, int addMin, int addMax, int start, int end, object value)
        {
            this.conv = conv;
            this.addMin = addMin;
            this.addMax = addMax;
            this.start = start;
            this.end = end;
            this.value = value;
        }
    }

    public class RangeDictBoxViewModel : ViewModelBase
    {
        public ObservableCollection<RangeDictElement> Collection { get; }

        private int currentElement;
        public int CurrentElement
        {
            get { return currentElement; }
            set
            {
                this.SetIfChanged(ref currentElement, value);
                if (currentElement > -1)
                {
                    CurrentStart = Collection[currentElement].DisplayStart;
                    CurrentEnd = Collection[currentElement].DisplayEnd;
                }
                else
                {
                    CurrentStart = 0 + AddMin;
                    CurrentEnd = 1 + AddMax;
                }
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
                {
                    Collection[currentElement].Start = currentStart - AddMin;
                    EraseRange(new IntRange(Collection[currentElement].Start, Collection[currentElement].End), currentElement);
                }
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
                {
                    Collection[currentElement].End = currentEnd - AddMax;
                    EraseRange(new IntRange(Collection[currentElement].Start, Collection[currentElement].End), currentElement);
                }
            }
        }


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

        public delegate void EditElementOp(IntRange key, object element);
        public delegate void ElementOp(IntRange key, object element, bool advancedEdit, EditElementOp op);

        public event ElementOp OnEditKey;
        public event ElementOp OnEditItem;
        public event Action OnMemberChanged;

        public StringConv StringConv;

        private Window parent;

        public bool ConfirmDelete;

        public RangeDictBoxViewModel(Window parent, StringConv conv)
        {
            StringConv = conv;
            this.parent = parent;
            Collection = new ObservableCollection<RangeDictElement>();
        }

        public T GetDict<T>() where T : IRangeDict
        {
            return (T)GetDict(typeof(T));
        }

        public IRangeDict GetDict(Type type)
        {
            IRangeDict result = (IRangeDict)Activator.CreateInstance(type);
            foreach (RangeDictElement item in Collection)
                result.SetRange(item.Value, new IntRange(item.Start, item.End));
            return result;
        }

        public void LoadFromDict(IRangeDict source)
        {
            Collection.Clear();
            foreach (IntRange obj in source.EnumerateRanges())
            {
                for (int ii = 0; ii <= Collection.Count; ii++)
                {
                    if (ii == Collection.Count || obj.Min < Collection[ii].Start)
                    {
                        Collection.Insert(ii, new RangeDictElement(StringConv, AddMin, AddMax, obj.Min, obj.Max, source.GetItem(obj.Min)));
                        break;
                    }
                }
            }
        }



        private void editItem(IntRange key, object element)
        {
            int index = getIndexFromKey(key);
            Collection[index] = new RangeDictElement(StringConv, AddMin, AddMax, Collection[index].Start, Collection[index].End, element);
            CurrentElement = index;
            OnMemberChanged?.Invoke();
        }

        private void insertKey(IntRange key, object element)
        {
            bool advancedEdit = false;
            OnEditItem(key, element, advancedEdit, insertItem);
        }

        private void insertItem(IntRange key, object element)
        {
            EraseRange(key, -1);
            for (int ii = 0; ii <= Collection.Count; ii++)
            {
                if (ii == Collection.Count || key.Min < Collection[ii].Start)
                {
                    Collection.Insert(ii, new RangeDictElement(StringConv, AddMin, AddMax, key.Min, key.Max, element));
                    CurrentElement = ii;
                    break;
                }
            }
            OnMemberChanged?.Invoke();
        }

        public void InsertOnKey(int index, object element)
        {
            IntRange key = new IntRange(0);
            if (0 <= index && index < Collection.Count)
            {
                key = new IntRange(Collection[index].End);
            }

            EraseRange(key, -1);
            for (int ii = 0; ii <= Collection.Count; ii++)
            {
                if (ii == Collection.Count || key.Min < Collection[ii].Start)
                {
                    Collection.Insert(ii, new RangeDictElement(StringConv, AddMin, AddMax, key.Min, key.Max, element));
                    CurrentElement = ii;
                    break;
                }
            }
            OnMemberChanged?.Invoke();
        }

        private void EraseRange(IntRange range, int exceptionIdx)
        {
            for (int ii = Collection.Count - 1; ii >= 0; ii--)
            {
                if (exceptionIdx == ii)
                    continue;
                if (range.Min <= Collection[ii].Start && Collection[ii].End <= range.Max)
                    Collection.RemoveAt(ii);
                else if (Collection[ii].Start < range.Min && range.Max < Collection[ii].End)
                {
                    Collection[ii] = new RangeDictElement(StringConv, AddMin, AddMax, Collection[ii].Start, range.Min, Collection[ii].Value);
                    Collection.Insert(ii+1, new RangeDictElement(StringConv, AddMin, AddMax, range.Max, Collection[ii].End, Collection[ii].Value));
                }
                else if (range.Min <= Collection[ii].Start && Collection[ii].Start < range.Max)
                    Collection[ii] = new RangeDictElement(StringConv, AddMin, AddMax, range.Max, Collection[ii].End, Collection[ii].Value);
                else if (range.Min < Collection[ii].End && Collection[ii].End <= range.Max)
                    Collection[ii] = new RangeDictElement(StringConv, AddMin, AddMax, Collection[ii].Start, range.Min, Collection[ii].Value);
            }
        }

        private int getIndexFromKey(IntRange key)
        {
            int curIndex = 0;
            foreach (RangeDictElement item in Collection)
            {
                if (item.Start == key.Min && item.End == key.Max)
                    return curIndex;
                curIndex++;
            }
            return -1;
        }


        public void lbxCollection_DoubleClick(object sender, RoutedEventArgs e)
        {
            //int index = lbxDictionary.IndexFromPoint(e.X, e.Y);
            int index = CurrentElement;
            if (index > -1)
            {
                RangeDictElement item = Collection[index];
                bool advancedEdit = false;
                OnEditItem?.Invoke(new IntRange(item.Start, item.End), item.Value, advancedEdit, editItem);
            }
        }

        public void btnAdd_Click()
        {
            IntRange newKey = new IntRange(0);
            object element = null;
            bool advancedEdit = false;
            OnEditKey?.Invoke(newKey, element, advancedEdit, insertKey);
        }

        public async void btnDelete_Click()
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
                OnMemberChanged?.Invoke();
            }
        }
    }
}
