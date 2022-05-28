using System;
using System.Collections.Generic;
using RogueElements;

namespace RogueEssence.LevelGen
{
    /// <summary>
    /// Categorizes the filtered rooms in the grid plan as halls.
    /// The targeted rooms must be permissive room gens.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class MarkAsHallStep<T> : GridPlanStep<T> where T : class, IRoomGridGenContext
    {

        public MarkAsHallStep()
        {
            Filters = new List<BaseRoomFilter>();
        }

        /// <summary>
        /// Determines which rooms are eligible to be marked as halls.
        /// </summary>
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
