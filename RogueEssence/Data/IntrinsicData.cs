using System;
namespace RogueEssence.Data
{
    [Serializable]
    public class IntrinsicData : ProximityPassive, IDescribedData
    {
        /// <summary>
        /// Returns the name of the intrinsic in the current language.
        /// </summary>
        /// <returns>The intrinsic name.</returns>
        public override string ToString()
        {
            return Name.ToLocal();
        }

        /// <summary>
        /// The name of the intrinsic, containing all translations.
        /// Use the name's ToLocal() function to get the name in current language.
        /// For proper battle log formatting, use GetColoredName() instead.
        /// </summary>
        public LocalText Name { get; set; }


        /// <summary>
        /// The description of the intrinsic, containing all translations.
        /// Use the name's ToLocal() function to get the name in current language.
        /// </summary>
        [Dev.Multiline(0)]
        public LocalText Desc { get; set; }

        /// <summary>
        /// Internal flag to show whether a intrinsic is completed and allowed to appear in the game.
        /// Intrinsics that are not released appear with an asterisk next to their names when viewed in the Dev Mode editors. 
        /// </summary>
        public bool Released { get; set; }

        /// <summary>
        /// An internal piece of text only visible using the Dev Mode editors, or by calling this property.
        /// Usually used to take notes on the intrinsic if necessary. 
        /// </summary>
        [Dev.Multiline(0)]
        public string Comment { get; set; }

        /// <summary>
        /// Index number of the intrinsic for sorting.  Must be unique.
        /// </summary>
        public int IndexNum;

        /// <summary>
        /// Returns an EntrySummary of the intrinsic.
        /// </summary>
        /// <returns></returns>
        public EntrySummary GenerateEntrySummary() { return new EntrySummary(Name, Released, Comment, IndexNum); }

        public IntrinsicData()
        {
            Name = new LocalText();
            Desc = new LocalText();
            Comment = "";
        }


        /// <summary>
        /// Gets the name of the intrinsic with appropriate color text code, ideal for use in menus and the message log.
        /// </summary>
        /// <returns></returns>
        public string GetColoredName()
        {
            return String.Format("[color=#00FF00]{0}[color]", Name.ToLocal());
        }
    }
}
