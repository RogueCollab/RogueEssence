using System;

namespace RogueEssence.Data
{
    [Serializable]
    public class RankData : Dev.EditorData, IEntryData
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

        public RankData() { }

        public RankData(LocalText name, int bagSize, int fameToNext)
        {
            Name = name;
            BagSize = bagSize;
            FameToNext = fameToNext;
        }

    }
}
