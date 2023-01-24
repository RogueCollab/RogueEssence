namespace RogueEssence.Dungeon
{
    public class SingleCharContext : GameContext
    {
        public AbortStatus TurnCancel;

        public SingleCharContext(Character user) : base()
        {
            User = user;
            TurnCancel = new AbortStatus();
        }

    }
}
