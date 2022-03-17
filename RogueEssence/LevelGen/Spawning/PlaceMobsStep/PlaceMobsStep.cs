using System;
using RogueElements;
using RogueEssence.Dungeon;

namespace RogueEssence.LevelGen
{
    [Serializable]
    public abstract class PlaceMobsStep<T> : GenStep<T>
        where T : class, IGroupPlaceableGenContext<TeamSpawn>
    {
        public IMultiTeamSpawner<T> Spawn;
        public bool Ally;

        public PlaceMobsStep() { }
        public PlaceMobsStep(IMultiTeamSpawner<T> spawn)
        {
            Spawn = spawn;
        }
    }
}
