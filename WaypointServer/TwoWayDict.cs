using System.Collections.Generic;

namespace WaypointServer
{
    public class TwoWayDict<T1, T2>
    {
        private Dictionary<T1, T2> _forward = new Dictionary<T1, T2>();
        private Dictionary<T2, T1> _reverse = new Dictionary<T2, T1>();

        public TwoWayDict()
        {
            this.Forward = new Indexer<T1, T2>(_forward);
            this.Reverse = new Indexer<T2, T1>(_reverse);
        }

        public class Indexer<T3, T4>
        {
            private Dictionary<T3, T4> _dictionary;
            public Indexer(Dictionary<T3, T4> dictionary)
            {
                _dictionary = dictionary;
            }
            public T4 this[T3 index]
            {
                get { return _dictionary[index]; }
                set { _dictionary[index] = value; }
            }
            public bool Contains(T3 key)
            {
                return _dictionary.ContainsKey(key);
            }
        }

        public void Add(T1 t1, T2 t2)
        {
            _forward.Add(t1, t2);
            _reverse.Add(t2, t1);
        }

        public bool RemoveForward(T1 t1)
        {
            if (!_forward.ContainsKey(t1))
                return false;
            T2 reverse_val = _forward[t1];
            _forward.Remove(t1);
            _reverse.Remove(reverse_val);
            return true;
        }

        public bool RemoveReverse(T2 t2)
        {
            if (!_reverse.ContainsKey(t2))
                return false;
            T1 forward_val = _reverse[t2];
            _reverse.Remove(t2);
            _forward.Remove(forward_val);
            return true;
        }

        public Indexer<T1, T2> Forward { get; private set; }
        public Indexer<T2, T1> Reverse { get; private set; }
    }
}
