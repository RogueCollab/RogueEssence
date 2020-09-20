using System;
using RogueElements;

namespace RogueEssence.LevelGen
{
    [Serializable]
    public class FloorNameIDPostProc : ZonePostProc
    {
        public LocalText Name;
        public int Priority;

        public FloorNameIDPostProc(int priority)
        {
            Priority = priority;
            Name = new LocalText();
        }
        public FloorNameIDPostProc(int priority, LocalText name)
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

        public override void Apply(ZoneGenContext zoneContext, IGenContext context, StablePriorityQueue<int, IGenStep> queue)
        {
            queue.Enqueue(Priority, new MapNameIDStep<BaseMapGenContext>(zoneContext.CurrentID, LocalText.FormatLocalText(Name, (zoneContext.CurrentID + 1).ToString())));
        }
    }
}
