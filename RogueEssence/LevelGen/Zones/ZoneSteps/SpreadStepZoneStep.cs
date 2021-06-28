using System;
using System.Collections.Generic;
using RogueElements;

namespace RogueEssence.LevelGen
{
    [Serializable]
    public class SpreadStepZoneStep : ZoneStep
    {
        public SpreadPlanBase SpreadPlan;
        public IRandPicker<IGenPriority> Spawns;

        [NonSerialized]
        public List<IGenPriority> DropItems;
        //spreads an item through the floors
        //ensures that the space in floors between occurrences is kept tame
        public SpreadStepZoneStep()
        {
        }

        public SpreadStepZoneStep(SpreadPlanBase plan, IRandPicker<IGenPriority> spawns) : this()
        {
            SpreadPlan = plan;
            Spawns = spawns;
        }

        protected SpreadStepZoneStep(SpreadStepZoneStep other, ulong seed) : this()
        {
            Spawns = other.Spawns.CopyState();
            SpreadPlan = other.SpreadPlan.Instantiate(seed);

            DropItems = new List<IGenPriority>();

            //Other SpredStep classes choose which step to place on which floor on the fly, but this one needs care, due to the potential of CanPick changing state
            for (int ii = 0; ii < SpreadPlan.DropPoints.Count; ii++)
            {
                if (!Spawns.CanPick)
                    break;

                ReRandom rand = new ReRandom(seed);
                IGenPriority genStep = Spawns.Pick(rand);
                DropItems.Add(genStep);
            }
        }
        public override ZoneStep Instantiate(ulong seed) { return new SpreadStepZoneStep(this, seed); }


        public override void Apply(ZoneGenContext zoneContext, IGenContext context, StablePriorityQueue<Priority, IGenStep> queue)
        {
            for (int ii = 0; ii < SpreadPlan.DropPoints.Count; ii++)
            {
                if (SpreadPlan.DropPoints[ii] != zoneContext.CurrentID)
                    continue;
                if (ii >= DropItems.Count)
                    continue;
                IGenPriority genStep = DropItems[ii];
                queue.Enqueue(genStep.Priority, genStep.GetItem());
            }
        }
    }
}
