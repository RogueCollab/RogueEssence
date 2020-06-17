using System;

namespace RogueEssence.Dungeon
{
    [Serializable]
    public struct EXPGain
    {
        public MonsterID SlainMonster;
        public int Level;

        public EXPGain(MonsterID slain, int level)
        {
            SlainMonster = slain;
            Level = level;
        }
    }
}
