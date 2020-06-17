using System;
using RogueEssence.Dungeon;

namespace RogueEssence.Data
{
    [Serializable]
    public abstract class BaseRescueMail
    {
        public abstract string Extension { get; }
        public string TeamName { get; set; }
        public string TeamID { get; set; }
        public ulong Seed { get; set; }
        public byte RescueSeed { get; set; }
        public int TurnsTaken { get; set; }
        public string DateDefeated { get; set; }
        public ZoneLoc Goal { get; set; }
        public Version DefeatedVersion { get; set; }
        public MapItem OfferedItem { get; set; }
        public LocalText GoalText { get; set; }
        public MonsterID[] TeamProfile { get; set; }
    }
}
