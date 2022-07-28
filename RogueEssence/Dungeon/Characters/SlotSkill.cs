using Newtonsoft.Json;
using RogueEssence.Dev;
using System;

namespace RogueEssence.Dungeon
{

    [Serializable]
    public class SlotSkill
    {
        [JsonConverter(typeof(SkillConverter))]
        public string SkillNum;
        public int Charges;

        public SlotSkill() : this("") { }

        public SlotSkill(string skillNum)
        {
            SkillNum = skillNum;
        }
        public SlotSkill(SlotSkill other)
        {
            SkillNum = other.SkillNum;
        }
    }
}
