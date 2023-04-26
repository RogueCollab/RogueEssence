using System.Collections.Generic;
using RogueElements;
using RogueEssence.Data;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RogueEssence.Content;
using RogueEssence.Dungeon;
using System;
using KeraLua;
using RogueEssence.Ground;
using RogueEssence.Script;

namespace RogueEssence.Menu
{
    public class MainMenu : SingleStripMenu
    {
        private static int defaultChoice;

        private bool inReplay;

        MenuText menuTimer;
        
        public List<MenuTextChoice> Choices { get; set; }
        public List<IMenuElement> TitleElements { get; set; }
        public List<IMenuElement> SummaryElements { get; set; }
        
        public int MenuWidth { get; set; }
        public SummaryMenu TitleMenu { get; set; }
        public Rect TitleMenuBounds { get; set; }
        public SummaryMenu SummaryMenu { get; set; }
        public Rect SummaryMenuBounds { get; set; }
        public MainMenu()
        {
            Choices = new List<MenuTextChoice>();
            TitleElements = new List<IMenuElement>();
            SummaryElements = new List<IMenuElement>();
        }
        
         public void SetupChoices()
        {
            bool equippedItems = false;
            foreach (Character character in DataManager.Instance.Save.ActiveTeam.Players)
            {
                if (!character.Dead && !String.IsNullOrEmpty(character.EquippedItem.ID))
                {
                    equippedItems = true;
                    break;
                }
            }
            bool invEnabled = !(DataManager.Instance.Save.ActiveTeam.GetInvCount() == 0 && !equippedItems);

            Choices.Clear();
            Choices.Add(new MenuTextChoice(Text.FormatKey("MENU_MAIN_SKILLS"), () =>
            {
                int mainIndex = DataManager.Instance.Save.ActiveTeam.LeaderIndex;
                if (GameManager.Instance.CurrentScene == DungeonScene.Instance)
                {
                    CharIndex turnChar = ZoneManager.Instance.CurrentMap.CurrentTurnMap.GetCurrentTurnChar();
                    if (turnChar.Faction == Faction.Player)
                        mainIndex = turnChar.Char;
                }
                MenuManager.Instance.AddMenu(new SkillMenu(mainIndex), false);
            }));
            Choices.Add(new MenuTextChoice(Text.FormatKey("MENU_MAIN_INVENTORY"), () => { MenuManager.Instance.AddMenu(new ItemMenu(), false); }, invEnabled, invEnabled ? Color.White : Color.Red));

            bool hasTactics = (DataManager.Instance.Save.ActiveTeam.Players.Count > 1);
            inReplay = (DataManager.Instance.CurrentReplay != null);
            Choices.Add(new MenuTextChoice(Text.FormatKey("MENU_TACTICS_TITLE"), () => { MenuManager.Instance.AddMenu(new TacticsMenu(), false); }, (hasTactics && !inReplay), (hasTactics && !inReplay) ? Color.White : Color.Red));
            Choices.Add(new MenuTextChoice(Text.FormatKey("MENU_TEAM_TITLE"), () => { MenuManager.Instance.AddMenu(new TeamMenu(false), false); }));

            if (GameManager.Instance.CurrentScene == DungeonScene.Instance)
            {
                bool hasGround = DungeonScene.Instance.CanCheckGround();
                Choices.Add(new MenuTextChoice(Text.FormatKey("MENU_GROUND_TITLE"), checkGround, (hasGround && !inReplay), (hasGround && !inReplay) ? Color.White : Color.Red));
            }

            Choices.Add(new MenuTextChoice(Text.FormatKey("MENU_OTHERS_TITLE"), () => { CoroutineManager.Instance.StartCoroutine(LuaEngine.Instance.OnOthersMenuButtonPressed()); }));
            
            if (ZoneManager.Instance.InDevZone)
                Choices.Add(new MenuTextChoice(Text.FormatKey("MENU_MAIN_EDITOR_RETURN"), ReturnToEditorAction));
            else if (!inReplay)
            {
                if (((GameManager.Instance.CurrentScene == DungeonScene.Instance)) || DataManager.Instance.Save is RogueProgress)
                    Choices.Add(new MenuTextChoice(Text.FormatKey("MENU_REST_TITLE"), () => { MenuManager.Instance.AddMenu(new RestMenu(), false); }));
                else
                    Choices.Add(new MenuTextChoice(Text.FormatKey("MENU_MAIN_SAVE"), SaveAction));
            }
            else
            
            Choices.Add(new MenuTextChoice(Text.FormatKey("MENU_REPLAY_TITLE"), () => { MenuManager.Instance.AddMenu(new ReplayMenu(), false); }));
            Choices.Add(new MenuTextChoice(Text.FormatKey("MENU_EXIT"), MenuManager.Instance.RemoveMenu));
        }

        public void SetupTitleAndSummary()
        {
            MenuWidth = CalculateChoiceLength(Choices, 72);
            
            TitleElements.Clear();
            TitleMenuBounds = Rect.FromPoints(new Loc(MenuWidth + 16, 32),
                new Loc(GraphicsManager.ScreenWidth - 16, 32 + LINE_HEIGHT + GraphicsManager.MenuBG.TileHeight * 2));
            MenuText title = new MenuText((GameManager.Instance.CurrentScene == DungeonScene.Instance) ? ZoneManager.Instance.CurrentMap.GetColoredName() : ZoneManager.Instance.CurrentGround.GetColoredName(),
                new Loc(TitleMenuBounds.Width / 2, GraphicsManager.MenuBG.TileHeight), DirH.None);
            TitleElements.Add(title);
            
            SummaryElements.Clear();
            SummaryMenuBounds = Rect.FromPoints(
                new Loc(16, 32 + Choices.Count * VERT_SPACE + GraphicsManager.MenuBG.TileHeight * 2),
                new Loc(GraphicsManager.ScreenWidth - 16, GraphicsManager.ScreenHeight - 8));
            SummaryElements.Add(new MenuText(Text.FormatKey("MENU_BAG_MONEY", Text.FormatKey("MONEY_AMOUNT", DataManager.Instance.Save.ActiveTeam.Money)),
                new Loc(GraphicsManager.MenuBG.TileWidth + 4, GraphicsManager.MenuBG.TileHeight)));
            
            int level_length = GraphicsManager.TextFont.SubstringWidth(Text.FormatKey("MENU_TEAM_LEVEL_SHORT") + $"{DataManager.Instance.Start.MaxLevel}");
            int hp_length = GraphicsManager.TextFont.SubstringWidth(Text.FormatKey("MENU_TEAM_HP") + $" {999}/{999}");
            int hunger_length = GraphicsManager.TextFont.SubstringWidth(Text.FormatKey("MENU_TEAM_HUNGER") + $" {Character.MAX_FULLNESS}/{Character.MAX_FULLNESS}");

            int remaining_width = SummaryMenuBounds.End.X - SummaryMenuBounds.X - (GraphicsManager.MenuBG.TileWidth + 4) * 2 - level_length - hp_length - hunger_length - NicknameMenu.MAX_LENGTH;

            if (GameManager.Instance.CurrentScene == DungeonScene.Instance)
            {
                string weather = DataManager.Instance.GetMapStatus(DataManager.Instance.DefaultMapStatus).GetColoredName();
                foreach (MapStatus status in ZoneManager.Instance.CurrentMap.Status.Values)
                {
                    if (status.StatusStates.Contains<MapWeatherState>())
                    {
                        MapStatusData entry = (MapStatusData)status.GetData();
                        weather = entry.GetColoredName();
                        break;
                    }
                }
                SummaryElements.Add(new MenuText(Text.FormatKey("MENU_MAP_CONDITION", weather),
                    new Loc(GraphicsManager.MenuBG.TileWidth + 4 + NicknameMenu.MAX_LENGTH + remaining_width / 3, GraphicsManager.MenuBG.TileHeight), DirH.Left));
            }
            
            int max_hp_length = GraphicsManager.TextFont.SubstringWidth("999");
            int max_fullness_length = GraphicsManager.TextFont.SubstringWidth(Character.MAX_FULLNESS.ToString());
            int hp_label_length = GraphicsManager.TextFont.SubstringWidth(Text.FormatKey("MENU_TEAM_HP"));
            int hunger_label_length = GraphicsManager.TextFont.SubstringWidth(Text.FormatKey("MENU_TEAM_HUNGER"));
            
            for (int ii = 0; ii < DataManager.Instance.Save.ActiveTeam.Players.Count; ii++)
            {
                Character character = DataManager.Instance.Save.ActiveTeam.Players[ii];
                int text_start = GraphicsManager.MenuBG.TileWidth + 4;
                SummaryElements.Add(new MenuText(character.GetDisplayName(true),
                new Loc(text_start, GraphicsManager.MenuBG.TileHeight + (ii + 1) * LINE_HEIGHT)));
                text_start += NicknameMenu.MAX_LENGTH + remaining_width / 3;
                SummaryElements.Add(new MenuText(Text.FormatKey("MENU_TEAM_LEVEL_SHORT"),
                    new Loc(text_start, GraphicsManager.MenuBG.TileHeight + (ii + 1) * LINE_HEIGHT), DirH.Left));
                text_start += level_length;
                SummaryElements.Add(new MenuText(character.Level.ToString(),
                    new Loc(text_start, GraphicsManager.MenuBG.TileHeight + (ii + 1) * LINE_HEIGHT), DirH.Right));
                text_start += remaining_width / 3;
                SummaryElements.Add(new MenuText(Text.FormatKey("MENU_TEAM_HP"),
                    new Loc(text_start, GraphicsManager.MenuBG.TileHeight + (ii + 1) * LINE_HEIGHT), DirH.Left));
                text_start += hp_label_length + max_hp_length + 4;
                SummaryElements.Add(new MenuText(character.HP.ToString(),
                    new Loc(text_start, GraphicsManager.MenuBG.TileHeight + (ii + 1) * LINE_HEIGHT), DirH.Right));
                text_start += 4;
                SummaryElements.Add(new MenuText("/",
                    new Loc(text_start, GraphicsManager.MenuBG.TileHeight + (ii + 1) * LINE_HEIGHT), DirH.None));
                text_start += character.MaxHP.ToString().StartsWith("1") ? 2 : 4;
                SummaryElements.Add(new MenuText(character.MaxHP.ToString(),
                    new Loc(text_start, GraphicsManager.MenuBG.TileHeight + (ii + 1) * LINE_HEIGHT), DirH.Left));
                text_start += max_hp_length + remaining_width / 3 + (character.MaxHP.ToString().StartsWith("1") ? 2 : 0);
                SummaryElements.Add(new MenuText(Text.FormatKey("MENU_TEAM_HUNGER"), 
                    new Loc(text_start, GraphicsManager.MenuBG.TileHeight + (ii + 1) * LINE_HEIGHT), DirH.Left));
                text_start += hunger_label_length + max_fullness_length + 2;
                SummaryElements.Add(new MenuText(character.Fullness.ToString(),
                    new Loc(text_start, GraphicsManager.MenuBG.TileHeight + (ii + 1) * LINE_HEIGHT), DirH.Right));
                text_start += 4;
                SummaryElements.Add(new MenuText("/",
                    new Loc(text_start, GraphicsManager.MenuBG.TileHeight + (ii + 1) * LINE_HEIGHT), DirH.None));
                text_start += character.MaxFullness.ToString().StartsWith("1") ? 2 : 4;
                SummaryElements.Add(new MenuText(character.MaxFullness.ToString(),
                    new Loc(text_start, GraphicsManager.MenuBG.TileHeight + (ii + 1) * LINE_HEIGHT), DirH.Left));
            }
            
            if (GameManager.Instance.CurrentScene == DungeonScene.Instance && !inReplay)
            {
                int timerStart = GraphicsManager.MenuBG.TileWidth + 4 + NicknameMenu.MAX_LENGTH + level_length + hp_length + remaining_width;
                menuTimer = new MenuText(Text.FormatKey("MENU_TIMER", DataManager.Instance.Save.GetDungeonTimeDisplay()),
                    new Loc(timerStart, GraphicsManager.MenuBG.TileHeight), DirH.Left);
                SummaryElements.Add(menuTimer);
            }
        }
        
        public void InitMenu()
        {
            Initialize(new Loc(16, 16), MenuWidth, Choices.ToArray(), Math.Min(Math.Max(0, defaultChoice), Choices.Count - 1));

            TitleMenu = new SummaryMenu(TitleMenuBounds);
            TitleMenu.Elements = TitleElements;

            SummaryMenu = new SummaryMenu(SummaryMenuBounds);
            SummaryMenu.Elements = SummaryElements;
        }

        private void checkGround()
        {
            MenuManager.Instance.AddMenu(DungeonScene.Instance.GetGroundCheckMenu(), false);
        }

        protected override void ChoiceChanged()
        {
            defaultChoice = CurrentChoice;
            base.ChoiceChanged();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!Visible)
                return;
            base.Draw(spriteBatch);

            //draw other windows
            TitleMenu.Draw(spriteBatch);
            SummaryMenu.Draw(spriteBatch);
        }


        private void SaveAction()
        {
            List<DialogueChoice> choices = new List<DialogueChoice>();
            choices.Add(new DialogueChoice(Text.FormatKey("MENU_SAVE_AND_CONTINUE"), () =>
            {
                MenuManager.Instance.ClearMenus();
                MenuManager.Instance.NextAction = processSave(false);
            }));
            choices.Add(new DialogueChoice(Text.FormatKey("MENU_SAVE_AND_QUIT"), () =>
            {
                MenuManager.Instance.ClearMenus();
                MenuManager.Instance.NextAction = processSave(true);
            }));
            choices.Add(new DialogueChoice(Text.FormatKey("MENU_CANCEL"), () => { }));
            MenuManager.Instance.AddMenu(MenuManager.Instance.CreateMultiQuestion(Text.FormatKey("DLG_WHAT_DO"), false, choices, 0, choices.Count - 1), false);
            
        }

        public override void Update(InputManager input)
        {
            base.Update(input);
            menuTimer?.SetText(Text.FormatKey("MENU_TIMER", DataManager.Instance.Save.GetDungeonTimeDisplay()));
        }


        public IEnumerator<YieldInstruction> processSave(bool returnToTitle)
        {
            Action exitAction;
            if (!returnToTitle)
                exitAction = () => { };
            else
                exitAction = exitToTitle;
            yield return CoroutineManager.Instance.StartCoroutine(GroundScene.Instance.SaveGame());
            MenuManager.Instance.AddMenu(MenuManager.Instance.CreateDialogue(MonsterID.Invalid, null, new EmoteStyle(0), false, exitAction, -1, false, false, false, Text.FormatKey("DLG_SAVE_COMPLETE")), true);
        }

        private void exitToTitle()
        {
            MenuManager.Instance.EndAction = GameManager.Instance.FadeOut(false);
            GameManager.Instance.SceneOutcome = GameManager.Instance.RestartToTitle();
        }

        private void ReturnToEditorAction()
        {
            MenuManager.Instance.ClearMenus();
            GameManager.Instance.SceneOutcome = GameManager.Instance.ReturnToEditor();
        }
    }
}
