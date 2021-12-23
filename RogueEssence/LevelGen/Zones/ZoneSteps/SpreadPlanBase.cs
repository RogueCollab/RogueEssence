using System;
using System.Collections.Generic;
using RogueElements;
using RogueEssence.Dev;

namespace RogueEssence.LevelGen
{
    [Serializable]
    public abstract class SpreadPlanBase
    {
        [RangeBorder(0, true, true)]
        public IntRange FloorRange;


        [NonSerialized]
        public List<int> DropPoints;


        public SpreadPlanBase()
        {
            DropPoints = new List<int>();
        }

        public SpreadPlanBase(IntRange floorRange)
        {
            FloorRange = floorRange;
            DropPoints = new List<int>();
        }

        protected SpreadPlanBase(SpreadPlanBase other, ulong seed)
        {
            FloorRange = other.FloorRange;
            DropPoints = new List<int>();
        }
        public abstract SpreadPlanBase Instantiate(ulong seed);
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

            ReRandom rand = new ReRandom(seed);
            int currentFloor = FloorRange.Min;

            while (currentFloor < FloorRange.Max)
            {
                if (rand.Next(100) < Chance)
                    DropPoints.Add(currentFloor);
                currentFloor++;
            }
        }
        public override SpreadPlanBase Instantiate(ulong seed) { return new SpreadPlanChance(this, seed); }
    }

    /// <summary>
    /// Spreads the item across floors with specified spacing between
    /// </summary>
    [Serializable]
    public class SpreadPlanSpaced : SpreadPlanBase
    {
        public RandRange FloorSpacing;

        public SpreadPlanSpaced() { }
        public SpreadPlanSpaced(RandRange spacing, IntRange floorRange) : base(floorRange)
        {
            FloorSpacing = spacing;
        }

        protected SpreadPlanSpaced(SpreadPlanSpaced other, ulong seed) : base(other, seed)
        {
            FloorSpacing = other.FloorSpacing;
            FloorRange = other.FloorRange;

            ReRandom rand = new ReRandom(seed);

            int currentFloor = FloorRange.Min;
            currentFloor += rand.Next(FloorSpacing.Max);

            while (currentFloor < FloorRange.Max)
            {
                DropPoints.Add(currentFloor);
                currentFloor += FloorSpacing.Pick(rand);
            }
        }
        public override SpreadPlanBase Instantiate(ulong seed) { return new SpreadPlanSpaced(this, seed); }
    }

    /// <summary>
    /// Spreads the item across floors by randomly distributing across them without repetition
    /// </summary>
    [Serializable]
    public class SpreadPlanQuota : SpreadPlanBase
    {
        public IRandPicker<int> Quota;
        public bool Replaceable;

        public SpreadPlanQuota() { Quota = new RandRange(); }

        public SpreadPlanQuota(IRandPicker<int> quota, IntRange floorRange) : base(floorRange)
        {
            Quota = quota;
        }
        public SpreadPlanQuota(IRandPicker<int> quota, IntRange floorRange, bool replaceable) : base(floorRange)
        {
            Quota = quota;
            Replaceable = replaceable;
        }

        protected SpreadPlanQuota(SpreadPlanQuota other, ulong seed) : base(other, seed)
        {
            Quota = other.Quota;
            Replaceable = other.Replaceable;

            ReRandom rand = new ReRandom(seed);
            int chosenAmount = Quota.Pick(rand);

            List<int> availableFloors = new List<int>();
            for (int ii = FloorRange.Min; ii < FloorRange.Max; ii++)
                availableFloors.Add(ii);

            while (availableFloors.Count > 0 && DropPoints.Count < chosenAmount)
            {
                int chosenIndex = rand.Next(availableFloors.Count);
                DropPoints.Add(availableFloors[chosenIndex]);
                if (!Replaceable)
                    availableFloors.RemoveAt(chosenIndex);
            }
        }
        public override SpreadPlanBase Instantiate(ulong seed) { return new SpreadPlanQuota(this, seed); }
    }

}
