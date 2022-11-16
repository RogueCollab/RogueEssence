using System;
using System.Collections.Generic;
using RogueElements;
using RogueEssence.Dungeon;

namespace RogueEssence.LevelGen
{
    /// <summary>
    /// Picks tiles that can be reached by walking from the entrance and spawns the mobs there, within a radius.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class PlaceRadiusMobsStep<T> : PlaceMobsStep<T>
        where T : StairsMapGenContext, ITiledGenContext
    {

        public int Radius;

        public PlaceRadiusMobsStep()
        {

        }

        public PlaceRadiusMobsStep(IMultiTeamSpawner<T> spawn) : base(spawn)
        {

        }

        public override void Apply(T map)
        {
            List<Team> spawns = Spawn.GetSpawns(map);

            if (spawns.Count == 0)
                return;

            Loc entrance = ((IViewPlaceableGenContext<MapGenEntrance>)map).GetLoc(0);
            Rect fillRect = Rect.FromPointRadius(entrance, Radius);
            bool[][] connectionGrid = new bool[fillRect.Width][];
            for (int xx = 0; xx < fillRect.Width; xx++)
            {
                connectionGrid[xx] = new bool[fillRect.Height];
                for (int yy = 0; yy < fillRect.Height; yy++)
                    connectionGrid[xx][yy] = false;
            }

            //first mark all tiles in the main path
            Grid.FloodFill(fillRect,
            (Loc testLoc) =>
            {
                if (connectionGrid[testLoc.X - fillRect.X][testLoc.Y - fillRect.Y])
                    return true;

                if (map.RoomTerrain.TileEquivalent(map.GetTile(testLoc)))
                    return false;

                return true;
            },
            (Loc testLoc) =>
            {
                return true;
            },
            (Loc fillLoc) =>
            {
                connectionGrid[fillLoc.X - fillRect.X][fillLoc.Y - fillRect.Y] = true;
            },
            entrance);


            //obtain all tiles not in the main path
            List<Loc> freeTiles = new List<Loc>();
            for (int xx = 0; xx < fillRect.Width; xx++)
            {
                for (int yy = 0; yy < fillRect.Height; yy++)
                {
                    if (connectionGrid[xx][yy])
                        freeTiles.Add(fillRect.Start + new Loc(xx, yy));
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
