using System;

namespace RogueEssence.Dev
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class SubGroupAttribute : Attribute
    {
        public SubGroupAttribute() { }
    }
}
