using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Reactive.Subjects;
using Avalonia.VisualTree;

namespace RogueEssence.Dev.Views
{
    public class DictionaryElement
    {
        private object key;
        public object Key
        {
            get { return key; }
        }
        private object value;
        public object Value
        {
            get { return value; }
        }

        public DictionaryElement(object key, object value)
        {
            this.key = key;
            this.value = value;
        }
    }

    public class DictionaryBox : UserControl
    {
        private ObservableCollection<DictionaryElement> collection;

        private int SelectedIndex
        {
            get { return gridCollection.SelectedIndex; }
            set { gridCollection.SelectedIndex = value; }
        }


        public delegate void EditElementOp(object key, object element);
        public delegate void ElementOp(object key, object element, EditElementOp op);

        public ElementOp OnEditKey;
        public ElementOp OnEditItem;

        private DataGrid gridCollection;


        public DictionaryBox()
        {
            this.InitializeComponent();
            gridCollection = this.FindControl<DataGrid>("gridItems");
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public IDictionary GetDict(Type type)
        {
            IDictionary result = (IDictionary)Activator.CreateInstance(type);
            foreach (DictionaryElement item in collection)
                result.Add(item.Key, item.Value);
            return result;
        }

        public void LoadFromDict(IDictionary source)
        {
            collection = new ObservableCollection<DictionaryElement>();
            foreach (object obj in source.Keys)
                collection.Add(new DictionaryElement(obj, source[obj]));

            //bind the collection
            var subject = new Subject<ObservableCollection<DictionaryElement>>();
            gridCollection.Bind(DataGrid.ItemsProperty, subject);
            subject.OnNext(collection);
        }






        private void editItem(object key, object element)
        {
            int index = getIndexFromKey(key);
            collection[index] = new DictionaryElement(collection[index].Key, element);
        }

        private async void insertKey(object key, object element)
        {
            int index = getIndexFromKey(key);
            if (index > -1)
            {
                await MessageBox.Show(this.GetOwningForm(), "Dictionary already contains this key!", "Error", MessageBox.MessageBoxButtons.Ok);
                return;
            }
            OnEditItem(key, element, insertItem);
        }

        private void insertItem(object key, object element)
        {
            collection.Add(new DictionaryElement(key, element));
        }

        private int getIndexFromKey(object key)
        {
            int curIndex = 0;
            foreach (DictionaryElement item in collection)
            {
                if (item.Key.Equals(key))
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
                DictionaryElement item = collection[index];
                OnEditItem(item.Key, item.Value, editItem);
            }
        }

        public void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            object newKey = null;
            object element = null;
            OnEditKey(newKey, element, insertKey);
        }

        public void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedIndex > -1)
                collection.RemoveAt(SelectedIndex);
        }
    }
}
