using System;
using System.Collections.Generic;
using RogueEssence.Dungeon;
using RogueElements;

namespace RogueEssence.LevelGen
{

    [Serializable]
    public class PresetMultiTeamSpawner<T> : IMultiTeamSpawner<T>
        where T : IGenContext, IMobSpawnMap
    {
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
                result.Add(spawner.Spawn(map));
            return result;
        }
    }
}
