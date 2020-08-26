using System;
using System.Collections.Generic;


namespace RogueEssence.Data
{
    [Serializable]
    public class MonsterData : IEntryData
    {

        public override string ToString()
        {
            return Name.DefaultText;
        }

        public LocalText Name { get; set; }

        public LocalText Title;

        public bool Released { get; set; }
        public string Comment { get; set; }

        public EntrySummary GenerateEntrySummary()
        {
            FormEntrySummary summary = new FormEntrySummary(Name, Released, Comment);
            foreach (BaseMonsterForm form in Forms)
                summary.FormTexts.Add(form.FormName);
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
        
    }




}