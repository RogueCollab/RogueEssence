using RogueElements;
using RogueEssence.Dungeon;

namespace RogueEssence.LevelGen
{
    public interface ITeamStepSpawner<T> where T : IGenContext
    {
        Team GetSpawn(T map);

    }
}
