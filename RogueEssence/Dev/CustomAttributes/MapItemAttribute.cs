using System;

namespace RogueEssence.Dev
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class MapItemAttribute : PassableAttribute
    {
        public readonly bool IncludePrice;

        public MapItemAttribute(int flags, bool price) : base(flags)
        {
            IncludePrice = price;
        }
    }
}
