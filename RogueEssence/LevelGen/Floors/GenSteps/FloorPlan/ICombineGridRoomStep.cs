// <copyright file="ICombinedGridRoomStep.cs" company="Audino">
// Copyright (c) Audino
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using RogueElements;

namespace RogueEssence.LevelGen
{

    public interface ICombineGridRoomStep
    {
        RandRange MergeRate { get; set; }

        ISpawnList Combos { get; }

        List<BaseRoomFilter> Filters { get; set; }

        ComponentCollection RoomComponents { get; set; }
    }
}
