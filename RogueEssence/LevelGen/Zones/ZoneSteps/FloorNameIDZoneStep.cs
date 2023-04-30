using System;
using RogueElements;

namespace RogueEssence.LevelGen
{
    /// <summary>
    /// Names all floors of the dungeon segment according to a naming convention.
    /// </summary>
    [Serializable]
    public class FloorNameIDZoneStep : ZoneStep
    {
        /// <summary>
        /// The name to give the floors.  Can use {0} for floor number.
        /// </summary>
        public LocalText Name;

        /// <summary>
        /// At what point in the map gen process to run the naming step in.
        /// </summary>
        public Priority Priority;

        public FloorNameIDZoneStep()
        {
            Name = new LocalText();
        }

        public FloorNameIDZoneStep(Priority priority, LocalText name)
        {
            Priority = priority;
            Name = new LocalText(name);
        }
        protected FloorNameIDZoneStep(FloorNameIDZoneStep other, ulong seed)
        {
            Priority = other.Priority;
            Name = other.Name;
        }

        public override ZoneStep Instantiate(ulong seed) { return new FloorNameIDZoneStep(this, seed); }

        public override void Apply(ZoneGenContext zoneContext, IGenContext context, StablePriorityQueue<Priority, IGenStep> queue)
        {
            queue.Enqueue(Priority, new MapNameIDStep<BaseMapGenContext>(Name, 1));
        }

        public override string ToString()
        {
            return string.Format("{0}: \"{1}\"", this.GetType().GetFormattedTypeName(), this.Name.DefaultText);
        }
    }
}
