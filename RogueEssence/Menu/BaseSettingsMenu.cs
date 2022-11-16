using RogueElements;
using RogueEssence.Content;

namespace RogueEssence.Menu
{
    public abstract class BaseSettingsMenu : TitledStripMenu
    {
        public MenuSetting[] TotalChoices;

        protected void Initialize(Loc start, int width, string title, MenuSetting[] totalChoices)
        {
            TotalChoices = totalChoices;
            
            Bounds = new Rect(start, new Loc(width, totalChoices.Length * VERT_SPACE + GraphicsManager.MenuBG.TileHeight * 2 + ContentOffset));

            IncludeTitle(title);

            SetChoices(totalChoices);
            CurrentChoice = 0;
        }

        protected virtual void SettingChanged(int index)
        {
            ChoiceChanged();
        }

        protected void SetCurrentSetting(int index, int choice)
        {
            //refresh the choice
            TotalChoices[index].SetChoice(choice);
            SettingChanged(index);
        }

        public void SaveCurrentChoices()
        {
            for (int ii = 0; ii < TotalChoices.Length; ii++)
                TotalChoices[ii].SaveChoice();
        }

        protected override void UpdateMouse(InputManager input)
        {
            base.UpdateMouse(input);
        }

        protected override void UpdateKeys(InputManager input)
        {
            bool moved = false;
            if (CurrentChoice < TotalChoices.Length)
            {
                if (IsInputting(input, Dir8.Left))
                {
                    SetCurrentSetting(CurrentChoice, (TotalChoices[CurrentChoice].CurrentChoice + TotalChoices[CurrentChoice].TotalChoices.Count - 1) % TotalChoices[CurrentChoice].TotalChoices.Count);
                    moved = true;
                }
                else if (IsInputting(input, Dir8.Right))
                {
                    SetCurrentSetting(CurrentChoice, (TotalChoices[CurrentChoice].CurrentChoice + 1) % TotalChoices[CurrentChoice].TotalChoices.Count);
                    moved = true;
                }
            }
            if (moved)
            {
                GameManager.Instance.SE("Menu/Skip");
                cursor.ResetTimeOffset();
            }
            else
                base.UpdateKeys(input);
        }
    }
}
