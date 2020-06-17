using System;
using System.Collections.Generic;
using RogueElements;

namespace RogueEssence.Content
{
    [Serializable]
    public class ListEmitter : FiniteEmitter
    {
        private bool finished;
        public override bool Finished { get { return finished; } }

        public ListEmitter()
        {
            Anim = new List<IEmittable>();
            Layer = DrawLayer.Normal;
        }
        public ListEmitter(ListEmitter other)
        {
            Anim = new List<IEmittable>();
            foreach (IEmittable emittable in other.Anim)
                Anim.Add(emittable.CloneIEmittable());
            Offset = other.Offset;
            LocHeight = other.LocHeight;
            Layer = other.Layer;
            UseDest = other.UseDest;
        }

        public override BaseEmitter Clone() { return new ListEmitter(this); }

        public int Offset;
        public List<IEmittable> Anim;
        public DrawLayer Layer;
        public bool UseDest;


        public override void Update(BaseScene scene, FrameTick elapsedTime)
        {
            foreach(IEmittable anim in Anim)
                scene.Anims[(int)Layer].Add(anim.CreateStatic(UseDest ? Destination : Origin + Dir.GetLoc() * Offset, LocHeight, Dir));
            finished = true;
        }


        public override string ToString()
        {
            return "List["+Anim.Count + "]";
        }
    }
}
