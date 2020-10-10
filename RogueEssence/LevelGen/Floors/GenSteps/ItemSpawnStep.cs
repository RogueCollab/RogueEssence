using System;
using RogueElements;
using RogueEssence.Dungeon;
using RogueEssence.Dev;

namespace RogueEssence.LevelGen
{
    /// <summary>
    /// Generates the table of items to spawn on a floor
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class ItemSpawnStep<T> : GenStep<T> where T : BaseMapGenContext
    {
        [SubGroup]
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
}
