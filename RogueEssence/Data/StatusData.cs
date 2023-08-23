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
            return Name.ToLocal();
        }

        public LocalText Name { get; set; }
        public LocalText Desc { get; set; }

        public bool Released { get; set; }
        [Dev.Multiline(0)]
        public string Comment { get; set; }

        public EntrySummary GenerateEntrySummary() { return new EntrySummary(Name, Released, Comment); }


        /// <summary>
        /// Determines if it shows up in the status menu.
        /// </summary>
        public bool MenuName;

        /// <summary>
        /// Determines if the status stays when changing floors.
        /// </summary>
        public bool CarryOver;

        /// <summary>
        /// The icon that appears over the character's head when they have the status.
        /// </summary>
        [Anim(0, "Icon/")]
        public string Emoticon;

        /// <summary>
        /// The icon that appears over the character's head when they have this status with a stack below 0
        /// </summary>
        [Anim(0, "Icon/")]
        public string DropEmoticon;

        /// <summary>
        /// Icon that appears on the character's body when they have the status.
        /// </summary>
        [Anim(0, "Icon/")]
        public string FreeEmote;

        /// <summary>
        /// Special visual effects applied to the character with this status.
        /// </summary>
        public DrawEffect DrawEffect;

        /// <summary>
        /// Will keep track of the character that inflicted the status.
        /// </summary>
        public bool Targeted;

        /// <summary>
        /// Special variables that this status contains.
        /// They are potentially checked against in a select number of battle events.
        /// </summary>
        [ListCollapse]
        public StateCollection<StatusState> StatusStates;

        /// <summary>
        /// Event for when the character's skills are changed or swapped around.
        /// </summary>
        [ListCollapse]
        public PriorityList<SkillChangeEvent> OnSkillChanges;

        /// <summary>
        /// Passive effects applied to the character that inflicted the status.
        /// </summary>
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


        public string GetColoredName()
        {
            return String.Format("[color=#00FF00]{0}[color]", Name.ToLocal());
        }
    }

}
