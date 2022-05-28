using System;
using System.Collections.Generic;
using RogueElements;
using RogueEssence.Dungeon;

namespace RogueEssence.LevelGen
{
    /// <summary>
    /// Spawns mob teams to the map based on the map's encounter table.
    /// </summary>
    /// <typeparam name="T"></typeparam>
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

        /// <summary>
        /// This amount is in total team members, not in teams.
        /// </summary>
        public RandRange Amount { get; set; }

        public List<Team> GetSpawns(T map)
        {
            int chosenAmount = Amount.Pick(map.Rand);
            int addedMembers = 0;
            List<Team> results = new List<Team>();
            for (int ii = 0; ii < chosenAmount; ii++)
            {
                if (!map.TeamSpawns.CanPick)
                    break;
                Team team = map.TeamSpawns.Pick(map.Rand).Spawn(map);
                if (team != null)
                {
                    results.Add(team);
                    addedMembers += team.Players.Count;
                    if (addedMembers >= chosenAmount)
                        break;
                }
            }

            return results;
        }

        public override string ToString()
        {
            return string.Format("{0}", this.GetType().Name);
        }
    }
}
