using System;
using RogueElements;
using System.Collections.Generic;
using RogueEssence.Dev;

namespace RogueEssence.LevelGen
{
    [Serializable]
    public class PoolTeamSpawner : TeamSpawner
    {
        [SubGroup]
        public SpawnList<MobSpawn> LeaderSpawns;
        [SubGroup]
        public SpawnList<MobSpawn> LonerSpawns;
        [SubGroup]
        public SpawnList<MobSpawn> PartnerSpawns;
        [SubGroup]
        public SpawnList<int> TeamSizes;

        public PoolTeamSpawner()
        {
            LeaderSpawns = new SpawnList<MobSpawn>();
            LonerSpawns = new SpawnList<MobSpawn>();
            PartnerSpawns = new SpawnList<MobSpawn>();
            TeamSizes = new SpawnList<int>();
        }
        protected PoolTeamSpawner(PoolTeamSpawner other)
        {
            LeaderSpawns = new SpawnList<MobSpawn>();
            for(int ii = 0; ii < other.LeaderSpawns.Count; ii++)
                LeaderSpawns.Add(other.LeaderSpawns.GetSpawn(ii).Copy(), other.LeaderSpawns.GetSpawnRate(ii));
            LonerSpawns = new SpawnList<MobSpawn>();
            for (int ii = 0; ii < other.LonerSpawns.Count; ii++)
                LonerSpawns.Add(other.LonerSpawns.GetSpawn(ii).Copy(), other.LonerSpawns.GetSpawnRate(ii));
            PartnerSpawns = new SpawnList<MobSpawn>();
            for (int ii = 0; ii < other.PartnerSpawns.Count; ii++)
                PartnerSpawns.Add(other.PartnerSpawns.GetSpawn(ii).Copy(), other.PartnerSpawns.GetSpawnRate(ii));
            TeamSizes = new SpawnList<int>();
            for (int ii = 0; ii < other.TeamSizes.Count; ii++)
                TeamSizes.Add(other.TeamSizes.GetSpawn(ii), other.TeamSizes.GetSpawnRate(ii));
        }
        public override TeamSpawner Clone() { return new PoolTeamSpawner(this); }

        public override SpawnList<MobSpawn> GetPossibleSpawns()
        {
            SpawnList<MobSpawn> spawnerList = new SpawnList<MobSpawn>();

            for (int ii = 0; ii < LeaderSpawns.Count; ii++)
                spawnerList.Add(LeaderSpawns.GetSpawn(ii), LeaderSpawns.GetSpawnRate(ii));
            for (int ii = 0; ii < LonerSpawns.Count; ii++)
                spawnerList.Add(LonerSpawns.GetSpawn(ii), LonerSpawns.GetSpawnRate(ii));
            for (int ii = 0; ii < PartnerSpawns.Count; ii++)
                spawnerList.Add(PartnerSpawns.GetSpawn(ii), PartnerSpawns.GetSpawnRate(ii));

            return spawnerList;
        }
        public override List<MobSpawn> ChooseSpawns(IRandom rand)
        {
            int totalSpawn = rand.Next(LeaderSpawns.SpawnTotal + LonerSpawns.SpawnTotal + PartnerSpawns.SpawnTotal);

            List<MobSpawn> chosenSpawns = new List<MobSpawn>();
            if (totalSpawn < LonerSpawns.SpawnTotal)
                chosenSpawns.Add(LonerSpawns.Pick(rand));
            else
            {
                int teamSize = TeamSizes.Pick(rand);
                if (totalSpawn < LeaderSpawns.SpawnTotal)
                {

                }
                else
                {
                    if (teamSize < 2)
                        teamSize = 2;
                }
                for (int ii = 0; ii < teamSize; ii++)
                {
                    int limitedSpawn = rand.Next(LeaderSpawns.SpawnTotal + PartnerSpawns.SpawnTotal);
                    if (limitedSpawn < LeaderSpawns.SpawnTotal)
                        chosenSpawns.Add(LeaderSpawns.Pick(rand));
                    else if (limitedSpawn < LeaderSpawns.SpawnTotal + PartnerSpawns.SpawnTotal)
                        chosenSpawns.Add(PartnerSpawns.Pick(rand));
                }
            }
            return chosenSpawns;
        }
    }
}
