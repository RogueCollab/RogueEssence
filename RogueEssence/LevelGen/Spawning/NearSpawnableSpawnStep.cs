// <copyright file="NearSpawnableSpawnStep.cs" company="Audino">
// Copyright (c) Audino
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using RogueElements;

namespace RogueEssence.LevelGen
{
    /// <summary>
    /// Places items on the map near the room as an existing spawnable.
    /// </summary>
    /// <typeparam name="TGenContext">Type of the MapGenContext.</typeparam>
    /// <typeparam name="TSpawnable">Type of the item to spawn.</typeparam>
    /// <typeparam name="TPriorSpawn">Type of the spawnable to refer to.</typeparam>
    [Serializable]
    public class NearSpawnableSpawnStep<TGenContext, TSpawnable, TPriorSpawn> : RoomSpawnStep<TGenContext, TSpawnable>
        where TGenContext : class, IFloorPlanGenContext, IPlaceableGenContext<TSpawnable>, IViewPlaceableGenContext<TPriorSpawn>
        where TSpawnable : ISpawnable
        where TPriorSpawn : ISpawnable
    {
        public NearSpawnableSpawnStep()
            : base()
        {
        }

        public NearSpawnableSpawnStep(IStepSpawner<TGenContext, TSpawnable> spawn, int successPercent)
            : base(spawn)
        {
            this.SuccessPercent = successPercent;
        }

        /// <summary>
        /// The percentage chance to multiply a room's spawning chance when it successfully spawns an item.
        /// </summary>
        public int SuccessPercent { get; set; }

        public override void DistributeSpawns(TGenContext map, List<TSpawnable> spawns)
        {
            // gather up all rooms and put in a spawn list
            SpawnList<RoomHallIndex> spawningRooms = new SpawnList<RoomHallIndex>();
            for (int ii = 0; ii < map.RoomPlan.RoomCount; ii++)
            {
                FloorRoomPlan room = map.RoomPlan.GetRoomPlan(ii);
                for (int jj = 0; jj < map.Count; jj++)
                {
                    if (Collision.InBounds(room.RoomGen.Draw, map.GetLoc(jj)))
                        spawningRooms.Add(new RoomHallIndex(ii, false), 100);
                }
            }

            this.SpawnRandInCandRooms(map, spawningRooms, spawns, this.SuccessPercent);
        }

        public override string ToString()
        {
            return string.Format("{0}<{1}>: MultOnSuccess:{2}%", this.GetType().Name, typeof(TSpawnable).Name, this.SuccessPercent);
        }
    }
}
