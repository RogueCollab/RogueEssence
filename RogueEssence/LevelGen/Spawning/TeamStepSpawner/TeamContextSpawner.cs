using System;
using RogueEssence.Dungeon;

namespace RogueEssence.LevelGen
{
    [Serializable]
    public class TeamContextSpawner<T> : ITeamStepSpawner<T> 
        where T : BaseMapGenContext
    {
        public TeamContextSpawner() { }
        
        public Team GetSpawn(T map)
        {
            return map.TeamSpawns.Pick(map.Rand).Spawn(map);
        }
    }
}
