using System;
using RogueElements;
using RogueEssence.Dev;

namespace RogueEssence.LevelGen
{
    [Serializable]
    public class MobSpawnStep<T> : GenStep<T> where T : BaseMapGenContext
    {
        [SubGroup]
        public SpawnList<TeamSpawner> Spawns;

        public MobSpawnStep()
        {
            Spawns = new SpawnList<TeamSpawner>();
        }

        public override void Apply(T map)
        {
            for(int ii = 0; ii < Spawns.Count; ii++)
                map.TeamSpawns.Add(Spawns.GetSpawn(ii).Clone(), Spawns.GetSpawnRate(ii));//Clone Use Case; convert to Instantiate?
        }
    }
}
