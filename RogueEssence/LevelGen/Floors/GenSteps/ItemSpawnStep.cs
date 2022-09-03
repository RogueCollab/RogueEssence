using System;
using RogueElements;
using RogueEssence.Dungeon;
using RogueEssence.Dev;

namespace RogueEssence.LevelGen
{
    /// <summary>
    /// Sets the floor's item spawn tables.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class ItemSpawnStep<T> : GenStep<T> where T : BaseMapGenContext
    {
        [SubGroup]
        [EditorHeight(0, 360)]
        public SpawnDict<string, SpawnList<InvItem>> Spawns;

        public ItemSpawnStep()
        {
            Spawns = new SpawnDict<string, SpawnList<InvItem>>();
        }

        public override void Apply(T map)
        {
            foreach (string key in Spawns.GetKeys())
            {
                SpawnList<InvItem> itemList = Spawns.GetSpawn(key);
                if (itemList.CanPick)
                {
                    if (!map.ItemSpawns.Spawns.ContainsKey(key))
                        map.ItemSpawns.Spawns.Add(key, new SpawnList<InvItem>(), Spawns.GetSpawnRate(key));

                    SpawnList<InvItem> destList = map.ItemSpawns.Spawns.GetSpawn(key);
                    for (int ii = 0; ii < itemList.Count; ii++)
                        destList.Add(new InvItem(itemList.GetSpawn(ii)), itemList.GetSpawnRate(ii));
                }
            }
        }
    }

    public static class CategorySpawnHelper
    {

        public static SpawnList<V> CollapseSpawnDict<K, V>(SpawnDict<K, SpawnList<V>> spawns)
        {
            SpawnList<V> result = new SpawnList<V>();
            //if you want to flatten this list,
            //the rate of an individual spawn must be multiplied by its category spawn rate
            //and then multiplied by the internal sums of the other categories
            foreach (K key in spawns.GetKeys())
            {
                int internalSumFactor = spawns.GetSpawnRate(key);
                foreach (K key2 in spawns.GetKeys())
                {
                    if (key2.Equals(key))
                        continue;
                    SpawnList<V> otherList = spawns.GetSpawn(key2);
                    internalSumFactor *= otherList.SpawnTotal;
                }

                SpawnList<V> list = spawns.GetSpawn(key);
                foreach (SpawnList<V>.SpawnRate spawn in list)
                {
                    V item = spawn.Spawn;
                    int rate = spawn.Rate;
                    result.Add(item, rate * internalSumFactor);
                }
            }
            return result;
        }
    }
}
