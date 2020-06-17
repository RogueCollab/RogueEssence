using System;

namespace RogueEssence.Dungeon
{
    [Serializable]
    public abstract class ContextState : GameplayState
    {
    }

    [Serializable]
    public abstract class ContextIntState : ContextState
    {
        public int Count;
        protected ContextIntState() { }
        protected ContextIntState(int count) { Count = count; }
        protected ContextIntState(ContextIntState other) { Count = other.Count; }
    }

    [Serializable]
    public abstract class ContextMultState : ContextState
    {
        public Multiplier Mult;
        protected ContextMultState() { }
        protected ContextMultState(ContextMultState other) { Mult = other.Mult; }
    }
}
