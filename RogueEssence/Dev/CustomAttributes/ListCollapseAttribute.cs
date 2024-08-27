using System;

namespace RogueEssence.Dev
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class ListCollapseAttribute : Attribute
    {
        public ListCollapseAttribute() { }
    }
}
