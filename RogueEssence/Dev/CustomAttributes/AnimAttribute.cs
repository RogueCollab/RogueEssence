using System;

namespace RogueEssence.Dev
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class AnimAttribute : PassableAttribute
    {
        public readonly string FolderPath;

        public AnimAttribute(int flags, string folder) : base(flags)
        {
            FolderPath = folder;
        }
    }
}
