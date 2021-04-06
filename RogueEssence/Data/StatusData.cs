using System;
using RogueEssence.Dungeon;
using RogueElements;
using RogueEssence.Dev;

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
        public bool CarryOver;
        public LocalText Desc { get; set; }

        public bool Released { get; set; }
        public string Comment { get; set; }

        public EntrySummary GenerateEntrySummary() { return new EntrySummary(Name, Released, Comment); }


        [Anim(0, "Icon/")]
        public string Emoticon;
        [Anim(0, "Icon/")]
        public string DropEmoticon;
        [Anim(0, "Icon/")]
        public string FreeEmote;
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
            Emoticon = "";
            DropEmoticon = "";
            FreeEmote = "";
            DrawEffect = DrawEffect.None;

            StatusStates = new StateCollection<StatusState>();

            OnSkillChanges = new PriorityList<SkillChangeEvent>();

            TargetPassive = new PassiveData();
        }
    }

}
