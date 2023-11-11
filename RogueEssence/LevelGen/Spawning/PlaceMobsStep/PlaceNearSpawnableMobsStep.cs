using System;
using System.Collections.Generic;
using RogueElements;
using RogueEssence.Dungeon;

namespace RogueEssence.LevelGen
{
    /// <summary>
    /// Places mobs in the room where a placeable is.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TSpawnable"></typeparam>
    [Serializable]
    public class PlaceNearSpawnableMobsStep<T, TSpawnable> : PlaceMobsStep<T> where T : ListMapGenContext, IViewPlaceableGenContext<TSpawnable>
        where TSpawnable : ISpawnable
    {
        public PlaceNearSpawnableMobsStep()
        {

        }

        public PlaceNearSpawnableMobsStep(IMultiTeamSpawner<T> spawn) : base(spawn) { }


        public override void Apply(T map)
        {
            List<Team> spawns = Spawn.GetSpawns(map);

            if (spawns.Count == 0)
                return;

            // get the start room
            SpawnList<RoomHallIndex> spawningRooms = new SpawnList<RoomHallIndex>();
            for (int ii = 0; ii < map.RoomPlan.RoomCount; ii++)
            {
                FloorRoomPlan room = map.RoomPlan.GetRoomPlan(ii);
                for (int jj = 0; jj < map.Count; jj++)
                {
                    if (map.RoomPlan.InBounds(room.RoomGen.Draw, map.GetLoc(jj)))
                        spawningRooms.Add(new RoomHallIndex(ii, false), 100);
                }
            }

            if (spawningRooms.CanPick)
            {
                int trials = 10 * spawns.Count;
                for (int ii = 0; ii < trials && spawns.Count > 0; ii++)
                {
                    int startRoom = spawningRooms.Pick(map.Rand).Index;
                    int teamIndex = map.Rand.Next(spawns.Count);
                    Team newTeam = spawns[teamIndex];

                    List<Loc> freeTiles = Grid.FindTilesInBox(map.RoomPlan.GetRoom(startRoom).Draw.Start, map.RoomPlan.GetRoom(startRoom).Draw.Size,
                        (Loc testLoc) =>
                        {
                            return map.CanPlaceTeam(testLoc);
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
                }
            }
        }

        public override string ToString()
        {
            return String.Format("{0}", this.GetType().GetFormattedTypeName());
        }
    }
}
