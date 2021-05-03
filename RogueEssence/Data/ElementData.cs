using System;

namespace RogueEssence.Data
{
    [Serializable]
    public class ElementData : IEntryData
    {
        public override string ToString()
        {
            return Name.DefaultText;
        }

        public LocalText Name { get; set; }
        public bool Released { get { return true; } }
        public string Comment { get; set; }

        public EntrySummary GenerateEntrySummary() { return new EntrySummary(Name, Released, Comment); }

        public char Symbol;

        public ElementData()
        {
            Name = new LocalText();
            Comment = "";
        }

        public ElementData(LocalText name, char symbol)
        {
            Name = name;
            Comment = "";
            Symbol = symbol;
        }

        public string GetColoredName()
        {
            return String.Format("[color=#FFFFFF]{0}[color]", Name.ToLocal());
        }

        public string GetIconName()
        {
            return String.Format("{0}\u2060{1}", Symbol, GetColoredName());
        }
    }
}
