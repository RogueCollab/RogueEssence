using System;
using System.Collections.Generic;
using RogueEssence.Dungeon;
using RogueElements;

namespace RogueEssence.LevelGen
{
    /// <summary>
    /// Places the mobs without altering their location in any way.
    /// This is useful for mobs that already have their location set on creation.
    /// </summary>
    /// <typeparam name="T"></typeparam>
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
