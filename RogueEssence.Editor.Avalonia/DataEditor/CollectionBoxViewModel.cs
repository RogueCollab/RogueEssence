using System;
using System.Collections.Generic;
using System.Text;
using ReactiveUI;
using System.Collections.ObjectModel;
using Avalonia.Interactivity;
using Avalonia.Controls;
using RogueElements;
using System.Collections;
using Avalonia.Data.Converters;
using RogueEssence.Dev.Views;

namespace RogueEssence.Dev.ViewModels
{
    public class ListElement : ViewModelBase
    {
        private int key;
        public int Key
        {
            get { return key; }
            set
            {
                key = value;
                DisplayKey = DisplayKey;
            }
        }

        public int DisplayKey
        {
            get { return key + addIndex; }
            set { this.RaisePropertyChanged(); }
        }

        private object val;
        public object Value
        {
            get { return val; }
        }

        private int addIndex;

        public string DisplayValue
        {
            get { return conv.GetString(val); }
        }

        private StringConv conv;

        public ListElement(StringConv conv, int addIndex, int key, object val)
        {
            this.addIndex = addIndex;
            this.key = key;
            this.conv = conv;
            this.val = val;
        }
    }

    public class CollectionBoxViewModel : ViewModelBase
    {
        public ObservableCollection<ListElement> Collection { get; }

        private int selectedIndex;
        public int SelectedIndex
        {
            get { return selectedIndex; }
            set { this.SetIfChanged(ref selectedIndex, value); }
        }

        public StringConv StringConv;

        private Window parent;


        public delegate void EditElementOp(int index, object element);
        public delegate void ElementOp(int index, object element, bool advancedEdit, EditElementOp op);

        public event ElementOp OnEditItem;
        public event Action OnMemberChanged;

        public bool Index1;
        public int AddIndex { get { return Index1 ? 1 : 0; } }

        public bool ConfirmDelete;

        public CollectionBoxViewModel(Window parent, StringConv conv)
        {
            StringConv = conv;
            this.parent = parent;
            Collection = new ObservableCollection<ListElement>();
            SelectedIndex = -1;
        }


        public T GetList<T>() where T : IList
        {
            return (T)GetList(typeof(T));
        }

        public IList GetList(Type type)
        {
            IList result = (IList)Activator.CreateInstance(type);
            foreach (ListElement obj in Collection)
                result.Add(obj.Value);
            return result;
        }

        public void LoadFromList(IList source)
        {
            Collection.Clear();
            foreach (object obj in source)
                Collection.Add(new ListElement(StringConv, AddIndex, Collection.Count, obj));
        }


        private void editItem(int index, object element)
        {
            index = Math.Min(Math.Max(0, index), Collection.Count);
            Collection[index] = new ListElement(StringConv, AddIndex, index, element);
            SelectedIndex = index;
            OnMemberChanged?.Invoke();
        }

        public void InsertItem(int index, object element)
        {
            index = Math.Min(Math.Max(0, index), Collection.Count + 1);
            Collection.Insert(index, new ListElement(StringConv, AddIndex, index, element));
            for (int ii = index + 1; ii < Collection.Count; ii++)
                Collection[ii].Key = ii;
            SelectedIndex = index;
            OnMemberChanged?.Invoke();
        }


        public void lbxCollection_DoubleClick(object sender, RoutedEventArgs e)
        {
            //int index = lbxCollection.IndexFromPoint(e.X, e.Y);
            int index = SelectedIndex;
            bool advancedEdit = false;
            if (index > -1)
            {
                object element = Collection[index].Value;
                OnEditItem?.Invoke(index, element, advancedEdit, editItem);
            }
        }


        private void btnAdd_Click()
        {
            int index = SelectedIndex;
            if (index < 0)
                index = Collection.Count;
            object element = null;
            bool advancedEdit = false;
            OnEditItem?.Invoke(index, element, advancedEdit, InsertItem);
        }

        private async void btnDelete_Click()
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

                int index = SelectedIndex;
                Collection.RemoveAt(SelectedIndex);
                for (int ii = index; ii < Collection.Count; ii++)
                    Collection[ii].Key = ii;
                OnMemberChanged?.Invoke();
            }
        }

        private void Switch(int a, int b)
        {
            ListElement obj = Collection[a];
            Collection[a] = Collection[b];
            Collection[b] = obj;

            Collection[a].Key = a;
            Collection[b].Key = b;
        }

        private void btnUp_Click()
        {
            if (SelectedIndex > 0)
            {
                int index = SelectedIndex;
                Switch(SelectedIndex, SelectedIndex - 1);
                SelectedIndex = index - 1;
                OnMemberChanged?.Invoke();
            }
        }

        private void btnDown_Click()
        {
            if (SelectedIndex > -1 && SelectedIndex < Collection.Count - 1)
            {
                int index = SelectedIndex;
                Switch(SelectedIndex, SelectedIndex + 1);
                SelectedIndex = index + 1;
                OnMemberChanged?.Invoke();
            }
        }
    }
}
