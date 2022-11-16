using System;
using System.Collections.Generic;
using RogueElements;

namespace RogueEssence.Content
{
    /// <summary>
    /// Emits particles that spin around the origin in a changing radius.
    /// Not very used.
    /// </summary>
    [Serializable]
    public class VortexEmitter : FiniteEmitter
    {
        public override bool Finished { get { return (CurrentBursts >= Bursts); } }

        public VortexEmitter()
        {
            Anims = new List<AnimData>();
        }

        public VortexEmitter(VortexEmitter other)
        {
            Anims = new List<AnimData>();
            foreach (AnimData anim in other.Anims)
                Anims.Add(new AnimData(anim));
            Bursts = other.Bursts;
            ParticlesPerBurst = other.ParticlesPerBurst;
            BurstTime = other.BurstTime;
            Range = other.Range;
            StartHeight = other.StartHeight;
            HeightSpeed = other.HeightSpeed;
            CycleSpeed = other.CycleSpeed;
            EndHeight = other.EndHeight;
        }

        public override BaseEmitter Clone() { return new VortexEmitter(this); }

        public List<AnimData> Anims;
        public int Bursts;
        public int ParticlesPerBurst;
        public int BurstTime;
        public int Range;
        public int StartHeight;
        public int EndHeight;
        public int HeightSpeed;
        public int CycleSpeed;

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
                    int angle = MathUtils.Rand.Next(0, 361);

                    int diff = EndHeight - StartHeight;
                    int lifeTime = HeightSpeed > 0 ? Math.Abs(diff * GraphicsManager.MAX_FPS / HeightSpeed) : 0;

                    if (Anims.Count > 0)
                    {
                        AnimData anim = Anims[MathUtils.Rand.Next(Anims.Count)];
                        if (anim.AnimIndex != "")
                            scene.Anims[(int)DrawLayer.Normal].Add(new HelixAnim(anim, lifeTime, Origin, Range, angle, CycleSpeed, StartHeight, (diff > 0 ? 1 : -1) * HeightSpeed));
                    }
                }
                CurrentBursts++;
                if (CurrentBursts >= Bursts)
                    break;
            }

        }
    }
}