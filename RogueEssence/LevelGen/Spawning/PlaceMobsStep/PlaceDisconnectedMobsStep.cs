using System;
using System.Collections.Generic;
using RogueElements;
using RogueEssence.Dungeon;

namespace RogueEssence.LevelGen
{
    /// <summary>
    /// Picks tiles that cannot be reached by walking from the entrance and spawns the mobs there.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class PlaceDisconnectedMobsStep<T> : PlaceMobsStep<T>
        where T : StairsMapGenContext, ITiledGenContext
    {
        /// <summary>
        /// The terrain types to spawn the mobs in.
        /// </summary>
        public List<ITile> AcceptedTiles;

        public PlaceDisconnectedMobsStep()
        {
            AcceptedTiles = new List<ITile>();
        }

        public PlaceDisconnectedMobsStep(IMultiTeamSpawner<T> spawn) : base(spawn)
        {
            AcceptedTiles = new List<ITile>();
        }

        public override void Apply(T map)
        {

            List<Team> spawns = Spawn.GetSpawns(map);

            if (spawns.Count == 0)
                return;

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
                if (connectionGrid[testLoc.X][testLoc.Y])
                    return true;

                foreach (ITile tile in AcceptedTiles)
                {
                    if (map.GetTile(testLoc).TileEquivalent(tile) || map.GetTile(testLoc).TileEquivalent(map.RoomTerrain))
                        return false;
                }
                return true;
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
                    if (!connectionGrid[xx][yy])
                    {
                        bool allowPlacement = false;
                        foreach (ITile tile in AcceptedTiles)
                        {
                            if (map.GetTile(new Loc(xx, yy)).TileEquivalent(tile))
                                allowPlacement = true;
                        }
                        if (allowPlacement)
                            freeTiles.Add(new Loc(xx, yy));
                    }
                }
            }


            int trials = 10 * spawns.Count;
            for (int ii = 0; ii < trials; ii++)
            {
                int teamIndex = map.Rand.Next(spawns.Count);
                Team newTeam = spawns[teamIndex];
                if (newTeam == null)
                    continue;

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

                if (freeTiles.Count == 0 || spawns.Count == 0)
                    break;
            }

        }

    }
}
