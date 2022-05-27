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
        [Dev.Multiline(0)]
        public string Comment { get; set; }

        /// <summary>
        /// Turn on to disable EXP gain in the dungeon.
        /// </summary>
        public bool NoEXP { get; set; }

        /// <summary>
        /// The recommended level to face the dungeon.
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// Turn on to cap the team at the recommended level.
        /// </summary>
        public bool LevelCap { get; set; }

        /// <summary>
        /// Turn on to force the player to enter with 1 team member.
        /// </summary>
        public bool TeamRestrict { get; set; }

        /// <summary>
        /// Overrides the normal maximum team size.
        /// </summary>
        public int TeamSize { get; set; }

        /// <summary>
        /// Forces all money to be stored on entry.
        /// </summary>
        public bool MoneyRestrict { get; set; }

        /// <summary>
        /// Forces items beyond the Nth slot to be stored upon entry.
        /// </summary>
        public int BagRestrict { get; set; }

        /// <summary>
        /// Forces the bag's maximum size.
        /// </summary>
        public int BagSize { get; set; }

        /// <summary>
        /// Turn this on for the zone to remember map layouts and load the old state when returning to the floor.
        /// It's not nice on memory though...
        /// </summary>
        public bool Persistent { get; set; }

        /// <summary>
        /// Rescues allowed for this zone.
        /// </summary>
        public int Rescues { get; set; }

        /// <summary>
        /// Determines if the dungeon can be played in Rogue mode, and what can be transferred.
        /// </summary>
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

        /// <summary>
        /// Sections of the dungeon.
        /// Ex. Splitting the dungeon into a normal and deeper section.
        /// </summary>
        public List<ZoneSegmentBase> Segments;
        
        /// <summary>
        /// Ground maps associated with this dungeon.
        /// Ex. Cutscene rooms for pre-boss events.
        /// </summary>
        [Dev.DataFolder(1, "Ground/")]
        public List<string> GroundMaps;


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
            zone.Persistent = Persistent;

            //NOTE: these are not deep copies!
            zone.Segments = Segments;
            zone.GroundMaps = GroundMaps;
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


        public override string GetColoredName()
        {
            return String.Format("[color=#FFC663]{0}[color]", Name.ToLocal());
        }
    }

}
