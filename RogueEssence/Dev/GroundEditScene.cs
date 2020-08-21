using System;
using System.Collections.Generic;
using RogueEssence.Content;
using RogueElements;
using RogueEssence.Data;
using RogueEssence.Dungeon;
using RogueEssence.Ground;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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


        public Loc FocusedLoc;
        public Loc DiffLoc;



        public override void UpdateMeta()
        {
            InputManager input = GameManager.Instance.MetaInputManager;

#if EDITORS
            if (Dev.GroundEditor.MapEditing)
            {
                if (Collision.InBounds(GraphicsManager.WindowWidth, GraphicsManager.WindowHeight, input.MouseLoc))
                {
                    if (Dev.GroundEditor.ChosenEditMode == Dev.GroundEditor.TileEditMode.Draw)
                    {
                        if (input[FrameInput.InputType.LeftMouse])
                            Dev.GroundEditor.PaintTile(ScreenCoordsToMapCoords(input.MouseLoc), Dev.DevForm.CurrentGroundEditor.GetBrush());
                        else if (input[FrameInput.InputType.RightMouse])
                            Dev.GroundEditor.PaintTile(ScreenCoordsToMapCoords(input.MouseLoc), new TileLayer());

                    }
                    else if (Dev.GroundEditor.ChosenEditMode == Dev.GroundEditor.TileEditMode.Eyedrop)
                    {
                        if (input[FrameInput.InputType.LeftMouse])
                            Dev.DevForm.CurrentGroundEditor.EyedropTile(ScreenCoordsToMapCoords(input.MouseLoc));
                        else if (input[FrameInput.InputType.LeftMouse])
                        {

                        }
                    }
                    else if (Dev.GroundEditor.ChosenEditMode == Dev.GroundEditor.TileEditMode.Fill)
                    {
                        if (input.JustReleased(FrameInput.InputType.LeftMouse))
                            Dev.GroundEditor.FillTile(ScreenCoordsToMapCoords(input.MouseLoc), Dev.DevForm.CurrentGroundEditor.GetBrush());
                        else if (input.JustReleased(FrameInput.InputType.RightMouse))
                            Dev.GroundEditor.FillTile(ScreenCoordsToMapCoords(input.MouseLoc), new TileLayer());
                    }
                    else if (Dev.GroundEditor.ChosenEditMode == Dev.GroundEditor.TileEditMode.PlaceEntity)
                    {
                        Loc coords = ScreenCoordsToGroundCoords(input.MouseLoc);
                        if (input.JustReleased(FrameInput.InputType.LeftMouse))
                        {
                            Dev.DevForm.CurrentGroundEditor.PlaceEntity(coords);
                        }
                        //else if (input.JustReleased(FrameInput.InputType.RightMouse))
                    }
                    else if (Dev.GroundEditor.ChosenEditMode == Dev.GroundEditor.TileEditMode.PlaceTemplateEntity)
                    {
                        Loc coords = ScreenCoordsToGroundCoords(input.MouseLoc);
                        if (input.JustReleased(FrameInput.InputType.LeftMouse))
                        {
                            Dev.DevForm.CurrentGroundEditor.PlaceTemplateEntity(coords);
                        }
                        //else if (input.JustReleased(FrameInput.InputType.RightMouse))
                    }
                    else if (Dev.GroundEditor.ChosenEditMode == Dev.GroundEditor.TileEditMode.SelectEntity)
                    {
                        Loc coords = ScreenCoordsToGroundCoords(input.MouseLoc);
                        //GraphicsManager.GraphicsDevice.Viewport.Bounds.Contains(input.MouseLoc.X, input.MouseLoc.Y)

                        if (input.JustReleased(FrameInput.InputType.LeftMouse))
                            Dev.DevForm.CurrentGroundEditor.SelectEntity(coords);
                        else if (input.JustReleased(FrameInput.InputType.RightMouse))
                            Dev.DevForm.CurrentGroundEditor.EntityContext(input.MouseLoc, coords);
                    }
                }
            }
#endif
        }


        protected override IEnumerator<YieldInstruction> ProcessInput(InputManager input)
        {
            GameAction action = new GameAction(GameAction.ActionType.None, Dir8.None);

            bool run = input[FrameInput.InputType.Run];

            if (input.Direction != Dir8.None)
            {
                GameAction.ActionType cmdType = GameAction.ActionType.Dir;

                //if (FrameTick.FromFrames(input.InputTime) > FrameTick.FromFrames(2)) //TODO: ensure that it does not freeze when transitioning walk to run and vice versa
                cmdType = GameAction.ActionType.Move;

                action = new GameAction(cmdType, input.Direction);

                if (cmdType == GameAction.ActionType.Move)
                    action.AddArg(input[FrameInput.InputType.Run] ? 1 : 0);
            }

            if (action.Type != GameAction.ActionType.None)
                DiffLoc = action.Dir.GetLoc() * (action[0] == 1 ? 1 : 8);

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
#if EDITORS
            if (Dev.GroundEditor.MapEditing && ZoneManager.Instance.CurrentGround != null)
            {

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
#endif
            base.DrawDev(spriteBatch);
        }


        public void EnterGroundEdit(int entryPoint)
        {
            if (ZoneManager.Instance.CurrentGround.Markers.Count > 0)
            {
                LocRay8 entry = ZoneManager.Instance.CurrentGround.GetEntryPoint(entryPoint);
                FocusedLoc = entry.Loc;
            }
#if EDITORS
            Dev.DevForm.OpenGround();
#endif
        }

        public override void Exit()
        {
#if EDITORS
            Dev.DevForm.CloseGround();
#endif
        }
    }
}
