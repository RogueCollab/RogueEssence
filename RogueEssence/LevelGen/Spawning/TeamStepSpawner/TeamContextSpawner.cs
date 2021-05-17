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
            if (!map.TeamSpawns.CanPick)
                return null;
            return map.TeamSpawns.Pick(map.Rand).Spawn(map);
        }

        public override string ToString()
        {
            return string.Format("{0}", this.GetType().Name);
        }
    }
}
