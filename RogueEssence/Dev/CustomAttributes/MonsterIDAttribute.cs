using System;

namespace RogueEssence.Dev
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class MonsterIDAttribute : PassableAttribute
    {
        public readonly bool InvalidSpecies;
        public readonly bool InvalidForm;
        public readonly bool InvalidSkin;
        public readonly bool InvalidGender;

        public MonsterIDAttribute(int flags, bool invalidSpecies, bool invalidForm, bool invalidSkin, bool invalidGender) : base(flags)
        {
            InvalidSpecies = invalidSpecies;
            InvalidForm = invalidForm;
            InvalidSkin = invalidSkin;
            InvalidGender = invalidGender;
        }
    }
}
