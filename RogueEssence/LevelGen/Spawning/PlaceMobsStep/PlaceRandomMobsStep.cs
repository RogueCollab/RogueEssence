using System;
using System.Collections.Generic;
using RogueElements;
using RogueEssence.Dungeon;

namespace RogueEssence.LevelGen
{
    /// <summary>
    /// Places mobs in random rooms.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class PlaceRandomMobsStep<T> : PlaceMobsStep<T> where T : ListMapGenContext
    {
        public const int AVERAGE_CLUMP_FACTOR = 100;

        /// <summary>
        /// A percentage value at or above 0.
        /// Lower numbers make enemies spread out more evenly.
        /// Higher numbers cause them to spawn in bigger groups.
        /// </summary>
        public int ClumpFactor;

        /// <summary>
        /// Used to filter out rooms unsuitable for spawning the mobs in.
        /// </summary>
        public List<BaseRoomFilter> Filters { get; set; }


        public bool IncludeHalls { get; set; }

        public PlaceRandomMobsStep()
        {
            Filters = new List<BaseRoomFilter>();
        }

        public PlaceRandomMobsStep(IMultiTeamSpawner<T> spawn) : base(spawn) { Filters = new List<BaseRoomFilter>(); }

        public PlaceRandomMobsStep(IMultiTeamSpawner<T> spawn, int clumpFactor) : base(spawn)
        {
            Filters = new List<BaseRoomFilter>();
            ClumpFactor = clumpFactor;
        }

        public override void Apply(T map)
        {
            List<Team> spawns = Spawn.GetSpawns(map);

            if (spawns.Count == 0)
                return;
            
            SpawnList<RoomHallIndex> spawningRooms = new SpawnList<RoomHallIndex>();

            //get all places that spawnings are eligible
            for (int ii = 0; ii < map.RoomPlan.RoomCount; ii++)
            {
                IRoomGen room = map.RoomPlan.GetRoom(ii);

                if (!BaseRoomFilter.PassesAllFilters(map.RoomPlan.GetRoomPlan(ii), this.Filters))
                    continue;

                spawningRooms.Add(new RoomHallIndex(ii, false), 10000);
            }

            if (IncludeHalls)
            {
                for (int ii = 0; ii < map.RoomPlan.HallCount; ii++)
                {
                    IRoomGen room = map.RoomPlan.GetHall(ii);

                    if (!BaseRoomFilter.PassesAllFilters(map.RoomPlan.GetHallPlan(ii), this.Filters))
                        continue;

                    spawningRooms.Add(new RoomHallIndex(ii, true), 10000);
                }
            }

            int trials = 10 * spawns.Count;
            for (int ii = 0; ii < trials && spawns.Count > 0; ii++)
            {
                if (spawningRooms.SpawnTotal == 0)//check to make sure there's still spawn choices left
                    break;
                int spawnIndex = spawningRooms.PickIndex(map.Rand);
                RoomHallIndex roomNum = spawningRooms.GetSpawn(spawnIndex);
                int teamIndex = map.Rand.Next(spawns.Count);
                Team newTeam = spawns[teamIndex];

                List<Loc> freeTiles = Grid.FindTilesInBox(map.RoomPlan.GetRoomHall(roomNum).RoomGen.Draw.Start, map.RoomPlan.GetRoomHall(roomNum).RoomGen.Draw.Size,
                    (Loc testLoc) =>
                    {
                        return ((IGroupPlaceableGenContext<TeamSpawn>)map).CanPlaceItem(testLoc);
                    });

                //this actually places the members of the team in random scattered locations, leaving them to group together via wandering
                if (freeTiles.Count >= newTeam.MemberGuestCount)
                {
                    Loc[] locs = new Loc[newTeam.MemberGuestCount];
                    for (int jj = 0; jj < locs.Length; jj++)
                    {
                        int randIndex = map.Rand.Next(freeTiles.Count);
                        locs[jj] = freeTiles[randIndex];
                        freeTiles.RemoveAt(randIndex);
                    }
                    ((IGroupPlaceableGenContext<TeamSpawn>)map).PlaceItems(new TeamSpawn(newTeam, Ally), locs);
                    spawns.RemoveAt(teamIndex);
                }

                if (freeTiles.Count == 0)//if spawningRooms is now impossible there, remove the room entirely
                    spawningRooms.RemoveAt(spawnIndex);
                else //otherwise decrease spawn rate for room
                    spawningRooms.SetSpawnRate(spawnIndex, Math.Max(spawningRooms.GetSpawnRate(spawnIndex) * ClumpFactor / 100, 1));
                    
            }
            
        }
    }
}
