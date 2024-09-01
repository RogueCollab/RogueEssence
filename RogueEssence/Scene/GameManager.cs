using System;
using System.Collections.Generic;
using System.IO;
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
using RogueEssence.LevelGen;
using System.Xml.Linq;

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

        private static GameManager instance;
        public static void InitInstance()
        {
            if (instance != null)
                GraphicsManager.ZoomChanged -= instance.ZoomChanged;
            instance = new GameManager();
            GraphicsManager.ZoomChanged += instance.ZoomChanged;
        }
        public static GameManager Instance { get { return instance; } }
        
        public RenderTarget2D GameScreen { get; private set; }


        public InputManager MetaInputManager;
        public InputManager InputManager;

        public BaseScene CurrentScene;
        public TextPopUp TextPopUp;

        public IEnumerator<YieldInstruction> SceneOutcome;

        public GameSpeed DebugSpeed;
        public bool Paused;
        public bool AdvanceFrame;
        public bool ShowDebug;

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

        public Dictionary<string, (float volume, float diff)> LoopingSE;

        /// <summary>
        /// Currently playing song
        /// </summary>
        public string Song;

        /// <summary>
        /// Songs that are playing in parallel but muted in wait for crossfade.
        /// Includes the currently playing song
        /// </summary>
        public Dictionary<string, string> SongFamily;

        public List<MusicEffect> MusicEffects;

        public const int MUSIC_FADE_TOTAL = 40;


        public GameManager()
        {
            fadedTitle = "";
            fadedBG = new BGAnimData();

            MetaInputManager = new InputManager();
            InputManager = new InputManager();
            TextPopUp = new TextPopUp();

            SongFamily = new Dictionary<string, string>();
            MusicEffects = new List<MusicEffect>();

            LoopingSE = new Dictionary<string, (float volume, float diff)>();

            DiagManager.Instance.SetErrorListener(OnError, ErrorTrace);

            ZoomChanged();
        }

        public void ZoomChanged()
        {
            GameScreen = new RenderTarget2D(GraphicsManager.GraphicsDevice,
                GraphicsManager.WindowWidth, GraphicsManager.WindowHeight,
                false, GraphicsManager.GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24Stencil8);
        }

        public void BattleSE(string newSE)
        {
            if (newSE != "")
                SE("Battle/" + newSE);
        }

        public void SE(string newSE)
        {
            try
            {
                if (DataManager.Instance.Loading != DataManager.LoadMode.None)
                    return;

                if (System.IO.File.Exists(PathMod.ModPath(GraphicsManager.SOUND_PATH + newSE + ".ogg")))
                    SoundManager.PlaySound(PathMod.ModPath(GraphicsManager.SOUND_PATH + newSE + ".ogg"), 1);
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex);
            }
        }

        public void LoopBattleSE(string newSE, int fadeTime = 0)
        {
            if (newSE != "")
                LoopSE("Battle/" + newSE, fadeTime);
        }

        public void LoopSE(string newSE, int fadeTime = 0)
        {
            try
            {
                if (DataManager.Instance.Loading != DataManager.LoadMode.None)
                    return;

                if (File.Exists(PathMod.ModPath(GraphicsManager.SOUND_PATH + newSE + ".ogg")))
                {
                    if (fadeTime > 0)
                        LoopingSE[newSE] = (0f, 1f / fadeTime);
                    else
                        LoopingSE[newSE] = (1f, 0f);
                    SoundManager.PlayLoopedSE(PathMod.ModPath(GraphicsManager.SOUND_PATH + newSE + ".ogg"), LoopingSE[newSE].volume);
                }
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex);
            }
        }

        public void StopLoopBattleSE(string newSE, int fadeTime = 0)
        {
            if (newSE != "")
                StopLoopSE("Battle/" + newSE, fadeTime);
        }

        public void StopLoopSE(string newSE, int fadeTime = 0)
        {
            if (fadeTime > 0)
            {
                if (LoopingSE.ContainsKey(newSE))
                {
                    (float volume, float diff) cur = LoopingSE[newSE];
                    float newDiff = -cur.volume / fadeTime;
                    LoopingSE[newSE] = (cur.volume, newDiff);
                }
            }
            else
            {
                LoopingSE.Remove(newSE);
                SoundManager.StopLoopedSE(PathMod.ModPath(GraphicsManager.SOUND_PATH + newSE + ".ogg"));
            }
        }


        public IEnumerator<YieldInstruction> WaitFanfareEnds()
        {
            yield return new WaitWhile(() =>
            {
                foreach (MusicEffect effect in MusicEffects)
                {
                    if (effect is FanfareEffect)
                        return true;
                }
                return false;
            });
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

            int pauseFrames = 0;
            try
            {
                if (File.Exists(PathMod.ModPath(GraphicsManager.SOUND_PATH + newSE + ".ogg")))
                    pauseFrames = SoundManager.PlaySound(PathMod.ModPath(GraphicsManager.SOUND_PATH + newSE + ".ogg"));
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex);
            }
            if (pauseFrames > 0)
                yield return new WaitForFrames(pauseFrames);
        }

        public void Fanfare(string newSE)
        {
            if (DataManager.Instance.Loading != DataManager.LoadMode.None)
                return;

            if (String.IsNullOrEmpty(newSE))
                return;

            FanfareEffect oldFanfare = null;
            //assume no fanfares happen within the same period
            for(int ii = 0; ii < MusicEffects.Count; ii++)
            {
                if (MusicEffects[ii] is FanfareEffect)
                {
                    FanfareEffect fanfare = (FanfareEffect)MusicEffects[ii];
                    if (fanfare.CurrentPhase == FanfareEffect.FanfarePhase.PhaseIn || fanfare.CurrentPhase == FanfareEffect.FanfarePhase.Wait)
                        oldFanfare = fanfare;
                    else
                        return;
                }
            }

            if (oldFanfare != null)
            {
                oldFanfare.Fanfare = newSE;
                FrameTick oldTime = oldFanfare.FanfareTime;
                if (oldFanfare.CurrentPhase == FanfareEffect.FanfarePhase.Wait)
                    oldFanfare.FanfareTime = FrameTick.Zero;
                else
                    oldFanfare.FanfareTime = FrameTick.FromFrames((FanfareEffect.FANFARE_FADE_END - oldTime.DivOf(FanfareEffect.FANFARE_FADE_END)) * FanfareEffect.FANFARE_FADE_START / FanfareEffect.FANFARE_FADE_END);
                oldFanfare.CurrentPhase = FanfareEffect.FanfarePhase.PhaseOut;
            }
            else
                MusicEffects.Add(new FanfareEffect(newSE));
        }

        public void BGM(string newBGM, bool fade, int fadeTime = GameManager.MUSIC_FADE_TOTAL)
        {
            if (DataManager.Instance.Loading != DataManager.LoadMode.None)
                return;

            if (String.IsNullOrEmpty(Song) || !fade) // if the current song is empty, or the game doesn't want to fade it
            {
                for (int ii = 0; ii < MusicEffects.Count; ii++)
                {
                    // remove all music effects, including fanfares, so that fade ins dont happen on transitions
                    //if (MusicEffects[ii] is MusicFadeEffect)
                    //{
                        MusicEffects.RemoveAt(ii);
                        //break;
                    //}
                }

                HashSet<string> instantFamily = new HashSet<string>();
                string instantFileName = PathMod.ModPath(GraphicsManager.MUSIC_PATH + newBGM);
                if (File.Exists(instantFileName))
                {
                    LoopedSong song = new LoopedSong(instantFileName);
                    instantFamily = getSongFamily(newBGM, song);
                }

                //immediately start
                MusicFadeOutEffect newEffect = new MusicFadeOutEffect(newBGM, instantFamily, FrameTick.Zero, 1);
                MusicEffects.Add(newEffect);
                return;
            }

            for (int ii = 0; ii < MusicEffects.Count; ii++)
            {
                if (MusicEffects[ii] is MusicFadeOutEffect)
                {
                    MusicFadeOutEffect oldFade = (MusicFadeOutEffect)MusicEffects[ii];

                    HashSet<string> instantFamily = new HashSet<string>();
                    string instantFileName = PathMod.ModPath(GraphicsManager.MUSIC_PATH + newBGM);
                    if (File.Exists(instantFileName))
                    {
                        LoopedSong song = new LoopedSong(instantFileName);
                        instantFamily = getSongFamily(newBGM, song);
                    }

                    oldFade.NextSong = newBGM;
                    oldFade.Family = instantFamily;
                    return;
                }
                else if (MusicEffects[ii] is MusicFadeEffect)
                {
                    //just stop the existing other fades
                    MusicEffects.RemoveAt(ii);
                }
            }

            //do nothing this is already the same song
            if (Song == newBGM)
                return;

            HashSet<string> oldFamily = new HashSet<string>();
            foreach(string key in SongFamily.Keys)
                oldFamily.Add(key);

            HashSet<string> newFamily = new HashSet<string>();
            string fileName = PathMod.ModPath(GraphicsManager.MUSIC_PATH + newBGM);
            if (File.Exists(fileName))
            {
                LoopedSong song = new LoopedSong(fileName);
                newFamily = getSongFamily(newBGM, song);
            }

            //if both the old song and new song share a family, cross-fade them
            bool crossFade = (oldFamily.SetEquals(newFamily)) && (newFamily.Count > 0);

            if (crossFade)
            {
                //normal case: create a new fade effect
                if (File.Exists(fileName))
                {
                    MusicCrossFadeEffect newEffect = new MusicCrossFadeEffect(newBGM, FrameTick.FromFrames(fadeTime), fadeTime);
                    MusicEffects.Add(newEffect);
                }
                else
                {
                    MusicFadeOutEffect newEffect = new MusicFadeOutEffect("", newFamily, FrameTick.FromFrames(fadeTime), fadeTime);
                    MusicEffects.Add(newEffect);
                }
            }
            else
            {
                //normal case: create a new fade effect
                {
                    MusicFadeOutEffect newEffect = new MusicFadeOutEffect(newBGM, newFamily, FrameTick.FromFrames(fadeTime), fadeTime);
                    MusicEffects.Add(newEffect);
                }
            }
        }

        private HashSet<string> getSongFamily(string baseFile, LoopedSong song)
        {
            if (song == null)
                return new HashSet<string>();
            HashSet<string> result = new HashSet<string>();
            result.Add(baseFile);
            if (song.Tags.ContainsKey("RELATIVE"))
                result.Add(song.Tags["RELATIVE"]);
            return result;
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

        public IEnumerator<YieldInstruction> FadeIn()
        {
            int fadeTime = 10 + ModifyBattleSpeed(20);
            return FadeIn(fadeTime);
        }
        public IEnumerator<YieldInstruction> FadeIn(int fadeTime)
        {
            return fade(true, fadeWhite, fadeTime);
        }

        public IEnumerator<YieldInstruction> FadeOut(bool useWhite)
        {
            int fadeTime = 10 + ModifyBattleSpeed(20);
            return FadeOut(useWhite, fadeTime);
        }
        public IEnumerator<YieldInstruction> FadeOut(bool useWhite, int fadeTime)
        {
            return fade(false, useWhite, fadeTime);
        }

        private IEnumerator<YieldInstruction> fade(bool fadeIn, bool useWhite, int fadeTime)
        {
            if (fadeIn && fadeAmount == 0f)
                yield break;
            if (!fadeIn && fadeAmount == 1f)
            {
                SetFade(true, useWhite);
                yield break;
            }

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

        public IEnumerator<YieldInstruction> FadeTitle(bool fadeIn, string title)
        {
            int fadeTime = 10 + ModifyBattleSpeed(20);
            return FadeTitle(fadeIn, title, fadeTime);
        }
        public IEnumerator<YieldInstruction> FadeTitle(bool fadeIn, string title, int fadeTime)
        {
            if (fadeIn)
                fadedTitle = title;
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


        public IEnumerator<YieldInstruction> FadeBG(bool fadeIn, BGAnimData bg)
        {
            int fadeTime = 10 + ModifyBattleSpeed(20);
            return FadeBG(fadeIn, bg, fadeTime);
        }
        public IEnumerator<YieldInstruction> FadeBG(bool fadeIn, BGAnimData bg, int fadeTime)
        {
            if (fadeIn)
                fadedBG = bg;
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

        public bool IsInGame()
        {
            return (GameManager.Instance.CurrentScene == DungeonScene.Instance &&
                DungeonScene.Instance.ActiveTeam.Players.Count > 0 && DungeonScene.Instance.FocusedCharacter != null);
        }

        public void Begin()
        {
            SoundManager.BGMBalance = DiagManager.Instance.CurSettings.BGMBalance * 0.1f;
            SoundManager.SEBalance = DiagManager.Instance.CurSettings.SEBalance * 0.1f;
            if (DiagManager.Instance.DevMode)
                DiagManager.Instance.ListenToMapGen();

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

                LuaEngine.Instance.SceneOver();
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


        public IEnumerator<YieldInstruction> MoveToQuest(ModHeader quest, ModHeader[] mods, List<int> loadOrder)
        {
            yield return CoroutineManager.Instance.StartCoroutine(FadeOut(false));

            SetQuest(quest, mods, loadOrder);

            DiagManager.Instance.PrintModSettings();
            DiagManager.Instance.SaveModSettings();

            yield return CoroutineManager.Instance.StartCoroutine(FadeIn());
        }
        public void SetQuest(ModHeader quest, ModHeader[] mods, List<int> loadOrder)
        {
            cleanup();
            PathMod.SetMods(quest, mods, loadOrder);
            Text.Init();
            if (!Text.LangNames.ContainsKey(DiagManager.Instance.CurSettings.Language))
                DiagManager.Instance.CurSettings.Language = "en";
            Text.SetCultureCode(DiagManager.Instance.CurSettings.Language);
            reInit();
            TitleScene.TitleMenuSaveState = null;
            //clean up and reload all caches
            GraphicsManager.ReloadStatic();
            DataManager.Instance.InitData();
            LuaEngine.Instance.OnDataLoad();
            DataManager.InitSaveDirs();
            //call data editor's load method to reload the dropdowns
            DiagManager.Instance.DevEditor.ReloadData(DataManager.DataType.All);
            MoveToScene(new TitleScene(false));

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
            LuaEngine.Instance.LoadScripts();
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
            if (!String.IsNullOrEmpty(ZoneManager.Instance.CurrentZoneID))
                destLoc = new ZoneLoc(ZoneManager.Instance.CurrentZoneID, ZoneManager.Instance.CurrentMapID);

            yield return CoroutineManager.Instance.StartCoroutine(exitMap(destScene));

            ZoneManager.Instance.MoveToDevZone(newGround, name);

            //Transparency mode
            MenuBase.Transparent = !newGround;

            //switch in new scene
            MoveToScene(destScene);

            SetFade(false, false);

            if (newGround)
            {
                ZoneManager.Instance.CurrentGround.OnEditorInit();
                GroundEditScene.Instance.EnterGroundEdit(0);
            }
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

        public IEnumerator<YieldInstruction> MoveToZone(ZoneLoc destId)
        {
            return MoveToZone(destId, false, false);
        }

        public IEnumerator<YieldInstruction> MoveToZone(ZoneLoc destId, bool forceNewZone, bool preserveMusic)
        {
            //if we're in a test map, return to editor
            if (ZoneManager.Instance.InDevZone && !forceNewZone)
            {
                yield return CoroutineManager.Instance.StartCoroutine(ReturnToEditor());
                yield break;
            }

            if (!DataManager.Instance.DataIndices[DataManager.DataType.Zone].ContainsKey(destId.ID))
                throw new ArgumentException(String.Format("Invalid Zone Name: {0}", destId.ID), nameof(destId.ID));

            ZoneEntrySummary summary = (ZoneEntrySummary)DataManager.Instance.DataIndices[DataManager.DataType.Zone].Get(destId.ID);
            if (!summary.SegLocValid(destId.StructID))
                throw new ArgumentException(String.Format("Invalid Segment ID: {0}", destId.StructID), nameof(destId));

            bool newGround = (destId.StructID.Segment <= -1);
            BaseScene destScene = newGround ? (BaseScene)GroundScene.Instance : DungeonScene.Instance;
            bool sameZone = destId.ID == ZoneManager.Instance.CurrentZoneID;
            bool sameSegment = sameZone && (destId.StructID.Segment == ZoneManager.Instance.CurrentMapID.Segment);
            bool newSegment = (!sameSegment || forceNewZone);

            // The only time a replay tries to move to a ground is when it ends its current segment.
            // Do the same logic as EndSegment.
            if (DataManager.Instance.CurrentReplay != null && newGround)
            {
                yield return CoroutineManager.Instance.StartCoroutine(handleReplayDungeonEnd(GameProgress.ResultType.Cleared));
            }
            else
            {
                yield return CoroutineManager.Instance.StartCoroutine(exitMap(destScene));

                //switch location
                if (sameZone && !forceNewZone)
                {
                    //don't need to do anything
                }
                else
                {
                    ZoneManager.Instance.MoveToZone(destId.ID, unchecked(DataManager.Instance.Save.Rand.FirstSeed + (ulong)Text.DeterministicHash(destId.ID)));//NOTE: there are better ways to seed a multi-dungeon adventure
                    yield return CoroutineManager.Instance.StartCoroutine(ZoneManager.Instance.CurrentZone.OnInit());
                }

                if (newSegment)
                {
                    bool rescuing = DataManager.Instance.Save.Rescue != null && DataManager.Instance.Save.Rescue.Rescuing;
                    yield return CoroutineManager.Instance.StartCoroutine(ZoneManager.Instance.CurrentZone.OnEnterSegment(rescuing, destId.StructID));
                }

                ZoneManager.Instance.CurrentZone.SetCurrentMap(destId.StructID);

                yield return CoroutineManager.Instance.StartCoroutine(moveToZoneInit(destId.EntryPoint, newGround, newSegment, preserveMusic));
            }
        }


        /// <summary>
        /// Enter a ground map by name, and makes the player spawn at the specified named marker
        /// </summary>
        /// <param name="zone"></param>
        /// <param name="mapname"></param>
        /// <param name="entrypoint"></param>
        /// <param name="preserveMusic"></param>
        /// <returns></returns>
        public IEnumerator<YieldInstruction> MoveToGround(string zone, string mapname, string entrypoint, bool preserveMusic)
        {
            //if we're in a test map, return to editor
            if (ZoneManager.Instance.InDevZone)
            {
                yield return CoroutineManager.Instance.StartCoroutine(ReturnToEditor());
                yield break;
            }

            if (!DataManager.Instance.DataIndices[DataManager.DataType.Zone].ContainsKey(zone))
                throw new ArgumentException(String.Format("Invalid Zone Name: {0}", zone), nameof(zone));

            ZoneEntrySummary summary = (ZoneEntrySummary)DataManager.Instance.DataIndices[DataManager.DataType.Zone].Get(zone);
            if (!summary.GroundValid(mapname))
                throw new ArgumentException(String.Format("Invalid Ground Map Name: {0}", mapname), nameof(mapname));

            bool sameZone = zone == ZoneManager.Instance.CurrentZoneID;
            bool newSegment = !sameZone;

            yield return CoroutineManager.Instance.StartCoroutine(exitMap(GroundScene.Instance));

            //switch location
            if (!sameZone)
            {
                ZoneManager.Instance.MoveToZone(zone, unchecked(DataManager.Instance.Save.Rand.FirstSeed + (ulong)zone.GetHashCode()));//NOTE: there are better ways to seed a multi-dungeon adventure
                yield return CoroutineManager.Instance.StartCoroutine(ZoneManager.Instance.CurrentZone.OnInit());
            }

            int mapId = ZoneManager.Instance.CurrentZone.GroundMaps.FindIndex((str) => (str == mapname));
            SegLoc destSegLoc = new SegLoc(-1, mapId);

            if (newSegment)
            {
                bool rescuing = DataManager.Instance.Save.Rescue != null && DataManager.Instance.Save.Rescue.Rescuing;
                yield return CoroutineManager.Instance.StartCoroutine(ZoneManager.Instance.CurrentZone.OnEnterSegment(rescuing, destSegLoc));
            }

            ZoneManager.Instance.CurrentZone.SetCurrentMap(destSegLoc);

            int entryIdx = ZoneManager.Instance.CurrentGround.GetEntryPointIdx(entrypoint);
            yield return CoroutineManager.Instance.StartCoroutine(moveToZoneInit(entryIdx, true, newSegment, preserveMusic));
        }


        private IEnumerator<YieldInstruction> moveToZoneInit(int entryPoint, bool newGround, bool newSegment, bool preserveMusic)
        {
            //Transparency mode
            MenuBase.Transparent = !newGround;

            if (newGround && CurrentScene != GroundScene.Instance)
            {
                MoveToScene(GroundScene.Instance);
                LuaEngine.Instance.OnGroundModeBegin();
            }
            else if (!newGround && CurrentScene != DungeonScene.Instance)
            {
                MoveToScene(DungeonScene.Instance);
                LuaEngine.Instance.OnDungeonModeBegin();
            }


            if (newGround)
            {
                if (!preserveMusic)
                    BGM(ZoneManager.Instance.CurrentGround.Music, true);

                GroundScene.Instance.EnterGround(entryPoint);


                yield return CoroutineManager.Instance.StartCoroutine(GroundScene.Instance.InitGround(false));
                //no fade; the script handles that itself
                yield return CoroutineManager.Instance.StartCoroutine(GroundScene.Instance.BeginGround());
            }
            else
            {
                if (!preserveMusic)
                    BGM(ZoneManager.Instance.CurrentMap.Music, true);

                DungeonScene.Instance.EnterFloor(entryPoint);

                yield return CoroutineManager.Instance.StartCoroutine(DungeonScene.Instance.InitFloor());

                yield return CoroutineManager.Instance.StartCoroutine(DungeonScene.Instance.BeginFloor());
            }
            DataManager.Instance.Save.NextDest = ZoneLoc.Invalid;
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
            yield return CoroutineManager.Instance.StartCoroutine(BeginSegment(new ZoneLoc(sos.Goal.ID, new SegLoc(0, 0)), true));


            //must also set script variables for dungeon if they matter
            //when this mission ends, all scripts must move the player back to the rescue start location...
            //if the mission was a success, the replay must be packaged into an AOK mail
        }

        public IEnumerator<YieldInstruction> BeginGameInSegment(ZoneLoc nextZone, GameProgress.DungeonStakes stakes, bool recorded, bool silentRestrict)
        {
            yield return CoroutineManager.Instance.StartCoroutine(BeginGame(nextZone.ID, MathUtils.Rand.NextUInt64(), stakes, recorded, silentRestrict));
            yield return CoroutineManager.Instance.StartCoroutine(BeginSegment(nextZone, true));
        }
        public IEnumerator<YieldInstruction> BeginGame(string zoneID, ulong seed, GameProgress.DungeonStakes stakes, bool recorded, bool silentRestrict)
        {
            //initiate the adventure
            DataManager.Instance.CurrentReplay = null;
            yield return CoroutineManager.Instance.StartCoroutine(DataManager.Instance.Save.BeginGame(zoneID, seed, stakes, recorded, silentRestrict));
        }
        public IEnumerator<YieldInstruction> BeginSegment(ZoneLoc nextZone, bool newGame)
        {
            DataManager.Instance.Save.NextDest = nextZone;
            if (DataManager.Instance.RecordingReplay)
                DataManager.Instance.LogState();

            SceneOutcome = MoveToZone(nextZone, newGame, false);
            yield break;
        }

        private IEnumerator<YieldInstruction> handleReplayDungeonEnd(GameProgress.ResultType result)
        {
            //try to get the game state.  if we can't get any states, then we must have quicksaved here
            if (DataManager.Instance.CurrentReplay.CurrentState < DataManager.Instance.CurrentReplay.States.Count)
            {
                GameState state = DataManager.Instance.CurrentReplay.ReadState();
                DataManager.Instance.SetProgress(state.Save);
                LuaEngine.Instance.LoadSavedData(DataManager.Instance.Save); //notify script engine
                ZoneManager.LoadFromState(state.Zone);
                LuaEngine.Instance.UpdateZoneInstance();

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
                    {
                        // Change dialogue message depending on the LoadMode.
                        DataManager.Instance.CurrentReplay.Desyncs++;
                        if (DataManager.Instance.Loading != DataManager.LoadMode.Verifying)
                        {
                            if (!DataManager.Instance.CurrentReplay.SilentVerify)
                                yield return CoroutineManager.Instance.StartCoroutine(MenuManager.Instance.SetDialogue(Text.FormatKey("DLG_REPLAY_DESYNC")));
                        }
                    }
                }

                if (rescued != null) //run the action
                    yield return CoroutineManager.Instance.StartCoroutine(DungeonScene.Instance.ProcessRescue(rescued, null));
                else if (DataManager.Instance.Save.Rescue != null && !DataManager.Instance.Save.Rescue.Rescuing)
                {
                    //resuming a game that was just rescued
                    DataManager.Instance.Save.ResumeSession(DataManager.Instance.CurrentReplay);
                    DataManager.Instance.ResumePlay(DataManager.Instance.CurrentReplay, DataManager.Instance.Save.SessionStartTime);
                    DataManager.Instance.CurrentReplay = null;
                    DataManager.Instance.Loading = DataManager.LoadMode.None;
                    SOSMail mail = DataManager.Instance.Save.Rescue.SOS;
                    //and then run the action

                    rescued = new GameAction(GameAction.ActionType.Rescue, Dir8.None);
                    rescued.AddArg(mail.OfferedItem.IsMoney ? 1 : 0);
                    rescued.AddArg(mail.OfferedItem.Value.Length);
                    for (int ii = 0; ii < mail.OfferedItem.Value.Length; ii++)
                        rescued.AddArg(mail.OfferedItem.Value[ii]);
                    rescued.AddArg(mail.OfferedItem.HiddenValue.Length);
                    for (int ii = 0; ii < mail.OfferedItem.HiddenValue.Length; ii++)
                        rescued.AddArg(mail.OfferedItem.HiddenValue[ii]);
                    rescued.AddArg(mail.OfferedItem.Amount);

                    rescued.AddArg(mail.RescuedBy.Length);
                    for (int ii = 0; ii < mail.RescuedBy.Length; ii++)
                        rescued.AddArg(mail.RescuedBy[ii]);

                    DataManager.Instance.LogPlay(rescued);
                    DataManager.Instance.Save.UpdateOptions();
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
                        DataManager.Instance.Save.ResumeSession(DataManager.Instance.CurrentReplay);
                        DataManager.Instance.ResumePlay(DataManager.Instance.CurrentReplay, DataManager.Instance.Save.SessionStartTime);
                        DataManager.Instance.CurrentReplay = null;
                        //Normally DataManager.Instance.Save.UpdateOptions would be called, but this is just the end of the run.

                        SetFade(true, false);

                        DataManager.Instance.Loading = DataManager.LoadMode.None;


                        bool rescuing = DataManager.Instance.Save.Rescue != null && DataManager.Instance.Save.Rescue.Rescuing;
                        //if failed, just show the death plaque
                        //if succeeded, run the script that follows.
                        SceneOutcome = ZoneManager.Instance.CurrentZone.OnExitSegment(result, rescuing);
                    }
                    else if (DataManager.Instance.Loading == DataManager.LoadMode.Verifying)
                    {
                        if (!DataManager.Instance.CurrentReplay.SilentVerify)
                        {
                            if (DataManager.Instance.CurrentReplay.Desyncs > 0)
                                yield return CoroutineManager.Instance.StartCoroutine(MenuManager.Instance.SetDialogue(Text.FormatKey("DLG_REPLAY_VERIFY_DESYNC")));
                            else
                                yield return CoroutineManager.Instance.StartCoroutine(MenuManager.Instance.SetDialogue(Text.FormatKey("DLG_REPLAY_VERIFY_OK")));
                        }
                        yield return CoroutineManager.Instance.StartCoroutine(EndReplay());
                    }
                    else //we've reached the end of the replay
                        yield return CoroutineManager.Instance.StartCoroutine(EndReplay());
                }
            }
        }

        public IEnumerator<YieldInstruction> EndSegment(GameProgress.ResultType result, bool preserveMusic = false)
        {
            if (ZoneManager.Instance.InDevZone)
            {
                SceneOutcome = ReturnToEditor();
                yield break;
            }

            if ((result == GameProgress.ResultType.Failed || result == GameProgress.ResultType.Downed || result == GameProgress.ResultType.TimedOut)
                && DataManager.Instance.CurrentReplay == null)
            {
                SE("Menu/Skip");
                yield return CoroutineManager.Instance.StartCoroutine(MenuManager.Instance.ProcessMenuCoroutine(new MsgLogMenu()));
            }

            if (!preserveMusic)
            {
                BGM("", true);
                yield return CoroutineManager.Instance.StartCoroutine(FadeOut(false));
                yield return new WaitForFrames(40);
            }
            else
            {
                yield return CoroutineManager.Instance.StartCoroutine(FadeOut(false));
            }

            if (DataManager.Instance.CurrentReplay != null)
                yield return CoroutineManager.Instance.StartCoroutine(handleReplayDungeonEnd(result));
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
                    //compute and update the current session time.  the value in the gameprogress wont matter, but we are just using this function to get the result value.
                    DataManager.Instance.Save.EndSession();
                    DataManager.Instance.SaveSessionTime(DataManager.Instance.Save.SessionTime);
                    DataManager.Instance.SuspendPlay();
                    GameState state = DataManager.Instance.LoadMainGameState(false);
                    SOSMail awaiting = new SOSMail(DataManager.Instance.Save, new ZoneLoc(ZoneManager.Instance.CurrentZoneID, ZoneManager.Instance.CurrentMapID), ZoneManager.Instance.CurrentMap.Name, dateDefeated, DataManager.Instance.Save.GetModVersion());
                    state.Save.Rescue = new RescueState(awaiting, false);
                    DataManager.Instance.SaveGameState(state);


                    DataManager.SaveRescueMail(PathMod.FromApp(DataManager.RESCUE_OUT_PATH + DataManager.SOS_FOLDER), state.Save.Rescue.SOS, true);

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

        public IEnumerator<YieldInstruction> RestartToRogue(RogueConfig config)
        {
            cleanup();
            reInit();
            yield return CoroutineManager.Instance.StartCoroutine(RogueProgress.StartRogue(config));
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
            DataManager.Instance.Save.FullStateRestore();
        }

        public IEnumerator<YieldInstruction> DebugWarp(ZoneLoc dest, ulong seed)
        {
            startCleanSave(seed);

            DataManager.Instance.Save.NextDest = dest;
            DataManager.Instance.Save.RestartLogs(MathUtils.Rand.NextUInt64());
            DataManager.Instance.Save.MidAdventure = true;
            yield return CoroutineManager.Instance.StartCoroutine(MoveToZone(DataManager.Instance.Save.NextDest, true, false));
        }


        public IEnumerator<YieldInstruction> TestWarp(string mapName, bool newGround, ulong seed)
        {
            startCleanSave(seed);

            BaseScene destScene = newGround ? (BaseScene)GroundScene.Instance : (BaseScene)DungeonScene.Instance;

            yield return CoroutineManager.Instance.StartCoroutine(exitMap(destScene));

            ZoneManager.Instance.MoveToDevZone(newGround, mapName);

            //move in like a normal map would
            yield return CoroutineManager.Instance.StartCoroutine(moveToZoneInit(0, newGround, true, false));
        }

        public void NewGamePlus(ulong seed)
        {
            try
            {
                DataManager.Instance.SetProgress(new MainProgress(seed, Guid.NewGuid().ToString().ToUpper()));
                DataManager.Instance.Save.UpdateVersion();
                DataManager.Instance.Save.UpdateOptions();
                DataManager.Instance.Save.StartDate = String.Format("{0:yyyy-MM-dd_HH-mm-ss}", DateTime.Now);
                DataManager.Instance.Save.ActiveTeam = new ExplorerTeam();
                LuaEngine.Instance.OnNewGame();
                if (DataManager.Instance.Save.ActiveTeam.Players.Count == 0)
                    throw new Exception("Script generated an invalid debug team!");
                return;
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex);
            }
            DataManager.Instance.SetProgress(new MainProgress(seed, Guid.NewGuid().ToString().ToUpper()));
            DataManager.Instance.Save.UpdateVersion();
            DataManager.Instance.Save.UpdateOptions();
            DataManager.Instance.Save.StartDate = String.Format("{0:yyyy-MM-dd_HH-mm-ss}", DateTime.Now);
            DataManager.Instance.Save.ActiveTeam = new ExplorerTeam();
            DataManager.Instance.Save.ActiveTeam.SetRank(DataManager.Instance.DefaultRank);
            DataManager.Instance.Save.ActiveTeam.Name = "Debug";
            DataManager.Instance.Save.ActiveTeam.Players.Add(DataManager.Instance.Save.ActiveTeam.CreatePlayer(DataManager.Instance.Save.Rand, new MonsterID(DataManager.Instance.DefaultMonster, 0, DataManager.Instance.DefaultSkin, Gender.Genderless), DataManager.Instance.Start.Level, "", 0));
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
                        SceneOutcome = DebugWarp(new ZoneLoc(DataManager.Instance.DefaultZone, new SegLoc()), 0);
                    else
                        SceneOutcome = DebugWarp(new ZoneLoc(DataManager.Instance.DefaultZone, new SegLoc(-1, 0), 0), 0);
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
                if (AdvanceFrame)
                {
                    CoroutineManager.Instance.Update();

                    FrameTick newElapsed = FrameTick.FromFrames(1);
                    Update(newElapsed);
                }
                else if (!Paused)
                {
                    double speedMult = Math.Pow(2, (int)DebugSpeed);
                    if (DebugSpeed <= GameSpeed.Normal)
                    {
                        //if actions are ready for queue, get a new result
                        CoroutineManager.Instance.Update();

                        FrameTick newElapsed = FrameTick.FromFrames(1) * (int)Math.Round(8 * speedMult) / 8;
                        Update(newElapsed);
                    }
                    else
                    {
                        int intMult = (int)Math.Round(speedMult);
                        for (int ii = 0; ii < intMult; ii++)
                        {
                            if (ii > 0)
                                InputManager.RepeatFrameInput();

                            CoroutineManager.Instance.Update();

                            FrameTick newElapsed = FrameTick.FromFrames(1);
                            Update(newElapsed);
                        }
                    }
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
            Dictionary<string, float> crossFadeFraction = new Dictionary<string, float>();
            foreach (string songName in SongFamily.Keys)
            {
                float defaultVol = (Song == songName) ? 1f : 0f;
                crossFadeFraction[SongFamily[songName]] = defaultVol;
            }
            for (int ii = MusicEffects.Count - 1; ii >= 0; ii--)
            {
                MusicEffects[ii].Update(elapsedTime, ref musicFadeFraction, crossFadeFraction);
                if (MusicEffects[ii].Finished)
                    MusicEffects.RemoveAt(ii);
            }
            SoundManager.SetBGMVolume(musicFadeFraction);
            SoundManager.SetBGMCrossVolume(crossFadeFraction);

            //update sounds
            List<string> seKeys = new List<string>();
            foreach (string seKey in LoopingSE.Keys)
                seKeys.Add(seKey);
            
            foreach (string key in seKeys)
            {
                (float volume, float diff) cur = LoopingSE[key];
                if (cur.volume + cur.diff <= 0)
                {
                    LoopingSE.Remove(key);
                    SoundManager.StopLoopedSE(PathMod.ModPath(GraphicsManager.SOUND_PATH + key + ".ogg"));
                }
                else if (cur.volume + cur.diff > 1)
                    LoopingSE[key] = (1f, 0f);
                else
                {
                    LoopingSE[key] = (cur.volume + cur.diff, cur.diff);
                    SoundManager.SetLoopedSEVolume(PathMod.ModPath(GraphicsManager.SOUND_PATH + key + ".ogg"), LoopingSE[key].volume);
                }
            }

            if (!thisFrameErrored)
                framesErrored = 0;
            thisFrameErrored = false;

            MenuManager.Instance.ProcessActions(elapsedTime);
            TextPopUp.Update(elapsedTime);

            //keep border flash off by default
            MenuBase.BorderFlash = 0;

            CurrentScene.Update(elapsedTime);
        }

        public void Draw(SpriteBatch spriteBatch, double updateTime)
        {
            GraphicsManager.GraphicsDevice.SetRenderTarget(GameScreen);
            GraphicsManager.GraphicsDevice.Clear(Color.Black);
            
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
                if (bgFadeAmount > 0 && fadedBG.AnimIndex != "")
                {
                    DirSheet bg = GraphicsManager.GetBackground(fadedBG.AnimIndex);
                    bg.DrawDir(spriteBatch, new Vector2(GraphicsManager.ScreenWidth / 2 - bg.TileWidth / 2, GraphicsManager.ScreenHeight / 2 - bg.TileHeight / 2),
                        fadedBG.GetCurrentFrame(GraphicsManager.TotalFrameTick, bg.TotalFrames), Dir8.Down, Color.White * ((float)fadedBG.Alpha / 255) * bgFadeAmount);
                }
                if (titleFadeAmount > 0)
                    GraphicsManager.DungeonFont.DrawText(spriteBatch, GraphicsManager.ScreenWidth / 2, GraphicsManager.ScreenHeight / 2,
                        fadedTitle, null, DirV.None, DirH.None, Color.White * titleFadeAmount);
            }

            MenuManager.Instance.DrawMenus(spriteBatch);
            TextPopUp.Draw(spriteBatch);

            if (totalErrorCount > 0)
                GraphicsManager.SysFont.DrawText(spriteBatch, GraphicsManager.ScreenWidth - 2, GraphicsManager.ScreenHeight - 2, String.Format("{0} ERRORS", totalErrorCount), null, DirV.Down, DirH.Right, Color.Red);
            if (ShowDebug)
            {
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, null, null);
                DrawDebug(spriteBatch, updateTime);
            }
            spriteBatch.End();

            GraphicsManager.GraphicsDevice.SetRenderTarget(null);

            GraphicsManager.GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, null, null, null, Matrix.CreateScale(new Vector3(1, 1, 1)));

            Loc screenPos = GraphicsManager.GetGameScreenOffset();
            spriteBatch.Draw(GameScreen, screenPos.ToVector2(), Color.White);

            spriteBatch.End();

        }

        private void DrawDebug(SpriteBatch spriteBatch, double updateTime)
        {
            int fps = 0;
            if (updateTime > 0)
                fps = (int)(1 / updateTime);
            if ((int)(updateTime * 1000) > longestFrame)
                longestFrame = (int)(updateTime * 1000);

            GraphicsManager.SysFont.DrawText(spriteBatch, 2, 32, String.Format("{0:D2} FPS  {1:D5} Longest", fps, longestFrame), null, DirV.Up, DirH.Left, Color.White);
            GraphicsManager.SysFont.DrawText(spriteBatch, 2, 42, Versioning.GetVersion().ToString(), null, DirV.Up, DirH.Left, Color.White);

            //if (DataManager.Instance.CurrentReplay != null)
            //    GraphicsManager.SysFont.DrawText(spriteBatch, 2, 52, String.Format("Replay: {0} {1}", DataManager.Instance.CurrentReplay.RecordVersion.ToString(), DataManager.Instance.CurrentReplay.RecordLang.ToString()), null, DirV.Up, DirH.Left, Color.White);

            if (DiagManager.Instance.DevMode)
            {
                CurrentScene.DrawDebug(spriteBatch);

                GraphicsManager.SysFont.DrawText(spriteBatch, 2, 52, String.Format("Speed: {0}", DebugSpeed.ToString()), null, DirV.Up, DirH.Left, Color.LightYellow);
                GraphicsManager.SysFont.DrawText(spriteBatch, 2, 62, String.Format("Zoom: {0}", GraphicsManager.Zoom.ToString()), null, DirV.Up, DirH.Left, Color.White);
            }


        }

        private void OnError(string msg)
        {
            totalErrorCount++;
            bool ping = (framesErrored == 0);
            if (!thisFrameErrored)
                framesErrored++;
            thisFrameErrored = true;
            if (framesErrored > 180)
                GameBase.CurrentPhase = GameBase.LoadPhase.Error;
            if (ping)
                SE("Menu/Error");

            if (DataManager.Instance.CurrentReplay != null)
                DataManager.Instance.CurrentReplay.Desyncs++;
        }

        private string ErrorTrace()
        {
            System.Text.StringBuilder str = new System.Text.StringBuilder();
            str.Append("\nLua Trace:\n");
            str.Append(LuaEngine.Instance.DumpStack());
            str.Append("\nCoroutine Trace:\n");
            str.Append(CoroutineManager.Instance.DumpCoroutines());

            return str.ToString();
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
