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
        /// The steps to distribute.
        /// </summary>
        public RangeDict<IGenPriority> Spawns;

        public RangeStepZoneStep()
        {
            Spawns = new RangeDict<IGenPriority>();
        }

        protected RangeStepZoneStep(RangeStepZoneStep other, ulong seed)
        {
            Spawns = other.Spawns;
        }
        public override ZoneStep Instantiate(ulong seed) { return new RangeStepZoneStep(this, seed); }


        public override void Apply(ZoneGenContext zoneContext, IGenContext context, StablePriorityQueue<Priority, IGenStep> queue)
        {

            IGenPriority section;
            //gets the section that intersect the current ID
            if (Spawns.TryGetItem(zoneContext.CurrentID, out section))
            {
                queue.Enqueue(section.Priority, section.GetItem());
            }
        }

    }
}
