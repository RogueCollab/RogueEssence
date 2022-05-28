using System;
using RogueEssence.Dungeon;
using RogueElements;


namespace RogueEssence.LevelGen
{
    /// <summary>
    /// Adds map statuses to the floor.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class MapExtraStatusStep<T> : GenStep<T> where T : BaseMapGenContext
    {
        /// <summary>
        /// The array of statuses to add to the map.
        /// </summary>
        public int[] ExtraMapStatus;

        public MapExtraStatusStep()
        {
            ExtraMapStatus = new int[1] { 0 };
        }
        public MapExtraStatusStep(params int[] defaultStatus)
        {
            ExtraMapStatus = defaultStatus;
        }

        public override void Apply(T map)
        {
            foreach (int statusIndex in ExtraMapStatus)
            {
                MapStatus status = new MapStatus(statusIndex);
                status.LoadFromData();
                map.Map.Status.Add(statusIndex, status);
            }
        }
    }
}
