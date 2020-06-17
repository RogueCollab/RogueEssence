using System;
using RogueElements;
using RogueEssence.Dungeon;

namespace RogueEssence.LevelGen
{
    [Serializable]
    public abstract class PlaceMobsStep<T> : GenStep<T>
        where T : class, IGroupPlaceableGenContext<Team>
    {
        public ITeamStepSpawner<T> Spawn;

        public PlaceMobsStep() { }
        public PlaceMobsStep(ITeamStepSpawner<T> spawn)
        {
            Spawn = spawn;
        }

    }
}
