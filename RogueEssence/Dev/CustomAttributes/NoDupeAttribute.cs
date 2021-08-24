using System;

namespace RogueEssence.Dev
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class NoDupeAttribute : PassableAttribute
    {
        public NoDupeAttribute(int flags) : base(flags) { }
    }
}
