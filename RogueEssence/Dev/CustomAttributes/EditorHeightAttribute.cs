using System;

namespace RogueEssence.Dev
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class EditorHeightAttribute : PassableAttribute
    {
        public readonly int Height;
        public EditorHeightAttribute(int flags, int height) : base(flags)
        {
            Height = height;
        }
    }
}
