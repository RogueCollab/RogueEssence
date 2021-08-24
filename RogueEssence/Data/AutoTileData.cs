using System;
using RogueEssence.Dungeon;

namespace RogueEssence.Data
{
    [Serializable]
    public class AutoTileData : IEntryData
    {
        public override string ToString()
        {
            return Name.ToLocal();
        }

        public LocalText Name { get; set; }
        public bool Released { get { return true; } }
        public string Comment { get; set; }

        public EntrySummary GenerateEntrySummary() { return new EntrySummary(Name, Released, Comment); }

        public AutoTileBase Tiles;

        public AutoTileData()
        {
            Name = new LocalText();
            Comment = "";
        }

        public string GetColoredName()
        {
            return String.Format("{0}", Name.ToLocal());
        }
    }
}
