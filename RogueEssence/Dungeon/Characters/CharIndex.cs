using System;

namespace RogueEssence.Dungeon
{
    public enum Faction
    {
        None = -1,
        Player = 0,
        Friend = 1,
        Foe = 2
    }

    [Serializable]
    public struct CharIndex
    {
        public Faction Faction;
        public int Team;
        public bool Guest;
        public int Char;

        public CharIndex(Faction faction, int teamIndex, bool guest, int memberIndex)
        {
            Faction = faction;
            Team = teamIndex;
            Guest = guest;
            Char = memberIndex;
        }
        private static readonly CharIndex invalid = new CharIndex(Faction.None, -1, false, -1);

        public static CharIndex Invalid { get { return invalid; } }

        public override bool Equals(object obj)
        {
            return (obj is CharIndex) && Equals((CharIndex)obj);
        }

        public bool Equals(CharIndex other)
        {
            return (Faction == other.Faction && Team == other.Team && Guest == other.Guest && Char == other.Char);
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
