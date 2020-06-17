using System;

namespace RogueEssence.Dungeon
{

    [Serializable]
    public class SlotSkill
    {
        public int SkillNum;
        public int Charges;

        public SlotSkill() : this(-1) { }

        public SlotSkill(int skillNum)
        {
            SkillNum = skillNum;
        }
        public SlotSkill(SlotSkill other)
        {
            SkillNum = other.SkillNum;
        }
    }
}
