using System;
using System.Collections.Generic;
using RogueElements;
using RogueEssence.Dungeon;

namespace RogueEssence.LevelGen
{
    [Serializable]
    public class TeamContextSpawner<T> : IMultiTeamSpawner<T> 
        where T : BaseMapGenContext
    {
        public TeamContextSpawner()
        {
            Amount = RandRange.Empty;
        }
        public TeamContextSpawner(RandRange amount)
        {
            Amount = amount;
        }

        public RandRange Amount { get; set; }

        public List<Team> GetSpawns(T map)
        {
            int chosenAmount = Amount.Pick(map.Rand);
            var results = new List<Team>();
            for (int ii = 0; ii < chosenAmount; ii++)
            {
                if (!map.TeamSpawns.CanPick)
                    break;
                Team team = map.TeamSpawns.Pick(map.Rand).Spawn(map);
                if (team != null)
                    results.Add(team);
            }

            return results;
        }

        public override string ToString()
        {
            return string.Format("{0}", this.GetType().Name);
        }
    }
}
