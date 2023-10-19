using Newtonsoft.Json;
using RogueEssence.Dev;
using System;
using System.Collections.Generic;


namespace RogueEssence.Data
{
    [Serializable]
    public class MonsterData : IEntryData
    {
        public override string ToString()
        {
            return Name.ToLocal();
        }

        /// <summary>
        /// Monster's name
        /// </summary>
        public LocalText Name { get; set; }

        /// <summary>
        /// Monster's species title
        /// </summary>
        public LocalText Title;

        /// <summary>
        /// Is it released and allowed to show up in the game?
        /// </summary>
        public bool Released { get; set; }

        /// <summary>
        /// Comments visible to only developers
        /// </summary>
        [Dev.Multiline(0)]
        public string Comment { get; set; }

        public EntrySummary GenerateEntrySummary()
        {
            MonsterEntrySummary summary = new MonsterEntrySummary(Name, Released, Comment, IndexNum);
            foreach (BaseMonsterForm form in Forms)
                summary.Forms.Add(form.GenerateEntrySummary());
            return summary;
        }

        /// <summary>
        /// Index number of the monster for sorting.  Must be unique
        /// </summary>
        public int IndexNum;

        /// <summary>
        /// How fast this unit levels up.  Uses the Growth Group EXP tables.
        /// </summary>
        [JsonConverter(typeof(GrowthGroupConverter))]
        [Dev.DataType(0, DataManager.DataType.GrowthGroup, false)]
        public string EXPTable;

        /// <summary>
        /// Skill group for shared skills
        /// </summary>
        [JsonConverter(typeof(SkillGroupConverter))]
        [Dev.DataType(0, DataManager.DataType.SkillGroup, false)]
        public string SkillGroup1;

        /// <summary>
        /// Skill group 2 for shared skills
        /// </summary>
        [JsonConverter(typeof(SkillGroupConverter))]
        [Dev.SharedRow]
        [Dev.DataType(0, DataManager.DataType.SkillGroup, false)]
        public string SkillGroup2;

        /// <summary>
        /// Chance to be recruited
        /// </summary>
        public int JoinRate;


        /// <summary>
        /// Monster SPECIES this was promoted from
        /// </summary>
        [JsonConverter(typeof(MonsterConverter))]
        [Dev.DataType(0, DataManager.DataType.Monster, true)]
        public string PromoteFrom;

        /// <summary>
        /// Possible promotions
        /// </summary>
        public List<PromoteBranch> Promotions;

        /// <summary>
        /// Forms of the monster
        /// </summary>
        public List<BaseMonsterForm> Forms;

        public MonsterData()
        {
            Name = new LocalText();
            Title = new LocalText();
            Comment = "";
            EXPTable = "";
            SkillGroup1 = "";
            SkillGroup2 = "";
            PromoteFrom = "";
            Promotions = new List<PromoteBranch>();
            Forms = new List<BaseMonsterForm>();
        }

        /// <summary>
        /// Gets the colored text string of the monster
        /// </summary>
        /// <returns></returns>
        public string GetColoredName()
        {
            return String.Format("[color=#00FF00]{0}[color]", Name.ToLocal());
        }
    }




    [Serializable]
    public class MonsterEntrySummary : EntrySummary
    {
        public List<BaseFormSummary> Forms;

        public MonsterEntrySummary() : base()
        {
            Forms = new List<BaseFormSummary>();
        }

        public MonsterEntrySummary(LocalText name, bool released, string comment, int sort) : base(name, released, comment, sort)
        {
            Forms = new List<BaseFormSummary>();
        }


        public override string GetColoredName()
        {
            return String.Format("[color=#00FF00]{0}[color]", Name.ToLocal());
        }
    }


}