using System;

namespace RogueEssence.Dev
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class DataTypeAttribute : PassableAttribute
    {
        public readonly Data.DataManager.DataType DataType;
        public readonly bool IncludeInvalid;
 
        public DataTypeAttribute(int flags, Data.DataManager.DataType dataType, bool includeInvalid) : base(flags)
        {
            DataType = dataType;
            IncludeInvalid = includeInvalid;
        }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class DataFolderAttribute : PassableAttribute
    {
        public readonly string FolderPath;

        public DataFolderAttribute(int flags, string folder) : base(flags)
        {
            FolderPath = folder;
        }
    }
}
