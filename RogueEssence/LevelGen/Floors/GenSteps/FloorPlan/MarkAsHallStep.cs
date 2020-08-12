using System;
using RogueElements;
//Delet this
namespace RogueEssence.LevelGen
{
    [Serializable]
    public class MarkAsHallStep<T> : GridPlanStep<T> where T : class, IRoomGridGenContext
    {

        public MarkAsHallStep()
        { }


        public override void ApplyToPath(IRandom rand, GridPlan floorPlan)
        {
            for (int ii = 0; ii < floorPlan.RoomCount; ii++)
            {
                GridRoomPlan plan = floorPlan.GetRoomPlan(ii);
                if (plan.RoomGen is IPermissiveRoomGen)
                    plan.PreferHall = true;
            }
        }


    }
}
