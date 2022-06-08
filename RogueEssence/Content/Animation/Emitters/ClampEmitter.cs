using System;
using RogueElements;

namespace RogueEssence.Content
{
    /// <summary>
    /// Creates two anims, separated by a width or a height, and then moves them towards each other in a clamping motion.
    /// </summary>
    [Serializable]
    public class ClampEmitter : FiniteEmitter
    {
        private bool finished;
        public override bool Finished { get { return finished; } }

        public ClampEmitter()
        {
            Anim1 = new AnimData();
            Anim2 = new AnimData();
        }
        public ClampEmitter(ClampEmitter other)
        {
            Anim1 = new AnimData(other.Anim1);
            Anim2 = new AnimData(other.Anim2);
            Offset = other.Offset;
            LocHeight = other.LocHeight;
            HalfOffset = other.HalfOffset;
            HalfHeight = other.HalfHeight;
            LingerStart = other.LingerStart;
            MoveTime = other.MoveTime;
            LingerEnd = other.LingerEnd;
        }

        public override BaseEmitter Clone() { return new ClampEmitter(this); }
        
        /// <summary>
        /// Anim at the bottom.
        /// </summary>
        public AnimData Anim1;
        /// <summary>
        /// Anim at the top.
        /// </summary>
        public AnimData Anim2;

        /// <summary>
        /// The number of pixels to move both animations forward, in the direction of the animation.
        /// </summary>
        public int Offset;

        /// <summary>
        /// The number of pixels to offset the bottom anim one way, and the top anim in the reverse way, before clamping them together.
        /// </summary>
        public Loc HalfOffset;
        
        /// <summary>
        /// The number of pixels in height to move the bottom anim down, and the top anim up, before clamping them together.
        /// </summary>
        public int HalfHeight;

        /// <summary>
        /// The number of frames for animations to linger before starting the clamp.
        /// </summary>
        public int LingerStart;

        /// <summary>
        /// The number of frames it takes for the clamp to complete from start to finish.
        /// </summary>
        public int MoveTime;

        /// <summary>
        /// The number of frames for animations to linger after the clamp is complete.
        /// </summary>
        public int LingerEnd;


        public override void Update(BaseScene scene, FrameTick elapsedTime)
        {
            Loc center = Origin + Dir.GetLoc() * Offset;
            if (Anim1.AnimIndex != "")
                scene.Anims[(int)DrawLayer.Normal].Add(new MoveToAnim(Anim1, new EmptyFiniteEmitter(), DrawLayer.Normal, MoveTime, center - HalfOffset, center, LocHeight - HalfHeight, LocHeight, LingerStart, LingerEnd, Dir));
            if (Anim2.AnimIndex != "")
                scene.Anims[(int)DrawLayer.Normal].Add(new MoveToAnim(Anim2, new EmptyFiniteEmitter(), DrawLayer.Normal, MoveTime, center + HalfOffset, center, LocHeight + HalfHeight, LocHeight, LingerStart, LingerEnd, Dir));
            finished = true;
        }
        
    }
}