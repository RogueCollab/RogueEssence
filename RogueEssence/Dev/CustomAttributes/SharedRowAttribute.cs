using System;

namespace RogueEssence.Dev
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class SharedRowAttribute : Attribute
    {
        public SharedRowAttribute() { }
    }
}
