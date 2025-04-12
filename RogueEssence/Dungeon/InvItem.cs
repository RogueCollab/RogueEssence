using System;
using RogueEssence.Data;
using RogueElements;
using RogueEssence.Dev;
using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace RogueEssence.Dungeon
{
    [Serializable]
    public class InvItem : PassiveActive, ISpawnable
    {
        public override GameEventPriority.EventCause GetEventCause()
        {
            return GameEventPriority.EventCause.Equip;
        }
        public override PassiveData GetData() { return DataManager.Instance.GetItem(ID); }

        public override string GetID() { return ID.ToString(); }

        [JsonConverter(typeof(ItemConverter))]
        [DataType(0, DataManager.DataType.Item, false)]
        public string ID { get; set; }
        public bool Cursed;

        public string HiddenValue;
        public int Amount;
        public int Price;

        public InvItem() : base()
        {
            ID = "";
            HiddenValue = "";
        }

        public InvItem(string index)
        {
            ID = index;
            HiddenValue = "";
        }

        public InvItem(string index, bool cursed)
        {
            ID = index;
            HiddenValue = "";
            Cursed = cursed;
        }

        public InvItem(string index, bool cursed, int amount)
        {
            ID = index;
            Cursed = cursed;
            HiddenValue = "";
            Amount = amount;
        }

        public InvItem(string index, bool cursed, int amount, int price)
        {
            ID = index;
            Cursed = cursed;
            HiddenValue = "";
            Amount = amount;
            Price = price;
        }

        //TODO: Created v0.5.20, revert on v1.1
        public InvItem(InvItem other)// : base(other)
        {
            //TODO: Created v0.5.20, revert on v1.1
            ID = other.ID;
            Cursed = other.Cursed;
            HiddenValue = other.HiddenValue;
            Amount = other.Amount;
            Price = other.Price;
        }
        public ISpawnable Copy() { return new InvItem(this); }


        public static InvItem CreateBox(string value, string hiddenValue, int price = 0, bool cursed = false)
        {
            InvItem item = new InvItem();
            item.ID = value;
            item.HiddenValue = hiddenValue;
            item.Cursed = cursed;
            item.Price = price;
            return item;
        }

        public string GetPriceString()
        {
            return MapItem.GetPriceString(Price);
        }

        public override string GetDisplayName()
        {
            EntryDataIndex idx = DataManager.Instance.DataIndices[DataManager.DataType.Item];
            if (!idx.ContainsKey(ID))
                return String.Format("[color=#FF0000]{0}[color]", ID);

            ItemEntrySummary summary = (ItemEntrySummary)idx.Get(ID);

            string prefix = "";
            if (summary.Icon > -1)
                prefix += ((char)(summary.Icon + 0xE0A0)).ToString();
            if (Cursed)
                prefix += "\uE10B";

            string nameStr = summary.Name.ToLocal();
            if (summary.MaxStack > 1)
                nameStr += " (" + Amount + ")";

            if (summary.UsageType == ItemData.UseType.Treasure)
                return String.Format("{0}[color=#6384E6]{1}[color]", prefix, nameStr);
            else
                return String.Format("{0}[color=#FFCEFF]{1}[color]", prefix, nameStr);
        }

        public override string ToString()
        {
            string nameStr = "";
            if (Price > 0)
                nameStr += String.Format("${0} ", Price);
            if (Cursed)
                nameStr += "[X]";

            nameStr += ID;
            if (Amount > 0)
                nameStr += String.Format("({0})", Amount);

            if (!String.IsNullOrEmpty(HiddenValue))
                nameStr += String.Format("[{0}]", HiddenValue);

            return nameStr;
        }

        public int GetSellValue()
        {
            ItemData entry = DataManager.Instance.GetItem(ID);
            if (entry == null)
                return 0;

            if (entry.MaxStack > 1)
                return entry.Price * Amount;
            else
                return entry.Price;
        }
    }
}
