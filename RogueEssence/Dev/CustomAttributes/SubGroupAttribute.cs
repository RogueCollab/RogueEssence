using System;

namespace RogueEssence.Dev
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class SubGroupAttribute : Attribute
    {
        public SubGroupAttribute() { }
    }
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class SepGroupAttribute : Attribute
    {
        public SepGroupAttribute() { }
    }
}
