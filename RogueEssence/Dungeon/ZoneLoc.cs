using System;

namespace RogueEssence.Dungeon
{
    [Serializable]
    public struct ZoneLoc
    {
        public int ID;
        public SegLoc StructID;
        public int EntryPoint;
        
        public ZoneLoc(int id, int structure, int structId, int entryPoint)
        {
            ID = id;
            StructID = new SegLoc(structure, structId);
            EntryPoint = entryPoint;
        }

        public ZoneLoc(int id, SegLoc structId)
        {
            ID = id;
            StructID = structId;
            EntryPoint = 0;
        }

        public ZoneLoc(int id, SegLoc structId, int entryPoint)
        {
            ID = id;
            StructID = structId;
            EntryPoint = entryPoint;
        }


        private static readonly ZoneLoc invalid = new ZoneLoc(-1, new SegLoc(-1, -1), -1);

        public static ZoneLoc Invalid { get { return invalid; } }

        public bool IsValid()
        {
            return (ID > -1) && StructID.IsValid();
        }
    }
}