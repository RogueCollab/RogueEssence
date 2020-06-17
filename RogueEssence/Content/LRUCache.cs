using System.Collections.Generic;

namespace RogueElements
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
        LinkedList<LRUNode> lruList;

        public delegate void ItemRemovedEvent(V value);
        public ItemRemovedEvent OnItemRemoved;

        public LRUCache(int capacity)
        {
            this.capacity = capacity;
            cacheMap = new Dictionary<K, LinkedListNode<LRUNode>>();
            lruList = new LinkedList<LRUNode>();
        }

        public void Add(K key, V val)
        {
            if (cacheMap.Count >= capacity)
            {
                remove();
            }
            LRUNode cacheItem = new LRUNode(key, val);
            LinkedListNode<LRUNode> node = new LinkedListNode<LRUNode>(cacheItem);
            lruList.AddLast(node);
            cacheMap.Add(key, node);
        }

        public bool TryGetValue(K key, out V val)
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

        public void Clear()
        {
            while (lruList.Count > 0)
                remove();
        }

        protected void remove()
        {
            LinkedListNode<LRUNode> node = lruList.First;
            lruList.RemoveFirst();

            cacheMap.Remove(node.Value.key);
            if (OnItemRemoved != null)
                OnItemRemoved(node.Value.value);
        }
    }
}