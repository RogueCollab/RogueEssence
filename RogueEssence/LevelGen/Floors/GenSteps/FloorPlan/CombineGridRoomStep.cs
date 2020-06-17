using System;
using RogueElements;

namespace RogueEssence.LevelGen
{
    [Serializable]
    public class CombineGridRoomStep<T> : GridPlanStep<T> where T : class, IRoomGridGenContext
    {
        //just combine simple squares for now
        public SpawnList<RoomGen<T>> GiantRooms;
        public int CombineChance;
        public bool Immutable;

        public CombineGridRoomStep()
        {
            GiantRooms = new SpawnList<RoomGen<T>>();
        }

        public CombineGridRoomStep(int combineChance, bool immutable) : this()
        {
            CombineChance = combineChance;
            Immutable = immutable;
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
                        floorPlan.SetHall(new LocRay4(xx, yy, Dir4.Down), null);
                        floorPlan.SetHall(new LocRay4(xx, yy, Dir4.Right), null);
                        floorPlan.SetHall(new LocRay4(xx + 1, yy, Dir4.Down), null);
                        floorPlan.SetHall(new LocRay4(xx, yy + 1, Dir4.Right), null);

                        //place the room
                        RoomGen<T> gen = GiantRooms.Pick(rand);
                        floorPlan.AddRoom(new Rect(xx, yy, 2, 2), gen.Copy(), Immutable, false);
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
            if (plan.Immutable)
                return false;
            return true;
        }


    }
}
