using System;
using RogueElements;


namespace RogueEssence.LevelGen
{
    /// <summary>
    /// Sets the Title of the floor, taking in an offset for ID substitutions.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class MapNameIDStep<T> : GenStep<T> where T : BaseMapGenContext
    {
        /// <summary>
        /// The title of the map.
        /// Can include one string format subtituion for floor number.
        /// </summary>
        public LocalText Name;

        /// <summary>
        /// The amount to add to the map ID to get the floor number substituted into the title.
        /// </summary>
        public int IDOffset;


        public MapNameIDStep()
        {
            Name = new LocalText();

        }
        public MapNameIDStep(LocalText name, int idOffset)
        {
            Name = new LocalText(name);
            IDOffset = idOffset;
        }

        public MapNameIDStep(LocalText name)
        {
            Name = new LocalText(name);
        }

        public override void Apply(T map)
        {
            map.Map.Name = LocalText.FormatLocalText(Name, (map.Map.ID + IDOffset).ToString());
        }

        public override string ToString()
        {
            return string.Format("{0}: {1}", this.GetType().GetFormattedTypeName(), Name.ToLocal());
        }
    }


}
