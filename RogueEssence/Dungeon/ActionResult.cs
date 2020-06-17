namespace RogueEssence.Dungeon
{
    public class ActionResult
    {
        public enum ResultType
        {
            Fail,
            Success,
            TurnTaken
        }

        public ResultType Success;
    }
}
