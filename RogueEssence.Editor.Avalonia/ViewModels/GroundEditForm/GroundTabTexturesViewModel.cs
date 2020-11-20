using RogueElements;
using RogueEssence.Content;
using RogueEssence.Dungeon;

namespace RogueEssence.Dev.ViewModels
{
    public class GroundTabTexturesViewModel : ViewModelBase
    {
        public GroundTabTexturesViewModel()
        {
            Layers = new LayerBoxViewModel();
            TileBrowser = new TileBrowserViewModel();
        }

        private TileEditMode texMode;
        public TileEditMode TexMode
        {
            get { return texMode; }
            set
            {
                this.SetIfChanged(ref texMode, value);
            }
        }


        private int currentLayer;
        public int CurrentLayer
        {
            get { return currentLayer; }
            set { this.SetIfChanged(ref currentLayer, value); }
        }

        public LayerBoxViewModel Layers { get; set; }
        public TileBrowserViewModel TileBrowser { get; set; }

        public void ProcessInput(InputManager input)
        {
            Loc tileCoords = GroundEditScene.Instance.ScreenCoordsToMapCoords(input.MouseLoc);
            switch (TexMode)
            {
                case TileEditMode.Draw:
                    {
                        if (input[FrameInput.InputType.LeftMouse])
                            paintTile(tileCoords, TileBrowser.GetBrush());
                        else if (input[FrameInput.InputType.RightMouse])
                            paintTile(tileCoords, new TileBrush(new TileLayer(), Loc.One));
                    }
                    break;
                case TileEditMode.Rectangle:
                    {
                        Loc groundCoords = GroundEditScene.Instance.ScreenCoordsToGroundCoords(input.MouseLoc);
                        if (input.JustPressed(FrameInput.InputType.LeftMouse))
                        {
                            GroundEditScene.Instance.AutoTileInProgress = TileBrowser.GetBrush().GetSanitizedTile();
                            GroundEditScene.Instance.RectInProgress = new Rect(groundCoords, Loc.Zero);
                        }
                        else if (input[FrameInput.InputType.LeftMouse])
                            GroundEditScene.Instance.RectInProgress.Size = (groundCoords - GroundEditScene.Instance.RectInProgress.Start);
                        else if (input.JustReleased(FrameInput.InputType.LeftMouse))
                        {
                            rectTile(GroundEditScene.Instance.TileRectPreview(), TileBrowser.GetBrush());
                            GroundEditScene.Instance.AutoTileInProgress = null;
                        }
                        else if (input.JustPressed(FrameInput.InputType.RightMouse))
                        {
                            GroundEditScene.Instance.AutoTileInProgress = new AutoTile(new TileLayer());
                            GroundEditScene.Instance.RectInProgress = new Rect(groundCoords, Loc.Zero);
                        }
                        else if (input[FrameInput.InputType.RightMouse])
                            GroundEditScene.Instance.RectInProgress.Size = (groundCoords - GroundEditScene.Instance.RectInProgress.Start);
                        else if (input.JustReleased(FrameInput.InputType.RightMouse))
                        {
                            rectTile(GroundEditScene.Instance.TileRectPreview(), new TileBrush(new TileLayer(), Loc.One));
                            GroundEditScene.Instance.AutoTileInProgress = null;
                        }
                    }
                    break;
                case TileEditMode.Fill:
                    {
                        if (input.JustReleased(FrameInput.InputType.LeftMouse))
                            fillTile(tileCoords, TileBrowser.GetBrush());
                        else if (input.JustReleased(FrameInput.InputType.RightMouse))
                            fillTile(tileCoords, new TileBrush(new TileLayer(), Loc.One));
                    }
                    break;
                case TileEditMode.Eyedrop:
                    {
                        if (input[FrameInput.InputType.LeftMouse])
                            eyedropTile(tileCoords);
                    }
                    break;
            }

        }


        private void paintTile(Loc loc, TileBrush brush)
        {
            if (!Collision.InBounds(ZoneManager.Instance.CurrentGround.Width, ZoneManager.Instance.CurrentGround.Height, loc))
                return;

            if (brush.MultiSelect == Loc.One)
                ZoneManager.Instance.CurrentGround.Layers[Layers.ChosenLayer].Tiles[loc.X][loc.Y] = brush.GetSanitizedTile();
            else
            {
                for (int xx = 0; xx < brush.MultiSelect.X; xx++)
                {
                    for (int yy = 0; yy < brush.MultiSelect.Y; yy++)
                    {
                        Loc offset = new Loc(xx, yy);
                        if (!Collision.InBounds(ZoneManager.Instance.CurrentGround.Width, ZoneManager.Instance.CurrentGround.Height, loc + offset))
                            continue;
                        ZoneManager.Instance.CurrentGround.Layers[Layers.ChosenLayer].Tiles[loc.X + xx][loc.Y + yy] = brush.GetSanitizedTile(offset);
                    }
                }
            }
        }

        private void rectTile(Rect rect, TileBrush brush)
        {
            for (int xx = rect.X; xx < rect.End.X; xx++)
            {
                for (int yy = rect.Y; yy < rect.End.Y; yy++)
                {
                    if (!Collision.InBounds(ZoneManager.Instance.CurrentGround.Width, ZoneManager.Instance.CurrentGround.Height, new Loc(xx, yy)))
                        continue;

                    ZoneManager.Instance.CurrentGround.Layers[Layers.ChosenLayer].Tiles[xx][yy] = brush.GetSanitizedTile();
                }
            }
        }

        private void eyedropTile(Loc loc)
        {
            if (!Collision.InBounds(ZoneManager.Instance.CurrentGround.Width, ZoneManager.Instance.CurrentGround.Height, loc))
                return;

            TileBrowser.SetBrush(ZoneManager.Instance.CurrentGround.Layers[Layers.ChosenLayer].Tiles[loc.X][loc.Y]);
        }


        private void fillTile(Loc loc, TileBrush brush)
        {
            if (!Collision.InBounds(ZoneManager.Instance.CurrentGround.Width, ZoneManager.Instance.CurrentGround.Height, loc))
                return;

            AutoTile tile = ZoneManager.Instance.CurrentGround.Layers[Layers.ChosenLayer].Tiles[loc.X][loc.Y].Copy();

            Grid.FloodFill(new Rect(0, 0, ZoneManager.Instance.CurrentGround.Width, ZoneManager.Instance.CurrentGround.Height),
                    (Loc testLoc) =>
                    {
                        return !tile.Equals(ZoneManager.Instance.CurrentGround.Layers[Layers.ChosenLayer].Tiles[testLoc.X][testLoc.Y]);
                    },
                    (Loc testLoc) =>
                    {
                        return true;
                    },
                    (Loc testLoc) =>
                    {
                        ZoneManager.Instance.CurrentGround.Layers[Layers.ChosenLayer].Tiles[testLoc.X][testLoc.Y] = brush.GetSanitizedTile();
                    },
                loc);
        }
    }
}
