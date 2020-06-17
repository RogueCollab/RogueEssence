namespace RogueEssence.Dungeon
{
    public class StatusCheckContext : GameContext
    {
        public StatusEffect Status;
        public bool msg;

        public int StackDiff;

        public StatusCheckContext(Character user, Character target, StatusEffect status, bool msg) : base()
        {
            User = user;
            Target = target;
            Status = status;
            this.msg = msg;
        }

    }
}
