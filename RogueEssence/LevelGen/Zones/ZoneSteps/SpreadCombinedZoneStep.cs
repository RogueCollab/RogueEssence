using System;
using System.Collections.Generic;
using RogueElements;

namespace RogueEssence.LevelGen
{
    /// <summary>
    /// Combines zonesteps that spread across floors, ensuring none of them collide by deleting the drop point from the later collision.
    /// First step takes priority over second step, etc.
    /// </summary>
    [Serializable]
    public class SpreadCombinedZoneStep : ZoneStep
    {
        /// <summary>
        /// The steps to distribute.
        /// </summary>
        public List<SpreadZoneStep> Steps;

        public SpreadCombinedZoneStep()
        {
            Steps = new List<SpreadZoneStep>();
        }

        protected SpreadCombinedZoneStep(SpreadCombinedZoneStep other, ulong seed)
        {
            Steps = new List<SpreadZoneStep>();
            HashSet<int> bannedFloors = new HashSet<int>();
            foreach (SpreadZoneStep step in other.Steps)
            {
                //instantiate but with restrictions
                SpreadZoneStep newStep = (SpreadZoneStep)step.Instantiate(seed);
                //removed the banned floors
                //NOTE: this does not account for two occurrences for one floor from the same step.
                for (int ii = newStep.SpreadPlan.DropPoints.Count - 1; ii >= 0; ii--)
                {
                    if (bannedFloors.Contains(newStep.SpreadPlan.DropPoints[ii]))
                        newStep.SpreadPlan.DropPoints.RemoveAt(ii);
                    else
                        bannedFloors.Add(newStep.SpreadPlan.DropPoints[ii]);
                }
                Steps.Add(newStep);
            }
        }
        public override ZoneStep Instantiate(ulong seed) { return new SpreadCombinedZoneStep(this, seed); }

        public override void Apply(ZoneGenContext zoneContext, IGenContext context, StablePriorityQueue<Priority, IGenStep> queue)
        {
            foreach (SpreadZoneStep step in Steps)
                step.Apply(zoneContext, context, queue);
        }
    }
}
