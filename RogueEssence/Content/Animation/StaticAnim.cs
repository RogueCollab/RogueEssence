using System;
using RogueElements;

namespace RogueEssence.Content
{
    [Serializable]
    public class StaticAnim : LoopingAnim, IEmittable
    {
        public StaticAnim() { }
        public StaticAnim(AnimData anim) : this(anim, 1, 0) { }
        public StaticAnim(AnimData anim, int cycles) : this(anim, cycles, 0) { }
        public StaticAnim(AnimData anim, int cycles, int totalTime)
            : base(anim, totalTime, cycles) { }


        protected StaticAnim(StaticAnim other) : base(other) { }
        public virtual IEmittable CloneIEmittable() { return new StaticAnim(this); }

        public IEmittable CreateStatic(Loc startLoc, int startHeight, Dir8 dir)
        {
            StaticAnim anim = (StaticAnim)CloneIEmittable();
            anim.SetupEmitted(startLoc, startHeight, dir);
            return anim;
        }

        public virtual void SetupEmitted(Loc startLoc, int startHeight, Dir8 dir)
        {
            mapLoc = startLoc;
            locHeight = startHeight;
            Direction = dir;
            SetUp();
        }
    }
}
