using System;
using System.Collections.Generic;
using RogueElements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RogueEssence.Menu
{
    public class MenuSetting : IChoosable
    {
        public string Label { get; set; }
        public MenuText SettingName;
        public MenuText Setting;

        public MenuGraphic Back;
        public MenuGraphic Forth;

        public Rect Bounds { get; set; }
        public bool Selected { get; private set; }

        public Action ChoiceAction;

        public List<string> TotalChoices;
        public int CurrentChoice;
        public int SavedChoice;

        public Color NormalColor;
        public Color ChangedColor;

        public MenuSetting() { }

        public MenuSetting(string text, int choiceOffset, int choiceWidth, List<string> totalChoices, int defaultChoice, Action choiceAction) : this("", text, Color.White, Color.Yellow, choiceOffset, choiceWidth, totalChoices, defaultChoice, defaultChoice, choiceAction) { }

        public MenuSetting(string label, string text, int choiceOffset, int choiceWidth, List<string> totalChoices, int defaultChoice, Action choiceAction) : this(label, text, Color.White, Color.Yellow, choiceOffset, choiceWidth, totalChoices, defaultChoice, defaultChoice, choiceAction) { }

        public MenuSetting(string text, int choiceOffset, int choiceWidth, List<string> totalChoices, int defaultChoice, int savedChoice, Action choiceAction) : this("", text, Color.White, Color.Yellow, choiceOffset, choiceWidth, totalChoices, defaultChoice, savedChoice, choiceAction) { }

        public MenuSetting(string label, string text, int choiceOffset, int choiceWidth, List<string> totalChoices, int defaultChoice, int savedChoice, Action choiceAction) : this(label, text, Color.White, Color.Yellow, choiceOffset, choiceWidth, totalChoices, defaultChoice, savedChoice, choiceAction) { }

        public MenuSetting(string text, Color nrmColor, Color chgColor, int choiceOffset, int choiceWidth, List<string> totalChoices, int defaultChoice, int savedChoice, Action choiceAction) : this("", text, nrmColor, chgColor, choiceOffset, choiceWidth, totalChoices, defaultChoice, savedChoice, choiceAction) { }

        public MenuSetting(string label, string text, Color nrmColor, Color chgColor, int choiceOffset, int choiceWidth, List<string> totalChoices, int defaultChoice, int savedChoice, Action choiceAction)
            : this()
        {
            Label = label;
            Bounds = new Rect();
            ChoiceAction = choiceAction;
            TotalChoices = totalChoices;
            CurrentChoice = defaultChoice;
            SavedChoice = savedChoice;

            NormalColor = nrmColor;
            ChangedColor = chgColor;
            SettingName = new MenuText(text + ":", new Loc(2, 1), nrmColor);
            Setting = new MenuText(totalChoices[defaultChoice], new Loc(choiceOffset + 16 + choiceWidth / 2, 1), DirV.Up, DirH.None, (CurrentChoice == SavedChoice) ? NormalColor : ChangedColor);
            Back = new MenuGraphic(new Loc(2 + choiceOffset, 1), MenuGraphic.GraphicType.Button, new Loc(4, 0));
            Forth = new MenuGraphic(new Loc(2 + choiceOffset + 16 + choiceWidth, 1), MenuGraphic.GraphicType.Button, new Loc(5, 0));
        }

        public void SetChoice(int choice)
        {
            CurrentChoice = choice;
            Setting.SetText(TotalChoices[CurrentChoice]);
            Setting.Color = (CurrentChoice == SavedChoice) ? NormalColor : ChangedColor;
        }

        //chosen by clicking
        public void OnMouseState(bool clicked) { }

        public void OnSelect(bool select) { }

        //selected by mouse
        public void OnHoverChanged(bool hover) { }

        //chosen by confirm button
        public void OnConfirm()
        {
            GameManager.Instance.SE("Menu/Confirm");
            if (ChoiceAction != null)
                ChoiceAction();
        }

        public void SaveChoice()
        {
            SavedChoice = CurrentChoice;
            Setting.Color = (CurrentChoice == SavedChoice) ? NormalColor : ChangedColor;
        }

        public void Draw(SpriteBatch spriteBatch, Loc offset)
        {
            //draw all elements with offset added
            SettingName.Draw(spriteBatch, Bounds.Start + offset);
            Setting.Draw(spriteBatch, Bounds.Start + offset);
            Back.Draw(spriteBatch, Bounds.Start + offset);
            Forth.Draw(spriteBatch, Bounds.Start + offset);
        }
    }
}
