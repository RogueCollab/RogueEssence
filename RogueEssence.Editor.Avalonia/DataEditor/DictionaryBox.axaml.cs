using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Reactive.Subjects;

namespace RogueEssence.Dev.Views
{
    public class DictionaryBox : UserControl
    {
        private ObservableCollection<(object, object)> collection;

        public delegate void EditElementOp(object key, object element);
        public delegate void ElementOp(object key, object element, EditElementOp op);

        public ElementOp OnEditKey;
        public ElementOp OnEditItem;

        private ListBox lbxCollection;


        public DictionaryBox()
        {
            this.InitializeComponent();
            lbxCollection = this.FindControl<ListBox>("lbxItems");
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public IDictionary GetDict(Type type)
        {
            IDictionary result = (IDictionary)Activator.CreateInstance(type);
            foreach ((object, object) item in collection)
                result.Add(item.Item1, item.Item2);
            return result;
        }

        public void LoadFromDict(IDictionary source)
        {
            collection = new ObservableCollection<(object, object)>();
            foreach (object obj in source.Keys)
                collection.Add((obj, source[obj]));

            //bind the collection
            var subject = new Subject<ObservableCollection<(object, object)>>();
            lbxCollection.Bind(ComboBox.ItemsProperty, subject);
            subject.OnNext(collection);
        }






        private void editItem(object key, object element)
        {
            int index = getIndexFromKey(key);
            collection[index] = (collection[index].Item1, element);
        }

        private void insertKey(object key, object element)
        {
            int index = getIndexFromKey(key);
            if (index == -1)
            {
                //TODO: pass in the owner window
                //await MessageBox.Show(this, "Dictionary already contains this key!", "Error", MessageBox.MessageBoxButtons.Ok);
                return;
            }
            OnEditItem(key, element, insertItem);
        }

        private void insertItem(object key, object element)
        {
            collection.Add((key, element));
        }

        private int getIndexFromKey(object key)
        {
            int curIndex = 0;
            foreach ((object, object) item in collection)
            {
                if (item.Item1 == key)
                    return curIndex;
                curIndex++;
            }
            return -1;
        }


        public void lbxCollection_DoubleClick(object sender, RoutedEventArgs e)
        {
            //int index = lbxDictionary.IndexFromPoint(e.X, e.Y);
            int index = lbxCollection.SelectedIndex;
            if (index > -1)
            {
                (object, object) item = collection[index];
                OnEditItem(item.Item1, item.Item2, editItem);
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
            if (lbxCollection.SelectedIndex > -1)
            {
                collection.RemoveAt(lbxCollection.SelectedIndex);
            }
        }
    }
}
