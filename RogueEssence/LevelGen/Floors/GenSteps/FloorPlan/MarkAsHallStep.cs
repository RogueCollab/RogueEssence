using System;
using System.Collections.Generic;
using RogueElements;

namespace RogueEssence.LevelGen
{
    [Serializable]
    public class MarkAsHallStep<T> : GridPlanStep<T> where T : class, IRoomGridGenContext
    {

        public MarkAsHallStep()
        {
            Filters = new List<BaseRoomFilter>();
        }

        public List<BaseRoomFilter> Filters { get; set; }

        public override void ApplyToPath(IRandom rand, GridPlan floorPlan)
        {
            for (int ii = 0; ii < floorPlan.RoomCount; ii++)
            {
                GridRoomPlan plan = floorPlan.GetRoomPlan(ii);
                if (!BaseRoomFilter.PassesAllFilters(plan, this.Filters))
                    continue;
                if (plan.RoomGen is IPermissiveRoomGen)
                    plan.PreferHall = true;
            }
        }


    }
}
