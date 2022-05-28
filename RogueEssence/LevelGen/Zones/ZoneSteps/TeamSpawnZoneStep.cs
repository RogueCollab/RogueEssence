using System;
using System.Collections.Generic;
using RogueElements;
using RogueEssence.Dungeon;
using RogueEssence.Dev;

namespace RogueEssence.LevelGen
{
    /// <summary>
    /// Generates the table of mobs to spawn on all floors.
    /// </summary>
    [Serializable]
    public class TeamSpawnZoneStep : ZoneStep
    {
        /// <summary>
        /// At what point in the map gen process to run the mob spawning in.
        /// </summary>
        public Priority Priority;

        /// <summary>
        /// The encounter table for mobs across all floors of the dungeon segment.
        /// When spawning, the chosen mobs will be grouped into teams of a size described in Team Sizes.
        /// </summary>
        [SubGroup]
        [RangeBorder(0, true, true)]
        [Dev.EditorHeight(0, 290)]
        public SpawnRangeList<TeamMemberSpawn> Spawns;

        /// <summary>
        /// The size of teams across all floors of the dungeon segment.
        /// </summary>
        [SubGroup]
        [RangeBorder(0, true, true)]
        public SpawnRangeList<int> TeamSizes;

        /// <summary>
        /// Pre-made teams and their spawn chances across floors.
        /// </summary>
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
