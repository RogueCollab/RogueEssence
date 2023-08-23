using System;
using RogueEssence.Dungeon;
using RogueEssence.Content;
using RogueElements;
using RogueEssence.Dev;

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
        [Dev.Multiline(0)]
        public string Comment { get; set; }

        public EntrySummary GenerateEntrySummary() { return new EntrySummary(Name, Released, Comment); }

        public bool CarryOver;

        public SwitchOffEmitter Emitter;
        public bool DefaultHidden;

        [ListCollapse]
        public StateCollection<MapStatusState> StatusStates;

        [ListCollapse]
        public PriorityList<RefreshEvent> OnMapRefresh;
        public MapStatusGivenEvent RepeatMethod;

        public MapStatusData()
        {
            Name = new LocalText();
            Desc = new LocalText();
            Comment = "";
            Emitter = new EmptySwitchOffEmitter();

            StatusStates = new StateCollection<MapStatusState>();
            OnMapRefresh = new PriorityList<RefreshEvent>();
        }


        public string GetColoredName()
        {
            return String.Format("[color=#00FFFF]{0}[color]", Name.ToLocal());
        }
    }
}
