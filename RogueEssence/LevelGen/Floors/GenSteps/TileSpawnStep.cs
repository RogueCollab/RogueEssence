using System;
using RogueElements;
using RogueEssence.Dungeon;
using RogueEssence.Dev;

namespace RogueEssence.LevelGen
{
    /// <summary>
    /// Generates the table of trap tiles to spawn on a floor
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class TileSpawnStep<T> : GenStep<T> where T : BaseMapGenContext
    {
        [SubGroup]
        public SpawnList<EffectTile> Spawns;

        public TileSpawnStep()
        {
            Spawns = new SpawnList<EffectTile>();
        }

        public override void Apply(T map)
        {
            for (int ii = 0; ii < Spawns.Count; ii++)
                map.TileSpawns.Add(Spawns.GetSpawn(ii), Spawns.GetSpawnRate(ii));
        }
    }
}
