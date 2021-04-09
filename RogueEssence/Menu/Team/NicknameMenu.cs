using System;

namespace RogueEssence.Menu
{
    public class NicknameMenu : TextInputMenu
    {
        public const int MAX_LENGTH = 88;
        public override int MaxLength { get { return MAX_LENGTH; } }

        OnChooseText chooseTextAction;
        Action cancelAction;

        public NicknameMenu(OnChooseText action, Action cancelAction)
        {
            chooseTextAction = action;
            this.cancelAction = cancelAction;
            Initialize(RogueEssence.Text.FormatKey("INPUT_NAME_TITLE"), RogueEssence.Text.FormatKey("INPUT_NAME_DESC"), 256);
        }

        protected override void Confirmed()
        {
            GameManager.Instance.SE("Menu/Confirm");
            MenuManager.Instance.RemoveMenu();
            chooseTextAction(Text.Text);
        }

        protected override void Canceled()
        {
            GameManager.Instance.SE("Menu/Cancel");
            MenuManager.Instance.RemoveMenu();
            cancelAction();
        }
    }
}
