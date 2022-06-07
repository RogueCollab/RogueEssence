using System;
using System.Collections.Generic;
using RogueElements;

namespace RogueEssence.Content
{

    /// <summary>
    /// Emits particles within a specified start range, which all move outwards to the maximum radius.
    /// </summary>
    [Serializable]
    public class FiniteReleaseRangeEmitter : FiniteReleaseEmitter
    {
        public FiniteReleaseRangeEmitter()
            : base() { }
        public FiniteReleaseRangeEmitter(params AnimData[] anims) : base(anims) { }
        public FiniteReleaseRangeEmitter(params ParticleAnim[] anims) : base(anims) { }

        /// <summary>
        /// In Pixels
        /// </summary>
        public int Range;

        public FiniteReleaseRangeEmitter(FiniteReleaseRangeEmitter other)
            : base(other)
        {
            Range = other.Range;
        }
        public override BaseEmitter Clone() { return new FiniteReleaseRangeEmitter(this); }

        protected override void ReleaseAnim(BaseScene scene, int dist, Loc startLoc, Loc speed)
        {
            int totalTime = Range - dist;
            if (Speed > 0)
            {
                totalTime *= GraphicsManager.MAX_FPS;
                totalTime /= Speed;
            }

            IParticleEmittable chosenAnim = Anims[MathUtils.Rand.Next(Anims.Count)];
            scene.Anims[(int)Layer].Add(chosenAnim.CreateParticle(totalTime, startLoc, speed, Loc.Zero, LocHeight, 0, 0, speed.ApproximateDir8()));
        }
    }

    /// <summary>
    /// Emits particles within a specified start range, which all move outwards until they finish animating.
    /// </summary>
    [Serializable]
    public class FiniteReleaseEmitter : FiniteEmitter
    {
        public override bool Finished { get { return (CurrentBursts >= Math.Max(1, Bursts)); } }

        public FiniteReleaseEmitter()
        {
            Anims = new List<IParticleEmittable>();
            Layer = DrawLayer.Normal;
        }
        public FiniteReleaseEmitter(params AnimData[] anims) : this()
        {
            foreach (AnimData anim in anims)
                Anims.Add(new ParticleAnim(anim));
        }
        public FiniteReleaseEmitter(params ParticleAnim[] anims) : this()
        {
            foreach (ParticleAnim anim in anims)
                Anims.Add(anim);
        }

        public FiniteReleaseEmitter(FiniteReleaseEmitter other)
        {
            Anims = new List<IParticleEmittable>();
            foreach (IParticleEmittable anim in other.Anims)
                Anims.Add((IParticleEmittable)anim.CloneIEmittable());
            Bursts = other.Bursts;
            ParticlesPerBurst = other.ParticlesPerBurst;
            BurstTime = other.BurstTime;
            StartDistance = other.StartDistance;
            Speed = other.Speed;
            Layer = other.Layer;
        }

        public override BaseEmitter Clone() { return new FiniteReleaseEmitter(this); }

        public List<IParticleEmittable> Anims;
        
        /// <summary>
        /// In Pixels Per Second
        /// </summary>
        public int Speed;

        public int Bursts;
        public int ParticlesPerBurst;
        public int BurstTime;
        public int StartDistance;
        public DrawLayer Layer;

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
                for (int ii = 0; ii < ParticlesPerBurst; ii++)
                {
                    List<int> openDirs = getOpenDirs();
                    int openIndex = openDirs[MathUtils.Rand.Next(openDirs.Count)];
                    Coverages[openIndex] = true;

                    double angle = (openIndex + MathUtils.Rand.NextDouble()) * Math.PI / 4;

                    Loc particleSpeed = new Loc((int)(Math.Cos(angle) * Speed), (int)(Math.Sin(angle) * Speed));
                    int dist = MathUtils.Rand.Next(StartDistance + 1);
                    Loc startDelta = new Loc((int)Math.Round(Math.Cos(angle) * dist), (int)Math.Round(Math.Sin(angle) * dist));

                    if (Anims.Count > 0)
                        ReleaseAnim(scene, dist, Origin + startDelta, particleSpeed);
                }
                CurrentBursts++;

                if (CurrentBursts >= Math.Max(1, Bursts))
                    break;
            }

        }

        protected virtual void ReleaseAnim(BaseScene scene, int dist, Loc startLoc, Loc speed)
        {
            IParticleEmittable chosenAnim = Anims[MathUtils.Rand.Next(Anims.Count)];
            scene.Anims[(int)Layer].Add(chosenAnim.CreateParticle(startLoc, speed, Loc.Zero, LocHeight, 0, 0, speed.ApproximateDir8()));
        }

    }

    /// <summary>
    /// Emits particles within a specified start range, which all move outwards until they reach the maximum range.
    /// </summary>
    [Serializable]
    public class AttachReleaseRangeEmitter : AttachReleaseEmitter
    {
        public AttachReleaseRangeEmitter()
            : base() { }
        public AttachReleaseRangeEmitter(params AnimData[] anims) : base(anims) { }
        public AttachReleaseRangeEmitter(params ParticleAnim[] anims) : base(anims) { }

        /// <summary>
        /// In Pixels
        /// </summary>
        public int Range;

        public AttachReleaseRangeEmitter(AttachReleaseRangeEmitter other)
            : base(other)
        {
            Range = other.Range;
        }
        public override BaseEmitter Clone() { return new AttachReleaseRangeEmitter(this); }

        protected override void ReleaseAnim(BaseScene scene, int dist, Loc startLoc, Loc speed)
        {
            int totalTime = Range - dist;
            if (Speed > 0)
            {
                totalTime *= GraphicsManager.MAX_FPS;
                totalTime /= Speed;
            }

            IParticleEmittable chosenAnim = Anims[MathUtils.Rand.Next(Anims.Count)];
            scene.Anims[(int)Layer].Add(chosenAnim.CreateParticle(totalTime, startLoc, speed, Loc.Zero, LocHeight, 0, 0, speed.ApproximateDir8()));
        }

    }

    /// <summary>
    /// Emits particles within a specified start range, which all move outwards until they finish animating.
    /// </summary>
    [Serializable]
    public class AttachReleaseEmitter : AttachPointEmitter
    {
        public AttachReleaseEmitter()
        {
            Anims = new List<IParticleEmittable>();
            Layer = DrawLayer.Normal;
        }
        public AttachReleaseEmitter(params AnimData[] anims) : this()
        {
            foreach (AnimData anim in anims)
                Anims.Add(new ParticleAnim(anim));
        }
        public AttachReleaseEmitter(params ParticleAnim[] anims) : this()
        {
            foreach (ParticleAnim anim in anims)
                Anims.Add(anim);
        }

        public AttachReleaseEmitter(AttachReleaseEmitter other)
        {
            Anims = new List<IParticleEmittable>();
            foreach (IParticleEmittable anim in other.Anims)
                Anims.Add((IParticleEmittable)anim.CloneIEmittable());
            ParticlesPerBurst = other.ParticlesPerBurst;
            BurstTime = other.BurstTime;
            StartDistance = other.StartDistance;
            Speed = other.Speed;
            Layer = other.Layer;
        }
        public override BaseEmitter Clone() { return new AttachReleaseEmitter(this); }

        public List<IParticleEmittable> Anims;

        /// <summary>
        /// In Pixels Per Second
        /// </summary>
        public int Speed;

        public int ParticlesPerBurst;
        public int BurstTime;
        public int StartDistance;
        public DrawLayer Layer;

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
                    List<int> openDirs = getOpenDirs();
                    int openIndex = openDirs[MathUtils.Rand.Next(openDirs.Count)];
                    Coverages[openIndex] = true;

                    double angle = (openIndex + MathUtils.Rand.NextDouble()) * Math.PI / 4;

                    Loc particleSpeed = new Loc((int)(Math.Cos(angle) * Speed), (int)(Math.Sin(angle) * Speed));
                    int dist = MathUtils.Rand.Next(StartDistance + 1);
                    Loc startDelta = new Loc((int)Math.Round(Math.Cos(angle) * dist), (int)Math.Round(Math.Sin(angle) * dist));

                    if (Anims.Count > 0)
                        ReleaseAnim(scene, dist, Origin + startDelta, particleSpeed);
                }
            }

        }

        protected virtual void ReleaseAnim(BaseScene scene, int dist, Loc startLoc, Loc speed)
        {
            IParticleEmittable chosenAnim = Anims[MathUtils.Rand.Next(Anims.Count)];
            scene.Anims[(int)Layer].Add(chosenAnim.CreateParticle(startLoc, speed, Loc.Zero, LocHeight, 0, 0, speed.ApproximateDir8()));
        }

    }

    /// <summary>
    /// Emits particles within a specified start range, which all move outwards until they reach the max range specified by the hitbox.
    /// </summary>
    [Serializable]
    public class CircleSquareReleaseEmitter : CircleSquareEmitter
    {
        public override bool Finished { get { return (CurrentBursts >= Bursts); } }

        public CircleSquareReleaseEmitter()
        {
            Anims = new List<IParticleEmittable>();
            Layer = DrawLayer.Normal;
        }
        public CircleSquareReleaseEmitter(params AnimData[] anims) : this()
        {
            foreach (AnimData anim in anims)
                Anims.Add(new ParticleAnim(anim));
        }

        public CircleSquareReleaseEmitter(CircleSquareReleaseEmitter other)
        {
            Anims = new List<IParticleEmittable>();
            foreach (IParticleEmittable anim in other.Anims)
                Anims.Add((IParticleEmittable)anim.CloneIEmittable());
            Bursts = other.Bursts;
            ParticlesPerBurst = other.ParticlesPerBurst;
            BurstTime = other.BurstTime;
            StartDistance = other.StartDistance;
            Range = other.Range;
            Speed = other.Speed;
            Layer = other.Layer;
        }

        public override BaseEmitter Clone() { return new CircleSquareReleaseEmitter(this); }

        public List<IParticleEmittable> Anims;
        public int Bursts;
        public int ParticlesPerBurst;
        public int BurstTime;
        public int StartDistance;
        public DrawLayer Layer;
        
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
                for (int ii = 0; ii < ParticlesPerBurst; ii++)
                {
                    List<int> openDirs = getOpenDirs();
                    int openIndex = openDirs[MathUtils.Rand.Next(openDirs.Count)];
                    Coverages[openIndex] = true;

                    double angle = (openIndex + MathUtils.Rand.NextDouble()) * Math.PI / 4;

                    int dist = MathUtils.Rand.Next(StartDistance + 1);
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
                        int totalTime = Range - dist;
                        if (Speed > 0)
                        {
                            totalTime *= GraphicsManager.MAX_FPS;
                            totalTime /= Speed;
                        }

                        IParticleEmittable chosenAnim = Anims[MathUtils.Rand.Next(Anims.Count)];
                        scene.Anims[(int)Layer].Add(chosenAnim.CreateParticle(totalTime, Origin + startDelta, particleSpeed, Loc.Zero, 0, 0, 0, particleSpeed.ApproximateDir8()));
                    }
                }
                CurrentBursts++;

                if (CurrentBursts >= Bursts)
                    break;
            }

        }
    }
}