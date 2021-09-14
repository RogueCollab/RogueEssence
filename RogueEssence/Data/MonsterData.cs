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

        public LocalText Name { get; set; }

        public LocalText Title;

        public bool Released { get; set; }
        [Dev.Multiline(0)]
        public string Comment { get; set; }

        public EntrySummary GenerateEntrySummary()
        {
            MonsterEntrySummary summary = new MonsterEntrySummary(Name, Released, Comment);
            foreach (BaseMonsterForm form in Forms)
                summary.Forms.Add(form.GenerateEntrySummary());
            return summary;
        }


        [Dev.DataType(0, DataManager.DataType.GrowthGroup, false)]
        public int EXPTable;

        [Dev.DataType(0, DataManager.DataType.SkillGroup, false)]
        public int SkillGroup1;

        [Dev.SharedRow]
        [Dev.DataType(0, DataManager.DataType.SkillGroup, false)]
        public int SkillGroup2;

        public int JoinRate;


        [Dev.DataType(0, DataManager.DataType.Monster, true)]
        public int PromoteFrom;
        public List<PromoteBranch> Promotions;

        public List<BaseMonsterForm> Forms;

        public MonsterData()
        {
            Name = new LocalText();
            Title = new LocalText();
            Comment = "";
            PromoteFrom = -1;
            Promotions = new List<PromoteBranch>();
            Forms = new List<BaseMonsterForm>();
        }


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

        public MonsterEntrySummary(LocalText name, bool released, string comment) : base(name, released, comment)
        {
            Forms = new List<BaseFormSummary>();
        }


        public override string GetColoredName()
        {
            return String.Format("[color=#00FF00]{0}[color]", Name.ToLocal());
        }
    }


}