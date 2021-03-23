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

        private string assembly;
        private string type;

        public FlagType()
        {
            fullType = typeof(object);
            assembly = fullType.FullName;
            this.type = fullType.FullName;
        }
        public FlagType(Type type)
        {
            fullType = type;
            this.assembly = type.Assembly.FullName;
            this.type = type.FullName;
        }


        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            if (type != null)
            {
                fullType = Type.GetType(String.Format("{0}, {1}", type, assembly));
                if (fullType == null)
                {
                    fullType = DiagManager.Instance.UpgradeBinder?.BindToType(assembly, type);

                    if (fullType == null)
                        throw new TypeInitializationException(type, null);

                    assembly = fullType.Assembly.FullName;
                    type = fullType.FullName;
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
