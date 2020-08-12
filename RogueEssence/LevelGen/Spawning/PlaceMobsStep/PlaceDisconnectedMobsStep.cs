using System;
using System.Collections.Generic;
using RogueElements;
using RogueEssence.Dungeon;
//Delet this
namespace RogueEssence.LevelGen
{
    [Serializable]
    public class PlaceDisconnectedMobsStep<T> : PlaceMobsStep<T> where T : ListMapGenContext
    {
        public RandRange Amount;

        public PlaceDisconnectedMobsStep()
        {
        }

        public PlaceDisconnectedMobsStep(ITeamStepSpawner<T> spawn) : base(spawn) { }

        public PlaceDisconnectedMobsStep(ITeamStepSpawner<T> spawn, RandRange amount) : base(spawn)
        {
            Amount = amount;
        }

        public override void Apply(T map)
        {

            int chosenAmount = Amount.Pick(map.Rand);
            if (chosenAmount > 0)
            {
                bool[][] connectionGrid = new bool[map.Width][];
                for (int xx = 0; xx < map.Width; xx++)
                {
                    connectionGrid[xx] = new bool[map.Height];
                    for (int yy = 0; yy < map.Height; yy++)
                        connectionGrid[xx][yy] = false;
                }

                //first mark all tiles in the main path
                Grid.FloodFill(new Rect(0, 0, map.Width, map.Height),
                (Loc testLoc) =>
                {
                    return (connectionGrid[testLoc.X][testLoc.Y] || !map.GetTile(testLoc).TileEquivalent(map.RoomTerrain));
                },
                (Loc testLoc) =>
                {
                    return true;
                },
                (Loc fillLoc) =>
                {
                    connectionGrid[fillLoc.X][fillLoc.Y] = true;
                },
                ((IViewPlaceableGenContext<MapGenEntrance>)map).GetLoc(0));


                //obtain all tiles not in the main path
                List<Loc> freeTiles = new List<Loc>();

                for (int xx = 0; xx < map.Width; xx++)
                {
                    for (int yy = 0; yy < map.Height; yy++)
                    {
                        if (((IGroupPlaceableGenContext<Team>)map).CanPlaceItem(new Loc(xx, yy)) && !connectionGrid[xx][yy])
                            freeTiles.Add(new Loc(xx, yy));
                    }
                }


                int trials = 10 * chosenAmount;
                for (int ii = 0; ii < trials; ii++)
                {
                    Team newTeam = Spawn.GetSpawn(map);
                    if (newTeam == null)
                        continue;


                    if (freeTiles.Count >= newTeam.Players.Count)
                    {
                        Loc[] locs = new Loc[newTeam.Players.Count];
                        for (int jj = 0; jj < newTeam.Players.Count; jj++)
                        {
                            int randIndex = map.Rand.Next(freeTiles.Count);
                            locs[jj] = freeTiles[randIndex];
                            freeTiles.RemoveAt(randIndex);
                        }
                        ((IGroupPlaceableGenContext<Team>)map).PlaceItems(newTeam, locs);
                        chosenAmount--;
                    }

                    if (freeTiles.Count == 0 || chosenAmount == 0)
                        break;                    
                }
            }
        }

    }
}
