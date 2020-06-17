using System;
using RogueElements;
using RogueEssence.Dungeon;

namespace RogueEssence.LevelGen
{
    [Serializable]
    public class ItemSpawnStep<T> : GenStep<T> where T : BaseMapGenContext
    {
        public SpawnList<SpawnList<InvItem>> Spawns;

        public ItemSpawnStep()
        {
            Spawns = new SpawnList<SpawnList<InvItem>>();
        }

        public override void Apply(T map)
        {
            for(int ii = 0; ii < Spawns.Count; ii++)
            {
                //chance of landing an item in a list is item chance * list chance
                SpawnList<InvItem> innerList = Spawns.GetSpawn(ii);
                for (int jj = 0; jj < innerList.Count; jj++)
                    map.ItemSpawns.Add(new InvItem(innerList.GetSpawn(jj)), innerList.GetSpawnRate(jj) * Spawns.GetSpawnRate(ii));
            }
        }
    }
}
