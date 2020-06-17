using System;

namespace RogueEssence.Dev
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class FrameTypeAttribute : PassableAttribute
    {
        public readonly bool DashOnly;
 
        public FrameTypeAttribute(int flags, bool dashOnly) : base(flags)
        {
            DashOnly = dashOnly;
        }
    }
}
