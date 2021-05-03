using System;

namespace RogueEssence.Data
{
    [Serializable]
    public class RankData : IEntryData
    {
        public override string ToString()
        {
            return Name.DefaultText;
        }

        public LocalText Name { get; set; }
        public bool Released { get { return true; } }
        public string Comment { get; set; }

        public int BagSize;
        public int FameToNext;

        public EntrySummary GenerateEntrySummary() { return new EntrySummary(Name, Released, Comment); }

        public RankData()
        {
            Name = new LocalText();
            Comment = "";
        }

        public RankData(LocalText name, int bagSize, int fameToNext)
        {
            Name = name;
            Comment = "";
            BagSize = bagSize;
            FameToNext = fameToNext;
        }

        public string GetColoredName()
        {
            return String.Format("[color=#FFA5FF]{0}[color]", Name.ToLocal());
        }
    }
}
