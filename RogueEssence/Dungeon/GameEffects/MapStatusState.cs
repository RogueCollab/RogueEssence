using System;

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
        protected MapIndexState(MapIndexState other) { Index = other.Index; }
        public override GameplayState Clone() { return new MapIndexState(); }
    }
}
