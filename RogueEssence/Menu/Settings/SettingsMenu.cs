using System;
using System.Collections.Generic;
using RogueElements;
using RogueEssence.Content;

namespace RogueEssence.Menu
{
    public class SettingsMenu : BaseSettingsMenu
    {
        //needs a summary menu?
        bool inGame;
        
        public SettingsMenu(bool inGame)
        {
            this.inGame = inGame;

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

            List<string> windowChoices = new List<string>();
            windowChoices.Add(Text.FormatKey("MENU_SETTINGS_FULL_SCREEN"));
            for(int ii = 1; ii < 9; ii++)
                windowChoices.Add(String.Format("{0}x{1}",GraphicsManager.ScreenWidth * ii, GraphicsManager.ScreenHeight * ii));
            totalChoices.Add(new MenuSetting(Text.FormatKey("MENU_SETTINGS_WINDOW_SIZE"), 88, 72, windowChoices, DiagManager.Instance.CurSettings.Window, confirmAction));
            
            List<string> borderChoices = new List<string>();
            for (int ii = 0; ii < 5; ii++)
                borderChoices.Add(ii.ToString());
            totalChoices.Add(new MenuSetting(Text.FormatKey("MENU_SETTINGS_BORDER_STYLE"), 88, 72, borderChoices, DiagManager.Instance.CurSettings.Border, confirmAction));

            if (!inGame)
            {
                List<string> langChoices = new List<string>();
                int langIndex = 0;
                for (int ii = 0; ii < TextInfo.SUPPORTED_LANGUAGES.Length; ii++)
                {
                    langChoices.Add(TextInfo.SUPPORTED_LANGUAGES[ii].ToName());
                    if (DiagManager.Instance.CurSettings.Language == TextInfo.SUPPORTED_LANGUAGES[ii])
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
            DiagManager.Instance.CurSettings.Window = TotalChoices[3].CurrentChoice;

            int zoom = (DiagManager.Instance.CurSettings.Window == 0) ? 2 : DiagManager.Instance.CurSettings.Window;
            if (DiagManager.Instance.CurSettings.Window != GraphicsManager.WindowZoom)
                GraphicsManager.WindowZoom = zoom;
            if ((DiagManager.Instance.CurSettings.Window == 0) != GraphicsManager.FullScreen)
                GraphicsManager.FullScreen = (DiagManager.Instance.CurSettings.Window == 0);

            DiagManager.Instance.CurSettings.Border = TotalChoices[4].CurrentChoice;

            bool changeLanguage = false;
            if (!inGame)
            {
                changeLanguage = DiagManager.Instance.CurSettings.Language != TextInfo.SUPPORTED_LANGUAGES[TotalChoices[5].CurrentChoice];
                DiagManager.Instance.CurSettings.Language = TextInfo.SUPPORTED_LANGUAGES[TotalChoices[5].CurrentChoice];

                Text.SetCultureCode(DiagManager.Instance.CurSettings.Language.ToString());
            }

            DiagManager.Instance.SaveSettings(DiagManager.Instance.CurSettings);

            if (changeLanguage)
            {
                //clear menu
                MenuManager.Instance.ClearMenus();
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
            else if (index == 4)
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
            MenuBase.BorderStyle = DiagManager.Instance.CurSettings.Border;
        }


        protected override void ChoiceChanged()
        {
            //change summary menu description...?

            base.ChoiceChanged();
        }
    }
}
