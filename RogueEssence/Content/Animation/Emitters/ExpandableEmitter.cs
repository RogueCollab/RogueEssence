using System;
using RogueElements;

namespace RogueEssence.Content
{
    [Serializable]
    public class ExpandableEmitter : CircleSquareEmitter
    {
        private bool finished;
        public override bool Finished { get { return finished; } }

        public ExpandableEmitter()
        {
            Anim = new MultiAnim();
            Layer = DrawLayer.Normal;
        }
        public ExpandableEmitter(MultiAnim anim)
        {
            Anim = anim;
            Layer = DrawLayer.Normal;
        }
        public ExpandableEmitter(ExpandableEmitter other)
        {
            Anim = other.Anim.Clone();
            Cycles = other.Cycles;
            Offset = other.Offset;
            LocHeight = other.LocHeight;
            Layer = other.Layer;
        }

        public override BaseEmitter Clone() { return new ExpandableEmitter(this); }

        public int Offset;
        public MultiAnim Anim;
        public int Cycles;
        public DrawLayer Layer;

        /// <summary>
        /// In pixels
        /// </summary>
        public int DefaultRadius;

        public override void Update(BaseScene scene, FrameTick elapsedTime)
        {
            MultiAnim anim = new MultiAnim(Anim, (float)Range / DefaultRadius, Origin + Dir.GetLoc() * Offset, Dir, LocHeight, Cycles);
            scene.Anims[(int)Layer].Add(anim);
            finished = true;
        }
    }
}
