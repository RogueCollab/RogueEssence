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

        public AutoTile AutoTileInProgress;
        public TerrainTile TerrainInProgress;
        public ObjAnimData TileInProgress;
        public Rect RectInProgress;
        public bool ShowTerrain;
        public bool ShowEntrances;

        public Rect RectPreview()
        {
            Rect resultRect = new Rect(RectInProgress.Start, RectInProgress.Size);
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
            base.UpdateMeta();

            InputManager input = GameManager.Instance.MetaInputManager;
            var mapEditor = DiagManager.Instance.DevEditor.MapEditor;

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

            for (int ii = 0; ii < DirKeys.Length; ii++)
            {
                if (input.BaseKeyDown(DirKeys[ii]))
                    dirLoc = dirLoc + ((Dir4)ii).GetLoc();
            }

            bool slow = input.BaseKeyDown(Keys.LeftShift);
            int speed = 8;
            if (slow)
                speed = 1;
            else
            {
                switch (GraphicsManager.Zoom)
                {
                    case GraphicsManager.GameZoom.x8Near:
                        speed = 1;
                        break;
                    case GraphicsManager.GameZoom.x4Near:
                        speed = 2;
                        break;
                    case GraphicsManager.GameZoom.x2Near:
                        speed = 4;
                        break;
                }
            }

            DiffLoc = dirLoc * speed;

            yield break;
        }


        public override void Update(FrameTick elapsedTime)
        {
            if (ZoneManager.Instance.CurrentMap != null)
            {
                foreach (Character character in ZoneManager.Instance.CurrentMap.IterateCharacters())
                    character.UpdateFrame();

                FocusedLoc += DiffLoc;
                DiffLoc = new Loc();

                float scale = GraphicsManager.Zoom.GetScale();

                if (ZoneManager.Instance.CurrentMap.EdgeView == Map.ScrollEdge.Clamp)
                    FocusedLoc = new Loc(Math.Max((int)(GraphicsManager.ScreenWidth / scale / 2), Math.Min(FocusedLoc.X,
                        ZoneManager.Instance.CurrentMap.Width * GraphicsManager.TileSize - (int)(GraphicsManager.ScreenWidth / scale / 2))),
                        Math.Max((int)(GraphicsManager.ScreenHeight / scale / 2), Math.Min(FocusedLoc.Y,
                        ZoneManager.Instance.CurrentMap.Height * GraphicsManager.TileSize - (int)(GraphicsManager.ScreenHeight / scale / 2))));
                else
                    FocusedLoc = new Loc(Math.Max(0, Math.Min(FocusedLoc.X, ZoneManager.Instance.CurrentMap.Width * GraphicsManager.TileSize)),
                        Math.Max(0, Math.Min(FocusedLoc.Y, ZoneManager.Instance.CurrentMap.Height * GraphicsManager.TileSize)));

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
                DrawGame(spriteBatch);
        }
        protected override void PostDraw(SpriteBatch spriteBatch)
        {
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
                        if (Collision.InBounds(ZoneManager.Instance.CurrentMap.Width, ZoneManager.Instance.CurrentMap.Height, new Loc(ii, jj)))
                        {
                            TerrainTile tile = ZoneManager.Instance.CurrentMap.Tiles[ii][jj].Data;
                            if (Collision.InBounds(RectPreview(), new Loc(ii, jj)))
                            {
                                if (TerrainInProgress != null)
                                {
                                    tile = TerrainInProgress;
                                    if (TerrainInProgress.Equals(new AutoTile(new TileLayer())))
                                        GraphicsManager.Pixel.Draw(spriteBatch, new Rectangle(ii * GraphicsManager.TileSize - ViewRect.X, jj * GraphicsManager.TileSize - ViewRect.Y, GraphicsManager.TileSize, GraphicsManager.TileSize), null, Color.Black);
                                    else
                                        TerrainInProgress.TileTex.Draw(spriteBatch, new Loc(ii * GraphicsManager.TileSize, jj * GraphicsManager.TileSize) - ViewRect.Start);
                                }
                            }
                            TerrainData data = tile.GetData();
                            Color color = Color.Transparent;
                            switch (data.BlockType)
                            {
                                case TerrainData.Mobility.Block:
                                    color = Color.Red;
                                    break;
                                case TerrainData.Mobility.Water:
                                    color = Color.Blue;
                                    break;
                                case TerrainData.Mobility.Lava:
                                    color = Color.Orange;
                                    break;
                                case TerrainData.Mobility.Abyss:
                                    color = Color.Black;
                                    break;
                                case TerrainData.Mobility.Impassable:
                                    color = Color.White;
                                    break;

                            }
                            if (color != Color.Transparent)
                                GraphicsManager.Pixel.Draw(spriteBatch, new Rectangle(ii * GraphicsManager.TileSize - ViewRect.X, jj * GraphicsManager.TileSize - ViewRect.Y, GraphicsManager.TileSize, GraphicsManager.TileSize), null, color * 0.5f);
                        }
                    }
                }
            }

            if (ShowEntrances)
            {
                foreach (LocRay8 entrance in ZoneManager.Instance.CurrentMap.EntryPoints)
                {
                    GraphicsManager.Pixel.Draw(spriteBatch, new Rectangle(entrance.Loc.X * GraphicsManager.TileSize - ViewRect.X, entrance.Loc.Y * GraphicsManager.TileSize - ViewRect.Y, GraphicsManager.TileSize, GraphicsManager.TileSize), null, Color.White * 0.75f);
                }
            }

            if (TileInProgress != null)
            {
                for (int jj = viewTileRect.Y; jj < viewTileRect.End.Y; jj++)
                {
                    for (int ii = viewTileRect.X; ii < viewTileRect.End.X; ii++)
                    {
                        if (Collision.InBounds(ZoneManager.Instance.CurrentMap.Width, ZoneManager.Instance.CurrentMap.Height, new Loc(ii, jj)) &&
                            Collision.InBounds(RectPreview(), new Loc(ii, jj)))
                        {
                            if (TileInProgress.Equals(new ObjAnimData()))
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
            }

            spriteBatch.End();
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
