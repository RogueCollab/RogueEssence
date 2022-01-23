using System;
using System.Collections.Generic;
using System.IO;
using RogueElements;
using RogueEssence.Data;
using RogueEssence.Dungeon;
using RogueEssence.Script;
using RogueEssence.Content;
using Microsoft.Xna.Framework.Graphics;

namespace RogueEssence.Menu
{
    public class TopMenu : SingleStripMenu
    {
        SummaryMenu titleMenu;

        public override bool CanMenu { get { return false; } }
        public override bool CanCancel { get { return false; } }

        public TopMenu()
        {
            bool inQuest = PathMod.Quest != "";
            List<MenuTextChoice> choices = new List<MenuTextChoice>();

            if (DataManager.Instance.Save != null)
            {
                if (DataManager.Instance.Save.Rescue != null)
                {
                    if (DataManager.Instance.Save.Rescue.SOS.RescuingTeam == null)
                        choices.Add(new MenuTextChoice(Text.FormatKey("MENU_TOP_RESCUE_AWAIT"), () => { MenuManager.Instance.AddMenu(new RescueMenu(), false); }));
                    else
                        choices.Add(new MenuTextChoice(Text.FormatKey("MENU_TOP_CONTINUE"), () => { Continue(DataManager.Instance.Save.Rescue.SOS); }));
                }
                else
                    choices.Add(new MenuTextChoice(Text.FormatKey("MENU_TOP_CONTINUE"), () => { Continue(null); }));
            }
            else
                choices.Add(new MenuTextChoice(Text.FormatKey("MENU_TOP_NEW"), () => { StartFlow(new MonsterID(-1, -1, -1, Gender.Unknown), null, -1); }));

            if (DiagManager.Instance.DevMode || DataManager.Instance.Save != null)
                choices.Add(new MenuTextChoice(Text.FormatKey("MENU_TOP_ROGUE"), () => { MenuManager.Instance.AddMenu(new RogueMenu(), false); }));


            if (DataManager.Instance.FoundRecords(PathMod.ModSavePath(DataManager.REPLAY_PATH)) || DataManager.Instance.Save != null || RecordHeaderData.LoadHighScores().Count > 0)
                choices.Add(new MenuTextChoice(Text.FormatKey("MENU_TOP_RECORD"), () => { MenuManager.Instance.AddMenu(new RecordsMenu(), false); }));
            choices.Add(new MenuTextChoice(Text.FormatKey("MENU_OPTIONS_TITLE"), () => { MenuManager.Instance.AddMenu(new OptionsMenu(), false); }));

            if (!inQuest)
            {
                string[] questsPath = Directory.GetDirectories(PathMod.MODS_PATH);
                if (QuestsMenu.GetEligibleQuests().Count > 0)
                    choices.Add(new MenuTextChoice(Text.FormatKey("MENU_QUESTS_TITLE"), () => { MenuManager.Instance.AddMenu(new QuestsMenu(), false); }));
            }
            else
                choices.Add(new MenuTextChoice(Text.FormatKey("MENU_QUESTS_EXIT"), exitMod));

            string[] modsPath = Directory.GetDirectories(PathMod.MODS_PATH);
            if (ModsMenu.GetEligibleMods().Count > 0)
                choices.Add(new MenuTextChoice(Text.FormatKey("MENU_MODS_TITLE"), () => { MenuManager.Instance.AddMenu(new ModsMenu(), false); }));

            choices.Add(new MenuTextChoice(Text.FormatKey("MENU_QUIT_GAME"), exitGame));

            Initialize(new Loc(16, 16), CalculateChoiceLength(choices, 72), choices.ToArray(), 0);

            titleMenu = new SummaryMenu(Rect.FromPoints(new Loc(Bounds.End.X + 16, 16), new Loc(GraphicsManager.ScreenWidth - 16, 16 + LINE_HEIGHT + GraphicsManager.MenuBG.TileHeight * 2)));
            MenuText title = new MenuText(getModName(PathMod.Quest), new Loc(titleMenu.Bounds.Width / 2, GraphicsManager.MenuBG.TileHeight), DirH.None);
            titleMenu.Elements.Add(title);

        }


        private string getModName(string path)
        {
            ModHeader header = PathMod.GetModDetails(path);
            if (header.IsValid())
                return header.Name;

            return Path.GetFileName(path);
        }

        protected override void MenuPressed()
        {

        }

        protected override void Canceled()
        {

        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!Visible)
                return;
            base.Draw(spriteBatch);

            //draw other windows
            if (PathMod.Quest != "")
                titleMenu.Draw(spriteBatch);
        }

        private void exitMod()
        {
            MenuManager.Instance.ClearMenus();
            GameManager.Instance.SceneOutcome = GameManager.Instance.SetQuest("", PathMod.Mod, true);
        }


        private void exitGame()
        {
            GameBase.CurrentPhase = GameBase.LoadPhase.Unload;
        }


        private static void cannotRead(string path)
        {
            MenuManager.Instance.AddMenu(MenuManager.Instance.CreateDialogue(Text.FormatKey("DLG_ERR_READ_FILE"),
                Text.FormatKey("DLG_ERR_READ_FILE_FALLBACK", path)), false);
        }

        public static void Continue(SOSMail rescueMail)
        {
            //check for presence of a main save-quicksave
            ReplayData replay = null;
            string recordDir = PathMod.ModSavePath(DataManager.SAVE_PATH, DataManager.QUICKSAVE_FILE_PATH);
            if (File.Exists(recordDir))
            {
                replay = DataManager.Instance.LoadReplay(recordDir, true);
                if (replay == null)
                {
                    cannotRead(recordDir);
                    return;
                }
            }
            if (replay != null)
            {
                MenuManager.Instance.ClearMenus();
                GameManager.Instance.SceneOutcome = continueReplay(replay, rescueMail);
                return;
            }

            //then, we should load a main save instead
            GameState state = DataManager.Instance.LoadMainGameState();
            if (state == null)
            {
                cannotRead(DataManager.SAVE_PATH + DataManager.SAVE_FILE_PATH);
                return;
            }
            if (state.Save.Rescue != null)
            {
                state.Save.Rescue = null;
                DataManager.Instance.SaveGameState(state);
            }
            MenuManager.Instance.ClearMenus();
            GameManager.Instance.SceneOutcome = continueMain(state);
        }

        private static IEnumerator<YieldInstruction> continueReplay(ReplayData replay, SOSMail rescueMail)
        {
            GameManager.Instance.BGM("", true);
            yield return CoroutineManager.Instance.StartCoroutine(GameManager.Instance.FadeOut(false));

            DataManager.Instance.MsgLog.Clear();
            //load that up instead if found
            GameState state = replay.ReadState();
            DataManager.Instance.SetProgress(state.Save);
            LuaEngine.Instance.LoadSavedData(DataManager.Instance.Save); //notify script engine
            ZoneManager.LoadFromState(state.Zone);
            LuaEngine.Instance.UpdateZoneInstance();

            //NOTE: In order to preserve debug consistency, you SHOULD set the language to that of the quicksave.
            //HOWEVER, it would be too inconvenient for players sharing their quicksaves, thus this feature is LEFT OUT.

            DataManager.Instance.Loading = DataManager.LoadMode.Loading;
            DataManager.Instance.CurrentReplay = replay;

            if (rescueMail != null)
                DataManager.Instance.Save.Rescue = new RescueState(rescueMail, false);

            yield return CoroutineManager.Instance.StartCoroutine(ZoneManager.Instance.CurrentZone.OnInit());

            if (DataManager.Instance.Save.NextDest.IsValid())
            {
                yield return CoroutineManager.Instance.StartCoroutine(GameManager.Instance.MoveToZone(DataManager.Instance.Save.NextDest));
            }
            else
            {
                //no valid next dest happens when the player has saved in a ground map in the middle of an adventure
                DataManager.Instance.ResumePlay(DataManager.Instance.CurrentReplay);
                DataManager.Instance.CurrentReplay = null;

                GameManager.Instance.SetFade(true, false);

                DataManager.Instance.Loading = DataManager.LoadMode.None;

                if (ZoneManager.Instance.CurrentMapID.Segment > -1)
                {
                    GameManager.Instance.MoveToScene(DungeonScene.Instance);
                    GameManager.Instance.BGM(ZoneManager.Instance.CurrentMap.Music, true);
                }
                else
                {
                    GameManager.Instance.MoveToScene(Ground.GroundScene.Instance);
                    GameManager.Instance.BGM(ZoneManager.Instance.CurrentGround.Music, true);
                }

                yield return CoroutineManager.Instance.StartCoroutine(GameManager.Instance.FadeIn());
            }
        }

        private static IEnumerator<YieldInstruction> continueMain(GameState mainState)
        {
            yield return CoroutineManager.Instance.StartCoroutine(GameManager.Instance.FadeOut(false));

            MainProgress mainSave = mainState.Save as MainProgress;
            DataManager.Instance.SetProgress(mainSave);
            mainSave.MergeDataTo(mainSave);
            ZoneManager.LoadFromState(mainState.Zone);
            LuaEngine.Instance.UpdateZoneInstance();

            yield return CoroutineManager.Instance.StartCoroutine(ZoneManager.Instance.CurrentZone.OnInit());
            if (ZoneManager.Instance.CurrentMapID.Segment > -1)
                GameManager.Instance.MoveToScene(DungeonScene.Instance);
            else
                GameManager.Instance.MoveToScene(Ground.GroundScene.Instance);

            if (DataManager.Instance.Save.NextDest.IsValid())
                yield return CoroutineManager.Instance.StartCoroutine(GameManager.Instance.MoveToZone(DataManager.Instance.Save.NextDest));
            else
            {
                if (ZoneManager.Instance.CurrentMapID.Segment > -1)
                {
                    GameManager.Instance.BGM(ZoneManager.Instance.CurrentMap.Music, true);
                    yield return CoroutineManager.Instance.StartCoroutine(DungeonScene.Instance.InitFloor());
                }
                else
                {
                    GameManager.Instance.BGM(ZoneManager.Instance.CurrentGround.Music, true);
                    yield return CoroutineManager.Instance.StartCoroutine(Ground.GroundScene.Instance.InitGround(true));
                }

                yield return CoroutineManager.Instance.StartCoroutine(GameManager.Instance.FadeIn());
            }
        }

        private static void StartFlow(MonsterID monId, string name, int backPhase)
        {
            if (monId.Species == -1 || backPhase == 0)
            {
                if (DataManager.Instance.StartChars.Count > 1)
                {
                    int startIndex = 0;
                    if (backPhase == 0)
                        startIndex = DataManager.Instance.StartChars.FindIndex(start => start.mon == monId);
                    MenuManager.Instance.AddMenu(new ChooseMonsterMenu(Text.FormatKey("MENU_CHARA_CHOICE_TITLE"), DataManager.Instance.StartChars, startIndex, (int index) =>
                    {
                        string newName = null;
                        if (DataManager.Instance.StartChars[index].name != "")
                            newName = DataManager.Instance.StartChars[index].name;
                        StartFlow(DataManager.Instance.StartChars[index].mon, newName, -1);
                    }, () => { }), false);
                    return;
                }
                else if (backPhase == 0)
                    return;
                else if (DataManager.Instance.StartChars.Count == 1)
                {
                    monId = DataManager.Instance.StartChars[0].mon;
                    if (DataManager.Instance.StartChars[0].name != "")
                        name = DataManager.Instance.StartChars[0].name;
                }
                else
                {
                    MenuManager.Instance.ClearMenus();
                    GameManager.Instance.SceneOutcome = Begin(new MonsterID(0, 0, 0, Gender.Genderless), "");
                    return;
                }
            }

            if (monId.Gender == Gender.Unknown || backPhase == 1)
            {
                MonsterData monEntry = DataManager.Instance.GetMonster(monId.Species);
                BaseMonsterForm form = monEntry.Forms[monId.Form];
                List<Gender> genders = form.GetPossibleGenders();
                if (genders.Count > 1)
                {
                    int startIndex = 0;
                    if (backPhase == 1)
                        startIndex = monId.Gender == Gender.Female ? 1 : 0;
                    List<DialogueChoice> choices = new()
                    {
                        new DialogueChoice(Text.FormatKey("MENU_BOY"), () =>
                        {
                            StartFlow(new MonsterID(monId.Species, monId.Form, monId.Skin, Gender.Male), name, -1);
                        }),
                        new DialogueChoice(Text.FormatKey("MENU_GIRL"), () =>
                        {
                            StartFlow(new MonsterID(monId.Species, monId.Form, monId.Skin, Gender.Female), name, -1);
                        }),
                        new DialogueChoice(Text.FormatKey("MENU_CANCEL"), () =>
                        { })
                    };
                    MenuManager.Instance.AddMenu(MenuManager.Instance.CreateMultiQuestion(Text.FormatKey("DLG_ASK_GENDER"), false, choices, startIndex, choices.Count - 1), false);
                    return;
                }
                else if (backPhase == 1)
                {
                    StartFlow(new MonsterID(monId.Species, monId.Form, monId.Skin, Gender.Unknown), name, 0);
                    return;
                }
                else
                    monId.Gender = genders[0];
            }

            if (name == null)
            {
                MenuManager.Instance.AddMenu(new NicknameMenu((string name) =>
                {
                    StartFlow(monId, name, -1);
                }, () =>
                {
                    StartFlow(monId, null, 1);
                }), false);
                return;
            }

            //begin

            MenuManager.Instance.ClearMenus();
            GameManager.Instance.SceneOutcome = Begin(monId, name);
        }


        private static IEnumerator<YieldInstruction> Begin(MonsterID monId, string name)
        {
            yield return CoroutineManager.Instance.StartCoroutine(GameManager.Instance.FadeOut(false));

            DataManager.Instance.SetProgress(new MainProgress(MathUtils.Rand.NextUInt64(), Guid.NewGuid().ToString().ToUpper()));
            DataManager.Instance.Save.StartDate = String.Format("{0:yyyy-MM-dd_HH-mm-ss}", DateTime.Now);
            DataManager.Instance.Save.ActiveTeam = new ExplorerTeam();

            Character newChar = DataManager.Instance.Save.ActiveTeam.CreatePlayer(MathUtils.Rand, monId, DataManager.Instance.StartLevel, -1, DataManager.Instance.StartPersonality);
            newChar.Nickname = name;
            newChar.IsFounder = true;
            DataManager.Instance.Save.ActiveTeam.Players.Add(newChar);

            try
            {
                LuaEngine.Instance.OnNewGame();
                if (DataManager.Instance.Save.ActiveTeam.Players.Count == 0)
                    throw new Exception("Script generated an invalid team!");
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex);
            }

            yield return CoroutineManager.Instance.StartCoroutine(GameManager.Instance.MoveToZone(DataManager.Instance.StartMap));
        }

        private static IEnumerator<YieldInstruction> DefaultBegin()
        {
            yield return CoroutineManager.Instance.StartCoroutine(GameManager.Instance.FadeOut(false));

            GameManager.Instance.NewGamePlus(MathUtils.Rand.NextUInt64());

            yield return CoroutineManager.Instance.StartCoroutine(GameManager.Instance.MoveToZone(DataManager.Instance.StartMap));
        }

    }
}
