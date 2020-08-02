using System;
using System.Collections.Generic;
using RogueElements;

namespace RogueEssence.Content
{

    [Serializable]
    public class WindEmitter : FiniteEmitter
    {
        public override bool Finished { get { return (CurrentBursts >= Math.Max(1, Bursts)); } }

        public WindEmitter()
        {
            Anims = new List<IParticleEmittable>();
            Layer = DrawLayer.Normal;
        }
        public WindEmitter(params AnimData[] anims) : this()
        {
            foreach (AnimData anim in anims)
                Anims.Add(new ParticleAnim(anim));
        }

        public WindEmitter(WindEmitter other)
        {
            Anims = new List<IParticleEmittable>();
            foreach (IParticleEmittable anim in other.Anims)
                Anims.Add((IParticleEmittable)anim.CloneIEmittable());
            Bursts = other.Bursts;
            ParticlesPerBurst = other.ParticlesPerBurst;
            BurstTime = other.BurstTime;
            StartDistance = other.StartDistance;
            Speed = other.Speed;
            SpeedDiff = other.SpeedDiff;
            Layer = other.Layer;
        }

        public override BaseEmitter Clone() { return new WindEmitter(this); }

        public List<IParticleEmittable> Anims;

        /// <summary>
        /// In Pixels Per Second
        /// </summary>
        public int Speed;

        public int SpeedDiff;
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
                    int particleSpeed = Speed + MathUtils.Rand.Next(SpeedDiff);
                    int startDist = MathUtils.Rand.Next(StartDistance);
                    Loc startDelta = new Loc(startDist + GraphicsManager.ScreenWidth / 2, MathUtils.Rand.Next(GraphicsManager.ScreenHeight) - GraphicsManager.ScreenHeight / 2) + Origin;

                    if (Anims.Count > 0)
                    {
                        int totalTime = GraphicsManager.ScreenWidth + startDist + GraphicsManager.TileSize * 2;
                        if (particleSpeed != 0)
                        {
                            totalTime *= GraphicsManager.MAX_FPS;
                            totalTime = (totalTime - 1) / -particleSpeed + 1;
                        }

                        IParticleEmittable chosenAnim = Anims[MathUtils.Rand.Next(Anims.Count)];
                        scene.Anims[(int)Layer].Add(chosenAnim.CreateParticle(totalTime, startDelta, new Loc(particleSpeed, 0), Loc.Zero, 0, 0, 0, Dir8.Left));
                    }
                }
                CurrentBursts++;

                if (CurrentBursts >= Math.Max(1, Bursts))
                    break;
            }

        }

    }
}