using RogueElements;
using System;
using System.Collections.Generic;

namespace RogueEssence.LevelGen
{
    /// <summary>
    /// Generates spawnable objects using a spawnlist and a regular list.
    /// The normal list is for choosing specific objects that are ALWAYS spawned.
    /// The spawnlist is for choosing several items randomly.
    /// </summary>
    /// <typeparam name="TGenContext"></typeparam>
    /// <typeparam name="TSpawnable"></typeparam>
    [Serializable]
    public class BulkSpawner<TGenContext, TSpawnable> : IStepSpawner<TGenContext, TSpawnable>
        where TGenContext : IGenContext
        where TSpawnable : ISpawnable
    {
        /// <summary>
        /// Objects that are always spawned.
        /// </summary>
        public List<TSpawnable> SpecificSpawns;

        /// <summary>
        /// An encounter/loot table of random spawnable objects.
        /// </summary>
        public SpawnList<TSpawnable> RandomSpawns;

        /// <summary>
        /// The number of objects to roll from Random Spawns.
        /// </summary>
        public int SpawnAmount;

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

        public override string ToString()
        {
            return string.Format("{0}: {1} + {2}x{3}", this.GetType().Name, this.SpecificSpawns.ToString(), this.SpawnAmount.ToString(), this.RandomSpawns.ToString());
        }
    }
}
