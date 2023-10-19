using System;
namespace RogueEssence.Data
{
    [Serializable]
    public class IntrinsicData : ProximityPassive, IDescribedData
    {
        public override string ToString()
        {
            return Name.ToLocal();
        }

        /// <summary>
        /// The name of the data
        /// </summary>
        public LocalText Name { get; set; }


        /// <summary>
        /// The description of the data
        /// </summary>
        [Dev.Multiline(0)]
        public LocalText Desc { get; set; }

        /// <summary>
        /// Is it released and allowed to show up in the game?
        /// </summary>
        public bool Released { get; set; }

        /// <summary>
        /// Comments visible to only developers
        /// </summary>
        [Dev.Multiline(0)]
        public string Comment { get; set; }

        /// <summary>
        /// Index number of the intrinsic for sorting.  Must be unique
        /// </summary>
        public int IndexNum;

        public EntrySummary GenerateEntrySummary() { return new EntrySummary(Name, Released, Comment, IndexNum); }

        public IntrinsicData()
        {
            Name = new LocalText();
            Desc = new LocalText();
            Comment = "";
        }


        /// <summary>
        /// Gets the colored text string of the intrinsic
        /// </summary>
        /// <returns></returns>
        public string GetColoredName()
        {
            return String.Format("[color=#00FF00]{0}[color]", Name.ToLocal());
        }
    }
}
