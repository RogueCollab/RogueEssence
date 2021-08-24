using System;
using RogueEssence.Dungeon;
using RogueElements;

namespace RogueEssence.Data
{
    [Serializable]
    public abstract class BaseData
    {
        public abstract string FileName { get; }
        public abstract DataManager.DataType TriggerType { get; }
        public abstract void ContentChanged(int idx);
        public abstract void ReIndex();
    }
}
