using System;
using Newtonsoft.Json;
using RogueEssence.Data;
using RogueEssence.Dev;

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

        public override string GetID() { return ID.ToString(); }

        [JsonConverter(typeof(IntrinsicConverter))]
        [DataType(0, DataManager.DataType.Intrinsic, false)]
        public string ID { get; set; }

        public Intrinsic() : base()
        {
            ID = "";
        }

        public Intrinsic(string index)
        {
            ID = index;
        }
        
    }
}
