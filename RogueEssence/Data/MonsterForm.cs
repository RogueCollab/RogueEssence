using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using RogueElements;
using RogueEssence.Dev;

namespace RogueEssence.Data
{

    [Serializable]
    public abstract class BaseMonsterForm
    {
        public LocalText FormName;

        public bool Released { get; set; }

        public bool Temporary;

        [JsonConverter(typeof(ElementConverter))]
        [Dev.DataType(0, DataManager.DataType.Element, false)]
        public string Element1;

        [JsonConverter(typeof(ElementConverter))]
        [Dev.SharedRow, Dev.DataType(0, DataManager.DataType.Element, false)]
        public string Element2;

        [Dev.DataType(0, DataManager.DataType.Intrinsic, false)]
        public int Intrinsic1;
        [Dev.DataType(0, DataManager.DataType.Intrinsic, false)]
        public int Intrinsic2;
        [Dev.DataType(0, DataManager.DataType.Intrinsic, false)]
        public int Intrinsic3;


        public List<LevelUpSkill> LevelSkills;



        public BaseFormSummary GenerateEntrySummary()
        {
            return new BaseFormSummary(FormName, Released, Temporary);
        }


        public BaseMonsterForm()
        {
            FormName = new LocalText();
            LevelSkills = new List<LevelUpSkill>();
            Element1 = "";
            Element2 = "";
        }



        public override string ToString()
        {
            return FormName.DefaultText;
        }


        public abstract int GetStat(int level, Stat stat, int bonus);
        public abstract int GetMaxStat(Stat stat);
        public abstract int ReverseGetStat(Stat stat, int val, int level);
        public abstract int GetMaxStatBonus(Stat stat);
        public abstract bool CanLearnSkill(int skill);

        public abstract int RollSkin(IRandom rand);
        public abstract int GetPersonalityType(int discriminator);
        public abstract Gender RollGender(IRandom rand);

        public abstract int RollIntrinsic(IRandom rand, int bounds);

        public abstract List<Gender> GetPossibleGenders();

        public abstract List<int> GetPossibleSkins();

        public abstract List<int> GetPossibleIntrinsicSlots();


        public IEnumerable<int> GetSkillsAtLevel(int levelLearned, bool relearn)
        {
            for (int ii = 0; ii < LevelSkills.Count; ii++)
            {
                if (LevelSkills[ii].Level == levelLearned || LevelSkills[ii].Level <= levelLearned && relearn)
                {
                    //TODO: String Assets
                    if (DataManager.Instance.DataIndices[DataManager.DataType.Skill].Entries[LevelSkills[ii].Skill.ToString()].Released)
                        yield return LevelSkills[ii].Skill;
                }
            }
        }

        public List<int> RollLatestSkills(int level, List<int> specifiedSkills)
        {
            List<int> skills = new List<int>();
            skills.AddRange(specifiedSkills);

            for (int ii = LevelSkills.Count - 1; ii >= 0 && Dungeon.CharData.MAX_SKILL_SLOTS - skills.Count > 0; ii--)
            {
                if (LevelSkills[ii].Level <= level && !skills.Contains(LevelSkills[ii].Skill))
                {
                    //TODO: String Assets
                    if (DataManager.Instance.DataIndices[DataManager.DataType.Skill].Entries[LevelSkills[ii].Skill.ToString()].Released)
                        skills.Insert(specifiedSkills.Count, LevelSkills[ii].Skill);
                }
            }
            return skills;
        }

    }


    [Serializable]
    public class BaseFormSummary
    {
        public LocalText Name;
        public bool Released;
        public bool Temporary;

        public BaseFormSummary() : base()
        {
            Name = new LocalText();
        }

        public BaseFormSummary(LocalText name, bool released, bool temporary)
        {
            Name = name;
            Released = released;
            Temporary = temporary;
        }
    }


    public enum Gender
    {
        Unknown = -1,
        Genderless = 0,
        Male = 1,
        Female = 2
    }

    public enum Stat
    {
        None = -1,
        HP = 0,
        Attack = 1,
        Defense = 2,
        MAtk = 3,
        MDef = 4,
        Speed = 5,
        HitRate = 6,
        DodgeRate = 7,
        Range = 8
    };



}