using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueEssence.Dev
{
    public class PartialType
    {
        public Type BaseType;
        public Type[] GenericArgs;

        public PartialType(Type baseType, params Type[] genericArgs)
        {
            BaseType = baseType;
            GenericArgs = genericArgs;
        }
    }
}
