using System;
using System.Collections.Generic;
using RogueElements;

namespace RogueEssence.Content
{


    [Serializable]
    public class FiniteSprinkleEmitter : FiniteEmitter
    {
        public override bool Finished { get { return (CurrentRadius /*- LagRange*/ >= Range); } }
        public FiniteSprinkleEmitter()
        {
            Anims = new List<IParticleEmittable>();
        }
        public FiniteSprinkleEmitter(params AnimData[] anims) : this()
        {
            foreach (AnimData anim in anims)
                Anims.Add(new ParticleAnim(anim));
        }


        public FiniteSprinkleEmitter(FiniteSprinkleEmitter other)
        {
            Anims = new List<IParticleEmittable>();
            foreach (IParticleEmittable anim in other.Anims)
                Anims.Add((IParticleEmittable)anim.CloneIEmittable());
            TotalParticles = other.TotalParticles;
            //LagRange = other.LagRange;
            Range = other.Range;
            Speed = other.Speed;
            AnimDir = other.AnimDir;
            HeightSpeed = other.HeightSpeed;
            SpeedDiff = other.SpeedDiff;
            StartHeight = other.StartHeight;
        }

        public override BaseEmitter Clone() { return new FiniteSprinkleEmitter(this); }

        public List<IParticleEmittable> Anims;

        /// <summary>
        /// In Pixels
        /// </summary>
        public int Range;

        /// <summary>
        /// In Pixels Per Second
        /// </summary>
        public int Speed;

        public int TotalParticles;
        public Dir8 AnimDir;
        public int HeightSpeed;
        public int SpeedDiff;
        public int StartHeight;

        [NonSerialized]
        private int CurrentRadius;
        [NonSerialized]
        private FrameTick ActionTime;

        public override void Update(BaseScene scene, FrameTick elapsedTime)
        {
            ActionTime += elapsedTime;
            int prevRadius = CurrentRadius;
            CurrentRadius = (int)ActionTime.FractionOf(Speed, GraphicsManager.MAX_FPS);
            if (CurrentRadius > Range)
                CurrentRadius = Range;

            int prevParticles = 0;
            int currentParticles = TotalParticles;
            if (Range > 0)
            {
                prevParticles = TotalParticles * prevRadius * prevRadius / Range / Range;
                currentParticles = TotalParticles * CurrentRadius * CurrentRadius / Range / Range;
            }

            for (int ii = prevParticles; ii < currentParticles; ii++)
            {
                List<int> openDirs = getOpenDirs();
                int openIndex = openDirs[MathUtils.Rand.Next(openDirs.Count)];
                Coverages[openIndex] = true;

                double angle = (openIndex + MathUtils.Rand.NextDouble()) * Math.PI / 4;

                int dist = CurrentRadius;

                if (dist >= 0 && dist <= Range)
                {
                    Loc startDelta = new Loc((int)Math.Round(Math.Cos(angle) * dist), (int)Math.Round(Math.Sin(angle) * dist));

                    Loc randDiff = new Loc((int)((MathUtils.Rand.NextDouble() * 2 - 1) * SpeedDiff), 0);

                    if (Anims.Count > 0)
                    {
                        IParticleEmittable chosenAnim = Anims[MathUtils.Rand.Next(Anims.Count)];
                        scene.Anims[(int)DrawLayer.Normal].Add(chosenAnim.CreateParticle(Origin + startDelta, randDiff, Loc.Zero, StartHeight, HeightSpeed, 0, AnimDir));
                    }
                }
            }
        }
    }

    [Serializable]
    public class AttachSprinkleEmitter : AttachPointEmitter
    {
        public AttachSprinkleEmitter()
        {
            Anims = new List<IParticleEmittable>();
        }

        public AttachSprinkleEmitter(AttachSprinkleEmitter other)
        {
            Anims = new List<IParticleEmittable>();
            foreach (IParticleEmittable anim in other.Anims)
                Anims.Add((IParticleEmittable)anim.CloneIEmittable());
            ParticlesPerBurst = other.ParticlesPerBurst;
            BurstTime = other.BurstTime;
            Range = other.Range;
            AnimDir = other.AnimDir;
            HeightSpeed = other.HeightSpeed;
            SpeedDiff = other.SpeedDiff;
            StartHeight = other.StartHeight;
        }

        public override BaseEmitter Clone() { return new AttachSprinkleEmitter(this); }

        public List<IParticleEmittable> Anims;
        public int Range;//pixels!
        public int ParticlesPerBurst;
        public int BurstTime;
        public Dir8 AnimDir;
        public int HeightSpeed;
        public int SpeedDiff;
        public int StartHeight;

        [NonSerialized]
        private FrameTick CurrentBurstTime;
        [NonSerialized]
        private FrameTick ActionTime;

        public override void Update(BaseScene scene, FrameTick elapsedTime)
        {
            ActionTime += elapsedTime;

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

                    int dist = MathUtils.Rand.Next(Range + 1);

                    if (dist >= 0 && dist <= Range)
                    {
                        Loc startDelta = new Loc((int)Math.Round(Math.Cos(angle) * dist), (int)Math.Round(Math.Sin(angle) * dist));

                        Loc randDiff = new Loc((int)((MathUtils.Rand.NextDouble() * 2 - 1) * SpeedDiff), 0);

                        if (Anims.Count > 0)
                        {
                            IParticleEmittable chosenAnim = Anims[MathUtils.Rand.Next(Anims.Count)];
                            scene.Anims[(int)DrawLayer.Normal].Add(chosenAnim.CreateParticle(Origin + startDelta, randDiff, Loc.Zero, StartHeight + LocHeight, HeightSpeed, 0, AnimDir));
                        }
                    }
                }
            }

        }
    }

    [Serializable]
    public class CircleSquareSprinkleEmitter : CircleSquareEmitter
    {
        public override bool Finished { get { return (CurrentRadius /*- LagRange*/ >= Range); } }

        public CircleSquareSprinkleEmitter()
        {
            Anims = new List<IParticleEmittable>();
        }
        public CircleSquareSprinkleEmitter(AnimData anim, int cycles, int totalTime) : this()
        {
            Anims.Add(new ParticleAnim(anim, cycles, totalTime));
        }
        public CircleSquareSprinkleEmitter(params AnimData[] anims) : this()
        {
            foreach (AnimData anim in anims)
                Anims.Add(new ParticleAnim(anim));
        }


        public CircleSquareSprinkleEmitter(CircleSquareSprinkleEmitter other)
        {
            Anims = new List<IParticleEmittable>();
            foreach (IParticleEmittable anim in other.Anims)
                Anims.Add((IParticleEmittable)anim.CloneIEmittable());
            ParticlesPerTile = other.ParticlesPerTile;
            //LagRange = other.LagRange;
            AnimDir = other.AnimDir;
            HeightSpeed = other.HeightSpeed;
            SpeedDiff = other.SpeedDiff;
            StartHeight = other.StartHeight;
        }

        public override BaseEmitter Clone() { return new CircleSquareSprinkleEmitter(this); }

        public List<IParticleEmittable> Anims;
        public double ParticlesPerTile;
        
        public Dir8 AnimDir;

        /// <summary>
        /// Pixels Per Second
        /// </summary>
        public int HeightSpeed;

        /// <summary>
        /// Pixels Per Second
        /// </summary>
        public int SpeedDiff;
        public int StartHeight;

        [NonSerialized]
        private int CurrentRadius;
        [NonSerialized]
        private FrameTick ActionTime;

        public override void Update(BaseScene scene, FrameTick elapsedTime)
        {
            ActionTime += elapsedTime;
            int prevRadius = CurrentRadius;
            CurrentRadius = (int)ActionTime.FractionOf(Speed, GraphicsManager.MAX_FPS);
            if (CurrentRadius > Range)
                CurrentRadius = Range;

            int totalParticles = (int)Math.Round(ParticlesPerTile * (Math.PI * Range * Range) / GraphicsManager.TileSize / GraphicsManager.TileSize);
            int prevParticles = 0;
            int currentParticles = totalParticles;
            if (Range > 0)
            {
                prevParticles = totalParticles * prevRadius * prevRadius / Range / Range;
                currentParticles = totalParticles * CurrentRadius * CurrentRadius / Range / Range;
            }

            for (int ii = prevParticles; ii < currentParticles; ii++)
            {
                List<int> openDirs = getOpenDirs();
                int openIndex = openDirs[MathUtils.Rand.Next(openDirs.Count)];
                Coverages[openIndex] = true;

                double angle = (openIndex + MathUtils.Rand.NextDouble()) * Math.PI / 4;
                Loc startDelta = new Loc();
                int dist = CurrentRadius;
                if (AreaLimit == Dungeon.Hitbox.AreaLimit.Cone)
                    angle = (45 * (int)Dir + 45) * Math.PI / 180 + angle / 4;
                else if (AreaLimit == Dungeon.Hitbox.AreaLimit.Sides)
                {
                    dist -= GraphicsManager.TileSize / 2;
                    int diffDist = MathUtils.Rand.Next(GraphicsManager.TileSize / 2 + 1);
                    startDelta += new Loc((int)Math.Round(Math.Cos(angle) * diffDist), (int)Math.Round(Math.Sin(angle) * diffDist));
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

                if (dist >= 0 && dist <= Range)
                {
                    startDelta += new Loc((int)Math.Round(Math.Cos(angle) * dist), (int)Math.Round(Math.Sin(angle) * dist));

                    Loc randDiff = new Loc((int)((MathUtils.Rand.NextDouble() * 2 - 1) * SpeedDiff), 0);

                    if (Anims.Count > 0)
                    {
                        IParticleEmittable chosenAnim = Anims[MathUtils.Rand.Next(Anims.Count)];
                        scene.Anims[(int)DrawLayer.Normal].Add(chosenAnim.CreateParticle(Origin + startDelta, randDiff, Loc.Zero, StartHeight, HeightSpeed, 0, AnimDir));
                    }
                }
            }

        }
    }
}