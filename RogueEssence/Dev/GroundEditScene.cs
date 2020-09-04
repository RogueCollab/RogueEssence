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
    public partial class GroundEditScene : BaseGroundScene
    {
        private static GroundEditScene instance;
        public static void InitInstance()
        {
            instance = new GroundEditScene();
        }
        public static GroundEditScene Instance { get { return instance; } }

        static Keys[] DirKeys = new Keys[4] { Keys.S, Keys.A, Keys.W, Keys.D };

        public Loc FocusedLoc;
        public Loc DiffLoc;

        public Loc MouseLoc;
        public AutoTile AutoTileInProgress;
        public bool? BlockInProgress;
        public Rect RectInProgress;

        public Rect TileRectPreview()
        {
            return RectPreview(GraphicsManager.TileSize);
        }
        public Rect BlockRectPreview()
        {
            return RectPreview(GraphicsManager.TileSize / GroundMap.SUB_TILES);
        }
        public Rect RectPreview(int size)
        {
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

            var groundEditor = DiagManager.Instance.DevEditor.GroundEditor;

            if (groundEditor.Active)
            {
                groundEditor.ProcessInput(input);
            }
        }



        protected override IEnumerator<YieldInstruction> ProcessInput(InputManager input)
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

            if (ZoneManager.Instance.CurrentGround != null)
            {

                foreach (GroundChar character in ZoneManager.Instance.CurrentGround.IterateCharacters())
                {
                    if (character.EntEnabled)
                        character.UpdateView(elapsedTime);
                }


                FocusedLoc += DiffLoc;
                DiffLoc = new Loc();


                if (ZoneManager.Instance.CurrentGround.EdgeView == Map.ScrollEdge.Clamp)
                    FocusedLoc = new Loc(Math.Max(GraphicsManager.ScreenWidth / 2, Math.Min(FocusedLoc.X, ZoneManager.Instance.CurrentGround.Width * GraphicsManager.TileSize - GraphicsManager.ScreenWidth / 2)),
                        Math.Max(GraphicsManager.ScreenHeight / 2, Math.Min(FocusedLoc.Y, ZoneManager.Instance.CurrentGround.Height * GraphicsManager.TileSize - GraphicsManager.ScreenHeight / 2)));
                else
                    FocusedLoc = new Loc(Math.Max(0, Math.Min(FocusedLoc.X, ZoneManager.Instance.CurrentGround.Width * GraphicsManager.TileSize)),
                        Math.Max(0, Math.Min(FocusedLoc.Y, ZoneManager.Instance.CurrentGround.Height * GraphicsManager.TileSize)));

                UpdateCam(FocusedLoc);

                base.Update(elapsedTime);
            }

        }


        public override void DrawDev(SpriteBatch spriteBatch)
        {
            //
            //When in editor mode, we want to display an overlay over some entities
            //

            if (DiagManager.Instance.DevEditor.GroundEditor.Active && ZoneManager.Instance.CurrentGround != null)
            {
                if (AutoTileInProgress != null)
                {
                    for (int jj = viewTileRect.Y; jj < viewTileRect.End.Y; jj++)
                    {
                        for (int ii = viewTileRect.X; ii < viewTileRect.End.X; ii++)
                        {
                            if (Collision.InBounds(ZoneManager.Instance.CurrentGround.Width, ZoneManager.Instance.CurrentGround.Height, new Loc(ii, jj)) &&
                                Collision.InBounds(TileRectPreview(), new Loc(ii, jj)))
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
                for (int jj = viewTileRect.Y * GroundMap.SUB_TILES; jj < viewTileRect.End.Y * GroundMap.SUB_TILES; jj++)
                {
                    for (int ii = viewTileRect.X * GroundMap.SUB_TILES; ii < viewTileRect.End.X * GroundMap.SUB_TILES; ii++)
                    {
                        if (Collision.InBounds(ZoneManager.Instance.CurrentGround.Width * GroundMap.SUB_TILES, ZoneManager.Instance.CurrentGround.Height * GroundMap.SUB_TILES, new Loc(ii, jj)))
                        {
                            bool blocked = ZoneManager.Instance.CurrentGround.GetObstacle(ii, jj) == 1u;
                            if (BlockInProgress != null && Collision.InBounds(BlockRectPreview(), new Loc(ii, jj)))
                                blocked = BlockInProgress.Value;

                            if (blocked)
                                GraphicsManager.Pixel.Draw(spriteBatch, new Rectangle(ii * GraphicsManager.TileSize / GroundMap.SUB_TILES - ViewRect.X, jj * GraphicsManager.TileSize / GroundMap.SUB_TILES - ViewRect.Y, GraphicsManager.TileSize / GroundMap.SUB_TILES, GraphicsManager.TileSize / GroundMap.SUB_TILES), null, Color.Red * 0.5f);
                        }
                    }
                }

                if (!DataManager.Instance.HideGrid)
                {
                    for (int jj = viewTileRect.Y; jj < viewTileRect.End.Y; jj++)
                    {
                        for (int ii = viewTileRect.X; ii < viewTileRect.End.X; ii++)
                        {
                            if (Collision.InBounds(ZoneManager.Instance.CurrentGround.Width, ZoneManager.Instance.CurrentGround.Height, new Loc(ii, jj)))
                                GraphicsManager.Tiling.DrawTile(spriteBatch, new Vector2(ii * GraphicsManager.TileSize - ViewRect.X, jj * GraphicsManager.TileSize - ViewRect.Y), 0, 0);
                        }
                    }
                }

                //DevHasGraphics()
                //Draw Entity bounds
                GroundDebug dbg = new GroundDebug(spriteBatch, Color.BlueViolet);
                foreach (GroundEntity entity in ZoneManager.Instance.CurrentGround.IterateEntities())
                {
                    if (entity.DevEntitySelected)
                    {
                        //Invert the color of selected entities
                        dbg.DrawColor = new Color(entity.DevEntColoring.B, entity.DevEntColoring.G, entity.DevEntColoring.R, entity.DevEntColoring.A);
                        dbg.LineThickness = 1.0f;
                        dbg.DrawFilledBox(entity.Bounds, 92);
                    }
                    else if (!entity.DevHasGraphics())
                    {
                        //Draw entities with no graphics of their own as a filled box
                        dbg.DrawColor = entity.DevEntColoring;
                        dbg.LineThickness = 1.0f;
                        dbg.DrawFilledBox(entity.Bounds, 128);
                    }
                    else
                    {
                        //Draw boxes around other entities with graphics using low opacity
                        dbg.DrawColor = new Color(entity.DevEntColoring.R, entity.DevEntColoring.G, entity.DevEntColoring.B, 92);
                        dbg.LineThickness = 1.0f;
                        dbg.DrawBox(entity.Bounds);
                    }
                    //And don't draw bounds of entities that have a graphics representation
                }
            }

            base.DrawDev(spriteBatch);
        }


        public override void DrawDebug(SpriteBatch spriteBatch)
        {
            if (ZoneManager.Instance.CurrentGround != null)
            {
                Loc loc = ScreenCoordsToGroundCoords(MouseLoc);
                Loc blockLoc = ScreenCoordsToBlockCoords(MouseLoc);
                Loc tileLoc = ScreenCoordsToMapCoords(MouseLoc);
                GraphicsManager.SysFont.DrawText(spriteBatch, GraphicsManager.WindowWidth - 2, 32, String.Format("X:{0:D3} Y:{1:D3}", loc.X, loc.Y), null, DirV.Up, DirH.Right, Color.White);
                GraphicsManager.SysFont.DrawText(spriteBatch, GraphicsManager.WindowWidth - 2, 42, String.Format("Block X:{0:D3} Y:{1:D3}", blockLoc.X, blockLoc.Y), null, DirV.Up, DirH.Right, Color.White);
                GraphicsManager.SysFont.DrawText(spriteBatch, GraphicsManager.WindowWidth - 2, 52, String.Format("Tile X:{0:D3} Y:{1:D3}", tileLoc.X, tileLoc.Y), null, DirV.Up, DirH.Right, Color.White);
            }
        }


        public void EnterGroundEdit(int entryPoint)
        {
            if (ZoneManager.Instance.CurrentGround.Markers.Count > 0)
            {
                LocRay8 entry = ZoneManager.Instance.CurrentGround.GetEntryPoint(entryPoint);
                FocusedLoc = entry.Loc;
            }

            DiagManager.Instance.DevEditor.OpenGround();

        }

        public override void Exit()
        {

            DiagManager.Instance.DevEditor.CloseGround();

        }
    }
}
