using System;

namespace RogueEssence.Dungeon
{
    [Serializable]
    public struct SegLoc
    {
        //a negative structure value with a positive ID will refer to ground maps
        public int Segment;
        public int ID;

        public SegLoc(int segment, int id)
        {
            Segment = segment;
            ID = id;
        }

        private static readonly SegLoc invalid = new SegLoc(-1, -1);

        public static SegLoc Invalid { get { return invalid; } }


        public bool IsValid()
        {
            return (ID > -1);
        }
    }
}