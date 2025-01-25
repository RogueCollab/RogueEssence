using System;
using System.Collections.Generic;
using RogueElements;
using RogueEssence.Dungeon;

namespace RogueEssence.LevelGen
{
    /// <summary>
    /// Spawns mob teams from a specified team builder, at a specified amount.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class LoopedRandTeamSpawner<T> : IMultiTeamSpawner<T>, ILoopedRandTeamSpawner
        where T : IGenContext, IMobSpawnMap
    {
        /// <summary>
        /// Builds the team to be spawned.
        /// </summary>
        public IRandPicker<TeamSpawner> Picker;

        /// <summary>
        /// Decides how many teams to spawn.
        /// </summary>
        public IRandPicker<int> AmountSpawner { get; set; }

        public LoopedRandTeamSpawner() { }

        public LoopedRandTeamSpawner(IRandPicker<TeamSpawner> picker)
        {
            Picker = picker;
        }

        public LoopedRandTeamSpawner(IRandPicker<TeamSpawner> picker, IRandPicker<int> amount)
        {
            Picker = picker;
            AmountSpawner = amount;
        }

        public List<Team> GetSpawns(T map)
        {
            List<Team> result = new List<Team>();
            int amount = AmountSpawner.Pick(((IGenContext)map).Rand);
            for (int ii = 0; ii < amount * 10; ii++)
            {
                TeamSpawner spawner = Picker.Pick(((IGenContext)map).Rand);
                Team team = spawner.Spawn(map);
                if (team != null)
                    result.Add(team);
                if (result.Count >= amount)
                    break;
            }

            return result;
        }

        public override string ToString()
        {
            if (AmountSpawner == null)
                return String.Format("{0}", this.GetType().GetFormattedTypeName());
            else
                return String.Format("{0}[{1}]", this.GetType().GetFormattedTypeName(), AmountSpawner.ToString());
        }
    }

    public interface ILoopedRandTeamSpawner
    {
        IRandPicker<int> AmountSpawner { get; set; }
    }
}
