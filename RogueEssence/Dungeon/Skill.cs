using System;
using RogueEssence.Data;

namespace RogueEssence.Dungeon
{

    [Serializable]
    public class Skill
    {

        public int SkillNum;
        public int Charges;
        public bool Enabled;

        public bool Sealed;

        public Skill()
            : this(-1, 0)
        { }

        public Skill(int skillNum, int charges)
            : this(skillNum, charges, true)
        { }
        public Skill(int skillNum, int charges, bool enabled)
        {
            SkillNum = skillNum;
            Charges = charges;
            Enabled = enabled;
        }
    }
}
