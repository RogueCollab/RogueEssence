using System;

namespace RogueEssence.Dungeon
{
    [Serializable]
    public struct TurnOrder
    {
        public int TurnTier;
        public bool EnemyFaction;
        public int TurnIndex;
        public bool SkipAll;

        public TurnOrder(int turnTier, bool enemyFaction, int turnIndex)
        {
            TurnTier = turnTier;
            EnemyFaction = enemyFaction;
            TurnIndex = turnIndex;
            SkipAll = false;
        }
    }
}
