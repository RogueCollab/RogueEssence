using System;

namespace RogueEssence.Dev
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class SanitizeAttribute : PassableAttribute
    {
        public SanitizeAttribute(int flags) : base(flags) { }
    }
}
