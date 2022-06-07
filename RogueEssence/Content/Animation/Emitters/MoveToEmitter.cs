using System;
using RogueElements;
using System.Runtime.Serialization;

namespace RogueEssence.Content
{
    /// <summary>
    /// Creates an animation that starts at one location and moves to another.
    /// </summary>
    [Serializable]
    public class MoveToEmitter : FiniteEmitter
    {
        private bool finished;
        public override bool Finished { get { return finished; } }

        public MoveToEmitter()
        {
            Anim = new AnimData();
            ResultAnim = new EmptyFiniteEmitter();
            ResultLayer = DrawLayer.Normal;
        }
        public MoveToEmitter(MoveToEmitter other)
        {
            Anim = new AnimData(other.Anim);
            OffsetStart = other.OffsetStart;
            HeightStart = other.HeightStart;
            OffsetEnd = other.OffsetEnd;
            HeightEnd = other.HeightEnd;
            LingerStart = other.LingerStart;
            MoveTime = other.MoveTime;
            LingerEnd = other.LingerEnd;
            ResultAnim = other.ResultAnim.CloneIEmittable();
            ResultLayer = other.ResultLayer;
        }

        public override BaseEmitter Clone() { return new MoveToEmitter(this); }
        
        /// <summary>
        /// The animation to create.
        /// </summary>
        public AnimData Anim;

        /// <summary>
        /// The number of pixels to offset both the animation forward, in the direction of the animation.
        /// This will be for the starting position.
        /// </summary>
        public Loc OffsetStart;

        /// <summary>
        /// The number of pixels to offset both the animation forward, in the direction of the animation.
        /// This will be for the ending position.
        /// </summary>
        public Loc OffsetEnd;

        /// <summary>
        /// The initial height the animation will appear at.
        /// </summary>
        public int HeightStart;

        /// <summary>
        /// The height that the animation will end at.
        /// </summary>
        public int HeightEnd;

        /// <summary>
        /// The number of frames for the animation to stay at its starting position, before moving.
        /// </summary>
        public int LingerStart;

        /// <summary>
        /// The number of frames for the animation to stay at its ending position, after moving.
        /// </summary>
        public int LingerEnd;

        /// <summary>
        /// The number of frames it takes to move the animation.
        /// </summary>
        public int MoveTime;

        /// <summary>
        /// The animation to play once the movement is complete.
        /// </summary>
        public IEmittable ResultAnim;

        /// <summary>
        /// The layer to put the result animation in.
        /// </summary>
        public DrawLayer ResultLayer;


        public override void Update(BaseScene scene, FrameTick elapsedTime)
        {
            if (Anim.AnimIndex != "")
                scene.Anims[(int)DrawLayer.Normal].Add(new MoveToAnim(Anim, ResultAnim.CloneIEmittable(), ResultLayer, MoveTime, Origin + OffsetStart, Origin + OffsetEnd, LocHeight + HeightStart, LocHeight + HeightEnd, LingerStart, LingerEnd, Dir));
            finished = true;
        }
    }
}