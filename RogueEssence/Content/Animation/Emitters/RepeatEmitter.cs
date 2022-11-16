using System;
using RogueElements;

namespace RogueEssence.Content
{
    /// <summary>
    /// An emitter that releases a single animation, or particle emitter, several times.
    /// </summary>
    [Serializable]
    public class RepeatEmitter : FiniteEmitter
    {
        public override bool Finished { get { return (CurrentBursts >= Math.Max(1, Bursts)); } }

        public RepeatEmitter()
        {
            Anim = new EmptyFiniteEmitter();
            Layer = DrawLayer.Normal;
        }
        public RepeatEmitter(AnimData anim)
        {
            Anim = new StaticAnim(anim);
            Layer = DrawLayer.Normal;
        }

        public RepeatEmitter(RepeatEmitter other)
        {
            Anim = other.Anim.CloneIEmittable();
            Bursts = other.Bursts;
            BurstTime = other.BurstTime;
            Layer = other.Layer;
            Offset = other.Offset;
            LocHeight = other.LocHeight;
        }

        public override BaseEmitter Clone() { return new RepeatEmitter(this); }

        /// <summary>
        /// The animation to play.  Can also be an emitter.
        /// </summary>
        public IEmittable Anim;

        /// <summary>
        /// The number of times to release the animation.
        /// </summary>
        public int Bursts;

        /// <summary>
        /// The number of frames between emissions.
        /// </summary>
        public int BurstTime;

        /// <summary>
        /// The layer to put the animation on.
        /// </summary>
        public DrawLayer Layer;

        /// <summary>
        /// Shifts the animation in the given number of pixels, based on the direction of the origin entity.
        /// </summary>
        public int Offset;

        [NonSerialized]
        private FrameTick CurrentBurstTime;
        [NonSerialized]
        private int CurrentBursts;

        public override void Update(BaseScene scene, FrameTick elapsedTime)
        {
            CurrentBurstTime += elapsedTime;
            while (CurrentBurstTime >= Math.Max(1, BurstTime))
            {
                CurrentBurstTime -= Math.Max(1, BurstTime);

                scene.Anims[(int)Layer].Add(Anim.CreateStatic(Origin + Dir.GetLoc() * Offset, LocHeight, Dir));

                CurrentBursts++;

                if (CurrentBursts >= Math.Max(1, Bursts))
                    break;
            }

        }

    }
}
