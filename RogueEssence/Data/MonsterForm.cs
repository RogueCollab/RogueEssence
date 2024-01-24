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
        /// <summary>
        /// Monster's form title
        /// </summary>
        public LocalText FormName;

        /// <summary>
        /// Is it released and allowed to show up in the game?
        /// </summary>
        public bool Released { get; set; }

        /// <summary>
        /// Is it a temporary form?  Cannot be picked in rogue mode if so.
        /// </summary>
        public bool Temporary;

        /// <summary>
        /// Monster FORM this was promoted from
        /// </summary>
        public int PromoteForm;

        /// <summary>
        /// elemental typing 1
        /// </summary>
        [JsonConverter(typeof(ElementConverter))]
        [Dev.DataType(0, DataManager.DataType.Element, false)]
        public string Element1;

        /// <summary>
        /// elemental typing 2
        /// </summary>
        [JsonConverter(typeof(ElementConverter))]
        [Dev.SharedRow, Dev.DataType(0, DataManager.DataType.Element, false)]
        public string Element2;

        /// <summary>
        /// possible intrinsic 1
        /// </summary>
        [JsonConverter(typeof(IntrinsicConverter))]
        [Dev.DataType(0, DataManager.DataType.Intrinsic, false)]
        public string Intrinsic1;

        /// <summary>
        /// possible intrinsic 2
        /// </summary>
        [JsonConverter(typeof(IntrinsicConverter))]
        [Dev.DataType(0, DataManager.DataType.Intrinsic, false)]
        public string Intrinsic2;

        /// <summary>
        /// possible intrinsic 3
        /// </summary>
        [JsonConverter(typeof(IntrinsicConverter))]
        [Dev.DataType(0, DataManager.DataType.Intrinsic, false)]
        public string Intrinsic3;


        /// <summary>
        /// skills learned on level up
        /// </summary>
        public List<LevelUpSkill> LevelSkills;



        public BaseFormSummary GenerateEntrySummary()
        {
            return new BaseFormSummary(FormName, Released, Temporary);
        }


        public BaseMonsterForm()
        {
            FormName = new LocalText();
            LevelSkills = new List<LevelUpSkill>();

            // TODO: Initialize to default element, when we can guarantee that DataManager.Instance.DefaultElement itself is initialized
            Element1 = "";
            Element2 = "";
            
            // TODO: Make invalid intrinsic represent no-ability, not default
            Intrinsic1 = "";
            Intrinsic2 = "";
            Intrinsic3 = "";
        }



        public override string ToString()
        {
            if (!String.IsNullOrEmpty(FormName.DefaultText))
                return FormName.DefaultText;
            else
                return "[EMPTY]";
        }


        public abstract int GetStat(int level, Stat stat, int bonus);
        public abstract int GetMaxStat(Stat stat);
        public abstract int ReverseGetStat(Stat stat, int val, int level);
        public abstract int GetMaxStatBonus(Stat stat);
        public abstract bool CanLearnSkill(string skill);

        public abstract string RollSkin(IRandom rand);
        public abstract int GetPersonalityType(int discriminator);
        public abstract Gender RollGender(IRandom rand);

        public abstract string RollIntrinsic(IRandom rand, int bounds);

        public abstract List<Gender> GetPossibleGenders();

        public abstract List<string> GetPossibleSkins();

        public abstract List<int> GetPossibleIntrinsicSlots();


        public IEnumerable<string> GetSkillsAtLevel(int levelLearned, bool relearn)
        {
            for (int ii = 0; ii < LevelSkills.Count; ii++)
            {
                if (LevelSkills[ii].Level == levelLearned || LevelSkills[ii].Level <= levelLearned && relearn)
                {
                    if (DataManager.Instance.DataIndices[DataManager.DataType.Skill].Get(LevelSkills[ii].Skill).Released)
                        yield return LevelSkills[ii].Skill;
                }
            }
        }

        public List<string> RollLatestSkills(int level, List<string> specifiedSkills)
        {
            List<string> skills = new List<string>();
            skills.AddRange(specifiedSkills);

            for (int ii = LevelSkills.Count - 1; ii >= 0 && Dungeon.CharData.MAX_SKILL_SLOTS - skills.Count > 0; ii--)
            {
                if (LevelSkills[ii].Level <= level && !skills.Contains(LevelSkills[ii].Skill))
                {
                    if (DataManager.Instance.DataIndices[DataManager.DataType.Skill].Get(LevelSkills[ii].Skill).Released)
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