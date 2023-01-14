using RogueElements;
using Microsoft.Xna.Framework.Input;
using RogueEssence.Data;

namespace RogueEssence.Menu
{
    public class RogueTeamInputMenu : TextInputMenu
    {
        public override int MaxLength { get { return 96; } }

        private bool randomized;
        private string chosenDest;
        private ulong? seed;

        public RogueTeamInputMenu(string chosenDungeon, ulong? seed)
        {
            chosenDest = chosenDungeon;
            this.seed = seed;
            Initialize(RogueEssence.Text.FormatKey("INPUT_TEAM_TITLE"), RogueEssence.Text.FormatKey("INPUT_TEAM_DESC"), 256);
        }

        public override void Update(InputManager input)
        {
            if (input.BaseKeyPressed(Keys.Tab))
            {
                randomized = true;
                //tab will replace the current line with a suggestion
                GameManager.Instance.SE("Menu/Skip");
                Text.SetText(DataManager.Instance.StartTeams[MathUtils.Rand.Next(DataManager.Instance.StartTeams.Count)]);

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
                Text.SetText(DataManager.Instance.StartTeams[MathUtils.Rand.Next(DataManager.Instance.StartTeams.Count)]);
                UpdatePickerPos();
            }
            MenuManager.Instance.AddMenu(new CharaChoiceMenu(Text.Text, randomized, chosenDest, seed), false);
        }
    }
}
