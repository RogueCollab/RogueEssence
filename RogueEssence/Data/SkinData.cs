using System;
using RogueEssence.Content;
using Microsoft.Xna.Framework;

namespace RogueEssence.Data
{
    [Serializable]
    public class SkinData : IEntryData
    {
        public override string ToString()
        {
            return Name.ToLocal();
        }

        public LocalText Name { get; set; }
        public bool Released { get { return true; } }
        [Dev.Multiline(0)]
        public string Comment { get; set; }

        public int IndexNum;
        public char Symbol;
        public Color MinimapColor;
        public BattleFX LeaderFX;
        public bool Challenge;

        public EntrySummary GenerateEntrySummary()
        {
            SkinEntrySummary summary = new SkinEntrySummary(Name, Released, Comment);
            summary.IndexNum = IndexNum;
            return summary;
        }

        public SkinData()
        {
            Name = new LocalText();
            Comment = "";
        }

        public SkinData(LocalText name, char symbol)
        {
            Name = name;
            Comment = "";
            Symbol = symbol;
            LeaderFX  = new BattleFX();
        }

        public string GetColoredName()
        {
            return String.Format("{0}", Name.ToLocal());
        }
    }

    [Serializable]
    public class SkinEntrySummary : EntrySummary
    {
        public int IndexNum;

        public SkinEntrySummary() : base()
        {
        }

        public SkinEntrySummary(LocalText name, bool released, string comment) : base(name, released, comment)
        {

        }


        public override int GetSortOrder()
        {
            return IndexNum;
        }
    }
}
