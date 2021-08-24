using System;
using RogueElements;
using System.Runtime.Serialization;

namespace RogueEssence.Content
{

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
        
        public AnimData Anim;
        public Loc OffsetStart;
        public Loc OffsetEnd;
        public int HeightStart;
        public int HeightEnd;
        public int LingerStart;
        public int LingerEnd;
        public int MoveTime;


        public IEmittable ResultAnim;
        public DrawLayer ResultLayer;


        public override void Update(BaseScene scene, FrameTick elapsedTime)
        {
            if (Anim.AnimIndex != "")
                scene.Anims[(int)DrawLayer.Normal].Add(new MoveToAnim(Anim, ResultAnim.CloneIEmittable(), ResultLayer, MoveTime, Origin + OffsetStart, Origin + OffsetEnd, LocHeight + HeightStart, LocHeight + HeightEnd, LingerStart, LingerEnd, Dir));
            finished = true;
        }
    }
}