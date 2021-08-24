using System;

namespace RogueEssence.Dev
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class RangeBorderAttribute : PassableAttribute
    {
        public readonly bool Index1;
        public readonly bool Inclusive;
        public RangeBorderAttribute(int flags, bool index1, bool inclusive) : base(flags)
        {
            Index1 = index1;
            Inclusive = inclusive;
        }

        public void GetAddVals(out int addMin, out int addMax)
        {
            addMin = 0;
            addMax = 0;
            if (Index1)
            {
                addMin += 1;
                addMax += 1;
            }
            if (Inclusive)
                addMax -= 1;
        }
    }
}
