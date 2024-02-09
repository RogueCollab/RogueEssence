using System;
using Newtonsoft.Json;
using RogueEssence.Data;
using RogueEssence.Dev;

namespace RogueEssence.Dungeon
{

    [Serializable]
    public class Skill
    {
        [JsonConverter(typeof(SkillConverter))]
        public string SkillNum;
        public int Charges;
        public bool Enabled;

        public bool Sealed;

        public Skill()
            : this("", 0)
        { }

        public Skill(string skillNum, int charges)
            : this(skillNum, charges, true)
        { }
        public Skill(string skillNum, int charges, bool enabled)
        {
            SkillNum = skillNum;
            Charges = charges;
            Enabled = enabled;
        }
    }
}
