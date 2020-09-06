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
        public SpawnList<InvItem> Spawns;

        public ItemSpawnStep()
        {
            Spawns = new SpawnList<InvItem>();
        }

        public override void Apply(T map)
        {
            map.ItemSpawns.Clear();
            for (int ii = 0; ii < Spawns.Count; ii++)
            {
                map.ItemSpawns.Add(new InvItem(Spawns.GetSpawn(ii)), Spawns.GetSpawnRate(ii));
            }
        }
    }
}
