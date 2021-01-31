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
    public abstract class BaseDungeonScene : BaseScene
    {

        public IEnumerator<YieldInstruction> PendingDevEvent;

        protected List<IDrawableSprite> groundDraw;
        protected List<IDrawableSprite> backDraw;
        protected List<IDrawableSprite> frontDraw;
        protected List<IDrawableSprite> foregroundDraw;
        protected List<Character> shownChars;


        /// <summary>
        /// Rectangle of the tiles that must be drawn.
        /// </summary>
        protected Rect viewTileRect;


        protected BlendState subtractBlend;

        protected RenderTarget2D gameScreen;

        public BaseDungeonScene()
        {

            groundDraw = new List<IDrawableSprite>();
            backDraw = new List<IDrawableSprite>();
            frontDraw = new List<IDrawableSprite>();
            foregroundDraw = new List<IDrawableSprite>();
            shownChars = new List<Character>();

            subtractBlend = new BlendState();
            subtractBlend.ColorBlendFunction = BlendFunction.ReverseSubtract;
            subtractBlend.AlphaBlendFunction = BlendFunction.ReverseSubtract;
            subtractBlend.ColorSourceBlend = Blend.One;
            subtractBlend.AlphaSourceBlend = Blend.One;
            subtractBlend.ColorDestinationBlend = Blend.One;
            subtractBlend.AlphaDestinationBlend = Blend.One;
            subtractBlend.ColorWriteChannels = ColorWriteChannels.Alpha;

            gameScreen = new RenderTarget2D(
                GraphicsManager.GraphicsDevice,
                GraphicsManager.ScreenWidth,
                GraphicsManager.ScreenHeight,
                false,
                GraphicsManager.GraphicsDevice.PresentationParameters.BackBufferFormat,
                DepthFormat.Depth24);
        }

        public void ZoomChanged()
        {
            int zoomMult = Math.Min(GraphicsManager.WindowZoom, (int)Math.Max(1, 1 / GraphicsManager.Zoom.GetScale()));
            gameScreen = new RenderTarget2D(GraphicsManager.GraphicsDevice,
                GraphicsManager.ScreenWidth * zoomMult, GraphicsManager.ScreenHeight * zoomMult,
                false, GraphicsManager.GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24);
        }

        public override void Begin()
        {
            PendingDevEvent = null;
        }


        protected void UpdateCam(Loc focusedLoc)
        {
            //update cam
            windowScale = GraphicsManager.WindowZoom;

            scale = GraphicsManager.Zoom.GetScale();

            matrixScale = windowScale;
            drawScale = scale;
            while (matrixScale > 1 && drawScale < 1)
            {
                matrixScale /= 2;
                drawScale *= 2;
            }


            Loc viewCenter = focusedLoc;

            if (ZoneManager.Instance.CurrentMap.EdgeView == Map.ScrollEdge.Clamp)
                viewCenter = new Loc(Math.Max(GraphicsManager.ScreenWidth / 2, Math.Min(viewCenter.X, ZoneManager.Instance.CurrentMap.Width * GraphicsManager.TileSize - GraphicsManager.ScreenWidth / 2)),
                    Math.Max(GraphicsManager.ScreenHeight / 2, Math.Min(viewCenter.Y, ZoneManager.Instance.CurrentMap.Height * GraphicsManager.TileSize - GraphicsManager.ScreenHeight / 2)));

            ViewRect = new Rect((int)(viewCenter.X - GraphicsManager.ScreenWidth / scale / 2), (int)(viewCenter.Y - GraphicsManager.ScreenHeight / scale / 2),
                (int)(GraphicsManager.ScreenWidth / scale), (int)(GraphicsManager.ScreenHeight / scale));
            viewTileRect = new Rect((int)Math.Floor((float)ViewRect.X / GraphicsManager.TileSize), (int)Math.Floor((float)ViewRect.Y / GraphicsManager.TileSize),
                (ViewRect.End.X - 1) / GraphicsManager.TileSize + 1 - (int)Math.Floor((float)ViewRect.X / GraphicsManager.TileSize), (ViewRect.End.Y - 1) / GraphicsManager.TileSize + 1 - (int)Math.Floor((float)ViewRect.Y / GraphicsManager.TileSize));

        }


        protected virtual bool CanSeeTile(int xx, int yy)
        {
            return true;
        }

        protected virtual void PrepareTileDraw(SpriteBatch spriteBatch, int xx, int yy)
        {
            ZoneManager.Instance.CurrentMap.DrawLoc(spriteBatch, new Loc(xx * GraphicsManager.TileSize, yy * GraphicsManager.TileSize) - ViewRect.Start, new Loc(xx, yy));
            EffectTile effect = ZoneManager.Instance.CurrentMap.Tiles[xx][yy].Effect;
            if (effect.ID > -1 && effect.Exposed && !DataManager.Instance.HideObjects)
            {
                if (DataManager.Instance.GetTile(effect.ID).ObjectLayer)
                    AddToDraw(backDraw, effect);
                else
                    AddToDraw(groundDraw, effect);
            }
        }

        protected virtual void PrepareBackDraw()
        {
            //draw effects in object space
            //get all back effects, see if they're in the screen, and put them in the list, sorted
            foreach (IFinishableSprite effect in Anims[(int)DrawLayer.Back])
            {
                if (CanSeeSprite(ViewRect, effect))
                    AddToDraw(backDraw, effect);
            }
            if (!DataManager.Instance.HideChars)
            {
                //check if player/enemies is in the screen, put in list
                foreach (Character character in ZoneManager.Instance.CurrentMap.IterateCharacters())
                {
                    if (CanSeeCharOnScreen(character))
                    {
                        AddToDraw(backDraw, character);
                        shownChars.Add(character);
                    }
                }
            }
            //get all effects, see if they're in the screen, and put them in the list, sorted
            foreach (IFinishableSprite effect in Anims[(int)DrawLayer.Normal])
            {
                if (CanSeeSprite(ViewRect, effect))
                    AddToDraw(backDraw, effect);
            }
        }

        protected virtual void PrepareFrontDraw()
        {
            //draw effects in top
            foreach (IFinishableSprite effect in Anims[(int)DrawLayer.Front])
            {
                if (CanSeeSprite(ViewRect, effect))
                    AddToDraw(frontDraw, effect);
            }
        }

        protected virtual bool CanSeeCharOnScreen(Character character)
        {
            if (character.Dead)
                return false;
            if (!CanSeeSprite(ViewRect, character))
                return false;

            return true;
        }

        protected virtual bool CanSeeHiddenItems()
        {
            return true;
        }

        protected virtual void DrawItems(SpriteBatch spriteBatch, bool showHiddenItem)
        {
            foreach (MapItem item in ZoneManager.Instance.CurrentMap.Items)
            {
                if (CanSeeSprite(ViewRect, item))
                {
                    TerrainData terrain = ZoneManager.Instance.CurrentMap.Tiles[item.TileLoc.X][item.TileLoc.Y].Data.GetData();
                    if (terrain.BlockType == TerrainData.Mobility.Impassable || terrain.BlockType == TerrainData.Mobility.Block)
                    {
                        if (showHiddenItem)
                            item.Draw(spriteBatch, ViewRect.Start, Color.White * 0.7f);
                    }
                    else if (showHiddenItem || ZoneManager.Instance.CurrentMap.DiscoveryArray[item.TileLoc.X][item.TileLoc.Y] == Map.DiscoveryState.Traversed)
                    {
                        if (terrain.BlockType == TerrainData.Mobility.Passable)
                            item.Draw(spriteBatch, ViewRect.Start, Color.White);
                        else
                            item.Draw(spriteBatch, ViewRect.Start, Color.White * 0.7f);
                    }
                }
            }
        }

        public virtual void DrawGame(SpriteBatch spriteBatch)
        {
            GraphicsManager.GraphicsDevice.SetRenderTarget(gameScreen);

            GraphicsManager.GraphicsDevice.Clear(Color.Transparent);

            Matrix matrix = Matrix.CreateScale(new Vector3(drawScale, drawScale, 1));
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, matrix);

            groundDraw.Clear();
            backDraw.Clear();
            frontDraw.Clear();
            foregroundDraw.Clear();
            shownChars.Clear();

            //draw the background
            ZoneManager.Instance.CurrentMap.DrawBG(spriteBatch);

            for (int yy = viewTileRect.Y - 1; yy < viewTileRect.End.Y + 1; yy++)
            {
                for (int xx = viewTileRect.X - 1; xx < viewTileRect.End.X + 1; xx++)
                {
                    //if it's a tile on the discovery array, show it
                    bool outOfBounds = !Collision.InBounds(ZoneManager.Instance.CurrentMap.Width, ZoneManager.Instance.CurrentMap.Height, new Loc(xx, yy));
                    if (CanSeeTile(xx, yy))
                    {
                        if (outOfBounds)
                            ZoneManager.Instance.CurrentMap.DrawDefaultTile(spriteBatch, new Loc(xx * GraphicsManager.TileSize, yy * GraphicsManager.TileSize) - ViewRect.Start);
                        else
                            PrepareTileDraw(spriteBatch, xx, yy);
                    }
                }
            }

            //draw effects laid on ground
            foreach (IFinishableSprite effect in Anims[(int)DrawLayer.Bottom])
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

            PrepareBackDraw();

            //draw shadows
            foreach (Character shadowChar in shownChars)
            {
                TerrainData terrain = ZoneManager.Instance.CurrentMap.Tiles[shadowChar.CharLoc.X][shadowChar.CharLoc.Y].Data.GetData();
                int terrainShadow = terrain.ShadowType;
                shadowChar.DrawShadow(spriteBatch, ViewRect.Start, terrainShadow);
            }


            //draw items
            if (!DataManager.Instance.HideObjects)
                DrawItems(spriteBatch, CanSeeHiddenItems());

            //draw object
            charIndex = 0;
            for (int yy = viewTileRect.Y; yy < viewTileRect.End.Y; yy++)
            {
                while (charIndex < backDraw.Count)
                {
                    int charY = backDraw[charIndex].MapLoc.Y;
                    if (charY == yy * GraphicsManager.TileSize)
                    {
                        backDraw[charIndex].Draw(spriteBatch, ViewRect.Start);
                        if (GameManager.Instance.ShowDebug)
                            backDraw[charIndex].DrawDebug(spriteBatch, ViewRect.Start);
                        charIndex++;
                    }
                    else
                        break;
                }

                while (charIndex < backDraw.Count)
                {
                    int charY = backDraw[charIndex].MapLoc.Y;
                    if (charY < (yy + 1) * GraphicsManager.TileSize)
                    {
                        backDraw[charIndex].Draw(spriteBatch, ViewRect.Start);
                        if (GameManager.Instance.ShowDebug)
                            backDraw[charIndex].DrawDebug(spriteBatch, ViewRect.Start);
                        charIndex++;
                    }
                    else
                        break;
                }
            }

            while (charIndex < backDraw.Count)
            {
                backDraw[charIndex].Draw(spriteBatch, ViewRect.Start);
                if (GameManager.Instance.ShowDebug)
                    backDraw[charIndex].DrawDebug(spriteBatch, ViewRect.Start);
                charIndex++;
            }

            PrepareFrontDraw();

            charIndex = 0;
            while (charIndex < frontDraw.Count)
            {
                frontDraw[charIndex].Draw(spriteBatch, ViewRect.Start);
                if (GameManager.Instance.ShowDebug)
                    frontDraw[charIndex].DrawDebug(spriteBatch, ViewRect.Start);
                charIndex++;
            }

            //draw effects in foreground
            foreach (IFinishableSprite effect in Anims[(int)DrawLayer.Top])
            {
                if (CanSeeSprite(ViewRect, effect))
                    AddToDraw(foregroundDraw, effect);
            }
            charIndex = 0;
            while (charIndex < foregroundDraw.Count)
            {
                foregroundDraw[charIndex].Draw(spriteBatch, ViewRect.Start);
                if (GameManager.Instance.ShowDebug)
                    foregroundDraw[charIndex].DrawDebug(spriteBatch, ViewRect.Start);
                charIndex++;
            }

            DrawOverlay(spriteBatch);

            spriteBatch.End();

            PostDraw(spriteBatch);

            GraphicsManager.GraphicsDevice.SetRenderTarget(null);

            GraphicsManager.GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, null, null, null, Matrix.CreateScale(new Vector3(matrixScale, matrixScale, 1)));

            spriteBatch.Draw(gameScreen, new Vector2(), Color.White);

            spriteBatch.End();

            spriteBatch.Begin();

            DrawDev(spriteBatch);

            spriteBatch.End();
        }

        protected virtual void PostDraw(SpriteBatch spriteBatch)
        { }

        public virtual void DrawOverlay(SpriteBatch spriteBatch)
        { }

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
            loc = loc - (ViewRect.Start / GraphicsManager.TileSize * GraphicsManager.TileSize) + new Loc(GraphicsManager.TileSize);
            loc /= GraphicsManager.TileSize;
            loc = loc + (ViewRect.Start / GraphicsManager.TileSize) - new Loc(1);

            return loc;
        }
    }
}
