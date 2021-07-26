using System;
using System.Collections.Generic;
using RogueEssence.Content;
using RogueElements;
using RogueEssence.Data;
using RogueEssence.Menu;
using RogueEssence.Dungeon;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RogueEssence.Ground
{
    //The game engine for Ground Mode, in which the player has free movement
    public partial class GroundScene : BaseGroundScene
    {
        private static GroundScene instance;
        public static void InitInstance()
        {
            if (instance != null)
                GraphicsManager.ZoomChanged -= instance.ZoomChanged;
            instance = new GroundScene();
            GraphicsManager.ZoomChanged += instance.ZoomChanged;
        }
        public static GroundScene Instance { get { return instance; } }

        public int DebugEmote;

        public GroundChar FocusedCharacter
        {
            get
            {
                return ZoneManager.Instance.CurrentGround.ActiveChar;
            }
        }

        public IEnumerator<YieldInstruction> PendingLeaderAction;

        public GroundScene()
        {

        }

        public override void Begin()
        {
            PendingLeaderAction = null;
            base.Begin();
        }

        private IEnumerator<YieldInstruction> test()
        {
            DebugEmote = (DebugEmote + 1) % GraphicsManager.Emotions.Count;

            //string targetUUID = "";
            //yield return CoroutineManager.Instance.StartCoroutine(MenuManager.Instance.ProcessMenuCoroutine(new ContactInputMenu((uuid) => targetUUID = uuid)));

            //if (targetUUID == "")
            //    yield break;

            //ActivityTradeTeam activity = new ActivityTradeTeam(new ServerInfo("", "127.0.0.1", 28911), DataManager.Instance.Save.CreateContactInfo(), new ContactInfo("", targetUUID));
            //NetworkManager.Instance.PrepareActivity(activity);

            //bool connected = false;
            //yield return CoroutineManager.Instance.StartCoroutine(MenuManager.Instance.ProcessMenuCoroutine(new ConnectingMenu(() => connected = true)));

            //if (!connected)
            //    yield break;

            //yield return CoroutineManager.Instance.StartCoroutine(MenuManager.Instance.ProcessMenuCoroutine(new GetHelpMenu()));

            yield break;
        }

        public override void UpdateMeta()
        {
            base.UpdateMeta();
            InputManager input = GameManager.Instance.MetaInputManager;

            if (input.JustPressed(FrameInput.InputType.Test))
                PendingLeaderAction = test();

            if (input.JustReleased(FrameInput.InputType.RightMouse) && input[FrameInput.InputType.Ctrl])
            {
                Loc coords = ScreenCoordsToGroundCoords(input.MouseLoc);
                //DataManager.Instance.Save.ViewCenter = coords * GraphicsManager.TILE_SIZE;
                if (Collision.InBounds(ZoneManager.Instance.CurrentGround.GroundWidth, ZoneManager.Instance.CurrentGround.GroundHeight, coords))
                {
                    FocusedCharacter.SetMapLoc(coords);
                    FocusedCharacter.UpdateFrame();
                }
            }
        }


        public override IEnumerator<YieldInstruction> ProcessInput()
        {
            GameManager.Instance.FrameProcessed = false;

            if (PendingLeaderAction != null)
            {
                yield return CoroutineManager.Instance.StartCoroutine(PendingLeaderAction);
                PendingLeaderAction = null;
            }

            if (PendingDevEvent != null)
            {
                yield return CoroutineManager.Instance.StartCoroutine(PendingDevEvent);
                PendingDevEvent = null;
            }
            else
                yield return CoroutineManager.Instance.StartCoroutine(ProcessInput(GameManager.Instance.InputManager));

            if (!GameManager.Instance.FrameProcessed)
                yield return new WaitForFrames(1);

            if (GameManager.Instance.SceneOutcome == null)
            {
                //psy's notes: put everything related to the check events in the ground map, so its more encapsulated.
                yield return CoroutineManager.Instance.StartCoroutine(ZoneManager.Instance.CurrentGround.OnCheck());
            }
        }

        IEnumerator<YieldInstruction> ProcessInput(InputManager input)
        {
            GameAction action = new GameAction(GameAction.ActionType.None, Dir8.None);

            if (!input[FrameInput.InputType.Skills] && input.JustPressed(FrameInput.InputType.Menu))
            {
                GameManager.Instance.SE("Menu/Skip");
                yield return CoroutineManager.Instance.StartCoroutine(MenuManager.Instance.ProcessMenuCoroutine(new MainMenu()));
            }
            else if (input.JustPressed(FrameInput.InputType.SkillMenu))
            {
                GameManager.Instance.SE("Menu/Skip");
                yield return CoroutineManager.Instance.StartCoroutine(MenuManager.Instance.ProcessMenuCoroutine(new SkillMenu(DataManager.Instance.Save.ActiveTeam.LeaderIndex)));
            }
            else if (input.JustPressed(FrameInput.InputType.ItemMenu))
            {
                bool heldItems = false;
                foreach (Character character in DataManager.Instance.Save.ActiveTeam.Players)
                {
                    if (!character.Dead && character.EquippedItem.ID > -1)
                    {
                        heldItems = true;
                        break;
                    }
                }
                if (!(DataManager.Instance.Save.ActiveTeam.GetInvCount() == 0 && !heldItems))
                {
                    GameManager.Instance.SE("Menu/Skip");
                    yield return CoroutineManager.Instance.StartCoroutine(MenuManager.Instance.ProcessMenuCoroutine(new ItemMenu()));
                }
                else
                    GameManager.Instance.SE("Menu/Cancel");
            }
            else if (input.JustPressed(FrameInput.InputType.TacticMenu))
            {
                if (DataManager.Instance.Save.ActiveTeam.Players.Count > 1)
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
                bool run = input[FrameInput.InputType.Run];

                if (input.JustPressed(FrameInput.InputType.Attack))
                {
                    //if confirm was the only move, then the command is an attack
                    action = new GameAction(GameAction.ActionType.Attack, Dir8.None);
                }
                else if (input.Direction != Dir8.None)
                {
                    GameAction.ActionType cmdType = GameAction.ActionType.Dir;

                    //if (FrameTick.FromFrames(input.InputTime) > FrameTick.FromFrames(2)) //TODO: ensure that it does not freeze when transitioning walk to run and vice versa
                        cmdType = GameAction.ActionType.Move;

                    action = new GameAction(cmdType, input.Direction);

                    if (cmdType == GameAction.ActionType.Move)
                    {
                        action.AddArg(run ? 1 : 0);
                        action.AddArg(run ? 5 : 2);
                    }
                }

                if (action.Type == GameAction.ActionType.None)
                {
                    if (input.JustPressed(FrameInput.InputType.LeaderSwap1))
                        action = new GameAction(GameAction.ActionType.SetLeader, Dir8.None, 0, 0);
                    else if (input.JustPressed(FrameInput.InputType.LeaderSwap2))
                        action = new GameAction(GameAction.ActionType.SetLeader, Dir8.None, 1, 0);
                    else if (input.JustPressed(FrameInput.InputType.LeaderSwap3))
                        action = new GameAction(GameAction.ActionType.SetLeader, Dir8.None, 2, 0);
                    else if (input.JustPressed(FrameInput.InputType.LeaderSwap4))
                        action = new GameAction(GameAction.ActionType.SetLeader, Dir8.None, 3, 0);
                    else if (input.JustPressed(FrameInput.InputType.LeaderSwapBack))
                    {
                        int newSlot = DataManager.Instance.Save.ActiveTeam.LeaderIndex;
                        do
                        {
                            newSlot = (newSlot + DataManager.Instance.Save.ActiveTeam.Players.Count - 1) % DataManager.Instance.Save.ActiveTeam.Players.Count;
                        }
                        while (!canSwitchToChar(newSlot));
                        action = new GameAction(GameAction.ActionType.SetLeader, Dir8.None, newSlot, 0);
                    }
                    else if (input.JustPressed(FrameInput.InputType.LeaderSwapForth))
                    {
                        int newSlot = DataManager.Instance.Save.ActiveTeam.LeaderIndex;
                        do
                        {
                            newSlot = (newSlot + 1) % DataManager.Instance.Save.ActiveTeam.Players.Count;
                        }
                        while (!canSwitchToChar(newSlot));
                        action = new GameAction(GameAction.ActionType.SetLeader, Dir8.None, newSlot, 0);
                    }
                }
            }

            if (action.Type != GameAction.ActionType.None)
                yield return CoroutineManager.Instance.StartCoroutine(ProcessInput(action));
        }



        public override void Update(FrameTick elapsedTime)
        {

            if (ZoneManager.Instance.CurrentGround != null)
            {

                //Make entities think!
                foreach (GroundEntity ent in ZoneManager.Instance.CurrentGround.IterateEntities())
                {
                    if (ent.EntEnabled && ent.GetType().IsSubclassOf(typeof(GroundAIUser)))
                    {
                        GroundAIUser tu = (GroundAIUser)ent;
                        tu.Think();
                    }
                }

                //update the hitboxes' movements

                //update the team/enemies
                foreach (GroundChar character in ZoneManager.Instance.CurrentGround.IterateCharacters())
                {
                    if (character.EntEnabled)
                        character.Update(elapsedTime);
                }
                foreach (GroundChar character in ZoneManager.Instance.CurrentGround.IterateCharacters())
                {
                    if (character.EntEnabled)
                        character.Collide();
                }


                Loc focusedLoc = new Loc();
                if (ZoneManager.Instance.CurrentGround.ViewCenter.HasValue)
                    focusedLoc = ZoneManager.Instance.CurrentGround.ViewCenter.Value;
                else if (FocusedCharacter != null)
                    focusedLoc = FocusedCharacter.Bounds.Center + ZoneManager.Instance.CurrentGround.ViewOffset;

                base.UpdateCamMod(elapsedTime, ref focusedLoc);

                UpdateCam(focusedLoc);

                base.Update(elapsedTime);
            }

        }

        public override void DrawDebug(SpriteBatch spriteBatch)
        {
            BaseSheet blank = GraphicsManager.Pixel;
            ZoneManager.Instance.CurrentGround.DrawDebug(ViewRect.X, ViewRect.Y, ViewRect.Width, ViewRect.Height,
                (int x, int y, int w, int h, float alpha) =>
                {
                    blank.Draw(spriteBatch, new Rectangle((int)((x - ViewRect.X) * windowScale * scale), (int)((y - ViewRect.Y) * windowScale * scale), (int)(w * windowScale * scale), 1), null, Color.White * alpha);
                    blank.Draw(spriteBatch, new Rectangle((int)((x - ViewRect.X) * windowScale * scale), (int)((y - ViewRect.Y) * windowScale * scale), 1, (int)(h * windowScale * scale)), null, Color.White * alpha);
                },
                (AABB.IObstacle box) =>
                {
                    if (box is GroundWall)
                        blank.Draw(spriteBatch, new Rectangle((int)((box.Bounds.X - ViewRect.X) * windowScale * scale), (int)((box.Bounds.Y - ViewRect.Y) * windowScale * scale), (int)(box.Bounds.Width * windowScale * scale), (int)(box.Bounds.Height * windowScale * scale)), null, Color.Red * 0.3f);
                    else if (box is GroundChar)
                    {
                        if (((GroundChar)box).EntEnabled)
                            blank.Draw(spriteBatch, new Rectangle((int)((box.Bounds.X - ViewRect.X) * windowScale * scale), (int)((box.Bounds.Y - ViewRect.Y) * windowScale * scale), (int)(box.Bounds.Width * windowScale * scale), (int)(box.Bounds.Height * windowScale * scale)), null, Color.Yellow * 0.7f);
                    }
                    else if (box is GroundObject)
                    {
                        if (((GroundObject)box).EntEnabled)
                            blank.Draw(spriteBatch, new Rectangle((int)((box.Bounds.X - ViewRect.X) * windowScale * scale), (int)((box.Bounds.Y - ViewRect.Y) * windowScale * scale), (int)(box.Bounds.Width * windowScale * scale), (int)(box.Bounds.Height * windowScale * scale)), null, Color.Cyan * 0.5f);
                    }
                    else
                        blank.Draw(spriteBatch, new Rectangle((int)((box.Bounds.X - ViewRect.X) * windowScale * scale), (int)((box.Bounds.Y - ViewRect.Y) * windowScale * scale), (int)(box.Bounds.Width * windowScale * scale), (int)(box.Bounds.Height * windowScale * scale)), null, Color.Gray * 0.5f);
                }, (string message, int x, int y, float alpha) =>
                {
                    int size = GraphicsManager.SysFont.SubstringWidth(message);
                    GraphicsManager.SysFont.DrawText(spriteBatch, (int)((x - ViewRect.X) * windowScale * scale), (int)((y - ViewRect.Y) * windowScale * scale), message, null, DirV.None, DirH.None);
                });

            base.DrawDebug(spriteBatch);
            if (FocusedCharacter != null)
            {
                GraphicsManager.SysFont.DrawText(spriteBatch, GraphicsManager.WindowWidth - 2, 62, String.Format("Z:{0:D3} S:{1:D3} M:{2:D3}", ZoneManager.Instance.CurrentZoneID, ZoneManager.Instance.CurrentMapID.Segment, ZoneManager.Instance.CurrentMapID.ID), null, DirV.Up, DirH.Right, Color.White);
                GraphicsManager.SysFont.DrawText(spriteBatch, GraphicsManager.WindowWidth - 2, 72, String.Format("X:{0:D3} Y:{1:D3}", FocusedCharacter.MapLoc.X, FocusedCharacter.MapLoc.Y), null, DirV.Up, DirH.Right, Color.White);

                MonsterID monId;
                Loc offset;
                int anim;
                int currentHeight, currentTime, currentFrame;
                FocusedCharacter.GetCurrentSprite(out monId, out offset, out currentHeight, out anim, out currentTime, out currentFrame);
                CharSheet charSheet = GraphicsManager.GetChara(FocusedCharacter.CurrentForm);
                Color frameColor = Color.White;
                string animName = GraphicsManager.Actions[anim].Name;
                int resultAnim = charSheet.GetReferencedAnimIndex(anim);
                if (resultAnim == -1)
                    frameColor = Color.Gray;
                else if (resultAnim != anim)
                {
                    animName += "->" + GraphicsManager.Actions[resultAnim].Name;
                    frameColor = Color.Yellow;
                }

                GraphicsManager.SysFont.DrawText(spriteBatch, GraphicsManager.WindowWidth - 2, 82, String.Format("{0}:{1}:{2:D2}", animName, FocusedCharacter.CharDir.ToString(), currentFrame), null, DirV.Up, DirH.Right, frameColor);
                GraphicsManager.SysFont.DrawText(spriteBatch, GraphicsManager.WindowWidth - 2, 92, String.Format("Frame {0:D3}", currentTime), null, DirV.Up, DirH.Right, Color.White);

                PortraitSheet sheet = GraphicsManager.GetPortrait(FocusedCharacter.CurrentForm);
                sheet.DrawPortrait(spriteBatch, new Vector2(0, GraphicsManager.WindowHeight - GraphicsManager.PortraitSize), new EmoteStyle(DebugEmote));
                frameColor = Color.White;
                string emoteName = GraphicsManager.Emotions[DebugEmote].Name;
                int resultEmote = sheet.GetReferencedEmoteIndex(DebugEmote);
                if (resultEmote == -1)
                    frameColor = Color.Gray;
                else if (resultEmote != DebugEmote)
                {
                    emoteName += "->" + GraphicsManager.Emotions[resultEmote].Name;
                    frameColor = Color.Yellow;
                }
                GraphicsManager.SysFont.DrawText(spriteBatch, 2, GraphicsManager.WindowHeight - GraphicsManager.PortraitSize - 2, emoteName, null, DirV.Down, DirH.Left, frameColor);
            }
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


        public IEnumerator<YieldInstruction> MoveCamera(Loc loc, int time, bool toPlayer)
        {
            Loc startLoc = ZoneManager.Instance.CurrentGround.ViewCenter.HasValue ? ZoneManager.Instance.CurrentGround.ViewCenter.Value : FocusedCharacter.Bounds.Center + ZoneManager.Instance.CurrentGround.ViewOffset;
            Loc endLoc = loc;
            if (toPlayer)
            {
                startLoc -= FocusedCharacter.Bounds.Center;
                endLoc -= FocusedCharacter.Bounds.Center;
                ZoneManager.Instance.CurrentGround.ViewCenter = null;
            }
            else
                ZoneManager.Instance.CurrentGround.ViewOffset = Loc.Zero;

            int currentFadeTime = time;
            while (currentFadeTime > 0)
            {
                currentFadeTime--;
                if (toPlayer)
                    ZoneManager.Instance.CurrentGround.ViewOffset = new Loc(AnimMath.Lerp(loc.X, startLoc.X, (double)currentFadeTime / time), AnimMath.Lerp(loc.Y, startLoc.Y, (double)currentFadeTime / time));
                else
                    ZoneManager.Instance.CurrentGround.ViewCenter = new Loc(AnimMath.Lerp(loc.X, startLoc.X, (double)currentFadeTime / time), AnimMath.Lerp(loc.Y, startLoc.Y, (double)currentFadeTime / time));
                yield return new WaitForFrames(1);
            }
        }

    }
}
