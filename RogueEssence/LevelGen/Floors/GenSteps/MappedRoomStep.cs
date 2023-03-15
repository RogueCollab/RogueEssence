using System;
using RogueElements;
using RogueEssence.Data;

namespace RogueEssence.LevelGen
{
    /// <summary>
    /// Loads a file from the Map directory to be used as the dungeon floor.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class MappedRoomStep<T> : GenStep<T> where T : MapLoadContext
    {
        /// <summary>
        /// Map file to load
        /// </summary>
        [Dev.DataFolder(0, "Map/")]
        public string MapID;

        public MappedRoomStep() { }
        public MappedRoomStep(MappedRoomStep<T> other)
        {
            MapID = other.MapID;
        }

        public override void Apply(T map)
        {
            //still use the old seed, ID, and Name
            ulong seed = map.Rand.FirstSeed;
            map.Map = DataManager.Instance.GetMap(MapID);
            map.InitSeed(seed);
        }

        public override string ToString()
        {
            return string.Format("{0}: Map:{1}", this.GetType().GetFormattedTypeName(), this.MapID);
        }
    }
}
