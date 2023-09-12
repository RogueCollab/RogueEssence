using System;

namespace RogueEssence.Dev
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class NumberRangeAttribute : PassableAttribute
    {
        public readonly int Min;
        public readonly int Max;
 
        public NumberRangeAttribute(int flags, int min, int max) : base(flags)
        {
            Min = min;
            Max = max;
        }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class IntRangeAttribute : PassableAttribute
    {
        public readonly bool Index1;

        public IntRangeAttribute(int flags, bool index1) : base(flags)
        {
            Index1 = index1;
        }
    }
}
