namespace RogueEssence.Menu
{
    public class TeamNameMenu : TextInputMenu
    {
        public override int MaxLength { get { return 116; } }
        public delegate void OnChooseString(string name);
        private OnChooseString action;

        public TeamNameMenu(string title, string desc, OnChooseString action)
        {
            this.action = action;
            Initialize(title, desc, 256);
        }
        
        protected override void Confirmed()
        {
            if (Text.Text == "")
                GameManager.Instance.SE("Menu/Cancel");
            else
            {
                GameManager.Instance.SE("Menu/Confirm");
                MenuManager.Instance.RemoveMenu();
                action(Text.Text);
            }
        }

    }
}
