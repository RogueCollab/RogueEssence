using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using RogueElements;
using RogueEssence.Data;
using RogueEssence.Dev;
using RogueEssence.Dungeon;

namespace RogueEssence.LevelGen
{
    /// <summary>
    /// Spreads a map gen step randomly across the dungeon segment.
    /// </summary>
    [Serializable]
    public class SpreadStepZoneStep : SpreadZoneStep
    {
        /// <summary>
        /// The steps to distribute.
        /// </summary>
        public IRandPicker<IGenPriority> Spawns;

        [NonSerialized]
        public List<IGenPriority> DropItems;

        public SpreadStepZoneStep()
        {
        }

        public SpreadStepZoneStep(SpreadPlanBase plan, IRandPicker<IGenPriority> spawns) : base(plan)
        {
            Spawns = spawns;
        }

        protected SpreadStepZoneStep(SpreadStepZoneStep other, ulong seed) : base(other, seed)
        {
            Spawns = other.Spawns.CopyState();

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


        protected override bool ApplyToFloor(ZoneGenContext zoneContext, IGenContext context, StablePriorityQueue<Priority, IGenStep> queue, int dropIdx)
        {
            if (dropIdx < -1)
            {
                //we don't know if changing the state of this step in a non-instantiation phase can lead to problems, stay on the safe side for now
                if (Spawns.ChangesState || !Spawns.CanPick)
                    return false;
                IGenPriority genStep = Spawns.Pick(context.Rand);
                queue.Enqueue(genStep.Priority, genStep.GetItem());
            }
            else
            {
                if (dropIdx >= DropItems.Count)
                    return false;
                IGenPriority genStep = DropItems[dropIdx];
                queue.Enqueue(genStep.Priority, genStep.GetItem());
            }
            return true;
        }

        public override string ToString()
        {
            int count = 0;
            IGenPriority singleGen = null;
            if (Spawns != null)
            {
                foreach (IGenPriority gen in Spawns.EnumerateOutcomes())
                {
                    count++;
                    singleGen = gen;
                }
            }
            if (count == 1)
                return string.Format("{0}: {1}", this.GetType().GetFormattedTypeName(), singleGen.ToString());
            return string.Format("{0}[{1}]", this.GetType().GetFormattedTypeName(), count);
        }
    }

    /// <summary>
    /// Spreads a map gen step randomly across the dungeon segment, allowing precise control over the spawn rate across different floors.
    /// </summary>
    [Serializable]
    public class SpreadStepRangeZoneStep : SpreadZoneStep
    {
        /// <summary>
        /// The steps to distribute.  Probabilities can be customized across floors.
        /// </summary>
        public SpawnRangeList<IGenPriority> Spawns;


        public SpreadStepRangeZoneStep()
        {
        }

        public SpreadStepRangeZoneStep(SpreadPlanBase plan, SpawnRangeList<IGenPriority> spawns) : base(plan)
        {
            Spawns = spawns;
        }

        protected SpreadStepRangeZoneStep(SpreadStepRangeZoneStep other, ulong seed) : base(other, seed)
        {
            Spawns = other.Spawns.CopyState();
        }
        public override ZoneStep Instantiate(ulong seed) { return new SpreadStepRangeZoneStep(this, seed); }


        protected override bool ApplyToFloor(ZoneGenContext zoneContext, IGenContext context, StablePriorityQueue<Priority, IGenStep> queue, int dropIdx)
        {
            SpawnList<IGenPriority> spawnList = Spawns.GetSpawnList(zoneContext.CurrentID);
            if (!spawnList.CanPick)
                return false;
            IGenPriority genStep = spawnList.Pick(context.Rand);
            queue.Enqueue(genStep.Priority, genStep.GetItem());
            return true;
        }

        public override string ToString()
        {
            int count = 0;
            IGenPriority singleGen = null;
            if (Spawns != null)
            {
                foreach (IGenPriority gen in Spawns.EnumerateOutcomes())
                {
                    count++;
                    singleGen = gen;
                }
            }
            if (count == 1)
                return string.Format("{0}: {1}", this.GetType().GetFormattedTypeName(), singleGen.ToString());
            return string.Format("{0}[{1}]", this.GetType().GetFormattedTypeName(), count);
        }
    }
}
