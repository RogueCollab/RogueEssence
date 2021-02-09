using System;
using RogueEssence.Data;

namespace RogueEssence.Dungeon
{
    [Serializable]
    public struct MonsterID
    {
        public int Species;
        public int Form;
        public int Skin;
        public Gender Gender;

        public MonsterID(int species, int form, int skin, Gender gender)
        {
            Species = species;
            Form = form;
            Skin = skin;
            Gender = gender;
        }

        public static readonly MonsterID Invalid = new MonsterID(-1, -1, -1, Gender.Unknown);

        public bool IsValid()
        {
            return (Species > -1);
        }

        public override bool Equals(object obj)
        {
            return (obj is MonsterID) && Equals((MonsterID)obj);
        }

        public bool Equals(MonsterID other)
        {
            if (Species != other.Species)
                return false;
            if (Form != other.Form)
                return false;
            if (Skin != other.Skin)
                return false;
            if (Gender != other.Gender)
                return false;

            return true;
        }

        public override int GetHashCode()
        {
            return Species.GetHashCode() ^ Form.GetHashCode() ^ Skin.GetHashCode() ^ Gender.GetHashCode();
        }

        public static bool operator ==(MonsterID value1, MonsterID value2)
        {
            return value1.Equals(value2);
        }

        public static bool operator !=(MonsterID value1, MonsterID value2)
        {
            return !(value1 == value2);
        }
    }
}
