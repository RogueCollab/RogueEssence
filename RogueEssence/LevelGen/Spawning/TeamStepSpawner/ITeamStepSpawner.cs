using RogueElements;
using RogueEssence.Dungeon;

namespace RogueEssence.LevelGen
{
    public interface ITeamStepSpawner<T> : ITeamStepSpawner where T : IGenContext
    {
        Team GetSpawn(T map);
    }

    public interface ITeamStepSpawner
    {

    }
}
