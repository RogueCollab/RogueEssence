using System;
using RogueElements;

namespace RogueEssence.LevelGen
{
    [Serializable]
    public class FloorChancePostProc : ZonePostProc
    {
        public int Chance;
        public IntRange FloorRange;
        public SpawnList<GenPriority<IGenStep>> Spawns;

        //spreads an item through the floors
        //ensures that the space in floors between occurrences is kept tame
        public FloorChancePostProc()
        {
            Spawns = new SpawnList<GenPriority<IGenStep>>();
        }

        public FloorChancePostProc(int chance, IntRange floorRange) : this()
        {
            Chance = chance;
            FloorRange = floorRange;
        }
        protected FloorChancePostProc(FloorChancePostProc other, ulong seed)
        {
            Spawns = other.Spawns;

            Chance = other.Chance;
            FloorRange = other.FloorRange;
        }

        public override ZonePostProc Instantiate(ulong seed) { return new FloorChancePostProc(this, seed); }

        public override void Apply(ZoneGenContext zoneContext, IGenContext context, StablePriorityQueue<int, IGenStep> queue)
        {
            if (FloorRange.Contains(zoneContext.CurrentID) && context.Rand.Next(100) < Chance)
            {
                //TODO: add a way to clamp the type of mapgencontext processed up to BaseMapGen
                GenPriority<IGenStep> step = Spawns.Pick(context.Rand);
                queue.Enqueue(step.Priority, step.Item);
            }
        }
    }
}
