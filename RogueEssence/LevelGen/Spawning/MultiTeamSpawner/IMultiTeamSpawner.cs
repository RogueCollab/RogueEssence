using RogueElements;
using RogueEssence.Dungeon;
using System.Collections.Generic;

namespace RogueEssence.LevelGen
{
    public interface IMultiTeamSpawner<T> : IMultiTeamStepSpawner where T : IGenContext
    {
        List<Team> GetSpawns(T map);
    }

    public interface IMultiTeamStepSpawner
    {

    }
}
