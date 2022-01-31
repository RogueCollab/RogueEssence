using System;

namespace RogueEssence
{
    [Serializable]
    public struct FrameTick
    {
        const int FRAME_TICKS_PER_FRAME = 120;
        public long Ticks;
        
        public FrameTick(long ticks)
        {
            Ticks = ticks;
        }

        public static FrameTick FromFrames(long frames)
        {
            return new FrameTick(frames * FRAME_TICKS_PER_FRAME);
        }

        public static FrameTick Zero
        {
            get { return new FrameTick(); }
        }

        public int ToFrames()
        {
            return (int)(Ticks / FRAME_TICKS_PER_FRAME);
        }
        public static ulong TickToFrames(ulong ticks)
        {
            return (ticks / FRAME_TICKS_PER_FRAME);
        }

        public override bool Equals(object obj)
        {
            return (obj is FrameTick) && Equals((FrameTick)obj);
        }

        public bool Equals(FrameTick other)
        {
            return Ticks == other.Ticks;
        }

        public override int GetHashCode()
        {
            return Ticks.GetHashCode();
        }


        public static bool operator >(FrameTick value1, FrameTick value2)
        {
            return (value1.Ticks > value2.Ticks);
        }

        public static bool operator >=(FrameTick value1, FrameTick value2)
        {
            return (value1.Ticks >= value2.Ticks);
        }

        public static bool operator <(FrameTick value1, FrameTick value2)
        {
            return (value1.Ticks < value2.Ticks);
        }

        public static bool operator <=(FrameTick value1, FrameTick value2)
        {
            return (value1.Ticks <= value2.Ticks);
        }

        public static bool operator ==(FrameTick value1, FrameTick value2)
        {
            return value1.Equals(value2);
        }

        public static bool operator !=(FrameTick value1, FrameTick value2)
        {
            return !(value1 == value2);
        }

        public static FrameTick operator +(FrameTick value1, FrameTick value2)
        {
            return new FrameTick(value1.Ticks + value2.Ticks);
        }

        public static FrameTick operator -(FrameTick value1, FrameTick value2)
        {
            return new FrameTick(value1.Ticks - value2.Ticks);
        }

        public static FrameTick operator *(FrameTick value1, FrameTick value2)
        {
            return new FrameTick(value1.Ticks * value2.Ticks);
        }

        public static FrameTick operator /(FrameTick value1, FrameTick value2)
        {
            return new FrameTick(value1.Ticks / value2.Ticks);
        }

        public static FrameTick operator %(FrameTick value1, FrameTick value2)
        {
            return new FrameTick(value1.Ticks % value2.Ticks);
        }



        public static FrameTick operator +(FrameTick value1, long value2)
        {
            return new FrameTick(value1.Ticks + FrameToTick(value2));
        }

        public static FrameTick operator -(FrameTick value1, long value2)
        {
            return new FrameTick(value1.Ticks - FrameToTick(value2));
        }

        public static FrameTick operator *(FrameTick value1, long value2)
        {
            return new FrameTick(value1.Ticks * value2);
        }

        public static FrameTick operator /(FrameTick value1, long value2)
        {
            return new FrameTick(value1.Ticks / value2);
        }

        public static FrameTick operator %(FrameTick value1, long value2)
        {
            return new FrameTick(value1.Ticks % FrameToTick(value2));
        }


        public static long FrameToTick(long frames)
        {
            return frames * FRAME_TICKS_PER_FRAME;
        }

        public int DivOf(long time2)
        {
            return (int)(Ticks / FrameToTick(time2));
        }

        public float FractionOf(int time2)
        {
            return (float)Ticks / FrameToTick(time2);
        }

        public long FractionOf(long frac, long time2)
        {
            return Ticks * frac / FrameToTick(time2);
        }

        public long FractionOf(long frac, FrameTick time2)
        {
            return Ticks * frac / time2.Ticks;
        }


        public static bool operator >(FrameTick value1, long value2)
        {
            return (value1.Ticks > FrameToTick(value2));
        }

        public static bool operator >=(FrameTick value1, long value2)
        {
            return (value1.Ticks >= FrameToTick(value2));
        }

        public static bool operator <(FrameTick value1, long value2)
        {
            return (value1.Ticks < FrameToTick(value2));
        }

        public static bool operator <=(FrameTick value1, long value2)
        {
            return (value1.Ticks <= FrameToTick(value2));
        }

        public static bool operator ==(FrameTick value1, long value2)
        {
            return (value1.Ticks == FrameToTick(value2));
        }

        public static bool operator !=(FrameTick value1, long value2)
        {
            return !(value1 == value2);
        }
    }
}
