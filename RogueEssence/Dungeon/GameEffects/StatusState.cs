using System;

namespace RogueEssence.Dungeon
{
    [Serializable]
    public abstract class StatusState : GameplayState
    {

    }
    [Serializable]
    public class CountState : StatusState
    {
        public int Count;
        public CountState() { }
        public CountState(int count) { Count = count; }
        protected CountState(CountState other) { Count = other.Count; }
        public override GameplayState Clone() { return new CountState(this); }
    }
    [Serializable]
    public class StackState : StatusState
    {
        public int Stack;
        public StackState() { }
        public StackState(int stack) { Stack = stack; }
        protected StackState(StackState other) { Stack = other.Stack; }
        public override GameplayState Clone() { return new StackState(this); }
    }
    [Serializable]
    public class CountDownState : StatusState
    {
        public int Counter;
        public CountDownState() { }
        public CountDownState(int counter) { Counter = counter; }
        protected CountDownState(CountDownState other) { Counter = other.Counter; }
        public override GameplayState Clone() { return new CountDownState(this); }
    }
}
