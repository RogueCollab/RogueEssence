using System;
using System.Collections.Generic;
using RogueElements;
using RogueEssence.Dungeon;

namespace RogueEssence.LevelGen
{
    [Serializable]
    public class LoopedTeamSpawner<T> : IMultiTeamSpawner<T>
        where T : IGenContext, IMobSpawnMap
    {
        public TeamSpawner Picker;

        public IRandPicker<int> AmountSpawner;

        public LoopedTeamSpawner() { }

        public LoopedTeamSpawner(TeamSpawner picker)
        {
            Picker = picker;
        }

        public LoopedTeamSpawner(TeamSpawner picker, IRandPicker<int> amount)
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
                Team team = Picker.Spawn(map);
                if (team != null)
                    result.Add(team);
                if (result.Count >= amount)
                    break;
            }

            return result;
        }
    }
}
