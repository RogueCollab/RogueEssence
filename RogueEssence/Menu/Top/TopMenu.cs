using System;
using System.Collections.Generic;
using System.IO;
using RogueElements;
using RogueEssence.Data;
using RogueEssence.Dungeon;
using RogueEssence.Script;
using RogueEssence.Content;

namespace RogueEssence.Menu
{
    public class TopMenu : SingleStripMenu
    {
        SummaryMenu titleMenu;

        public override bool CanMenu { get { return false; } }
        public override bool CanCancel { get { return false; } }
        
        public TopMenu() : this(MenuLabel.TOP_MENU) { }
        public TopMenu(string label)
        {
            Label = label;
            bool inQuest = PathMod.Quest.IsValid();
            List<MenuTextChoice> choices = new List<MenuTextChoice>();

            if (DataManager.Instance.Save != null)
            {
                if (DataManager.Instance.Save.Rescue != null)
                {
                    if (DataManager.Instance.Save.Rescue.SOS.RescuingTeam == null)
                        choices.Add(new MenuTextChoice(MenuLabel.TOP_RESCUE, Text.FormatKey("MENU_TOP_RESCUE_AWAIT"), () => { MenuManager.Instance.AddMenu(new RescueMenu(), false); }));
                    else
                        choices.Add(new MenuTextChoice(MenuLabel.TOP_CONTINUE, Text.FormatKey("MENU_TOP_CONTINUE"), () => { Continue(DataManager.Instance.Save.Rescue.SOS); }));
                }
                else
                    choices.Add(new MenuTextChoice(MenuLabel.TOP_CONTINUE, Text.FormatKey("MENU_TOP_CONTINUE"), () => { Continue(null); }));
            }
            else
                choices.Add(new MenuTextChoice(MenuLabel.TOP_NEW, Text.FormatKey("MENU_TOP_NEW"), () => { StartFlow(MonsterID.Invalid, null, -1); }));

            if (DiagManager.Instance.DevMode || DataManager.Instance.Save != null)
                choices.Add(new MenuTextChoice(MenuLabel.TOP_ROGUE, Text.FormatKey("MENU_TOP_ROGUE"), () => { MenuManager.Instance.AddMenu(new RogueMenu(), false); }));


            if (DataManager.Instance.FoundRecords(PathMod.ModSavePath(DataManager.REPLAY_PATH), DataManager.REPLAY_EXTENSION) || DataManager.Instance.Save != null || RecordHeaderData.LoadHighScores().Count > 0)
                choices.Add(new MenuTextChoice(MenuLabel.TOP_RECORD, Text.FormatKey("MENU_TOP_RECORD"), () => { MenuManager.Instance.AddMenu(new RecordsMenu(), false); }));
            choices.Add(new MenuTextChoice(MenuLabel.TOP_OPTIONS, Text.FormatKey("MENU_OPTIONS_TITLE"), () => { MenuManager.Instance.AddMenu(new OptionsMenu(), false); }));

            if (!inQuest)
            {
                string[] questsPath = Directory.GetDirectories(PathMod.MODS_PATH);
                if (PathMod.GetEligibleMods(PathMod.ModType.Quest).Count > 0)
                    choices.Add(new MenuTextChoice(MenuLabel.TOP_QUEST, Text.FormatKey("MENU_QUESTS_TITLE"), () => { MenuManager.Instance.AddMenu(new QuestsMenu(), false); }));
            }
            else
                choices.Add(new MenuTextChoice(MenuLabel.TOP_QUEST_EXIT, Text.FormatKey("MENU_QUESTS_EXIT"), exitQuest));

            string[] modsPath = Directory.GetDirectories(PathMod.MODS_PATH);
            if (PathMod.GetEligibleMods(PathMod.ModType.Mod).Count > 0)
                choices.Add(new MenuTextChoice(MenuLabel.TOP_MODS, Text.FormatKey("MENU_MODS_TITLE"), () => { MenuManager.Instance.AddMenu(new ModsMenu(), false); }));

            choices.Add(new MenuTextChoice(MenuLabel.TOP_QUIT, Text.FormatKey("MENU_QUIT_GAME"), exitGame));

            Initialize(new Loc(16, 16), CalculateChoiceLength(choices, 72), choices.ToArray(), 0);

            titleMenu = new SummaryMenu(MenuLabel.TOP_TITLE_SUMMARY, Rect.FromPoints(new Loc(Bounds.End.X + 16, 16), new Loc(GraphicsManager.ScreenWidth - 16, 16 + LINE_HEIGHT + GraphicsManager.MenuBG.TileHeight * 2)));
            MenuText title = new MenuText(MenuLabel.TOP_TITLE_SUMMARY, PathMod.Quest.GetMenuName(), new Loc(titleMenu.Bounds.Width / 2, GraphicsManager.MenuBG.TileHeight), DirH.None);
            titleMenu.Elements.Add(title);
            titleMenu.Visible = PathMod.Quest.IsValid();
            SummaryMenus.Add(titleMenu);
        }

        protected override void MenuPressed()
        {

        }

        protected override void Canceled()
        {

        }

        private void exitQuest()
        {
            List<int> loadOrder = new List<int>();
            List<(ModRelationship, List<ModHeader>)> loadErrors = new List<(ModRelationship, List<ModHeader>)>();
            PathMod.ValidateModLoad(ModHeader.Invalid, PathMod.Mods, loadOrder, loadErrors);
            if (loadErrors.Count > 0)
            {
                MenuManager.Instance.AddMenu(new ModLogMenu(loadErrors), false);
                return;
            }
            MenuManager.Instance.ClearMenus();
            GameManager.Instance.SceneOutcome = GameManager.Instance.MoveToQuest(ModHeader.Invalid, PathMod.Mods, loadOrder);
        }


        private void exitGame()
        {
            GameBase.CurrentPhase = GameBase.LoadPhase.Unload;
        }


        private static void cannotRead(string path)
        {
            MenuManager.Instance.AddMenu(MenuManager.Instance.CreateDialogue(Text.FormatKey("DLG_ERR_READ_FILE"),
                Text.FormatKey("DLG_ERR_READ_FILE_FALLBACK", PathMod.GetRelativePath(PathMod.APP_PATH, path))), false);
        }

        public static void Continue(SOSMail rescueMail)
        {
            //check for presence of a main save-quicksave
            ReplayData replay = null;
            string recordDir = PathMod.ModSavePath(DataManager.SAVE_PATH, DataManager.QUICKSAVE_FILE_PATH);
            if (File.Exists(recordDir))
            {
                replay = DataManager.Instance.LoadReplay(recordDir, true);
                if (replay == null || replay.States.Count == 0)
                {
                    MenuManager.Instance.AddMenu(MenuManager.Instance.CreateDialogue(() => { continueMain(rescueMail); }, Text.FormatKey("DLG_ERR_READ_QUICKSAVE")), false);
                    return;
                }
            }
            if (replay != null)
            {
                MenuManager.Instance.ClearMenus();
                List<ModDiff> replayDiffs = replay.States[0].Save.GetModDiffs();
                if (replayDiffs.Count > 0)
                    MenuManager.Instance.AddMenu(MenuManager.Instance.CreateDialogue(Text.FormatKey("DLG_FILE_VERSION_DIFF")), false);
                GameManager.Instance.SceneOutcome = continueReplay(replay, rescueMail);
                return;
            }

            //otherwise, load just the main save
            continueMain(rescueMail);
        }

        private static void continueMain(SOSMail rescueMail)
        {
            List<ModDiff> modDiffs = DataManager.Instance.Save.GetModDiffs();
            if (modDiffs.Count > 0)
                DiagManager.Instance.LogInfo("Loading with version diffs:");

            List<ModDiff> removedMods = new List<ModDiff>();
            foreach (ModDiff diff in modDiffs)
            {
                DiagManager.Instance.LogInfo(String.Format("{0}\t{1}\t{2}->{3}", diff.UUID, diff.Name, diff.OldVersion == null ? "Added" : diff.OldVersion.ToString(), diff.NewVersion == null ? "Removed" : diff.NewVersion.ToString()));
                if (diff.NewVersion == null)
                    removedMods.Add(diff);
            }

            if (removedMods.Count > 0)
            {
                DialogueChoice[] choices = new DialogueChoice[2];
                choices[0] = new DialogueChoice(Text.FormatKey("DLG_CHOICE_YES"), () => { attemptLoadMain(); });
                choices[1] = new DialogueChoice(Text.FormatKey("DLG_CHOICE_NO"), () => {  });
                MenuManager.Instance.AddMenu(new ModDiffDialog(Text.FormatKey("DLG_ASK_LOAD_UPGRADE"), Text.FormatKey("MENU_MODS_MISSING"), removedMods, false, choices, 0, 1), false);
            }
            else
                attemptLoadMain();
        }

        private static void attemptLoadMain()
        {
            //then, we should load a main save instead
            GameState state = DataManager.Instance.LoadMainGameState(true);
            if (state == null)
            {
                cannotRead(DataManager.SAVE_PATH + DataManager.SAVE_FILE_PATH);
                return;
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
                DataManager.Instance.Save.ResumeSession(DataManager.Instance.CurrentReplay);
                DataManager.Instance.ResumePlay(DataManager.Instance.CurrentReplay, DataManager.Instance.Save.SessionStartTime);
                DataManager.Instance.CurrentReplay = null;
                DataManager.Instance.Save.UpdateOptions();

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

            yield return CoroutineManager.Instance.StartCoroutine(mainState.Save.LoadedWithoutQuicksave());

            MainProgress mainSave = mainState.Save as MainProgress;
            DataManager.Instance.SetProgress(mainSave);
            mainSave.MergeDataTo(mainSave);
            ZoneManager.LoadFromState(mainState.Zone);
            LuaEngine.Instance.UpdateZoneInstance();

            //upgrade here
            if (DataManager.Instance.Save.IsOldVersion())
                LuaEngine.Instance.OnUpgrade();

            DataManager.Instance.Save.UpdateVersion();
            DataManager.Instance.Save.UpdateOptions();

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
            if (String.IsNullOrEmpty(monId.Species) || backPhase == 0)
            {
                if (DataManager.Instance.Start.Chars.Count > 1)
                {
                    int startIndex = 0;
                    if (backPhase == 0)
                        startIndex = DataManager.Instance.Start.Chars.FindIndex(start => start.ID == monId);
                    MenuManager.Instance.AddMenu(new ChooseMonsterMenu(Text.FormatKey("MENU_CHARA_CHOICE_TITLE"), DataManager.Instance.Start.Chars, startIndex, (int index) =>
                    {
                        string newName = null;
                        if (DataManager.Instance.Start.Chars[index].Name != "")
                            newName = DataManager.Instance.Start.Chars[index].Name;
                        StartFlow(DataManager.Instance.Start.Chars[index].ID, newName, -1);
                    }, () => { }), false);
                    return;
                }
                else if (backPhase == 0)
                    return;
                else if (DataManager.Instance.Start.Chars.Count == 1)
                {
                    monId = DataManager.Instance.Start.Chars[0].ID;
                    if (DataManager.Instance.Start.Chars[0].Name != "")
                        name = DataManager.Instance.Start.Chars[0].Name;
                }
                else
                {
                    MenuManager.Instance.ClearMenus();
                    GameManager.Instance.SceneOutcome = Begin(DataManager.Instance.DefaultMonsterID, "");
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
                    List<DialogueChoice> choices = new List<DialogueChoice>();
                    foreach (Gender gender in genders)
                    {
                        string menuGender = Text.FormatKey("MENU_GENDERLESS");
                        if (gender == Gender.Male)
                            menuGender = Text.FormatKey("MENU_BOY");
                        if (gender == Gender.Female)
                            menuGender = Text.FormatKey("MENU_GIRL");

                        if (backPhase == 1 && monId.Gender == gender)
                            startIndex = choices.Count;
                        choices.Add(new DialogueChoice(menuGender, () =>
                        {
                            StartFlow(new MonsterID(monId.Species, monId.Form, monId.Skin, gender), name, -1);
                        }));
                    }
                    choices.Add(new DialogueChoice(Text.FormatKey("MENU_CANCEL"), () => { }));

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
            DataManager.Instance.Save.UpdateVersion();
            DataManager.Instance.Save.UpdateOptions();
            DataManager.Instance.Save.StartDate = String.Format("{0:yyyy-MM-dd_HH-mm-ss}", DateTime.Now);
            DataManager.Instance.Save.ActiveTeam = new ExplorerTeam();

            Character newChar = DataManager.Instance.Save.ActiveTeam.CreatePlayer(MathUtils.Rand, monId, DataManager.Instance.Start.Level, "", DataManager.Instance.Start.Personality);
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

            yield return CoroutineManager.Instance.StartCoroutine(GameManager.Instance.MoveToZone(DataManager.Instance.Start.Map, true, false));
        }

        private static IEnumerator<YieldInstruction> DefaultBegin()
        {
            yield return CoroutineManager.Instance.StartCoroutine(GameManager.Instance.FadeOut(false));

            GameManager.Instance.NewGamePlus(MathUtils.Rand.NextUInt64());

            yield return CoroutineManager.Instance.StartCoroutine(GameManager.Instance.MoveToZone(DataManager.Instance.Start.Map, true, false));
        }

    }
}
