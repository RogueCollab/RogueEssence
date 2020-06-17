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
}
