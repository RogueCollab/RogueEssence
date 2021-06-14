using System;
using System.Collections.Generic;
using RogueEssence.Dungeon;
using RogueElements;

namespace RogueEssence.LevelGen
{
    [Serializable]
    public class PlaceNoLocMobsStep<T> : PlaceMobsStep<T>
        where T : class, IGroupPlaceableGenContext<TeamSpawn>, IMobSpawnMap
    {

        public PlaceNoLocMobsStep()
        {
        }

        public PlaceNoLocMobsStep(IMultiTeamSpawner<T> spawn) : base(spawn) { }

        public override void Apply(T map)
        {
            List<Team> spawns = Spawn.GetSpawns(map);
            foreach (Team team in spawns)
                map.PlaceItems(new TeamSpawn(team, Ally), null);
        }
    }
}
