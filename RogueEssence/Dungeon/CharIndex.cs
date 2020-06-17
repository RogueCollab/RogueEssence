using System;

namespace RogueEssence.Dungeon
{
    [Serializable]
    public struct CharIndex
    {
        public int Team;
        public int Char;

        public CharIndex(int teamIndex, int memberIndex)
        {
            Team = teamIndex;
            Char = memberIndex;
        }
        private static readonly CharIndex invalid = new CharIndex(-1, -1);

        public static CharIndex Invalid { get { return invalid; } }

        public override bool Equals(object obj)
        {
            return (obj is CharIndex) && Equals((CharIndex)obj);
        }

        public bool Equals(CharIndex other)
        {
            return (Team == other.Team && Char == other.Char);
        }

        public override int GetHashCode()
        {
            return Team.GetHashCode() ^ Char.GetHashCode();
        }

        public static bool operator ==(CharIndex value1, CharIndex value2)
        {
            return value1.Equals(value2);
        }

        public static bool operator !=(CharIndex value1, CharIndex value2)
        {
            return !(value1 == value2);
        }
    }
}
