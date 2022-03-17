using System;
using RogueElements;
using RogueEssence.Data;

namespace RogueEssence.LevelGen
{
    [Serializable]
    public class MappedRoomStep<T> : GenStep<T> where T : MapLoadContext
    {
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
            int id = map.ID;
            LocalText name = map.Map.Name;
            map.Map = DataManager.Instance.GetMap(MapID);
            map.InitSeed(seed);
            map.Map.ID = id;
            map.Map.Name = name;
        }

        public override string ToString()
        {
            return string.Format("{0}: Map:{1}", this.GetType().Name, this.MapID);
        }
    }
}
