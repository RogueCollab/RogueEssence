using System;
using RogueElements;
using System.Collections.Generic;
using RogueEssence.Dev;

namespace RogueEssence.LevelGen
{
    [Serializable]
    public class PoolTeamSpawner : TeamSpawner
    {
        /// <summary>
        /// Normal spawns.  Can be put in teams of any size at any quantity.
        /// </summary>
        [SubGroup]
        public SpawnList<MobSpawn> NormalSpawns;

        /// <summary>
        /// Only one can spawn in a team.  Any size team.
        /// </summary>
        [SubGroup]
        public SpawnList<MobSpawn> LeaderSpawns;

        /// <summary>
        /// Only one can spawn in a team.  Team of 1.
        /// </summary>
        [SubGroup]
        public SpawnList<MobSpawn> LonerSpawns;

        /// <summary>
        /// Only one can spawn in a team.  Team size > 1.
        /// </summary>
        [SubGroup]
        public SpawnList<MobSpawn> SupportSpawns;



        [SubGroup]
        public SpawnList<int> TeamSizes;

        public PoolTeamSpawner()
        {
            NormalSpawns = new SpawnList<MobSpawn>();
            LeaderSpawns = new SpawnList<MobSpawn>();
            LonerSpawns = new SpawnList<MobSpawn>();
            SupportSpawns = new SpawnList<MobSpawn>();
            TeamSizes = new SpawnList<int>();
        }
        protected PoolTeamSpawner(PoolTeamSpawner other)
        {
            NormalSpawns = new SpawnList<MobSpawn>();
            for(int ii = 0; ii < other.NormalSpawns.Count; ii++)
                NormalSpawns.Add(other.NormalSpawns.GetSpawn(ii).Copy(), other.NormalSpawns.GetSpawnRate(ii));
            LeaderSpawns = new SpawnList<MobSpawn>();
            for (int ii = 0; ii < other.LeaderSpawns.Count; ii++)
                LonerSpawns.Add(other.LeaderSpawns.GetSpawn(ii).Copy(), other.LeaderSpawns.GetSpawnRate(ii));
            LonerSpawns = new SpawnList<MobSpawn>();
            for (int ii = 0; ii < other.LonerSpawns.Count; ii++)
                LonerSpawns.Add(other.LonerSpawns.GetSpawn(ii).Copy(), other.LonerSpawns.GetSpawnRate(ii));
            SupportSpawns = new SpawnList<MobSpawn>();
            for (int ii = 0; ii < other.SupportSpawns.Count; ii++)
                SupportSpawns.Add(other.SupportSpawns.GetSpawn(ii).Copy(), other.SupportSpawns.GetSpawnRate(ii));
            TeamSizes = new SpawnList<int>();
            for (int ii = 0; ii < other.TeamSizes.Count; ii++)
                TeamSizes.Add(other.TeamSizes.GetSpawn(ii), other.TeamSizes.GetSpawnRate(ii));
        }
        public override TeamSpawner Clone() { return new PoolTeamSpawner(this); }

        public override SpawnList<MobSpawn> GetPossibleSpawns()
        {
            SpawnList<MobSpawn> spawnerList = new SpawnList<MobSpawn>();

            for (int ii = 0; ii < NormalSpawns.Count; ii++)
                spawnerList.Add(NormalSpawns.GetSpawn(ii), NormalSpawns.GetSpawnRate(ii));
            for (int ii = 0; ii < LeaderSpawns.Count; ii++)
                spawnerList.Add(LeaderSpawns.GetSpawn(ii), LeaderSpawns.GetSpawnRate(ii));
            for (int ii = 0; ii < LonerSpawns.Count; ii++)
                spawnerList.Add(LonerSpawns.GetSpawn(ii), LonerSpawns.GetSpawnRate(ii));
            for (int ii = 0; ii < SupportSpawns.Count; ii++)
                spawnerList.Add(SupportSpawns.GetSpawn(ii), SupportSpawns.GetSpawnRate(ii));

            return spawnerList;
        }
        public override List<MobSpawn> ChooseSpawns(IRandom rand)
        {
            List<MobSpawn> chosenSpawns = new List<MobSpawn>();

            if (!TeamSizes.CanPick)
                return chosenSpawns;
            int teamSize = TeamSizes.Pick(rand);

            bool selectedLeader = false;
            bool selectedSupport = false;

            //pick first team member
            List<SpawnList<MobSpawn>> eligibleSpawns = new List<SpawnList<MobSpawn>>
            {
                NormalSpawns, LeaderSpawns,
            };

            if (teamSize > 1)
                eligibleSpawns.Add(SupportSpawns);
            else
                eligibleSpawns.Add(LonerSpawns);

            SpawnList<MobSpawn> chosenList = chooseSpawnList(eligibleSpawns, rand);
            if (chosenList == LeaderSpawns)
                selectedLeader = true;
            else if (chosenList == SupportSpawns)
                selectedSupport = true;

            chosenSpawns.Add(chosenList.Pick(rand));

            //pick remaining team members
            for (int ii = 1; ii < teamSize; ii++)
            {
                eligibleSpawns = new List<SpawnList<MobSpawn>>();
                eligibleSpawns.Add(NormalSpawns);
                if (!selectedLeader)
                    eligibleSpawns.Add(LeaderSpawns);
                if (!selectedSupport)
                    eligibleSpawns.Add(SupportSpawns);


                chosenList = chooseSpawnList(eligibleSpawns, rand);
                if (chosenList == LeaderSpawns)
                    selectedLeader = true;
                else if (chosenList == SupportSpawns)
                    selectedSupport = true;

                chosenSpawns.Add(chosenList.Pick(rand));
            }

            return chosenSpawns;
        }

        private SpawnList<MobSpawn> chooseSpawnList(List<SpawnList<MobSpawn>> eligibleSpawns, IRandom rand)
        {
            int totalSpawn = 0;
            foreach (SpawnList<MobSpawn> spawn in eligibleSpawns)
                totalSpawn += spawn.SpawnTotal;

            int pickedSpawn = rand.Next(totalSpawn);

            totalSpawn = 0;
            foreach (SpawnList<MobSpawn> spawn in eligibleSpawns)
            {
                totalSpawn += spawn.SpawnTotal;
                if (totalSpawn > pickedSpawn)
                    return spawn;
            }
            return null;
        }
    }
}
