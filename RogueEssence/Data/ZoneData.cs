using System;
using System.Collections.Generic;
using RogueEssence.LevelGen;
using RogueEssence.Dungeon;
using RogueEssence.Script;
using System.Runtime.Serialization;

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
        public override string ToString()
        {
            return Name.ToLocal();
        }

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
            foreach (ZoneSegmentBase structure in Segments)
            {
                if (structure.IsRelevant)
                    totalFloors += structure.FloorCount;
            }
            return new ZoneEntrySummary(Name, Released, Comment, NoEXP, Level, LevelCap, TeamRestrict, TeamSize, MoneyRestrict, BagRestrict, BagSize, Rescues, totalFloors, Rogue);
        }


        public List<ZoneSegmentBase> Segments;
        [Dev.DataFolder(1, "Ground/")]
        public List<string> GroundMaps;

        [Dev.NoDupe(0)]
        public List<LuaEngine.EZoneCallbacks> ScriptEvents;


        public ZoneData()
        {
            Name = new LocalText();
            Comment = "";

            Level = -1;
            TeamSize = -1;
            BagRestrict = -1;
            BagSize = -1;

            Segments = new List<ZoneSegmentBase>();
            GroundMaps = new List<string>();

            ScriptEvents = new List<LuaEngine.EZoneCallbacks>();
        }

        public string GetColoredName()
        {
            return String.Format("[color=#FFC663]{0}[color]", Name.ToLocal());
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
            zone.Segments = Segments;
            zone.GroundMaps = GroundMaps;
            zone.LoadScriptEvents(ScriptEvents);
            return zone;
        }

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            //TODO: v0.5: remove this
            if (ScriptEvents == null)
                ScriptEvents = new List<LuaEngine.EZoneCallbacks>();
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


        public override string GetColoredName()
        {
            return String.Format("[color=#FFC663]{0}[color]", Name.ToLocal());
        }
    }

}
