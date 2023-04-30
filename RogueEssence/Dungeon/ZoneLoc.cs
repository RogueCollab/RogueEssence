using Newtonsoft.Json;
using RogueEssence.Data;
using RogueEssence.Dev;
using System;

namespace RogueEssence.Dungeon
{
    [Serializable]
    public struct ZoneLoc
    {
        [JsonConverter(typeof(DungeonConverter))]
        [DataType(0, DataManager.DataType.Zone, false)]
        public string ID;
        public SegLoc StructID;
        public int EntryPoint;

        public ZoneLoc(string id, int structure, int structId, int entryPoint)
        {
            ID = id;
            StructID = new SegLoc(structure, structId);
            EntryPoint = entryPoint;
        }

        public ZoneLoc(string id, SegLoc structId)
        {
            ID = id;
            StructID = structId;
            EntryPoint = 0;
        }

        public ZoneLoc(string id, SegLoc structId, int entryPoint)
        {
            ID = id;
            StructID = structId;
            EntryPoint = entryPoint;
        }


        private static readonly ZoneLoc invalid = new ZoneLoc("", new SegLoc(-1, -1), -1);

        public static ZoneLoc Invalid { get { return invalid; } }

        public bool IsValid()
        {
            return (!String.IsNullOrEmpty(ID)) && StructID.IsValid();
        }

        public override string ToString()
        {
            return String.Format("{0} {1} {2}", ID, StructID.ToString(), EntryPoint);
        }
    }
}