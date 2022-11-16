namespace RogueEssence.Menu
{
    public class ReplayInfoMenu : InfoMenu
    {
        public ReplayInfoMenu() : base(Text.FormatKey("MENU_REPLAY_INFO_TITLE"),
            DiagManager.Instance.GetControlString(FrameInput.InputType.Run) + " - " + Text.FormatKey("MENU_REPLAY_PAUSE") + "\n" +
                DiagManager.Instance.GetControlString(FrameInput.InputType.Attack) + " - " + Text.FormatKey("MENU_REPLAY_ADVANCE_TURN") + "\n" +
                DiagManager.Instance.GetControlString(FrameInput.InputType.Skills) + " - " + Text.FormatKey("MENU_REPLAY_SLOW_DOWN") + "\n" +
                DiagManager.Instance.GetControlString(FrameInput.InputType.Turn) + " - " + Text.FormatKey("MENU_REPLAY_SPEED_UP"),
            () => { })
        {
        }

    }
}
