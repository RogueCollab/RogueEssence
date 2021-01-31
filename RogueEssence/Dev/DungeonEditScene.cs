using System;
using System.Collections.Generic;
using RogueEssence.Content;
using RogueElements;
using RogueEssence.Data;
using RogueEssence.Dungeon;
using RogueEssence.Ground;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace RogueEssence.Dev
{
    //The game engine for Ground Mode, in which the player has free movement
    public partial class DungeonEditScene : BaseDungeonScene
    {
        private static DungeonEditScene instance;
        public static void InitInstance()
        {
            if (instance != null)
                GraphicsManager.ZoomChanged -= instance.ZoomChanged;
            instance = new DungeonEditScene();
            GraphicsManager.ZoomChanged += instance.ZoomChanged;
        }
        public static DungeonEditScene Instance { get { return instance; } }

        static Keys[] DirKeys = new Keys[4] { Keys.S, Keys.A, Keys.W, Keys.D };

        public Loc FocusedLoc;
        public Loc DiffLoc;

        public Loc MouseLoc;
        public AutoTile AutoTileInProgress;
        public AutoTile TerrainInProgress;
        public ObjAnimData TileInProgress;
        public Rect RectInProgress;
        public bool ShowTerrain;

        public Rect RectPreview()
        {
            int size = GraphicsManager.DungeonTexSize;
            Rect resultRect = new Rect(RectInProgress.Start / size, RectInProgress.Size / size);
            if (resultRect.Size.X <= 0)
            {
                resultRect.Start = new Loc(resultRect.Start.X + resultRect.Size.X, resultRect.Start.Y);
                resultRect.Size = new Loc(-resultRect.Size.X + 1, resultRect.Size.Y);
            }
            else
                resultRect.Size = new Loc(resultRect.Size.X + 1, resultRect.Size.Y);

            if (resultRect.Size.Y <= 0)
            {
                resultRect.Start = new Loc(resultRect.Start.X, resultRect.Start.Y + resultRect.Size.Y);
                resultRect.Size = new Loc(resultRect.Size.X, -resultRect.Size.Y + 1);
            }
            else
                resultRect.Size = new Loc(resultRect.Size.X, resultRect.Size.Y + 1);

            return resultRect;
        }

        public override void UpdateMeta()
        {
            InputManager input = GameManager.Instance.MetaInputManager;

            var mapEditor = DiagManager.Instance.DevEditor.MapEditor;
            MouseLoc = input.MouseLoc;

            if (mapEditor.Active)
                mapEditor.ProcessInput(input);
        }

        public override IEnumerator<YieldInstruction> ProcessInput()
        {
            GameManager.Instance.FrameProcessed = false;

            if (PendingDevEvent != null)
            {
                yield return CoroutineManager.Instance.StartCoroutine(PendingDevEvent);
                PendingDevEvent = null;
            }
            else
                yield return CoroutineManager.Instance.StartCoroutine(ProcessInput(GameManager.Instance.InputManager));

            if (!GameManager.Instance.FrameProcessed)
                yield return new WaitForFrames(1);
        }

        IEnumerator<YieldInstruction> ProcessInput(InputManager input)
        {
            Loc dirLoc = Loc.Zero;
            bool fast = !input.BaseKeyDown(Keys.LeftShift);

            for (int ii = 0; ii < DirKeys.Length; ii++)
            {
                if (input.BaseKeyDown(DirKeys[ii]))
                    dirLoc = dirLoc + ((Dir4)ii).GetLoc();
            }

            DiffLoc = dirLoc * (fast ? 8 : 1);
            yield break;
        }


        public override void Update(FrameTick elapsedTime)
        {
            if (ZoneManager.Instance.CurrentMap != null)
            {
                FocusedLoc += DiffLoc;
                DiffLoc = new Loc();

                base.UpdateCamMod(elapsedTime, ref FocusedLoc);

                UpdateCam(FocusedLoc);

                base.Update(elapsedTime);
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            //
            //When in editor mode, we want to display an overlay over some entities
            //

            if (DiagManager.Instance.DevEditor.MapEditor.Active && ZoneManager.Instance.CurrentMap != null)
            {
                DrawGame(spriteBatch);

                Matrix matrix = Matrix.CreateScale(new Vector3(drawScale, drawScale, 1));
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, matrix);

                if (AutoTileInProgress != null)
                {
                    for (int jj = viewTileRect.Y; jj < viewTileRect.End.Y; jj++)
                    {
                        for (int ii = viewTileRect.X; ii < viewTileRect.End.X; ii++)
                        {
                            if (Collision.InBounds(ZoneManager.Instance.CurrentMap.Width, ZoneManager.Instance.CurrentMap.Height, new Loc(ii, jj)) &&
                                Collision.InBounds(RectPreview(), new Loc(ii, jj)))
                            {
                                if (AutoTileInProgress.Equals(new AutoTile(new TileLayer())))
                                    GraphicsManager.Pixel.Draw(spriteBatch, new Rectangle(ii * GraphicsManager.TileSize - ViewRect.X, jj * GraphicsManager.TileSize - ViewRect.Y, GraphicsManager.TileSize, GraphicsManager.TileSize), null, Color.Black);
                                else
                                    AutoTileInProgress.Draw(spriteBatch, new Loc(ii * GraphicsManager.TileSize, jj * GraphicsManager.TileSize) - ViewRect.Start);
                            }
                        }
                    }
                }

                //draw the blocks
                if (ShowTerrain)
                {
                    for (int jj = viewTileRect.Y; jj < viewTileRect.End.Y; jj++)
                    {
                        for (int ii = viewTileRect.X; ii < viewTileRect.End.X; ii++)
                        {
                            if (Collision.InBounds(ZoneManager.Instance.CurrentMap.Width, ZoneManager.Instance.CurrentMap.Height, new Loc(ii, jj)) &&
                                Collision.InBounds(RectPreview(), new Loc(ii, jj)))
                            {
                                if (TerrainInProgress.Equals(new AutoTile(new TileLayer())))
                                    GraphicsManager.Pixel.Draw(spriteBatch, new Rectangle(ii * GraphicsManager.TileSize - ViewRect.X, jj * GraphicsManager.TileSize - ViewRect.Y, GraphicsManager.TileSize, GraphicsManager.TileSize), null, Color.Black);
                                else
                                    TerrainInProgress.Draw(spriteBatch, new Loc(ii * GraphicsManager.TileSize, jj * GraphicsManager.TileSize) - ViewRect.Start);
                            }
                        }
                    }
                }

                for (int jj = viewTileRect.Y; jj < viewTileRect.End.Y; jj++)
                {
                    for (int ii = viewTileRect.X; ii < viewTileRect.End.X; ii++)
                    {
                        if (Collision.InBounds(ZoneManager.Instance.CurrentMap.Width, ZoneManager.Instance.CurrentMap.Height, new Loc(ii, jj)) &&
                            Collision.InBounds(RectPreview(), new Loc(ii, jj)))
                        {
                            if (TileInProgress == null)
                                GraphicsManager.Pixel.Draw(spriteBatch, new Rectangle(ii * GraphicsManager.TileSize - ViewRect.X, jj * GraphicsManager.TileSize - ViewRect.Y, GraphicsManager.TileSize, GraphicsManager.TileSize), null, Color.Black);
                            else
                            {
                                if (TileInProgress.AnimIndex != "")
                                {
                                    DirSheet sheet = GraphicsManager.GetObject(TileInProgress.AnimIndex);
                                    Loc drawLoc = new Loc(ii * GraphicsManager.TileSize, jj * GraphicsManager.TileSize) - ViewRect.Start + new Loc(GraphicsManager.TileSize / 2) - new Loc(sheet.Width, sheet.Height) / 2;
                                    sheet.DrawDir(spriteBatch, drawLoc.ToVector2(), TileInProgress.GetCurrentFrame(GraphicsManager.TotalFrameTick, sheet.TotalFrames),
                                        TileInProgress.AnimDir, Color.White);
                                }
                            }
                        }
                    }
                }

                spriteBatch.End();
            }
        }


        public override void DrawDev(SpriteBatch spriteBatch)
        {
            BaseSheet blank = GraphicsManager.Pixel;
            int tileSize = GraphicsManager.TileSize;
            for (int jj = viewTileRect.Y; jj < viewTileRect.End.Y; jj++)
            {
                for (int ii = viewTileRect.X; ii < viewTileRect.End.X; ii++)
                {
                    if (Collision.InBounds(ZoneManager.Instance.CurrentMap.Width, ZoneManager.Instance.CurrentMap.Height, new Loc(ii, jj)))
                    {
                        blank.Draw(spriteBatch, new Rectangle((int)((ii * tileSize - ViewRect.X) * windowScale * scale), (int)((jj * tileSize - ViewRect.Y) * windowScale * scale), (int)(tileSize * windowScale * scale), 1), null, Color.White * 0.5f);
                        blank.Draw(spriteBatch, new Rectangle((int)((ii * tileSize - ViewRect.X) * windowScale * scale), (int)((jj * tileSize - ViewRect.Y) * windowScale * scale), 1, (int)(tileSize * windowScale * scale)), null, Color.White * 0.5f);
                    }
                    else if (ii == ZoneManager.Instance.CurrentMap.Width && Collision.InBounds(ZoneManager.Instance.CurrentMap.Height, jj))
                        blank.Draw(spriteBatch, new Rectangle((int)((ii * tileSize - ViewRect.X) * windowScale * scale), (int)((jj * tileSize - ViewRect.Y) * windowScale * scale), 1, (int)(tileSize * windowScale * scale)), null, Color.White * 0.5f);
                    else if (jj == ZoneManager.Instance.CurrentMap.Height && Collision.InBounds(ZoneManager.Instance.CurrentMap.Width, ii))
                        blank.Draw(spriteBatch, new Rectangle((int)((ii * tileSize - ViewRect.X) * windowScale * scale), (int)((jj * tileSize - ViewRect.Y) * windowScale * scale), (int)(tileSize * windowScale * scale), 1), null, Color.White * 0.5f);
                }
            }

            base.DrawDev(spriteBatch);
        }

        public override void DrawDebug(SpriteBatch spriteBatch)
        {
            if (ZoneManager.Instance.CurrentMap != null)
            {
                Loc loc = ScreenCoordsToGroundCoords(MouseLoc);
                Loc tileLoc = ScreenCoordsToMapCoords(MouseLoc);
                GraphicsManager.SysFont.DrawText(spriteBatch, GraphicsManager.WindowWidth - 2, 32, String.Format("X:{0:D3} Y:{1:D3}", loc.X, loc.Y), null, DirV.Up, DirH.Right, Color.White);
                GraphicsManager.SysFont.DrawText(spriteBatch, GraphicsManager.WindowWidth - 2, 42, String.Format("Tile X:{0:D3} Y:{1:D3}", tileLoc.X, tileLoc.Y), null, DirV.Up, DirH.Right, Color.White);
            }
        }


        public void EnterMapEdit(int entryPoint)
        {
            if (ZoneManager.Instance.CurrentMap.EntryPoints.Count > 0)
            {
                LocRay8 entry = ZoneManager.Instance.CurrentMap.EntryPoints[entryPoint];
                FocusedLoc = entry.Loc;
            }

            DiagManager.Instance.DevEditor.OpenMap();
        }

        public override void Exit()
        {
            DiagManager.Instance.DevEditor.CloseMap();
        }
    }
}
