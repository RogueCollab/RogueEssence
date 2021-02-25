using System;
using System.Collections.Generic;
using RogueEssence.LevelGen;
using RogueEssence.Dungeon;
using RogueEssence.Script;

namespace RogueEssence.Data
{
    public enum RogueStatus
    {
        None,
        NoTransfer,
        ItemTransfer,
        AllTransfer
    }

    public interface IZoneData : IEntryData
    {
        bool NoEXP { get; set; }
        int Level { get; set; }
        bool TeamRestrict { get; set; }
        int TeamSize { get; set; }
        bool MoneyRestrict { get; set; }
        int BagRestrict { get; set; }
        int BagSize { get; set; }
        int Rescues { get; set; }
        RogueStatus Rogue { get; set; }

    }

    [Serializable]
    public class ZoneData : IEntryData
    {

        public LocalText Name { get; set; }
        public bool Released { get; set; }
        public string Comment { get; set; }

        public bool NoEXP { get; set; }
        public int Level { get; set; }
        public bool LevelCap { get; set; }
        public bool TeamRestrict { get; set; }
        public int TeamSize { get; set; }
        public bool MoneyRestrict { get; set; }
        public int BagRestrict { get; set; }
        public int BagSize { get; set; }
        public int Rescues { get; set; }
        public RogueStatus Rogue { get; set; }

        public EntrySummary GenerateEntrySummary()
        {
            int totalFloors = 0;
            foreach (ZoneSegmentBase structure in Structures)
            {
                if (structure.IsRelevant)
                    totalFloors += structure.FloorCount;
            }
            return new ZoneEntrySummary(Name, Released, Comment, NoEXP, Level, LevelCap, TeamRestrict, TeamSize, MoneyRestrict, BagRestrict, BagSize, Rescues, totalFloors, Rogue);
        }


        public List<ZoneSegmentBase> Structures;
        public List<string> GroundMaps;

        private Dictionary<LuaEngine.EZoneCallbacks, ScriptEvent> ScriptEvents;


        public ZoneData()
        {
            Name = new LocalText();
            Comment = "";

            Level = -1;
            TeamSize = -1;
            BagRestrict = -1;
            BagSize = -1;

            Structures = new List<ZoneSegmentBase>();
            GroundMaps = new List<string>();

            ScriptEvents = new Dictionary<LuaEngine.EZoneCallbacks, ScriptEvent>();
        }

        public void AddZoneScriptEvent(int idx, LuaEngine.EZoneCallbacks ev)
        {
            string assetName = "zone_" + idx;
            DiagManager.Instance.LogInfo(String.Format("Zone.AddZoneScriptEvent(): Added event {0} to zone {1}!", ev.ToString(), assetName));
            ScriptEvents[ev] = new ScriptEvent(LuaEngine.MakeZoneScriptCallbackName(assetName, ev));
        }

        public void RemoveZoneScriptEvent(int idx, LuaEngine.EZoneCallbacks ev)
        {
            string assetName = "zone_" + idx;
            DiagManager.Instance.LogInfo(String.Format("Zone.RemoveZoneScriptEvent(): Removed event {0} from zone {1}!", ev.ToString(), assetName));
            if (ScriptEvents.ContainsKey(ev))
                ScriptEvents.Remove(ev);
        }

        public Zone CreateActiveZone(ulong seed, int zoneIndex)
        {
            Zone zone = new Zone(seed, zoneIndex);
            zone.Name = Name;

            zone.NoEXP = NoEXP;
            zone.Level = Level;
            zone.LevelCap = LevelCap;
            zone.TeamRestrict = TeamRestrict;
            zone.TeamSize = TeamSize;
            zone.MoneyRestrict = MoneyRestrict;
            zone.BagRestrict = BagRestrict;
            zone.BagSize = BagSize;

            //NOTE: these are not deep copies!
            zone.Structures = Structures;
            zone.GroundMaps = GroundMaps;
            zone.LoadScriptEvents(ScriptEvents);
            return zone;
        }

    }


    [Serializable]
    public class ZoneEntrySummary : EntrySummary
    {
        public bool NoEXP;
        public int Level;
        public bool LevelCap;
        public bool TeamRestrict;
        public int TeamSize;
        public bool MoneyRestrict;
        public int BagRestrict;
        public int BagSize;
        public int Rescues;
        public int CountedFloors;
        public RogueStatus Rogue;

        public ZoneEntrySummary() : base()
        {

        }

        public ZoneEntrySummary(LocalText name, bool released, string comment, bool noEXP, int level, bool levelCap, bool teamRestrict, int teamSize, bool moneyRestrict, int bagRestrict, int bagSize, int rescues, int countedFloors, RogueStatus rogue)
            : base(name, released, comment)
        {
            NoEXP = noEXP;
            Level = level;
            LevelCap = levelCap;
            TeamRestrict = teamRestrict;
            TeamSize = teamSize;
            MoneyRestrict = moneyRestrict;
            BagRestrict = bagRestrict;
            BagSize = bagSize;
            Rescues = rescues;
            CountedFloors = countedFloors;
            Rogue = rogue;
        }
    }

}
