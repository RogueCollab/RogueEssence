using System;
using RogueElements;

namespace RogueEssence.LevelGen
{
    [Serializable]
    public class NoRescueStep<T> : GenStep<T> where T : BaseMapGenContext
    {
        public NoRescueStep() { }
        public NoRescueStep(NoRescueStep<T> other)
        { }

        public override void Apply(T map)
        {
            map.NoRescue = true;
        }

    }
}
