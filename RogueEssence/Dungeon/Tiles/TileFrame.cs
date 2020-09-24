using System;
using RogueElements;

namespace RogueEssence.Dungeon
{
    [Serializable]
    public struct TileFrame
    {
        public static readonly TileFrame Empty = new TileFrame(new Loc(), "");

        public string Sheet;
        public Loc TexLoc;

        public TileFrame(Loc texture, string sheet)
        {
            TexLoc = texture;
            Sheet = sheet;
        }

        public override string ToString()
        {
            return String.Format("Tile {0}: {1}", Sheet, TexLoc.ToString());
        }


        public override bool Equals(object obj)
        {
            return (obj is TileFrame) && Equals((TileFrame)obj);
        }

        public bool Equals(TileFrame other)
        {
            return (TexLoc == other.TexLoc && Sheet == other.Sheet);
        }

        public override int GetHashCode()
        {
            return TexLoc.GetHashCode() ^ Sheet.GetHashCode();
        }

        public static bool operator ==(TileFrame value1, TileFrame value2)
        {
            return value1.Equals(value2);
        }

        public static bool operator !=(TileFrame value1, TileFrame value2)
        {
            return !(value1 == value2);
        }

    }
}
