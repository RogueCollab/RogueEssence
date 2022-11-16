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
            ItemData entry = DataManager.Instance.GetItem(ID);

            string prefix = "";
            if (entry.Icon > -1)
                prefix += ((char)(entry.Icon + 0xE0A0)).ToString();
            if (Cursed)
                prefix += "\uE10B";

            string nameStr = entry.Name.ToLocal();
            if (entry.MaxStack > 1)
                nameStr += " (" + Amount + ")";

            return String.Format("{0}[color=#FFCEFF]{1}[color]", prefix, nameStr);
        }

        public override string ToString()
        {
            ItemData entry = DataManager.Instance.GetItem(ID);

            string nameStr = "";
            if (Cursed)
                nameStr += "[X]";

            nameStr += entry.Name.ToLocal();
            if (entry.MaxStack > 1)
                nameStr += " (" + Amount + ")";

            return nameStr;
        }

        public int GetSellValue()
        {
            ItemData entry = DataManager.Instance.GetItem(ID);
            if (entry.MaxStack > 1)
                return entry.Price * Amount;
            else
                return entry.Price;
        }

        //TODO: Created v0.5.20, delete on v1.1
        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            if (!String.IsNullOrEmpty(ID))
            {
                ItemData item = DataManager.Instance.GetItem(ID);

                int amt;
                if (int.TryParse(HiddenValue, out amt))
                {
                    if (item.MaxStack > 0)
                    {
                        Amount = amt;
                        HiddenValue = "";
                    }
                    else if (amt > 0)
                    {
                        string asset_name = DataManager.Instance.MapAssetName(DataManager.DataType.Item, amt);
                        HiddenValue = asset_name;
                    }
                    else
                    {
                        HiddenValue = "";
                    }
                }
            }
        }
    }
}
