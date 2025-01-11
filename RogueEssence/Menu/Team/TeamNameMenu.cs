namespace RogueEssence.Menu
{
    public class TeamNameMenu : TextInputMenu
    {
        private int maxLength;
        public override int MaxLength { get { return maxLength; } }
        public delegate void OnChooseString(string name);
        private OnChooseString action;

        public TeamNameMenu(string title, string desc, int maxLength, string defaultName, OnChooseString action) :
            this(MenuLabel.TEAM_NAME_MENU, title, desc, maxLength, defaultName, action) { }
        public TeamNameMenu(string label, string title, string desc, int maxLength, string defaultName, OnChooseString action)
        {
            this.action = action;
            this.maxLength = maxLength;
            Initialize(title, desc, 256, defaultName);
        }
        
        protected override void Confirmed()
        {
            if (Text.Text.Trim() == "")
                GameManager.Instance.SE("Menu/Cancel");
            else
            {
                GameManager.Instance.SE("Menu/Confirm");
                MenuManager.Instance.RemoveMenu();
                action(Text.Text.Trim());
            }
        }

    }
}
