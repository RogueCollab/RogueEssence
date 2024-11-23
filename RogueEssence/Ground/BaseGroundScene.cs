using System;
using System.Collections.Generic;
using RogueEssence.Content;
using RogueElements;
using RogueEssence.Data;
using RogueEssence.Menu;
using RogueEssence.Dungeon;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace RogueEssence.Ground
{
    //The game engine for Ground Mode, in which the player has free movement
    public abstract class BaseGroundScene : BaseScene
    {
        public Loc MouseLoc;

        public IEnumerator<YieldInstruction> PendingDevEvent;


        protected List<(IDrawableSprite sprite, Loc viewOffset)> groundDraw;

        protected List<(IDrawableSprite sprite, Loc viewOffset)> objectDraw;

        protected List<(IDrawableSprite sprite, Loc viewOffset)> frontDraw;

        protected List<(IDrawableSprite sprite, Loc viewOffset)> foregroundDraw;

        /// <summary>
        /// Ground char and ground object draw, utilizing their specific entity order variable.
        /// </summary>
        protected List<(GroundEntity sprite, Loc viewOffset)> groundObjectDraw;

        protected Rect viewTileRect;
        

        private RenderTarget2D gameScreen;

        public BaseGroundScene()
        {

            groundDraw = new List<(IDrawableSprite, Loc)>();
            objectDraw = new List<(IDrawableSprite, Loc)>();
            frontDraw = new List<(IDrawableSprite, Loc)>();
            foregroundDraw = new List<(IDrawableSprite, Loc)>();
            groundObjectDraw = new List<(GroundEntity sprite, Loc viewOffset)>();

            ZoomChanged();
        }

        public void ZoomChanged()
        {
            int zoomMult = Math.Min(GraphicsManager.WindowZoom, (int)Math.Max(1, 1 / GraphicsManager.Zoom.GetScale()));
            gameScreen = new RenderTarget2D(GraphicsManager.GraphicsDevice,
                GraphicsManager.ScreenWidth * zoomMult, GraphicsManager.ScreenHeight * zoomMult,
                false, GraphicsManager.GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24);
        }

        protected bool Screenshotting;

        protected RenderTarget2D screenshotScreen;
        public void Screenshot()
        {
            Screenshotting = true;
            screenshotScreen = new RenderTarget2D(GraphicsManager.GraphicsDevice,
                ZoneManager.Instance.CurrentGround.GroundWidth, ZoneManager.Instance.CurrentGround.GroundHeight,
                false, GraphicsManager.GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24);
        }

        public void BeginScreenshot()
        {
            if (Screenshotting)
            {
                GraphicsManager.GraphicsDevice.SetRenderTarget(screenshotScreen);
                ViewRect = new Rect(Loc.Zero, ZoneManager.Instance.CurrentGround.GroundSize);
                viewTileRect = new Rect(Loc.Zero, ZoneManager.Instance.CurrentGround.Size);
            }
        }

        public void ProcessScreenshot()
        {
            if (Screenshotting)
            {
                GraphicsManager.SaveScreenshot(screenshotScreen);
                screenshotScreen.Dispose();
                screenshotScreen = null;
                Screenshotting = false;
            }
        }

        public override void Begin()
        {
            PendingDevEvent = null;
        }


        public override void UpdateMeta()
        {
            base.UpdateMeta();
            InputManager input = GameManager.Instance.MetaInputManager;

            if (input.JustPressed(FrameInput.InputType.Screenshot))
            {
                GameManager.Instance.SE("Menu/Skip");
                Screenshot();
            }

            MouseLoc = input.MouseLoc;
        }

        protected void UpdateCam(ref Loc focusedLoc)
        {

            //update cam
            WindowScale = GraphicsManager.WindowZoom;

            scale = GraphicsManager.Zoom.GetScale();

            matrixScale = WindowScale;
            drawScale = scale;
            while (matrixScale > 1 && drawScale < 1)
            {
                matrixScale /= 2;
                drawScale *= 2;
            }

            if (ZoneManager.Instance.CurrentGround.EdgeView == Map.ScrollEdge.Clamp)
            {
                int clampedX = focusedLoc.X;
                int clampedY = focusedLoc.Y;
                int screenPixelWidth = (int)(GraphicsManager.ScreenWidth / scale);
                int screenPixelHeight = (int)(GraphicsManager.ScreenHeight / scale);

                if (ZoneManager.Instance.CurrentGround.GroundWidth < screenPixelWidth) // center it
                    clampedX = ZoneManager.Instance.CurrentGround.GroundWidth / 2;
                else
                    clampedX = Math.Max(screenPixelWidth / 2, Math.Min(clampedX, ZoneManager.Instance.CurrentGround.GroundWidth - screenPixelWidth / 2));

                if (ZoneManager.Instance.CurrentGround.GroundHeight < screenPixelHeight)
                    clampedY = ZoneManager.Instance.CurrentGround.GroundHeight / 2;
                else
                    clampedY = Math.Max(screenPixelHeight / 2, Math.Min(clampedY, ZoneManager.Instance.CurrentGround.GroundHeight - screenPixelHeight / 2));
                focusedLoc = new Loc(clampedX, clampedY);
            }

            ViewRect = new Rect((int)(focusedLoc.X - GraphicsManager.ScreenWidth / scale / 2), (int)(focusedLoc.Y - GraphicsManager.ScreenHeight / scale / 2),
                (int)(GraphicsManager.ScreenWidth / scale), (int)(GraphicsManager.ScreenHeight / scale));
            viewTileRect = new Rect(MathUtils.DivDown(ViewRect.X, ZoneManager.Instance.CurrentGround.TileSize), MathUtils.DivDown(ViewRect.Y, ZoneManager.Instance.CurrentGround.TileSize),
                MathUtils.DivUp(ViewRect.End.X, ZoneManager.Instance.CurrentGround.TileSize) - MathUtils.DivDown(ViewRect.X, ZoneManager.Instance.CurrentGround.TileSize),
                MathUtils.DivUp(ViewRect.End.Y, ZoneManager.Instance.CurrentGround.TileSize) - MathUtils.DivDown(ViewRect.Y, ZoneManager.Instance.CurrentGround.TileSize));

        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (ZoneManager.Instance.CurrentGround != null)
            {
                GraphicsManager.GraphicsDevice.SetRenderTarget(gameScreen);
                GraphicsManager.GraphicsDevice.Clear(Color.Transparent);

                BeginScreenshot();

                Matrix matrix = Matrix.CreateScale(new Vector3(drawScale, drawScale, 1));
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, matrix);

                groundDraw.Clear();
                objectDraw.Clear();
                frontDraw.Clear();
                foregroundDraw.Clear();
                groundObjectDraw.Clear();

                DrawGame(spriteBatch);

                spriteBatch.End();

                ProcessScreenshot();

                GraphicsManager.GraphicsDevice.SetRenderTarget(GameManager.Instance.GameScreen);

                GraphicsManager.GraphicsDevice.Clear(Color.Black);

                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, null, null, null, Matrix.CreateScale(new Vector3(matrixScale, matrixScale, 1)));

                spriteBatch.Draw(gameScreen, new Vector2(), Color.White);

                spriteBatch.End();

                spriteBatch.Begin();

                DrawDev(spriteBatch);

                spriteBatch.End();
            }

        }

        public virtual void DrawGame(SpriteBatch spriteBatch)
        {
            bool wrapped = ZoneManager.Instance.CurrentGround.EdgeView == BaseMap.ScrollEdge.Wrap;

            //draw the background
            ZoneManager.Instance.CurrentGround.Background.Draw(spriteBatch, ViewRect.Start);

            for (int yy = viewTileRect.Y; yy < viewTileRect.End.Y; yy++)
            {
                for (int xx = viewTileRect.X; xx < viewTileRect.End.X; xx++)
                {
                    Loc visualLoc = new Loc(xx, yy);
                    //if it's a tile on the discovery array, show it
                    if (ZoneManager.Instance.CurrentGround.InMapBounds(visualLoc))
                        ZoneManager.Instance.CurrentGround.DrawLoc(spriteBatch, new Loc(xx * ZoneManager.Instance.CurrentGround.TileSize, yy * ZoneManager.Instance.CurrentGround.TileSize) - ViewRect.Start, visualLoc, false);
                    else
                        ZoneManager.Instance.CurrentGround.DrawDefaultTile(spriteBatch, new Loc(xx * ZoneManager.Instance.CurrentGround.TileSize, yy * ZoneManager.Instance.CurrentGround.TileSize) - ViewRect.Start, visualLoc);
                }
            }

            //draw effects laid on ground
            foreach (AnimLayer layer in ZoneManager.Instance.CurrentGround.Decorations)
            {
                if (layer.Visible)
                {
                    foreach (IDrawableSprite effect in layer.Anims)
                        AddRelevantDraw((layer.Layer == DrawLayer.Top) ? frontDraw : groundDraw, wrapped, ZoneManager.Instance.CurrentGround.GroundSize, effect);
                }
            }
            foreach (IDrawableSprite effect in Anims[(int)DrawLayer.Bottom])
                AddRelevantDraw(groundDraw, wrapped, ZoneManager.Instance.CurrentGround.GroundSize, effect);

            int charIndex = 0;
            while (charIndex < groundDraw.Count)
            {
                groundDraw[charIndex].sprite.Draw(spriteBatch, groundDraw[charIndex].viewOffset);
                charIndex++;
            }

            List<(GroundChar, Loc)> shownShadows = new List<(GroundChar, Loc)>();

            //draw effects in object space
            //get all back effects, see if they're in the screen, and put them in the list, sorted
            foreach (BaseAnim effect in Anims[(int)DrawLayer.Back])
                AddRelevantDraw(objectDraw, wrapped, ZoneManager.Instance.CurrentGround.GroundSize, effect);

            if (!DataManager.Instance.HideChars)
            {
                //check if player/enemies is in the screen, put in list
                foreach (GroundChar character in ZoneManager.Instance.CurrentGround.IterateCharacters())
                {
                    if (!character.EntEnabled)
                        continue;
                    foreach(Loc viewLoc in IterateRelevantDraw(wrapped, ZoneManager.Instance.CurrentGround.GroundSize, character))
                    {
                        AddToGroundDraw(groundObjectDraw, character, viewLoc);
                        shownShadows.Add((character, viewLoc));
                    }
                }
            }
            //get all effects, see if they're in the screen, and put them in the list, sorted
            foreach (BaseAnim effect in Anims[(int)DrawLayer.Normal])
                AddRelevantDraw(objectDraw, wrapped, ZoneManager.Instance.CurrentGround.GroundSize, effect);

            //draw shadows
            foreach ((GroundChar sprite, Loc viewLoc) shadowChar in shownShadows)
            {
                if (shadowChar.sprite.EntEnabled)
                    shadowChar.sprite.DrawShadow(spriteBatch, shadowChar.viewLoc);
            }

            //draw items
            if (!DataManager.Instance.HideObjects)
            {
                foreach (GroundObject item in ZoneManager.Instance.CurrentGround.Entities[0].IterateObjects())
                {
                    if (!item.EntEnabled)
                        continue;
                    AddRelevantGroundDraw(groundObjectDraw, wrapped, ZoneManager.Instance.CurrentGround.GroundSize, item);
                }
            }

            //add the objects to the objectDraw
            foreach ((GroundEntity sprite, Loc offset) ent in groundObjectDraw)
                AddToDraw(objectDraw, ent.sprite, ent.offset);

            //draw object
            charIndex = 0;
            while (charIndex < objectDraw.Count)
            {
                objectDraw[charIndex].sprite.Draw(spriteBatch, objectDraw[charIndex].viewOffset);
                charIndex++;
            }

            //draw effects in top
            foreach (BaseAnim effect in Anims[(int)DrawLayer.Front])
                AddRelevantDraw(frontDraw, wrapped, ZoneManager.Instance.CurrentGround.GroundSize, effect);

            charIndex = 0;
            while (charIndex < frontDraw.Count)
            {
                frontDraw[charIndex].sprite.Draw(spriteBatch, frontDraw[charIndex].viewOffset);
                charIndex++;
            }

            //draw tiles in front
            for (int yy = viewTileRect.Y; yy < viewTileRect.End.Y; yy++)
            {
                for (int xx = viewTileRect.X; xx < viewTileRect.End.X; xx++)
                {
                    //if it's a tile on the discovery array, show it
                    Loc frontLoc = new Loc(xx, yy);
                    if (ZoneManager.Instance.CurrentGround.InMapBounds(frontLoc))
                        ZoneManager.Instance.CurrentGround.DrawLoc(spriteBatch, new Loc(xx * ZoneManager.Instance.CurrentGround.TileSize, yy * ZoneManager.Instance.CurrentGround.TileSize) - ViewRect.Start, frontLoc, true);
                }
            }

            //draw effects in foreground
            foreach (BaseAnim effect in Anims[(int)DrawLayer.Top])
                AddRelevantDraw(foregroundDraw, wrapped, ZoneManager.Instance.CurrentGround.GroundSize, effect);

            charIndex = 0;
            while (charIndex < foregroundDraw.Count)
            {
                foregroundDraw[charIndex].sprite.Draw(spriteBatch, foregroundDraw[charIndex].viewOffset);
                charIndex++;
            }
        }

        public virtual void DrawDev(SpriteBatch spriteBatch)
        { }

        public override void DrawDebug(SpriteBatch spriteBatch)
        {
            base.DrawDebug(spriteBatch);

            if (ZoneManager.Instance.CurrentGround != null)
            {
                Loc loc = ScreenCoordsToGroundCoords(MouseLoc);
                Loc blockLoc = ScreenCoordsToBlockCoords(MouseLoc);
                Loc tileLoc = ScreenCoordsToMapCoords(MouseLoc);
                GraphicsManager.SysFont.DrawText(spriteBatch, 2, 102, String.Format("MOUSE  X:{0:D3} Y:{1:D3}", loc.X, loc.Y), null, DirV.Up, DirH.Left, Color.White);
                GraphicsManager.SysFont.DrawText(spriteBatch, 2, 112, String.Format("M WALL X:{0:D3} Y:{1:D3}", blockLoc.X, blockLoc.Y), null, DirV.Up, DirH.Left, Color.White);
                GraphicsManager.SysFont.DrawText(spriteBatch, 2, 122, String.Format("M TILE X:{0:D3} Y:{1:D3}", tileLoc.X, tileLoc.Y), null, DirV.Up, DirH.Left, Color.White);
            }
        }


        public Loc ScreenCoordsToGroundCoords(Loc loc)
        {
            loc.X = (int)(loc.X / scale / WindowScale);
            loc.Y = (int)(loc.Y / scale / WindowScale);
            loc += ViewRect.Start;

            return loc;
        }

        public Loc ScreenCoordsToMapCoords(Loc loc)
        {
            loc.X = (int)(loc.X / scale / WindowScale);
            loc.Y = (int)(loc.Y / scale / WindowScale);
            loc += ViewRect.Start;
            loc = loc - (ViewRect.Start / ZoneManager.Instance.CurrentGround.TileSize * ZoneManager.Instance.CurrentGround.TileSize) + new Loc(ZoneManager.Instance.CurrentGround.TileSize);
            loc /= ZoneManager.Instance.CurrentGround.TileSize;
            loc = loc + (ViewRect.Start / ZoneManager.Instance.CurrentGround.TileSize) - new Loc(1);

            return loc;
        }


        public Loc ScreenCoordsToBlockCoords(Loc loc)
        {
            int blockSize = GraphicsManager.TEX_SIZE;

            loc.X = (int)(loc.X / scale / WindowScale);
            loc.Y = (int)(loc.Y / scale / WindowScale);
            loc += ViewRect.Start;
            loc = loc - (ViewRect.Start / blockSize * blockSize) + new Loc(blockSize);
            loc /= blockSize;
            loc = loc + (ViewRect.Start / blockSize) - new Loc(1);

            return loc;
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

            if (msg == Text.DIVIDER_STR)
            {
                if (DataManager.Instance.MsgLog.Count == 0 || DataManager.Instance.MsgLog[DataManager.Instance.MsgLog.Count - 1] == Text.DIVIDER_STR)
                    return;
            }
            else if (String.IsNullOrWhiteSpace(msg))
                return;

            DataManager.Instance.MsgLog.Add(msg);
        }



        public void AddToGroundDraw(List<(GroundEntity, Loc)> sprites, GroundEntity sprite, Loc viewOffset)
        {
            CollectionExt.AddToSortedList(sprites, (sprite, viewOffset), CompareGroundEntCoords);
        }


        public int CompareGroundEntCoords((GroundEntity sprite, Loc viewOffset) sprite1, (GroundEntity sprite, Loc viewOffset) sprite2)
        {
            int sign = Math.Sign((sprite1.sprite.MapLoc.Y - sprite1.viewOffset.Y) - (sprite2.sprite.MapLoc.Y - sprite2.viewOffset.Y));
            if (sign != 0)
                return sign;
            return Math.Sign(sprite1.sprite.EntOrder - sprite2.sprite.EntOrder);
        }


        public void AddRelevantGroundDraw(List<(GroundEntity, Loc)> sprites, bool wrapped, Loc wrapSize, GroundEntity sprite)
        {
            foreach (Loc viewOffset in IterateRelevantDraw(wrapped, wrapSize, sprite))
                AddToGroundDraw(sprites, sprite, viewOffset);
        }

    }
}
