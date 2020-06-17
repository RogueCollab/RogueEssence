using System;
using RogueElements;

namespace RogueEssence.LevelGen
{
    [Serializable]
    public class GenPriority<T> : IGenPriority where T : IGenStep
    {
        public int Priority { get; set; }
        public T Item;

        public GenPriority() { }
        public GenPriority(T effect)
        {
            Item = effect;
        }
        public GenPriority(int priority, T effect)
        {
            Priority = priority;
            Item = effect;
        }

        public IGenStep GetItem() { return Item; }
    }

    public interface IGenPriority
    {
        int Priority { get; set; }
        IGenStep GetItem();
    }
}
