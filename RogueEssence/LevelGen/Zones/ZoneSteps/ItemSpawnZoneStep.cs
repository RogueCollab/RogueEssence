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
        /// <summary>
        /// The objects to spawn within this category.
        /// Assuming that this category was chosen for spawning,
        /// the chance that an object will spawn on a given floor is its spawn rate for that floor,
        /// divided by the sum of all objects' spawn rates on that floor.
        /// </summary>
        [RangeBorder(0, true, true)]
        [EditorHeight(0, 290)]
        public SpawnRangeList<T> Spawns;

        /// <summary>
        /// The spawn rate of the entire category across the entire dungeon segment.
        /// The chance that this category is chosen on a given floor is the spawn rate for that floor,
        /// divided by the sum of all categories' spawn rates on that floor.
        /// </summary>
        [RangeBorder(0, true, true)]
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
    /// Generates the table of items to spawn on all floors.
    /// </summary>
    [Serializable]
    public class ItemSpawnZoneStep : ZoneStep
    {
        /// <summary>
        /// At what point in the map gen process to run the item spawning in.
        /// </summary>
        public Priority Priority;

        /// <summary>
        /// The spawn table, organized by category.
        /// </summary>
        [Dev.SubGroup]
        [Dev.EditorHeight(0, 260)]
        public Dictionary<string, CategorySpawn<InvItem>> Spawns;

        public ItemSpawnZoneStep()
        {
            Spawns = new Dictionary<string, CategorySpawn<InvItem>>();
        }

        protected ItemSpawnZoneStep(ItemSpawnZoneStep other, ulong seed) : this()
        {
            Spawns = other.Spawns;
            Priority = other.Priority;
        }
        public override ZoneStep Instantiate(ulong seed) { return new ItemSpawnZoneStep(this, seed); }


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


    /// <summary>
    /// Generates the table of items to spawn on all floors.  Breaks them into sections such that probability is easier.
    /// </summary>
    [Serializable]
    public class ItemSectionedZoneStep : ZoneStep
    {
        /// <summary>
        /// At what point in the map gen process to run the item spawning in.
        /// </summary>
        public Priority Priority;

        /// <summary>
        /// The spawn table, organized by category.
        /// </summary>
        [SubGroup]
        [EditorHeight(0, 360)]
        [EditorHeight(1, 360)]
        [RangeBorder(0, true, true)]
        public RangeDict<SpawnDict<string, SpawnList<InvItem>>> Spawns;

        public ItemSectionedZoneStep()
        {
            Spawns = new RangeDict<SpawnDict<string, SpawnList<InvItem>>>();
        }

        protected ItemSectionedZoneStep(ItemSectionedZoneStep other, ulong seed) : this()
        {
            Spawns = other.Spawns;
            Priority = other.Priority;
        }
        public override ZoneStep Instantiate(ulong seed) { return new ItemSectionedZoneStep(this, seed); }


        public override void Apply(ZoneGenContext zoneContext, IGenContext context, StablePriorityQueue<Priority, IGenStep> queue)
        {
            SpawnDict<string, SpawnList<InvItem>> section;
            //gets the section that intersect the current ID
            if (Spawns.TryGetItem(zoneContext.CurrentID, out section))
            {
                ItemSpawnStep<BaseMapGenContext> spawnStep = new ItemSpawnStep<BaseMapGenContext>();
                spawnStep.Spawns = section;
                queue.Enqueue(Priority, spawnStep);
            }
        }

        public override string ToString()
        {
            return string.Format("{0}[{1}]", this.GetType().Name, this.Spawns.RangeCount.ToString());
        }
    }
}
