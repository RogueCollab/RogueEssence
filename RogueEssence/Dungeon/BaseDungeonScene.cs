﻿using System;
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
using Microsoft.Xna.Framework.Input;

namespace RogueEssence.Dungeon
{
    //The game engine for Dungeon Mode, in which everyone takes an ordered turn in lock-step
    public abstract class BaseDungeonScene : BaseScene
    {
        public Loc MouseLoc;

        public IEnumerator<YieldInstruction> PendingDevEvent;

        protected List<(IDrawableSprite sprite, Loc viewOffset)> groundDraw;
        protected List<(IDrawableSprite sprite, Loc viewOffset)> backDraw;
        protected List<(IDrawableSprite sprite, Loc viewOffset)> frontDraw;
        protected List<(IDrawableSprite sprite, Loc viewOffset)> foregroundDraw;
        protected List<(Character sprite, Loc viewOffset)> shownChars;


        /// <summary>
        /// Rectangle of the tiles that must be drawn.
        /// </summary>
        protected Rect viewTileRect;


        protected BlendState subtractBlend;

        protected RenderTarget2D gameScreen;

        public BaseDungeonScene()
        {

            groundDraw = new List<(IDrawableSprite, Loc)>();
            backDraw = new List<(IDrawableSprite, Loc)>();
            frontDraw = new List<(IDrawableSprite, Loc)>();
            foregroundDraw = new List<(IDrawableSprite, Loc)>();
            shownChars = new List<(Character sprite, Loc viewOffset)>();

            subtractBlend = new BlendState();
            subtractBlend.ColorBlendFunction = BlendFunction.ReverseSubtract;
            subtractBlend.AlphaBlendFunction = BlendFunction.ReverseSubtract;
            subtractBlend.ColorSourceBlend = Blend.One;
            subtractBlend.AlphaSourceBlend = Blend.One;
            subtractBlend.ColorDestinationBlend = Blend.One;
            subtractBlend.AlphaDestinationBlend = Blend.One;
            subtractBlend.ColorWriteChannels = ColorWriteChannels.Alpha;

            ZoomChanged();
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

        public override void UpdateMeta()
        {
            base.UpdateMeta();

            InputManager input = GameManager.Instance.MetaInputManager;
            MouseLoc = input.MouseLoc;
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
            {
                int clampedX = viewCenter.X;
                int clampedY = viewCenter.Y;
                int screenPixelWidth = (int)(GraphicsManager.ScreenWidth / scale);
                int screenPixelHeight = (int)(GraphicsManager.ScreenHeight / scale);

                if (ZoneManager.Instance.CurrentMap.GroundWidth < screenPixelWidth) // center it
                    clampedX = ZoneManager.Instance.CurrentMap.GroundWidth / 2;
                else
                    clampedX = Math.Max(screenPixelWidth / 2, Math.Min(clampedX, ZoneManager.Instance.CurrentMap.GroundWidth - screenPixelWidth / 2));

                if (ZoneManager.Instance.CurrentMap.GroundHeight < screenPixelHeight)
                    clampedY = ZoneManager.Instance.CurrentMap.GroundHeight / 2;
                else
                    clampedY = Math.Max(screenPixelHeight / 2, Math.Min(clampedY, ZoneManager.Instance.CurrentMap.GroundHeight - screenPixelHeight / 2));
                viewCenter = new Loc(clampedX, clampedY);
            }

            ViewRect = new Rect((int)(viewCenter.X - GraphicsManager.ScreenWidth / scale / 2), (int)(viewCenter.Y - GraphicsManager.ScreenHeight / scale / 2),
                (int)(GraphicsManager.ScreenWidth / scale), (int)(GraphicsManager.ScreenHeight / scale));
            viewTileRect = new Rect(MathUtils.DivDown(ViewRect.X, GraphicsManager.TileSize), MathUtils.DivDown(ViewRect.Y, GraphicsManager.TileSize),
                MathUtils.DivUp(ViewRect.End.X, GraphicsManager.TileSize) - MathUtils.DivDown(ViewRect.X, GraphicsManager.TileSize),
                MathUtils.DivUp(ViewRect.End.Y, GraphicsManager.TileSize) - MathUtils.DivDown(ViewRect.Y, GraphicsManager.TileSize));

        }


        protected virtual bool CanSeeTile(int xx, int yy)
        {
            return true;
        }

        protected virtual void PrepareTileDraw(SpriteBatch spriteBatch, int xx, int yy, bool objDrawOnly, bool seeTrap)
        {
            Loc visualLoc = new Loc(xx, yy);
            Loc wrappedLoc = ZoneManager.Instance.CurrentMap.WrapLoc(visualLoc);
            if (!objDrawOnly)
                ZoneManager.Instance.CurrentMap.DrawLoc(spriteBatch, new Loc(xx * GraphicsManager.TileSize, yy * GraphicsManager.TileSize) - ViewRect.Start, wrappedLoc, false);
            
            EffectTile effect = ZoneManager.Instance.CurrentMap.Tiles[wrappedLoc.X][wrappedLoc.Y].Effect;
            if (!String.IsNullOrEmpty(effect.ID) && effect.Exposed && !DataManager.Instance.HideObjects)
            {
                List<(IDrawableSprite, Loc)> targetDraw;
                switch (DataManager.Instance.GetTile(effect.ID).Layer)
                {
                    case DrawLayer.Back:
                        targetDraw = backDraw;
                        break;
                    case DrawLayer.Front:
                        targetDraw = frontDraw;
                        break;
                    default:
                        targetDraw = groundDraw;
                        break;
                }

                if (seeTrap || effect.Revealed)
                    AddToDraw(targetDraw, new DrawTile(visualLoc, effect.ID));
                else
                    AddToDraw(targetDraw, new DrawTile(visualLoc, DataManager.Instance.DefaultTile));
            }
        }

        protected virtual void PrepareBackDraw()
        {
            bool wrapped = ZoneManager.Instance.CurrentMap.EdgeView == Map.ScrollEdge.Wrap;
            Loc wrapSize = ZoneManager.Instance.CurrentMap.GroundSize;

            //draw effects in object space
            //get all back effects, see if they're in the screen, and put them in the list, sorted
            foreach (IFinishableSprite effect in Anims[(int)DrawLayer.Back])
                AddRelevantDraw(backDraw, wrapped, wrapSize, effect);

            if (!DataManager.Instance.HideChars)
            {
                //check if player/enemies is in the screen, put in list
                foreach (Character character in ZoneManager.Instance.CurrentMap.IterateCharacters())
                {
                    if (CanSeeCharOnScreen(character))
                    {
                        foreach (Loc viewLoc in IterateRelevantDraw(wrapped, wrapSize, character))
                        {
                            shownChars.Add((character, viewLoc));
                            if (CanIdentifyCharOnScreen(character))
                                AddToDraw(backDraw, character, viewLoc);
                        }
                    }
                }
            }
            //get all effects, see if they're in the screen, and put them in the list, sorted
            foreach (IFinishableSprite effect in Anims[(int)DrawLayer.Normal])
                AddRelevantDraw(backDraw, wrapped, wrapSize, effect);
        }

        protected virtual void PrepareFrontDraw()
        {
            bool wrapped = ZoneManager.Instance.CurrentMap.EdgeView == BaseMap.ScrollEdge.Wrap;
            Loc wrapSize = ZoneManager.Instance.CurrentMap.GroundSize;

            //draw effects in top
            foreach (IFinishableSprite effect in Anims[(int)DrawLayer.Front])
                AddRelevantDraw(frontDraw, wrapped, wrapSize, effect);
        }

        protected virtual bool CanIdentifyCharOnScreen(Character character)
        {
            return true;
        }

        protected virtual bool CanSeeCharOnScreen(Character character)
        {
            if (character.Dead)
                return false;

            if (!ZoneManager.Instance.CurrentMap.InBounds(viewTileRect, character.CharLoc))
                return false;

            return true;
        }

        protected virtual bool CanSeeHiddenItems()
        {
            return true;
        }

        protected virtual bool CanSeeTraps()
        {
            return true;
        }

        protected virtual void DrawItems(SpriteBatch spriteBatch, bool showHiddenItem)
        {
            bool wrapped = ZoneManager.Instance.CurrentMap.EdgeView == BaseMap.ScrollEdge.Wrap;
            Loc wrapSize = ZoneManager.Instance.CurrentMap.GroundSize;

            foreach (MapItem item in ZoneManager.Instance.CurrentMap.Items)
            {
                foreach(Loc viewLoc in IterateRelevantDraw(wrapped, wrapSize, item))
                {
                    Tile rootTile = ZoneManager.Instance.CurrentMap.Tiles[item.TileLoc.X][item.TileLoc.Y];
                    TerrainTile tile = rootTile.Data;
                    TerrainData terrain = tile.GetData();

                    //Object tiles should hide items beneath them
                    bool hiddenBehindObject = false;
                    if (!String.IsNullOrEmpty(rootTile.Effect.ID))
                    {
                        TileData effect = DataManager.Instance.GetTile(rootTile.Effect.ID);
                        if (effect.StepType == TileData.TriggerType.Object)
                        {
                            hiddenBehindObject = true;
                        }
                    }
                    
                    if (terrain.BlockType == TerrainData.Mobility.Impassable || terrain.BlockType == TerrainData.Mobility.Block || hiddenBehindObject)
                    {
                        if (showHiddenItem)
                            item.Draw(spriteBatch, viewLoc, Color.White * 0.7f);
                    }
                    else if (showHiddenItem || ZoneManager.Instance.CurrentMap.DiscoveryArray[item.TileLoc.X][item.TileLoc.Y] == Map.DiscoveryState.Traversed)
                    {
                        if (tile.ID == DataManager.Instance.GenFloor)
                            item.Draw(spriteBatch, viewLoc, Color.White);
                        else
                            item.Draw(spriteBatch, viewLoc, Color.White * 0.7f);
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

            bool wrapped = ZoneManager.Instance.CurrentMap.EdgeView == Map.ScrollEdge.Wrap;
            Loc wrapSize = ZoneManager.Instance.CurrentMap.GroundSize;
            bool seeTrap = CanSeeTraps();

            //draw a little more outside the view rect
            for (int yy = viewTileRect.Y - 1; yy < viewTileRect.End.Y + 1; yy++)
            {
                for (int xx = viewTileRect.X - 1; xx < viewTileRect.End.X + 1; xx++)
                {
                    //but only draw objects outside the view rect
                    bool objDrawOnly = !Collision.InBounds(viewTileRect, new Loc(xx, yy));
                    //if it's a tile on the discovery array, show it
                    if (CanSeeTile(xx, yy))
                    {
                        if (wrapped || Collision.InBounds(ZoneManager.Instance.CurrentMap.Width, ZoneManager.Instance.CurrentMap.Height, new Loc(xx, yy)))
                            PrepareTileDraw(spriteBatch, xx, yy, objDrawOnly, seeTrap);
                        else
                            ZoneManager.Instance.CurrentMap.DrawDefaultTile(spriteBatch, new Loc(xx * GraphicsManager.TileSize, yy * GraphicsManager.TileSize) - ViewRect.Start, new Loc(xx, yy));
                    }
                }
            }

            //draw effects laid on ground
            foreach (IFinishableSprite effect in Anims[(int)DrawLayer.Bottom])
                AddRelevantDraw(groundDraw, wrapped, wrapSize, effect);

            int charIndex = 0;
            while (charIndex < groundDraw.Count)
            {
                groundDraw[charIndex].sprite.Draw(spriteBatch, groundDraw[charIndex].viewOffset);
                if (GameManager.Instance.ShowDebug && DiagManager.Instance.DevMode)
                    groundDraw[charIndex].sprite.DrawDebug(spriteBatch, groundDraw[charIndex].viewOffset);
                charIndex++;
            }

            PrepareBackDraw();

            //draw shadows
            foreach ((Character sprite, Loc viewOffset) shadowChar in shownChars)
            {
                TerrainData terrain = ZoneManager.Instance.CurrentMap.Tiles[shadowChar.sprite.CharLoc.X][shadowChar.sprite.CharLoc.Y].Data.GetData();
                int terrainShadow = terrain.ShadowType;
                shadowChar.sprite.DrawShadow(spriteBatch, shadowChar.viewOffset, terrainShadow);
            }


            //draw items
            if (!DataManager.Instance.HideObjects)
                DrawItems(spriteBatch, CanSeeHiddenItems());

            //draw object
            charIndex = 0;
            while (charIndex < backDraw.Count)
            {
                backDraw[charIndex].sprite.Draw(spriteBatch, backDraw[charIndex].viewOffset);
                if (GameManager.Instance.ShowDebug && DiagManager.Instance.DevMode)
                    backDraw[charIndex].sprite.DrawDebug(spriteBatch, backDraw[charIndex].viewOffset);
                charIndex++;
            }

            PrepareFrontDraw();

            //draw front tiles
            for (int yy = viewTileRect.Y - 1; yy < viewTileRect.End.Y + 1; yy++)
            {
                for (int xx = viewTileRect.X - 1; xx < viewTileRect.End.X + 1; xx++)
                {
                    //if it's a tile on the discovery array, show it
                    Loc frontLoc = new Loc(xx, yy);
                    if (wrapped || Collision.InBounds(ZoneManager.Instance.CurrentMap.Width, ZoneManager.Instance.CurrentMap.Height, frontLoc))
                    {
                        if (CanSeeTile(xx, yy))
                        {
                            if (wrapped)
                                frontLoc = ZoneManager.Instance.CurrentMap.WrapLoc(frontLoc);
                            ZoneManager.Instance.CurrentMap.DrawLoc(spriteBatch, new Loc(xx * GraphicsManager.TileSize, yy * GraphicsManager.TileSize) - ViewRect.Start, frontLoc, true);
                        }
                    }
                }
            }

            charIndex = 0;
            while (charIndex < frontDraw.Count)
            {
                frontDraw[charIndex].sprite.Draw(spriteBatch, frontDraw[charIndex].viewOffset);
                if (GameManager.Instance.ShowDebug && DiagManager.Instance.DevMode)
                    frontDraw[charIndex].sprite.DrawDebug(spriteBatch, frontDraw[charIndex].viewOffset);
                charIndex++;
            }

            //draw effects in foreground
            foreach (IFinishableSprite effect in Anims[(int)DrawLayer.Top])
                AddRelevantDraw(foregroundDraw, wrapped, wrapSize, effect);

            charIndex = 0;
            while (charIndex < foregroundDraw.Count)
            {
                foregroundDraw[charIndex].sprite.Draw(spriteBatch, foregroundDraw[charIndex].viewOffset);
                if (GameManager.Instance.ShowDebug && DiagManager.Instance.DevMode)
                    foregroundDraw[charIndex].sprite.DrawDebug(spriteBatch, foregroundDraw[charIndex].viewOffset);
                charIndex++;
            }

            DrawOverlay(spriteBatch);

            spriteBatch.End();

            PostDraw(spriteBatch);

            GraphicsManager.GraphicsDevice.SetRenderTarget(GameManager.Instance.GameScreen);

            GraphicsManager.GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, null, null, null, Matrix.CreateScale(new Vector3(matrixScale, matrixScale, 1)));

            //draw the background
            ZoneManager.Instance.CurrentMap.Background.Draw(spriteBatch, ViewRect.Start);

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

        public override void DrawDebug(SpriteBatch spriteBatch)
        {
            base.DrawDebug(spriteBatch);

            if (ZoneManager.Instance.CurrentMap != null)
            {
                Loc loc = ScreenCoordsToGroundCoords(MouseLoc);
                Loc tileLoc = ScreenCoordsToMapCoords(MouseLoc);
                GraphicsManager.SysFont.DrawText(spriteBatch, 2, 82, String.Format("Mouse  X:{0:D3} Y:{1:D3}", loc.X, loc.Y), null, DirV.Up, DirH.Left, Color.White);
                GraphicsManager.SysFont.DrawText(spriteBatch, 2, 92, String.Format("M Tile X:{0:D3} Y:{1:D3}", tileLoc.X, tileLoc.Y), null, DirV.Up, DirH.Left, Color.White);
            }
        }

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
