using System;
using System.Collections.Generic;
using RogueElements;

namespace RogueEssence.LevelGen
{
    [Serializable]
    public class GridCombo<T> where T : class, IFloorPlanGenContext
    {
        /// <summary>
        /// Size of the merge in cells
        /// </summary>
        public Loc Size;

        /// <summary>
        /// The roomgen to use for the merged room.
        /// </summary>
        public RoomGen<T> GiantRoom;

        public GridCombo()
        {

        }
        public GridCombo(Loc size, RoomGen<T> giantRoom)
        {
            Size = size;
            GiantRoom = giantRoom;
        }
    }

    /// <summary>
    /// Merges adjacent single-cell rooms together into larger rooms, specified in Combos.
    /// This is done by choosing randomly from the entire grid and checking to see if it works as a merge point.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class CombineGridRoomRandStep<T> : CombineGridRoomBaseStep<T> where T : class, IRoomGridGenContext
    {
        public CombineGridRoomRandStep()
        {
        }

        public CombineGridRoomRandStep(RandRange mergeRate, List<BaseRoomFilter> filters) : base(mergeRate, filters)
        {
        }

        protected override Loc? chooseViableGridLoc(IRandom rand, GridPlan floorPlan, GridCombo<T> combo)
        {
            Rect allowedRange = Rect.FromPoints(Loc.Zero, new Loc(floorPlan.GridWidth, floorPlan.GridHeight) - combo.Size + new Loc(1));
            if (floorPlan.Wrap)
                allowedRange = Rect.FromPoints(Loc.Zero, new Loc(floorPlan.GridWidth, floorPlan.GridHeight));

            //attempt to place it
            for (int ii = 0; ii < 10; ii++)
            {
                int xx = rand.Next(allowedRange.X, allowedRange.End.X);
                int yy = rand.Next(allowedRange.Y, allowedRange.End.Y);

                bool viable = true;
                //check for room presence in all rooms (must be SINGLE and immutable)

                for (int x2 = xx; x2 < xx + combo.Size.X; x2++)
                {
                    for (int y2 = yy; y2 < yy + combo.Size.Y; y2++)
                    {
                        if (!roomViable(floorPlan, x2, y2))
                        {
                            viable = false;
                            break;
                        }
                    }
                    if (!viable)
                        break;
                }
                if (!viable)
                    continue;

                return new Loc(xx, yy);
            }

            return null;
        }
    }

    /// <summary>
    /// Merges adjacent single-cell rooms together into larger rooms, specified in Combos.
    /// This is done by getting all possible merge points and then choosing randomly from them.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class CombineGridRoomStep<T> : CombineGridRoomBaseStep<T> where T : class, IRoomGridGenContext
    {
        public CombineGridRoomStep()
        {
        }

        public CombineGridRoomStep(RandRange mergeRate, List<BaseRoomFilter> filters) : base(mergeRate, filters)
        {
        }

        protected override Loc? chooseViableGridLoc(IRandom rand, GridPlan floorPlan, GridCombo<T> combo)
        {
            List<Loc> viableLocs = new List<Loc>();

            Rect allowedRange = Rect.FromPoints(Loc.Zero, new Loc(floorPlan.GridWidth, floorPlan.GridHeight) - combo.Size + new Loc(1));
            if (floorPlan.Wrap)
                allowedRange = Rect.FromPoints(Loc.Zero, new Loc(floorPlan.GridWidth, floorPlan.GridHeight));

            //attempt to place it
            for (int xx = allowedRange.X; xx < allowedRange.End.X; xx++)
            {
                for (int yy = allowedRange.Y; yy < allowedRange.End.Y; yy++)
                {
                    bool viable = true;
                    //check for room presence in all rooms (must be SINGLE and immutable)

                    for (int x2 = xx; x2 < xx + combo.Size.X; x2++)
                    {
                        for (int y2 = yy; y2 < yy + combo.Size.Y; y2++)
                        {
                            if (!roomViable(floorPlan, x2, y2))
                            {
                                viable = false;
                                break;
                            }
                        }
                        if (!viable)
                            break;
                    }
                    if (!viable)
                        continue;


                    //TODO: check for connectivity: all constituent rooms must be connected to each other somehow
                    //Check for connectivity within the whole map.

                    viableLocs.Add(new Loc(xx, yy));
                }
            }

            if (viableLocs.Count == 0)
                return null;

            return viableLocs[rand.Next(viableLocs.Count)];
        }
    }

    /// <summary>
    /// Merges adjacent single-cell rooms together into larger rooms, specified in Combos.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public abstract class CombineGridRoomBaseStep<T> : GridPlanStep<T> where T : class, IRoomGridGenContext, ICombinedGridRoomStep
    {
        /// <summary>
        /// The number of merges to add to the grid plan.
        /// </summary>
        public RandRange MergeRate;

        /// <summary>
        /// List of possible merges that can be done.
        /// Maps a certain area of specifified size and merges it using a specified room gen.
        /// Only merges of the specified sizes will be made.
        /// </summary>
        public SpawnList<GridCombo<T>> Combos;

        /// <summary>
        /// Determines which rooms are eligible to be merged into a new room.
        /// </summary>
        public List<BaseRoomFilter> Filters { get; set; }

        /// <summary>
        /// Components that the newly added room will be labeled with.
        /// </summary>
        public ComponentCollection RoomComponents { get; set; }

        public CombineGridRoomBaseStep()
        {
            Combos = new SpawnList<GridCombo<T>>();
            RoomComponents = new ComponentCollection();
            Filters = new List<BaseRoomFilter>();
        }

        public CombineGridRoomBaseStep(RandRange mergeRate, List<BaseRoomFilter> filters)
        {
            MergeRate = mergeRate;
            Combos = new SpawnList<GridCombo<T>>();
            RoomComponents = new ComponentCollection();
            Filters = filters;
        }

        protected abstract Loc? chooseViableGridLoc(IRandom rand, GridPlan floorPlan, GridCombo<T> combo);

        public override void ApplyToPath(IRandom rand, GridPlan floorPlan)
        {
            int merges = MergeRate.Pick(rand);
            for (int ii = 0; ii < merges; ii++)
            {
                //roll a merge
                GridCombo<T> combo = Combos.Pick(rand);

                Loc? chosenLoc = chooseViableGridLoc(rand, floorPlan, combo);

                if (!chosenLoc.HasValue)
                    continue;

                Loc destLoc = chosenLoc.Value;

                //erase the constituent rooms
                for (int x2 = destLoc.X; x2 < destLoc.X + combo.Size.X; x2++)
                {
                    for (int y2 = destLoc.Y; y2 < destLoc.Y + combo.Size.Y; y2++)
                    {
                        floorPlan.EraseRoom(new Loc(x2, y2));
                        if (x2 > destLoc.X)
                            floorPlan.SetHall(new LocRay4(x2, y2, Dir4.Left), null, new ComponentCollection());
                        if (y2 > destLoc.Y)
                            floorPlan.SetHall(new LocRay4(x2, y2, Dir4.Up), null, new ComponentCollection());
                    }
                }

                //place the room
                floorPlan.AddRoom(new Rect(destLoc.X, destLoc.Y, combo.Size.X, combo.Size.Y), combo.GiantRoom.Copy(), this.RoomComponents.Clone(), false);
            }
        }

        protected bool roomViable(GridPlan floorPlan, int xx, int yy)
        {
            //must be PRESENT, SINGLE and immutable
            GridRoomPlan plan = floorPlan.GetRoomPlan(new Loc(xx, yy));
            if (plan == null)
                return false;
            if (plan.Bounds.Area > 1)
                return false;
            if (!BaseRoomFilter.PassesAllFilters(plan, this.Filters))
                return false;

            return true;
        }


        public override string ToString()
        {
            return string.Format("{0}[{1}]: Amount:{2}", this.GetType().GetFormattedTypeName(), Combos.Count, MergeRate.ToString());
        }
    }
}
