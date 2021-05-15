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
    public class ZoneTeamSpawnPostProc : ZonePostProc
    {
        public Priority Priority;

        [SubGroup]
        [RangeBorder(0, true, true)]
        public SpawnRangeList<MobSpawn> NormalSpawns;
        [SubGroup]
        [RangeBorder(0, true, true)]
        public SpawnRangeList<MobSpawn> LonerSpawns;
        [SubGroup]
        [RangeBorder(0, true, true)]
        public SpawnRangeList<MobSpawn> LeaderSpawns;
        [SubGroup]
        [RangeBorder(0, true, true)]
        public SpawnRangeList<MobSpawn> SupportSpawns;
        [SubGroup]
        [RangeBorder(0, true, true)]
        public SpawnRangeList<int> TeamSizes;

        [SubGroup]
        [RangeBorder(0, true, true)]
        public SpawnRangeList<SpecificTeamSpawner> SpecificSpawns;

        //range list for weights

        public ZoneTeamSpawnPostProc()
        {
            NormalSpawns = new SpawnRangeList<MobSpawn>();
            LonerSpawns = new SpawnRangeList<MobSpawn>();
            LeaderSpawns = new SpawnRangeList<MobSpawn>();
            SupportSpawns = new SpawnRangeList<MobSpawn>();
            TeamSizes = new SpawnRangeList<int>();
            SpecificSpawns = new SpawnRangeList<SpecificTeamSpawner>();
        }

        protected ZoneTeamSpawnPostProc(ZoneTeamSpawnPostProc other, ulong seed) : this()
        {
            NormalSpawns = other.NormalSpawns;
            LonerSpawns = other.LonerSpawns;
            LeaderSpawns = other.LeaderSpawns;
            SupportSpawns = other.SupportSpawns;
            TeamSizes = other.TeamSizes;
            SpecificSpawns = other.SpecificSpawns;
            Priority = other.Priority;
        }
        public override ZonePostProc Instantiate(ulong seed) { return new ZoneTeamSpawnPostProc(this, seed); }


        public override void Apply(ZoneGenContext zoneContext, IGenContext context, StablePriorityQueue<Priority, IGenStep> queue)
        {
            MobSpawnStep<BaseMapGenContext> spawnStep = new MobSpawnStep<BaseMapGenContext>();

            PoolTeamSpawner spawner = new PoolTeamSpawner();
            spawner.NormalSpawns = NormalSpawns.GetSpawnList(zoneContext.CurrentID);
            spawner.LonerSpawns = LonerSpawns.GetSpawnList(zoneContext.CurrentID);
            spawner.LeaderSpawns = LeaderSpawns.GetSpawnList(zoneContext.CurrentID);
            spawner.SupportSpawns = SupportSpawns.GetSpawnList(zoneContext.CurrentID);
            spawner.TeamSizes = TeamSizes.GetSpawnList(zoneContext.CurrentID);
            spawnStep.Spawns.Add(spawner, spawner.NormalSpawns.SpawnTotal + spawner.LeaderSpawns.SpawnTotal + spawner.LonerSpawns.SpawnTotal);

            SpawnList<SpecificTeamSpawner> specificSpawner = SpecificSpawns.GetSpawnList(zoneContext.CurrentID);
            for (int ii = 0; ii < specificSpawner.Count; ii++)
                spawnStep.Spawns.Add(specificSpawner.GetSpawn(ii), specificSpawner.GetSpawnRate(ii));

            queue.Enqueue(Priority, spawnStep);
        }

        public override string ToString()
        {
            int totalCount = 0;
            totalCount += NormalSpawns.Count;
            totalCount += LonerSpawns.Count;
            totalCount += LeaderSpawns.Count;
            totalCount += SupportSpawns.Count;
            return string.Format("{0}[{1}] Spec:{2}", this.GetType().Name, totalCount.ToString(), this.SpecificSpawns.Count.ToString());
        }
    }
}
