using System;
using System.Collections.Generic;
using RogueElements;

namespace RogueEssence.LevelGen
{
    [Serializable]
    public abstract class SpreadPlanBase
    {
        public IntRange FloorRange;

        public SpreadPlanBase(IntRange floorRange)
        {
            FloorRange = floorRange;
        }

        protected SpreadPlanBase(SpreadPlanBase other, ulong seed)
        {
            FloorRange = other.FloorRange;
        }
        public abstract SpreadPlanBase Instantiate(ulong seed);

        public abstract bool CheckIfDistributed(ZoneGenContext zoneContext, IGenContext context);
    }


    /// <summary>
    /// Spreads the item across floors by rolling a fixed chance for each
    /// </summary>
    [Serializable]
    public class SpreadPlanChance : SpreadPlanBase
    {
        /// <summary>
        /// In percent
        /// </summary>
        public int Chance;

        public SpreadPlanChance(int chance, IntRange floorRange) : base(floorRange)
        {
            Chance = chance;
        }

        protected SpreadPlanChance(SpreadPlanChance other, ulong seed) : base(other, seed)
        {
            Chance = other.Chance;
        }
        public override SpreadPlanBase Instantiate(ulong seed) { return new SpreadPlanChance(this, seed); }

        public override bool CheckIfDistributed(ZoneGenContext zoneContext, IGenContext context)
        {
            return (FloorRange.Contains(zoneContext.CurrentID) && context.Rand.Next(100) < Chance);
        }
    }

    /// <summary>
    /// Spreads the item across floors with specified spacing between
    /// </summary>
    [Serializable]
    public class SpreadPlanSpaced : SpreadPlanBase
    {
        public RandRange FloorSpacing;

        [NonSerialized]
        private HashSet<int> dropPoints;

        public SpreadPlanSpaced(RandRange spacing, IntRange floorRange) : base(floorRange)
        {
            FloorSpacing = spacing;
        }

        protected SpreadPlanSpaced(SpreadPlanSpaced other, ulong seed) : base(other, seed)
        {
            FloorSpacing = other.FloorSpacing;
            FloorRange = other.FloorRange;

            ReRandom rand = new ReRandom(seed);
            dropPoints = new HashSet<int>();

            int currentFloor = FloorRange.Min;
            currentFloor += rand.Next(FloorSpacing.Max);

            while (currentFloor < FloorRange.Max)
            {
                dropPoints.Add(currentFloor);
                currentFloor += FloorSpacing.Pick(rand);
            }
        }
        public override SpreadPlanBase Instantiate(ulong seed) { return new SpreadPlanSpaced(this, seed); }

        public override bool CheckIfDistributed(ZoneGenContext zoneContext, IGenContext context)
        {
            return dropPoints.Contains(zoneContext.CurrentID);
        }
    }

    /// <summary>
    /// Spreads the item across floors by randomly distributing across them without repetition
    /// </summary>
    [Serializable]
    public class SpreadPlanQuota : SpreadPlanBase
    {
        public IRandPicker<int> Quota;

        [NonSerialized]
        private HashSet<int> dropPoints;

        public SpreadPlanQuota(IRandPicker<int> quota, IntRange floorRange) : base(floorRange)
        {
            Quota = quota;
        }

        protected SpreadPlanQuota(SpreadPlanQuota other, ulong seed) : base(other, seed)
        {
            Quota = other.Quota;

            ReRandom rand = new ReRandom(seed);
            int chosenAmount = Quota.Pick(rand);
            dropPoints = new HashSet<int>();

            List<int> availableFloors = new List<int>();
            for (int ii = FloorRange.Min; ii < FloorRange.Max; ii++)
                availableFloors.Add(ii);

            while (availableFloors.Count > 0 && dropPoints.Count < chosenAmount)
            {
                int chosenIndex = rand.Next(availableFloors.Count);
                dropPoints.Add(availableFloors[chosenIndex]);
                availableFloors.RemoveAt(chosenIndex);
            }

        }
        public override SpreadPlanBase Instantiate(ulong seed) { return new SpreadPlanQuota(this, seed); }

        public override bool CheckIfDistributed(ZoneGenContext zoneContext, IGenContext context)
        {
            return dropPoints.Contains(zoneContext.CurrentID);
        }
    }

}
