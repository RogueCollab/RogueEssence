using System;
using System.Collections.Generic;
using RogueElements;

namespace RogueEssence.LevelGen
{
    [Serializable]
    public class SpecificTeamSpawner : TeamSpawner
    {
        public List<MobSpawn> Spawns;

        public SpecificTeamSpawner(params MobSpawn[] spawners)
        {
            Spawns = new List<MobSpawn>();
            Spawns.AddRange(spawners);
        }
        protected SpecificTeamSpawner(SpecificTeamSpawner other)
        {
            Spawns = new List<MobSpawn>();
            foreach (MobSpawn spawner in other.Spawns)
                Spawns.Add(spawner.Copy());
        }
        public override TeamSpawner Clone() { return new SpecificTeamSpawner(this); }

        public override SpawnList<MobSpawn> GetPossibleSpawns()
        {
            SpawnList<MobSpawn> spawnerList = new SpawnList<MobSpawn>();
            foreach (MobSpawn spawner in Spawns)
                spawnerList.Add(spawner, 100);

            return spawnerList;
        }

        public override List<MobSpawn> ChooseSpawns(IRandom rand)
        {
            List<MobSpawn> chosenSpawns = new List<MobSpawn>();
            chosenSpawns.AddRange(Spawns);
            return chosenSpawns;
        }
    }
}
