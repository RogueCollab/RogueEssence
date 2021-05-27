using System;
using System.Collections.Generic;
using RogueElements;
using RogueEssence.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RogueEssence.Data;
using RogueEssence.Menu;
using RogueEssence.Dungeon;
using RogueEssence.Ground;
using RogueEssence.Script;
using RogueEssence.Dev;


namespace RogueEssence
{
    public class GameManager
    {
        public enum GameSpeed
        {
            Eighth = -3,
            Fourth = -2,
            Half = -1,
            Normal = 0,
            Double = 1,
            Quadruple = 2,
            Octuple = 3
        }
        public enum FanfarePhase
        {
            None,
            PhaseOut,
            Wait,
            PhaseIn
        }

        private static GameManager instance;
        public static void InitInstance()
        {
            instance = new GameManager();
        }
        public static GameManager Instance { get { return instance; } }

        public InputManager MetaInputManager;
        public InputManager InputManager;

        public BaseScene CurrentScene;

        public IEnumerator<YieldInstruction> SceneOutcome;

        public GameSpeed DebugSpeed;
        public bool Paused;
        public bool AdvanceFrame;
        public bool ShowDebug;
        public string DebugUI;

        public bool FrameProcessed;

        private int totalErrorCount;
        private bool thisFrameErrored;
        private int framesErrored;
        private int longestFrame;

        private float fadeAmount;
        private bool fadeWhite;
        
        private float titleFadeAmount;
        private string fadedTitle;

        private float bgFadeAmount;
        private BGAnimData fadedBG;

        public string Song;
        public string NextSong;
        public FrameTick MusicFadeTime;
        public const int MUSIC_FADE_TOTAL = 120;
        public string QueuedFanfare;
        public FanfarePhase CurrentFanfarePhase;
        public FrameTick FanfareTime;
        public const int FANFARE_FADE_START = 3;
        public const int FANFARE_FADE_END = 40;
        public const int FANFARE_WAIT_EXTRA = 20;


        public GameManager()
        {
            fadedTitle = "";
            fadedBG = new BGAnimData();
            DebugUI = "";

            MetaInputManager = new InputManager();
            InputManager = new InputManager();

            DiagManager.Instance.SetErrorListener(OnError);
        }

        public void BattleSE(string newSE)
        {
            if (newSE != "")
                SE("Battle/" + newSE);
        }

        public void SE(string newSE)
        {
            if (DataManager.Instance.Loading != DataManager.LoadMode.None)
                return;

            if (System.IO.File.Exists(PathMod.ModPath(GraphicsManager.SOUND_PATH + newSE + ".ogg")))
                SoundManager.PlaySound(PathMod.ModPath(GraphicsManager.SOUND_PATH + newSE + ".ogg"), 1);
        }

        public IEnumerator<YieldInstruction> WaitFanfareEnds()
        {
            yield return new WaitWhile(() => { return CurrentFanfarePhase != FanfarePhase.None; });
        }

        /// <summary>
        /// Plays a sound effect and waits until it's over
        /// </summary>
        /// <param name="newSE"></param>
        /// <returns></returns>
        public IEnumerator<YieldInstruction> WaitSE(string newSE)
        {
            if (DataManager.Instance.Loading != DataManager.LoadMode.None)
                yield break;

            if (System.IO.File.Exists(PathMod.ModPath(GraphicsManager.SOUND_PATH + newSE + ".ogg")))
            {
                int pauseFrames = SoundManager.PlaySound(PathMod.ModPath(GraphicsManager.SOUND_PATH + newSE + ".ogg"));
                yield return new WaitForFrames(pauseFrames);
            }
        }

        public void Fanfare(string newSE)
        {
            if (DataManager.Instance.Loading != DataManager.LoadMode.None)
                return;
            if (Song == "" || NextSong == "")
            {
                SE(newSE);
                return;
            }

            if (String.IsNullOrEmpty(QueuedFanfare))//assume no fanfares happen within the same period
            {
                if (CurrentFanfarePhase == FanfarePhase.None)//begin the brief fade out if playing music as normal
                {
                    CurrentFanfarePhase = FanfarePhase.PhaseOut;
                    FanfareTime = FrameTick.FromFrames(FANFARE_FADE_START);
                    QueuedFanfare = newSE;
                }
                else if (CurrentFanfarePhase == FanfarePhase.PhaseIn)//fade from the partial progress
                {
                    CurrentFanfarePhase = FanfarePhase.PhaseOut;
                    FanfareTime = FrameTick.FromFrames((FANFARE_FADE_END - FanfareTime.DivOf(FANFARE_FADE_END)) * FANFARE_FADE_START / FANFARE_FADE_END);
                    QueuedFanfare = newSE;
                }
            }
        }

        public void SetFade(bool faded, bool useWhite)
        {
            fadeAmount = faded ? 1f : 0f;
            fadeWhite = useWhite;
        }

        public bool IsFading()
        {
            return fadeAmount > 0f;
        }
        public bool IsFaded()
        {
            return fadeAmount == 1f;
        }

        public IEnumerator<YieldInstruction> FadeIn(int totalTime = 20)
        {
            return fade(true, fadeWhite, totalTime);
        }

        public IEnumerator<YieldInstruction> FadeOut(bool useWhite, int totalTime = 20)
        {
            return fade(false, useWhite, totalTime);
        }
        private IEnumerator<YieldInstruction> fade(bool fadeIn, bool useWhite, int totalTime)
        {
            if (fadeIn && fadeAmount == 0f)
                yield break;
            if (!fadeIn && fadeAmount == 1f)
            {
                SetFade(true, useWhite);
                yield break;
            }

            int fadeTime = 10 + ModifyBattleSpeed(totalTime);
            int currentFadeTime = fadeTime;
            while (currentFadeTime > 0)
            {
                currentFadeTime--;
                float amount = 0f;
                if (fadeIn)
                    amount = ((float)currentFadeTime / (float)fadeTime);
                else
                    amount = ((float)(fadeTime - currentFadeTime) / (float)fadeTime);
                fadeAmount = amount;
                fadeWhite = useWhite;
                yield return new WaitForFrames(1);
            }
        }

        public IEnumerator<YieldInstruction> FadeTitle(bool fadeIn, string title, int totalTime = 20)
        {
            if (fadeIn)
                fadedTitle = title;
            int fadeTime = 10 + ModifyBattleSpeed(totalTime);
            long currentFadeTime = fadeTime;
            while (currentFadeTime > 0)
            {
                currentFadeTime--;
                float amount = 0f;
                if (fadeIn)
                    amount = ((float)currentFadeTime / (float)fadeTime);
                else
                    amount = ((float)(fadeTime - currentFadeTime) / (float)fadeTime);
                titleFadeAmount = 1f - amount;
                yield return new WaitForFrames(1);
            }
            if (!fadeIn)
                fadedTitle = "";
        }


        public IEnumerator<YieldInstruction> FadeBG(bool fadeIn, BGAnimData bg, int totalTime = 20)
        {
            if (fadeIn)
                fadedBG = bg;
            int fadeTime = 10 + ModifyBattleSpeed(totalTime);
            long currentFadeTime = fadeTime;
            while (currentFadeTime > 0)
            {
                currentFadeTime--;
                float amount = 0f;
                if (fadeIn)
                    amount = ((float)currentFadeTime / (float)fadeTime);
                else
                    amount = ((float)(fadeTime - currentFadeTime) / (float)fadeTime);
                bgFadeAmount = 1f - amount;
                yield return new WaitForFrames(1);
            }
            if (!fadeIn)
                fadedBG = new BGAnimData();
        }

        public int ModifyBattleSpeed(int waitTime, Loc origin)
        {
            if (DungeonScene.Instance.FocusedCharacter.IsInSightBounds(origin))
                return ModifyBattleSpeed(waitTime, Settings.BattleSpeed.Fast);
            else
                return 0;
        }

        public int ModifyBattleSpeed(int waitTime, Loc origin, Settings.BattleSpeed minSpeed)
        {
            if (DungeonScene.Instance.FocusedCharacter.IsInSightBounds(origin))
                return ModifyBattleSpeed(waitTime, minSpeed);
            else
                return 0;
        }

        public int ModifyBattleSpeed(int waitTime)
        {
            return ModifyBattleSpeed(waitTime, Settings.BattleSpeed.Fast);
        }

        public int ModifyBattleSpeed(int waitTime, Settings.BattleSpeed minSpeed)
        {
            if (DiagManager.Instance.CurSettings.BattleFlow > minSpeed)
                return 0;
            else if (DiagManager.Instance.CurSettings.BattleFlow == Settings.BattleSpeed.VeryFast || DiagManager.Instance.CurSettings.BattleFlow == Settings.BattleSpeed.Fast)
                return waitTime / 2;
            else if (DiagManager.Instance.CurSettings.BattleFlow == Settings.BattleSpeed.Slow)
                return waitTime * 3 / 2;
            else if (DiagManager.Instance.CurSettings.BattleFlow == Settings.BattleSpeed.VerySlow)
                return waitTime * 2;
            else
                return waitTime;
        }

        public void BGM(string newBGM, bool fade)
        {
            if (DataManager.Instance.Loading != DataManager.LoadMode.None)
                return;

            if (Song != newBGM || (Song == newBGM && NextSong != null && NextSong != newBGM))
            {
                if (String.IsNullOrEmpty(Song) || !fade)//if the current song is empty, or the game doesn't want to fade it
                {
                    //immediately start
                    MusicFadeTime = FrameTick.Zero;
                }
                else if (NextSong != null)
                {
                    //do nothing, and watch it tick down
                }
                else
                {
                    //otherwise, set up the tick-down
                    MusicFadeTime = FrameTick.FromFrames(MUSIC_FADE_TOTAL);
                }
                NextSong = newBGM;
            }
        }

        public bool IsInGame()
        {
            return (GameManager.Instance.CurrentScene == DungeonScene.Instance &&
                DungeonScene.Instance.ActiveTeam.Players.Count > 0 && DungeonScene.Instance.FocusedCharacter != null);
        }

        public void Begin()
        {

            SoundManager.BGMBalance = DiagManager.Instance.CurSettings.BGMBalance * 0.1f;
            SoundManager.SEBalance = DiagManager.Instance.CurSettings.SEBalance * 0.1f;

            //coroutines.Clear();
            MoveToScene(new SplashScene());
            CoroutineManager.Instance.StartCoroutine(ScreenMainCoroutine()); //Create our main context
        }

        public IEnumerator<YieldInstruction> ScreenMainCoroutine()
        {
            while (true)
            {
                while (SceneOutcome == null)
                    yield return CoroutineManager.Instance.StartCoroutine(CurrentScene.ProcessInput());

                IEnumerator<YieldInstruction> outcome = SceneOutcome;
                SceneOutcome = null;
                yield return CoroutineManager.Instance.StartCoroutine(outcome);
            }
        }

        public IEnumerator<YieldInstruction> RestartToTitle()
        {
            cleanup();
            reInit();
            MoveToScene(new TitleScene(false));
            yield return CoroutineManager.Instance.StartCoroutine(FadeIn());
        }


        public IEnumerator<YieldInstruction> SetMod(string modPath, bool fade)
        {
            if (fade)
                yield return CoroutineManager.Instance.StartCoroutine(FadeOut(false));

            cleanup();
            PathMod.Mod = modPath;
            reInit();
            TitleScene.TitleMenuSaveState = null;
            MoveToScene(new TitleScene(true));
            //clean up and reload all caches
            GraphicsManager.ReloadStatic();
            DataManager.Instance.InitData();
            LuaEngine.Instance.OnDataLoad();
            DataManager.InitSaveDirs();
            //call data editor's load method to reload the dropdowns
            DiagManager.Instance.DevEditor.ReloadData(DataManager.DataType.All);

            if (fade)
                yield return CoroutineManager.Instance.StartCoroutine(FadeIn());
        }

        private void cleanup()
        {
            DataManager.Instance.SetProgress(null);
            ZoneManager.Instance.Cleanup();
        }
        private void reInit()
        {
            //remove all state variables
            DungeonScene.InitInstance();
            GroundScene.InitInstance();
            LuaEngine.Instance.Reset();
            LuaEngine.Instance.ReInit();
        }

        private IEnumerator<YieldInstruction> exitMap(BaseScene nextScene)
        {
            if (CurrentScene == DungeonScene.Instance)
                yield return CoroutineManager.Instance.StartCoroutine(DungeonScene.Instance.ExitFloor());
            else if (CurrentScene == GroundScene.Instance)
                yield return CoroutineManager.Instance.StartCoroutine(GroundScene.Instance.ExitGround());

            CurrentScene.Exit();

            //Notify Script engine; swap out old scene
            if (nextScene != DungeonScene.Instance && CurrentScene == DungeonScene.Instance)
                LuaEngine.Instance.OnDungeonModeEnd();
            else if (nextScene != GroundScene.Instance && CurrentScene == GroundScene.Instance)
                LuaEngine.Instance.OnGroundModeEnd();
        }

        public IEnumerator<YieldInstruction> MoveToEditor(bool newGround, string name)
        {
            BaseScene destScene = newGround ? (BaseScene)GroundEditScene.Instance : (BaseScene)DungeonEditScene.Instance;

            ZoneLoc destLoc = ZoneLoc.Invalid;
            if (ZoneManager.Instance.CurrentZoneID > -1)
                destLoc = new ZoneLoc(ZoneManager.Instance.CurrentZoneID, ZoneManager.Instance.CurrentMapID);

            yield return CoroutineManager.Instance.StartCoroutine(exitMap(destScene));

            ZoneManager.Instance.MoveToDevZone(newGround, name);

            //Transparency mode
            MenuBase.Transparent = !newGround;

            //switch in new scene
            MoveToScene(destScene);

            SetFade(false, false);

            if (newGround)
                GroundEditScene.Instance.EnterGroundEdit(0);
            else
                DungeonEditScene.Instance.EnterMapEdit(0);
            if (DataManager.Instance.Save != null)
                DataManager.Instance.Save.NextDest = destLoc;
            yield break;
        }

        public IEnumerator<YieldInstruction> ReturnToEditor()
        {
            if (ZoneManager.Instance.CurrentZone.CurrentMapID.Segment == -1)//ground
                yield return CoroutineManager.Instance.StartCoroutine(MoveToEditor(true, ZoneManager.Instance.CurrentZone.CurrentGround.AssetName));
            else
                yield return CoroutineManager.Instance.StartCoroutine(MoveToEditor(false, ZoneManager.Instance.CurrentZone.CurrentMap.AssetName));
        }

        public IEnumerator<YieldInstruction> MoveToZone(ZoneLoc destId, bool forceNewZone = false, bool preserveMusic = false)
        {
            //if we're in a test map, return to editor
            if (ZoneManager.Instance.InDevZone)
            {
                yield return CoroutineManager.Instance.StartCoroutine(ReturnToEditor());
                yield break;
            }

            bool newGround = (destId.StructID.Segment <= -1);
            BaseScene destScene = newGround ? (BaseScene)GroundScene.Instance : DungeonScene.Instance;

            yield return CoroutineManager.Instance.StartCoroutine(exitMap(destScene));

            //switch location
            bool sameZone = destId.ID == ZoneManager.Instance.CurrentZoneID;
            bool sameSegment = sameZone && destId.StructID.Segment == ZoneManager.Instance.CurrentMapID.Segment;
            if (sameZone && !forceNewZone)
                ZoneManager.Instance.CurrentZone.SetCurrentMap(destId.StructID);
            else
            {
                ZoneManager.Instance.MoveToZone(destId.ID, destId.StructID, unchecked(DataManager.Instance.Save.Rand.FirstSeed + (ulong)destId.ID));//NOTE: there are better ways to seed a multi-dungeon adventure
                yield return CoroutineManager.Instance.StartCoroutine(ZoneManager.Instance.CurrentZone.OnInit());
            }

            if (!sameSegment || forceNewZone)
                yield return CoroutineManager.Instance.StartCoroutine(ZoneManager.Instance.CurrentZone.OnEnterSegment());

            //switch in new scene
            MoveToScene(destScene);

            yield return CoroutineManager.Instance.StartCoroutine(moveToZoneInit(destId.EntryPoint, newGround, preserveMusic));
        }

        private IEnumerator<YieldInstruction> moveToZoneInit(int entryPoint, bool newGround, bool preserveMusic)
        {
            //Transparency mode
            MenuBase.Transparent = !newGround;

            if (newGround && CurrentScene != GroundScene.Instance)
                LuaEngine.Instance.OnGroundModeBegin();
            else if (!newGround && CurrentScene != DungeonScene.Instance)
                LuaEngine.Instance.OnDungeonModeBegin();


            if (newGround)
            {
                if (!preserveMusic)
                    BGM(ZoneManager.Instance.CurrentGround.Music, true);

                GroundScene.Instance.EnterGround(entryPoint);
                yield return CoroutineManager.Instance.StartCoroutine(GroundScene.Instance.InitGround());
                //no fade; the script handles that itself
                yield return CoroutineManager.Instance.StartCoroutine(GroundScene.Instance.BeginGround());
            }
            else
            {
                if (!preserveMusic)
                    BGM(ZoneManager.Instance.CurrentMap.Music, true);

                DungeonScene.Instance.EnterFloor(entryPoint);
                yield return CoroutineManager.Instance.StartCoroutine(DungeonScene.Instance.InitFloor());

                if (IsFaded())
                {
                    if (ZoneManager.Instance.CurrentMap.DropTitle)
                    {
                        yield return CoroutineManager.Instance.StartCoroutine(FadeTitle(true, ZoneManager.Instance.CurrentMap.Name.ToLocal()));
                        yield return new WaitForFrames(30);
                        yield return CoroutineManager.Instance.StartCoroutine(FadeTitle(false, ""));
                    }
                }

                yield return CoroutineManager.Instance.StartCoroutine(DungeonScene.Instance.BeginFloor());
            }
            DataManager.Instance.Save.NextDest = ZoneLoc.Invalid;
        }




        /// <summary>
        /// Enter a ground map by name, and makes the player spawn at the specified named marker
        /// </summary>
        /// <param name="mapname"></param>
        /// <param name="entrypoint"></param>
        public IEnumerator<YieldInstruction> MoveToGround(string mapname, string entrypoint, bool preserveMusic)
        {
            //if we're in a test map, return to editor
            if (ZoneManager.Instance.InDevZone)
            {
                yield return CoroutineManager.Instance.StartCoroutine(ReturnToEditor());
                yield break;
            }

            yield return CoroutineManager.Instance.StartCoroutine(exitMap(GroundScene.Instance));

            ZoneManager.Instance.CurrentZone.SetCurrentGround(mapname);
            if (ZoneManager.Instance.CurrentGround == null)
                throw new Exception(String.Format("GroundScene.MoveToGround(): Failed to load map {0}!", mapname));


            //Transparency mode
            MenuBase.Transparent = false;

            //switch in new scene
            if (CurrentScene != GroundScene.Instance)
            {
                MoveToScene(GroundScene.Instance);
                LuaEngine.Instance.OnGroundModeBegin();
            }

            if (!preserveMusic)
                BGM(ZoneManager.Instance.CurrentGround.Music, true);

            GroundScene.Instance.EnterGround(entrypoint);

            yield return CoroutineManager.Instance.StartCoroutine(GroundScene.Instance.InitGround());
            //no fade; the script handles that itself
            yield return CoroutineManager.Instance.StartCoroutine(GroundScene.Instance.BeginGround());
        }



        public IEnumerator<YieldInstruction> BeginRescue(string sosPath)
        {
            SOSMail sos = (SOSMail)DataManager.LoadRescueMail(sosPath);
            //specify seed with a random byte, verify goals work in all cases
            sos.RescueSeed = (byte)(MathUtils.Rand.NextUInt64() % byte.MaxValue);
            ReRandom rerand = new ReRandom(sos.Seed);
            for (int ii = 0; ii < sos.RescueSeed; ii++)
                rerand.NextUInt64();


            yield return CoroutineManager.Instance.StartCoroutine(BeginGame(sos.Goal.ID, rerand.NextUInt64(), GameProgress.DungeonStakes.Risk, true, false));

            //set a rescuing boolean so that rescuability is disabled
            //must also set a goal variable somewhere
            //actually, just save the entire SOS mail somewhere as the current SOS
            DataManager.Instance.Save.Rescue = new RescueState(sos, true);
            //this is after saving the fail file, but before saving the first state of the replay
            yield return CoroutineManager.Instance.StartCoroutine(BeginSegment(new ZoneLoc(sos.Goal.ID, new SegLoc(0, 0))));


            //must also set script variables for dungeon if they matter
            //when this mission ends, all scripts must move the player back to the rescue start location...
            //if the mission was a success, the replay must be packaged into an AOK mail
        }

        public IEnumerator<YieldInstruction> BeginGameInSegment(ZoneLoc nextZone, GameProgress.DungeonStakes stakes, bool recorded, bool silentRestrict)
        {
            yield return CoroutineManager.Instance.StartCoroutine(BeginGame(nextZone.ID, MathUtils.Rand.NextUInt64(), stakes, recorded, silentRestrict));
            yield return CoroutineManager.Instance.StartCoroutine(BeginSegment(nextZone));
        }
        public IEnumerator<YieldInstruction> BeginGame(int zoneID, ulong seed, GameProgress.DungeonStakes stakes, bool recorded, bool silentRestrict)
        {
            //initiate the adventure
            DataManager.Instance.CurrentReplay = null;
            yield return CoroutineManager.Instance.StartCoroutine(DataManager.Instance.Save.BeginGame(zoneID, seed, stakes, recorded, silentRestrict));
        }
        public IEnumerator<YieldInstruction> BeginSegment(ZoneLoc nextZone)
        {
            DataManager.Instance.Save.NextDest = nextZone;
            if (DataManager.Instance.RecordingReplay)
                DataManager.Instance.LogState();

            SceneOutcome = MoveToZone(nextZone);
            yield break;
        }

        public IEnumerator<YieldInstruction> EndSegment(GameProgress.ResultType result)
        {
            if (ZoneManager.Instance.InDevZone)
            {
                yield return CoroutineManager.Instance.StartCoroutine(ReturnToEditor());
                yield break;
            }

            if ((result == GameProgress.ResultType.Failed || result == GameProgress.ResultType.Downed || result == GameProgress.ResultType.TimedOut)
                && DataManager.Instance.CurrentReplay == null)
            {
                SE("Menu/Skip");
                yield return CoroutineManager.Instance.StartCoroutine(MenuManager.Instance.ProcessMenuCoroutine(new MsgLogMenu()));
            }

            BGM("", true);

            yield return CoroutineManager.Instance.StartCoroutine(FadeOut(false));

            yield return new WaitForFrames(40);

            if (DataManager.Instance.CurrentReplay != null)
            {
                //try to get the game state.  if we can't get any states, then we must have quicksaved here
                if (DataManager.Instance.CurrentReplay.CurrentState < DataManager.Instance.CurrentReplay.States.Count)
                {
                    GameState state = DataManager.Instance.CurrentReplay.ReadState();
                    DataManager.Instance.SetProgress(state.Save);
                    LuaEngine.Instance.LoadSavedData(DataManager.Instance.Save); //notify script engine
                    ZoneManager.LoadFromState(state.Zone);

                    SceneOutcome = MoveToZone(DataManager.Instance.Save.NextDest);
                }
                else
                {
                    //check for a rescue input
                    GameAction rescued = null;
                    if (DataManager.Instance.CurrentReplay.CurrentAction < DataManager.Instance.CurrentReplay.Actions.Count)
                    {
                        GameAction nextAction = DataManager.Instance.CurrentReplay.ReadCommand();
                        if (nextAction.Type == GameAction.ActionType.Rescue)
                            rescued = nextAction;
                        else if (result != GameProgress.ResultType.Unknown)//we shouldn't be hitting this point!  give an error notification!
                            yield return CoroutineManager.Instance.StartCoroutine(MenuManager.Instance.SetDialogue(Text.FormatKey("DLG_REPLAY_DESYNC")));
                    }

                    if (rescued != null) //run the action
                        yield return CoroutineManager.Instance.StartCoroutine(DungeonScene.Instance.ProcessRescue(rescued, null));
                    else if (DataManager.Instance.Save.Rescue != null && !DataManager.Instance.Save.Rescue.Rescuing)
                    {
                        //resuming a game that was just rescued
                        DataManager.Instance.ResumePlay(DataManager.Instance.CurrentReplay.RecordDir, DataManager.Instance.CurrentReplay.QuicksavePos);
                        DataManager.Instance.CurrentReplay = null;
                        DataManager.Instance.Loading = DataManager.LoadMode.None;
                        SOSMail mail = DataManager.Instance.Save.Rescue.SOS;
                        //and then run the action

                        rescued = new GameAction(GameAction.ActionType.Rescue, Dir8.None);
                        rescued.AddArg(mail.OfferedItem.IsMoney ? 1 : 0);
                        rescued.AddArg(mail.OfferedItem.Value);
                        rescued.AddArg(mail.OfferedItem.HiddenValue);

                        rescued.AddArg(mail.RescuedBy.Length);
                        for (int ii = 0; ii < mail.RescuedBy.Length; ii++)
                            rescued.AddArg(mail.RescuedBy[ii]);

                        DataManager.Instance.LogPlay(rescued);
                        yield return CoroutineManager.Instance.StartCoroutine(DungeonScene.Instance.ProcessRescue(rescued, mail));
                    }
                    else
                    {
                        if (DataManager.Instance.Loading == DataManager.LoadMode.Rescuing)
                        {
                            if (result == GameProgress.ResultType.Rescue)
                            {
                                //mark as verified
                                RescueMenu menu = (RescueMenu)TitleScene.TitleMenuSaveState[TitleScene.TitleMenuSaveState.Count - 1];
                                menu.Verified = true;
                            }
                            //default is failure to verify
                            yield return CoroutineManager.Instance.StartCoroutine(EndReplay());
                        }
                        else if (DataManager.Instance.Loading == DataManager.LoadMode.Loading)
                        {
                            //the game accepts loading into a file that has been downed, or passed its section with nothing else
                            DataManager.Instance.ResumePlay(DataManager.Instance.CurrentReplay.RecordDir, DataManager.Instance.CurrentReplay.QuicksavePos);
                            DataManager.Instance.CurrentReplay = null;

                            SetFade(true, false);

                            DataManager.Instance.Loading = DataManager.LoadMode.None;


                            bool rescuing = DataManager.Instance.Save.Rescue != null && DataManager.Instance.Save.Rescue.Rescuing;
                            //if failed, just show the death plaque
                            //if succeeded, run the script that follows.
                            SceneOutcome = ZoneManager.Instance.CurrentZone.OnExitSegment(result, rescuing);
                        }
                        else //we've reached the end of the replay
                            yield return CoroutineManager.Instance.StartCoroutine(EndReplay());
                    }
                }
            }
            else
            {
                string dateDefeated = String.Format("{0:yyyy-MM-dd}", DateTime.Now);
                bool rescue = false;
                if (result == GameProgress.ResultType.Downed && DataManager.Instance.Save.RescuesLeft > 0)
                {
                    yield return CoroutineManager.Instance.StartCoroutine(MenuManager.Instance.ProcessMenuCoroutine(MenuManager.Instance.CreateQuestion(Text.FormatKey("DLG_RESCUE_ASK"),
                        () => { rescue = true; },
                        () => { })));
                }
                //trigger the end-segment script
                if (rescue)
                {
                    DataManager.Instance.SuspendPlay();
                    GameState state = DataManager.Instance.LoadMainGameState();
                    SOSMail awaiting = new SOSMail(DataManager.Instance.Save, new ZoneLoc(ZoneManager.Instance.CurrentZoneID, ZoneManager.Instance.CurrentMapID), ZoneManager.Instance.CurrentMap.Name, dateDefeated, Versioning.GetVersion());
                    state.Save.Rescue = new RescueState(awaiting, false);
                    DataManager.Instance.SaveGameState(state);


                    DataManager.SaveRescueMail(PathMod.NoMod(DataManager.RESCUE_OUT_PATH + DataManager.SOS_FOLDER), state.Save.Rescue.SOS, true);

                    yield return CoroutineManager.Instance.StartCoroutine(MenuManager.Instance.SetDialogue(Text.FormatKey("DLG_RESCUE_INFO_1")));

                    yield return new WaitForFrames(1);

                    MenuBase.Transparent = false;
                    SceneOutcome = RestartToTitle();

                    yield return CoroutineManager.Instance.StartCoroutine(DungeonScene.Instance.SuspendGame());
                }
                else
                {
                    bool rescuing = DataManager.Instance.Save.Rescue != null && DataManager.Instance.Save.Rescue.Rescuing;
                    SceneOutcome = ZoneManager.Instance.CurrentZone.OnExitSegment(result, rescuing);
                }
            }

        }

        public IEnumerator<YieldInstruction> EndReplay()
        {
            DataManager.Instance.MsgLog.Clear();

            DataManager.Instance.CurrentReplay = null;
            DataManager.Instance.Loading = DataManager.LoadMode.None;

            //don't record outcome, just go back to the replay menu
            SceneOutcome = ReturnToReplayMenu();
            yield break;
        }

        public IEnumerator<YieldInstruction> ReturnToReplayMenu()
        {
            cleanup();
            reInit();
            MoveToScene(new TitleScene(true));

            MenuBase.Transparent = false;

            SetFade(false, false);
            yield break;
        }

        public void MoveToScene(BaseScene scene)
        {
            //need to transfer player data
            CurrentScene = scene;
            CurrentScene.Begin();
        }

        private void startCleanSave(ulong seed)
        {
            DataManager.Instance.MsgLog.Clear();
            DataManager.Instance.EndPlay(null, null);
            if (DataManager.Instance.Save == null)
                NewGamePlus(seed);
            else
                DataManager.Instance.Save.Rand = new ReRandom(seed);
        }

        public IEnumerator<YieldInstruction> DebugWarp(ZoneLoc dest, ulong seed)
        {
            startCleanSave(seed);

            DataManager.Instance.Save.NextDest = dest;
            DataManager.Instance.Save.RestartLogs(MathUtils.Rand.NextUInt64());
            DataManager.Instance.Save.MidAdventure = true;
            yield return CoroutineManager.Instance.StartCoroutine(MoveToZone(DataManager.Instance.Save.NextDest, true));
        }


        public IEnumerator<YieldInstruction> TestWarp(string mapName, bool newGround, ulong seed)
        {
            startCleanSave(seed);

            BaseScene destScene = newGround ? (BaseScene)GroundScene.Instance : (BaseScene)DungeonScene.Instance;

            yield return CoroutineManager.Instance.StartCoroutine(exitMap(destScene));

            ZoneManager.Instance.MoveToDevZone(newGround, mapName);

            //switch in new scene
            MoveToScene(destScene);

            //move in like a normal map would
            yield return CoroutineManager.Instance.StartCoroutine(moveToZoneInit(0, newGround, false));
        }

        public void NewGamePlus(ulong seed)
        {
            try
            {
                DataManager.Instance.SetProgress(new MainProgress(seed, Guid.NewGuid().ToString().ToUpper()));
                DataManager.Instance.Save.ActiveTeam = new ExplorerTeam();
                LuaEngine.Instance.OnDebugLoad();
                if (DataManager.Instance.Save.ActiveTeam.Players.Count == 0)
                    throw new Exception("Script generated an invalid debug team!");
                return;
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex);
            }
            DataManager.Instance.SetProgress(new MainProgress(seed, Guid.NewGuid().ToString().ToUpper()));
            DataManager.Instance.Save.ActiveTeam = new ExplorerTeam();
            DataManager.Instance.Save.ActiveTeam.SetRank(0);
            DataManager.Instance.Save.ActiveTeam.Name = "Debug";
            DataManager.Instance.Save.ActiveTeam.Players.Add(DataManager.Instance.Save.ActiveTeam.CreatePlayer(DataManager.Instance.Save.Rand, new MonsterID(), DataManager.Instance.StartLevel, -1, 0));
            DataManager.Instance.Save.UpdateTeamProfile(true);
        }

        public void UpdateMeta()
        {
            if (DataManager.Instance.Loading != DataManager.LoadMode.None)
                return;

            if (MetaInputManager.JustPressed(FrameInput.InputType.SpeedDown))
            {
                SE("Menu/Cancel");
                if (DebugSpeed > GameSpeed.Eighth)
                    DebugSpeed--;
            }

            if (MetaInputManager.JustPressed(FrameInput.InputType.SpeedUp))
            {
                SE("Menu/Confirm");
                if (DebugSpeed < GameSpeed.Octuple)
                    DebugSpeed++;
            }

            if (MetaInputManager.JustPressed(FrameInput.InputType.Pause))
            {
                SE("Menu/Sort");
                Paused = !Paused;
            }

            if (MetaInputManager.JustPressed(FrameInput.InputType.AdvanceFrame))
                AdvanceFrame = true;
            else
                AdvanceFrame = false;

            if (MetaInputManager.JustPressed(FrameInput.InputType.ShowDebug))
            {
                SE("Menu/Select");
                ShowDebug = !ShowDebug;
                longestFrame = 0;
            }

            if (DiagManager.Instance.DevMode)
            {
                if (MetaInputManager.JustPressed(FrameInput.InputType.Restart))
                {
                    MenuManager.Instance.ClearMenus();
                    if (MetaInputManager[FrameInput.InputType.Ctrl])
                        SceneOutcome = RestartToTitle();
                    else if (MetaInputManager[FrameInput.InputType.ShowDebug])
                        SceneOutcome = DebugWarp(new ZoneLoc(0, new SegLoc()), 0);
                    else
                        SceneOutcome = DebugWarp(new ZoneLoc(0, new SegLoc(-1, 0), 0), 0);
                }
            }

            if (MetaInputManager.JustPressed(FrameInput.InputType.MuteMusic))
            {
                SE("Menu/Select");
                if (SoundManager.BGMBalance != 0f)
                    SoundManager.BGMBalance = 0f;
                else
                    SoundManager.BGMBalance = DiagManager.Instance.CurSettings.BGMBalance * 0.1f;
            }

            if (MetaInputManager.MouseWheelDiff != 0)
            {
                GraphicsManager.Zoom -= MetaInputManager.MouseWheelDiff / 120;
                if (GraphicsManager.Zoom < GraphicsManager.GameZoom.x8Near)
                    GraphicsManager.Zoom = GraphicsManager.GameZoom.x8Near;
                if (GraphicsManager.Zoom > GraphicsManager.GameZoom.x8Far)
                    GraphicsManager.Zoom = GraphicsManager.GameZoom.x8Far;
            }

            CurrentScene.UpdateMeta();
        }

        public void SetMetaInput(FrameInput input)
        {
            MetaInputManager.SetFrameInput(input);
        }

        public void SetFrameInput(FrameInput input)
        {
            if (!Paused || AdvanceFrame)
                InputManager.SetFrameInput(input);
        }

        public void Update()
        {
            FrameProcessed = true;
            if (DataManager.Instance.Loading != DataManager.LoadMode.None)
            {
                for (int ii = 0; ii < 100; ii++)
                {
                    CoroutineManager.Instance.Update();
                    if (DataManager.Instance.Loading == DataManager.LoadMode.None)
                        break;
                    ForceReady();
                }
            }
            else
            {
                if (!Paused || AdvanceFrame)
                {
                    //if actions are ready for queue, get a new result
                    CoroutineManager.Instance.Update();

                    int speedFactor = 8;
                    speedFactor = (int)Math.Round(speedFactor * Math.Pow(2, (int)DebugSpeed));

                    FrameTick newElapsed = FrameTick.FromFrames(1) * speedFactor / 8;
                    Update(newElapsed);
                }
            }
        }

        public void ForceReady()
        {
            Update(FrameTick.FromFrames(3600));
        }

        public void Update(FrameTick elapsedTime)
        {
            GraphicsManager.TotalFrameTick += (ulong)elapsedTime.Ticks;

            //update music
            float musicFadeFraction = 1;
            if (NextSong != null)
            {
                MusicFadeTime -= elapsedTime;
                if (MusicFadeTime <= FrameTick.Zero)
                {
                    if (System.IO.File.Exists(PathMod.ModPath(GraphicsManager.MUSIC_PATH + NextSong)))
                    {
                        Song = NextSong;
                        NextSong = null;
                        SoundManager.PlayBGM(PathMod.ModPath(GraphicsManager.MUSIC_PATH + Song));
                    }
                    else
                    {
                        Song = "";
                        SoundManager.PlayBGM(Song);
                    }
                }
                else
                    musicFadeFraction *= MusicFadeTime.FractionOf(MUSIC_FADE_TOTAL);
            }
            if (CurrentFanfarePhase != FanfarePhase.None)
            {
                FanfareTime -= elapsedTime;
                if (CurrentFanfarePhase == FanfarePhase.PhaseOut)
                {
                    musicFadeFraction *= FanfareTime.FractionOf(FANFARE_FADE_START);
                    if (FanfareTime <= FrameTick.Zero)
                    {
                        int pauseFrames = 0;
                        if (!String.IsNullOrEmpty(QueuedFanfare) && System.IO.File.Exists(PathMod.ModPath(GraphicsManager.SOUND_PATH + QueuedFanfare + ".ogg")))
                            pauseFrames = SoundManager.PlaySound(PathMod.ModPath(GraphicsManager.SOUND_PATH + QueuedFanfare + ".ogg"), 1) + FANFARE_WAIT_EXTRA;
                        CurrentFanfarePhase = FanfarePhase.Wait;
                        if (FanfareTime < pauseFrames)
                            FanfareTime = FrameTick.FromFrames(pauseFrames);
                        QueuedFanfare = null;
                    }
                }
                else if (CurrentFanfarePhase == FanfarePhase.Wait)
                {
                    musicFadeFraction *= 0;
                    if (FanfareTime <= FrameTick.Zero)
                    {
                        CurrentFanfarePhase = FanfarePhase.PhaseIn;
                        FanfareTime = FrameTick.FromFrames(FANFARE_FADE_END);
                    }
                }
                else if (CurrentFanfarePhase == FanfarePhase.PhaseIn)
                {
                    musicFadeFraction *= (1f - FanfareTime.FractionOf(FANFARE_FADE_END));
                    if (FanfareTime <= FrameTick.Zero)
                        CurrentFanfarePhase = FanfarePhase.None;
                }
            }
            SoundManager.SetBGMVolume(musicFadeFraction);
            if (!thisFrameErrored)
                framesErrored = 0;
            thisFrameErrored = false;

            MenuManager.Instance.ProcessActions(elapsedTime);

            //keep border flash off by default
            MenuBase.BorderFlash = 0;

            CurrentScene.Update(elapsedTime);
        }

        public void Draw(SpriteBatch spriteBatch, double updateTime)
        {
            if (DataManager.Instance.Loading == DataManager.LoadMode.None || DiagManager.Instance.DevMode)
                CurrentScene.Draw(spriteBatch);

            if (DataManager.Instance.Loading != DataManager.LoadMode.None)
            {
                float window_scale = GraphicsManager.WindowZoom;
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, Matrix.CreateScale(new Vector3(window_scale, window_scale, 1)));

                string loading = "Loading";
                int actions = DataManager.Instance.CurrentReplay.Actions.Count;
                if (actions > 0)
                {
                    int progress = DataManager.Instance.CurrentReplay.CurrentAction * 95 / actions;
                    for (int ii = 0; ii < progress; ii++)
                        loading += ".";
                }

                GraphicsManager.TextFont.DrawText(spriteBatch, 2, GraphicsManager.ScreenHeight - 2,
                        loading, null, DirV.Down, DirH.Left, Color.White);
            }
            else
            {
                float window_scale = GraphicsManager.WindowZoom;
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, null, null, null, Matrix.CreateScale(new Vector3(window_scale, window_scale, 1)));

                //draw transitions
                if (fadeAmount > 0)
                    GraphicsManager.Pixel.Draw(spriteBatch, new Rectangle(0, 0, GraphicsManager.ScreenWidth, GraphicsManager.ScreenHeight), null, (fadeWhite ? Color.White : Color.Black) * fadeAmount);
                if (titleFadeAmount > 0)
                    GraphicsManager.DungeonFont.DrawText(spriteBatch, GraphicsManager.ScreenWidth / 2, GraphicsManager.ScreenHeight / 2,
                        fadedTitle, null, DirV.None, DirH.None, Color.White * titleFadeAmount);
                if (bgFadeAmount > 0 && fadedBG.AnimIndex != "")
                {
                    DirSheet bg = GraphicsManager.GetBackground(fadedBG.AnimIndex);
                    bg.DrawDir(spriteBatch, new Vector2(GraphicsManager.ScreenWidth / 2 - bg.TileWidth / 2, GraphicsManager.ScreenHeight / 2 - bg.TileHeight / 2),
                        fadedBG.GetCurrentFrame(GraphicsManager.TotalFrameTick, bg.TotalFrames), Dir8.Down, Color.White * ((float)fadedBG.Alpha / 255) * bgFadeAmount);
                }
            }

            MenuManager.Instance.DrawMenus(spriteBatch);

            if (totalErrorCount > 0)
                GraphicsManager.SysFont.DrawText(spriteBatch, GraphicsManager.ScreenWidth - 2, GraphicsManager.ScreenHeight - 2, String.Format("{0} ERRORS", totalErrorCount), null, DirV.Down, DirH.Right, Color.Red);

            if (ShowDebug)
            {
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, null, null);
                DrawDebug(spriteBatch, updateTime);
            }
            spriteBatch.End();
        }

        private void DrawDebug(SpriteBatch spriteBatch, double updateTime)
        {
            int fps = 0;
            if (updateTime > 0)
                fps = (int)(1 / updateTime);
            if ((int)(updateTime * 1000) > longestFrame)
                longestFrame = (int)(updateTime * 1000);

            if (DiagManager.Instance.DevMode)
            {
                CurrentScene.DrawDebug(spriteBatch);

                GraphicsManager.SysFont.DrawText(spriteBatch, 2, 62, String.Format("Speed: {0}", DebugSpeed.ToString()), null, DirV.Up, DirH.Left, Color.LightYellow);
                GraphicsManager.SysFont.DrawText(spriteBatch, 2, 72, String.Format("Zoom: {0}", GraphicsManager.Zoom.ToString()), null, DirV.Up, DirH.Left, Color.White);
            }

            GraphicsManager.SysFont.DrawText(spriteBatch, 2, 32, String.Format("{0:D2} FPS  {1:D5} Longest", fps, longestFrame), null, DirV.Up, DirH.Left, Color.White);
            GraphicsManager.SysFont.DrawText(spriteBatch, 2, 42, Versioning.GetVersion().ToString(), null, DirV.Up, DirH.Left, Color.White);
            if (DataManager.Instance.CurrentReplay != null)
                GraphicsManager.SysFont.DrawText(spriteBatch, 2, 52, String.Format("Replay: {0} {1}", DataManager.Instance.CurrentReplay.RecordVersion.ToString(), DataManager.Instance.CurrentReplay.RecordLang.ToString()), null, DirV.Up, DirH.Left, Color.White);
            if (DebugUI != null)
            {
                string[] lines = DebugUI.Split('\n');
                for (int ii = 0; ii < lines.Length; ii++)
                    GraphicsManager.SysFont.DrawText(spriteBatch, 2, GraphicsManager.WindowHeight - 2 + (ii + 1 - lines.Length) * 10, lines[ii], null, DirV.Down, DirH.Left, Color.White);
            }
        }

        private void OnError(string msg)
        {
            totalErrorCount++;
            if (framesErrored == 0)
                SE("Menu/Error");
            if (!thisFrameErrored)
                framesErrored++;
            thisFrameErrored = true;
            if (framesErrored > 300)
                GameBase.CurrentPhase = GameBase.LoadPhase.Error;
        }

        public IEnumerator<YieldInstruction> LogSkippableMsg(string msg)
        {
            return LogSkippableMsg(msg, DataManager.Instance.Save.ActiveTeam);
        }
        public IEnumerator<YieldInstruction> LogSkippableMsg(string msg, Team involvedTeam)
        {
            return LogSkippableMsg(MonsterID.Invalid, null, new EmoteStyle(0), msg, involvedTeam);
        }
        public IEnumerator<YieldInstruction> LogSkippableMsg(MonsterID speaker, string msg, Team involvedTeam)
        {
            return LogSkippableMsg(speaker, DataManager.Instance.GetMonster(speaker.Species).Name.ToLocal(), new EmoteStyle(0), msg, involvedTeam);
        }
        public IEnumerator<YieldInstruction> LogSkippableMsg(MonsterID speaker, string name, EmoteStyle emotion, string msg)
        {
            return LogSkippableMsg(speaker, name, emotion, msg, DataManager.Instance.Save.ActiveTeam);
        }
        public IEnumerator<YieldInstruction> LogSkippableMsg(MonsterID speaker, string name, EmoteStyle emotion, string msg, Team involvedTeam)
        {
            if (involvedTeam == DataManager.Instance.Save.ActiveTeam && DataManager.Instance.CurrentReplay == null)
                yield return CoroutineManager.Instance.StartCoroutine(MenuManager.Instance.SetDialogue(speaker, name, emotion, false, msg));
            else
            {
                DungeonScene.Instance.LogMsg(msg);
                yield return new WaitForFrames(GameManager.Instance.ModifyBattleSpeed(30));
            }
        }
    }
}
