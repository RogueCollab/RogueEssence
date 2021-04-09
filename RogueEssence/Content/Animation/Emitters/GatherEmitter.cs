using System;
using System.Collections.Generic;
using RogueElements;

namespace RogueEssence.Content
{
    [Serializable]
    public class FiniteGatherEmitter : FiniteEmitter
    {
        public override bool Finished { get { return (CurrentBursts >= Math.Max(1, Bursts)); } }

        public FiniteGatherEmitter()
        {
            Anims = new List<AnimData>();
            Layer = DrawLayer.Normal;
        }
        public FiniteGatherEmitter(params AnimData[] anims) : this()
        {
            Anims.AddRange(anims);
        }

        public FiniteGatherEmitter(FiniteGatherEmitter other)
        {
            Anims = new List<AnimData>();
            foreach (AnimData anim in other.Anims)
                Anims.Add(new AnimData(anim));
            Bursts = other.Bursts;
            ParticlesPerBurst = other.ParticlesPerBurst;
            BurstTime = other.BurstTime;
            StartDistance = other.StartDistance;
            EndDistance = other.EndDistance;
            StartVariance = other.StartVariance;
            TravelTime = other.TravelTime;
            UseDest = other.UseDest;
            Layer = other.Layer;
            Cycles = other.Cycles;
        }

        public override BaseEmitter Clone() { return new FiniteGatherEmitter(this); }

        public List<AnimData> Anims;
        public bool UseDest;
        public int TravelTime;
        public int Bursts;
        public int ParticlesPerBurst;
        public int BurstTime;
        public int StartDistance;
        public int EndDistance;
        public int StartVariance;
        public DrawLayer Layer;
        public int Cycles;

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
                    if (Anims.Count > 0)
                    {
                        List<int> openDirs = getOpenDirs();
                        int openIndex = openDirs[MathUtils.Rand.Next(openDirs.Count)];
                        Coverages[openIndex] = true;

                        double angle = (openIndex + MathUtils.Rand.NextDouble()) * Math.PI / 4;

                        int dist = StartDistance + MathUtils.Rand.Next(StartVariance + 1);
                        Loc startDelta = new Loc((int)Math.Round(Math.Cos(angle) * dist), (int)Math.Round(Math.Sin(angle) * dist));

                        double endAngle = MathUtils.Rand.NextDouble() * Math.PI * 2;
                        int endDist = MathUtils.Rand.Next(EndDistance + 1);
                        Loc endDelta = new Loc((int)Math.Round(Math.Cos(endAngle) * endDist), (int)Math.Round(Math.Sin(endAngle) * endDist));

                        Loc particleSpeed = ((UseDest ? Destination : Origin) + endDelta - (Origin + startDelta)) * GraphicsManager.MAX_FPS / TravelTime;

                        Loc startLoc = Origin + startDelta;
                        {
                            AnimData animData = Anims[MathUtils.Rand.Next(Anims.Count)];
                            AnimData scaledAnim = new AnimData(animData);
                            DirSheet fxSheet = GraphicsManager.GetAttackSheet(animData.AnimIndex);
                            scaledAnim.FrameTime = (int)Math.Round((float)TravelTime / scaledAnim.GetTotalFrames(fxSheet.TotalFrames) / Math.Max(1, Cycles));
                            scaledAnim.FrameTime = Math.Max(1, scaledAnim.FrameTime);
                            ParticleAnim anim = new ParticleAnim(scaledAnim, 0, TravelTime);
                            anim.SetupEmitted(startLoc, particleSpeed, Loc.Zero, 0, 0, 0, particleSpeed.ApproximateDir8());
                            scene.Anims[(int)Layer].Add(anim);
                        }
                    }
                }
                CurrentBursts++;

                if (CurrentBursts >= Math.Max(1, Bursts))
                    break;
            }

        }

    }
}