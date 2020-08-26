using System;
using System.Collections.Generic;
using RogueElements;


namespace RogueEssence.Data
{

    [Serializable]
    public abstract class BaseMonsterForm
    {
        public LocalText FormName;

        public bool Released { get; set; }

        public bool Temporary;

        [Dev.DataType(0, DataManager.DataType.Element, false)]
        public int Element1;

        [Dev.SharedRow, Dev.DataType(0, DataManager.DataType.Element, false)]
        public int Element2;

        public double Height;

        [Dev.SharedRow]
        public double Weight;

        [Dev.DataType(0, DataManager.DataType.Intrinsic, false)]
        public int Intrinsic1;
        [Dev.DataType(0, DataManager.DataType.Intrinsic, false)]
        public int Intrinsic2;
        [Dev.DataType(0, DataManager.DataType.Intrinsic, false)]
        public int Intrinsic3;


        public int BaseHP;

        public int BaseAtk;

        [Dev.SharedRow]
        public int BaseDef;

        public int BaseMAtk;

        [Dev.SharedRow]
        public int BaseMDef;

        public int BaseSpeed;

        public int ExpYield;


        public List<LevelUpSkill> LevelSkills;

        public List<LearnableSkill> TeachSkills;

        public List<LearnableSkill> SharedSkills;

        public List<LearnableSkill> SecretSkills;

        public BaseMonsterForm()
        {
            FormName = new LocalText();
            LevelSkills = new List<LevelUpSkill>();
            TeachSkills = new List<LearnableSkill>();
            SharedSkills = new List<LearnableSkill>();
            SecretSkills = new List<LearnableSkill>();

        }



        public override string ToString()
        {
            return FormName.DefaultText;
        }

        public int GetBaseStat(Stat stat)
        {
            switch (stat)
            {
                case Stat.HP:
                    return BaseHP;
                case Stat.Speed:
                    return BaseSpeed;
                case Stat.Attack:
                    return BaseAtk;
                case Stat.Defense:
                    return BaseDef;
                case Stat.MAtk:
                    return BaseMAtk;
                case Stat.MDef:
                    return BaseMDef;
                default:
                    return 0;
            }
        }

        public abstract int GetStat(int level, Stat stat, int bonus);
        public abstract int GetMaxStat(Stat stat);
        public abstract int ReverseGetStat(Stat stat, int val, int level);
        public abstract int GetMaxStatBonus(Stat stat);
        public abstract int GetExp(int level, int recipientLv);

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
                    if (DataManager.Instance.DataIndices[DataManager.DataType.Skill].Entries[LevelSkills[ii].Skill].Released)
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
                    if (DataManager.Instance.DataIndices[DataManager.DataType.Skill].Entries[LevelSkills[ii].Skill].Released)
                        skills.Insert(specifiedSkills.Count, LevelSkills[ii].Skill);
                }
            }
            return skills;
        }

    }


    [Serializable]
    public class FormEntrySummary : EntrySummary
    {
        public List<LocalText> FormTexts;

        public FormEntrySummary() : base()
        {
            FormTexts = new List<LocalText>();
        }

        public FormEntrySummary(LocalText name, bool released, string comment) : base(name, released, comment)
        {
            FormTexts = new List<LocalText>();
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
        None,
        HP,
        Attack,
        Defense,
        MAtk,
        MDef,
        Speed,
        HitRate,
        DodgeRate,
        Range
    };



}