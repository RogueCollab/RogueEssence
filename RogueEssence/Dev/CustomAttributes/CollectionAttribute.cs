using System;

namespace RogueEssence.Dev
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class CollectionAttribute : PassableAttribute
    {
        public readonly bool ConfirmDelete;
        public CollectionAttribute(int flags, bool confirmDelete) : base(flags)
        {
            ConfirmDelete = confirmDelete;
        }
    }
}
