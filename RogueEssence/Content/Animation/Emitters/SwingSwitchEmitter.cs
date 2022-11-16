using System;
using RogueElements;

namespace RogueEssence.Content
{
    /// <summary>
    /// Emits a particle that moves from the source to the target, with a parabolic circular motion.
    /// </summary>
    [Serializable]
    public class SwingSwitchEmitter : FiniteEmitter
    {
        private bool finished;
        public override bool Finished { get { return finished; } }

        public SwingSwitchEmitter()
        {
            Anim = new AnimData();
            Layer = DrawLayer.Normal;
        }
        public SwingSwitchEmitter(AnimData anim)
        {
            Anim = anim;
            Layer = DrawLayer.Normal;
        }
        public SwingSwitchEmitter(SwingSwitchEmitter other)
        {
            Anim = new AnimData(other.Anim);
            Amount = other.Amount;
            StreamTime = other.StreamTime;
            RotationTime = other.RotationTime;
            Offset = other.Offset;
            LocHeight = other.LocHeight;
            Layer = other.Layer;
            AxisRatio = other.AxisRatio;
        }

        public override BaseEmitter Clone() { return new SwingSwitchEmitter(this); }

        public int Amount;
        public int StreamTime;

        public int Offset;
        public AnimData Anim;
        public int RotationTime;
        public float AxisRatio;
        public DrawLayer Layer;


        public override void Update(BaseScene scene, FrameTick elapsedTime)
        {
            if (Anim.AnimIndex != "")
            {
                scene.Anims[(int)Layer].Add(new SwingAnim(Anim, Math.Max(1, RotationTime), Origin + Dir.GetLoc() * Offset, Destination + Dir.GetLoc() * Offset, AxisRatio, Dir));
                scene.Anims[(int)Layer].Add(new SwingAnim(Anim, Math.Max(1, RotationTime), Destination + Dir.GetLoc() * Offset, Origin + Dir.GetLoc() * Offset, AxisRatio, Dir));
            }
            finished = true;
        }
    }
}
