using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using RogueElements;

namespace RogueEssence.Content
{
    /// <summary>
    /// Emits particles within a specified circular area.
    /// </summary>
    [Serializable]
    public class StaticAreaEmitter : FiniteEmitter
    {
        public override bool Finished { get { return (CurrentBursts >= Bursts); } }

        public StaticAreaEmitter()
        {
            Anims = new List<IEmittable>();
            Layer = DrawLayer.Normal;
        }
        public StaticAreaEmitter(params AnimData[] anims) : this()
        {
            foreach (AnimData anim in anims)
                Anims.Add(new StaticAnim(anim));
        }

        public StaticAreaEmitter(StaticAreaEmitter other)
        {
            Anims = new List<IEmittable>();
            foreach (IEmittable anim in other.Anims)
                Anims.Add(anim.CloneIEmittable());
            Bursts = other.Bursts;
            ParticlesPerBurst = other.ParticlesPerBurst;
            BurstTime = other.BurstTime;
            Range = other.Range;
            LocHeight = other.LocHeight;
            Layer = other.Layer;
        }

        public override BaseEmitter Clone() { return new StaticAreaEmitter(this); }

        /// <summary>
        /// The particles to emit.
        /// </summary>
        public List<IEmittable> Anims;
        

        public int Bursts;

        public int ParticlesPerBurst;

        /// <summary>
        /// Number of frames between bursts.
        /// </summary>
        public int BurstTime;

        /// <summary>
        /// Radius of the emitting area in pixels.
        /// </summary>
        public int Range;

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

                    Loc startDelta = new Loc((int)Math.Round(Math.Cos(angle) * Range), (int)Math.Round(Math.Sin(angle) * Range));

                    if (Anims.Count > 0)
                    {
                        IEmittable chosenAnim = Anims[MathUtils.Rand.Next(Anims.Count)];
                        scene.Anims[(int)Layer].Add(chosenAnim.CreateStatic(Origin + startDelta, LocHeight, Dir));
                    }
                }
                CurrentBursts++;
                if (CurrentBursts >= Bursts)
                    break;
            }
        }
    }

    /// <summary>
    /// Emits particles within a specified circular area.
    /// The area of emission starts at a radius of 0 and grows to a maximum range.
    /// </summary>
    [Serializable]
    public class FiniteAreaEmitter : FiniteEmitter
    {
        public override bool Finished { get { return (CurrentRadius /*- LagRange*/ >= Range); } }

        public FiniteAreaEmitter()
        {
            Anims = new List<IEmittable>();
            Layer = DrawLayer.Normal;
        }
        public FiniteAreaEmitter(params AnimData[] anims) : this()
        {
            foreach (AnimData anim in anims)
                Anims.Add(new StaticAnim(anim));
        }

        public FiniteAreaEmitter(FiniteAreaEmitter other)
        {
            Anims = new List<IEmittable>();
            foreach (IEmittable anim in other.Anims)
                Anims.Add(anim.CloneIEmittable());
            TotalParticles = other.TotalParticles;
            //LagRange = other.LagRange;
            LocHeight = other.LocHeight;
            Range = other.Range;
            Speed = other.Speed;
            Layer = other.Layer;
        }

        public override BaseEmitter Clone() { return new FiniteAreaEmitter(this); }

        /// <summary>
        /// The particles to emit.
        /// </summary>
        public List<IEmittable> Anims;

        /// <summary>
        /// The maximum radius of the emitting area.
        /// In Pixels
        /// </summary>
        public int Range;

        /// <summary>
        /// Speed of the radius's increase.
        /// In Pixels Per Second
        /// </summary>
        public int Speed;

        /// <summary>
        /// Total particles to emit by the time the emitter finishes.
        /// </summary>
        public int TotalParticles;

        /// <summary>
        /// The draw layer to put the particles on.
        /// </summary>
        public DrawLayer Layer;

        [NonSerialized]
        private int CurrentRadius;
        [NonSerialized]
        private FrameTick ActionTime;

        public override void Update(BaseScene scene, FrameTick elapsedTime)
        {
            ActionTime += elapsedTime;
            int prevRadius = CurrentRadius;
            CurrentRadius = (int)ActionTime.FractionOf(Speed, GraphicsManager.MAX_FPS);

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

                    if (Anims.Count > 0)
                    {
                        IEmittable chosenAnim = Anims[MathUtils.Rand.Next(Anims.Count)];
                        scene.Anims[(int)Layer].Add(chosenAnim.CreateStatic(Origin + startDelta, LocHeight, Dir));
                    }
                }
            }

        }
    }

    /// <summary>
    /// Emits particles within a specified circular area, attached to a moving user or object.
    /// </summary>
    [Serializable]
    public class AttachAreaEmitter : AttachPointEmitter
    {
        public AttachAreaEmitter()
        {
            Anims = new List<IEmittable>();
            Layer = DrawLayer.Normal;
        }
        public AttachAreaEmitter(params AnimData[] anims) : this()
        {
            foreach (AnimData anim in anims)
                Anims.Add(new StaticAnim(anim));
        }

        public AttachAreaEmitter(AttachAreaEmitter other)
        {
            Anims = new List<IEmittable>();
            foreach (IEmittable anim in other.Anims)
                Anims.Add(anim.CloneIEmittable());
            ParticlesPerBurst = other.ParticlesPerBurst;
            BurstTime = other.BurstTime;
            AddHeight = other.AddHeight;
            Range = other.Range;
            Layer = other.Layer;
        }

        public override BaseEmitter Clone() { return new AttachAreaEmitter(this); }

        /// <summary>
        /// The particles to emit.
        /// </summary>
        public List<IEmittable> Anims;

        /// <summary>
        /// Radius of the emitting area in pixels.
        /// </summary>
        public int Range;

        public int ParticlesPerBurst;

        /// <summary>
        /// The amount of height to add to the particles.
        /// </summary>
        public int AddHeight;

        /// <summary>
        /// Number of frames between bursts.
        /// </summary>
        public int BurstTime;

        /// <summary>
        /// The draw layer to put the particles on.
        /// </summary>
        public DrawLayer Layer;

        [NonSerialized]
        private FrameTick CurrentBurstTime;
        [NonSerialized]
        private FrameTick ActionTime;

        public override void Update(BaseScene scene, FrameTick elapsedTime)
        {
            ActionTime += elapsedTime;

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

                    int dist = MathUtils.Rand.Next(Range + 1);
                    Loc startDelta = new Loc((int)Math.Round(Math.Cos(angle) * dist), (int)Math.Round(Math.Sin(angle) * dist));

                    if (Anims.Count > 0)
                    {
                        IEmittable chosenAnim = Anims[MathUtils.Rand.Next(Anims.Count)];
                        scene.Anims[(int)Layer].Add(chosenAnim.CreateStatic(Origin + startDelta, LocHeight + AddHeight, Dir));
                    }
                }
            }
        }
    }

    /// <summary>
    /// Emits particles within the range of the hitbox that owns it.
    /// The area of emission is a circle that starts at a radius of 0 and grows to the range of the square-shaped hitbox.
    /// </summary>
    [Serializable]
    public class CircleSquareAreaEmitter : CircleSquareEmitter
    {
        public override bool Finished { get { return (CurrentRadius >= Range); } }

        public CircleSquareAreaEmitter()
        {
            Anims = new List<IEmittable>();
            Layer = DrawLayer.Normal;
        }
        public CircleSquareAreaEmitter(params AnimData[] anims) : this()
        {
            foreach (AnimData anim in anims)
                Anims.Add(new StaticAnim(anim));
        }

        public CircleSquareAreaEmitter(CircleSquareAreaEmitter other)
        {
            Anims = new List<IEmittable>();
            foreach(IEmittable anim in other.Anims)
                Anims.Add(anim.CloneIEmittable());
            ParticlesPerTile = other.ParticlesPerTile;
            LocHeight = other.LocHeight;
            Range = other.Range;
            RangeDiff = other.RangeDiff;
            Speed = other.Speed;
            Layer = other.Layer;
        }

        public override BaseEmitter Clone() { return new CircleSquareAreaEmitter(this); }

        /// <summary>
        /// The particles to emit.
        /// </summary>
        public List<IEmittable> Anims;

        /// <summary>
        /// The amount of particles to emit per tile covered by the hitbox.
        /// </summary>
        public double ParticlesPerTile;

        /// <summary>
        /// A modification to the emission radius initially supplied by the hitbox, in pixels.
        /// Negative numbers take the hitbox's radius and decrease it.
        /// Positive numbers take the hitbox's radius and increase it.
        /// </summary>
        public int RangeDiff;

        /// <summary>
        /// The draw layer to put the particles on.
        /// </summary>
        public DrawLayer Layer; 

        [NonSerialized]
        private int CurrentRadius;
        [NonSerialized]
        private FrameTick ActionTime;

        public override void Update(BaseScene scene, FrameTick elapsedTime)
        {
            ActionTime += elapsedTime;
            int prevRadius = CurrentRadius;
            CurrentRadius = (int)ActionTime.FractionOf(Speed, GraphicsManager.MAX_FPS);
            int endRange = Range + RangeDiff;
            if (CurrentRadius > endRange)
                CurrentRadius = endRange;

            int totalParticles = (int)Math.Round(ParticlesPerTile * (Math.PI * endRange * endRange) / GraphicsManager.TileSize / GraphicsManager.TileSize);
            int prevParticles = 0;
            int currentParticles = totalParticles;
            if (Range > 0)
            {
                prevParticles = totalParticles * prevRadius * prevRadius / endRange / endRange;
                currentParticles = totalParticles * CurrentRadius * CurrentRadius / endRange / endRange;
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
                else if (AreaLimit == Dungeon.Hitbox.AreaLimit.Cross)
                {
                    dist -= GraphicsManager.TileSize / 2;
                    int diffDist = MathUtils.Rand.Next(GraphicsManager.TileSize / 2 + 1);
                    startDelta += new Loc((int)Math.Round(Math.Cos(angle) * diffDist), (int)Math.Round(Math.Sin(angle) * diffDist));

                    //any multiple of 90
                    int dir = MathUtils.Rand.Next(4);
                    angle = (dir * 90) * Math.PI / 180;
                }
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

                startDelta += new Loc((int)Math.Round(Math.Cos(angle) * dist), (int)Math.Round(Math.Sin(angle) * dist));

                if (Anims.Count > 0)
                {
                    IEmittable chosenAnim = Anims[MathUtils.Rand.Next(Anims.Count)];
                    scene.Anims[(int)Layer].Add(chosenAnim.CreateStatic(Origin + startDelta, LocHeight, Dir));
                }
            }
        }
    }
}