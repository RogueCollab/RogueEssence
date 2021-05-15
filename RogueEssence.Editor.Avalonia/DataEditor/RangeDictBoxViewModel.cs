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

        public RangeDictElement(int addMin, int addMax, int start, int end, object value)
        {
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

        private int selectedIndex;
        public int SelectedIndex
        {
            get { return selectedIndex; }
            set { this.SetIfChanged(ref selectedIndex, value); }
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
        public delegate void ElementOp(IntRange key, object element, EditElementOp op);

        public event ElementOp OnEditKey;
        public event ElementOp OnEditItem;
        public event Action OnMemberChanged;

        private Window parent;

        public RangeDictBoxViewModel(Window parent)
        {
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
                        Collection.Insert(ii, new RangeDictElement(AddMin, AddMax, obj.Min, obj.Max, source.GetItem(obj.Min)));
                        break;
                    }
                }
            }
        }



        private void editItem(IntRange key, object element)
        {
            int index = getIndexFromKey(key);
            Collection[index] = new RangeDictElement(AddMin, AddMax, Collection[index].Start, Collection[index].End, element);
            OnMemberChanged?.Invoke();
        }

        private void insertKey(IntRange key, object element)
        {
            OnEditItem(key, element, insertItem);
        }

        private void insertItem(IntRange key, object element)
        {
            EraseRange(key);
            for (int ii = 0; ii <= Collection.Count; ii++)
            {
                if (ii == Collection.Count || key.Min < Collection[ii].Start)
                {
                    Collection.Insert(ii, new RangeDictElement(AddMin, AddMax, key.Min, key.Max, element));
                    break;
                }
            }
            OnMemberChanged?.Invoke();
        }

        private void EraseRange(IntRange range)
        {
            for (int ii = Collection.Count - 1; ii >= 0; ii--)
            {
                if (range.Min <= Collection[ii].Start && Collection[ii].End <= range.Max)
                    Collection.RemoveAt(ii);
                else if (Collection[ii].Start < range.Min && range.Max < Collection[ii].End)
                {
                    Collection[ii] = new RangeDictElement(AddMin, AddMax, Collection[ii].Start, range.Min, Collection[ii].Value);
                    Collection.Add(new RangeDictElement(AddMin, AddMax, range.Max, Collection[ii].End, Collection[ii].Value));
                }
                else if (range.Min <= Collection[ii].Start && range.Max < Collection[ii].End)
                    Collection[ii] = new RangeDictElement(AddMin, AddMax, range.Max, Collection[ii].End, Collection[ii].Value);
                else if (Collection[ii].Start < range.Min && Collection[ii].End <= range.Max)
                    Collection[ii] = new RangeDictElement(AddMin, AddMax, Collection[ii].Start, range.Min, Collection[ii].Value);
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
            int index = SelectedIndex;
            if (index > -1)
            {
                RangeDictElement item = Collection[index];
                OnEditItem?.Invoke(new IntRange(item.Start, item.End), item.Value, editItem);
            }
        }

        public void btnAdd_Click()
        {
            IntRange newKey = new IntRange(0);
            object element = null;
            OnEditKey?.Invoke(newKey, element, insertKey);
        }

        public void btnDelete_Click()
        {
            if (SelectedIndex > -1 && SelectedIndex < Collection.Count)
            {
                Collection.RemoveAt(SelectedIndex);
                OnMemberChanged?.Invoke();
            }
        }
    }
}
