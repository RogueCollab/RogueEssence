using System;
using System.Collections.Generic;
using RogueElements;
using RogueEssence.Data;
using RogueEssence.Dev;
using RogueEssence.Dungeon;

namespace RogueEssence.LevelGen
{
    /// <summary>
    /// Places steps on the floor based on a table.
    /// </summary>
    [Serializable]
    public class RangeStepZoneStep : ZoneStep
    {
        /// <summary>
        /// The priority to insert the step at.
        /// </summary>
        public Priority StepPriority;

        /// <summary>
        /// The steps to distribute.
        /// </summary>
        public RangeDict<IGenStep> Spawns;

        public RangeStepZoneStep()
        {
            Spawns = new RangeDict<IGenStep>();
        }

        protected RangeStepZoneStep(RangeStepZoneStep other, ulong seed)
        {
            StepPriority = other.StepPriority;
            Spawns = other.Spawns;
        }
        public override ZoneStep Instantiate(ulong seed) { return new RangeStepZoneStep(this, seed); }


        public override void Apply(ZoneGenContext zoneContext, IGenContext context, StablePriorityQueue<Priority, IGenStep> queue)
        {

            IGenStep section;
            //gets the section that intersect the current ID
            if (Spawns.TryGetItem(zoneContext.CurrentID, out section))
            {
                queue.Enqueue(StepPriority, section);
            }
        }


        public override string ToString()
        {
            int count = 0;
            IGenStep singleGen = null;
            if (Spawns != null)
            {
                foreach (IntRange range in Spawns.EnumerateRanges())
                {
                    count++;
                    singleGen = Spawns[range.Min];
                }
            }
            if (count == 1)
                return string.Format("{0}: {1}", this.GetType().GetFormattedTypeName(), singleGen.ToString());
            return string.Format("{0}[{1}]", this.GetType().GetFormattedTypeName(), count);
        }
    }
}
