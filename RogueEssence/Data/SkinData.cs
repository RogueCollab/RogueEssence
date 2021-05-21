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
        public string Comment { get; set; }

        public char Symbol;
        public Color MinimapColor;
        public BattleFX LeaderFX;
        public bool Challenge;

        public EntrySummary GenerateEntrySummary() { return new EntrySummary(Name, Released, Comment); }

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
}
