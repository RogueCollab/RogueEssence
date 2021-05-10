using System;
using RogueElements;

namespace RogueEssence.LevelGen
{
    [Serializable]
    public class FloorNameIDPostProc : ZonePostProc
    {
        public LocalText Name;
        public Priority Priority;

        public FloorNameIDPostProc(Priority priority)
        {
            Priority = priority;
            Name = new LocalText();
        }
        public FloorNameIDPostProc(Priority priority, LocalText name)
        {
            Priority = priority;
            Name = new LocalText(name);
        }
        protected FloorNameIDPostProc(FloorNameIDPostProc other, ulong seed)
        {
            Priority = other.Priority;
            Name = other.Name;
        }

        public override ZonePostProc Instantiate(ulong seed) { return new FloorNameIDPostProc(this, seed); }

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
