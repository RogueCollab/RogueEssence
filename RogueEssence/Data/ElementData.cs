using System;

namespace RogueEssence.Data
{
    [Serializable]
    public class ElementData : Dev.EditorData, IEntryData
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

        public ElementData() { }

        public ElementData(LocalText name, char symbol)
        {
            Name = name;
            Symbol = symbol;
        }

    }
}
