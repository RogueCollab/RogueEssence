using System;
using RogueElements;
using RogueEssence.Dungeon;
using RogueEssence.Dev;
using System.Collections.Generic;

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

        public static List<(object, double)> CollapseSpawnDict<K, V>(SpawnDict<K, SpawnList<V>> spawns)
        {
            List<(object, double)> flatList = new List<(object, double)>();

            foreach (K key in spawns.GetKeys())
            {

                SpawnList<V> list = spawns.GetSpawn(key);
                foreach (SpawnList<V>.SpawnRate spawn in list)
                {
                    V item = spawn.Spawn;
                    double totalRate = (double)spawn.Rate / list.SpawnTotal * spawns.GetSpawnRate(key) / spawns.SpawnTotal;
                    flatList.Add((item, totalRate));
                }
            }

            return flatList;
        }
    }
}
