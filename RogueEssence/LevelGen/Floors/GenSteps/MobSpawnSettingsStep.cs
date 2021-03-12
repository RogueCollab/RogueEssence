using System;
using RogueElements;
using RogueEssence.Dev;

namespace RogueEssence.LevelGen
{
    [Serializable]
    public class MobSpawnSettingsStep<T> : GenStep<T> where T : BaseMapGenContext
    {
        public int RespawnTime;
        public int MaxFoes;

        public MobSpawnSettingsStep()
        {
            RespawnTime = -1;
            MaxFoes = -1;
        }

        public MobSpawnSettingsStep(int respawnTime, int maxTeams)
        {
            RespawnTime = respawnTime;
            MaxFoes = maxTeams;
        }

        public override void Apply(T map)
        {
            if (RespawnTime > -1)
                map.RespawnTime = RespawnTime;
            if (MaxFoes > -1)
                map.MaxFoes = MaxFoes;
        }


        public override string ToString()
        {
            return String.Format("{0}: MaxFoes:{1} RespawnTurns:{2}", this.GetType().Name, RespawnTime, RespawnTime);
        }
    }
}
