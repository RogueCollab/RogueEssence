using RogueElements;
using System;
using System.Collections.Generic;

namespace RogueEssence
{

    [Serializable]
    public class BulkSpawner<TGenContext, TSpawnable> :  IStepSpawner<TGenContext, TSpawnable>
        where TGenContext : IGenContext
        where TSpawnable : ISpawnable
    {
        public List<TSpawnable> SpecificSpawns;

        public int SpawnAmount;
        public SpawnList<TSpawnable> RandomSpawns;

        public BulkSpawner()
        {
            SpecificSpawns = new List<TSpawnable>();
            RandomSpawns = new SpawnList<TSpawnable>();
        }
        protected BulkSpawner(BulkSpawner<TGenContext, TSpawnable> other) : this()
        {
            foreach (TSpawnable specificSpawn in other.SpecificSpawns)
                SpecificSpawns.Add((TSpawnable)specificSpawn.Copy());
            SpawnAmount = other.SpawnAmount;
            for (int ii = 0; ii < other.RandomSpawns.Count; ii++)
                RandomSpawns.Add((TSpawnable)other.RandomSpawns.GetSpawn(ii).Copy(), other.RandomSpawns.GetSpawnRate(ii));
        }
        public BulkSpawner<TGenContext, TSpawnable> Copy() { return new BulkSpawner<TGenContext, TSpawnable>(this); }

        public List<TSpawnable> GetSpawns(TGenContext map)
        {
            List<TSpawnable> spawns = new List<TSpawnable>();
            foreach (TSpawnable element in SpecificSpawns)
                spawns.Add(element);
            for (int ii = 0; ii < SpawnAmount; ii++)
                spawns.Add(RandomSpawns.Pick(map.Rand));
            
            return spawns;
        }

    }
}
