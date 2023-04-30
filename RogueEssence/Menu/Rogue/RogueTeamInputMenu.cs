using RogueElements;
using Microsoft.Xna.Framework.Input;
using RogueEssence.Data;

namespace RogueEssence.Menu
{
    public class RogueTeamInputMenu : TextInputMenu
    {
        public override int MaxLength { get { return 96; } }
        private RogueConfig config;
        private bool randomized;

        public RogueTeamInputMenu(RogueConfig config)
        {
            this.config = config;
            Initialize(RogueEssence.Text.FormatKey("INPUT_TEAM_TITLE"), RogueEssence.Text.FormatKey("INPUT_TEAM_DESC"), 256);
        }

        public override void Update(InputManager input)
        {
            if (input.BaseKeyPressed(Keys.Tab))
            {
                randomized = true;
                //tab will replace the current line with a suggestion
                GameManager.Instance.SE("Menu/Skip");
                Text.SetText(DataManager.Instance.Start.Teams[MathUtils.Rand.Next(DataManager.Instance.Start.Teams.Count)]);

                UpdatePickerPos();
            }
            else
            {
                string prevText = Text.Text;
                base.Update(input); 
                if (prevText != Text.Text)
                {
                    randomized = false;
                }
            }

        }

        protected override void Confirmed()
        {
            GameManager.Instance.SE("Menu/Confirm");
            if (Text.Text == "")
            {
                randomized = true;
                Text.SetText(DataManager.Instance.Start.Teams[MathUtils.Rand.Next(DataManager.Instance.Start.Teams.Count)]);
                UpdatePickerPos();
            }

            config.TeamName = Text.Text;
            config.TeamRandomized = randomized;
            MenuManager.Instance.AddMenu(new CharaChoiceMenu(config), false);
        }
    }
}
