using System;
using System.Collections.Generic;
using RogueElements;

namespace RogueEssence.Dungeon
{
    [Serializable]
    public class GameAction
    {
        public enum ActionType
        {
            None = -1,
            Dir = 0,
            Move,
            Attack,
            Pickup,
            Tile,
            UseItem,
            Give,
            Take,
            Drop,
            Throw,
            UseSkill,
            Wait,
            TeamMode,
            ShiftTeam,
            SetLeader,
            SendHome,
            Tactics,
            ShiftSkill,
            SetSkill,
            SortItems,
            GiveUp,
            Rescue
        };

        public ActionType Type;
        public Dir8 Dir;
        private List<int> args;
        public int this[int index]
        {
            get
            {
                return args[index];
            }
        }
        public int ArgCount { get { return args.Count; } }


        public GameAction(ActionType type, Dir8 dir, params int[] args)
        {
            Type = type;
            Dir = dir;
            this.args = new List<int>();
            for (int ii = 0; ii < args.Length; ii++)
            {
                this.args.Add(args[ii]);
            }
        }

        public void AddArg(int arg)
        {
            args.Add(arg);
        }

    }
}
