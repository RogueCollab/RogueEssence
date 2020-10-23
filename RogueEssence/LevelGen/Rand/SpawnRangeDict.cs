// <copyright file="SpawnRangeDict.cs" company="Audino">
// Copyright (c) Audino
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Collections;
using System.Collections.Generic;

namespace RogueElements
{
    /// <summary>
    /// A data structure representing spawn rates of items spread across a range of floors.
    /// </summary>
    /// <typeparam name="TV"></typeparam>
    // TODO: Binary Space Partition Tree
    [Serializable]
    public class SpawnRangeDict<TK, TV> : ISpawnRangeDict<TK, TV>, ISpawnRangeDict
    {
        private readonly Dictionary<TK, SpawnRange> spawns;

        public SpawnRangeDict()
        {
            this.spawns = new Dictionary<TK, SpawnRange>();
        }

        public int Count => this.spawns.Count;

        public void Add(TK key, TV spawn, IntRange range, int rate)
        {
            if (rate < 0)
                throw new ArgumentException("Spawn rate must be 0 or higher.");
            if (range.Length <= 0)
                throw new ArgumentException("Spawn range must be 1 or higher.");
            this.spawns.Add(key, new SpawnRange(spawn, rate, range));
        }

        public void Clear()
        {
            this.spawns.Clear();
        }

        public IEnumerator<TV> GetEnumerator()
        {
            foreach (SpawnRange spawn in this.spawns.Values)
                yield return spawn.Spawn;
        }

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();


        public IEnumerable<TK> GetKeys()
        {
            foreach (TK key in this.spawns.Keys)
                yield return key;
        }

        public SpawnDict<TK, TV> GetSpawnList(int level)
        {
            SpawnDict<TK, TV> newList = new SpawnDict<TK, TV>();
            foreach (TK key in this.spawns.Keys)
            {
                SpawnRange spawn = this.spawns[key];
                if (spawn.Range.Min <= level && level < spawn.Range.Max)
                    newList.Add(key, spawn.Spawn, spawn.Rate);
            }

            return newList;
        }

        public bool CanPick(int level)
        {
            foreach (TK key in this.spawns.Keys)
            {
                SpawnRange spawn = this.spawns[key];
                if (spawn.Range.Min <= level && level < spawn.Range.Max && spawn.Rate > 0)
                    return true;
            }

            return false;
        }

        public TV Pick(IRandom random, int level)
        {
            int spawnTotal = 0;
            List<SpawnRange> spawns = new List<SpawnRange>();
            foreach (SpawnRange spawn in this.GetLevelSpawns(level))
            {
                spawns.Add(spawn);
                spawnTotal += spawn.Rate;
            }

            if (spawnTotal > 0)
            {
                int rand = random.Next(spawnTotal);
                int total = 0;
                for (int ii = 0; ii < spawns.Count; ii++)
                {
                    total += spawns[ii].Rate;
                    if (rand < total)
                        return spawns[ii].Spawn;
                }
            }

            throw new InvalidOperationException("Cannot spawn from a spawnlist of total rate 0!");
        }

        public TV GetSpawn(TK key)
        {
            return this.spawns[key].Spawn;
        }

        public int GetSpawnRate(TK key)
        {
            return this.spawns[key].Rate;
        }

        public IntRange GetSpawnRange(TK key)
        {
            return this.spawns[key].Range;
        }

        public void SetSpawn(TK key, TV spawn)
        {
            this.spawns[key] = new SpawnRange(spawn, this.spawns[key].Rate, this.spawns[key].Range);
        }

        public void SetSpawnRate(TK key, int rate)
        {
            if (rate < 0)
                throw new ArgumentException("Spawn rate must be 0 or higher.");
            this.spawns[key] = new SpawnRange(this.spawns[key].Spawn, rate, this.spawns[key].Range);
        }

        public void SetSpawnRange(TK key, IntRange range)
        {
            this.spawns[key] = new SpawnRange(this.spawns[key].Spawn, this.spawns[key].Rate, range);
        }

        public void Remove(TK key)
        {
            this.spawns.Remove(key);
        }

        public bool ContainsKey(TK key)
        {
            return this.spawns.ContainsKey(key);
        }

        void ISpawnRangeDict.Add(object key, object spawn, IntRange range, int rate)
        {
            this.Add((TK)key, (TV)spawn, range, rate);
        }

        object ISpawnRangeDict.GetSpawn(object key)
        {
            return this.GetSpawn((TK)key);
        }

        int ISpawnRangeDict.GetSpawnRate(object key)
        {
            return this.GetSpawnRate((TK)key);
        }

        IntRange ISpawnRangeDict.GetSpawnRange(object key)
        {
            return this.GetSpawnRange((TK)key);
        }

        void ISpawnRangeDict.SetSpawn(object key, object spawn)
        {
            this.SetSpawn((TK)key, (TV)spawn);
        }

        void ISpawnRangeDict.SetSpawnRate(object key, int rate)
        {
            this.SetSpawnRate((TK)key, rate);
        }

        void ISpawnRangeDict.SetSpawnRange(object key, IntRange range)
        {
            this.SetSpawnRange((TK)key, range);
        }

        void ISpawnRangeDict.Remove(object key)
        {
            this.Remove((TK)key);
        }

        bool ISpawnRangeDict.Contains(object key)
        {
            return this.spawns.ContainsKey((TK)key);
        }

        private IEnumerable<SpawnRange> GetLevelSpawns(int level)
        {
            foreach (TK key in this.spawns.Keys)
            {
                SpawnRange spawn = this.spawns[key];
                if (spawn.Range.Min <= level && level < spawn.Range.Max)
                    yield return spawn;
            }
        }

        [Serializable]
        private struct SpawnRange
        {
            public TV Spawn;
            public int Rate;
            public IntRange Range;

            public SpawnRange(TV item, int rate, IntRange range)
            {
                this.Spawn = item;
                this.Rate = rate;
                this.Range = range;
            }
        }
    }
}
