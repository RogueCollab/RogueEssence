using System;
using System.Collections.Generic;
using System.Text;
using ReactiveUI;
using System.Collections.ObjectModel;
using Avalonia.Interactivity;
using Avalonia.Controls;
using RogueElements;
using System.Collections;

namespace RogueEssence.Dev.ViewModels
{
    public class CollectionBoxViewModel : ViewModelBase
    {
        public ObservableCollection<object> Collection { get; }

        private int selectedIndex;
        public int SelectedIndex
        {
            get { return selectedIndex; }
            set { this.SetIfChanged(ref selectedIndex, value); }
        }


        public delegate void EditElementOp(int index, object element);
        public delegate void ElementOp(int index, object element, EditElementOp op);

        public event ElementOp OnEditItem;
        public event Action OnMemberChanged;

        public CollectionBoxViewModel()
        {
            Collection = new ObservableCollection<object>();
        }


        public T GetList<T>() where T : IList
        {
            return (T)GetList(typeof(T));
        }

        public IList GetList(Type type)
        {
            IList result = (IList)Activator.CreateInstance(type);
            foreach (object obj in Collection)
                result.Add(obj);
            return result;
        }

        public void LoadFromList(IList source)
        {
            Collection.Clear();
            foreach (object obj in source)
                Collection.Add(obj);
        }


        private void editItem(int index, object element)
        {
            index = Math.Min(Math.Max(0, index), Collection.Count);
            Collection[index] = element;
            OnMemberChanged?.Invoke();
        }

        private void insertItem(int index, object element)
        {
            index = Math.Min(Math.Max(0, index), Collection.Count + 1);
            Collection.Insert(index, element);
            OnMemberChanged?.Invoke();
        }


        public void lbxCollection_DoubleClick(object sender, RoutedEventArgs e)
        {
            //int index = lbxCollection.IndexFromPoint(e.X, e.Y);
            int index = SelectedIndex;
            if (index > -1)
            {
                object element = Collection[index];
                OnEditItem?.Invoke(index, element, editItem);
            }
        }


        private void btnAdd_Click()
        {
            int index = SelectedIndex;
            if (index < 0)
                index = Collection.Count;
            object element = null;
            OnEditItem?.Invoke(index, element, insertItem);
        }

        private void btnDelete_Click()
        {
            if (SelectedIndex > -1 && SelectedIndex < Collection.Count)
            {
                Collection.RemoveAt(SelectedIndex);
                OnMemberChanged?.Invoke();
            }
        }

        private void Switch(int a, int b)
        {
            object obj = Collection[a];
            Collection[a] = Collection[b];
            Collection[b] = obj;
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
