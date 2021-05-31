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
            TileBrowser.CanMultiSelect = true;
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
            bool inWindow = Collision.InBounds(GraphicsManager.WindowWidth, GraphicsManager.WindowHeight, input.MouseLoc);

            Loc tileCoords = DungeonEditScene.Instance.ScreenCoordsToMapCoords(input.MouseLoc);
            switch (TexMode)
            {
                case TileEditMode.Draw:
                    {
                        TileBrush brush = getBrush();
                        if (brush.MultiSelect == Loc.One)
                        {
                            CanvasStroke<AutoTile>.ProcessCanvasInput(input, tileCoords, inWindow,
                                () => new DrawStroke<AutoTile>(tileCoords, brush.GetSanitizedTile()),
                                () => new DrawStroke<AutoTile>(tileCoords, new AutoTile()),
                                paintStroke, ref DungeonEditScene.Instance.AutoTileInProgress);
                        }
                        else
                        {
                            CanvasStroke<AutoTile>.ProcessCanvasInput(input, tileCoords, inWindow,
                                () => new ClusterStroke<AutoTile>(tileCoords, getCluster(brush)),
                                () => new DrawStroke<AutoTile>(tileCoords, new AutoTile()),
                                paintStroke, ref DungeonEditScene.Instance.AutoTileInProgress);
                        }
                    }
                    break;
                case TileEditMode.Rectangle:
                    {
                        CanvasStroke<AutoTile>.ProcessCanvasInput(input, tileCoords, inWindow,
                            () => new RectStroke<AutoTile>(tileCoords, getBrush().GetSanitizedTile()),
                            () => new RectStroke<AutoTile>(tileCoords, new AutoTile()),
                            paintStroke, ref DungeonEditScene.Instance.AutoTileInProgress);
                    }
                    break;
                case TileEditMode.Fill:
                    {
                        CanvasStroke<AutoTile>.ProcessCanvasInput(input, tileCoords, inWindow,
                            () => new FillStroke<AutoTile>(tileCoords, getBrush().GetSanitizedTile()),
                            () => new FillStroke<AutoTile>(tileCoords, new AutoTile()),
                            fillStroke, ref DungeonEditScene.Instance.AutoTileInProgress);
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



        private AutoTile[][] getCluster(TileBrush brush)
        {
            AutoTile[][] tiles = new AutoTile[brush.MultiSelect.X][];
            for (int xx = 0; xx < brush.MultiSelect.X; xx++)
            {
                tiles[xx] = new AutoTile[brush.MultiSelect.Y];
                for (int yy = 0; yy < brush.MultiSelect.Y; yy++)
                    tiles[xx][yy] = brush.GetSanitizedTile(new Loc(xx, yy));
            }
            return tiles;
        }

        private void paintStroke(CanvasStroke<AutoTile> stroke)
        {
            foreach (Loc loc in stroke.GetLocs())
            {
                if (!Collision.InBounds(ZoneManager.Instance.CurrentMap.Width, ZoneManager.Instance.CurrentMap.Height, loc))
                    continue;

                ZoneManager.Instance.CurrentMap.Tiles[loc.X][loc.Y].FloorTile = stroke.GetBrush(loc).Copy();
            }

            //now recompute all tiles within the multiselect rectangle + 1
            Rect bounds = stroke.CoveredRect;
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



        private void fillStroke(CanvasStroke<AutoTile> stroke)
        {
            if (!Collision.InBounds(ZoneManager.Instance.CurrentMap.Width, ZoneManager.Instance.CurrentMap.Height, stroke.CoveredRect.Start))
                return;

            AutoTile tile = ZoneManager.Instance.CurrentMap.Tiles[stroke.CoveredRect.Start.X][stroke.CoveredRect.Start.Y].FloorTile.Copy();
            Rect bounds = new Rect(stroke.CoveredRect.Start, Loc.One);

            AutoTile brushTile = stroke.GetBrush(stroke.CoveredRect.Start);
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
                            new Loc(Math.Max(bounds.End.X, testLoc.X + 1), Math.Max(bounds.End.Y, testLoc.Y + 1)));
                        ZoneManager.Instance.CurrentMap.Tiles[testLoc.X][testLoc.Y].FloorTile = brushTile.Copy();
                    },
                stroke.CoveredRect.Start);

            //now recompute all autotiles within the rectangle
            bounds.Inflate(1, 1);
            ZoneManager.Instance.CurrentMap.CalculateAutotiles(bounds.Start, bounds.Size);
        }
    }
}
