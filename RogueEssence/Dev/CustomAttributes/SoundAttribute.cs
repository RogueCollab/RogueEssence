using System;

namespace RogueEssence.Dev
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class SoundAttribute : PassableAttribute
    {
        public SoundAttribute(int flags) : base(flags) { }
    }
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class MusicAttribute : PassableAttribute
    {
        public MusicAttribute(int flags) : base(flags) { }
    }
}
