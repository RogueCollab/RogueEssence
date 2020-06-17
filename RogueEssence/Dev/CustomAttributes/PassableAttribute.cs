using System;

namespace RogueEssence.Dev
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public abstract class PassableAttribute : Attribute
    {
        public readonly int PassableArgFlag;

        public PassableAttribute(int flags)
        {
            PassableArgFlag = flags;
        }
    }
}
