using System;
using RogueElements;
using RogueEssence.Dungeon;

namespace RogueEssence.LevelGen
{
    [Serializable]
    public abstract class PlaceMobsStep<T> : GenStep<T>
        where T : class, IGroupPlaceableGenContext<TeamSpawn>
    {
        /// <summary>
        /// The generator responsible for creating the list of teams to spawn.
        /// </summary>
        public IMultiTeamSpawner<T> Spawn;

        /// <summary>
        /// Determines if the mobs should be spawned as allies.
        /// </summary>
        public bool Ally;

        public PlaceMobsStep() { }
        public PlaceMobsStep(IMultiTeamSpawner<T> spawn)
        {
            Spawn = spawn;
        }
    }
}
