using System;
using RogueElements;

namespace RogueEssence.Content
{


    [Serializable]
    public class ScreenRainEmitter : SwitchOffEmitter
    {
        private bool finished;
        public override bool Finished { get { return finished; } }

        public ScreenRainEmitter()
        { }

        public ScreenRainEmitter(AnimData anim) : this()
        {
            Anim = anim;
        }

        public ScreenRainEmitter(ScreenRainEmitter other)
        {
            Anim = new AnimData(other.Anim);
            ResultAnim = new AnimData(other.ResultAnim);
            Layer = other.Layer;
            ParticlesPerBurst = other.ParticlesPerBurst;
            BurstTime = other.BurstTime;
            HeightSpeed = other.HeightSpeed;
            SpeedDiff = other.SpeedDiff;
        }

        public override BaseEmitter Clone() { return new ScreenRainEmitter(this); }

        public AnimData Anim;
        public AnimData ResultAnim;
        public DrawLayer Layer;
        public int ParticlesPerBurst;
        public int BurstTime;
        //Pixels per sec
        public int HeightSpeed;
        //Pixels per sec
        public int SpeedDiff;

        [NonSerialized]
        private FrameTick CurrentBurstTime;

        public override void Update(BaseScene scene, FrameTick elapsedTime)
        {

            CurrentBurstTime += elapsedTime;
            while (CurrentBurstTime >= Math.Max(1, BurstTime))
            {
                CurrentBurstTime -= Math.Max(1, BurstTime);
                for (int ii = 0; ii < ParticlesPerBurst; ii++)
                {
                    Loc startLoc = new Loc(MathUtils.Rand.Next(GraphicsManager.ScreenWidth), MathUtils.Rand.Next(GraphicsManager.ScreenHeight * 2));
                    int height = startLoc.Y;
                    int time = (height - 1) * GraphicsManager.MAX_FPS / -HeightSpeed;// + 1;
                    startLoc += scene.ViewRect.Start;

                    WrappedRainAnim anim = new WrappedRainAnim(Anim, ResultAnim, Layer, time);
                    anim.SetupEmitted(startLoc, new Loc(SpeedDiff, 0), Loc.Zero, height, HeightSpeed, 0, Dir);
                    scene.Anims[(int)Layer].Add(anim);
                }
            }
        }

        public override void SwitchOff()
        {
            finished = true;
        }
    }

}