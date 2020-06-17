using System;

namespace RogueEssence.Dev
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class NonEditedAttribute : Attribute
    {
        public NonEditedAttribute() { }
    }
}
