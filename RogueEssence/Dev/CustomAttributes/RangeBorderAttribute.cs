using System;

namespace RogueEssence.Dev
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class RangeBorderAttribute : PassableAttribute
    {
        /// <summary>
        /// The internal range value is 0-indexed, but the editor value should be 1-indexed.
        /// </summary>
        public readonly bool Index1;

        /// <summary>
        /// The internal range value's upper bound is exclusive, but the editor value should display it as inclusive.
        /// </summary>
        public readonly bool Inclusive;
        public RangeBorderAttribute(int flags, bool index1, bool inclusive) : base(flags)
        {
            Index1 = index1;
            Inclusive = inclusive;
        }

        public void GetAddVals(out int addMin, out int addMax)
        {
            GetAddVals(Index1, Inclusive, out addMin, out addMax);
        }

        public static void GetAddVals(bool index1, bool inclusive, out int addMin, out int addMax)
        {
            addMin = 0;
            addMax = 0;
            if (index1)
            {
                addMin += 1;
                addMax += 1;
            }
            if (inclusive)
                addMax -= 1;
        }
    }
}
