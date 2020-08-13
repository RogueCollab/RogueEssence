using System;
using System.Collections.Generic;
using RogueElements;

namespace RogueEssence.LevelGen
{
    [Serializable]
    public class CombineGridRoomStep<T> : GridPlanStep<T> where T : class, IRoomGridGenContext
    {
        //just combine simple squares for now
        public SpawnList<RoomGen<T>> GiantRooms;
        public ComponentCollection RoomComponents { get; set; }
        public int CombineChance;

        public List<BaseRoomFilter> Filters { get; set; }

        public CombineGridRoomStep()
        {
            GiantRooms = new SpawnList<RoomGen<T>>();
            RoomComponents = new ComponentCollection();
            Filters = new List<BaseRoomFilter>();
        }

        public CombineGridRoomStep(int combineChance, List<BaseRoomFilter> filters)
        {
            CombineChance = combineChance;
            GiantRooms = new SpawnList<RoomGen<T>>();
            RoomComponents = new ComponentCollection();
            Filters = filters;
        }


        public override void ApplyToPath(IRandom rand, GridPlan floorPlan)
        {
            for(int xx = 0; xx < floorPlan.GridWidth-1; xx++)
            {
                for (int yy = 0; yy < floorPlan.GridHeight-1; yy++)
                {
                    //check for room presence in all rooms (must be SINGLE and immutable)
                    if (!roomViable(floorPlan, xx, yy))
                        continue;
                    if (!roomViable(floorPlan, xx, yy+1))
                        continue;
                    if (!roomViable(floorPlan, xx+1, yy))
                        continue;
                    if (!roomViable(floorPlan, xx+1, yy+1))
                        continue;

                    //check for hall connectivity in all constituent halls
                    if (floorPlan.GetHall(new LocRay4(xx, yy, Dir4.Down)) == null)
                        continue;
                    if (floorPlan.GetHall(new LocRay4(xx, yy, Dir4.Right)) == null)
                        continue;
                    if (floorPlan.GetHall(new LocRay4(xx+1, yy, Dir4.Down)) == null)
                        continue;
                    if (floorPlan.GetHall(new LocRay4(xx, yy+1, Dir4.Right)) == null)
                        continue;

                    if (rand.Next(100) < CombineChance)
                    {
                        //erase the constituent rooms
                        floorPlan.EraseRoom(new Loc(xx, yy));
                        floorPlan.EraseRoom(new Loc(xx + 1, yy));
                        floorPlan.EraseRoom(new Loc(xx, yy + 1));
                        floorPlan.EraseRoom(new Loc(xx + 1, yy + 1));

                        //erase the constituent halls
                        floorPlan.SetHall(new LocRay4(xx, yy, Dir4.Down), null, new ComponentCollection());
                        floorPlan.SetHall(new LocRay4(xx, yy, Dir4.Right), null, new ComponentCollection());
                        floorPlan.SetHall(new LocRay4(xx + 1, yy, Dir4.Down), null, new ComponentCollection());
                        floorPlan.SetHall(new LocRay4(xx, yy + 1, Dir4.Right), null, new ComponentCollection());

                        //place the room
                        RoomGen<T> gen = GiantRooms.Pick(rand);
                        floorPlan.AddRoom(new Rect(xx, yy, 2, 2), gen.Copy(), this.RoomComponents.Clone(), false);
                    }
                }
            }
        }

        private bool roomViable(GridPlan floorPlan, int xx, int yy)
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


    }
}
