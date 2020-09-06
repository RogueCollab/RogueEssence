using System;
using RogueElements;
using RogueEssence.Dev;

namespace RogueEssence.LevelGen
{
    [Serializable]
    public class MobSpawnStep<T> : GenStep<T> where T : BaseMapGenContext
    {
        public int RespawnTime;
        public int MaxFoes;

        [SubGroup]
        public SpawnList<TeamSpawner> Spawns;

        public MobSpawnStep()
        {
            RespawnTime = -1;
            MaxFoes = -1;
            Spawns = new SpawnList<TeamSpawner>();
        }

        public MobSpawnStep(int respawnTime, int maxTeams)
        {
            RespawnTime = respawnTime;
            Spawns = new SpawnList<TeamSpawner>();
        }

        public override void Apply(T map)
        {
            for(int ii = 0; ii < Spawns.Count; ii++)
                map.MobSpawns.Add(Spawns.GetSpawn(ii).Clone(), Spawns.GetSpawnRate(ii));//Clone Use Case; convert to Instantiate?
            if (RespawnTime > -1)
                map.RespawnTime = RespawnTime;
            if (MaxFoes > -1)
                map.MaxFoes = MaxFoes;
        }
    }
}
