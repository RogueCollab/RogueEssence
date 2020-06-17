using System;
using RogueElements;

namespace RogueEssence.Content
{
    [Serializable]
    public class ScreenMover
    {
        public bool Finished { get { return (ShakeTime.ToFrames() >= MaxShakeTime); } }

        public int MinShake;
        public int MaxShake;
        public int MaxShakeTime;

        [NonSerialized]
        public FrameTick ShakeTime;

        public ScreenMover()
        {

        }
        public ScreenMover(int minShake, int maxShake, int shakeTime)
        {
            MinShake = minShake;
            MaxShake = maxShake;
            MaxShakeTime = shakeTime;
        }
        public ScreenMover(ScreenMover other)
        {
            MaxShake = other.MaxShake;
            MinShake = other.MinShake;
            MaxShakeTime = other.MaxShakeTime;
        }

        public void Update(FrameTick elapsedTime, ref Loc offsetLoc)
        {
            ShakeTime += elapsedTime;
            if (ShakeTime < MaxShakeTime)
            {
                int divShake = MinShake + (int)ShakeTime.FractionOf(MaxShake, MaxShakeTime);
                offsetLoc += new Loc(MathUtils.Rand.Next(divShake), MathUtils.Rand.Next(divShake));
            }
            //else
            //    offsetLoc = new Loc();
        }
    }
}
