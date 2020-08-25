using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Drawing;
using System.Text;

namespace RogueEssence.Dungeon
{
    [Serializable]
    public abstract class GameplayState
    {
        public T Clone<T>() where T : GameplayState { return (T)Clone(); }
        public abstract GameplayState Clone();
    }

    [Serializable]
    public class StateCollection<T> : IEnumerable<T> where T : GameplayState
    {
        [NonSerialized]
        private Dictionary<string, T> pointers;


        public StateCollection()
        {
            pointers = new Dictionary<string, T>();
        }
        protected StateCollection(StateCollection<T> other) : this()
        {
            foreach (string key in other.pointers.Keys)
                pointers[key] = (T)other.pointers[key].Clone();
        }
        public StateCollection<T> Clone() { return new StateCollection<T>(this); }

        public void Clear()
        {
            pointers.Clear();
        }

        public bool Contains<K>() where K : T
        {
            Type type = typeof(K);
            return Contains(type);
        }

        public bool Contains(Type type)
        {
            return pointers.ContainsKey(type.FullName);
        }
        public bool Contains(string typeFullName)
        {
            return pointers.ContainsKey(typeFullName);
        }

        public K Get<K>() where K : T
        {
            Type type = typeof(K);
            T state;
            if (pointers.TryGetValue(type.FullName, out state))
                return (K)state;
            return default(K);
        }

        public T Get(Type type)
        {
            T state;
            if (pointers.TryGetValue(type.FullName, out state))
                return state;
            return default(T);
        }

        public void Set(T state)
        {
            pointers[state.GetType().FullName] = state;
        }

        public void Remove<K>() where K : T
        {
            Type type = typeof(K);
            Remove(type);
        }

        public void Remove(Type type)
        {
            pointers.Remove(type.FullName);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator() { return pointers.Values.GetEnumerator(); }
        IEnumerator IEnumerable.GetEnumerator() { return pointers.Values.GetEnumerator(); }


        public override string ToString()
        {
            if (pointers.Count == 0)
                return "[Empty " + typeof(T).ToString() + "s]";
            StringBuilder builder = new StringBuilder();
            int count = 0;
            int total = pointers.Count;
            foreach (T value in pointers.Values)
            {
                if (count == 3)
                {
                    builder.Append("...");
                    break;
                }
                builder.Append(value.ToString());
                count++;
                if (count < total)
                    builder.Append(", ");
            }
            return builder.ToString();
        }

        private List<T> serializationObjects;

        [OnSerializing]
        internal void OnSerializingMethod(StreamingContext context)
        {
            serializationObjects = new List<T>();
            foreach(string key in pointers.Keys)
                serializationObjects.Add(pointers[key]);
        }

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            pointers = new Dictionary<string, T>();
            for (int ii = 0; ii < serializationObjects.Count; ii++)
                pointers[serializationObjects[ii].GetType().FullName] = serializationObjects[ii];
        }
    }
}
