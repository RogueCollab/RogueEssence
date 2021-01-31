using RogueElements;
using RogueEssence.Content;
using RogueEssence.Dungeon;
using System;

namespace RogueEssence.Dev.ViewModels
{
    public class MapTabTexturesViewModel : ViewModelBase
    {
        public MapTabTexturesViewModel()
        {
            TileBrowser = new TileBrowserViewModel();
            AutotileBrowser = new AutotileBrowserViewModel();
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

        public TileBrowserViewModel TileBrowser { get; set; }
        public AutotileBrowserViewModel AutotileBrowser { get; set; }



        private int tabIndex;
        public int TabIndex
        {
            get => tabIndex;
            set
            {
                this.SetIfChanged(ref tabIndex, value);
            }
        }


        public void ProcessInput(InputManager input)
        {
            Loc tileCoords = DungeonEditScene.Instance.ScreenCoordsToMapCoords(input.MouseLoc);
            switch (TexMode)
            {
                case TileEditMode.Draw:
                    {
                        if (input[FrameInput.InputType.LeftMouse])
                            paintTile(tileCoords, getBrush());
                        else if (input[FrameInput.InputType.RightMouse])
                            paintTile(tileCoords, new TileBrush(new TileLayer(), Loc.One));
                    }
                    break;
                case TileEditMode.Rectangle:
                    {
                        if (input.JustPressed(FrameInput.InputType.LeftMouse))
                        {
                            DungeonEditScene.Instance.AutoTileInProgress = getBrush().GetSanitizedTile();
                            DungeonEditScene.Instance.RectInProgress = new Rect(tileCoords, Loc.Zero);
                        }
                        else if (input[FrameInput.InputType.LeftMouse])
                            DungeonEditScene.Instance.RectInProgress.Size = (tileCoords - DungeonEditScene.Instance.RectInProgress.Start);
                        else if (input.JustReleased(FrameInput.InputType.LeftMouse))
                        {
                            rectTile(DungeonEditScene.Instance.RectPreview(), getBrush());
                            DungeonEditScene.Instance.AutoTileInProgress = null;
                        }
                        else if (input.JustPressed(FrameInput.InputType.RightMouse))
                        {
                            DungeonEditScene.Instance.AutoTileInProgress = new AutoTile(new TileLayer());
                            DungeonEditScene.Instance.RectInProgress = new Rect(tileCoords, Loc.Zero);
                        }
                        else if (input[FrameInput.InputType.RightMouse])
                            DungeonEditScene.Instance.RectInProgress.Size = (tileCoords - DungeonEditScene.Instance.RectInProgress.Start);
                        else if (input.JustReleased(FrameInput.InputType.RightMouse))
                        {
                            rectTile(DungeonEditScene.Instance.RectPreview(), new TileBrush(new TileLayer(), Loc.One));
                            DungeonEditScene.Instance.AutoTileInProgress = null;
                        }
                    }
                    break;
                case TileEditMode.Fill:
                    {
                        if (input.JustReleased(FrameInput.InputType.LeftMouse))
                            fillTile(tileCoords, getBrush());
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

        private TileBrush getBrush()
        {
            if (tabIndex == 0)
                return TileBrowser.GetBrush();
            else
                return AutotileBrowser.GetBrush();
        }

        private void paintTile(Loc loc, TileBrush brush)
        {
            if (!Collision.InBounds(ZoneManager.Instance.CurrentMap.Width, ZoneManager.Instance.CurrentMap.Height, loc))
                return;

            if (brush.MultiSelect == Loc.One)
                ZoneManager.Instance.CurrentMap.Tiles[loc.X][loc.Y].FloorTile = brush.GetSanitizedTile();
            else
            {
                for (int xx = 0; xx < brush.MultiSelect.X; xx++)
                {
                    for (int yy = 0; yy < brush.MultiSelect.Y; yy++)
                    {
                        Loc offset = new Loc(xx, yy);
                        if (!Collision.InBounds(ZoneManager.Instance.CurrentMap.Width, ZoneManager.Instance.CurrentMap.Height, loc + offset))
                            continue;
                        ZoneManager.Instance.CurrentMap.Tiles[loc.X + xx][loc.Y + yy].FloorTile = brush.GetSanitizedTile(offset);
                    }
                }
            }

            Rect bounds = new Rect(loc, brush.MultiSelect);
            //now recompute all tiles within the multiselect rectangle + 1
            bounds.Inflate(1, 1);
            ZoneManager.Instance.CurrentMap.CalculateAutotiles(bounds.Start, bounds.Size);
        }

        private void rectTile(Rect rect, TileBrush brush)
        {
            for (int xx = rect.X; xx < rect.End.X; xx++)
            {
                for (int yy = rect.Y; yy < rect.End.Y; yy++)
                {
                    if (!Collision.InBounds(ZoneManager.Instance.CurrentMap.Width, ZoneManager.Instance.CurrentMap.Height, new Loc(xx, yy)))
                        continue;

                    ZoneManager.Instance.CurrentMap.Tiles[xx][yy].FloorTile = brush.GetSanitizedTile();
                }
            }

            Rect bounds = rect;
            //now recompute all tiles within the multiselect rectangle + 1
            bounds.Inflate(1, 1);
            ZoneManager.Instance.CurrentMap.CalculateAutotiles(bounds.Start, bounds.Size);
        }

        private void eyedropTile(Loc loc)
        {
            if (!Collision.InBounds(ZoneManager.Instance.CurrentMap.Width, ZoneManager.Instance.CurrentMap.Height, loc))
                return;

            AutoTile autoTile = ZoneManager.Instance.CurrentMap.Tiles[loc.X][loc.Y].FloorTile;

            if (autoTile.AutoTileset > -1)
            {
                //switch to autotile tab
                AutotileBrowser.SetBrush(autoTile);
            }
            else
            {
                TileLayer layer = (autoTile.Layers.Count > 0) ? autoTile.Layers[0] : new TileLayer();
                TileBrowser.SetBrush(layer);
            }
        }


        private void fillTile(Loc loc, TileBrush brush)
        {
            if (!Collision.InBounds(ZoneManager.Instance.CurrentMap.Width, ZoneManager.Instance.CurrentMap.Height, loc))
                return;

            AutoTile tile = ZoneManager.Instance.CurrentMap.Tiles[loc.X][loc.Y].FloorTile.Copy();
            Rect bounds = new Rect(loc, Loc.One);
            Grid.FloodFill(new Rect(0, 0, ZoneManager.Instance.CurrentMap.Width, ZoneManager.Instance.CurrentMap.Height),
                    (Loc testLoc) =>
                    {
                        return !tile.Equals(ZoneManager.Instance.CurrentMap.Tiles[testLoc.X][testLoc.Y].FloorTile);
                    },
                    (Loc testLoc) =>
                    {
                        return true;
                    },
                    (Loc testLoc) =>
                    {
                        bounds = Rect.FromPoints(new Loc(Math.Min(bounds.X, testLoc.X), Math.Min(bounds.Y, testLoc.Y)),
                            new Loc(Math.Max(bounds.End.X, testLoc.X+1), Math.Max(bounds.End.Y, testLoc.Y + 1)));
                        ZoneManager.Instance.CurrentMap.Tiles[testLoc.X][testLoc.Y].FloorTile = brush.GetSanitizedTile();
                    },
                loc);

            //now recompute all autotiles within the rectangle
            bounds.Inflate(1, 1);
            ZoneManager.Instance.CurrentMap.CalculateAutotiles(bounds.Start, bounds.Size);
        }
    }
}
