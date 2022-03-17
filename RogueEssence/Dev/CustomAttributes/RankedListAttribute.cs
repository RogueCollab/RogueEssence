using System;

namespace RogueEssence.Dev
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class RankedListAttribute : PassableAttribute
    {
        public readonly bool Index1;
        public RankedListAttribute(int flags, bool index1) : base(flags)
        {
            Index1 = index1;
        }
    }
}
