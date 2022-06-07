using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.Xna.Framework;
using RogueElements;

namespace RogueEssence.Content
{
    /// <summary>
    /// Emits particles in a stream, traveling in the trajectory of the hitbox.
    /// </summary>
    [Serializable]
    public class StreamEmitter : ShootingEmitter
    {
        public override bool Finished { get { return (CurrentShots >= Shots); } }

        public StreamEmitter()
        {
            Anims = new List<IParticleEmittable>();
            Layer = DrawLayer.Normal;
        }
        public StreamEmitter(params AnimData[] anims) : this()
        {
            foreach (AnimData anim in anims)
                Anims.Add(new ParticleAnim(anim));
        }

        public StreamEmitter(StreamEmitter other)
        {
            Anims = new List<IParticleEmittable>();
            foreach (IParticleEmittable anim in other.Anims)
                Anims.Add((IParticleEmittable)anim.CloneIEmittable());
            Shots = other.Shots;
            BurstTime = other.BurstTime;
            StartDistance = other.StartDistance;
            EndDiff = other.EndDiff;
            Range = other.Range;
            Speed = other.Speed;
            LocHeight = other.LocHeight;
            Layer = other.Layer;
        }

        public override BaseEmitter Clone() { return new StreamEmitter(this); }

        /// <summary>
        /// Particles to emit.
        /// </summary>
        public List<IParticleEmittable> Anims;

        /// <summary>
        /// The number of times to repeat emitting.
        /// </summary>
        public int Shots;

        /// <summary>
        /// The number of frames between each shot.
        /// </summary>
        public int BurstTime;

        /// <summary>
        /// The starting distance of the emitted particle, in pixels from the origin.
        /// A positive number shifts the spawn to be further in the direction being fired.
        /// A negative number shifts it back.
        /// </summary>
        public int StartDistance;

        /// <summary>
        /// Modifies the end distance of the emitted particle, in pixels.
        /// A positive number shifts the spawn to be further in the direction being fired.
        /// A negative number shifts it back.
        /// </summary>
        public int EndDiff;

        /// <summary>
        /// The layer to spawn the particles in.
        /// </summary>
        public DrawLayer Layer;


        [NonSerialized]
        private FrameTick CurrentShotTime;
        [NonSerialized]
        private int CurrentShots;

        public override void Update(BaseScene scene, FrameTick elapsedTime)
        {
            CurrentShotTime += elapsedTime;
            while (CurrentShotTime >= BurstTime)
            {
                CurrentShotTime -= BurstTime;

                int range = Range;

                Vector2 totalDistance = (Dir.GetLoc() * (range - StartDistance)).ToVector2();

                double angle = MathUtils.Rand.NextDouble() * Math.PI * 2;
                int dist = MathUtils.Rand.Next(EndDiff + 1);
                Vector2 endDelta = new Vector2((int)Math.Round(Math.Cos(angle) * dist), (int)Math.Round(Math.Sin(angle) * dist));
                totalDistance += endDelta;

                //pixels
                float totalTime = range - StartDistance;
                //seconds
                if (Speed > 0)
                    totalTime /= Speed;

                //pixels
                Vector2 particleSpeed = totalDistance;
                //pixels per second
                if (totalTime > 0)
                    particleSpeed /= totalTime;

                Loc startDelta = Dir.GetLoc() * StartDistance;

                if (Anims.Count > 0)
                {
                    IParticleEmittable chosenAnim = Anims[CurrentShots % Anims.Count];
                    scene.Anims[(int)Layer].Add(chosenAnim.CreateParticle((int)Math.Round(totalTime * GraphicsManager.MAX_FPS), Origin + startDelta, particleSpeed.ToLoc(), Loc.Zero, LocHeight, 0, 0, Dir));
                }
                
                CurrentShots++;
                if (CurrentShots >= Shots)
                    break;
            }
        }
    }
}