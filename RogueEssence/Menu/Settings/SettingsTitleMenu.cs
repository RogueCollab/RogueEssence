using NLua;
using RogueElements;
using RogueEssence.Content;
using RogueEssence.Data;
using RogueEssence.Dungeon;
using RogueEssence.Ground;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;

namespace RogueEssence.Menu
{
    public class SettingData
    {
        public string Name;
        public List<string> Options;
        public int Default;
        public Action<MenuSetting> SettingChangeAction;
        public Action<MenuSetting> SaveAction;
        public SettingData(string name, string[] options, int defaultValue, Action<MenuSetting> saveAction, Action<MenuSetting> settingChangeAction = null) : this(name, options.ToList(), defaultValue, saveAction, settingChangeAction) { }
        public SettingData(string name, List<string> options, int defaultValue, Action<MenuSetting> saveAction, Action<MenuSetting> settingChangeAction = null)
        {
            Name = name;
            Options = options;
            Default = defaultValue;
            SaveAction = saveAction;
            SettingChangeAction = settingChangeAction;
        }
    }
    public class SettingsPage
    {
        public string Title = "";
        public List<SettingData> Choices = new List<SettingData>();
        public Action GlobalSaveAction;
        public SettingsPage(string title)
        {
            Title = title;
        }
        public void AddSetting(string name, LuaTable options, int defaultValue, Action<MenuSetting> action, Action<MenuSetting> settingChangedAction = null)
        {
            Object[] arr = new Object[options.Values.Count];
            options.Values.CopyTo(arr, 0);
            List<string> list = arr.Select(a => a.ToString()).ToList();
            AddSetting(name, list, defaultValue, action, settingChangedAction);
        }
        public void AddSetting(string name, string[] options, int defaultValue, Action<MenuSetting> action, Action<MenuSetting> settingChangedAction = null)
        {
            AddSetting(name, options.ToList(), defaultValue, action, settingChangedAction);
        }
        public void AddSetting(string name, List<string> options, int defaultValue, Action<MenuSetting> action, Action<MenuSetting> settingChangedAction = null)
        {
            Choices.Add(new SettingData(name, options, defaultValue, action, settingChangedAction));
        }
    }

    public class SettingsTitleMenu : InteractableMenu
    {
        private static string originId { get { return PathMod.BaseNamespace; } }
        private readonly List<string> PageIds = new List<string>();
        private readonly Dictionary<string, SettingsPage> Pages = new Dictionary<string, SettingsPage>();
        private bool checkedPages = false;
        public static bool InGame
        {
            get
            {
                if (GameManager.Instance.CurrentScene == GroundScene.Instance ||
                    GameManager.Instance.CurrentScene == DungeonScene.Instance)
                    return true;
                return false;
            }
        }
        public int CurrentPage;
        public string CurrentPageId => PageIds[CurrentPage];
        public MenuText Title;
        public MenuCursor Left;
        public MenuCursor Right;
        internal SettingsPageSummaryMenu Preview;
        public SettingsTitleMenu() : this(MenuLabel.SETTINGS_MENU) { }
        public SettingsTitleMenu(string label)
        {
            Label = label;
            SetupDefaultSettingsPage();

            Bounds = new Rect(new Loc(16, 8), new Loc(224, VERT_SPACE + GraphicsManager.MenuBG.TileHeight * 2));

            Title = new MenuText(MenuLabel.TITLE, Pages[CurrentPageId].Title, new Loc(Bounds.Width / 2, GraphicsManager.MenuBG.TileHeight+1), DirH.None);
            Left = new MenuCursor($"{MenuLabel.CURSOR}_LEFT", this, Dir4.Left);
            Left.Loc = new Loc(GraphicsManager.MenuBG.TileWidth, GraphicsManager.MenuBG.TileHeight + 1);
            Right = new MenuCursor($"{MenuLabel.CURSOR}_RIGHT", this, Dir4.Right);
            Right.Loc = new Loc(Bounds.Width - GraphicsManager.MenuBG.TileWidth - GraphicsManager.Cursor.TileWidth, GraphicsManager.MenuBG.TileHeight+1);

            Elements.Add(Title);

            Preview = new SettingsPageSummaryMenu(this, Pages[CurrentPageId]);
            SummaryMenus.Add(Preview);
        }

        private bool changeLanguage = false;
        private void SetupDefaultSettingsPage()
        {
            SettingsPage origin = AddPage(originId, Text.FormatKey("MENU_SETTINGS_TITLE"));

            List<string> musicChoices = new List<string>();
            for (int ii = 0; ii <= 10; ii++)
                musicChoices.Add(ii.ToString());
            origin.AddSetting(Text.FormatKey("MENU_SETTINGS_MUSIC"), musicChoices, DiagManager.Instance.CurSettings.BGMBalance, setting => DiagManager.Instance.CurSettings.BGMBalance = setting.CurrentChoice, setting => SoundManager.BGMBalance = setting.CurrentChoice * 0.1f);

            List<string> soundChoices = new List<string>();
            for (int ii = 0; ii <= 10; ii++)
                soundChoices.Add(ii.ToString());
            origin.AddSetting(Text.FormatKey("MENU_SETTINGS_SOUND"), soundChoices, DiagManager.Instance.CurSettings.SEBalance, setting => DiagManager.Instance.CurSettings.SEBalance = setting.CurrentChoice, setting => SoundManager.SEBalance = setting.CurrentChoice * 0.1f);

            List<string> speedChoices = new List<string>();
            for (int ii = 0; ii <= (int)Settings.BattleSpeed.VeryFast; ii++)
                speedChoices.Add(((Settings.BattleSpeed)ii).ToLocal());
            origin.AddSetting(Text.FormatKey("MENU_SETTINGS_BATTLE_SPEED"), speedChoices, (int)DiagManager.Instance.CurSettings.BattleFlow, setting => DiagManager.Instance.CurSettings.BattleFlow = (Settings.BattleSpeed)setting.CurrentChoice);

            List<string> textSpeedChoices = new List<string>();
            for (int ii = 1; ii <= 6; ii++)
                textSpeedChoices.Add((ii * 0.5).ToString(CultureInfo.InvariantCulture));
            origin.AddSetting(Text.FormatKey("MENU_SETTINGS_TEXT_SPEED"), textSpeedChoices, (int)(DiagManager.Instance.CurSettings.TextSpeed * 2) - 1, setting => DiagManager.Instance.CurSettings.TextSpeed = (setting.CurrentChoice + 1) * 0.5);

            List<string> skillChoices = new List<string>();
            for (int ii = 0; ii <= (int)Settings.SkillDefault.All; ii++)
                skillChoices.Add(((Settings.SkillDefault)ii).ToLocal());
            origin.AddSetting(Text.FormatKey("MENU_SETTINGS_SKILL_DEFAULT"), skillChoices, (int)DiagManager.Instance.CurSettings.DefaultSkills, setting => {
                DiagManager.Instance.CurSettings.DefaultSkills = (Settings.SkillDefault)setting.CurrentChoice;
                if (InGame)
                    DataManager.Instance.Save.UpdateOptions();
            });

            List<string> minimapChoices = new List<string>();
            for (int ii = 0; ii < 10; ii++)
                minimapChoices.Add(String.Format("{0}%", (ii + 1) * 10));
            origin.AddSetting(Text.FormatKey("MENU_SETTINGS_MINIMAP_VISIBILITY"), minimapChoices, DiagManager.Instance.CurSettings.Minimap / 10 - 1, setting => DiagManager.Instance.CurSettings.Minimap = (setting.CurrentChoice + 1) * 10);

            List<string> minimapColorChoices = new List<string>();
            for (int ii = 0; ii <= (int)Settings.MinimapStyle.Blue; ii++)
                minimapColorChoices.Add(((Settings.MinimapStyle)ii).ToLocal());
            origin.AddSetting(Text.FormatKey("MENU_SETTINGS_MINIMAP_COLOR"), minimapColorChoices, (int)DiagManager.Instance.CurSettings.MinimapColor, setting => DiagManager.Instance.CurSettings.MinimapColor = (Settings.MinimapStyle)setting.CurrentChoice);

            List<string> borderChoices = new List<string>();
            for (int ii = 0; ii < 5; ii++)
                borderChoices.Add(ii.ToString());
            origin.AddSetting(Text.FormatKey("MENU_SETTINGS_BORDER_STYLE"), borderChoices, DiagManager.Instance.CurSettings.Border, setting => DiagManager.Instance.CurSettings.Border = setting.CurrentChoice, setting => MenuBase.BorderStyle = setting.CurrentChoice);


            List<string> windowChoices = new List<string>();
            windowChoices.Add(Text.FormatKey("MENU_SETTINGS_FULL_SCREEN"));
            for (int ii = 1; ii < 9; ii++)
                windowChoices.Add(String.Format("{0}x{1}", GraphicsManager.ScreenWidth * ii, GraphicsManager.ScreenHeight * ii));
            origin.AddSetting(Text.FormatKey("MENU_SETTINGS_WINDOW_SIZE"), windowChoices, DiagManager.Instance.CurSettings.Window, setting =>
            {
                DiagManager.Instance.CurSettings.Window = setting.CurrentChoice;
                GraphicsManager.SetWindowMode(DiagManager.Instance.CurSettings.Window);
            });

            if (!InGame)
            {
                List<string> langChoices = new List<string>();
                int langIndex = 0;
                for (int ii = 0; ii < Text.SupportedLangs.Length; ii++)
                {
                    langChoices.Add(Text.SupportedLangs[ii].ToName());
                    if (DiagManager.Instance.CurSettings.Language == Text.SupportedLangs[ii])
                        langIndex = ii;
                }

                origin.AddSetting(Text.FormatKey("MENU_SETTINGS_LANGUAGE"), langChoices, langIndex, setting =>
                {
                    changeLanguage = DiagManager.Instance.CurSettings.Language != Text.SupportedLangs[setting.CurrentChoice];
                    DiagManager.Instance.CurSettings.Language = Text.SupportedLangs[setting.CurrentChoice];

                    Text.SetCultureCode(DiagManager.Instance.CurSettings.Language);
                });

                origin.GlobalSaveAction = OriginGlobalSaveAction;
            }
        }

        public void OriginGlobalSaveAction()
        {
            DiagManager.Instance.SaveSettings(DiagManager.Instance.CurSettings);

            if (changeLanguage)
            {
                //clear menu
                MenuManager.Instance.ClearMenus();
                GraphicsManager.ReloadStatic();
                MenuManager.Instance.AddMenu(MenuManager.Instance.CreateDialogue(Text.FormatKey("DLG_LANGUAGE_SET", DiagManager.Instance.CurSettings.Language.ToName())), false);
            }
        }

        public bool HasPage(string key)
        {
            return PageIds.Contains(key);
        }

        //creates the page if absent
        public SettingsPage AddPage(string key, string title)
        {
            if (PageIds.Contains(key)) throw new DuplicateNameException($"Key \"{key}\" already exists");

            PageIds.Add(key);
            Pages[key] = new(title);
            PageIds.Sort((s1, s2) =>
            {
                //origin must always be at the start
                if (s1 == originId) return -1;
                if (s2 == originId) return 1;
                //the rest is in id order
                return s1.CompareTo(s2);
            });

            return Pages[key];
        }
        public override void Update(InputManager input)
        {
            if (!checkedPages)
            {
                checkedPages = true;
                if (Pages.Count > 1)
                {
                    Elements.Add(Left);
                    Elements.Add(Right);
                }
                else
                    OpenPage();
            }
            else if (Pages.Count == 1)
            {
                MenuManager.Instance.RemoveMenu();
            }

            if (input.JustPressed(FrameInput.InputType.Confirm))
            {
                    OpenPage();
            }
            else if(input.JustPressed(FrameInput.InputType.Cancel))
            {
                MenuManager.Instance.RemoveMenu();
            }
            else if (IsInputting(input, Dir8.Right))
            {
                CurrentPage = (CurrentPage + 1) % PageIds.Count;
                ChoiceChanged();
                GameManager.Instance.SE("Menu/Skip");
            }
            else if (IsInputting(input, Dir8.Left))
            {
                CurrentPage = (PageIds.Count + CurrentPage - 1) % PageIds.Count;
                ChoiceChanged();
                GameManager.Instance.SE("Menu/Skip");
            }
        }

        private void OpenPage()
        {
            SummaryMenus.Remove(Preview);
            MenuManager.Instance.AddMenu(new SettingsPageMenu(this, Pages[CurrentPageId]), true);
        }

        public void ChoiceChanged()
        {
            Title.SetText(Pages[CurrentPageId].Title);
            Preview.LoadOptions(Pages[CurrentPageId]);
        }
    }
}
