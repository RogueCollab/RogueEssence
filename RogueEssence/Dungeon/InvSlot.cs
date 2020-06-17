namespace RogueEssence.Dungeon
{
    public struct InvSlot
    {
        public int Slot;
        public bool IsEquipped;
        
        public InvSlot(bool isEquipped, int slot)
        {
            IsEquipped = isEquipped;
            Slot = slot;
        }




        private static readonly InvSlot invalid = new InvSlot(false, -1);

        public static InvSlot Invalid { get { return invalid; } }

        public bool IsValid()
        {
            return (Slot > -1);
        }
    }
}