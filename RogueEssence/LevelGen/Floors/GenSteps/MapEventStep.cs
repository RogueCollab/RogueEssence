using System;
using RogueEssence.Dungeon;
using System.Collections.Generic;
using RogueElements;


namespace RogueEssence.LevelGen
{
    [Serializable]
    public class MapEventsStep<T> : GenStep<T> where T : BaseMapGenContext
    {

        public List<SingleCharEvent> PrepareEvents;
        public List<SingleCharEvent> StartEvents;
        public List<SingleCharEvent> CheckEvents;

        public MapEventsStep()
        {
            PrepareEvents = new List<SingleCharEvent>();
            StartEvents = new List<SingleCharEvent>();
            CheckEvents = new List<SingleCharEvent>();
        }

        public override void Apply(T map)
        {
            foreach (SingleCharEvent effect in PrepareEvents)
                map.Map.PrepareEvents.Add((SingleCharEvent)effect.Clone());
            foreach (SingleCharEvent effect in StartEvents)
                map.Map.StartEvents.Add((SingleCharEvent)effect.Clone());
            foreach (SingleCharEvent effect in CheckEvents)
                map.Map.CheckEvents.Add((SingleCharEvent)effect.Clone());
        }
    }
}
