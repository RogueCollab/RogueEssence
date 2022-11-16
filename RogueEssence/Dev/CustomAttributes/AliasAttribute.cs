using System;

namespace RogueEssence.Dev
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class AliasAttribute : PassableAttribute
    {
        public readonly string Name;
 
        public AliasAttribute(int flags, string name) : base(flags)
        {
            Name = name;
        }
    }
}
