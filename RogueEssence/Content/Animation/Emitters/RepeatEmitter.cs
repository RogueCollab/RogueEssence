using System;
using RogueElements;

namespace RogueEssence.Content
{
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

        public IEmittable Anim;
        public int Bursts;
        public int BurstTime;
        public DrawLayer Layer;
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
