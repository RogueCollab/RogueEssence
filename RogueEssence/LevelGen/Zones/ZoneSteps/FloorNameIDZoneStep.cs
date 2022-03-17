using System;
using RogueElements;

namespace RogueEssence.LevelGen
{
    [Serializable]
    public class FloorNameIDZoneStep : ZoneStep
    {
        public LocalText Name;
        public Priority Priority;

        public FloorNameIDZoneStep()
        {
            Name = new LocalText();
        }

        public FloorNameIDZoneStep(Priority priority)
        {
            Priority = priority;
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
            queue.Enqueue(Priority, new MapNameIDStep<BaseMapGenContext>(zoneContext.CurrentID, LocalText.FormatLocalText(Name, (zoneContext.CurrentID + 1).ToString())));
        }

        public override string ToString()
        {
            return string.Format("{0}: \"{1}\"", this.GetType().Name, this.Name.DefaultText);
        }
    }
}
