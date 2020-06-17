using System;
using RogueElements;

namespace RogueEssence.Content
{
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

        public int Offset;
        public IEmittable Anim;
        public DrawLayer Layer;
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
