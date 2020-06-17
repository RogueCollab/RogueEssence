using System;

namespace RogueEssence.Dev
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class NonNullAttribute : PassableAttribute
    {
        public NonNullAttribute(int flags) : base(flags) { }
    }
}
