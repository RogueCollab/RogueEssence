// <copyright file="IGridPathBranch.cs" company="Audino">
// Copyright (c) Audino
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using RogueElements;

namespace RogueEssence.LevelGen
{
    /// <summary>
    /// Populates the empty grid plan of a map by creating a minimum spanning tree of connected rooms and halls.
    /// Prefers to add rooms where there are no neighbors.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class GridPathBranchSpread<T> : GridPathBranch<T>, IGridPathBranch
        where T : class, IRoomGridGenContext
    {
        public GridPathBranchSpread()
            : base()
        {
        }

        protected override Loc PopRandomLoc(GridPlan floorPlan, IRandom rand, List<Loc> locs)
        {
            //choose the location with the lowest adjacencies out of 5
            int branchIdx = 0;
            Loc branch = locs[0];
            int rating = getRating(floorPlan, branch);

            for (int ii = 1; ii < locs.Count; ii++)
            {
                int newBranchIdx = ii;
                Loc newBranch = locs[newBranchIdx];
                int newRating = getRating(floorPlan, newBranch);
                if (newRating > rating)
                {
                    branchIdx = newBranchIdx;
                    branch = newBranch;
                    rating = newRating;

                    if (newRating >= 10)
                        break;
                }
            }
            locs.RemoveAt(branchIdx);
            return branch;
        }

        private int getRating(GridPlan floorPlan, Loc branch)
        {
            int rating = 0;
            foreach (Dir8 checkDir in DirExt.VALID_DIR8)
            {
                Loc checkLoc = branch + checkDir.GetLoc();
                //TODO: actually, count out of bounds as empty room
                if ((floorPlan.Wrap || Collision.InBounds(floorPlan.GridWidth, floorPlan.GridHeight, checkLoc))
                    && floorPlan.GetRoomIndex(checkLoc) == -1)
                {
                    if (checkDir.IsDiagonal())
                        rating += 1;
                    else
                        rating *= 2;
                }
            }

            return rating;
        }

        protected override SpawnList<LocRay4> GetExpandDirChances(GridPlan floorPlan, Loc newTerminal)
        {
            SpawnList<LocRay4> availableRays = new SpawnList<LocRay4>();
            foreach (Dir4 dir in DirExt.VALID_DIR4)
            {
                Loc endLoc = newTerminal + dir.GetLoc();
                if ((floorPlan.Wrap || Collision.InBounds(floorPlan.GridWidth, floorPlan.GridHeight, endLoc))
                    && floorPlan.GetRoomIndex(endLoc) == -1)
                {
                    //can be added, but how much free space is around this potential coordinate?
                    //become ten times more likely to be picked for every free cardinal space around this destination
                    int chance = 1;
                    foreach (Dir8 checkDir in DirExt.VALID_DIR8)
                    {
                        if (checkDir != dir.ToDir8().Reverse())
                        {
                            //TODO: actually, count out of bounds as empty room
                            Loc checkLoc = endLoc + checkDir.GetLoc();
                            if ((floorPlan.Wrap || Collision.InBounds(floorPlan.GridWidth, floorPlan.GridHeight, checkLoc))
                                && floorPlan.GetRoomIndex(checkLoc) == -1)
                            {
                                if (checkDir.IsDiagonal())
                                    chance *= 3;
                                else
                                    chance *= 10;
                            }
                        }
                    }
                    availableRays.Add(new LocRay4(newTerminal, dir), chance);
                }
            }
            return availableRays;
        }

    }
}
