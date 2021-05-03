using System;
using RogueEssence.Data;

namespace RogueEssence.Dungeon
{
    [Serializable]
    public class Intrinsic : PassiveActive
    {
        public override GameEventPriority.EventCause GetEventCause()
        {
            return GameEventPriority.EventCause.Intrinsic;
        }
        public override PassiveData GetData() { return DataManager.Instance.GetIntrinsic(ID); }
        public override string GetDisplayName() { return DataManager.Instance.GetIntrinsic(ID).GetColoredName(); }

        public Intrinsic() : base()
        { }

        public Intrinsic(int index)
        {
            ID = index;
        }
        
    }
}
