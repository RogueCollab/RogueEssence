using System;
using System.Collections.Generic;
using RogueElements;
using RogueEssence.Dungeon;

namespace RogueEssence.LevelGen
{
    [Serializable]
    public class PlaceEntranceMobsStep<T, TEntrance> : PlaceMobsStep<T> where T : ListMapGenContext, IViewPlaceableGenContext<TEntrance>
        where TEntrance : IEntrance
    {
        public PlaceEntranceMobsStep()
        {

        }

        public PlaceEntranceMobsStep(IMultiTeamSpawner<T> spawn) : base(spawn) { }


        public override void Apply(T map)
        {
            List<Team> spawns = Spawn.GetSpawns(map);

            if (spawns.Count == 0)
                return;
            
            // get the start room
            int startRoom = 0;
            Loc startLoc = map.GetLoc(0);
            for (int ii = 0; ii < map.RoomPlan.RoomCount; ii++)
            {
                FloorRoomPlan room = map.RoomPlan.GetRoomPlan(ii);
                if (Collision.InBounds(room.RoomGen.Draw, startLoc))
                {
                    startRoom = ii;
                    break;
                }
            }

            int trials = 10 * spawns.Count;
            for (int ii = 0; ii < trials && spawns.Count > 0; ii++)
            {
                int teamIndex = map.Rand.Next(spawns.Count);
                Team newTeam = spawns[teamIndex];

                List<Loc> freeTiles = Grid.FindTilesInBox(map.RoomPlan.GetRoom(startRoom).Draw.Start, map.RoomPlan.GetRoom(startRoom).Draw.Size,
                    (Loc testLoc) =>
                    {
                        return map.BaseCanPlaceTeam(testLoc) && testLoc != startLoc;
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

        public override string ToString()
        {
            return String.Format("{0}", this.GetType().Name);
        }
    }
}
