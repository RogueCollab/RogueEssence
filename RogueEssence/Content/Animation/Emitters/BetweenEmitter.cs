using System;
using RogueElements;

namespace RogueEssence.Content
{

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

        public IEmittable AnimBack;
        public IEmittable AnimFront;
        public int Offset;
        public int HeightBack;
        public int HeightFront;


        public override void Update(BaseScene scene, FrameTick elapsedTime)
        {
            Loc center = Origin + Dir.GetLoc() * Offset;

            scene.Anims[(int)DrawLayer.Back].Add(AnimBack.CreateStatic(center, HeightBack, Dir));
            scene.Anims[(int)DrawLayer.Front].Add(AnimFront.CreateStatic(center, HeightFront, Dir));

            finished = true;
        }

    }
}