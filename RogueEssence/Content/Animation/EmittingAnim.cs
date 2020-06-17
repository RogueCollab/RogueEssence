using System;

namespace RogueEssence.Content
{
    [Serializable]
    public class EmittingAnim : ParticleAnim
    {
        public EmittingAnim() { ResultAnim = new EmptyFiniteEmitter(); }
        public EmittingAnim(AnimData anim, IEmittable emittable, DrawLayer layer) : base(anim) { ResultAnim = emittable; Layer = layer; }
        public EmittingAnim(AnimData anim, IEmittable emittable, DrawLayer layer, int cycles) : base(anim, cycles) { ResultAnim = emittable; Layer = layer; }
        public EmittingAnim(AnimData anim, IEmittable emittable, DrawLayer layer, int cycles, int totalTime) : base(anim, cycles, totalTime) { ResultAnim = emittable; Layer = layer; }

        protected EmittingAnim(EmittingAnim other)
            : base(other)
        {
            ResultAnim = other.ResultAnim.CloneIEmittable();
            Layer = other.Layer;
        }
        public override IEmittable CloneIEmittable() { return new EmittingAnim(this); }

        public IEmittable ResultAnim;
        public DrawLayer Layer;

        public override void Update(BaseScene scene, FrameTick elapsedTime)
        {
            base.Update(scene, elapsedTime);

            if (Finished)
                scene.Anims[(int)Layer].Add(ResultAnim.CreateStatic(MapLoc, LocHeight, Direction));
        }
    }
}