using System;
using RogueElements;

namespace RogueEssence.Content
{
    /// <summary>
    /// Creates an animation (Or particle emitter) behind and in front of the target sprite.
    /// </summary>
    [Serializable]
    public class BetweenEmitter : FiniteEmitter
    {
        private bool finished;
        public override bool Finished { get { return finished; } }

        const int BETWEEN_DIFF = 2;

        public BetweenEmitter()
        {
            AnimBack = new EmptyFiniteEmitter();
            AnimFront = new EmptyFiniteEmitter();
        }
        public BetweenEmitter(AnimData animBack, AnimData animFront)
        {
            AnimBack = new StaticAnim(animBack);
            AnimFront = new StaticAnim(animFront);
        }

        public BetweenEmitter(BetweenEmitter other)
        {
            AnimBack = other.AnimBack.CloneIEmittable();
            AnimFront = other.AnimFront.CloneIEmittable();
            Offset = other.Offset;
            HeightBack = other.HeightBack;
            HeightFront = other.HeightFront;
        }

        public override BaseEmitter Clone() { return new BetweenEmitter(this); }

        /// <summary>
        /// The animation or particle emitter behind the target sprite.
        /// </summary>
        public IEmittable AnimBack;

        /// <summary>
        /// The animation or particle emitter in front of the target sprite.
        /// </summary>
        public IEmittable AnimFront;

        /// <summary>
        /// The height of the animation or particle emitter behind the target sprite.
        /// </summary>
        public int HeightBack;

        /// <summary>
        /// The height of the animation or particle emitter in front of the target sprite.
        /// </summary>
        public int HeightFront;

        /// <summary>
        /// The number of pixels to offset both animations forward, in the direction of the animation.
        /// </summary>
        public int Offset;


        public override void Update(BaseScene scene, FrameTick elapsedTime)
        {
            Loc center = Origin + Dir.GetLoc() * Offset;

            scene.Anims[(int)DrawLayer.Back].Add(AnimBack.CreateStatic(center, HeightBack, Dir));
            scene.Anims[(int)DrawLayer.Front].Add(AnimFront.CreateStatic(center, HeightFront, Dir));

            finished = true;
        }

    }
}