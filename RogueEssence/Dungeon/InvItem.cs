using System;
using RogueEssence.Data;
using RogueElements;

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

        public bool Cursed;
        public int HiddenValue;

        public InvItem() : base()
        { }

        public InvItem(int index)
        {
            ID = index;
        }

        public InvItem(int index, bool cursed)
        {
            ID = index;
            Cursed = cursed;
        }
        public InvItem(int index, bool cursed, int hiddenValue)
        {
            ID = index;
            Cursed = cursed;
            HiddenValue = hiddenValue;
        }
        public InvItem(InvItem other) : base(other)
        {
            Cursed = other.Cursed;
            HiddenValue = other.HiddenValue;
        }
        public ISpawnable Copy() { return new InvItem(this); }

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
                nameStr += " (" + HiddenValue + ")";

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
                nameStr += " (" + HiddenValue + ")";

            return nameStr;
        }

        public int GetSellValue()
        {
            ItemData entry = Data.DataManager.Instance.GetItem(ID);
            if (entry.MaxStack > 1)
                return entry.Price * HiddenValue;
            else
                return entry.Price;
        }
    }
}
