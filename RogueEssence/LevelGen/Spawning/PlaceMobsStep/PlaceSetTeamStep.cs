using System;
using System.Collections.Generic;
using RogueEssence.Dungeon;
using RogueElements;

namespace RogueEssence.LevelGen
{
    [Serializable]
    public class PlaceSetTeamStep<T> : GenStep<T> where T : class, ITiledGenContext, IGroupPlaceableGenContext<Team>, IMobSpawnMap
    {
        public List<SpecificTeamSpawner> Spawns;
        
        public PlaceSetTeamStep()
        {
            Spawns = new List<SpecificTeamSpawner>();
        }

        public override void Apply(T map)
        {
            foreach (SpecificTeamSpawner spawner in Spawns)
                map.PlaceItems(spawner.Spawn(map), null);
        }
    }
}
