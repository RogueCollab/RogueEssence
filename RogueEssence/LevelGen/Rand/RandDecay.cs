using RogueElements;
using System;
using System.Collections;
using System.Collections.Generic;

namespace RogueEssence.LevelGen
{
    /// <summary>
    /// Selects an integer in a predefined range.  Starts with minimum and continually rolls until a failure.  Chance of higher numbers decays exponentially.
    /// </summary>
    [Serializable]
    public struct RandDecay : IRandPicker<int>, IEquatable<RandDecay>
    {
        public int Min;
        public int Max;
        public int Rate;

        public RandDecay(int num)
        {
            this.Min = num;
            this.Max = num;
            this.Rate = 0;
        }

        public RandDecay(int min, int max, int rate)
        {
            this.Min = min;
            this.Max = max;
            this.Rate = rate;
        }

        public RandDecay(RandDecay other)
        {
            this.Min = other.Min;
            this.Max = other.Max;
            this.Rate = other.Rate;
        }

        public static RandDecay Empty => new RandDecay(0);

        public bool ChangesState => false;

        public bool CanPick => this.Min <= this.Max;

        public static bool operator ==(RandDecay lhs, RandDecay rhs) => lhs.Equals(rhs);

        public static bool operator !=(RandDecay lhs, RandDecay rhs) => !lhs.Equals(rhs);

        public IRandPicker<int> CopyState() => new RandDecay(this);

        public IEnumerable<int> EnumerateOutcomes()
        {
            yield return this.Min;
            for (int ii = this.Min + 1; ii < this.Max; ii++)
                yield return ii;
        }

        public int Pick(IRandom rand)
        {
            int cur = this.Min;
            while (cur < this.Max)
            {
                if (rand.Next(100) < this.Rate)
                    cur++;
                else
                    break;
            }
            return cur;
        }

        public bool Equals(RandDecay other) => this.Min == other.Min && this.Max == other.Max && this.Rate == other.Rate;

        public override bool Equals(object obj) => (obj is RandDecay) && this.Equals((RandDecay)obj);

        public override int GetHashCode() => unchecked(191 + (this.Min.GetHashCode() * 313) ^ (this.Max.GetHashCode() * 739));

        public override string ToString()
        {
            return string.Format("{0}+{1}%^{2}", this.Min, this.Rate, this.Max - this.Min);
        }
    }
}
