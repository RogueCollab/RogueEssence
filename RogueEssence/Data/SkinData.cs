using System;
using RogueEssence.Content;
using Microsoft.Xna.Framework;

namespace RogueEssence.Data
{
    [Serializable]
    public class SkinData : Dev.EditorData, IEntryData
    {
        public override string ToString()
        {
            return Name.DefaultText;
        }

        public LocalText Name { get; set; }
        public bool Released { get { return true; } }
        public string Comment { get; set; }

        public char Symbol;
        public Color MinimapColor;
        public BattleFX LeaderFX;
        public bool Challenge;

        public EntrySummary GenerateEntrySummary() { return new EntrySummary(Name, Released, Comment); }

        public SkinData() { }

        public SkinData(LocalText name, char symbol)
        {
            Name = name;
            Symbol = symbol;
            LeaderFX  = new BattleFX();
        }

    }
}
