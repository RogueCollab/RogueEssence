using System;
using RogueElements;

namespace RogueEssence.Content
{
    /// <summary>
    /// A simple emitter that releases a single animation, or particle emitter.
    /// </summary>
    [Serializable]
    public class SingleEmitter : FiniteEmitter
    {
        private bool finished;
        public override bool Finished { get { return finished; } }

        public SingleEmitter()
        {
            Anim = new EmptyFiniteEmitter();
            Layer = DrawLayer.Normal;
        }
        public SingleEmitter(AnimData anim)
        {
            Anim = new StaticAnim(anim);
            Layer = DrawLayer.Normal;
        }
        public SingleEmitter(AnimData anim, int cycles)
        {
            Anim = new StaticAnim(anim, cycles, 0);
            Layer = DrawLayer.Normal;
        }
        public SingleEmitter(BeamAnimData anim)
        {
            Anim = new ColumnAnim(anim);
            Layer = DrawLayer.Normal;
        }
        public SingleEmitter(SingleEmitter other)
        {
            Anim = other.Anim.CloneIEmittable();
            Offset = other.Offset;
            LocHeight = other.LocHeight;
            Layer = other.Layer;
            UseDest = other.UseDest;
        }

        public override BaseEmitter Clone() { return new SingleEmitter(this); }

        /// <summary>
        /// Shifts the animation in the given number of pixels, based on the direction of the origin entity.
        /// </summary>
        public int Offset;

        /// <summary>
        /// The animation to play.  Can also be an emitter.
        /// </summary>
        public IEmittable Anim;

        /// <summary>
        /// The layer to put the animation on.
        /// </summary>
        public DrawLayer Layer;

        /// <summary>
        /// Uses the other entity as the origin point of the animation, if there is one.
        /// </summary>
        public bool UseDest;


        public override void Update(BaseScene scene, FrameTick elapsedTime)
        {
            scene.Anims[(int)Layer].Add(Anim.CreateStatic(UseDest ? Destination : Origin + Dir.GetLoc() * Offset, LocHeight, Dir));
            finished = true;
        }


        public override string ToString()
        {
            return "Single["+Anim.ToString() + "]";
        }
    }
}
