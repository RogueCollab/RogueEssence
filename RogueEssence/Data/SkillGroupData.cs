using System;

namespace RogueEssence.Data
{
    [Serializable]
    public class SkillGroupData : IEntryData
    {
        public override string ToString()
        {
            return Name.ToLocal();
        }

        public LocalText Name { get; set; }
        public bool Released { get { return true; } }
        public string Comment { get; set; }

        public EntrySummary GenerateEntrySummary() { return new EntrySummary(Name, Released, Comment); }

        public SkillGroupData()
        {
            Name = new LocalText();
            Comment = "";
        }

        public SkillGroupData(LocalText name)
        {
            Name = name;
            Comment = "";
        }

        public string GetColoredName()
        {
            return String.Format("[color=#00FF00]{0}[color]", Name.ToLocal());
        }
    }
}
