using RogueEssence.Dungeon;

namespace RogueEssence.Ground
{
    public class GroundContext : GameContext
    {
        public GroundChar Owner;

        public InvItem Item;

        public GroundContext(InvItem item, GroundChar owner, Character target)
        {
            Item = item;
            Owner = owner;
            User = target;
        }

    }
}