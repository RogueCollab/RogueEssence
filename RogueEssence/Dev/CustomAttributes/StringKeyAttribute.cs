using System;

namespace RogueEssence.Dev
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class StringKeyAttribute : PassableAttribute
    {
        public readonly bool IncludeInvalid;
 
        public StringKeyAttribute(int flags, bool includeInvalid) : base(flags)
        {
            IncludeInvalid = includeInvalid;
        }
    }

}
