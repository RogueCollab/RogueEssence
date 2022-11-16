using System;
using RogueElements;


namespace RogueEssence.LevelGen
{
    /// <summary>
    /// Sets the ID and title of the floor.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class MapNameIDStep<T> : GenStep<T> where T : BaseMapGenContext
    {
        /// <summary>
        /// The floor ID.  This is typically the floor number and must be equal to the ID being requested.
        /// </summary>
        public int ID;

        /// <summary>
        /// The title of the map.
        /// </summary>
        public LocalText Name;

        public MapNameIDStep()
        {
            Name = new LocalText();

        }
        public MapNameIDStep(int id, LocalText name)
        {
            ID = id;
            Name = new LocalText(name);
        }

        public override void Apply(T map)
        {
            map.Map.ID = ID;
            map.Map.Name = new LocalText(Name);
            map.DropTitle = true;
        }
    }


}
