using System;
using Newtonsoft.Json;
using RogueEssence.Data;
using RogueEssence.Dev;

namespace RogueEssence.Content
{
    public struct CharID
    {
        public int Species;
        public int Form;
        public int Skin;
        public int Gender;

        public CharID(int species, int form, int skin, int gender)
        {
            Species = species;
            Form = form;
            Skin = skin;
            Gender = gender;
        }

        public static readonly CharID Invalid = new CharID(-1, -1, -1, -1);

        public bool IsValid()
        {
            return (Species > -1);
        }

        public override string ToString()
        {
            return String.Format("{0} {1} {2} {3}", Species, Form, Skin, Gender);
        }

        public override bool Equals(object obj)
        {
            return (obj is CharID) && Equals((CharID)obj);
        }

        public bool Equals(CharID other)
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

        public static bool operator ==(CharID value1, CharID value2)
        {
            return value1.Equals(value2);
        }

        public static bool operator !=(CharID value1, CharID value2)
        {
            return !(value1 == value2);
        }
    }
}
