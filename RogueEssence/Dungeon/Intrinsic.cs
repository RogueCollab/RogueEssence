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
        public override string GetName() { return DataManager.Instance.GetIntrinsic(ID).Name.ToLocal(); }

        public Intrinsic() : base()
        { }

        public Intrinsic(int index)
        {
            ID = index;
        }
        
    }
}
