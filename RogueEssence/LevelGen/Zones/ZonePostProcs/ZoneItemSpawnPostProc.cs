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
    public class ZoneItemSpawnPostProc : ZonePostProc
    {
        public Priority Priority;

        [Dev.SubGroup]
        public SpawnRangeList<InvItem> Spawns;

        public ZoneItemSpawnPostProc()
        {
            Spawns = new SpawnRangeList<InvItem>();
        }

        protected ZoneItemSpawnPostProc(ZoneItemSpawnPostProc other, ulong seed) : this()
        {
            Spawns = other.Spawns;
            Priority = other.Priority;
        }
        public override ZonePostProc Instantiate(ulong seed) { return new ZoneItemSpawnPostProc(this, seed); }


        public override void Apply(ZoneGenContext zoneContext, IGenContext context, StablePriorityQueue<Priority, IGenStep> queue)
        {
            ItemSpawnStep<BaseMapGenContext> spawnStep = new ItemSpawnStep<BaseMapGenContext>();
            spawnStep.Spawns = Spawns.GetSpawnList(zoneContext.CurrentID);
            queue.Enqueue(Priority, spawnStep);
        }
    }
}
