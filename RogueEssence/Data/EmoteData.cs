using System;
using RogueEssence.Content;

namespace RogueEssence.Data
{
    [Serializable]
    public class EmoteData : IEntryData
    {
        public override string ToString()
        {
            return Name.DefaultText;
        }

        public LocalText Name { get; set; }
        public bool Released { get { return true; } }
        public string Comment { get; set; }

        public EntrySummary GenerateEntrySummary() { return new EntrySummary(Name, Released, Comment); }

        public int LocHeight;
        public AnimData Anim;

        public EmoteData() { }

        public EmoteData(LocalText name, AnimData anim, int locHeight)
        {
            Name = name;
            Comment = "";
            Anim = anim;
            LocHeight = locHeight;
        }
    }
}
