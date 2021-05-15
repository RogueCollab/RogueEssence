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
    }
}
