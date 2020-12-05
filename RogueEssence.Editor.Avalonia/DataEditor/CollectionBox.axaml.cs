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
    public class CollectionBox : UserControl
    {
        private ObservableCollection<object> collection;

        public int SelectedIndex
        {
            get { return lbxCollection.SelectedIndex; }
            set { lbxCollection.SelectedIndex = value; }
        }

        public delegate void EditElementOp(int index, object element);
        public delegate void ElementOp(int index, object element, EditElementOp op);

        public ElementOp OnEditItem;

        private ListBox lbxCollection;

        public CollectionBox()
        {
            this.InitializeComponent();
            lbxCollection = this.FindControl<ListBox>("lbxItems");
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public IList GetList(Type type)
        {
            IList result = (IList)Activator.CreateInstance(type);
            foreach (object obj in collection)
                result.Add(obj);
            return result;
        }

        public void LoadFromList(IList source)
        {
            collection = new ObservableCollection<object>();
            foreach (object obj in source)
                collection.Add(obj);

            //bind the collection
            var subject = new Subject<ObservableCollection<object>>();
            lbxCollection.Bind(ComboBox.ItemsProperty, subject);
            subject.OnNext(collection);
        }


        private void editItem(int index, object element)
        {
            index = Math.Min(Math.Max(0, index), collection.Count);
            collection[index] = element;
        }

        private void insertItem(int index, object element)
        {
            index = Math.Min(Math.Max(0, index), collection.Count + 1);
            collection.Insert(index, element);
        }


        public void lbxCollection_DoubleClick(object sender, RoutedEventArgs e)
        {
            //int index = lbxCollection.IndexFromPoint(e.X, e.Y);
            int index = lbxCollection.SelectedIndex;
            if (index > -1)
            {
                object element = collection[index];
                OnEditItem(index, element, editItem);
            }
        }


        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            int index = lbxCollection.SelectedIndex;
            if (index < 0)
                index = collection.Count;
            object element = null;
            OnEditItem(index, element, insertItem);
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (lbxCollection.SelectedIndex > -1)
            {
                collection.RemoveAt(lbxCollection.SelectedIndex);
            }
        }

        private void Switch(int a, int b)
        {
            object obj = collection[a];
            collection[a] = collection[b];
            collection[b] = obj;
        }

        private void btnUp_Click(object sender, RoutedEventArgs e)
        {
            if (lbxCollection.SelectedIndex > 0)
            {
                int index = lbxCollection.SelectedIndex;
                Switch(lbxCollection.SelectedIndex, lbxCollection.SelectedIndex - 1);
                lbxCollection.SelectedIndex = index - 1;
            }
        }

        private void btnDown_Click(object sender, RoutedEventArgs e)
        {
            if (lbxCollection.SelectedIndex > -1 && lbxCollection.SelectedIndex < collection.Count - 1)
            {
                int index = lbxCollection.SelectedIndex;
                Switch(lbxCollection.SelectedIndex, lbxCollection.SelectedIndex + 1);
                lbxCollection.SelectedIndex = index + 1;
            }
        }
    }
}
