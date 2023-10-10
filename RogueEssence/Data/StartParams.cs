using RogueEssence.Dev;
using RogueEssence.Dungeon;
using System;
using System.Collections.Generic;

namespace RogueEssence.Data
{
    [Serializable]
    public class StartParams
    {
        public List<StartChar> Chars;
        public int Personality;
        public ZoneLoc Map;
        public int Level;
        public int MaxLevel;
        public List<string> Teams;
    }


    [Serializable]
    public class StartChar
    {
        [MonsterID(0, false, false, true, true)]
        public MonsterID ID;
        public string Name;

        public StartChar()
        {
            Name = "";
        }
        public StartChar(MonsterID id, string name)
        {
            ID = id;
            Name = name;
        }

        public override string ToString()
        {
            return ID.ToString();
        }
    }
}
