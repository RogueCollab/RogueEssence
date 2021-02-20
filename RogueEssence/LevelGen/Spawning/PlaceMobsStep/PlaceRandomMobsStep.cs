using System;
using System.Collections.Generic;
using RogueElements;
using RogueEssence.Dungeon;

namespace RogueEssence.LevelGen
{
    [Serializable]
    public class PlaceRandomMobsStep<T> : PlaceMobsStep<T> where T : ListMapGenContext
    {
        const int MIN_DIST_FROM_START = 5;
        public const int AVERAGE_CLUMP_FACTOR = 100;

        public RandRange Amount;
        public int ClumpFactor;

        public List<BaseRoomFilter> Filters { get; set; }

        public PlaceRandomMobsStep()
        {
            Filters = new List<BaseRoomFilter>();
        }

        public PlaceRandomMobsStep(ITeamStepSpawner<T> spawn) : base(spawn) { Filters = new List<BaseRoomFilter>(); }

        public PlaceRandomMobsStep(ITeamStepSpawner<T> spawn, RandRange amount, int clumpFactor) : base(spawn)
        {
            Filters = new List<BaseRoomFilter>();
            Amount = amount;
            ClumpFactor = clumpFactor;
        }

        public override void Apply(T map)
        {

            int chosenAmount = Amount.Pick(map.Rand);
            if (chosenAmount > 0)
            {
                SpawnList<int> spawningRooms = new SpawnList<int>();

                //get all places that spawnings are eligible
                for (int ii = 0; ii < map.RoomPlan.RoomCount; ii++)
                {
                    IRoomGen room = map.RoomPlan.GetRoom(ii);

                    if (!BaseRoomFilter.PassesAllFilters(map.RoomPlan.GetRoomPlan(ii), this.Filters))
                        continue;

                    spawningRooms.Add(ii, 10000);
                }

                int trials = 10 * chosenAmount;
                for (int ii = 0; ii < trials && chosenAmount > 0; ii++)
                {
                    if (spawningRooms.SpawnTotal == 0)//check to make sure there's still spawn choices left
                        break;
                    int spawnIndex = spawningRooms.PickIndex(map.Rand);
                    int roomNum = spawningRooms.GetSpawn(spawnIndex);
                    Team newTeam = Spawn.GetSpawn(map);
                    if (newTeam == null)
                        continue;

                    List<Loc> freeTiles = Grid.FindTilesInBox(map.RoomPlan.GetRoom(roomNum).Draw.Start, map.RoomPlan.GetRoom(roomNum).Draw.Size,
                        (Loc testLoc) =>
                        {
                            return ((IGroupPlaceableGenContext<Team>)map).CanPlaceItem(testLoc);
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
                        ((IGroupPlaceableGenContext<Team>)map).PlaceItems(newTeam, locs);
                        chosenAmount--;
                    }

                    if (freeTiles.Count == 0)//if spawningRooms is now impossible there, remove the room entirely
                        spawningRooms.RemoveAt(spawnIndex);
                    else //otherwise decrease spawn rate for room
                        spawningRooms.SetSpawnRate(spawnIndex, Math.Max(spawningRooms.GetSpawnRate(spawnIndex) * ClumpFactor / 100, 1));
                    
                }
            }
        }

    }
}
