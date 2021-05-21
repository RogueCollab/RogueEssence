using System;
using RogueEssence.Dungeon;
using RogueEssence.Content;

namespace RogueEssence.Data
{
    [Serializable]
    public class MapStatusData : ProximityPassive, IDescribedData
    {
        public override string ToString()
        {
            return Name.ToLocal();
        }

        public LocalText Name { get; set; }

        [Dev.Multiline(0)]
        public LocalText Desc { get; set; }

        public bool Released { get; set; }
        public string Comment { get; set; }

        public EntrySummary GenerateEntrySummary() { return new EntrySummary(Name, Released, Comment); }

        public bool CarryOver;

        public SwitchOffEmitter Emitter;
        public bool DefaultHidden;

        public StateCollection<MapStatusState> StatusStates;

        public MapStatusGivenEvent RepeatMethod;

        public MapStatusData()
        {
            Name = new LocalText();
            Desc = new LocalText();
            Comment = "";
            Emitter = new EmptySwitchOffEmitter();

            StatusStates = new StateCollection<MapStatusState>();
        }


        public string GetColoredName()
        {
            return String.Format("[color=#00FFFF]{0}[color]", Name.ToLocal());
        }
    }
}
