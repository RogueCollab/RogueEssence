// <copyright file="ISpawnRangeDict.cs" company="Audino">
// Copyright (c) Audino
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Collections;
using System.Collections.Generic;

namespace RogueElements
{
    public interface ISpawnRangeDict<TK, TV> : IEnumerable<TV>, IEnumerable
    {
        int Count { get; }

        void Add(TK key, TV spawn, IntRange range, int rate);

        void Clear();

        TV GetSpawn(TK key);

        IntRange GetSpawnRange(TK key);

        int GetSpawnRate(TK key);

        void SetSpawn(TK key, TV spawn);

        void SetSpawnRange(TK key, IntRange range);

        void SetSpawnRate(TK key, int rate);

        void Remove(TK key);

        bool ContainsKey(TK key);
    }

    public interface ISpawnRangeDict : IEnumerable
    {
        int Count { get; }

        void Add(object key, object spawn, IntRange range, int rate);

        void Clear();

        object GetSpawn(object key);

        IntRange GetSpawnRange(object key);

        int GetSpawnRate(object key);

        void SetSpawn(object key, object spawn);

        void SetSpawnRange(object key, IntRange range);

        void SetSpawnRate(object key, int rate);

        void Remove(object key);

        bool Contains(object key);
    }
}
