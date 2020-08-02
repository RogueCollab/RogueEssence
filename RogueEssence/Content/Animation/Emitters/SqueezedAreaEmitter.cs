using System;
using System.Collections.Generic;
using RogueElements;

namespace RogueEssence.Content
{

    [Serializable]
    public class SqueezedAreaEmitter : FiniteEmitter
    {
        public override bool Finished { get { return (CurrentBursts >= Bursts); } }

        public SqueezedAreaEmitter()
        {
            Anims = new List<IParticleEmittable>();
        }
        public SqueezedAreaEmitter(params AnimData[] anims) : this()
        {
            foreach (AnimData anim in anims)
                Anims.Add(new ParticleAnim(anim));
        }
        public SqueezedAreaEmitter(params ParticleAnim[] anims) : this()
        {
            foreach (ParticleAnim anim in anims)
                Anims.Add(anim);
        }


        public SqueezedAreaEmitter(SqueezedAreaEmitter other)
        {
            Anims = new List<IParticleEmittable>();
            foreach (IParticleEmittable anim in other.Anims)
                Anims.Add((IParticleEmittable)anim.CloneIEmittable());
            Bursts = other.Bursts;
            ParticlesPerBurst = other.ParticlesPerBurst;
            BurstTime = other.BurstTime;
            Range = other.Range;
            AnimDir = other.AnimDir;
            HeightSpeed = other.HeightSpeed;
            SpeedDiff = other.SpeedDiff;
            StartHeight = other.StartHeight;
            HeightDiff = other.HeightDiff;
        }

        public override BaseEmitter Clone() { return new SqueezedAreaEmitter(this); }

        public List<IParticleEmittable> Anims;
        public int Bursts;
        public int ParticlesPerBurst;
        public int BurstTime;
        public int Range;
        public Dir8 AnimDir;
        public int HeightSpeed;
        public int SpeedDiff;
        public int StartHeight;
        public int HeightDiff;

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

                    Loc startDelta = new Loc((int)Math.Round(Math.Cos(angle) * Range), (int)Math.Round(Math.Sin(angle) * Range) / 3);

                    if (Anims.Count > 0)
                    {
                        Loc randDiff = new Loc((int)((MathUtils.Rand.NextDouble() * 2 - 1) * SpeedDiff), 0);
                        int heightDiff = (int)((MathUtils.Rand.NextDouble() * 2 - 1) * HeightDiff);

                        IParticleEmittable chosenAnim = Anims[MathUtils.Rand.Next(Anims.Count)];
                        scene.Anims[(int)DrawLayer.Normal].Add(chosenAnim.CreateParticle(Origin + startDelta, randDiff, Loc.Zero, StartHeight + heightDiff, HeightSpeed, 0, AnimDir));
                    }
                }
                CurrentBursts++;
                if (CurrentBursts >= Bursts)
                    break;
            }

        }
    }
}