using System;
using RogueElements;
using RogueEssence.Dev;

namespace RogueEssence.LevelGen
{
    /// <summary>
    /// Chooses the enemy limit and respawn time for the map.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class MobSpawnSettingsStep<T> : GenStep<T> where T : BaseMapGenContext
    {
        /// <summary>
        /// The limit to the number of enemies on the map.  If this number is reached or exceeded, no more respawns will occur.
        /// </summary>
        public int MaxFoes;

        /// <summary>
        /// The amount of time it takes for a new enemy team to respawn, in turns.
        /// </summary>
        public int RespawnTime;

        public MobSpawnSettingsStep()
        {
            MaxFoes = -1;
            RespawnTime = -1;
        }

        public MobSpawnSettingsStep(int maxTeams, int respawnTime)
        {
            MaxFoes = maxTeams;
            RespawnTime = respawnTime;
        }

        public override void Apply(T map)
        {
            if (MaxFoes > -1)
                map.MaxFoes = MaxFoes;
            if (RespawnTime > -1)
                map.RespawnTime = RespawnTime;
        }


        public override string ToString()
        {
            return String.Format("{0}: MaxFoes:{1} RespawnTurns:{2}", this.GetType().Name, MaxFoes, RespawnTime);
        }
    }
}
