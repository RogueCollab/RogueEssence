using System;
using RogueEssence.Dungeon;
using RogueElements;


namespace RogueEssence.LevelGen
{
    [Serializable]
    public class MapExtraStatusStep<T> : GenStep<T> where T : BaseMapGenContext
    {
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
