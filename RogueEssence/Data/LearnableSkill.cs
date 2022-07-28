using Newtonsoft.Json;
using RogueEssence.Dev;
using System;

namespace RogueEssence.Data
{
    [Serializable]
    public class LevelUpSkill : LearnableSkill
    {
        public int Level;

        public LevelUpSkill()
        {
        }

        public LevelUpSkill(string skill, int level) : base(skill)
        {
            Level = level;
        }

        public override string ToString()
        {
            return "[Lv. " + Level + "] " + DataManager.Instance.GetSkill(Skill).Name.ToLocal();
        }

        public override bool Equals(object obj)
        {
            LevelUpSkill other = obj as LevelUpSkill;
            if (other == null)
                return false;
            return other.Skill == Skill && other.Level == Level;
        }

        public override int GetHashCode()
        {
            return Level.GetHashCode() ^ Skill.GetHashCode();
        }
    }

    [Serializable]
    public class LearnableSkill
    {
        [JsonConverter(typeof(SkillConverter))]
        [DataType(0, DataManager.DataType.Skill, false)]
        public string Skill;

        public LearnableSkill()
        {
            Skill = "";
        }

        public LearnableSkill(string skill)
        {
            Skill = skill;
        }

        public override string ToString()
        {
            return DataManager.Instance.GetSkill(Skill).Name.ToLocal();
        }

        public override bool Equals(object obj)
        {
            LearnableSkill other = obj as LearnableSkill;
            if (other == null)
                return false;
            return other.Skill == Skill;
        }

        public override int GetHashCode()
        {
            return Skill.GetHashCode();
        }
    }
}
