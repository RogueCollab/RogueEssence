using System;

namespace RogueEssence.Data
{
    [Serializable]
    public class GrowthData : IEntryData
    {
        public override string ToString()
        {
            return Name.DefaultText;
        }

        public LocalText Name { get; set; }
        public bool Released { get { return true; } }
        public string Comment { get; set; }

        public EntrySummary GenerateEntrySummary() { return new EntrySummary(Name, Released, Comment); }

        public int[] EXPTable;

        public GrowthData()
        {
            Name = new LocalText();
            Comment = "";
        }

        public GrowthData(LocalText name, int[] expTable)
        {
            Name = name;
            Comment = "";
            EXPTable = expTable;
        }

        public int GetExpToNext(int level)
        {
            return GetExpTo(level, level + 1);
        }
        public int GetExpTo(int fromLevel, int toLevel)
        {
            return EXPTable[toLevel - 1] - EXPTable[fromLevel - 1];
        }

        public string GetColoredName()
        {
            return String.Format("{0}", Name.ToLocal());
        }
    }
}
