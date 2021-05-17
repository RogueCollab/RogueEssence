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
    public class TeamSpawnZoneStep : ZoneStep
    {
        public Priority Priority;

        [SubGroup]
        [RangeBorder(0, true, true)]
        public SpawnRangeList<TeamMemberSpawn> Spawns;
        [SubGroup]
        [RangeBorder(0, true, true)]
        public SpawnRangeList<int> TeamSizes;

        [SubGroup]
        [RangeBorder(0, true, true)]
        public SpawnRangeList<SpecificTeamSpawner> SpecificSpawns;

        //range list for weights

        public TeamSpawnZoneStep()
        {
            Spawns = new SpawnRangeList<TeamMemberSpawn>();
            TeamSizes = new SpawnRangeList<int>();
            SpecificSpawns = new SpawnRangeList<SpecificTeamSpawner>();
        }

        protected TeamSpawnZoneStep(TeamSpawnZoneStep other, ulong seed) : this()
        {
            Spawns = other.Spawns;
            TeamSizes = other.TeamSizes;
            SpecificSpawns = other.SpecificSpawns;
            Priority = other.Priority;
        }
        public override ZoneStep Instantiate(ulong seed) { return new TeamSpawnZoneStep(this, seed); }


        public override void Apply(ZoneGenContext zoneContext, IGenContext context, StablePriorityQueue<Priority, IGenStep> queue)
        {
            MobSpawnStep<BaseMapGenContext> spawnStep = new MobSpawnStep<BaseMapGenContext>();

            PoolTeamSpawner spawner = new PoolTeamSpawner();
            spawner.Spawns = Spawns.GetSpawnList(zoneContext.CurrentID);
            spawner.TeamSizes = TeamSizes.GetSpawnList(zoneContext.CurrentID);
            spawnStep.Spawns.Add(spawner, spawner.Spawns.SpawnTotal);

            SpawnList<SpecificTeamSpawner> specificSpawner = SpecificSpawns.GetSpawnList(zoneContext.CurrentID);
            for (int ii = 0; ii < specificSpawner.Count; ii++)
                spawnStep.Spawns.Add(specificSpawner.GetSpawn(ii), specificSpawner.GetSpawnRate(ii));

            queue.Enqueue(Priority, spawnStep);
        }

        public override string ToString()
        {
            return string.Format("{0}[{1}] Spec:{2}", this.GetType().Name, Spawns.Count.ToString(), this.SpecificSpawns.Count.ToString());
        }
    }
}
