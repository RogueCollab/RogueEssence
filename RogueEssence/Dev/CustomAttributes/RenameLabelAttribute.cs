using System;

namespace RogueEssence.Dev
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class RenameLabelAttribute : Attribute
    {
        public string Label { get; }

        public RenameLabelAttribute(string label)
        {
            Label = label;
        }
    }
}