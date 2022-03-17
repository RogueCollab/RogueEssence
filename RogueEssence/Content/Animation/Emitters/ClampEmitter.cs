using System;
using RogueElements;

namespace RogueEssence.Content
{

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
        
        public AnimData Anim1;
        public AnimData Anim2;
        public int Offset;
        public Loc HalfOffset;
        public int HalfHeight;
        public int LingerStart;
        public int MoveTime;
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