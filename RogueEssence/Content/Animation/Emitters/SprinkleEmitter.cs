using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using RogueElements;

namespace RogueEssence.Content
{
    /// <summary>
    /// Emits particles within a specified circular area.
    /// The area of emission starts at a radius of 0 and grows to a maximum range.
    /// The particles themselves will move in the specified direction.
    /// Often used to simulate confetti-like objects that float downwards.
    /// </summary>
    [Serializable]
    public class FiniteSprinkleEmitter : FiniteEmitter
    {
        public override bool Finished { get { return (CurrentRadius /*- LagRange*/ >= Range); } }
        public FiniteSprinkleEmitter()
        {
            Anims = new List<IParticleEmittable>();
            Layer = DrawLayer.Normal;
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
            HeightSpeed = other.HeightSpeed;
            SpeedDiff = other.SpeedDiff;
            StartHeight = other.StartHeight;
            Layer = other.Layer;
        }

        public override BaseEmitter Clone() { return new FiniteSprinkleEmitter(this); }

        /// <summary>
        /// The particles to emit.
        /// </summary>
        public List<IParticleEmittable> Anims;

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
        /// The speed for the particles to travel in height.
        /// In pixels per second.
        /// </summary>
        public int HeightSpeed;

        /// <summary>
        /// How far left or right the particle's trajectory will drift as it moves.
        /// In pixels per second.
        /// </summary>
        public int SpeedDiff;

        /// <summary>
        /// Height added to the particles when they are initialy created.
        /// </summary>
        public int StartHeight;

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
                        scene.Anims[(int)Layer].Add(chosenAnim.CreateParticle(Origin + startDelta, randDiff, Loc.Zero, StartHeight, HeightSpeed, 0, Dir));
                    }
                }
            }
        }
    }

    /// <summary>
    /// Emits particles within a specified circular area, attached to a moving user or object.
    /// The particles themselves will move in the specified direction.
    /// Often used to simulate confetti-like objects that float downwards.
    /// </summary>
    [Serializable]
    public class AttachSprinkleEmitter : AttachPointEmitter
    {
        public AttachSprinkleEmitter()
        {
            Anims = new List<IParticleEmittable>();
            Layer = DrawLayer.Normal;
        }

        public AttachSprinkleEmitter(AttachSprinkleEmitter other)
        {
            Anims = new List<IParticleEmittable>();
            foreach (IParticleEmittable anim in other.Anims)
                Anims.Add((IParticleEmittable)anim.CloneIEmittable());
            ParticlesPerBurst = other.ParticlesPerBurst;
            BurstTime = other.BurstTime;
            Range = other.Range;
            HeightSpeed = other.HeightSpeed;
            SpeedDiff = other.SpeedDiff;
            StartHeight = other.StartHeight;
            Layer = other.Layer;
        }

        public override BaseEmitter Clone() { return new AttachSprinkleEmitter(this); }

        /// <summary>
        /// The particles to emit.
        /// </summary>
        public List<IParticleEmittable> Anims;

        /// <summary>
        /// Radius of the emitting area in pixels.
        /// </summary>
        public int Range;

        public int ParticlesPerBurst;

        /// <summary>
        /// Number of frames between bursts.
        /// </summary>
        public int BurstTime;

        /// <summary>
        /// The speed for the particles to travel in height.
        /// In pixels per second.
        /// </summary>
        public int HeightSpeed;

        /// <summary>
        /// How far left or right the particle's trajectory will drift as it moves.
        /// In pixels per second.
        /// </summary>
        public int SpeedDiff;

        /// <summary>
        /// The amount of height to add to the particles when they initially spawn.
        /// </summary>
        public int StartHeight;

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

                    if (dist >= 0 && dist <= Range)
                    {
                        Loc startDelta = new Loc((int)Math.Round(Math.Cos(angle) * dist), (int)Math.Round(Math.Sin(angle) * dist));

                        Loc randDiff = new Loc((int)((MathUtils.Rand.NextDouble() * 2 - 1) * SpeedDiff), 0);

                        if (Anims.Count > 0)
                        {
                            IParticleEmittable chosenAnim = Anims[MathUtils.Rand.Next(Anims.Count)];
                            scene.Anims[(int)Layer].Add(chosenAnim.CreateParticle(Origin + startDelta, randDiff, Loc.Zero, StartHeight + LocHeight, HeightSpeed, 0, Dir));
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Emits particles within the range of the hitbox that owns it.
    /// The area of emission is a circle that starts at a radius of 0 and grows to the range of the square-shaped hitbox.
    /// The particles themselves will move in the specified direction.
    /// Often used to simulate confetti-like objects that float downwards.
    /// </summary>
    [Serializable]
    public class CircleSquareSprinkleEmitter : CircleSquareEmitter
    {
        public override bool Finished { get { return (CurrentRadius /*- LagRange*/ >= Range); } }

        public CircleSquareSprinkleEmitter()
        {
            Anims = new List<IParticleEmittable>();
            Layer = DrawLayer.Normal;
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
            HeightSpeed = other.HeightSpeed;
            SpeedDiff = other.SpeedDiff;
            StartHeight = other.StartHeight;
            Layer = other.Layer;
        }

        public override BaseEmitter Clone() { return new CircleSquareSprinkleEmitter(this); }

        /// <summary>
        /// The particles to emit.
        /// </summary>
        public List<IParticleEmittable> Anims;

        /// <summary>
        /// The amount of particles to emit per tile covered by the hitbox.
        /// </summary>
        public double ParticlesPerTile;

        /// <summary>
        /// The speed for the particles to travel in height.
        /// In pixels per second.
        /// </summary>
        public int HeightSpeed;

        /// <summary>
        /// How far left or right the particle's trajectory will drift as it moves.
        /// In pixels per second.
        /// </summary>
        public int SpeedDiff;

        /// <summary>
        /// The amount of height to add to the particles when they initially spawn.
        /// </summary>
        public int StartHeight;

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
                        scene.Anims[(int)Layer].Add(chosenAnim.CreateParticle(Origin + startDelta, randDiff, Loc.Zero, StartHeight, HeightSpeed, 0, Dir));
                    }
                }
            }
        }
    }
}