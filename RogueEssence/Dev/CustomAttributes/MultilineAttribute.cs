using System;

namespace RogueEssence.Dev
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class MultilineAttribute : PassableAttribute
    {
        public MultilineAttribute(int flags) : base(flags) { }
    }
}
