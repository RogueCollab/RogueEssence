using System;
using System.Collections.Generic;
using RogueElements;

namespace RogueEssence.LevelGen
{
    [Serializable]
    public class SpreadOutPostProc : ZonePostProc
    {
        public RandRange FloorSpacing;
        public IntRange FloorRange;
        public SpawnList<IGenPriority> Spawns;

        [NonSerialized]
        private HashSet<int> dropPoints;

        //spreads an item through the floors
        //ensures that the space in floors between occurrences is kept tame
        public SpreadOutPostProc()
        {
            Spawns = new SpawnList<IGenPriority>();
        }

        public SpreadOutPostProc(RandRange spacing, IntRange floorRange) : this()
        {
            FloorSpacing = spacing;
            FloorRange = floorRange;
        }

        protected SpreadOutPostProc(SpreadOutPostProc other, ulong seed) : this()
        {
            Spawns = other.Spawns;

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
        public override ZonePostProc Instantiate(ulong seed) { return new SpreadOutPostProc(this, seed); }


        public override void Apply(ZoneGenContext zoneContext, IGenContext context, StablePriorityQueue<int, IGenStep> queue)
        {
            if (dropPoints.Contains(zoneContext.CurrentID))
            {
                IGenPriority genStep = Spawns.Pick(context.Rand);
                queue.Enqueue(genStep.Priority, genStep.GetItem());
            }
        }
    }
}
