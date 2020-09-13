using System;
using System.Collections.Generic;
using RogueElements;

namespace RogueEssence.LevelGen
{
    [Serializable]
    public class SpreadStepPostProc : ZonePostProc
    {
        public SpreadPlanBase SpreadPlan;
        public SpawnList<IGenPriority> Spawns;

        //spreads an item through the floors
        //ensures that the space in floors between occurrences is kept tame
        public SpreadStepPostProc()
        {
            Spawns = new SpawnList<IGenPriority>();
        }

        public SpreadStepPostProc(SpreadPlanBase plan) : this()
        {
            SpreadPlan = plan;
        }

        protected SpreadStepPostProc(SpreadStepPostProc other, ulong seed) : this()
        {
            Spawns = other.Spawns;
            SpreadPlan = other.SpreadPlan.Instantiate(seed);
        }
        public override ZonePostProc Instantiate(ulong seed) { return new SpreadStepPostProc(this, seed); }


        public override void Apply(ZoneGenContext zoneContext, IGenContext context, StablePriorityQueue<int, IGenStep> queue)
        {
            if (SpreadPlan.CheckIfDistributed(zoneContext, context))
            {
                IGenPriority genStep = Spawns.Pick(context.Rand);
                queue.Enqueue(genStep.Priority, genStep.GetItem());
            }
        }
    }
}
