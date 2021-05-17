using System;
using System.Collections.Generic;
using RogueElements;

namespace RogueEssence.LevelGen
{
    [Serializable]
    public class SpreadStepZoneStep : ZoneStep
    {
        public SpreadPlanBase SpreadPlan;
        public SpawnList<IGenPriority> Spawns;

        //spreads an item through the floors
        //ensures that the space in floors between occurrences is kept tame
        public SpreadStepZoneStep()
        {
            Spawns = new SpawnList<IGenPriority>();
        }

        public SpreadStepZoneStep(SpreadPlanBase plan) : this()
        {
            SpreadPlan = plan;
        }

        protected SpreadStepZoneStep(SpreadStepZoneStep other, ulong seed) : this()
        {
            Spawns = other.Spawns;
            SpreadPlan = other.SpreadPlan.Instantiate(seed);
        }
        public override ZoneStep Instantiate(ulong seed) { return new SpreadStepZoneStep(this, seed); }


        public override void Apply(ZoneGenContext zoneContext, IGenContext context, StablePriorityQueue<Priority, IGenStep> queue)
        {
            if (SpreadPlan.CheckIfDistributed(zoneContext, context))
            {
                IGenPriority genStep = Spawns.Pick(context.Rand);
                queue.Enqueue(genStep.Priority, genStep.GetItem());
            }
        }
    }
}
