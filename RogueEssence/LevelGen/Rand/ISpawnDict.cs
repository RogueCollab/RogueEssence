// <copyright file="ISpawnDict.cs" company="Audino">
// Copyright (c) Audino
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Collections;
using System.Collections.Generic;

namespace RogueElements
{
    public interface ISpawnDict<TK, TV>
    {
        int Count { get; }

        int SpawnTotal { get; }

        void Add(TK key, TV spawn, int rate);

        void Clear();

        IEnumerable<TK> GetKeys();

        TV GetSpawn(TK key);

        int GetSpawnRate(TK key);

        void SetSpawn(TK key, TV spawn);

        void SetSpawnRate(TK key, int rate);

        void Remove(TK key);

        bool ContainsKey(TK key);
    }

    public interface ISpawnDict
    {
        int Count { get; }

        int SpawnTotal { get; }

        void Add(object key, object spawn, int rate);

        void Clear();

        IEnumerable GetKeys();

        object GetSpawn(object key);

        int GetSpawnRate(object key);

        void SetSpawn(object key, object spawn);

        void SetSpawnRate(object key, int rate);

        void Remove(object key);

        bool Contains(object key);
    }
}
