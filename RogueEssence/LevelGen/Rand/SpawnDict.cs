// <copyright file="SpawnDict.cs" company="Audino">
// Copyright (c) Audino
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Collections;
using System.Collections.Generic;

namespace RogueElements
{
    /// <summary>
    /// Selects an item randomly from a weighted list.
    /// </summary>
    /// <typeparam name="TK"></typeparam>
    /// <typeparam name="TV"></typeparam>
    [Serializable]
    public class SpawnDict<TK, TV> : IRandPicker<TV>, ISpawnDict<TK, TV>, ISpawnDict
    {
        private readonly Dictionary<TK, SpawnRate> spawns;
        private int spawnTotal;

        public SpawnDict()
        {
            this.spawns = new Dictionary<TK, SpawnRate>();
        }

        protected SpawnDict(SpawnDict<TK, TV> other)
        {
            this.spawns = new Dictionary<TK, SpawnRate>();

            foreach (TK key in other.spawns.Keys)
                this.spawns.Add(key, new SpawnRate(other.spawns[key].Spawn, other.spawns[key].Rate));
        }

        public int Count => this.spawns.Count;

        public int SpawnTotal => this.spawnTotal;

        public bool CanPick => this.spawnTotal > 0;

        public bool ChangesState => false;

        public IRandPicker<TV> CopyState() => new SpawnDict<TK, TV>(this);

        public void Add(TK key, TV spawn, int rate)
        {
            if (rate < 0)
                throw new ArgumentException("Spawn rate must be 0 or higher.");
            this.spawns.Add(key, new SpawnRate(spawn, rate));
            this.spawnTotal += rate;
        }

        public void Clear()
        {
            this.spawns.Clear();
            this.spawnTotal = 0;
        }

        public IEnumerable<TV> EnumerateOutcomes()
        {
            foreach (SpawnRate element in this.spawns.Values)
                yield return element.Spawn;
        }

        public IEnumerable<TK> GetKeys()
        {
            foreach (TK key in this.spawns.Keys)
                yield return key;
        }


        public TV Pick(IRandom random)
        {
            TK key = this.PickKey(random);
            return this.spawns[key].Spawn;
        }

        public TK PickKey(IRandom random)
        {
            if (this.spawnTotal > 0)
            {
                int rand = random.Next(this.spawnTotal);
                int total = 0;
                foreach (TK key in this.spawns.Keys)
                {
                    total += this.spawns[key].Rate;
                    if (rand < total)
                        return key;
                }
            }

            throw new InvalidOperationException("Cannot spawn from a SpawnDict of total rate 0!");
        }

        public TV GetSpawn(TK key)
        {
            return this.spawns[key].Spawn;
        }

        public int GetSpawnRate(TK key)
        {
            return this.spawns[key].Rate;
        }

        public void SetSpawn(TK key, TV spawn)
        {
            this.spawns[key] = new SpawnRate(spawn, this.spawns[key].Rate);
        }

        public void SetSpawnRate(TK key, int rate)
        {
            if (rate < 0)
                throw new ArgumentException("Spawn rate must be 0 or higher.");
            this.spawnTotal = this.spawnTotal - this.spawns[key].Rate + rate;
            this.spawns[key] = new SpawnRate(this.spawns[key].Spawn, rate);
        }

        public void Remove(TK key)
        {
            this.spawnTotal -= this.spawns[key].Rate;
            this.spawns.Remove(key);
        }

        public bool ContainsKey(TK key)
        {
            return this.spawns.ContainsKey(key);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is SpawnDict<TK, TV> other))
                return false;
            if (this.spawns.Count != other.spawns.Count)
                return false;
            foreach (TK key in this.spawns.Keys)
            {
                if (!other.spawns.ContainsKey(key))
                    return false;
                if (!this.spawns[key].Spawn.Equals(other.spawns[key].Spawn))
                    return false;
                if (this.spawns[key].Rate != other.spawns[key].Rate)
                    return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            int code = 0;
            foreach (TK key in this.spawns.Keys)
                code ^= this.spawns[key].Spawn.GetHashCode() ^ key.GetHashCode() ^ this.spawns[key].Rate;
            return code;
        }

        void ISpawnDict.Add(object key, object spawn, int rate)
        {
            this.Add((TK)key, (TV)spawn, rate);
        }

        object ISpawnDict.GetSpawn(object key)
        {
            return this.GetSpawn((TK)key);
        }

        void ISpawnDict.SetSpawn(object key, object spawn)
        {
            this.SetSpawn((TK)key, (TV)spawn);
        }

        int ISpawnDict.GetSpawnRate(object key)
        {
            return this.GetSpawnRate((TK)key);
        }

        void ISpawnDict.SetSpawnRate(object key, int rate)
        {
            this.SetSpawnRate((TK)key, rate);
        }

        public void Remove(object key)
        {
            this.Remove((TK)key);
        }

        bool ISpawnDict.Contains(object key)
        {
            return this.ContainsKey((TK)key);
        }

        [Serializable]
        private struct SpawnRate
        {
            public TV Spawn;
            public int Rate;

            public SpawnRate(TV item, int rate)
            {
                this.Spawn = item;
                this.Rate = rate;
            }
        }
    }
}
