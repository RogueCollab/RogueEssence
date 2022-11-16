namespace RogueEssence.Dungeon
{
    public class ItemCheckContext : GameContext
    {
        public MapItem PrevItem;
        public MapItem Item;

        public ItemCheckContext(Character user, MapItem item, MapItem prevItem) : base()
        {
            User = user;
            Item = item;
            PrevItem = prevItem;
        }

    }
}
