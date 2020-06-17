namespace RogueEssence.Menu
{
    public class ReplayInfoMenu : InfoMenu
    {
        public ReplayInfoMenu() : base(Text.FormatKey("MENU_REPLAY_INFO_TITLE"),
            "[" + DiagManager.Instance.CurSettings.ActionKeys[(int)FrameInput.InputType.Run].ToLocal() + "] - " + Text.FormatKey("MENU_REPLAY_PAUSE") + "\n" +
                "[" + DiagManager.Instance.CurSettings.ActionKeys[(int)FrameInput.InputType.Attack].ToLocal() + "] - " + Text.FormatKey("MENU_REPLAY_ADVANCE_TURN") + "\n" +
                "[" + DiagManager.Instance.CurSettings.ActionKeys[(int)FrameInput.InputType.Skills].ToLocal() + "] - " + Text.FormatKey("MENU_REPLAY_SLOW_DOWN") + "\n" +
                "[" + DiagManager.Instance.CurSettings.ActionKeys[(int)FrameInput.InputType.Turn].ToLocal() + "] - " + Text.FormatKey("MENU_REPLAY_SPEED_UP"))
        {
        }

    }
}
