using System;
using System.Collections.Generic;
using RogueElements;

namespace RogueEssence.LevelGen
{
    /// <summary>
    /// Adds large rooms to the grid plan.
    /// This is done by choosing an area that contains at least one eligible room, and no ineligible rooms.
    /// The step then bulldozes the area and places the new room in it.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class AddLargeRoomStep<T> : GridPlanStep<T> where T : class, IRoomGridGenContext
    {
        /// <summary>
        /// The amount of rooms to place.
        /// </summary>
        public RandRange RoomAmount;

        /// <summary>
        /// The types of rooms to place.
        /// </summary>
        public SpawnList<LargeRoom<T>> GiantRooms;

        /// <summary>
        /// Determines which rooms are eligible to be replaced with the new room.
        /// </summary>
        public List<BaseRoomFilter> Filters { get; set; }

        /// <summary>
        /// Components that the newly added room will be labeled with.
        /// </summary>
        public ComponentCollection RoomComponents { get; set; }

        public AddLargeRoomStep()
            : base()
        {
            GiantRooms = new SpawnList<LargeRoom<T>>();
            RoomComponents = new ComponentCollection();
            Filters = new List<BaseRoomFilter>();
        }
        public AddLargeRoomStep(RandRange roomAmount, List<BaseRoomFilter> filters)
            : this()
        {
            RoomAmount = roomAmount;
            RoomComponents = new ComponentCollection();
            Filters = filters;
        }


        public override void ApplyToPath(IRandom rand, GridPlan floorPlan)
        {
            int chosenBigRooms = RoomAmount.Pick(rand);
            for (int ii = 0; ii < chosenBigRooms; ii++)
            {
                for (int jj = 0; jj < 20; jj++)
                {
                    LargeRoom<T> chosenRoom = GiantRooms.Pick(rand);
                    Rect destRect = new Rect(new Loc(rand.Next(floorPlan.GridWidth - chosenRoom.Size.X), rand.Next(floorPlan.GridHeight - chosenRoom.Size.Y)), chosenRoom.Size);
                    if (spaceViable(floorPlan, destRect))
                    {
                        List<LocRay4> raysOut = new List<LocRay4>();
                        for (int xx = destRect.Start.X; xx < destRect.End.X; xx++)
                        {
                            LocRay4 locUp = new LocRay4(xx, destRect.Start.Y, Dir4.Up);
                            if (destRect.Start.Y > 0 && floorPlan.GetHall(locUp) != null)
                                raysOut.Add(locUp);
                            LocRay4 locDown = new LocRay4(xx, destRect.Start.Y + chosenRoom.Size.Y - 1, Dir4.Down);
                            if (destRect.Start.Y < floorPlan.GridHeight - 1 && floorPlan.GetHall(locDown) != null)
                                raysOut.Add(locDown);
                        }
                        for (int yy = destRect.Start.Y; yy < destRect.End.Y; yy++)
                        {
                            LocRay4 locLeft = new LocRay4(destRect.Start.X, yy, Dir4.Left);
                            if (destRect.Start.X > 0 && floorPlan.GetHall(locLeft) != null)
                                raysOut.Add(locLeft);
                            LocRay4 locRight = new LocRay4(destRect.Start.X + chosenRoom.Size.X - 1, yy, Dir4.Right);
                            if (destRect.Start.X < floorPlan.GridWidth - 1 && floorPlan.GetHall(locRight) != null)
                                raysOut.Add(locRight);
                        }

                        List<List<LocRay4>> exitSets = findHallSets(floorPlan, destRect, raysOut);

                        //exits: no more than allowed
                        if (exitSets.Count > chosenRoom.AllowedEntrances)
                            continue;

                        //this block tallies all sets that can be joined to the big room
                        //it also chooses which ones to keep by randomly removing them from the raysOut list
                        int setsTaken = 0;
                        foreach (List<LocRay4> set in exitSets)
                        {
                            List<LocRay4> possibleRays = new List<LocRay4>();
                            foreach(LocRay4 ray in set)
                            {
                                int scalar = (ray.Loc - destRect.Start).GetScalar(ray.Dir.ToAxis().Orth());
                                if (chosenRoom.OpenBorders[(int)ray.Dir][scalar])
                                    possibleRays.Add(ray);
                            }
                            if (possibleRays.Count > 0)
                            {
                                LocRay4 locRay = possibleRays[rand.Next(possibleRays.Count)];
                                raysOut.Remove(locRay);
                                setsTaken++;
                            }
                            else
                                break;
                        }

                        if (setsTaken == exitSets.Count)
                        {
                            for (int xx = 0; xx < chosenRoom.Size.X; xx++)
                            {
                                for (int yy = 0; yy < chosenRoom.Size.Y; yy++)
                                {
                                    //erase rooms in vicinity
                                    Loc loc = new Loc(xx + destRect.Start.X, yy + destRect.Start.Y);
                                    //erase halls in vicinity
                                    floorPlan.EraseRoom(loc);
                                    if (xx > 0)
                                        floorPlan.SetHall(new LocRay4(loc, Dir4.Left), null, new ComponentCollection());
                                    if (yy > 0)
                                        floorPlan.SetHall(new LocRay4(loc, Dir4.Up), null, new ComponentCollection());

                                }
                            }

                            //remove all halls still in the list
                            foreach (LocRay4 rayOut in raysOut)
                                floorPlan.SetHall(rayOut, null, new ComponentCollection());


                            //add room
                            floorPlan.AddRoom(destRect, chosenRoom.Gen, new ComponentCollection());

                            break;
                        }
                    }
                }
            }


        }

        private bool spaceViable(GridPlan floorPlan, Rect rect)
        {
            //all tiles must be ABSENT, or SINGLE AND not immutable
            for(int xx = rect.Start.X; xx < rect.End.X; xx++)
            {
                for (int yy = rect.Start.Y; yy < rect.End.Y; yy++)
                {
                    GridRoomPlan plan = floorPlan.GetRoomPlan(new Loc(xx, yy));
                    if (plan == null)
                        continue;
                    if (plan.Bounds.Area > 1)
                        return false;
                    if (!BaseRoomFilter.PassesAllFilters(plan, this.Filters))
                        return false;
                }
            }
            return true;
        }

        private List<List<LocRay4>> findHallSets(GridPlan floorPlan, Rect rect, List<LocRay4> raysOut)
        {
            bool[] raysCovered = new bool[raysOut.Count];

            List<List<LocRay4>> resultList = new List<List<LocRay4>>();

            Graph.GetAdjacents<int> getAdj = (int nodeIndex) =>
            {
                List<int> returnList = new List<int>();

                Loc loc = new Loc(nodeIndex % floorPlan.GridWidth, nodeIndex / floorPlan.GridWidth);
                int roomIndex = floorPlan.GetRoomIndex(loc);
                for (int dd = 0; dd < DirExt.DIR4_COUNT; dd++)
                {
                    Dir4 dir = (Dir4)dd;
                    Loc destLoc = loc + dir.GetLoc();
                    //check against outside floor bound
                    if (!Collision.InBounds(floorPlan.GridWidth, floorPlan.GridHeight, destLoc))
                        continue;
                    //check against inside rect bound
                    if (Collision.InBounds(rect, destLoc))
                        continue;
                    //check against a valid room
                    int destRoom = floorPlan.GetRoomIndex(destLoc);
                    if (destRoom == roomIndex)
                        returnList.Add(destLoc.Y * floorPlan.GridWidth + destLoc.X);
                    else if (destRoom > -1)
                    {
                        if (floorPlan.GetHall(new LocRay4(loc, dir)) != null)
                            returnList.Add(destLoc.Y * floorPlan.GridWidth + destLoc.X);
                    }
                }

                return returnList;
            };

            //group the exits together
            for (int ii = 0; ii < raysOut.Count; ii++)
            {
                if (raysCovered[ii])
                    continue;

                Loc startLoc = raysOut[ii].Traverse(1);
                List<LocRay4> set = new List<LocRay4>();

                Graph.TraverseBreadthFirst(startLoc.Y * floorPlan.GridWidth + startLoc.X,
                    (int nodeIndex, int distance) =>
                    {
                        Loc fillLoc = new Loc(nodeIndex % floorPlan.GridWidth, nodeIndex / floorPlan.GridWidth);
                        for (int nn = 0; nn < raysOut.Count; nn++)
                        {
                            if (raysOut[nn].Traverse(1) == fillLoc)
                            {
                                set.Add(raysOut[nn]);
                                raysCovered[nn] = true;
                            }
                        }
                    },
                    getAdj);

                resultList.Add(set);
            }

            return resultList;
        }

        public override string ToString()
        {
            return string.Format("{0}[{1}]: Amount:{2}", this.GetType().Name, GiantRooms.Count, RoomAmount.ToString());
        }
    }

    [Serializable]
    public class LargeRoom<T> where T : class, IRoomGridGenContext
    {
        public RoomGen<T> Gen;
        public Loc Size;
        public bool[][] OpenBorders;
        public int AllowedEntrances;

        public LargeRoom(RoomGen<T> gen, Loc size, int allowedEntrances)
        {
            Gen = gen;
            Size = size;
            OpenBorders = new bool[4][];
            OpenBorders[(int)Dir4.Down] = new bool[size.X];
            OpenBorders[(int)Dir4.Up] = new bool[size.X];
            OpenBorders[(int)Dir4.Left] = new bool[size.Y];
            OpenBorders[(int)Dir4.Right] = new bool[size.Y];
            AllowedEntrances = allowedEntrances;
        }
    }
}
