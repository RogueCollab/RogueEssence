using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RogueEssence.Dev
{
    public class PartialType
    {
        public Type Type;
        public Assembly[] SearchAssemblies;
        public Type[] GenericArgs;

        public PartialType(Type baseType, Assembly[] searchAssemblies, params Type[] genericArgs)
        {
            Type = baseType;
            SearchAssemblies = searchAssemblies;
            GenericArgs = genericArgs;
        }

        public override string ToString()
        {
            return Type.ToString();
        }
    }
}
