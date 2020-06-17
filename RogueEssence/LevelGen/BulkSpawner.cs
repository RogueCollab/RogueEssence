using System;
using System.Collections.Generic;

namespace RogueElements
{

    [Serializable]
    public class BulkSpawner<T> where T : ISpawnable
    {
        public List<T> SpecificSpawns;

        public int SpawnAmount;
        public SpawnList<T> RandomSpawns;

        public BulkSpawner()
        {
            SpecificSpawns = new List<T>();
            RandomSpawns = new SpawnList<T>();
        }
        protected BulkSpawner(BulkSpawner<T> other) : this()
        {
            foreach (T specificSpawn in other.SpecificSpawns)
                SpecificSpawns.Add((T)specificSpawn.Copy());
            SpawnAmount = other.SpawnAmount;
            for (int ii = 0; ii < other.RandomSpawns.Count; ii++)
                RandomSpawns.Add((T)other.RandomSpawns.GetSpawn(ii).Copy(), other.RandomSpawns.GetSpawnRate(ii));
        }
        public BulkSpawner<T> Copy() { return new BulkSpawner<T>(this); }

        public List<T> GetSpawnedList(IRandom rand)
        {
            List<T> spawns = new List<T>();
            foreach (T element in SpecificSpawns)
                spawns.Add(element);
            for (int ii = 0; ii < SpawnAmount; ii++)
                spawns.Add(RandomSpawns.Pick(rand));
            
            return spawns;
        }

    }
}
