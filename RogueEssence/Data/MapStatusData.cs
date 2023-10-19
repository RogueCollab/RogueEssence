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

        /// <summary>
        /// The name of the data
        /// </summary>
        public LocalText Name { get; set; }

        /// <summary>
        /// The description of the data
        /// </summary>
        [Dev.Multiline(0)]
        public LocalText Desc { get; set; }

        /// <summary>
        /// Is it released and allowed to show up in the game?
        /// </summary>
        public bool Released { get; set; }

        /// <summary>
        /// Comments visible to only developers
        /// </summary>
        [Dev.Multiline(0)]
        public string Comment { get; set; }

        public EntrySummary GenerateEntrySummary() { return new EntrySummary(Name, Released, Comment); }

        /// <summary>
        /// Does it carry over between floors?
        /// </summary>
        public bool CarryOver;

        /// <summary>
        /// The VFX to play while the map status is in effect.
        /// </summary>
        public SwitchOffEmitter Emitter;

        /// <summary>
        /// Should this map status start off hidden from the menu?
        /// </summary>
        public bool DefaultHidden;

        /// <summary>
        /// States of the map status
        /// </summary>
        [ListCollapse]
        public StateCollection<MapStatusState> StatusStates;

        /// <summary>
        /// Triggers when a characer needs to refresh traits
        /// </summary>
        [ListCollapse]
        public PriorityList<RefreshEvent> OnMapRefresh;

        /// <summary>
        /// Triggers when something attempts to add a map status when it already exists
        /// </summary>
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

        /// <summary>
        /// Gets the colored string for the map status
        /// </summary>
        /// <returns></returns>
        public string GetColoredName()
        {
            return String.Format("[color=#00FFFF]{0}[color]", Name.ToLocal());
        }
    }
}
