// <copyright file="GridPathTiered.cs" company="Audino">
// Copyright (c) Audino
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using RogueElements;

namespace RogueEssence.LevelGen
{
    /// <summary>
    /// Populates the empty floor plan of a map by creating a path consisting of tiers of rooms, with a random value of hallways connecting each tier.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class GridPathTiered<T> : GridPathStartStepGeneric<T>
        where T : class, IRoomGridGenContext
    {
        public GridPathTiered()
            : base()
        {
        }

        /// <summary>
        /// Choose a horizontal or vertical orientation.
        /// </summary>
        public Axis4 TierAxis { get; set; }

        /// <summary>
        /// The number of halls that connects each tier.  Minimum bound at 1.
        /// </summary>
        public RandRange TierConnections { get; set; }

        public override void ApplyToPath(IRandom rand, GridPlan floorPlan)
        {
            // open rooms on both sides
            Loc gridSize = new Loc(floorPlan.GridWidth, floorPlan.GridHeight);
            int scalar = gridSize.GetScalar(this.TierAxis);
            int orth = gridSize.GetScalar(this.TierAxis.Orth());

            GenContextDebug.StepIn("Initial Tiers");

            try
            {
                for (int ii = 0; ii < scalar; ii++)
                {
                    for (int jj = 0; jj < orth; jj++)
                    {
                        // place the rooms at the edge
                        floorPlan.AddRoom(this.TierAxis.CreateLoc(ii, jj), this.GenericRooms.Pick(rand), this.RoomComponents.Clone());
                        GenContextDebug.DebugProgress("Room");
                        
                        //Don't create a connection on the last tier of rooms!
                        if (ii == scalar - 1)
                            continue;
                        
                        if (scalar > 0 || floorPlan.Wrap)
                        {
                            this.PlaceOrientedHall(this.TierAxis, ii, jj, 1, floorPlan, this.GenericHalls.Pick(rand));
                            GenContextDebug.DebugProgress("Side Connection");
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                GenContextDebug.DebugError(ex);
            }

            GenContextDebug.StepOut();

            GenContextDebug.StepIn("Connecting Tiers");

            try
            {
                // paint hallways
                for (int jj = 0; jj < orth; jj++)
                {
                    if (jj > 0 || floorPlan.Wrap)
                    {
                        // choose a random number of bridges
                        int amt = Math.Max(1, this.TierConnections.Pick(rand));
                        List<int> possibleScalars = new List<int>();
                        for (int ii = 0; ii < scalar; ii++)
                            possibleScalars.Add(ii);

                        List<int> chosenScalars = new List<int>();
                        for (int nn = 0; nn < amt; nn++)
                        {
                            int idx = rand.Next(possibleScalars.Count);
                            chosenScalars.Add(possibleScalars[idx]);
                            possibleScalars.RemoveAt(idx);
                        }

                        foreach (int chosenScalar in chosenScalars)
                        {
                            this.PlaceOrientedHall(this.TierAxis.Orth(), chosenScalar, jj, -1, floorPlan, this.GenericHalls.Pick(rand));
                            GenContextDebug.DebugProgress("Bridge");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                GenContextDebug.DebugError(ex);
            }

            GenContextDebug.StepOut();
        }

        public void PlaceOrientedHall(Axis4 axis, int scalar, int orth, int scalarDiff, GridPlan floorPlan, PermissiveRoomGen<T> hallGen)
        {
            Loc loc = this.TierAxis.CreateLoc(scalar, orth);
            floorPlan.SetHall(new LocRay4(loc, axis.GetDir(scalarDiff)), hallGen, this.HallComponents.Clone());
        }

        public override string ToString()
        {
            return string.Format("{0}: Axis:{1}", this.GetType().GetFormattedTypeName(), this.TierAxis);
        }
    }
}
