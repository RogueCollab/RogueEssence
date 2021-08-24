using System;

namespace RogueEssence.Dungeon
{
    [Serializable]
    public struct TurnOrder
    {
        public int TurnTier;
        public Faction Faction;
        public int TurnIndex;
        public bool SkipAll;

        public TurnOrder(int turnTier, Faction faction, int turnIndex, bool skipAll)
        {
            TurnTier = turnTier;
            Faction = faction;
            TurnIndex = turnIndex;
            SkipAll = skipAll;
        }
    }
}
