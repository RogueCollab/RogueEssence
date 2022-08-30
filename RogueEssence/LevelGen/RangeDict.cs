using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using RogueElements;

namespace RogueEssence.LevelGen
{
    [Serializable]
    public class RangeDict<T> : IRangeDict<T>, IRangeDict
    {
        //TODO: make this use a binary tree for O(log(n)) access time

        private readonly List<RangeNode> nodes;

        public RangeDict()
        {
            nodes = new List<RangeNode>();
        }

        public void Clear()
        {
            nodes.Clear();
        }

        public void SetRange(T item, IntRange range)
        {
            EraseRange(range);
            nodes.Add(new RangeNode(item, range));
        }

        void IRangeDict.SetRange(object item, IntRange range)
        {
            SetRange((T)item, range);
        }

        public void EraseRange(IntRange range)
        {
            for (int ii = nodes.Count - 1; ii >= 0; ii--)
            {
                if (range.Min <= nodes[ii].Range.Min && nodes[ii].Range.Max <= range.Max)
                    nodes.RemoveAt(ii);
                else if (nodes[ii].Range.Min < range.Min && range.Max < nodes[ii].Range.Max)
                {
                    nodes[ii] = new RangeNode(nodes[ii].Item, new IntRange(nodes[ii].Range.Min, range.Min));
                    nodes.Insert(ii+1, new RangeNode(nodes[ii].Item, new IntRange(range.Max, nodes[ii].Range.Max)));
                }
                else if (range.Min <= nodes[ii].Range.Min && nodes[ii].Range.Min < range.Max)
                    nodes[ii] = new RangeNode(nodes[ii].Item, new IntRange(range.Max, nodes[ii].Range.Max));
                else if (range.Min < nodes[ii].Range.Max && nodes[ii].Range.Max <= range.Max)
                    nodes[ii] = new RangeNode(nodes[ii].Item, new IntRange(nodes[ii].Range.Min, range.Min));
            }
        }

        public T GetItem(int index)
        {
            foreach (RangeNode node in nodes)
            {
                if (node.Range.Min <= index && index < node.Range.Max)
                    return node.Item;
            }
            throw new KeyNotFoundException();
        }

        object IRangeDict.GetItem(int index)
        {
            return GetItem(index);
        }

        public bool TryGetItem(int index, out T item)
        {
            foreach (RangeNode node in nodes)
            {
                if (node.Range.Min <= index && index < node.Range.Max)
                {
                    item = node.Item;
                    return true;
                }
            }
            item = default(T);
            return false;
        }

        public IEnumerable<IntRange> EnumerateRanges()
        {
            foreach (RangeNode node in nodes)
                yield return node.Range;
        }

        public T this[int index]
        {
            get { return GetItem(index); }
        }

        public bool ContainsItem(int index)
        {
            foreach (RangeNode node in nodes)
            {
                if (node.Range.Min <= index && index < node.Range.Max)
                    return true;
            }
            return false;
        }

        [Serializable]
        private struct RangeNode
        {
            public T Item;
            public IntRange Range;

            public RangeNode(T item, IntRange range)
            {
                this.Item = item;
                this.Range = range;
            }
        }
    }



    public interface IRangeDict<T> 
    {
        void Clear();

        void SetRange(T item, IntRange range);
        void EraseRange(IntRange range);
        T GetItem(int index);
        bool ContainsItem(int index);

        IEnumerable<IntRange> EnumerateRanges();
    }

    public interface IRangeDict
    {
        void Clear();

        void SetRange(object item, IntRange range);
        void EraseRange(IntRange range);
        object GetItem(int index);
        bool ContainsItem(int index);

        IEnumerable<IntRange> EnumerateRanges();
    }
}
