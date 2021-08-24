using NLua;
using System;
using System.Collections.Generic;

namespace RogueEssence.Dungeon
{
    [Serializable]
    public abstract class MapStatusState : GameplayState
    {

    }

    [Serializable]
    public class MapCountDownState : MapStatusState
    {
        public int Counter;
        public MapCountDownState() { }
        public MapCountDownState(int counter) { Counter = counter; }
        protected MapCountDownState(MapCountDownState other) { Counter = other.Counter; }
        public override GameplayState Clone() { return new MapCountDownState(this); }
    }
    [Serializable]
    public class MapWeatherState : MapStatusState
    {
        public MapWeatherState() { }
        public override GameplayState Clone() { return new MapWeatherState(); }
    }
    [Serializable]
    public class MapIndexState : MapStatusState
    {
        public int Index;
        public MapIndexState() { }
        public MapIndexState(int index) { Index = index; }
        protected MapIndexState(MapIndexState other) { Index = other.Index; }
        public override GameplayState Clone() { return new MapIndexState(this); }
    }
    [Serializable]
    public class MapCheckState : MapStatusState
    {
        public List<SingleCharEvent> CheckEvents;
        public MapCheckState() { CheckEvents = new List<SingleCharEvent>(); }
        protected MapCheckState(MapCheckState other)
        {
            CheckEvents = new List<SingleCharEvent>();
            foreach (SingleCharEvent effect in other.CheckEvents)
                CheckEvents.Add((SingleCharEvent)effect.Clone());
        }
        public override GameplayState Clone() { return new MapCheckState(this); }
    }
}
