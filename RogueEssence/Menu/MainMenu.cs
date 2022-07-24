using System.Collections.Generic;
using RogueElements;
using RogueEssence.Data;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RogueEssence.Content;
using RogueEssence.Dungeon;
using System;
using RogueEssence.Ground;

namespace RogueEssence.Menu
{
    public class MainMenu : SingleStripMenu
    {
        private static int defaultChoice;

        SummaryMenu titleMenu;

        SummaryMenu summaryMenu;


        public MainMenu()
        {
            bool equippedItems = false;
            foreach (Character character in DataManager.Instance.Save.ActiveTeam.Players)
            {
                if (!character.Dead && character.EquippedItem.ID > -1)
                {
                    equippedItems = true;
                    break;
                }
            }
            bool invEnabled = !(DataManager.Instance.Save.ActiveTeam.GetInvCount() == 0 && !equippedItems);

            List<MenuTextChoice> choices = new List<MenuTextChoice>();
            choices.Add(new MenuTextChoice(Text.FormatKey("MENU_MAIN_SKILLS"), () =>
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
            choices.Add(new MenuTextChoice(Text.FormatKey("MENU_MAIN_INVENTORY"), () => { MenuManager.Instance.AddMenu(new ItemMenu(), false); }, invEnabled, invEnabled ? Color.White : Color.Red));

            bool hasTactics = (DataManager.Instance.Save.ActiveTeam.Players.Count > 1);
            bool inReplay = (DataManager.Instance.CurrentReplay != null);
            choices.Add(new MenuTextChoice(Text.FormatKey("MENU_TACTICS_TITLE"), () => { MenuManager.Instance.AddMenu(new TacticsMenu(), false); }, (hasTactics && !inReplay), (hasTactics && !inReplay) ? Color.White : Color.Red));
            choices.Add(new MenuTextChoice(Text.FormatKey("MENU_TEAM_TITLE"), () => { MenuManager.Instance.AddMenu(new TeamMenu(false), false); }));

            if (GameManager.Instance.CurrentScene == DungeonScene.Instance)
            {
                bool hasGround = DungeonScene.Instance.CanCheckGround();
                choices.Add(new MenuTextChoice(Text.FormatKey("MENU_GROUND_TITLE"), checkGround, (hasGround && !inReplay), (hasGround && !inReplay) ? Color.White : Color.Red));
            }

            choices.Add(new MenuTextChoice(Text.FormatKey("MENU_OTHERS_TITLE"), () => { MenuManager.Instance.AddMenu(new OthersMenu(), false); }));
            if (!inReplay)
            {
                if (((GameManager.Instance.CurrentScene == DungeonScene.Instance)) || DataManager.Instance.Save is RogueProgress)
                    choices.Add(new MenuTextChoice(Text.FormatKey("MENU_REST_TITLE"), () => { MenuManager.Instance.AddMenu(new RestMenu(), false); }));
                else
                    choices.Add(new MenuTextChoice(Text.FormatKey("MENU_MAIN_SAVE"), SaveAction));
            }
            else
                choices.Add(new MenuTextChoice(Text.FormatKey("MENU_REPLAY_TITLE"), () => { MenuManager.Instance.AddMenu(new ReplayMenu(), false); }));
            choices.Add(new MenuTextChoice(Text.FormatKey("MENU_EXIT"), MenuManager.Instance.RemoveMenu));

            Initialize(new Loc(16, 16), CalculateChoiceLength(choices, 72), choices.ToArray(), Math.Min(Math.Max(0, defaultChoice), choices.Count - 1));

            titleMenu = new SummaryMenu(Rect.FromPoints(new Loc(Bounds.End.X + 16, 32), new Loc(GraphicsManager.ScreenWidth - 16, 32 + LINE_HEIGHT + GraphicsManager.MenuBG.TileHeight * 2)));
            MenuText title = new MenuText((GameManager.Instance.CurrentScene == DungeonScene.Instance) ? ZoneManager.Instance.CurrentMap.GetColoredName() : ZoneManager.Instance.CurrentGround.GetColoredName(),
                new Loc(titleMenu.Bounds.Width / 2, GraphicsManager.MenuBG.TileHeight), DirH.None);
            titleMenu.Elements.Add(title);

            summaryMenu = new SummaryMenu(Rect.FromPoints(new Loc(16, 32 + choices.Count * VERT_SPACE + GraphicsManager.MenuBG.TileHeight * 2),
                new Loc(GraphicsManager.ScreenWidth - 16, GraphicsManager.ScreenHeight - 8)));
            summaryMenu.Elements.Add(new MenuText(Text.FormatKey("MENU_BAG_MONEY", Text.FormatKey("MONEY_AMOUNT", DataManager.Instance.Save.ActiveTeam.Money)),
                new Loc(GraphicsManager.MenuBG.TileWidth + 8, GraphicsManager.MenuBG.TileHeight)));

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
                summaryMenu.Elements.Add(new MenuText(Text.FormatKey("MENU_MAP_CONDITION", weather),
                    new Loc(summaryMenu.Bounds.Width / 2, GraphicsManager.MenuBG.TileHeight)));
            }

            int level_length = GraphicsManager.TextFont.SubstringWidth(Text.FormatKey("MENU_TEAM_LEVEL_SHORT", DataManager.Instance.MaxLevel));
            int hp_length = GraphicsManager.TextFont.SubstringWidth(Text.FormatKey("MENU_TEAM_HP", 999, 999));
            int hunger_length = GraphicsManager.TextFont.SubstringWidth(Text.FormatKey("MENU_TEAM_HUNGER", Character.MAX_FULLNESS, Character.MAX_FULLNESS));

            int remaining_width = summaryMenu.Bounds.End.X - summaryMenu.Bounds.X - (GraphicsManager.MenuBG.TileWidth + 4) * 2 - level_length - hp_length - hunger_length - NicknameMenu.MAX_LENGTH;

            for (int ii = 0; ii < DataManager.Instance.Save.ActiveTeam.Players.Count; ii++)
            {
                Character character = DataManager.Instance.Save.ActiveTeam.Players[ii];
                int text_start = GraphicsManager.MenuBG.TileWidth + 4;
                summaryMenu.Elements.Add(new MenuText(character.GetDisplayName(true),
                new Loc(text_start, GraphicsManager.MenuBG.TileHeight + (ii + 1) * LINE_HEIGHT)));
                text_start += NicknameMenu.MAX_LENGTH + remaining_width / 3;
                summaryMenu.Elements.Add(new MenuText(Text.FormatKey("MENU_TEAM_LEVEL_SHORT", character.Level),
                new Loc(text_start + level_length / 2, GraphicsManager.MenuBG.TileHeight + (ii + 1) * LINE_HEIGHT), DirH.None));
                text_start += level_length + remaining_width / 3;
                summaryMenu.Elements.Add(new MenuText(Text.FormatKey("MENU_TEAM_HP", character.HP, character.MaxHP),
                new Loc(text_start + hp_length / 2, GraphicsManager.MenuBG.TileHeight + (ii + 1) * LINE_HEIGHT), DirH.None));
                text_start += hp_length + remaining_width / 3;
                summaryMenu.Elements.Add(new MenuText(Text.FormatKey("MENU_TEAM_HUNGER", character.Fullness, character.MaxFullness),
                new Loc(text_start + hunger_length / 2, GraphicsManager.MenuBG.TileHeight + (ii + 1) * LINE_HEIGHT), DirH.None));
            }
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
            titleMenu.Draw(spriteBatch);
            summaryMenu.Draw(spriteBatch);
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
    }
}
