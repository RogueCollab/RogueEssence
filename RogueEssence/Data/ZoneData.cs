using System;
using System.Collections.Generic;
using RogueEssence.LevelGen;
using RogueEssence.Dungeon;
using RogueEssence.Script;
using System.Runtime.Serialization;
using RogueEssence.Dev;

namespace RogueEssence.Data
{
    public enum RogueStatus
    {
        /// <summary>
        /// Disallowed for Rogue mode.
        /// </summary>
        None,
        /// <summary>
        /// Allowed for rogue mode, cannot transfer anything.
        /// </summary>
        NoTransfer,
        /// <summary>
        /// Allowed for rogue mode, can only transfer items to main save.
        /// </summary>
        ItemTransfer,
        /// <summary>
        /// Allowed for rogue mode, can transfer items and characters to main save.
        /// </summary>
        AllTransfer
    }

    public interface IZoneData : IEntryData
    {
        int ExpPercent { get; set; }
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
        [NonEdited]
        public bool NoEXP { get; set; }

        /// <summary>
        /// Percent to multiply EXP gain for the dungeon.
        /// 0 means no EXP.
        /// </summary>
        public int ExpPercent { get; set; }

        /// <summary>
        /// The recommended level to face the dungeon.
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// Turn on to cap the team at the recommended level.
        /// </summary>
        public bool LevelCap { get; set; }

        /// <summary>
        /// Turn on to keep the teams moveset during level restrictions.
        /// </summary>
        [SharedRow]
        public bool KeepSkills { get; set; }
        
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
        /// Exempts Treasure items from BagRestrict limit
        /// </summary>
        public bool KeepTreasure { get; set; }

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
            ZoneEntrySummary summary = new ZoneEntrySummary(Name, Released, Comment);
            summary.ExpPercent = ExpPercent;
            summary.Level = Level;
            summary.LevelCap = LevelCap;
            summary.KeepSkills = KeepSkills;
            summary.TeamRestrict = TeamRestrict;
            summary.TeamSize = TeamSize;
            summary.MoneyRestrict = MoneyRestrict;
            summary.BagRestrict = BagRestrict;
            summary.KeepTreasure = KeepTreasure;
            summary.BagSize = BagSize;
            summary.Rescues = Rescues;
            summary.CountedFloors = totalFloors;
            summary.Rogue = Rogue;
            summary.Grounds.AddRange(GroundMaps);
            for (int ii = 0; ii < Segments.Count; ii++)
            {
                if (Segments[ii].FloorCount < 0)
                    summary.Maps.Add(null);
                else
                {
                    HashSet<int> floors = new HashSet<int>();
                    foreach (int id in Segments[ii].GetFloorIDs())
                        floors.Add(id);
                    summary.Maps.Add(floors);
                }
            }
            return summary;
        }

        /// <summary>
        /// Sections of the dungeon.
        /// Ex. Splitting the dungeon into a normal and deeper section.
        /// </summary>
        [Collection(0, true)]
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

            ExpPercent = 100;
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


        public Zone CreateActiveZone(ulong seed, string zoneIndex)
        {
            Zone zone = new Zone(seed, zoneIndex);
            zone.Name = Name;

            zone.ExpPercent = ExpPercent;
            zone.Level = Level;
            zone.LevelCap = LevelCap;
            zone.KeepSkills = KeepSkills;
            zone.TeamRestrict = TeamRestrict;
            zone.TeamSize = TeamSize;
            zone.MoneyRestrict = MoneyRestrict;
            zone.BagRestrict = BagRestrict;
            zone.KeepTreasure = KeepTreasure;
            zone.BagSize = BagSize;
            zone.Persistent = Persistent;

            //NOTE: these are not deep copies!
            zone.Segments = Segments;
            zone.GroundMaps = GroundMaps;
            return zone;
        }

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            //TODO: remove on v1.1
            if (Serializer.OldVersion < new Version(0, 7, 22))
            {
                if (!NoEXP)
                    ExpPercent = 100;
            }
        }
    }


    [Serializable]
    public class ZoneEntrySummary : EntrySummary
    {
        public int ExpPercent;
        public int Level;
        public bool LevelCap;
        public bool KeepSkills;
        public bool TeamRestrict;
        public int TeamSize;
        public bool MoneyRestrict;
        public int BagRestrict;
        public bool KeepTreasure;
        public int BagSize;
        public int Rescues;
        public int CountedFloors;
        public RogueStatus Rogue;
        public List<string> Grounds;
        public List<HashSet<int>> Maps;

        public ZoneEntrySummary() : base()
        {
            Grounds = new List<string>();
            Maps = new List<HashSet<int>>();
        }

        public ZoneEntrySummary(LocalText name, bool released, string comment)
            : base(name, released, comment)
        {
            Grounds = new List<string>();
            Maps = new List<HashSet<int>>();
        }

        public bool SegLocValid(SegLoc segLoc)
        {
            if (segLoc.Segment == -1)
                return (0 <= segLoc.ID && segLoc.ID < Grounds.Count);
            else if (0 <= segLoc.Segment && segLoc.Segment < Maps.Count)
            {
                if (Maps[segLoc.Segment] == null)
                    return true;
                return Maps[segLoc.Segment].Contains(segLoc.ID);
            }
            return false;
        }

        public bool GroundValid(string groundName)
        {
            return Grounds.Contains(groundName);
        }


        public override string GetColoredName()
        {
            return String.Format("[color=#FFC663]{0}[color]", Name.ToLocal());
        }
    }

}
