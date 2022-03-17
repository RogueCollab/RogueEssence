using System;

namespace RogueEssence.Dev
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class EditorHeightAttribute : PassableAttribute
    {
        public readonly int Height;
        public EditorHeightAttribute(int flags, int height) : base(flags)
        {
            Height = height;
        }
    }
}
