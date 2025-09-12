using System;

namespace RogueEssence.Dungeon
{
    [Serializable]
    public struct TurnOrder
    {
        public const int TURN_TIER_0 = 0;
        public const int TURN_TIER_1_4 = 1;
        public const int TURN_TIER_1_3 = 2;
        public const int TURN_TIER_1_2 = 3;
        public const int TURN_TIER_2_3 = 4;
        public const int TURN_TIER_3_4 = 5;

        public int TurnTier;
        public Faction Faction;
        public int TurnIndex;

        public TurnOrder(int turnTier, Faction faction, int turnIndex)
        {
            TurnTier = turnTier;
            Faction = faction;
            TurnIndex = turnIndex;
        }
    }
}
