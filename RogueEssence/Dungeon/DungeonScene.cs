using System;
using System.Collections.Generic;
using System.Linq;
using RogueEssence.LevelGen;
using RogueEssence.Content;
using RogueElements;
using RogueEssence.Data;
using RogueEssence.Dev;
using RogueEssence.Menu;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RogueEssence.Dungeon
{
    //The game engine for Dungeon Mode, in which everyone takes an ordered turn in lock-step
    public partial class DungeonScene : BaseDungeonScene
    {
        private static DungeonScene instance;
        public static void InitInstance()
        {
            if (instance != null)
                GraphicsManager.ZoomChanged -= instance.ZoomChanged;
            instance = new DungeonScene();
            GraphicsManager.ZoomChanged += instance.ZoomChanged;
        }
        public static DungeonScene Instance { get { return instance; } }
        
        public enum MinimapState
        {
            None,
            Clear,
            Detail
        }

        private const float DARK_TRANSPARENT = 0.75f;

        const int MAX_MINIMAP_WIDTH = 80;
        const int MAX_MINIMAP_HEIGHT = 56;
        //public bool GodMode;

        public bool SeeAll;
        public int DebugEmote;
        public GraphicsManager.AssetType DebugAsset;
        public string DebugAnim;

        public ExplorerTeam ActiveTeam {  get { return ZoneManager.Instance.CurrentMap.ActiveTeam; } }

        
        private int focusedPlayerIndex;
        public Character FocusedCharacter
        {
            get { return ActiveTeam.Players[focusedPlayerIndex]; }
        }
        public Character CurrentCharacter
        {
            get { return ZoneManager.Instance.CurrentMap.CurrentCharacter; }
        }

        public Loc GetFocusedMapLoc()
        {
            Loc focusedLoc = new Loc();
            if (ZoneManager.Instance.CurrentMap.ViewCenter.HasValue)
                focusedLoc = ZoneManager.Instance.CurrentMap.ViewCenter.Value;
            else if (FocusedCharacter != null)
                focusedLoc = FocusedCharacter.MapLoc + new Loc(GraphicsManager.TileSize / 2, GraphicsManager.TileSize / 3) + ZoneManager.Instance.CurrentMap.ViewOffset;
            return focusedLoc;
        }
        
        public List<EXPGain> GainedEXP;
        
        public List<CharIndex> LevelGains;

                
        public IEnumerator<YieldInstruction> PendingLeaderAction;
        
        
        public bool RunMode;
        
        public bool RunCancel;
        
        private HashSet<Character> revealedEnemies;

        
        LiveMsgLog LiveBattleLog;
        
        TeamModeNotice TeamModeNote;
        
        HotkeyMenu[] ShownHotkeys;

        
        public List<PickupItem> PickupItems;
        
        public List<Hitbox> Hitboxes;

        
        public bool Diagonal;
        
        public bool Turn;
        
        public bool ShowActions;
        
        public MinimapState ShowMap;

        
        /// <summary>
        /// Rectangle of the tiles that are relevant to sight computation.
        /// </summary>
        private Rect sightRect;
        
        private float[][] charSightValues;
        
        public DungeonScene()
        {

            LevelGains = new List<CharIndex>();
            PickupItems = new List<PickupItem>();
            Hitboxes = new List<Hitbox>();

            LiveBattleLog = new LiveMsgLog();
            TeamModeNote = new TeamModeNotice();
            ShownHotkeys = new HotkeyMenu[CharData.MAX_SKILL_SLOTS];
            for (int ii = 0; ii < ShownHotkeys.Length; ii++)
                ShownHotkeys[ii] = new HotkeyMenu(ii);

            ShowMap = MinimapState.Clear;
            Loc drawSight = getDrawSight();
            charSightValues = new float[drawSight.X][];
            for (int ii = 0; ii < drawSight.X; ii++)
                charSightValues[ii] = new float[drawSight.Y];

        }

        public override void Begin()
        {
            GainedEXP = new List<EXPGain>();
            LevelGains = new List<CharIndex>();
            PendingLeaderAction = null;
            base.Begin();
        }

        public override void UpdateMeta()
        {
            base.UpdateMeta();
            InputManager input = GameManager.Instance.MetaInputManager;

            if (DataManager.Instance.CurrentReplay != null && !DataManager.Instance.CurrentReplay.OpenMenu)
            {
                if (input.JustPressed(FrameInput.InputType.Menu))
                {
                    GameManager.Instance.SE("Menu/Skip");
                    DataManager.Instance.CurrentReplay.Paused = true;
                    DataManager.Instance.CurrentReplay.OpenMenu = true;
                }
                else if (input.JustPressed(FrameInput.InputType.Cancel))
                {
                    if (input[FrameInput.InputType.ShowDebug])
                    {
                        //play from this position in the replay
                        DataManager.Instance.Loading = DataManager.LoadMode.Loading;
                        DataManager.Instance.CurrentReplay.CurrentAction = DataManager.Instance.CurrentReplay.Actions.Count;
                        DataManager.Instance.CurrentReplay.CurrentUI = DataManager.Instance.CurrentReplay.UICodes.Count;
                        DataManager.Instance.CurrentReplay.CurrentState = DataManager.Instance.CurrentReplay.States.Count;
                        DataManager.Instance.CurrentReplay.Paused = false;
                        LogMsg(String.Format("Playing replay from this point on."));
                    }
                    else
                    {
                        GameManager.Instance.SE("Menu/Confirm");
                        DataManager.Instance.CurrentReplay.Paused = !DataManager.Instance.CurrentReplay.Paused;
                        LogMsg(DataManager.Instance.CurrentReplay.Paused ? Text.FormatKey("MSG_REPLAY_PAUSED") : Text.FormatKey("MSG_REPLAY_UNPAUSED"));
                    }
                }
                else if (input.JustPressed(FrameInput.InputType.Skills))
                {
                    //play slower
                    GameManager.Instance.SE("Menu/Cancel");
                    if (DataManager.Instance.CurrentReplay.ReplaySpeed > GameManager.GameSpeed.Eighth)
                        DataManager.Instance.CurrentReplay.ReplaySpeed--;
                    LogMsg(Text.FormatKey("MSG_REPLAY_SPEED", DataManager.Instance.CurrentReplay.ReplaySpeed.ToLocal()));
                }
                else if (input.JustPressed(FrameInput.InputType.Turn))
                {
                    //play faster
                    GameManager.Instance.SE("Menu/Confirm");
                    if (DataManager.Instance.CurrentReplay.ReplaySpeed < GameManager.GameSpeed.Octuple)
                        DataManager.Instance.CurrentReplay.ReplaySpeed++;
                    LogMsg(Text.FormatKey("MSG_REPLAY_SPEED", DataManager.Instance.CurrentReplay.ReplaySpeed.ToLocal()));
                }
            }

            if (input.JustPressed(FrameInput.InputType.Test))
            {
                //For Test
                DebugEmote = (DebugEmote + 1) % GraphicsManager.Emotions.Count;
                LogMsg("TESTING...");
                string baseStr = "THE QUICK BROWN FOX JUMPS OVER THE LAZY DOG!?";
                string resultStr = "";
                for (int ii = 0; ii < baseStr.Length; ii++)
                {
                    if (baseStr[ii] != ' ')
                    {
                        int en = (int)baseStr[ii];
                        int un = en + 0xE000;
                        resultStr = resultStr + (char)un;
                    }
                    else
                        resultStr += ' ';
                }
                LogMsg(resultStr);
                BaseMonsterForm form = DataManager.Instance.GetMonster(ActiveTeam.Leader.BaseForm.Species).Forms[ActiveTeam.Leader.BaseForm.Form];
                ActiveTeam.Leader.MaxHPBonus = form.GetMaxStatBonus(Stat.HP);
                ActiveTeam.Leader.AtkBonus = form.GetMaxStatBonus(Stat.Attack);
                ActiveTeam.Leader.DefBonus = form.GetMaxStatBonus(Stat.Defense);
                ActiveTeam.Leader.MAtkBonus = form.GetMaxStatBonus(Stat.MAtk);
                ActiveTeam.Leader.MDefBonus = form.GetMaxStatBonus(Stat.MDef);
                ActiveTeam.Leader.SpeedBonus = form.GetMaxStatBonus(Stat.Speed);
            }

            if (input.Direction != Dir8.None && input.Direction != input.PrevDirection && input[FrameInput.InputType.Ctrl])
                PendingDevEvent = skipFloor(input.Direction.GetLoc());



            if (input.JustPressed(FrameInput.InputType.SeeAll))
            {
                GameManager.Instance.SE("Menu/Confirm");
                SeeAll = !SeeAll;
            }
            
            if (input.JustReleased(FrameInput.InputType.RightMouse) && input[FrameInput.InputType.Ctrl])
            {
                Loc coords = ScreenCoordsToMapCoords(input.MouseLoc);
                //DataManager.Instance.Save.ViewCenter = coords * GraphicsManager.TILE_SIZE;
                if (Collision.InBounds(ZoneManager.Instance.CurrentMap.Width, ZoneManager.Instance.CurrentMap.Height, coords))
                {
                    FocusedCharacter.CharLoc = coords;
                    FocusedCharacter.UpdateFrame();
                    ZoneManager.Instance.CurrentMap.UpdateExploration(FocusedCharacter);
                }
            }
            
        }


        private IEnumerator<YieldInstruction> skipFloor(Loc change)
        {
            int newStruct = Math.Max(0, Math.Min(ZoneManager.Instance.CurrentMapID.Segment + change.X, ZoneManager.Instance.CurrentZone.Structures.Count));
            if ((newStruct != ZoneManager.Instance.CurrentMapID.Segment || change.X == 0))
            {
                ZoneSegmentBase structure = ZoneManager.Instance.CurrentZone.Structures[newStruct] as ZoneSegmentBase;
                if (structure == null)
                {
                    GameManager.Instance.SE("Menu/Cancel");
                    yield break;
                }

                int newFloor = Math.Max(0, Math.Min(ZoneManager.Instance.CurrentMapID.ID - change.Y, structure.FloorCount - 1));
                if (newFloor != ZoneManager.Instance.CurrentMapID.ID || change.Y == 0)
                {
                    GameManager.Instance.SE("Menu/Sort");
                    yield return CoroutineManager.Instance.StartCoroutine(GameManager.Instance.MoveToZone(new ZoneLoc(ZoneManager.Instance.CurrentZoneID, new SegLoc(newStruct, newFloor))));
                    yield break;
                }
            }
            GameManager.Instance.SE("Menu/Cancel");
        }

        public IEnumerator<YieldInstruction> ProcessCheck()
        {
            //trigger check events- just before player action
            List<SingleCharEvent> effects = new List<SingleCharEvent>();
            effects.AddRange(ZoneManager.Instance.CurrentMap.CheckEvents);
            foreach (SingleCharEvent effect in effects)
            {
                yield return CoroutineManager.Instance.StartCoroutine(effect.Apply(null, null, FocusedCharacter));
                if (GameManager.Instance.SceneOutcome != null)
                    break;
            }
        }

        public override IEnumerator<YieldInstruction> ProcessInput()
        {
            if (!IsPlayerTurn())
            {
                yield return CoroutineManager.Instance.StartCoroutine(ProcessCheck());

                //the check events may have ended the scene
                if (GameManager.Instance.SceneOutcome == null)
                    yield return CoroutineManager.Instance.StartCoroutine(ProcessAI());
            }
            else
            {
                yield return new WaitUntil(AnimationsOver);

                GameManager.Instance.FrameProcessed = false;

                focusedPlayerIndex = ZoneManager.Instance.CurrentMap.CurrentTurnMap.GetCurrentTurnChar().Char;

                if (IsPlayerLeaderTurn() && PendingLeaderAction != null)
                {
                    if (DataManager.Instance.CurrentReplay == null)
                        yield return CoroutineManager.Instance.StartCoroutine(PendingLeaderAction);
                    PendingLeaderAction = null;
                }
                else if (PendingDevEvent != null)
                {
                    yield return CoroutineManager.Instance.StartCoroutine(PendingDevEvent);
                    PendingDevEvent = null;
                }
                else
                {
                    yield return CoroutineManager.Instance.StartCoroutine(ProcessCheck());

                    if (IsPlayerLeaderTurn() && !CanUseTeamMode())
                        SetTeamMode(false);

                    //the check events may have ended the scene
                    if (GameManager.Instance.SceneOutcome == null)
                        yield return CoroutineManager.Instance.StartCoroutine(ProcessInput(GameManager.Instance.InputManager));
                }

                if (!GameManager.Instance.FrameProcessed)
                    yield return new WaitForFrames(1);
            }

            if (IsGameOver())
            {
                bool allowRescue = true;
                if (DataManager.Instance.Save.Rescue != null && DataManager.Instance.Save.Rescue.Rescuing)//no rescues allowed when in a rescue mission yourself
                    allowRescue = false;
                if (ZoneManager.Instance.CurrentMap.NoRescue)
                    allowRescue = false;
                if (!DataManager.Instance.Save.AllowRescue)
                    allowRescue = false;
                yield return CoroutineManager.Instance.StartCoroutine(GameManager.Instance.EndSegment(allowRescue ? GameProgress.ResultType.Downed : GameProgress.ResultType.Failed));
            }
        }


        IEnumerator<YieldInstruction> ProcessInput(InputManager input)
        {
            bool replayPlaying = false;
            if (DataManager.Instance.CurrentReplay != null)
            {
                replayPlaying = true;
                bool advanceTurn = false;
                //completely different controls
                if (DataManager.Instance.CurrentReplay.Paused)
                {
                    if (input.JustPressed(FrameInput.InputType.Minimap))
                        ShowMap = (MinimapState)((int)(ShowMap + 1) % 3);

                    //multi-button presses
                    if (ShowMap == MinimapState.Detail)
                    {

                    }
                    else if (DataManager.Instance.CurrentReplay.OpenMenu)
                    {
                        yield return CoroutineManager.Instance.StartCoroutine(MenuManager.Instance.ProcessMenuCoroutine(new MainMenu()));
                        DataManager.Instance.CurrentReplay.OpenMenu = false;
                    }
                    else if (input.JustPressed(FrameInput.InputType.Attack))
                    {
                        GameManager.Instance.SE("Menu/Skip");
                        advanceTurn = true;
                    }
                }

                if (!DataManager.Instance.CurrentReplay.Paused || advanceTurn)
                {
                    if (DataManager.Instance.CurrentReplay.CurrentAction < DataManager.Instance.CurrentReplay.Actions.Count)
                        yield return CoroutineManager.Instance.StartCoroutine(ProcessPlayerInput(DataManager.Instance.CurrentReplay.ReadCommand()));
                    else if (DataManager.Instance.Loading == DataManager.LoadMode.Loading)
                    {
                        DataManager.Instance.ResumePlay(DataManager.Instance.CurrentReplay.RecordDir, DataManager.Instance.CurrentReplay.QuicksavePos);
                        DataManager.Instance.CurrentReplay = null;

                        GameManager.Instance.SetFade(true, false);

                        DataManager.Instance.Loading = DataManager.LoadMode.None;

                        GameManager.Instance.BGM(ZoneManager.Instance.CurrentMap.Music, true);

                        yield return CoroutineManager.Instance.StartCoroutine(GameManager.Instance.FadeIn());
                    }
                    else
                        yield return CoroutineManager.Instance.StartCoroutine(ProcessPlayerInput(new GameAction(GameAction.ActionType.GiveUp, Dir8.None, (int)GameProgress.ResultType.Unknown)));
                }
            }

            if (replayPlaying)
            {

            }
            else
            {
                GameAction action = new GameAction(GameAction.ActionType.None, Dir8.None);

                bool showSkills = false;
                bool turn = false;
                bool diagonal = false;
                bool runCommand = false;
                bool runCancelling = false;
                HashSet<Character> newRevealed = null;

                if (!input[FrameInput.InputType.Skills] && input.JustPressed(FrameInput.InputType.Menu))
                {
                    GameManager.Instance.SE("Menu/Skip");
                    yield return CoroutineManager.Instance.StartCoroutine(MenuManager.Instance.ProcessMenuCoroutine(new MainMenu()));
                }
                else if (input.JustPressed(FrameInput.InputType.MsgLog))
                {
                    GameManager.Instance.SE("Menu/Skip");
                    yield return CoroutineManager.Instance.StartCoroutine(MenuManager.Instance.ProcessMenuCoroutine(new MsgLogMenu()));
                }
                else if (input.JustPressed(FrameInput.InputType.SkillMenu))
                {
                    ShowActions = false;
                    GameManager.Instance.SE("Menu/Skip");

                    CharIndex turnChar = ZoneManager.Instance.CurrentMap.CurrentTurnMap.GetCurrentTurnChar();
                    if (turnChar.Faction == Faction.Player)
                        yield return CoroutineManager.Instance.StartCoroutine(MenuManager.Instance.ProcessMenuCoroutine(new SkillMenu(turnChar.Char)));
                }
                else if (input.JustPressed(FrameInput.InputType.ItemMenu))
                {
                    bool heldItems = false;
                    foreach (Character character in ActiveTeam.Players)
                    {
                        if (!character.Dead && character.EquippedItem.ID > -1)
                        {
                            heldItems = true;
                            break;
                        }
                    }
                    if (!(ActiveTeam.GetInvCount() == 0 && !heldItems))
                    {
                        GameManager.Instance.SE("Menu/Skip");
                        yield return CoroutineManager.Instance.StartCoroutine(MenuManager.Instance.ProcessMenuCoroutine(new ItemMenu()));
                    }
                    else
                        GameManager.Instance.SE("Menu/Cancel");
                }
                else if (input.JustPressed(FrameInput.InputType.TacticMenu))
                {
                    if (ActiveTeam.Players.Count > 1)
                    {
                        GameManager.Instance.SE("Menu/Skip");
                        yield return CoroutineManager.Instance.StartCoroutine(MenuManager.Instance.ProcessMenuCoroutine(new TacticsMenu()));
                    }
                    else
                        GameManager.Instance.SE("Menu/Cancel");
                }
                else if (input.JustPressed(FrameInput.InputType.TeamMenu))
                {
                    GameManager.Instance.SE("Menu/Skip");
                    yield return CoroutineManager.Instance.StartCoroutine(MenuManager.Instance.ProcessMenuCoroutine(new TeamMenu(false)));
                }
                else
                {
                    if (input.JustPressed(FrameInput.InputType.Minimap) && !input[FrameInput.InputType.Skills])
                        ShowMap = (MinimapState)((int)(ShowMap + 1) % 3);

                    //multi-button presses
                    if (ShowMap == MinimapState.Detail)
                    {

                    }
                    else if (input[FrameInput.InputType.Skills])
                    {
                        int skillIndex = -1;
                        if (input.JustPressed(FrameInput.InputType.Skill1))
                            skillIndex = 0;
                        else if (input.JustPressed(FrameInput.InputType.Skill2))
                            skillIndex = 1;
                        else if (input.JustPressed(FrameInput.InputType.Skill3))
                            skillIndex = 2;
                        else if (input.JustPressed(FrameInput.InputType.Skill4))
                            skillIndex = 3;

                        if (skillIndex > -1)
                        {
                            Skill skillState = FocusedCharacter.Skills[skillIndex].Element;
                            if (skillState.SkillNum > -1 && skillState.Charges > 0 && !skillState.Sealed)
                                action = new GameAction(GameAction.ActionType.UseSkill, Dir8.None, skillIndex);
                            else
                                GameManager.Instance.SE("Menu/Cancel");
                        }

                        if (action.Type == GameAction.ActionType.None)
                        {
                            //keep action display
                            showSkills = true;
                            if (input.Direction != Dir8.None)
                                action = new GameAction(GameAction.ActionType.Dir, input.Direction);
                        }
                    }
                    else
                    {
                        bool run = input[FrameInput.InputType.Run];
                        if (input[FrameInput.InputType.Turn])
                            turn = true;
                        if (input[FrameInput.InputType.Diagonal])
                            diagonal = true;

                        bool justPressedWait = run && input.JustPressed(FrameInput.InputType.Attack) || input.JustPressed(FrameInput.InputType.Run) && input[FrameInput.InputType.Attack] || input.JustPressed(FrameInput.InputType.Wait);
                        if (!DataManager.Instance.Save.TeamMode && (FocusedCharacter.Fullness > 0) && (run && input[FrameInput.InputType.Attack] || input[FrameInput.InputType.Wait]) || justPressedWait)
                        {
                            //if A+B is held down (or pressed, in the case of team mode), then the command is a wait.
                            action = new GameAction(GameAction.ActionType.Wait, Dir8.None);
                        }
                        else if (input.JustPressed(FrameInput.InputType.Attack))
                        {
                            //if confirm was the only move, then the command is an attack
                            action = new GameAction(GameAction.ActionType.Attack, Dir8.None);
                        }//directions
                        else if (input.JustPressed(FrameInput.InputType.Turn))
                        {
                            for (int ii = 1; ii < DirExt.DIR8_COUNT; ii++)
                            {
                                Dir8 testDir = DirExt.AddAngles(FocusedCharacter.CharDir, (Dir8)ii);
                                Loc checkLoc = FocusedCharacter.CharLoc + testDir.GetLoc();
                                if (ZoneManager.Instance.CurrentMap.GetCharAtLoc(checkLoc) != null)
                                {
                                    action = new GameAction(GameAction.ActionType.Dir, testDir);
                                    break;
                                }
                            }
                        }
                        else if (input.Direction != Dir8.None)
                        {
                            bool moveRun = run && (FocusedCharacter.Fullness > 0);
                            GameAction.ActionType cmdType = GameAction.ActionType.None;
                            if (input.Direction.IsDiagonal())
                                cmdType = GameAction.ActionType.Dir;
                            else if (FrameTick.FromFrames(input.InputTime) > FrameTick.FromFrames(2) || input.Direction == Dir8.None)
                                cmdType = GameAction.ActionType.Dir;

                            if (FrameTick.FromFrames(input.InputTime) > FrameTick.FromFrames(moveRun ? 1 : 5))
                            {
                                if (moveRun)
                                {
                                    if (RunCancel)
                                        runCancelling = true;
                                    else if (RunMode)
                                    {
                                        if (AreTilesDistinct(FocusedCharacter.CharLoc, FocusedCharacter.CharLoc + FocusedCharacter.CharDir.GetLoc()) ||
                                            IsRunningHazard(FocusedCharacter.CharLoc + FocusedCharacter.CharDir.GetLoc()))
                                        {
                                            runCancelling = true;
                                        }
                                        else if (!IsRunningHall(FocusedCharacter, FocusedCharacter.CharLoc) && IsRunningHall(FocusedCharacter, FocusedCharacter.CharLoc - FocusedCharacter.CharDir.GetLoc()))
                                        {
                                            runCancelling = true;
                                            //AreTilesDistinct(FocusedCharacter.CharLoc + DirExt.AddAngles(FocusedCharacter.CharDir, Dir8.Left).GetLoc(), FocusedCharacter.CharLoc + DirExt.AddAngles(FocusedCharacter.CharDir, Dir8.DownLeft).GetLoc())
                                            //AreTilesDistinct(FocusedCharacter.CharLoc + DirExt.AddAngles(FocusedCharacter.CharDir, Dir8.Right).GetLoc(), FocusedCharacter.CharLoc + DirExt.AddAngles(FocusedCharacter.CharDir, Dir8.DownRight).GetLoc()))
                                        }
                                        else
                                        {
                                            newRevealed = new HashSet<Character>();
                                            foreach (Character player in ActiveTeam.Players)
                                            {
                                                if (!player.Dead)
                                                {
                                                    foreach (Character seenEnemy in player.GetSeenCharacters(Alignment.Foe))
                                                        newRevealed.Add(seenEnemy);
                                                }
                                            }
                                            //check for new enemies appearing
                                            if (revealedEnemies != null)
                                            {
                                                foreach (Character enemy in newRevealed)
                                                {
                                                    if (!revealedEnemies.Contains(enemy))
                                                    {
                                                        runCancelling = true;
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                if (!runCancelling)
                                    cmdType = GameAction.ActionType.Move;
                            }

                            if (turn && cmdType == GameAction.ActionType.Move)
                                cmdType = GameAction.ActionType.Dir;

                            if (!diagonal || input.Direction.IsDiagonal())
                            {
                                action = new GameAction(cmdType, input.Direction);
                                if (cmdType == GameAction.ActionType.Move)
                                {
                                    action.AddArg(moveRun ? 0 : 1);
                                    runCommand = moveRun;
                                }
                            }
                            if (!turn)
                                diagonal = false;
                        }
                    }
                    RunMode = runCommand;
                    RunCancel = runCancelling;
                    revealedEnemies = newRevealed;

                    if (!ShowActions && showSkills)
                    {
                        for (int ii = 0; ii < CharData.MAX_SKILL_SLOTS; ii++)
                        {
                            Skill skill = FocusedCharacter.Skills[ii].Element;
                            ShownHotkeys[ii].SetArrangement(DiagManager.Instance.GamePadActive);
                            if (skill.SkillNum > -1)
                            {
                                SkillData skillData = DataManager.Instance.GetSkill(skill.SkillNum);
                                ShownHotkeys[ii].SetSkill(skillData.Name.ToLocal(), skillData.Data.Element, skill.Charges, skillData.BaseCharges+FocusedCharacter.ChargeBoost, skill.Sealed);
                            }
                            else
                                ShownHotkeys[ii].SetSkill("", 00, 0, 0, false);
                        }
                    }

                    if (action.Type == GameAction.ActionType.None && !showSkills)
                    {
                        if (input.JustPressed(FrameInput.InputType.TeamMode))
                            action = new GameAction(GameAction.ActionType.TeamMode, Dir8.None);
                        else if (input.JustPressed(FrameInput.InputType.LeaderSwap1))
                            action = new GameAction(GameAction.ActionType.SetLeader, Dir8.None, 0, 0);
                        else if (input.JustPressed(FrameInput.InputType.LeaderSwap2))
                            action = new GameAction(GameAction.ActionType.SetLeader, Dir8.None, 1, 0);
                        else if (input.JustPressed(FrameInput.InputType.LeaderSwap3))
                            action = new GameAction(GameAction.ActionType.SetLeader, Dir8.None, 2, 0);
                        else if (input.JustPressed(FrameInput.InputType.LeaderSwap4))
                            action = new GameAction(GameAction.ActionType.SetLeader, Dir8.None, 3, 0);
                        else if (input.JustPressed(FrameInput.InputType.LeaderSwapBack))
                        {
                            int newSlot = ActiveTeam.LeaderIndex;
                            do
                            {
                                newSlot = (newSlot + ActiveTeam.Players.Count - 1) % ActiveTeam.Players.Count;
                            }
                            while (!canSwitchToChar(newSlot));
                            action = new GameAction(GameAction.ActionType.SetLeader, Dir8.None, newSlot, 0);
                        }
                        else if (input.JustPressed(FrameInput.InputType.LeaderSwapForth))
                        {
                            int newSlot = ActiveTeam.LeaderIndex;
                            do
                            {
                                newSlot = (newSlot + 1) % ActiveTeam.Players.Count;
                            }
                            while (!canSwitchToChar(newSlot));
                            action = new GameAction(GameAction.ActionType.SetLeader, Dir8.None, newSlot, 0);
                        }
                    }
                }

                ShowActions = showSkills;
                Turn = turn;
                Diagonal = diagonal;

                if (action.Type != GameAction.ActionType.None)
                    yield return CoroutineManager.Instance.StartCoroutine(ProcessPlayerInput(action));
            }
        }

        public override void Update(FrameTick elapsedTime)
        {
            //update UI notes
            LiveBattleLog.Update(elapsedTime);
            TeamModeNote.Update(elapsedTime);
            if (GameManager.Instance.IsFading())
                LiveBattleLog.ForceOff();

            if (DataManager.Instance.CurrentReplay != null)
            {
                int speedFactor = 8;
                speedFactor = (int)Math.Round(speedFactor * Math.Pow(2, (int)DataManager.Instance.CurrentReplay.ReplaySpeed));

                elapsedTime = FrameTick.FromFrames(1) * speedFactor / 8;
            }

            if (ZoneManager.Instance.CurrentMap != null)
            {
                //update the team/enemies
                foreach (Character character in ZoneManager.Instance.CurrentMap.IterateCharacters())
                    character.Update(elapsedTime);

                for (int ii = PickupItems.Count - 1; ii >= 0; ii--)
                {
                    //every time the game updates, each pickup object will check if their character has completed their action
                    if (!PickupItems[ii].WaitingChar.OccupiedwithAction())
                    {
                        //if so, fire the pickup event:
                        //the sound is made, and a non-logged message is printed
                        GameManager.Instance.SE(PickupItems[ii].Sound);
                        if (PickupItems[ii].LocalMsg)
                            LogMsg(PickupItems[ii].PickupMessage, false, true, PickupItems[ii].TileLoc, null, null);
                        else
                            LogMsg(PickupItems[ii].PickupMessage, false, true);
                        //the pickup object is removed
                        PickupItems.RemoveAt(ii);
                    }
                }
                //make sure the entire list is cleared when needed

                for (int ii = Hitboxes.Count - 1; ii >= 0; ii--)
                {
                    Hitboxes[ii].Update(elapsedTime);
                    if (Hitboxes[ii].Finished)
                        Hitboxes.RemoveAt(ii);
                }

                //update screen flash
                if (GraphicsManager.TotalFrameTick / (ulong)FrameTick.FrameToTick(20) % 2 == 1)
                {
                    foreach (Character character in ZoneManager.Instance.CurrentMap.ActiveTeam.Players)
                    {
                        if (!character.Dead && character.HP * 4 <= character.MaxHP)
                        {
                            MenuBase.BorderFlash = 2;
                            break;
                        }
                    }
                }

                Loc focusedLoc = GetFocusedMapLoc();

                base.UpdateCamMod(elapsedTime, ref focusedLoc);

                base.UpdateCam(focusedLoc);

                Loc viewCenter = focusedLoc;

                base.Update(elapsedTime);

                //recompute character sight
                Loc sightStart = new Loc(viewCenter.X - GraphicsManager.ScreenWidth / 2 - GraphicsManager.TileSize / 3, viewCenter.Y - GraphicsManager.ScreenHeight / 2 - GraphicsManager.TileSize / 3);
                Loc sightEnd = new Loc(viewCenter.X + GraphicsManager.ScreenWidth / 2 + GraphicsManager.TileSize / 3, viewCenter.Y + GraphicsManager.ScreenHeight / 2 + GraphicsManager.TileSize / 3);
                sightRect = new Rect((int)Math.Floor((float)sightStart.X / GraphicsManager.TileSize), (int)Math.Floor((float)sightStart.Y / GraphicsManager.TileSize),
                    (sightEnd.X - 1) / GraphicsManager.TileSize + 1 - (int)Math.Floor((float)sightStart.X / GraphicsManager.TileSize), (sightEnd.Y - 1) / GraphicsManager.TileSize + 1 - (int)Math.Floor((float)sightStart.Y / GraphicsManager.TileSize));

                if ((FocusedCharacter.GetCharSight() != Map.SightRange.Clear || FocusedCharacter.GetTileSight() != Map.SightRange.Clear) && !SeeAll)
                {
                    Loc seen = getDrawSight();
                    for (int xx = 0; xx < charSightValues.Length; xx++)
                    {
                        for (int yy = 0; yy < charSightValues[xx].Length; yy++)
                            charSightValues[xx][yy] = 0f;
                    }

                    //get all tile sight values and put them on the array
                    foreach (Character member in ActiveTeam.Players)
                    {
                        if (!member.Dead || FocusedCharacter == member)
                        {
                            foreach (VisionLoc visionLoc in member.GetVisionLocs())
                            {
                                if (visionLoc.Weight > 0)
                                    AddSeenLocs(visionLoc, member.GetCharSight());
                            }
                        }
                    }
                }

            }
        }

        public bool AnimationsOver()
        {
            if (ZoneManager.Instance.CurrentMap != null)
            {
                //update the team/enemies
                foreach (Character character in ZoneManager.Instance.CurrentMap.IterateCharacters())
                {
                    if (DiagManager.Instance.CurSettings.BattleFlow < Settings.BattleSpeed.VeryFast && !character.Dead && !character.ActionDone || character.OccupiedwithAction())
                        return false;
                }
            }
            return true;
        }

        protected override bool CanSeeTile(int xx, int yy)
        {
            if (SeeAll)
                return true;
            if (FocusedCharacter.GetTileSight() == Map.SightRange.Clear)
                return true;
            bool outOfBounds = !Collision.InBounds(ZoneManager.Instance.CurrentMap.Width, ZoneManager.Instance.CurrentMap.Height, new Loc(xx, yy));
            return !outOfBounds && (ZoneManager.Instance.CurrentMap.DiscoveryArray[xx][yy] == Map.DiscoveryState.Traversed);
        }

        protected override void PrepareTileDraw(SpriteBatch spriteBatch, int xx, int yy)
        {
            base.PrepareTileDraw(spriteBatch, xx, yy);

            if (Turn && !ZoneManager.Instance.CurrentMap.TileBlocked(new Loc(xx, yy), FocusedCharacter.Mobility))
            {
                if (Collision.InFront(FocusedCharacter.CharLoc, new Loc(xx, yy), FocusedCharacter.CharDir, -1))
                    GraphicsManager.Tiling.DrawTile(spriteBatch, new Vector2(xx * GraphicsManager.TileSize - ViewRect.X, yy * GraphicsManager.TileSize - ViewRect.Y), 1, 0);
                else
                    GraphicsManager.Tiling.DrawTile(spriteBatch, new Vector2(xx * GraphicsManager.TileSize - ViewRect.X, yy * GraphicsManager.TileSize - ViewRect.Y), 0, 0);
            }
        }

        protected override void PrepareFrontDraw()
        {
            //draw hitboxes on top
            foreach (Hitbox hitbox in Hitboxes)
            {
                if (CanSeeSprite(ViewRect, hitbox))
                    AddToDraw(frontDraw, hitbox);
            }

            base.PrepareFrontDraw();
        }

        protected override bool CanSeeCharOnScreen(Character character)
        {
            if (!base.CanSeeCharOnScreen(character))
                return false;
            if (SeeAll)
                return true;

            foreach (Character member in ActiveTeam.Players)
            {
                if (member.SeeAllChars)
                    return true;
                else if (!member.Dead || member == FocusedCharacter)
                {
                    if (member == character)
                        return true;
                    else if (!character.Invis)
                    {
                        Map.SightRange sight = member.GetCharSight();
                        foreach (Loc loc in character.GetLocsVisible())
                        {
                            if (Collision.InBounds(sightRect, loc))
                            {
                                if (sight == Map.SightRange.Clear)
                                    return true;
                                else
                                {
                                    Loc dest = loc - sightRect.Start;
                                    if (charSightValues[dest.X][dest.Y] > 0f)
                                        return true;
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }

        protected override bool CanSeeHiddenItems()
        {
            if (SeeAll)
                return true;

            foreach (Character member in ActiveTeam.Players)
            {
                if (member.SeeWallItems)
                    return true;
            }
            return false;
        }

        protected override void DrawItems(SpriteBatch spriteBatch, bool showHiddenItem)
        {
            base.DrawItems(spriteBatch, showHiddenItem);

            //draw pickup items
            foreach (PickupItem item in PickupItems)
            {
                if (CanSeeSprite(ViewRect, item))
                {
                    TerrainData terrain = ZoneManager.Instance.CurrentMap.Tiles[item.TileLoc.X][item.TileLoc.Y].Data.GetData();
                    if (terrain.BlockType == TerrainData.Mobility.Impassable || terrain.BlockType == TerrainData.Mobility.Block)
                    {
                        if (showHiddenItem)
                            item.Draw(spriteBatch, ViewRect.Start, Color.White * 0.5f);
                    }
                    else if (ZoneManager.Instance.CurrentMap.DiscoveryArray[item.TileLoc.X][item.TileLoc.Y] == Map.DiscoveryState.Traversed)
                    {
                        if (terrain.BlockType == TerrainData.Mobility.Passable)
                            item.Draw(spriteBatch, ViewRect.Start, Color.White);
                        else
                            item.Draw(spriteBatch, ViewRect.Start, Color.White * 0.5f);
                    }
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (ZoneManager.Instance.CurrentMap != null)
            {

                if (ShowMap != MinimapState.Detail)
                    DrawGame(spriteBatch);


                if ((ShowMap != MinimapState.None) && !Turn && !ShowActions && !DataManager.Instance.Save.CutsceneMode && MenuManager.Instance.MenuCount == 0)
                {
                    //draw minimap
                    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, Matrix.CreateScale(new Vector3(matrixScale, matrixScale, 1)));

                    TileSheet mapSheet = GraphicsManager.MapSheet;

                    Vector2 mapStart = new Vector2(0, 16);
                    uint mobility = 0;
                    if (ShowMap == MinimapState.Detail)
                    {
                        mobility |= (1U << (int)TerrainData.Mobility.Water);
                        mobility |= (1U << (int)TerrainData.Mobility.Lava);
                        mobility |= (1U << (int)TerrainData.Mobility.Abyss);
                    }

                    Loc startLoc = new Loc();
                    if (FocusedCharacter != null)
                        startLoc = FocusedCharacter.CharLoc;
                    startLoc = new Loc(Math.Max(0, Math.Min(startLoc.X - MAX_MINIMAP_WIDTH / 2, ZoneManager.Instance.CurrentMap.Width - MAX_MINIMAP_WIDTH)),
                        Math.Max(0, Math.Min(startLoc.Y - MAX_MINIMAP_HEIGHT / 2, ZoneManager.Instance.CurrentMap.Height - MAX_MINIMAP_HEIGHT)));

                    for (int ii = startLoc.X; ii < ZoneManager.Instance.CurrentMap.Width && ii - startLoc.X < MAX_MINIMAP_WIDTH; ii++)
                    {
                        for (int jj = startLoc.Y; jj < ZoneManager.Instance.CurrentMap.Height && jj - startLoc.Y < MAX_MINIMAP_HEIGHT; jj++)
                        {
                            Map.DiscoveryState discovery = ZoneManager.Instance.CurrentMap.DiscoveryArray[ii][jj];
                            if (SeeAll)
                                discovery = Map.DiscoveryState.Traversed;
                            if (discovery != Map.DiscoveryState.None)
                            {
                                Vector2 destVector = mapStart + (new Vector2(ii, jj) - startLoc.ToVector2()) * new Vector2(mapSheet.TileWidth, mapSheet.TileHeight);
                                Tile tile = ZoneManager.Instance.CurrentMap.Tiles[ii][jj];
                                TerrainData terrain = tile.Data.GetData();
                                if (ShowMap == MinimapState.Detail)
                                {
                                    if (terrain.BlockType == TerrainData.Mobility.Water)
                                        GraphicsManager.Pixel.Draw(spriteBatch, destVector, null, Color.Blue, new Vector2(mapSheet.TileWidth, mapSheet.TileHeight));
                                    else if (terrain.BlockType == TerrainData.Mobility.Lava)
                                        GraphicsManager.Pixel.Draw(spriteBatch, destVector, null, Color.DarkOrange, new Vector2(mapSheet.TileWidth, mapSheet.TileHeight));
                                    else if (terrain.BlockType == TerrainData.Mobility.Abyss)
                                        GraphicsManager.Pixel.Draw(spriteBatch, destVector, null, Color.Gray, new Vector2(mapSheet.TileWidth, mapSheet.TileHeight));
                                }

                                if (!ZoneManager.Instance.CurrentMap.TileBlocked(new Loc(ii, jj), mobility))
                                {
                                    //draw halls
                                    if (ZoneManager.Instance.CurrentMap.TerrainBlocked(new Loc(ii, jj - 1), mobility))
                                        mapSheet.DrawTile(spriteBatch, destVector, 0, 0, discovery == Map.DiscoveryState.Traversed ? Color.White : Color.DarkGray);
                                    if (ZoneManager.Instance.CurrentMap.TerrainBlocked(new Loc(ii, jj + 1), mobility))
                                        mapSheet.DrawTile(spriteBatch, destVector, 0, 1, discovery == Map.DiscoveryState.Traversed ? Color.White : Color.DarkGray);
                                    if (ZoneManager.Instance.CurrentMap.TerrainBlocked(new Loc(ii - 1, jj), mobility))
                                        mapSheet.DrawTile(spriteBatch, destVector, 1, 0, discovery == Map.DiscoveryState.Traversed ? Color.White : Color.DarkGray);
                                    if (ZoneManager.Instance.CurrentMap.TerrainBlocked(new Loc(ii + 1, jj), mobility))
                                        mapSheet.DrawTile(spriteBatch, destVector, 1, 1, discovery == Map.DiscoveryState.Traversed ? Color.White : Color.DarkGray);
                                }

                                if (discovery == Map.DiscoveryState.Traversed && tile.Effect.ID > -1 && (tile.Effect.Exposed || SeeAll))
                                {
                                    TileData entry = DataManager.Instance.GetTile(tile.Effect.ID);

                                    //draw tiles
                                    if (!tile.Effect.Revealed)
                                        entry = DataManager.Instance.GetTile(0);
                                    mapSheet.DrawTile(spriteBatch, destVector, entry.MinimapIcon.X, entry.MinimapIcon.Y, entry.MinimapColor);
                                }
                            }
                        }
                    }

                    foreach (MapItem item in ZoneManager.Instance.CurrentMap.Items)
                    {
                        bool seeItem = false;
                        if (SeeAll)
                            seeItem = true;
                        else
                        {
                            TerrainData terrain = ZoneManager.Instance.CurrentMap.Tiles[item.TileLoc.X][item.TileLoc.Y].Data.GetData();
                            if (ZoneManager.Instance.CurrentMap.DiscoveryArray[item.TileLoc.X][item.TileLoc.Y] == Map.DiscoveryState.Traversed &&
                                !(terrain.BlockType == TerrainData.Mobility.Impassable || terrain.BlockType == TerrainData.Mobility.Block))
                            {
                                seeItem = true;
                            }
                            else
                            {
                                foreach (Character member in ActiveTeam.Players)
                                {
                                    if (member.SeeWallItems)
                                    {
                                        if (member.CanSeeLoc(item.TileLoc, Map.SightRange.Clear))
                                        {
                                            seeItem = true;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        if (seeItem)
                            mapSheet.DrawTile(spriteBatch, mapStart + (new Vector2(item.TileLoc.X, item.TileLoc.Y) - startLoc.ToVector2()) * new Vector2(mapSheet.TileWidth, mapSheet.TileHeight), 3, 0, Color.Cyan);
                    }

                    foreach (Team team in ZoneManager.Instance.CurrentMap.MapTeams)
                    {
                        foreach (Character character in team.Players)
                        {
                            if (!character.Dead)
                            {
                                bool seen = false;
                                foreach (Character player in ActiveTeam.Players)
                                {
                                    if (!player.Dead && player.CanSeeCharacter(character))
                                    {
                                        seen = true;
                                        break;
                                    }
                                }
                                if (seen || SeeAll)
                                {
                                    SkinData skinData = DataManager.Instance.GetSkin(character.Appearance.Skin);
                                    mapSheet.DrawTile(spriteBatch, mapStart + (new Vector2(character.CharLoc.X, character.CharLoc.Y) - startLoc.ToVector2()) * new Vector2(mapSheet.TileWidth, mapSheet.TileHeight), 3, 0, skinData.MinimapColor);
                                }
                            }
                        }
                    }

                    foreach (Team team in ZoneManager.Instance.CurrentMap.AllyTeams)
                    {
                        foreach (Character character in team.EnumerateChars())
                        {
                            if (!character.Dead)
                            {
                                bool seen = false;
                                foreach (Character player in ActiveTeam.Players)
                                {
                                    if (!player.Dead && player.CanSeeCharacter(character))
                                    {
                                        seen = true;
                                        break;
                                    }
                                }
                                if (seen || SeeAll)
                                    mapSheet.DrawTile(spriteBatch, mapStart + (new Vector2(character.CharLoc.X, character.CharLoc.Y) - startLoc.ToVector2()) * new Vector2(mapSheet.TileWidth, mapSheet.TileHeight), 3, 0, Color.Green);
                            }
                        }
                    }

                    foreach(Character player in ActiveTeam.EnumerateChars())
                    {
                        if (!player.Dead)
                        {
                            mapSheet.DrawTile(spriteBatch, mapStart + (new Vector2(player.CharLoc.X, player.CharLoc.Y) - startLoc.ToVector2()) * new Vector2(mapSheet.TileWidth, mapSheet.TileHeight),
                                3, (player == ActiveTeam.Leader) ? ((GraphicsManager.TotalFrameTick / (ulong)FrameTick.FrameToTick(10) % 2 == 0) ? 0 : 1) : 0,
                                (player == ActiveTeam.Leader) ? Color.White : Color.Yellow);
                        }
                    }

                    spriteBatch.End();
                }
            }

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, Matrix.CreateScale(new Vector3(matrixScale, matrixScale, 1)));



            //HP bars
            if (!Turn && !DataManager.Instance.Save.CutsceneMode)
            {
                if (ActiveTeam.Players.Count > 1)
                {
                    int players = Math.Min(ActiveTeam.Players.Count, ExplorerTeam.MAX_TEAM_SLOTS);
                    for (int ii = 0; ii < players; ii++)
                    {
                        int start = GraphicsManager.ScreenWidth / players * ii;
                        Color digitColor = Color.White;
                        if (!ActiveTeam.Players[ii].Dead)
                        {
                            if (ActiveTeam.Players[ii].Fullness <= 0 && (GraphicsManager.TotalFrameTick / (ulong)FrameTick.FrameToTick(20) % 2 == 0))
                                digitColor = new Color(255, 239, 90);
                            if (ActiveTeam.Players[ii].HP * 4 <= ActiveTeam.Players[ii].MaxHP && (GraphicsManager.TotalFrameTick / (ulong)FrameTick.FrameToTick(20) % 2 == 1))
                                digitColor = new Color(0, 0, 0);
                        }
                        GraphicsManager.HPMenu.DrawTile(spriteBatch, new Vector2(start, 0), 0, 1, (digitColor == Color.White) ? new Color(88, 248, 88) : digitColor);
                        GraphicsManager.HPMenu.DrawTile(spriteBatch, new Vector2(start + 8, 0), 1, 1, (digitColor == Color.White) ? new Color(88, 248, 88) : digitColor);
                        GraphicsManager.HPMenu.DrawTile(spriteBatch, new Vector2(start + 13, 0), ii + 1, 0, (digitColor == Color.White) ? new Color(88, 248, 88) : digitColor);
                        DrawHP(spriteBatch, start + 22, GraphicsManager.ScreenWidth / players - 26, ActiveTeam.Players[ii].HP, ActiveTeam.Players[ii].MaxHP, digitColor);
                    }
                }
                else if (ActiveTeam.Players.Count == 1)
                {
                    Color digitColor = Color.White;
                    if (!ActiveTeam.Players[0].Dead)
                    {
                        if (ActiveTeam.Players[0].Fullness <= 0 && (GraphicsManager.TotalFrameTick / (ulong)FrameTick.FrameToTick(20) % 2 == 0))
                            digitColor = new Color(255, 239, 90);
                        if (ActiveTeam.Players[0].HP * 4 <= ActiveTeam.Players[0].MaxHP && (GraphicsManager.TotalFrameTick / (ulong)FrameTick.FrameToTick(20) % 2 == 1))
                            digitColor = new Color(0, 0, 0);
                    }
                    GraphicsManager.HPMenu.DrawTile(spriteBatch, new Vector2(0, 0), 0, 1, (digitColor == Color.White) ? new Color(88, 248, 88) : digitColor);
                    GraphicsManager.HPMenu.DrawTile(spriteBatch, new Vector2(8, 0), 1, 1, (digitColor == Color.White) ? new Color(88, 248, 88) : digitColor);
                    DrawHP(spriteBatch, 14, GraphicsManager.ScreenWidth / 2, ActiveTeam.Players[0].HP, ActiveTeam.Players[0].MaxHP, digitColor);
                }
            }

            if (MenuManager.Instance.MenuCount == 0)
            {
                if (ShowActions)
                {
                    for (int ii = 0; ii < CharData.MAX_SKILL_SLOTS; ii++)
                        ShownHotkeys[ii].Draw(spriteBatch);
                }
                else
                {
                    LiveBattleLog.Draw(spriteBatch);
                    TeamModeNote.Draw(spriteBatch);
                }
            }

            spriteBatch.End();
        }


        protected override void PostDraw(SpriteBatch spriteBatch)
        {
            //draw foreground (includes darkness)
            //prepare grid
            if ((FocusedCharacter.GetCharSight() != Map.SightRange.Clear || FocusedCharacter.GetTileSight() != Map.SightRange.Clear) && !SeeAll)
            {
                Matrix matrix = Matrix.CreateScale(new Vector3(drawScale, drawScale, 1));
                //subtractive blending
                spriteBatch.Begin(SpriteSortMode.Deferred, subtractBlend, SamplerState.PointClamp, null, null, null, matrix);

                for (int jj = viewTileRect.Y; jj < viewTileRect.End.Y; jj++)
                {
                    for (int ii = viewTileRect.X; ii < viewTileRect.End.X; ii++)
                    {
                        //set tile sprite position
                        if (!Collision.InBounds(ZoneManager.Instance.CurrentMap.Width, ZoneManager.Instance.CurrentMap.Height, new Loc(ii, jj)))
                        {
                            if (FocusedCharacter.GetTileSight() == Map.SightRange.Clear)
                                GraphicsManager.Pixel.Draw(spriteBatch, new Vector2(ii * GraphicsManager.TileSize - ViewRect.X, jj * GraphicsManager.TileSize - ViewRect.Y), null, Color.White * DARK_TRANSPARENT, new Vector2(GraphicsManager.TileSize));
                        }
                        else if (!Collision.InBounds(sightRect, new Loc(ii, jj)))
                            GraphicsManager.Pixel.Draw(spriteBatch, new Vector2(ii * GraphicsManager.TileSize - ViewRect.X, jj * GraphicsManager.TileSize - ViewRect.Y), null, Color.White * DARK_TRANSPARENT, new Vector2(GraphicsManager.TileSize));
                        else
                        {
                            if (charSightValues[ii - sightRect.X][jj - sightRect.Y] < 1f)
                            {
                                GraphicsManager.Pixel.Draw(spriteBatch, new Vector2(ii * GraphicsManager.TileSize - ViewRect.X, jj * GraphicsManager.TileSize - ViewRect.Y), null,
                                    Color.White * DARK_TRANSPARENT * Math.Max(1f - charSightValues[ii - sightRect.X][jj - sightRect.Y], 0), new Vector2(GraphicsManager.TileSize));
                            }

                            for (int dir = 0; dir < DirExt.DIR8_COUNT; dir++)
                            {
                                foreach (VisionLoc tex in getDarknessTextures(new Loc(ii, jj), sightRect.Start, (Dir8)dir))
                                {
                                    //draw in correct location
                                    Loc dest = new Loc(ii * GraphicsManager.TileSize + GraphicsManager.TileSize / 3 - ViewRect.X, jj * GraphicsManager.TileSize + GraphicsManager.TileSize / 3 - ViewRect.Y)
                                        + ((Dir8)dir).GetLoc() * (GraphicsManager.TileSize / 3);
                                    GraphicsManager.Darkness.DrawTile(spriteBatch, dest.ToVector2(), tex.Loc.X, tex.Loc.Y, Color.White * tex.Weight);
                                }
                            }
                        }
                    }
                }

                spriteBatch.End();
            }
        }


        public override void DrawOverlay(SpriteBatch spriteBatch)
        {
            if (Turn)
            {
                foreach (Character hpChar in shownChars)
                {
                    Loc drawLoc = hpChar.CharLoc * GraphicsManager.TileSize - ViewRect.Start + new Loc(2, GraphicsManager.TileSize - 6);
                    GraphicsManager.MiniHP.Draw(spriteBatch, drawLoc.ToVector2(), null);
                    int hpAmount = (hpChar.HP * 18 - 1) / hpChar.MaxHP + 1;
                    Color hpColor = new Color(88, 248, 88);
                    if (hpChar.HP * 4 <= hpChar.MaxHP)
                        hpColor = new Color(248, 128, 88);
                    else if (hpChar.HP * 2 <= hpChar.MaxHP)
                        hpColor = new Color(248, 232, 88);
                    GraphicsManager.Pixel.Draw(spriteBatch, new Rectangle(drawLoc.X + 1, drawLoc.Y + 1, hpAmount, 2), null, hpColor);
                }
            }

            if (Diagonal)
            {
                int base_offset = (int)(GraphicsManager.TotalFrameTick / (ulong)FrameTick.FrameToTick(3) % 6);
                if (base_offset >= 4)
                    base_offset = 6 - base_offset;
                Vector2 arrow_offset = new Vector2(base_offset);
                Vector2 arrow_alt_offset = new Vector2(base_offset, -base_offset);
                GraphicsManager.Arrows.DrawTile(spriteBatch, new Vector2((GraphicsManager.ScreenWidth / scale - GraphicsManager.TileSize) / 2 - GraphicsManager.Arrows.TileWidth, (GraphicsManager.ScreenHeight / scale - GraphicsManager.TileSize) / 2 - GraphicsManager.Arrows.TileHeight) - arrow_offset, 0, 0);
                GraphicsManager.Arrows.DrawTile(spriteBatch, new Vector2((GraphicsManager.ScreenWidth / scale + GraphicsManager.TileSize) / 2, (GraphicsManager.ScreenHeight / scale - GraphicsManager.TileSize) / 2 - GraphicsManager.Arrows.TileHeight) + arrow_alt_offset, 2, 0);
                GraphicsManager.Arrows.DrawTile(spriteBatch, new Vector2((GraphicsManager.ScreenWidth / scale - GraphicsManager.TileSize) / 2 - GraphicsManager.Arrows.TileWidth, (GraphicsManager.ScreenHeight / scale + GraphicsManager.TileSize) / 2) - arrow_alt_offset, 0, 2);
                GraphicsManager.Arrows.DrawTile(spriteBatch, new Vector2((GraphicsManager.ScreenWidth / scale + GraphicsManager.TileSize) / 2, (GraphicsManager.ScreenHeight / scale + GraphicsManager.TileSize) / 2) + arrow_offset, 2, 2);
            }



            //draw example texture
            if (DebugAsset != GraphicsManager.AssetType.None)
            {
                switch (DebugAsset)
                {
                    case GraphicsManager.AssetType.VFX:
                        DirSheet dirSheet = GraphicsManager.GetAttackSheet(DebugAnim);
                        dirSheet.DrawDir(spriteBatch, new Vector2(GraphicsManager.ScreenWidth / scale / 2 - dirSheet.TileWidth / 2, GraphicsManager.ScreenHeight / scale / 2 - dirSheet.TileHeight / 2),
                            (int)(GraphicsManager.TotalFrameTick / (ulong)FrameTick.FrameToTick(1) % (ulong)dirSheet.TotalFrames),
                            FocusedCharacter.CharDir, Color.White);

                        break;
                }
            }
        }

        public override void DrawDebug(SpriteBatch spriteBatch)
        {
            base.DrawDebug(spriteBatch);
            if (FocusedCharacter != null)
            {
                PortraitSheet sheet = GraphicsManager.GetPortrait(FocusedCharacter.Appearance);
                sheet.DrawPortrait(spriteBatch, new Vector2(0, GraphicsManager.WindowHeight - GraphicsManager.PortraitSize), new EmoteStyle(DebugEmote));

                GraphicsManager.SysFont.DrawText(spriteBatch, GraphicsManager.WindowWidth - 2, 52, String.Format("Z:{0:D3} S:{1:D3} M:{2:D3}", ZoneManager.Instance.CurrentZoneID, ZoneManager.Instance.CurrentMapID.Segment, ZoneManager.Instance.CurrentMapID.ID), null, DirV.Up, DirH.Right, Color.White);
                GraphicsManager.SysFont.DrawText(spriteBatch, GraphicsManager.WindowWidth - 2, 62, String.Format("X:{0:D3} Y:{1:D3}", FocusedCharacter.CharLoc.X, FocusedCharacter.CharLoc.Y), null, DirV.Up, DirH.Right, Color.White);

                MonsterID monId;
                Loc offset;
                int anim;
                int currentHeight, currentTime, currentFrame;
                FocusedCharacter.GetCurrentSprite(out monId, out offset, out currentHeight, out anim, out currentTime, out currentFrame);

                CharSheet charSheet = GraphicsManager.GetChara(FocusedCharacter.Appearance);
                Color frameColor = Color.White;
                if (charSheet.IsAnimCopied(anim))
                    frameColor = Color.Yellow;
                if (!charSheet.HasOwnAnim(anim))
                    frameColor = Color.Gray;
                GraphicsManager.SysFont.DrawText(spriteBatch, GraphicsManager.WindowWidth - 2, 72, String.Format("{0}:{1}:{2}", GraphicsManager.Actions[anim].Name, FocusedCharacter.CharDir.ToString(), currentFrame), null, DirV.Up, DirH.Right, frameColor);
                GraphicsManager.SysFont.DrawText(spriteBatch, GraphicsManager.WindowWidth - 2, 82, String.Format("Frame {0:D3}", currentTime), null, DirV.Up, DirH.Right, Color.White);

            }

            if (ZoneManager.Instance.CurrentMap != null)
                GraphicsManager.SysFont.DrawText(spriteBatch, GraphicsManager.WindowWidth - 2, 92, String.Format("Turn {0:D4}", ZoneManager.Instance.CurrentMap.MapTurns), null, DirV.Up, DirH.Right, Color.White);
            GraphicsManager.SysFont.DrawText(spriteBatch, GraphicsManager.WindowWidth - 2, 102, String.Format("Total {0:D6}", DataManager.Instance.Save.TotalTurns), null, DirV.Up, DirH.Right, Color.White);

            //if (GodMode)
            //    GraphicsManager.SysFont.DrawText(spriteBatch, 2, 72, "God Mode", null, DirV.Up, DirH.Right, Color.LightYellow);
            if (SeeAll)
                GraphicsManager.SysFont.DrawText(spriteBatch, 2, 82, "See All", null, DirV.Up, DirH.Right, Color.LightYellow);

        }




        public void DrawHP(SpriteBatch spriteBatch, int startX, int lengthX, int hp, int maxHP, Color digitColor)
        {
            //bars
            Color color = new Color(88, 248, 88);
            if (hp * 4 <= maxHP)
                color = new Color(248, 128, 88);
            else if (hp * 2 <= maxHP)
                color = new Color(248, 232, 88);

            int size = (int)Math.Ceiling((double)hp * (lengthX - 2) / maxHP);
            GraphicsManager.Pixel.Draw(spriteBatch, new Rectangle(startX + 1, 0, size, 8), null, color);
            GraphicsManager.HPMenu.DrawTile(spriteBatch, new Rectangle(startX + 1, 0, lengthX - 2, 8), 4, 1, Color.White);
            GraphicsManager.HPMenu.DrawTile(spriteBatch, new Vector2(startX - 8 + 1, 0), 3, 1);
            GraphicsManager.HPMenu.DrawTile(spriteBatch, new Vector2(startX + lengthX - 1, 0), 5, 1);
            
            //numbers
            int total_digits = 0;
            int test_hp = maxHP;
            while (test_hp > 0)
            {
                test_hp /= 10;
                total_digits++;
            }
            int digitX = startX + 22 + 8 * total_digits;
            test_hp = maxHP;
            while (test_hp > 0)
            {
                int digit = test_hp % 10;
                GraphicsManager.HPMenu.DrawTile(spriteBatch, new Vector2(digitX, 8), digit, 0);

                test_hp /= 10;
                digitX -= 8;
            }
            GraphicsManager.HPMenu.DrawTile(spriteBatch, new Vector2(startX + 24, 8), 2, 1, (digitColor == Color.White) ? new Color(248, 128, 88) : digitColor);
            digitX = startX + 16;
            test_hp = hp;
            while (test_hp > 0)
            {
                int digit = test_hp % 10;
                GraphicsManager.HPMenu.DrawTile(spriteBatch, new Vector2(digitX, 8), digit, 0);

                test_hp /= 10;
                digitX -= 8;
            }
            if (hp == 0)
                GraphicsManager.HPMenu.DrawTile(spriteBatch, new Vector2(digitX, 8), 0, 0);
        }

        public void AddSeenLocs(VisionLoc loc, Map.SightRange sight)
        {
            //needs to be edited according to FOV
            Loc seen = Character.GetSightDims() + new Loc(1);
            Loc minLoc = new Loc(Math.Max(sightRect.X, loc.Loc.X - seen.X), Math.Max(sightRect.Y, loc.Loc.Y - seen.Y));
            Loc addLoc = new Loc(Math.Min(sightRect.End.X, loc.Loc.X + seen.X + 1), Math.Min(sightRect.End.Y, loc.Loc.Y + seen.Y + 1)) - minLoc;
            switch (sight)
            {
                case Map.SightRange.Blind:
                    break;
                case Map.SightRange.Murky:
                    {
                        for (int x = -1; x <= 1; x++)
                        {
                            for (int y = -1; y <= 1; y++)
                            {
                                if (Collision.InBounds(ZoneManager.Instance.CurrentMap.Width, ZoneManager.Instance.CurrentMap.Height, loc.Loc + new Loc(x, y))
                                    && Collision.InBounds(minLoc, addLoc, new Loc(x, y) + loc.Loc))
                                    charSightValues[loc.Loc.X + x - sightRect.X][loc.Loc.Y + y - sightRect.Y] += loc.Weight;
                            }
                        }
                        break;
                    }
                case Map.SightRange.Dark:
                    {
                        CalculateSymmetricFOV(minLoc, addLoc, loc);
                        break;
                    }
                default:
                    {
                        for (int x = 0; x < addLoc.X; x++)
                        {
                            for (int y = 0; y < addLoc.Y; y++)
                            {
                                if (Collision.InBounds(ZoneManager.Instance.CurrentMap.Width, ZoneManager.Instance.CurrentMap.Height, minLoc + new Loc(x, y)))
                                    charSightValues[minLoc.X + x - sightRect.X][minLoc.Y + y - sightRect.Y] += loc.Weight;
                            }
                        }
                        break;
                    }
            }
        }


        public void CalculateSymmetricFOV(Loc rectStart, Loc rectSize, VisionLoc start)
        {
            Fov.LightOperation lightOp = (int locX, int locY, float light) =>
            {
                charSightValues[locX - sightRect.X][locY - sightRect.Y] += start.Weight;
            };
            Fov.CalculateAnalogFOV(rectStart, rectSize, start.Loc, VisionBlocked, lightOp);
        }


        private IEnumerable<VisionLoc> getDarknessTextures(Loc loc, Loc rectStart, Dir8 dir)
        {
            //y0 = inward, y1 = outward, y2 = wall
            float curLight = getBrightnessValue(loc, rectStart);
            if (dir.IsDiagonal())
            {
                DirH horiz;
                DirV vert;
                dir.Separate(out horiz, out vert);
                Loc horizLoc = loc + horiz.GetLoc();
                Loc vertLoc = loc + vert.GetLoc();
                Loc diagLoc = loc + dir.GetLoc();
                float horizLight = getBrightnessValue(horizLoc, rectStart);
                float vertLight = getBrightnessValue(vertLoc, rectStart);
                float diagLight = getBrightnessValue(diagLoc, rectStart);
                float darkest = curLight;
                //if any of the 4-dir lights are darker than
                if (horizLight == vertLight)
                {
                    if (curLight > horizLight)
                    {
                        //just one inward texture
                        yield return new VisionLoc(new Loc((int)dir.Reverse() / 2, 0), curLight - horizLight);
                        darkest = horizLight;
                    }
                }
                else if (curLight > horizLight && curLight > vertLight)
                {
                    //one texture of inwards, one texture straight
                    float diff = horizLight - vertLight;
                    float max = Math.Max(horizLight, vertLight);
                    //inward texture is always the lightest
                    yield return new VisionLoc(new Loc((int)dir.Reverse() / 2, 0), curLight - max);
                    if (diff < 0)
                    {
                        yield return new VisionLoc(new Loc((int)horiz.ToDir4().Reverse(), 2), max - horizLight);
                        darkest = horizLight;
                    }
                    else
                    {
                        yield return new VisionLoc(new Loc((int)vert.ToDir4().Reverse(), 2), max - vertLight);
                        darkest = vertLight;
                    }
                }
                else if (curLight > horizLight)
                {
                    yield return new VisionLoc(new Loc((int)horiz.ToDir4().Reverse(), 2), curLight - horizLight);
                    darkest = horizLight;
                }
                else if (curLight > vertLight)
                {
                    yield return new VisionLoc(new Loc((int)vert.ToDir4().Reverse(), 2), curLight - vertLight);
                    darkest = vertLight;
                }

                //check if the diag light is darker than the darkest so far
                if (darkest > diagLight)
                    yield return new VisionLoc(new Loc((int)dir.Reverse() / 2, 1), darkest - diagLight);
            }
            else
            {
                Loc diagLoc = loc + dir.GetLoc();
                float diff = curLight - getBrightnessValue(diagLoc, rectStart);
                if (diff > 0)
                    yield return new VisionLoc(new Loc((int)dir.Reverse() / 2, 2), diff);
            }
        }

        private float getBrightnessValue(Loc loc, Loc rectStart)
        {
            if (SeeAll)
                return 1f;

            //if it's out of bounds, use the decided-on darkness of out-of-bounds
            if (!Collision.InBounds(ZoneManager.Instance.CurrentMap.Width, ZoneManager.Instance.CurrentMap.Height, loc))
            {
                if (FocusedCharacter.GetTileSight() == Map.SightRange.Clear)
                {
                    if (FocusedCharacter.GetCharSight() != Map.SightRange.Clear)
                        return 1f - DARK_TRANSPARENT;
                    else
                        return 1f;
                }
                else
                    return 0f;
            }

            //if it's undiscovered, it's black
            if (ZoneManager.Instance.CurrentMap.DiscoveryArray[loc.X][loc.Y] != Map.DiscoveryState.Traversed)
                return 0f;

            //otherwise, use fade value
            Loc dest = loc - rectStart;
            if (!Collision.InBounds(charSightValues.Length, charSightValues[0].Length, dest))
                return 1f - DARK_TRANSPARENT;

            return (1f - DARK_TRANSPARENT) + DARK_TRANSPARENT * Math.Min(1f, charSightValues[dest.X][dest.Y]);
        }


        static Loc getDrawSight()
        {
            return new Loc(15, 12);
            //return Character.GetSightDims() * 2 + new Loc(1, 2);
        }

        public IEnumerator<YieldInstruction> MoveCamera(Loc loc, int time)
        {
            Loc startLoc = ZoneManager.Instance.CurrentMap.ViewCenter.HasValue ? ZoneManager.Instance.CurrentMap.ViewCenter.Value : FocusedCharacter.MapLoc + new Loc(GraphicsManager.TileSize / 2, GraphicsManager.TileSize / 3);
            int currentFadeTime = time;
            while (currentFadeTime > 0)
            {
                currentFadeTime--;
                ZoneManager.Instance.CurrentMap.ViewCenter = new Loc(AnimMath.Lerp(loc.X, startLoc.X, (double)currentFadeTime / time), AnimMath.Lerp(loc.Y, startLoc.Y, (double)currentFadeTime / time));
                yield return new WaitForFrames(1);
            }
        }


        public void Missed(Loc loc)
        {
            Anims[(int)DrawLayer.Top].Add(new TextAnim(loc, "MISS", GraphicsManager.DamageFont));
        }

        public void MeterChanged(Loc loc, int amt, bool exp)
        {
            if (exp)
                Anims[(int)DrawLayer.Top].Add(new TextAnim(loc, amt.ToString(), GraphicsManager.EXPFont));
            else if (amt < 0)
                Anims[(int)DrawLayer.Top].Add(new TextAnim(loc, (-amt).ToString(), GraphicsManager.DamageFont));
            else if (amt > 0)
                Anims[(int)DrawLayer.Top].Add(new TextAnim(loc, amt.ToString(), GraphicsManager.HealFont));
        }

        public void LogPickup(PickupItem pickup)
        {
            Loc? originLoc = null;
            if (pickup.LocalMsg)
                originLoc = pickup.TileLoc;
            if (pickup.WaitingChar.OccupiedwithAction())
            {
                //in order to delay pickups to the right time, the following needs to occur:
                //first, change this log message to internal-only
                LogMsg(pickup.PickupMessage, true, false, originLoc, null, null);
                //then, add to the list in this processor that keeps tabs of picked-up objects
                //picked up objects are to be stored as a struct containing their sprite, name, and character
                //hold off actually making the message until after landing
                PickupItems.Add(pickup);
            }
            else
            {
                GameManager.Instance.SE(pickup.Sound);
                LogMsg(pickup.PickupMessage, false, false, originLoc, null, null);
            }
        }

        public void LogMsg(string msg)
        {
            LogMsg(msg, false, false);
        }

        public void LogMsg(string msg, Loc origin)
        {
            LogMsg(msg, false, false, origin, null, null);
        }
        public void LogMsg(string msg, Character targetChar)
        {
            LogMsg(msg, false, false, targetChar, null);
        }
        public void LogMsg(string msg, bool silent, bool logSilent, Character targetChar, Team team)
        {
            LogMsg(msg, silent, logSilent, null, targetChar, team);
        }

        public void LogMsg(string msg, bool silent, bool logSilent, Loc? origin, Character targetChar, Team team)
        {
            if (team != null && team != ActiveTeam)
                return;

            bool heard = false;
            if (origin == null && targetChar == null)
                heard = true;

            if (origin != null)
            {
                if (CanTeamSeeLoc(ActiveTeam, origin.Value))
                    heard = true;
            }
            if (targetChar != null)
            {
                if (CanTeamSeeChar(ActiveTeam, targetChar))
                    heard = true;
            }

            if (heard)
                LogMsg(msg, silent, logSilent);
        }


        public void LogMsg(string msg, bool silent, bool logSilent)
        {
            //remove tags such as pauses
            int tabIndex = msg.IndexOf("[pause=", 0, StringComparison.OrdinalIgnoreCase);
            while (tabIndex > -1)
            {
                int endIndex = msg.IndexOf("]", tabIndex);
                if (endIndex == -1)
                    break;
                int param;
                if (Int32.TryParse(msg.Substring(tabIndex + "[pause=".Length, endIndex - (tabIndex + "[pause=".Length)), out param))
                {
                    TextPause pause = new TextPause();
                    pause.LetterIndex = tabIndex;
                    pause.Time = param;
                    msg = msg.Remove(tabIndex, endIndex - tabIndex + "]".Length);

                    tabIndex = msg.IndexOf("[pause=", tabIndex, StringComparison.OrdinalIgnoreCase);
                }
                else
                    break;
            }

            if (msg == "\n")
            {
                if (DataManager.Instance.MsgLog.Count == 0 || DataManager.Instance.MsgLog[DataManager.Instance.MsgLog.Count - 1] == "\n")
                    return;
            }
            else if (String.IsNullOrWhiteSpace(msg))
                return;

            if (!logSilent)
                DataManager.Instance.MsgLog.Add(msg);
            if (!silent)
                LiveBattleLog.LogAdded(msg);
        }

    }
}
