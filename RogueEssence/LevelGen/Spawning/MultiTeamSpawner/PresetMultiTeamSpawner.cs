using System;
using System.Collections.Generic;
using RogueEssence.Dungeon;
using RogueElements;

namespace RogueEssence.LevelGen
{
    /// <summary>
    /// Spawns specific mob teams defined in a list.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class PresetMultiTeamSpawner<T> : IMultiTeamSpawner<T>
        where T : IGenContext, IMobSpawnMap
    {
        /// <summary>
        /// The list of teams to spawn.
        /// </summary>
        public List<SpecificTeamSpawner> Spawns;

        public PresetMultiTeamSpawner()
        {
            Spawns = new List<SpecificTeamSpawner>();
        }
        public PresetMultiTeamSpawner(params SpecificTeamSpawner[] spawns) : this()
        {
            Spawns.AddRange(spawns);
        }

        public List<Team> GetSpawns(T map)
        {
            List<Team> result = new List<Team>();
            foreach (SpecificTeamSpawner spawner in Spawns)
            {
                Team team = spawner.Spawn(map);
                if (team != null)
                    result.Add(team);
            }
            return result;
        }

        public override string ToString()
        {
            if (Spawns == null)
                return String.Format("{0}", this.GetType().GetFormattedTypeName());
            else
                return String.Format("{0}[{1}]", this.GetType().GetFormattedTypeName(), Spawns.Count);
        }
    }
}
