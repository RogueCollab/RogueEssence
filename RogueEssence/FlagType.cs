using System;
using System.Runtime.Serialization;

namespace RogueEssence
{

    [Serializable]
    public class FlagType
    {
        [NonSerialized]
        private Type fullType;

        public Type FullType => fullType;

        private readonly string assembly;
        private readonly string type;

        public FlagType()
        {
            fullType = typeof(object);
            assembly = fullType.FullName;
            this.type = fullType.FullName;
        }
        public FlagType(Type type)
        {
            fullType = type;
            assembly = type.Assembly.FullName;
            this.type = type.FullName;
        }


        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            if (type != null)
            {
                fullType = System.Type.GetType(String.Format("{0}, {1}", type, assembly));
                if (fullType == null)
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
            return type;
        }

        public override int GetHashCode()
        {
            return (assembly == null ? 0 : assembly.GetHashCode()) ^ (type == null ? 0 : type.GetHashCode());
        }
    }
}
