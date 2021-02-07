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
            if (instance != null)
                GraphicsManager.ZoomChanged -= instance.ZoomChanged;
            instance = new GroundEditScene();
            GraphicsManager.ZoomChanged += instance.ZoomChanged;
        }
        public static GroundEditScene Instance { get { return instance; } }

        static Keys[] DirKeys = new Keys[4] { Keys.S, Keys.A, Keys.W, Keys.D };

        public Loc FocusedLoc;
        public Loc DiffLoc;

        public AutoTile AutoTileInProgress;
        public bool? BlockInProgress;
        public Rect RectInProgress;
        public bool ShowWalls;

        public Rect TileRectPreview()
        {
            return RectPreview(ZoneManager.Instance.CurrentGround.TileSize);
        }
        public Rect BlockRectPreview()
        {
            return RectPreview(GraphicsManager.TEX_SIZE);
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
            base.UpdateMeta();

            InputManager input = GameManager.Instance.MetaInputManager;
            var groundEditor = DiagManager.Instance.DevEditor.GroundEditor;

            if (groundEditor.Active)
                groundEditor.ProcessInput(input);
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
                    FocusedLoc = new Loc(Math.Max(GraphicsManager.ScreenWidth / 2, Math.Min(FocusedLoc.X, ZoneManager.Instance.CurrentGround.GroundWidth - GraphicsManager.ScreenWidth / 2)),
                        Math.Max(GraphicsManager.ScreenHeight / 2, Math.Min(FocusedLoc.Y, ZoneManager.Instance.CurrentGround.GroundHeight - GraphicsManager.ScreenHeight / 2)));
                else
                    FocusedLoc = new Loc(Math.Max(0, Math.Min(FocusedLoc.X, ZoneManager.Instance.CurrentGround.GroundWidth)),
                        Math.Max(0, Math.Min(FocusedLoc.Y, ZoneManager.Instance.CurrentGround.GroundHeight)));

                UpdateCam(FocusedLoc);

                base.Update(elapsedTime);
            }

        }


        public override void DrawGame(SpriteBatch spriteBatch)
        {
            //
            //When in editor mode, we want to display an overlay over some entities
            //
            base.DrawGame(spriteBatch);


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
                                    GraphicsManager.Pixel.Draw(spriteBatch, new Rectangle(ii * ZoneManager.Instance.CurrentGround.TileSize - ViewRect.X, jj * ZoneManager.Instance.CurrentGround.TileSize - ViewRect.Y, ZoneManager.Instance.CurrentGround.TileSize, ZoneManager.Instance.CurrentGround.TileSize), null, Color.Black);
                                else
                                    AutoTileInProgress.Draw(spriteBatch, new Loc(ii * ZoneManager.Instance.CurrentGround.TileSize, jj * ZoneManager.Instance.CurrentGround.TileSize) - ViewRect.Start);
                            }
                        }
                    }
                }

                //draw the blocks
                if (ShowWalls)
                {
                    int texSize = ZoneManager.Instance.CurrentGround.TexSize;
                    for (int jj = viewTileRect.Y * texSize; jj < viewTileRect.End.Y * texSize; jj++)
                    {
                        for (int ii = viewTileRect.X * texSize; ii < viewTileRect.End.X * texSize; ii++)
                        {
                            if (Collision.InBounds(ZoneManager.Instance.CurrentGround.Width * texSize, ZoneManager.Instance.CurrentGround.Height * texSize, new Loc(ii, jj)))
                            {
                                bool blocked = ZoneManager.Instance.CurrentGround.GetObstacle(ii, jj) == 1u;
                                if (BlockInProgress != null && Collision.InBounds(BlockRectPreview(), new Loc(ii, jj)))
                                    blocked = BlockInProgress.Value;

                                if (blocked)
                                    GraphicsManager.Pixel.Draw(spriteBatch, new Rectangle(ii * GraphicsManager.TEX_SIZE - ViewRect.X, jj * GraphicsManager.TEX_SIZE - ViewRect.Y, GraphicsManager.TEX_SIZE, GraphicsManager.TEX_SIZE), null, Color.Red * 0.6f);
                            }
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

        }


        public override void DrawDev(SpriteBatch spriteBatch)
        {
            BaseSheet blank = GraphicsManager.Pixel;
            int tileSize = ZoneManager.Instance.CurrentGround.TileSize;
            for (int jj = viewTileRect.Y; jj < viewTileRect.End.Y; jj++)
            {
                for (int ii = viewTileRect.X; ii < viewTileRect.End.X; ii++)
                {
                    if (Collision.InBounds(ZoneManager.Instance.CurrentGround.Width, ZoneManager.Instance.CurrentGround.Height, new Loc(ii, jj)))
                    {
                        blank.Draw(spriteBatch, new Rectangle((int)((ii * tileSize - ViewRect.X) * windowScale * scale), (int)((jj * tileSize - ViewRect.Y) * windowScale * scale), (int)(tileSize * windowScale * scale), 1), null, Color.White * 0.5f);
                        blank.Draw(spriteBatch, new Rectangle((int)((ii * tileSize - ViewRect.X) * windowScale * scale), (int)((jj * tileSize - ViewRect.Y) * windowScale * scale), 1, (int)(tileSize * windowScale * scale)), null, Color.White * 0.5f);
                    }
                    else if (ii == ZoneManager.Instance.CurrentGround.Width && Collision.InBounds(ZoneManager.Instance.CurrentGround.Height, jj))
                        blank.Draw(spriteBatch, new Rectangle((int)((ii * tileSize - ViewRect.X) * windowScale * scale), (int)((jj * tileSize - ViewRect.Y) * windowScale * scale), 1, (int)(tileSize * windowScale * scale)), null, Color.White * 0.5f);
                    else if (jj == ZoneManager.Instance.CurrentGround.Height && Collision.InBounds(ZoneManager.Instance.CurrentGround.Width, ii))
                        blank.Draw(spriteBatch, new Rectangle((int)((ii * tileSize - ViewRect.X) * windowScale * scale), (int)((jj * tileSize - ViewRect.Y) * windowScale * scale), (int)(tileSize * windowScale * scale), 1), null, Color.White * 0.5f);
                }
            }

            base.DrawDev(spriteBatch);
        }


        public void EnterGroundEdit(int entryPoint)
        {
            if (ZoneManager.Instance.CurrentGround.Entities[0].Markers.Count > 0)
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
