using System;
using RogueElements;
using RogueEssence.Dungeon;

namespace RogueEssence.LevelGen
{
    [Serializable]
    public abstract class PlaceMobsStep<T> : GenStep<T>, IPlaceMobsStep
        where T : class, IGroupPlaceableGenContext<TeamSpawn>
    {
        /// <summary>
        /// The generator responsible for creating the list of teams to spawn.
        /// </summary>
        public IMultiTeamSpawner<T> Spawn { get; set; }

        /// <summary>
        /// Determines if the mobs should be spawned as allies.
        /// </summary>
        public bool Ally { get; set; }

        IMultiTeamStepSpawner IPlaceMobsStep.Spawn => this.Spawn;

        public PlaceMobsStep() { }
        public PlaceMobsStep(IMultiTeamSpawner<T> spawn)
        {
            Spawn = spawn;
        }

        public override string ToString()
        {
            return String.Format("{0}: {1}", this.GetType().GetFormattedTypeName(), Spawn.ToString());
        }
    }

    public interface IPlaceMobsStep
    {
        IMultiTeamStepSpawner Spawn { get; }
    }
}
