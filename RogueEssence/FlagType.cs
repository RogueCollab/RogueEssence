using System;
using System.Runtime.Serialization;

namespace RogueEssence
{

    [Serializable]
    public struct FlagType
    {
        public string Assembly;
        public string Type;

        public FlagType(Type type)
        {
            Assembly = type.Assembly.FullName;
            Type = type.FullName;
        }


        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            if (Type != null)
            {
                Type type = System.Type.GetType(String.Format("{0}, {1}", Type, Assembly));
                if (type == null)
                {
                    throw new Exception();
                    //string newType = typeof(int).FullName;
                    //string newAssembly = typeof(int).Assembly.FullName;
                    ////then the type moved to a new namespace
                    //typeToDeserialize = Type.GetType(String.Format("{0}, {1}", newType, newAssembly));
                }
            }
        }

        public override string ToString()
        {
            return Type;
        }

        public string AssemblyQualifiedName => String.Format("{0}, {1}", Type, Assembly);

        public override int GetHashCode()
        {
            return (Assembly == null ? 0 : Assembly.GetHashCode()) ^ (Type == null ? 0 : Type.GetHashCode());
        }
    }
}
