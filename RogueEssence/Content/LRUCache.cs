using System.Collections.Generic;

namespace RogueEssence
{
    public class LRUCache<K, V>
    {
        private class LRUNode
        {
            public K key;
            public V value;

            public LRUNode(K k, V v)
            {
                key = k;
                value = v;
            }

        }

        Dictionary<K, LinkedListNode<LRUNode>> cacheMap;
        int capacity;
        int total;
        LinkedList<LRUNode> lruList;

        public delegate void ItemRemovedEvent(V value);
        public ItemRemovedEvent OnItemRemoved;

        public delegate int ItemCountMethod(V value);
        public ItemCountMethod ItemCount;

        private object lockObj = new object();

        public LRUCache(int capacity)
        {
            this.capacity = capacity;
            ItemCount = defaultCount;
            cacheMap = new Dictionary<K, LinkedListNode<LRUNode>>();
            lruList = new LinkedList<LRUNode>();
        }

        public void Add(K key, V val)
        {
            lock (lockObj)
            {
                while (total >= capacity)
                {
                    remove();
                }
                LRUNode cacheItem = new LRUNode(key, val);
                LinkedListNode<LRUNode> node = new LinkedListNode<LRUNode>(cacheItem);
                lruList.AddLast(node);
                cacheMap.Add(key, node);
                total += ItemCount(val);
            }
        }

        public bool TryGetValue(K key, out V val)
        {
            lock (lockObj)
            {
                LinkedListNode<LRUNode> node;
                if (cacheMap.TryGetValue(key, out node))
                {
                    val = node.Value.value;

                    lruList.Remove(node);
                    lruList.AddLast(node);
                    return true;
                }
                else
                {
                    val = default(V);
                    return false;
                }
            }
        }

        public void Clear()
        {
            lock (lockObj)
            {
                while (lruList.Count > 0)
                    remove();
            }
        }

        protected void remove()
        {
            LinkedListNode<LRUNode> node = lruList.First;
            lruList.RemoveFirst();

            cacheMap.Remove(node.Value.key);
            total -= ItemCount(node.Value.value);
            OnItemRemoved?.Invoke(node.Value.value);
        }

        private int defaultCount(V val)
        {
            return 1;
        }
    }
}