using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Drawing;
using System.Text;
using RogueElements;

namespace RogueEssence.Dungeon
{
    [Serializable]
    public abstract class GameplayState
    {
        public T Clone<T>() where T : GameplayState { return (T)Clone(); }
        public abstract GameplayState Clone();
    }

    [Serializable]
    public class StateCollection<T> : TypeDict<T> where T : GameplayState
    {
        public StateCollection() { }

        protected StateCollection(StateCollection<T> other) : this()
        {
            foreach (T obj in other)
                Set((T)obj.Clone());
        }
        public StateCollection<T> Clone() { return new StateCollection<T>(this); }


        public bool ContainsName(string typeFullName)
        {
            return Contains(Type.GetType(typeFullName));
        }

        public K GetWithDefault<K>() where K : T
        {
            K state;
            if (TryGet(out state))
                return state;
            return default;
        }


        public override string ToString()
        {
            if (Count == 0)
                return "[Empty " + typeof(T).ToString() + "s]";
            StringBuilder builder = new StringBuilder();
            int count = 0;
            int total = Count;
            foreach (T value in this)
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
    }


    public interface IStateCollection : IEnumerable
    {

        void Set(object value);
        object Get(Type type);

        void Clear();

        bool Contains(Type type);

        void Remove(Type type);
    }

}
