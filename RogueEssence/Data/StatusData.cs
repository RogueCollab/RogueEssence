using System;
using RogueEssence.Dungeon;
using RogueElements;

namespace RogueEssence.Data
{
    [Serializable]
    public class StatusData : ProximityPassive, IDescribedData
    {
        public override string ToString()
        {
            return Name.DefaultText;
        }

        public LocalText Name { get; set; }
        public bool MenuName;
        public LocalText Desc { get; set; }

        public bool Released { get; set; }
        public string Comment { get; set; }

        public EntrySummary GenerateEntrySummary() { return new EntrySummary(Name, Released, Comment); }

        public int Emoticon;
        public int DropEmoticon;
        public int FreeEmote;
        public DrawEffect DrawEffect;
        public bool Targeted;

        //initial state for variables
        public StateCollection<StatusState> StatusStates;

        public PriorityList<SkillChangeEvent> OnSkillChanges;

        public PassiveData TargetPassive;

        public StatusData()
        {
            Name = new LocalText();
            Desc = new LocalText();
            Comment = "";
            Emoticon = -1;
            DropEmoticon = -1;
            FreeEmote = -1;
            DrawEffect = DrawEffect.None;

            StatusStates = new StateCollection<StatusState>();

            OnSkillChanges = new PriorityList<SkillChangeEvent>();

            TargetPassive = new PassiveData();
        }
    }

}
