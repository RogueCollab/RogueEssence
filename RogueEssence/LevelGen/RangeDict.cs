using System;
using System.Collections.Generic;
using System.Text;
using RogueElements;

namespace RogueEssence.LevelGen
{
    [Serializable]
    public class RangeDict<T>
    {
        //TODO: make this use a binary tree for O(log(n)) access time

        private readonly List<RangeNode> nodes;

        public RangeDict()
        {
            nodes = new List<RangeNode>();
        }

        public void SetRange(T item, IntRange range)
        {
            EraseRange(range);
            nodes.Add(new RangeNode(item, range));
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
                    nodes.Add(new RangeNode(nodes[ii].Item, new IntRange(range.Max, nodes[ii].Range.Max)));
                }
                else if (range.Min < nodes[ii].Range.Min && range.Max < nodes[ii].Range.Max)
                    nodes[ii] = new RangeNode(nodes[ii].Item, new IntRange(range.Max, nodes[ii].Range.Max));
                else if (nodes[ii].Range.Min < range.Min && nodes[ii].Range.Max <= range.Max)
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
}
