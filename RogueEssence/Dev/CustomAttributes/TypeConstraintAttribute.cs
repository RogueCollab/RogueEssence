using System;

namespace RogueEssence.Dev
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class TypeConstraintAttribute : PassableAttribute
    {
        public readonly Type BaseClass;
        public TypeConstraintAttribute(int flags, Type type) : base(flags)
        {
            BaseClass = type;
        }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class StringTypeConstraintAttribute : PassableAttribute
    {
        public readonly Type BaseClass;
        public StringTypeConstraintAttribute(int flags, Type type) : base(flags)
        {
            BaseClass = type;
        }
    }
}
