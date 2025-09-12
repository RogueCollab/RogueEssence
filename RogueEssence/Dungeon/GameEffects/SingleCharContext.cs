namespace RogueEssence.Dungeon
{
    public class SingleCharContext : GameContext
    {
        public bool InCombat;
        public AbortStatus TurnCancel;

        public SingleCharContext(Character user, bool inCombat = false) : base()
        {
            User = user;
            InCombat = inCombat;
            TurnCancel = new AbortStatus();
        }

    }
}
