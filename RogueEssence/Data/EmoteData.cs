using System;
using RogueEssence.Content;

namespace RogueEssence.Data
{
    [Serializable]
    public class EmoteData : IEntryData
    {
        public override string ToString()
        {
            return Name.ToLocal();
        }

        public LocalText Name { get; set; }
        public bool Released { get { return true; } }
        public string Comment { get; set; }

        public EntrySummary GenerateEntrySummary() { return new EntrySummary(Name, Released, Comment); }

        public int LocHeight;
        public AnimData Anim;

        public EmoteData()
        {
            Name = new LocalText();
            Comment = "";
            Anim = new AnimData();
        }

        public EmoteData(LocalText name, AnimData anim, int locHeight)
        {
            Name = name;
            Comment = "";
            Anim = anim;
            LocHeight = locHeight;
        }

        public string GetColoredName()
        {
            return String.Format("{0}", Name.ToLocal());
        }
    }
}
