using System;
using System.Collections.Generic;
using RogueElements;
using RogueEssence.Dungeon;
using RogueEssence.Dev;

namespace RogueEssence.LevelGen
{
    /// <summary>
    /// Generates the table of items to spawn on all floors
    /// </summary>
    [Serializable]
    public class ZoneMoneySpawnPostProc : ZonePostProc
    {
        public Priority Priority;

        [Dev.RangeBorder(0, false, true)]
        public RandRange StartAmount;

        [Dev.RangeBorder(0, false, true)]
        public RandRange AddAmount;


        [NonSerialized]
        private int chosenStart;
        [NonSerialized]
        private int chosenAdd;

        public ZoneMoneySpawnPostProc()
        {
        }

        public ZoneMoneySpawnPostProc(Priority priority, RandRange start, RandRange add)
        {
            Priority = priority;
            StartAmount = start;
            AddAmount = add;
        }

        protected ZoneMoneySpawnPostProc(ZoneMoneySpawnPostProc other, ulong seed) : this()
        {
            StartAmount = other.StartAmount;
            AddAmount = other.AddAmount;
            Priority = other.Priority;

            ReRandom rand = new ReRandom(seed);
            chosenStart = StartAmount.Pick(rand);
            chosenAdd = AddAmount.Pick(rand);
        }
        public override ZonePostProc Instantiate(ulong seed) { return new ZoneMoneySpawnPostProc(this, seed); }


        public override void Apply(ZoneGenContext zoneContext, IGenContext context, StablePriorityQueue<Priority, IGenStep> queue)
        {
            RandRange amount = new RandRange(chosenStart + chosenAdd * zoneContext.CurrentID);
            queue.Enqueue(Priority, new MoneySpawnStep<BaseMapGenContext>(amount));
        }
    }
}
