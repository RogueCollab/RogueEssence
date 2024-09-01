using RogueElements;
using RogueEssence.Content;
using System.Collections.Generic;

namespace RogueEssence.Menu
{
    public class SettingsPageMenu : SingleStripMenu
    {
        public SettingsTitleMenu Parent;
        public SettingsPage Page;
        public Dictionary<MenuSetting, SettingData> SettingsData;
        public SettingsPageMenu(SettingsTitleMenu parent, SettingsPage page)
        {
            SettingsData = new Dictionary<MenuSetting, SettingData>();
            Page = page;
            Parent = parent;

            Bounds = new Rect(new Loc(16, parent.Bounds.Bottom), new Loc(parent.Bounds.Width, page.Choices.Count * VERT_SPACE + GraphicsManager.MenuBG.TileHeight * 2));

            SetChoices(GenerateChoices(Page).ToArray());
            CurrentChoice = 0;
        }

        private List<MenuSetting> GenerateChoices(SettingsPage page)
        {
            List<MenuSetting> choices = new List<MenuSetting>();
            foreach(SettingData setting in page.Choices)
            {
                MenuSetting element = new MenuSetting(setting.Name, 88, 72, setting.Options, setting.Default, ConfirmAction);
                choices.Add(element);
                SettingsData.Add(element, setting);
            }
            return choices;
        }
        private void ConfirmAction()
        {
            foreach (IChoosable element in Choices)
            {
                if(element is MenuSetting || SettingsData.ContainsKey((MenuSetting)element))
                {
                    MenuSetting setting = element as MenuSetting;
                    SettingsData[setting].SaveAction.Invoke(setting);
                    SettingsData[setting].Default = setting.CurrentChoice;
                    setting.SaveChoice();
                }
            }
            Page.GlobalSaveAction?.Invoke();
        }

        protected override void UpdateKeys(InputManager input)
        {
            bool moved = false;
            if (CurrentChoice < Choices.Count && Choices[CurrentChoice] is MenuSetting)
            {
                if (IsInputting(input, Dir8.Left))
                {
                    MenuSetting setting = (MenuSetting)Choices[CurrentChoice];
                    SetCurrentSetting(CurrentChoice, (setting.CurrentChoice + setting.TotalChoices.Count - 1) % setting.TotalChoices.Count);
                    moved = true;
                }
                else if (IsInputting(input, Dir8.Right))
                {
                    MenuSetting setting = (MenuSetting)Choices[CurrentChoice];
                    SetCurrentSetting(CurrentChoice, (setting.CurrentChoice + 1) % setting.TotalChoices.Count);
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
        protected void SetCurrentSetting(int index, int choice)
        {
            if (Choices[index] is MenuSetting setting)
            {
                setting.SetChoice(choice);
                SettingsData[setting].SettingChangeAction?.Invoke(setting);
            }
        }

        protected override void MenuPressed()
        {
            base.MenuPressed();
            resetExamples();
        }

        protected override void Canceled()
        {
            base.Canceled();
            Parent.Preview.Reload();
            resetExamples();
        }

        private void resetExamples()
        {
            for (int i = 0; i < Choices.Count; i++) {
                if (Choices[i] is MenuSetting setting)
                {
                    SettingData data = SettingsData[setting];
                    if (data.SettingChangeAction != null)
                    {
                        SetCurrentSetting(i, data.Default);
                        SettingsData[setting].SaveAction.Invoke(setting);
                    }
                }
            }
        }
    }
    public class SettingsPageSummaryMenu : SummaryMenu
    {
        public SettingsTitleMenu Parent;
        public SettingsPage Page;
        public Dictionary<MenuSetting, SettingData> SettingsData = new Dictionary<MenuSetting, SettingData>();
        public SettingsPageSummaryMenu(SettingsTitleMenu parent, SettingsPage page) : base(new Rect(new Loc(parent.Bounds.Left, parent.Bounds.Bottom), new Loc(parent.Bounds.Width, 16)))
        {
            Parent = parent;
            Page = page;
            LoadOptions(Page);
        }

        public void LoadOptions(SettingsPage page)
        {
            Page = page;
            Elements.Clear();
            List<MenuSetting> choices = new List<MenuSetting>();
            for (int i = 0; i < page.Choices.Count; i++)
            {
                SettingData setting = page.Choices[i];
                MenuSetting element = new MenuSetting(setting.Name, 88, 72, setting.Options, setting.Default, () => {});
                element.Bounds = new Rect(new Loc(GraphicsManager.MenuBG.TileWidth + 16 - 5, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * i - 1),
                    new Loc(Bounds.Width - GraphicsManager.MenuBG.TileWidth * 2 - 16 + 5 - 4, VERT_SPACE - 2));
                Elements.Add(element);
            }

            Bounds = new Rect(Bounds.Start, new Loc(Bounds.Width, page.Choices.Count * VERT_SPACE + GraphicsManager.MenuBG.TileHeight * 2));
        }

        internal void Reload()
        {
            LoadOptions(Page);
            if (!Parent.SummaryMenus.Contains(this))
            {
                Parent.SummaryMenus.Add(this);
            }
        }
    }
}
