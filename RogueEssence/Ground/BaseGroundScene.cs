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
    public abstract class BaseGroundScene : BaseScene
    {
        public IEnumerator<YieldInstruction> PendingDevEvent;


        private List<IDrawableSprite> groundDraw;

        private List<IDrawableSprite> otherDraw;
        
        protected Rect viewTileRect;
        

        private RenderTarget2D gameScreen;

        public BaseGroundScene()
        {

            groundDraw = new List<IDrawableSprite>();
            otherDraw = new List<IDrawableSprite>();

            Loc drawSight = getDrawSight();

            gameScreen = new RenderTarget2D(
                GraphicsManager.GraphicsDevice,
                GraphicsManager.ScreenWidth,
                GraphicsManager.ScreenHeight,
                false,
                GraphicsManager.GraphicsDevice.PresentationParameters.BackBufferFormat,
                DepthFormat.Depth24);
        }

        public override void Begin()
        {
            PendingDevEvent = null;
        }



        public override IEnumerator<YieldInstruction> ProcessInput()
        {
            GameManager.Instance.FrameProcessed = false;


            yield return CoroutineManager.Instance.StartCoroutine(ProcessFrameInput());
        }


        public IEnumerator<YieldInstruction> ProcessFrameInput()
        {
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

        protected abstract IEnumerator<YieldInstruction> ProcessInput(InputManager input);



        protected void UpdateCam(Loc focusedLoc)
        {

            //update cam
            windowScale = GraphicsManager.WindowZoom;

            scale = GameManager.Instance.Zoom.GetScale();
            Loc viewCenter = focusedLoc;

            if (ZoneManager.Instance.CurrentGround.EdgeView == Map.ScrollEdge.Clamp)
                viewCenter = new Loc(Math.Max(GraphicsManager.ScreenWidth / 2, Math.Min(viewCenter.X, ZoneManager.Instance.CurrentGround.GroundWidth - GraphicsManager.ScreenWidth / 2)),
                    Math.Max(GraphicsManager.ScreenHeight / 2, Math.Min(viewCenter.Y, ZoneManager.Instance.CurrentGround.GroundHeight - GraphicsManager.ScreenHeight / 2)));

            ViewRect = new Rect((int)(viewCenter.X - GraphicsManager.ScreenWidth / scale / 2), (int)(viewCenter.Y - GraphicsManager.ScreenHeight / scale / 2),
                (int)(GraphicsManager.ScreenWidth / scale), (int)(GraphicsManager.ScreenHeight / scale));
            viewTileRect = new Rect((int)Math.Floor((float)ViewRect.X / ZoneManager.Instance.CurrentGround.TileSize), (int)Math.Floor((float)ViewRect.Y / ZoneManager.Instance.CurrentGround.TileSize),
                (ViewRect.End.X - 1) / ZoneManager.Instance.CurrentGround.TileSize + 1 - (int)Math.Floor((float)ViewRect.X / ZoneManager.Instance.CurrentGround.TileSize), (ViewRect.End.Y - 1) / ZoneManager.Instance.CurrentGround.TileSize + 1 - (int)Math.Floor((float)ViewRect.Y / ZoneManager.Instance.CurrentGround.TileSize));

        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (ZoneManager.Instance.CurrentGround != null)
            {
                GraphicsManager.GraphicsDevice.SetRenderTarget(gameScreen);

                GraphicsManager.GraphicsDevice.Clear(Color.Transparent);

                Matrix matrix = Matrix.CreateScale(new Vector3(scale, scale, 1));
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, matrix);

                groundDraw.Clear();
                otherDraw.Clear();

                DrawGame(spriteBatch);


                spriteBatch.End();


                GraphicsManager.GraphicsDevice.SetRenderTarget(null);

                GraphicsManager.GraphicsDevice.Clear(Color.Black);

                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, null, null, null, Matrix.CreateScale(new Vector3(windowScale, windowScale, 1)));

                spriteBatch.Draw(gameScreen, new Vector2(), Color.White);

                spriteBatch.End();

                spriteBatch.Begin();

                DrawDev(spriteBatch);

                spriteBatch.End();
            }

        }

        public virtual void DrawGame(SpriteBatch spriteBatch)
        {
            //draw the background
            ZoneManager.Instance.CurrentGround.DrawBG(spriteBatch);

            for (int jj = viewTileRect.Y; jj < viewTileRect.End.Y; jj++)
            {
                for (int ii = viewTileRect.X; ii < viewTileRect.End.X; ii++)
                {
                    //if it's a tile on the discovery array, show it
                    bool outOfBounds = !Collision.InBounds(ZoneManager.Instance.CurrentGround.Width, ZoneManager.Instance.CurrentGround.Height, new Loc(ii, jj));

                    if (!outOfBounds)
                        ZoneManager.Instance.CurrentGround.DrawLoc(spriteBatch, new Loc(ii * ZoneManager.Instance.CurrentGround.TileSize, jj * ZoneManager.Instance.CurrentGround.TileSize) - ViewRect.Start, new Loc(ii, jj), false);
                }
            }

            //draw effects laid on ground
            foreach (IDrawableSprite effect in ZoneManager.Instance.CurrentGround.Anims)
            {
                if (CanSeeSprite(ViewRect, effect))
                    AddToDraw(groundDraw, effect);
            }
            foreach (IDrawableSprite effect in Anims[(int)DrawLayer.Bottom])
            {
                if (CanSeeSprite(ViewRect, effect))
                    AddToDraw(groundDraw, effect);
            }
            int charIndex = 0;
            while (charIndex < groundDraw.Count)
            {
                groundDraw[charIndex].Draw(spriteBatch, ViewRect.Start);
                if (GameManager.Instance.ShowDebug)
                    groundDraw[charIndex].DrawDebug(spriteBatch, ViewRect.Start);
                charIndex++;
            }

            List<GroundChar> shownShadows = new List<GroundChar>();

            //draw effects in object space
            //get all back effects, see if they're in the screen, and put them in the list, sorted
            foreach (BaseAnim effect in Anims[(int)DrawLayer.Back])
            {
                if (CanSeeSprite(ViewRect, effect))
                    AddToDraw(otherDraw, effect);
            }

            if (!DataManager.Instance.HideChars)
            {
                //check if player/enemies is in the screen, put in list
                foreach (GroundChar character in ZoneManager.Instance.CurrentGround.IterateCharacters())
                {
                    if (!character.EntEnabled)
                        continue;
                    if (CanSeeSprite(ViewRect, character))
                    {
                        AddToDraw(otherDraw, character);
                        shownShadows.Add(character);
                    }
                }
            }
            //get all effects, see if they're in the screen, and put them in the list, sorted
            foreach (BaseAnim effect in Anims[(int)DrawLayer.Normal])
            {
                if (CanSeeSprite(ViewRect, effect))
                    AddToDraw(otherDraw, effect);
            }

            //draw shadows
            foreach (GroundChar shadowChar in shownShadows)
            {
                if (shadowChar.EntEnabled)
                    shadowChar.DrawShadow(spriteBatch, ViewRect.Start);
            }

            //draw items
            if (!DataManager.Instance.HideObjects)
            {
                foreach (GroundObject item in ZoneManager.Instance.CurrentGround.GroundObjects)
                {
                    if (!item.EntEnabled)
                        continue;
                    if (CanSeeSprite(ViewRect, item))
                        AddToDraw(otherDraw, item);
                }
            }

            //draw object
            charIndex = 0;
            for (int j = viewTileRect.Y; j < viewTileRect.End.Y; j++)
            {
                while (charIndex < otherDraw.Count)
                {
                    int charY = otherDraw[charIndex].MapLoc.Y;
                    if (charY == j * ZoneManager.Instance.CurrentGround.TileSize)
                    {
                        otherDraw[charIndex].Draw(spriteBatch, ViewRect.Start);
                        if (GameManager.Instance.ShowDebug)
                            otherDraw[charIndex].DrawDebug(spriteBatch, ViewRect.Start);
                        charIndex++;
                    }
                    else
                        break;
                }

                while (charIndex < otherDraw.Count)
                {
                    int charY = otherDraw[charIndex].MapLoc.Y;
                    if (charY < (j + 1) * ZoneManager.Instance.CurrentGround.TileSize)
                    {
                        otherDraw[charIndex].Draw(spriteBatch, ViewRect.Start);
                        if (GameManager.Instance.ShowDebug)
                            otherDraw[charIndex].DrawDebug(spriteBatch, ViewRect.Start);
                        charIndex++;
                    }
                    else
                        break;
                }
            }

            while (charIndex < otherDraw.Count)
            {
                otherDraw[charIndex].Draw(spriteBatch, ViewRect.Start);
                if (GameManager.Instance.ShowDebug)
                    otherDraw[charIndex].DrawDebug(spriteBatch, ViewRect.Start);
                charIndex++;
            }

            //draw effects in top
            foreach (BaseAnim effect in Anims[(int)DrawLayer.Front])
            {
                if (CanSeeSprite(ViewRect, effect))
                    AddToDraw(otherDraw, effect);
            }
            charIndex = 0;
            while (charIndex < otherDraw.Count)
            {
                otherDraw[charIndex].Draw(spriteBatch, ViewRect.Start);
                if (GameManager.Instance.ShowDebug)
                    otherDraw[charIndex].DrawDebug(spriteBatch, ViewRect.Start);
                charIndex++;
            }

            //draw tiles in front
            for (int jj = viewTileRect.Y; jj < viewTileRect.End.Y; jj++)
            {
                for (int ii = viewTileRect.X; ii < viewTileRect.End.X; ii++)
                {
                    //if it's a tile on the discovery array, show it
                    bool outOfBounds = !Collision.InBounds(ZoneManager.Instance.CurrentGround.Width, ZoneManager.Instance.CurrentGround.Height, new Loc(ii, jj));

                    if (!outOfBounds)
                        ZoneManager.Instance.CurrentGround.DrawLoc(spriteBatch, new Loc(ii * ZoneManager.Instance.CurrentGround.TileSize, jj * ZoneManager.Instance.CurrentGround.TileSize) - ViewRect.Start, new Loc(ii, jj), true);
                }
            }

            //draw effects in foreground
            otherDraw.Clear();
            foreach (BaseAnim effect in Anims[(int)DrawLayer.Top])
            {
                if (CanSeeSprite(ViewRect, effect))
                    AddToDraw(otherDraw, effect);
            }
            charIndex = 0;
            while (charIndex < otherDraw.Count)
            {
                otherDraw[charIndex].Draw(spriteBatch, ViewRect.Start);
                if (GameManager.Instance.ShowDebug)
                    otherDraw[charIndex].DrawDebug(spriteBatch, ViewRect.Start);
                charIndex++;
            }

        }

        public virtual void DrawDev(SpriteBatch spriteBatch)
        { }

        public Loc ScreenCoordsToGroundCoords(Loc loc)
        {
            loc.X = (int)(loc.X / scale / windowScale);
            loc.Y = (int)(loc.Y / scale / windowScale);
            loc += ViewRect.Start;

            return loc;
        }

        public Loc ScreenCoordsToMapCoords(Loc loc)
        {
            loc.X = (int)(loc.X / scale / windowScale);
            loc.Y = (int)(loc.Y / scale / windowScale);
            loc += ViewRect.Start;
            loc = loc - (ViewRect.Start / ZoneManager.Instance.CurrentGround.TileSize * ZoneManager.Instance.CurrentGround.TileSize) + new Loc(ZoneManager.Instance.CurrentGround.TileSize);
            loc /= ZoneManager.Instance.CurrentGround.TileSize;
            loc = loc + (ViewRect.Start / ZoneManager.Instance.CurrentGround.TileSize) - new Loc(1);

            return loc;
        }


        public Loc ScreenCoordsToBlockCoords(Loc loc)
        {
            int blockSize = GraphicsManager.TEX_SIZE;

            loc.X = (int)(loc.X / scale / windowScale);
            loc.Y = (int)(loc.Y / scale / windowScale);
            loc += ViewRect.Start;
            loc = loc - (ViewRect.Start / blockSize * blockSize) + new Loc(blockSize);
            loc /= blockSize;
            loc = loc + (ViewRect.Start / blockSize) - new Loc(1);

            return loc;
        }

        static Loc getDrawSight()
        {
            return Character.GetSightDims() * 2 + new Loc(1, 2);
        }


        public void LogMsg(string msg)
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

            DataManager.Instance.MsgLog.Add(msg);
        }


        public Rect GetViewRectangle()
        {
            return ViewRect;
        }

        public float GetWindowScale()
        {
            return windowScale;
        }

    }
}
