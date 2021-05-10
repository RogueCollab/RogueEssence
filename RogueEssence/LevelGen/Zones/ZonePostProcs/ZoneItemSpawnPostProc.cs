using System;
using System.Collections.Generic;
using RogueElements;
using RogueEssence.Dungeon;
using RogueEssence.Dev;

namespace RogueEssence.LevelGen
{
    [Serializable]
    public class CategorySpawn<T>
    {
        public SpawnRangeList<T> Spawns;
        public RangeDict<int> SpawnRates;

        public CategorySpawn()
        {
            Spawns = new SpawnRangeList<T>();
            SpawnRates = new RangeDict<int>();
        }

        public override string ToString()
        {
            return string.Format("{0}[{1}]", this.GetType().Name, this.Spawns.Count.ToString());
        }
    }


    /// <summary>
    /// Generates the table of items to spawn on all floors
    /// </summary>
    [Serializable]
    public class ZoneItemSpawnPostProc : ZonePostProc
    {
        public Priority Priority;

        [Dev.SubGroup]
        public Dictionary<string, CategorySpawn<InvItem>> Spawns;

        public ZoneItemSpawnPostProc()
        {
            Spawns = new Dictionary<string, CategorySpawn<InvItem>>();
        }

        protected ZoneItemSpawnPostProc(ZoneItemSpawnPostProc other, ulong seed) : this()
        {
            Spawns = other.Spawns;
            Priority = other.Priority;
        }
        public override ZonePostProc Instantiate(ulong seed) { return new ZoneItemSpawnPostProc(this, seed); }


        public override void Apply(ZoneGenContext zoneContext, IGenContext context, StablePriorityQueue<Priority, IGenStep> queue)
        {
            SpawnDict<string, SpawnList<InvItem>> spawns = new SpawnDict<string, SpawnList<InvItem>>();
            //contains all LISTS that intersect the current ID
            foreach (string key in Spawns.Keys)
            {
                //get all items within the spawnrangelist that intersect the current ID
                SpawnList<InvItem> slicedList = Spawns[key].Spawns.GetSpawnList(zoneContext.CurrentID);

                // add the spawnlist under the current key, with the key having the spawnrate for this id
                if (slicedList.CanPick && Spawns[key].SpawnRates.ContainsItem(zoneContext.CurrentID) && Spawns[key].SpawnRates[zoneContext.CurrentID] > 0)
                    spawns.Add(key, slicedList, Spawns[key].SpawnRates[zoneContext.CurrentID]);
            }

            ItemSpawnStep<BaseMapGenContext> spawnStep = new ItemSpawnStep<BaseMapGenContext>();
            spawnStep.Spawns = spawns;
            queue.Enqueue(Priority, spawnStep);
        }

        public override string ToString()
        {
            return string.Format("{0}[{1}]", this.GetType().Name, this.Spawns.Count.ToString());
        }
    }
}
