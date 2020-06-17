using System;
using RogueElements;

namespace RogueEssence.LevelGen
{
    [Serializable]
    public class FloorNamePostProc : ZonePostProc
    {
        public LocalText Name;
        public int Priority;

        //spreads an item through the floors
        //ensures that the space in floors between occurrences is kept tame
        public FloorNamePostProc(int priority)
        {
            Priority = priority;
            Name = new LocalText();
        }
        public FloorNamePostProc(int priority, LocalText name)
        {
            Priority = priority;
            Name = new LocalText(name);
        }
        protected FloorNamePostProc(FloorNamePostProc other, ulong seed)
        {
            Priority = other.Priority;
            Name = other.Name;
        }

        public override ZonePostProc Instantiate(ulong seed) { return new FloorNamePostProc(this, seed); }

        public override void Apply(ZoneGenContext zoneContext, IGenContext context, StablePriorityQueue<int, IGenStep> queue)
        {
            queue.Enqueue(Priority, new MapNameStep<BaseMapGenContext>(LocalText.FormatLocalText(Name, (zoneContext.CurrentID + 1).ToString())));
        }
    }
}
