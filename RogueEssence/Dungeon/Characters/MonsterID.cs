using System;
using Newtonsoft.Json;
using RogueEssence.Data;
using RogueEssence.Dev;

namespace RogueEssence.Dungeon
{
    [Serializable]
    public struct MonsterID
    {
        [JsonConverter(typeof(MonsterConverter))]
        public string Species;
        public int Form;
        [JsonConverter(typeof(SkinConverter))]
        public string Skin;
        public Gender Gender;

        public MonsterID(string species, int form, string skin, Gender gender)
        {
            Species = species;
            Form = form;
            Skin = skin;
            Gender = gender;
        }

        public static readonly MonsterID Invalid = new MonsterID("", -1, "", Gender.Unknown);

        public bool IsValid()
        {
            return !String.IsNullOrEmpty(Species);
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

        public Content.CharID ToCharID()
        {
            int mon = DataManager.Instance.DataIndices[DataManager.DataType.Monster].Entries[Species].GetSortOrder();
            int skin = DataManager.Instance.DataIndices[DataManager.DataType.Skin].Entries[Skin].GetSortOrder();
            return new Content.CharID(mon, Form, skin, (int)Gender);
        }
    }
}
