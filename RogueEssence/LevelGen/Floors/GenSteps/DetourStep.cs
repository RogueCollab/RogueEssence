using System;
using System.Collections.Generic;
using RogueElements;
using RogueEssence.Dungeon;
//Delet this but need to make a new class for sealing marked rooms off
namespace RogueEssence.LevelGen
{
    [Serializable]
    public class DetourStep<T> : GenStep<T>
        where T : class, IFloorPlanGenContext, IPlaceableGenContext<EffectTile>, IGroupPlaceableGenContext<Team>, IMobSpawnMap
    {
        public SpawnList<EffectTile> Spawns;
        public SpawnList<TeamSpawner> GuardSpawns;


        public DetourStep()
        {
            Spawns = new SpawnList<EffectTile>();
            GuardSpawns = new SpawnList<TeamSpawner>();
        }
                

        public override void Apply(T map)
        {
            if (Spawns.Count > 0)
            {
                List<Loc> freeTiles = new List<Loc>();
                //get all places that traps are eligible
                for (int ii = 0; ii < map.RoomPlan.RoomCount; ii++)
                {
                    IRoomGen room = map.RoomPlan.GetRoom(ii);

                    List<Loc> tiles = ((IPlaceableGenContext<EffectTile>)map).GetFreeTiles(room.Draw);

                    freeTiles.AddRange(tiles);
                }

                // add tile
                if (freeTiles.Count > 0)
                {
                    int randIndex = ((IGenContext)map).Rand.Next(freeTiles.Count);
                    Loc loc = freeTiles[randIndex];
                    EffectTile spawnedTrap = Spawns.Pick(((IGenContext)map).Rand);
                    map.PlaceItem(loc, spawnedTrap);
                    freeTiles.RemoveAt(randIndex);

                    if (GuardSpawns.Count > 0)
                    {
                        for (int ii = 0; ii < 10; ii++)
                        {
                            Team newTeam = GuardSpawns.Pick(((IGenContext)map).Rand).Spawn(map);
                            if (newTeam == null)
                                continue;
                            //spawn guards

                            Grid.LocTest checkSpawnOpen = (Loc testLoc) =>
                            {
                                return ((IGroupPlaceableGenContext<Team>)map).CanPlaceItem(testLoc);
                            };
                            Grid.LocTest checkSpawnBlock = (Loc testLoc) =>
                            {
                                return map.TileBlocked(testLoc);
                            };
                            Grid.LocTest checkDiagSpawnBlock = (Loc testLoc) =>
                            {
                                return map.TileBlocked(testLoc, true);
                            };

                            List<Loc> resultLocs = new List<Loc>();
                            foreach (Loc resultLoc in Grid.FindClosestConnectedTiles(new Loc(), new Loc(map.Width, map.Height),
                                checkSpawnOpen, checkSpawnBlock, checkDiagSpawnBlock, loc, newTeam.Players.Count))
                            {
                                resultLocs.Add(resultLoc);
                            }

                            if (resultLocs.Count >= newTeam.Players.Count)
                            {
                                Loc[] locs = new Loc[newTeam.Players.Count];
                                for (int jj = 0; jj < newTeam.Players.Count; jj++)
                                    locs[jj] = resultLocs[jj];
                                map.PlaceItems(newTeam, locs);
                                break;
                            }
                        }
                    }
                }
            }
        }

    }
}
