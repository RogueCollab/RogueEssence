using System;
using System.Collections.Generic;

namespace RogueEssence.Content
{

    [Serializable]
    public class SpinEmitter : FiniteEmitter
    {
        public override bool Finished { get { return (CurrentBursts >= Bursts); } }

        public SpinEmitter()
        {
            Anims = new List<AnimData>();
        }

        public SpinEmitter(SpinEmitter other)
        {
            Anims = new List<AnimData>();
            foreach (AnimData anim in other.Anims)
                Anims.Add(new AnimData(anim));
            Bursts = other.Bursts;
            BurstTime = other.BurstTime;
            Range = other.Range;
            StartHeight = other.StartHeight;
            HeightSpeed = other.HeightSpeed;
            CycleSpeed = other.CycleSpeed;
            EndHeight = other.EndHeight;
        }

        public override BaseEmitter Clone() { return new SpinEmitter(this); }

        public List<AnimData> Anims;
        public int Bursts;
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
            while (CurrentBurstTime >= BurstTime)
            {
                CurrentBurstTime -= BurstTime;
                for (int ii = 0; ii < Anims.Count; ii++)
                {
                    int angle = (int)Math.Round((double)360 / Anims.Count * ii);

                    int diff = EndHeight - StartHeight;
                    int lifeTime = HeightSpeed > 0 ? Math.Abs(diff * GraphicsManager.MAX_FPS / HeightSpeed) : 0;

                    if (Anims[ii].AnimIndex != "")
                        scene.Anims[(int)DrawLayer.Normal].Add(new HelixAnim(Anims[ii], lifeTime, Origin, Range, angle, CycleSpeed, StartHeight, (diff > 0 ? 1 : -1) * HeightSpeed));
                }
                CurrentBursts++;
                if (CurrentBursts >= Bursts)
                    break;
            }

        }
    }
}