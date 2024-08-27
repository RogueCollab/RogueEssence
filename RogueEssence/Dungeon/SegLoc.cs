using System;
using System.Diagnostics.CodeAnalysis;

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

        public override string ToString()
        {
            return String.Format("{0} {1}", Segment, ID);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is SegLoc))
                return false;

            SegLoc other = (SegLoc)obj;

            return this.Segment == other.Segment && this.ID == other.ID;
        }
    }
}