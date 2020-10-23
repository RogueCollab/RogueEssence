// <copyright file="ISpawnDict.cs" company="Audino">
// Copyright (c) Audino
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Collections;
using System.Collections.Generic;

namespace RogueElements
{
    public interface ISpawnDict<TK, TV> : IEnumerable<TV>, IEnumerable
    {
        int Count { get; }

        int SpawnTotal { get; }

        void Add(TK key, TV spawn, int rate);

        void Clear();

        TV GetSpawn(TK key);

        int GetSpawnRate(TK key);

        void SetSpawn(TK key, TV spawn);

        void SetSpawnRate(TK key, int rate);

        void Remove(TK key);

        public bool ContainsKey(TK key);
    }

    public interface ISpawnDict : IEnumerable
    {
        int Count { get; }

        int SpawnTotal { get; }

        void Add(object key, object spawn, int rate);

        void Clear();

        object GetSpawn(object key);

        int GetSpawnRate(object key);

        void SetSpawn(object key, object spawn);

        void SetSpawnRate(object key, int rate);

        void Remove(object key);

        public bool Contains(object key);
    }
}
