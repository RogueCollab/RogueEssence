using System;
using RogueElements;
using RogueEssence.Data;

namespace RogueEssence.LevelGen
{
    [Serializable]
    public class MappedRoomStep<T> : GenStep<T> where T : MapLoadContext
    {
        public string MapID;

        public MappedRoomStep() { }
        public MappedRoomStep(MappedRoomStep<T> other)
        {
            MapID = other.MapID;
        }

        public override void Apply(T map)
        {
            map.Map = DataManager.Instance.GetMap(MapID);
        }

    }
}
