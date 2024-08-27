using System;
using System.Collections.Generic;
using System.Globalization;
using RogueElements;
using RogueEssence.Content;
using RogueEssence.Data;
using RogueEssence.Dungeon;
using RogueEssence.Ground;

namespace RogueEssence.Menu
{
    //This menu is unused since 0.8.4
    public class SettingsMenu : BaseSettingsMenu
    {
        //needs a summary menu?
        bool inGame;

        public SettingsMenu() : this(MenuLabel.SETTINGS_MENU) { }
        public SettingsMenu(string label)
        {
            Label = label;
            this.inGame = false;
            if (GameManager.Instance.CurrentScene == GroundScene.Instance)
                this.inGame = true;
            if (GameManager.Instance.CurrentScene == DungeonScene.Instance)
                this.inGame = true;

            //individual tactics
            List<MenuSetting> totalChoices = new List<MenuSetting>();
            
            List<string> musicChoices = new List<string>();
            for (int ii = 0; ii <= 10; ii++)
                musicChoices.Add(ii.ToString());
            totalChoices.Add(new MenuSetting(Text.FormatKey("MENU_SETTINGS_MUSIC"), 88, 72, musicChoices, DiagManager.Instance.CurSettings.BGMBalance, confirmAction));

            List<string> soundChoices = new List<string>();
            for (int ii = 0; ii <= 10; ii++)
                soundChoices.Add(ii.ToString());
            totalChoices.Add(new MenuSetting(Text.FormatKey("MENU_SETTINGS_SOUND"), 88, 72, soundChoices, DiagManager.Instance.CurSettings.SEBalance, confirmAction));

            List<string> speedChoices = new List<string>();
            for (int ii = 0; ii <= (int)Settings.BattleSpeed.VeryFast; ii++)
                speedChoices.Add(((Settings.BattleSpeed)ii).ToLocal());
            totalChoices.Add(new MenuSetting(Text.FormatKey("MENU_SETTINGS_BATTLE_SPEED"), 88, 72, speedChoices, (int)DiagManager.Instance.CurSettings.BattleFlow, confirmAction));
            
            List<string> textSpeedChoices = new List<string>();
            for (int ii = 1; ii <= 6; ii++)
                textSpeedChoices.Add((ii * 0.5).ToString(CultureInfo.InvariantCulture));
            totalChoices.Add(new MenuSetting(Text.FormatKey("MENU_SETTINGS_TEXT_SPEED"), 88, 72, textSpeedChoices, (int)(DiagManager.Instance.CurSettings.TextSpeed * 2) - 1, confirmAction));
            
            List<string> skillChoices = new List<string>();
            for (int ii = 0; ii <= (int)Settings.SkillDefault.All; ii++)
                skillChoices.Add(((Settings.SkillDefault)ii).ToLocal());
            totalChoices.Add(new MenuSetting(Text.FormatKey("MENU_SETTINGS_SKILL_DEFAULT"), 88, 72, skillChoices, (int)DiagManager.Instance.CurSettings.DefaultSkills, confirmAction));

            List<string> minimapChoices = new List<string>();
            for (int ii = 0; ii < 10; ii++)
                minimapChoices.Add(String.Format("{0}%", (ii+1) * 10));
            totalChoices.Add(new MenuSetting(Text.FormatKey("MENU_SETTINGS_MINIMAP_VISIBILITY"), 88, 72, minimapChoices, DiagManager.Instance.CurSettings.Minimap / 10 - 1, confirmAction));

            List<string> minimapColorChoices = new List<string>();
            for (int ii = 0; ii <= (int)Settings.MinimapStyle.Blue; ii++)
                minimapColorChoices.Add(((Settings.MinimapStyle)ii).ToLocal());
            totalChoices.Add(new MenuSetting(Text.FormatKey("MENU_SETTINGS_MINIMAP_COLOR"), 88, 72, minimapColorChoices, (int)DiagManager.Instance.CurSettings.MinimapColor, confirmAction));

            List<string> borderChoices = new List<string>();
            for (int ii = 0; ii < 5; ii++)
                borderChoices.Add(ii.ToString());
            totalChoices.Add(new MenuSetting(Text.FormatKey("MENU_SETTINGS_BORDER_STYLE"), 88, 72, borderChoices, DiagManager.Instance.CurSettings.Border, confirmAction));

            List<string> windowChoices = new List<string>();
            windowChoices.Add(Text.FormatKey("MENU_SETTINGS_FULL_SCREEN"));
            for (int ii = 1; ii < 9; ii++)
                windowChoices.Add(String.Format("{0}x{1}", GraphicsManager.ScreenWidth * ii, GraphicsManager.ScreenHeight * ii));
            totalChoices.Add(new MenuSetting(Text.FormatKey("MENU_SETTINGS_WINDOW_SIZE"), 88, 72, windowChoices, DiagManager.Instance.CurSettings.Window, confirmAction));

            if (!inGame)
            {
                List<string> langChoices = new List<string>();
                int langIndex = 0;
                for (int ii = 0; ii < Text.SupportedLangs.Length; ii++)
                {
                    langChoices.Add(Text.SupportedLangs[ii].ToName());
                    if (DiagManager.Instance.CurSettings.Language == Text.SupportedLangs[ii])
                        langIndex = ii;
                }

                totalChoices.Add(new MenuSetting(Text.FormatKey("MENU_SETTINGS_LANGUAGE"), 88, 72, langChoices, langIndex, confirmAction));
            }

            Initialize(new Loc(16, 16), 224, Text.FormatKey("MENU_SETTINGS_TITLE"), totalChoices.ToArray());

        }

        private void confirmAction()
        {
            SaveCurrentChoices();

            //saves all the settings
            DiagManager.Instance.CurSettings.BGMBalance = TotalChoices[0].CurrentChoice;
            DiagManager.Instance.CurSettings.SEBalance = TotalChoices[1].CurrentChoice;
            DiagManager.Instance.CurSettings.BattleFlow = (Settings.BattleSpeed)TotalChoices[2].CurrentChoice;
            DiagManager.Instance.CurSettings.TextSpeed = (TotalChoices[3].CurrentChoice + 1) * 0.5;
            DiagManager.Instance.CurSettings.DefaultSkills = (Settings.SkillDefault)TotalChoices[4].CurrentChoice;
            if (this.inGame)
                DataManager.Instance.Save.UpdateOptions();
            DiagManager.Instance.CurSettings.Minimap = (TotalChoices[5].CurrentChoice + 1) * 10;
            DiagManager.Instance.CurSettings.MinimapColor = (Settings.MinimapStyle)TotalChoices[6].CurrentChoice;
            DiagManager.Instance.CurSettings.Border = TotalChoices[7].CurrentChoice;
            DiagManager.Instance.CurSettings.Window = TotalChoices[8].CurrentChoice;
            GraphicsManager.SetWindowMode(DiagManager.Instance.CurSettings.Window);


            bool changeLanguage = false;
            if (!inGame)
            {
                changeLanguage = DiagManager.Instance.CurSettings.Language != Text.SupportedLangs[TotalChoices[9].CurrentChoice];
                DiagManager.Instance.CurSettings.Language = Text.SupportedLangs[TotalChoices[9].CurrentChoice];

                Text.SetCultureCode(DiagManager.Instance.CurSettings.Language);
            }

            DiagManager.Instance.SaveSettings(DiagManager.Instance.CurSettings);

            if (changeLanguage)
            {
                //clear menu
                MenuManager.Instance.ClearMenus();
                GraphicsManager.ReloadStatic();
                MenuManager.Instance.AddMenu(MenuManager.Instance.CreateDialogue(Text.FormatKey("DLG_LANGUAGE_SET", DiagManager.Instance.CurSettings.Language.ToName())), false);
            }

            //MenuManager.Instance.RemoveMenu();
        }

        protected override void SettingChanged(int index)
        {
            //change the sounds as an example
            if (index == 0)
                SoundManager.BGMBalance = TotalChoices[index].CurrentChoice * 0.1f;
            else if (index == 1)
                SoundManager.SEBalance = TotalChoices[index].CurrentChoice * 0.1f;
            else if (index == 7)
                MenuBase.BorderStyle = TotalChoices[index].CurrentChoice;

            base.SettingChanged(index);
        }

        protected override void MenuPressed()
        {
            base.MenuPressed();
            resetExamples();
        }

        protected override void Canceled()
        {
            base.Canceled();
            resetExamples();
        }

        private void resetExamples()
        {
            SoundManager.BGMBalance = DiagManager.Instance.CurSettings.BGMBalance * 0.1f;
            SoundManager.SEBalance = DiagManager.Instance.CurSettings.SEBalance * 0.1f;
            DiagManager.Instance.CurSettings.Border = DiagManager.Instance.CurSettings.Border;
        }


        protected override void ChoiceChanged()
        {
            //change summary menu description...?

            base.ChoiceChanged();
        }
    }
}
