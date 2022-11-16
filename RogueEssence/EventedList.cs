using System;
using System.Collections;
using System.Collections.Generic;

namespace RogueEssence
{

    [Serializable]
    public class EventedList<T> : ICollection<T>, IEnumerable<T>, IEnumerable, IList<T>, IReadOnlyCollection<T>, IReadOnlyList<T>, ICollection, IList
    {
        private List<T> list;

        public delegate void EventedListAction(int index, T item);

        public event EventedListAction ItemChanging;
        public event EventedListAction ItemAdding;
        public event EventedListAction ItemRemoving;
        
        public event Action ItemsClearing;

        public T this[int index]
        {
            get => list[index];
            set
            {
                ItemChanging?.Invoke(index, value);
                list[index] = value;
            }
        }
        object IList.this[int index]
        {
            get => list[index];
            set
            {
                ItemChanging?.Invoke(index, (T)value);
                list[index] = (T)value;
            }
        }

        public int Count => list.Count;

        public bool IsReadOnly => ((IList)list).IsReadOnly;
        public bool IsFixedSize => ((IList)list).IsFixedSize;
        public bool IsSynchronized => ((IList)list).IsSynchronized;
        public object SyncRoot => ((IList)list).SyncRoot;

        public EventedList()
        {
            list = new List<T>();
        }

        public void Add(T item)
        {
            ItemAdding?.Invoke(list.Count, item);
            list.Add(item);
        }

        int IList.Add(object value)
        {
            ItemAdding?.Invoke(list.Count, (T)value);
            return ((IList)list).Add(value);
        }

        public void Clear()
        {
            ItemsClearing?.Invoke();
            list.Clear();
        }

        public bool Contains(T item)
        {
            return list.Contains(item);
        }

        bool IList.Contains(object value)
        {
            return Contains((T)value);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            list.CopyTo(array, arrayIndex);
        }

        void ICollection.CopyTo(Array array, int index)
        {
            ((IList)list).CopyTo(array, index);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int IndexOf(T item)
        {
            return list.IndexOf(item);
        }

        int IList.IndexOf(object value)
        {
            return IndexOf((T)value);
        }

        public void Insert(int index, T item)
        {
            ItemAdding?.Invoke(index, item);
            list.Insert(index, item);
        }

        void IList.Insert(int index, object value)
        {
            Insert(index, (T)value);
        }

        public bool Remove(T item)
        {
            int index = IndexOf(item);
            if (index > -1)
            {
                RemoveAt(index);
                return true;
            }
            return false;
        }

        void IList.Remove(object value)
        {
            Remove((T)value);
        }

        public void RemoveAt(int index)
        {
            ItemRemoving?.Invoke(index, list[index]);
            list.RemoveAt(index);
        }
    }
}
