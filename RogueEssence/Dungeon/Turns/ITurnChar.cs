using System;

namespace RogueEssence.Dungeon
{
    public interface ITurnChar
    {
        bool Dead { get; set; }
        int TiersUsed { get; set; }
        bool FactionTierUsed { get; set; }
        bool ActionTaken { get; set; }
        int TurnWait { get; set; }
        int MovementSpeed { get; set; }
    }
}
