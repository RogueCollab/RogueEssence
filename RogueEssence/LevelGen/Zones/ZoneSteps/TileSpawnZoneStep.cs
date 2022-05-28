using System;
using System.Collections.Generic;
using RogueElements;
using RogueEssence.Dungeon;
using RogueEssence.Dev;

namespace RogueEssence.LevelGen
{
    /// <summary>
    /// Generates the table of trap tiles to spawn on all floors
    /// </summary>
    [Serializable]
    public class TileSpawnZoneStep : ZoneStep
    {
        /// <summary>
        /// At what point in the map gen process to run the trap spawning in.
        /// </summary>
        public Priority Priority;

        /// <summary>
        /// The encounter table for traps across all floors of the dungeon segment.
        /// </summary>
        [SubGroup]
        [RangeBorder(0, true, true)]
        public SpawnRangeList<EffectTile> Spawns;

        //range list for weights

        public TileSpawnZoneStep()
        {
            Spawns = new SpawnRangeList<EffectTile>();
        }

        protected TileSpawnZoneStep(TileSpawnZoneStep other, ulong seed) : this()
        {
            Spawns = other.Spawns;
            Priority = other.Priority;
        }
        public override ZoneStep Instantiate(ulong seed) { return new TileSpawnZoneStep(this, seed); }


        public override void Apply(ZoneGenContext zoneContext, IGenContext context, StablePriorityQueue<Priority, IGenStep> queue)
        {
            TileSpawnStep<BaseMapGenContext> spawnStep = new TileSpawnStep<BaseMapGenContext>();

            SpawnList<EffectTile> spawner = Spawns.GetSpawnList(zoneContext.CurrentID);
            for (int ii = 0; ii < spawner.Count; ii++)
                spawnStep.Spawns.Add(spawner.GetSpawn(ii), spawner.GetSpawnRate(ii));

            queue.Enqueue(Priority, spawnStep);
        }

        public override string ToString()
        {
            return string.Format("{0}[{1}]", this.GetType().Name, Spawns.Count.ToString());
        }
    }
}
