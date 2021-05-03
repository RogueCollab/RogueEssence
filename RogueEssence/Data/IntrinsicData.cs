using System;
namespace RogueEssence.Data
{
    [Serializable]
    public class IntrinsicData : ProximityPassive, IDescribedData
    {

        public override string ToString()
        {
            return Name.DefaultText;
        }

        public LocalText Name { get; set; }

        [Dev.Multiline(0)]
        public LocalText Desc { get; set; }

        public bool Released { get; set; }
        public string Comment { get; set; }

        public EntrySummary GenerateEntrySummary() { return new EntrySummary(Name, Released, Comment); }

        public IntrinsicData()
        {
            Name = new LocalText();
            Desc = new LocalText();
            Comment = "";
        }


        public string GetColoredName()
        {
            return String.Format("[color=#00FF00]{0}[color]", Name.ToLocal());
        }
    }
}
