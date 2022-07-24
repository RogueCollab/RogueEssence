using System;
using RogueEssence.Dungeon;
using RogueElements;
using Newtonsoft.Json;
using RogueEssence.Dev;

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
        [JsonConverter(typeof(MapStatusArrayConverter))]
        public string[] ExtraMapStatus;

        public MapExtraStatusStep()
        { }
        public MapExtraStatusStep(params string[] defaultStatus)
        {
            ExtraMapStatus = defaultStatus;
        }

        public override void Apply(T map)
        {
            foreach (string statusIndex in ExtraMapStatus)
            {
                MapStatus status = new MapStatus(statusIndex);
                status.LoadFromData();
                map.Map.Status.Add(statusIndex, status);
            }
        }
    }
}
