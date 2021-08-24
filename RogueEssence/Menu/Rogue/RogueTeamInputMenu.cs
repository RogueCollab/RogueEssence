using RogueElements;
using Microsoft.Xna.Framework.Input;
using RogueEssence.Data;

namespace RogueEssence.Menu
{
    public class RogueTeamInputMenu : TextInputMenu
    {
        public override int MaxLength { get { return 96; } }

        private int chosenDest;
        private ulong? seed;

        public RogueTeamInputMenu(int chosenDungeon, ulong? seed)
        {
            chosenDest = chosenDungeon;
            this.seed = seed;
            Initialize(RogueEssence.Text.FormatKey("INPUT_TEAM_TITLE"), RogueEssence.Text.FormatKey("INPUT_TEAM_DESC"), 256);
        }

        public override void Update(InputManager input)
        {
            if (input.BaseKeyPressed(Keys.Tab))
            {
                //tab will replace the current line with a suggestion
                GameManager.Instance.SE("Menu/Skip");
                Text.SetText(DataManager.Instance.StartTeams[MathUtils.Rand.Next(DataManager.Instance.StartTeams.Count)]);

                UpdatePickerPos();
            }
            else
                base.Update(input);
        }

        protected override void Confirmed()
        {
            GameManager.Instance.SE("Menu/Confirm");
            if (Text.Text == "")
            {
                Text.SetText(DataManager.Instance.StartTeams[MathUtils.Rand.Next(DataManager.Instance.StartTeams.Count)]);
                UpdatePickerPos();
            }
            MenuManager.Instance.AddMenu(new CharaChoiceMenu(Text.Text, chosenDest, seed), false);
        }
    }
}
