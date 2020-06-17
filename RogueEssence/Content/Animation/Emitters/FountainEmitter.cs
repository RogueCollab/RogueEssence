using System;
using System.Collections.Generic;
using RogueElements;

namespace RogueEssence.Content
{


    [Serializable]
    public class CircleSquareFountainEmitter : CircleSquareEmitter
    {
        public override bool Finished { get { return (CurrentBursts >= Bursts); } }

        public CircleSquareFountainEmitter()
        {
            Anims = new List<IParticleEmittable>();
            Layer = DrawLayer.Normal;
        }
        public CircleSquareFountainEmitter(params AnimData[] anims) : this()
        {
            foreach (AnimData anim in anims)
                Anims.Add(new ParticleAnim(anim));
        }

        public CircleSquareFountainEmitter(CircleSquareFountainEmitter other)
        {
            Anims = new List<IParticleEmittable>();
            foreach (IParticleEmittable anim in other.Anims)
                Anims.Add((IParticleEmittable)anim.CloneIEmittable());
            Bursts = other.Bursts;
            ParticlesPerBurst = other.ParticlesPerBurst;
            BurstTime = other.BurstTime;
            StartDistance = other.StartDistance;
            HeightRatio = other.HeightRatio;
            RangeDiff = other.RangeDiff;
            Range = other.Range;
            Speed = other.Speed;
            Layer = other.Layer;
        }

        public override BaseEmitter Clone() { return new CircleSquareFountainEmitter(this); }

        public List<IParticleEmittable> Anims;
        public int Bursts;
        public int ParticlesPerBurst;
        public int BurstTime;
        public int StartDistance;
        public int RangeDiff;
        public float HeightRatio;
        public DrawLayer Layer;
        
        [NonSerialized]
        private FrameTick CurrentBurstTime;
        [NonSerialized]
        private int CurrentBursts;

        public override void Update(BaseScene scene, FrameTick elapsedTime)
        {
            CurrentBurstTime += elapsedTime;
            while (CurrentBurstTime >= BurstTime)
            {
                CurrentBurstTime -= BurstTime;
                for (int ii = 0; ii < ParticlesPerBurst; ii++)
                {
                    List<int> openDirs = getOpenDirs();
                    int openIndex = openDirs[MathUtils.Rand.Next(openDirs.Count)];
                    Coverages[openIndex] = true;

                    double angle = (openIndex + MathUtils.Rand.NextDouble()) * Math.PI / 4;

                    int dist = Math.Min(Range, MathUtils.Rand.Next(StartDistance + 1));
                    Loc startDelta = new Loc((int)Math.Round(Math.Cos(angle) * dist), (int)Math.Round(Math.Sin(angle) * dist));
                    if (AreaLimit == Dungeon.Hitbox.AreaLimit.Cone)
                        angle = (45 * (int)Dir + 45) * Math.PI / 180 + angle / 4;
                    else if (AreaLimit == Dungeon.Hitbox.AreaLimit.Sides)
                    {
                        if (Dir.IsDiagonal())
                        {
                            //either +135 or -135 from the direction
                            if (MathUtils.Rand.Next(2) == 0)
                                angle = (45 * (int)Dir + 90 + 135) * Math.PI / 180;
                            else
                                angle = (45 * (int)Dir + 90 - 135) * Math.PI / 180;
                        }
                        else
                        {
                            //either +90 or -90 from the direction
                            if (MathUtils.Rand.Next(2) == 0)
                                angle = (45 * (int)Dir + 90 + 90) * Math.PI / 180;
                            else
                                angle = (45 * (int)Dir + 90 - 90) * Math.PI / 180;
                        }
                    }

                    Loc particleSpeed = new Loc((int)(Math.Cos(angle) * Speed), (int)(Math.Sin(angle) * Speed));

                    if (Anims.Count > 0)
                    {
                        //pixels
                        int resultRange = MathUtils.Rand.Next(dist, Range + RangeDiff) + 1;
                        int totalTime = resultRange - dist;
                        if (Speed > 0)
                        {
                            totalTime *= GraphicsManager.MAX_FPS;
                            totalTime /= Speed;
                        }

                        float maxHeight = HeightRatio * resultRange;
                        float velocity = 4 * maxHeight * GraphicsManager.MAX_FPS / totalTime;
                        float acceleration = -8 * maxHeight * GraphicsManager.MAX_FPS * GraphicsManager.MAX_FPS / totalTime / totalTime;

                        IParticleEmittable chosenAnim = Anims[MathUtils.Rand.Next(Anims.Count)];
                        scene.Anims[(int)Layer].Add(chosenAnim.CreateParticle(totalTime, Origin + startDelta, particleSpeed, Loc.Zero, 0, (int)Math.Round(velocity), (int)Math.Round(acceleration), particleSpeed.ApproximateDir8()));
                    }
                }
                CurrentBursts++;

                if (CurrentBursts >= Bursts)
                    break;
            }

        }
    }
}