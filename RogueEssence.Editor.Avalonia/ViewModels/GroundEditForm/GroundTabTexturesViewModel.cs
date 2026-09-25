using RogueElements;
using RogueEssence.Content;
using RogueEssence.Dungeon;
using ReactiveUI;
using System;
using System.Collections.Generic;

namespace RogueEssence.Dev.ViewModels
{
    public class GroundTabTexturesViewModel : ViewModelBase
    {
        public GroundTabTexturesViewModel()
        {
            Layers = new TextureLayerBoxViewModel(true);
            TileBrowser = new TileBrowserViewModel();
            TileBrowser.CanMultiSelect = true;
            AutotileBrowser = new AutotileBrowserViewModel();
        }

        private AutoTile[][] copiedRegion;

        private TileEditMode texMode;
        public TileEditMode TexMode
        {
            get { return texMode; }
            set
            {
                if (this.SetIfChanged(ref texMode, value))
                    CancelStroke();
            }
        }


        public ILayerBoxViewModel Layers { get; set; }
        public TileBrowserViewModel TileBrowser { get; set; }
        public AutotileBrowserViewModel AutotileBrowser { get; set; }



        private int tabIndex;
        public int TabIndex
        {
            get => tabIndex;
            set
            {
                if (this.SetIfChanged(ref tabIndex, value))
                    CancelStroke();
            }
        }


        public void ProcessInput(InputManager input)
        {
            bool inWindow = Collision.InBounds(GraphicsManager.WindowWidth, GraphicsManager.WindowHeight, input.MouseLoc);

            Loc tileCoords = GroundEditScene.Instance.ScreenCoordsToMapCoords(input.MouseLoc);
            switch (TexMode)
            {
                case TileEditMode.Draw:
                    {
                        TileBrush brush = getBrush();
                        if (brush.MultiSelect == Loc.One)
                        {
                            CanvasStroke<AutoTile>.ProcessCanvasInput(input, tileCoords, inWindow,
                                () =>
                                {
                                    AutoTile tex = brush.GetSanitizedTile();
                                    if (tex.IsEmpty())
                                        return null;
                                    return new DrawStroke<AutoTile>(tileCoords, tex);
                                },
                                () => new DrawStroke<AutoTile>(tileCoords, new AutoTile()),
                                paintStroke, ref GroundEditScene.Instance.AutoTileInProgress);
                        }
                        else
                        {
                            CanvasStroke<AutoTile>.ProcessCanvasInput(input, tileCoords, inWindow,
                                () => new ClusterStroke<AutoTile>(tileCoords, getCluster(brush)),
                                () => new DrawStroke<AutoTile>(tileCoords, new AutoTile()),
                                paintStroke, ref GroundEditScene.Instance.AutoTileInProgress);
                        }
                    }
                    break;
                case TileEditMode.Rectangle:
                    {
                        CanvasStroke<AutoTile>.ProcessCanvasInput(input, tileCoords, inWindow,
                                () =>
                                {
                                    AutoTile tex = getBrush().GetSanitizedTile();
                                    if (tex.IsEmpty())
                                        return null;
                                    return new RectStroke<AutoTile>(tileCoords, tex);
                                },
                            () => new RectStroke<AutoTile>(tileCoords, new AutoTile()),
                            paintStroke, ref GroundEditScene.Instance.AutoTileInProgress);
                    }
                    break;
                case TileEditMode.Fill:
                    {
                        CanvasStroke<AutoTile>.ProcessCanvasInput(input, tileCoords, inWindow,
                                () =>
                                {
                                    AutoTile tex = getBrush().GetSanitizedTile();
                                    if (tex.IsEmpty())
                                        return null;
                                    return new FillStroke<AutoTile>(tileCoords, tex);
                                },
                            () => new FillStroke<AutoTile>(tileCoords, new AutoTile()),
                            fillStroke, ref GroundEditScene.Instance.AutoTileInProgress);
                    }
                    break;
                case TileEditMode.Eyedrop:
                    {
                        if (input[FrameInput.InputType.LeftMouse])
                            eyedropTile(tileCoords);
                    }
                    break;
                case TileEditMode.Copy:
                    {
                        CanvasStroke<bool>.ProcessCanvasInput(input, tileCoords, inWindow,
                            () => new RectStroke<bool>(tileCoords, true),
                            () => null,
                            copyRegion, ref GroundEditScene.Instance.TextureSelectionInProgress);
                    }
                    break;
                case TileEditMode.Paste:
                    {
                        CanvasStroke<AutoTile>.ProcessCanvasInput(input, tileCoords, inWindow,
                            () => copiedRegion == null ? null : new ClusterStroke<AutoTile>(tileCoords, copiedRegion),
                            () => null,
                            paintStroke, ref GroundEditScene.Instance.AutoTileInProgress);
                    }
                    break;
            }
        }

        public bool CanPaste => copiedRegion != null;

        public string CopyStatus
        {
            get
            {
                if (copiedRegion == null)
                    return "Ctrl+C: drag a region on the map  Ctrl+V: paste it";
                return String.Format("Copied {0} x {1} tiles from the active texture layer", copiedRegion.Length, copiedRegion[0].Length);
            }
        }

        public void BeginCopy()
        {
            TexMode = TileEditMode.Copy;
            CancelStroke();
        }

        public void BeginPaste()
        {
            if (copiedRegion == null)
                return;

            TexMode = TileEditMode.Paste;
            CancelStroke();
        }

        public void CancelStroke()
        {
            GroundEditScene.Instance.AutoTileInProgress = null;
            GroundEditScene.Instance.TextureSelectionInProgress = null;
        }

        private void copyRegion(CanvasStroke<bool> stroke)
        {
            Rect mapBounds = new Rect(0, 0, ZoneManager.Instance.CurrentGround.Width, ZoneManager.Instance.CurrentGround.Height);
            Rect selected = Rect.Intersect(stroke.CoveredRect, mapBounds);
            if (selected.Size.X <= 0 || selected.Size.Y <= 0)
                return;

            copiedRegion = new AutoTile[selected.Size.X][];
            for (int xx = 0; xx < selected.Size.X; xx++)
            {
                copiedRegion[xx] = new AutoTile[selected.Size.Y];
                for (int yy = 0; yy < selected.Size.Y; yy++)
                {
                    AutoTile tile = ZoneManager.Instance.CurrentGround.Layers[Layers.ChosenLayer].Tiles[selected.X + xx][selected.Y + yy];
                    copiedRegion[xx][yy] = tile.Copy();
                }
            }

            this.RaisePropertyChanged(nameof(CanPaste));
            this.RaisePropertyChanged(nameof(CopyStatus));
            TexMode = TileEditMode.Paste;
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
            Dictionary<Loc, AutoTile> brush = new Dictionary<Loc, AutoTile>();
            Rect appliedBounds = new Rect();
            foreach (Loc loc in stroke.GetLocs())
            {
                if (!Collision.InBounds(ZoneManager.Instance.CurrentGround.Width, ZoneManager.Instance.CurrentGround.Height, loc))
                    continue;

                appliedBounds = brush.Count == 0 ? Rect.FromPoint(loc) : Rect.IncludeLoc(appliedBounds, loc);
                brush[loc] = stroke.GetBrush(loc).Copy();
            }

            if (brush.Count > 0)
                DiagManager.Instance.DevEditor.GroundEditor.Edits.Apply(new DrawGroundTexUndo(Layers.ChosenLayer, brush, appliedBounds));
        }

        private void eyedropTile(Loc loc)
        {
            if (!Collision.InBounds(ZoneManager.Instance.CurrentGround.Width, ZoneManager.Instance.CurrentGround.Height, loc))
                return;

            AutoTile autoTile = ZoneManager.Instance.CurrentGround.Layers[Layers.ChosenLayer].Tiles[loc.X][loc.Y];

            if (!String.IsNullOrEmpty(autoTile.AutoTileset))
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
            if (!Collision.InBounds(ZoneManager.Instance.CurrentGround.Width, ZoneManager.Instance.CurrentGround.Height, stroke.CoveredRect.Start))
                return;

            AutoTile tile = ZoneManager.Instance.CurrentGround.Layers[Layers.ChosenLayer].Tiles[stroke.CoveredRect.Start.X][stroke.CoveredRect.Start.Y].Copy();
            Rect bounds = new Rect(stroke.CoveredRect.Start, Loc.One);

            Dictionary<Loc, AutoTile> brush = new Dictionary<Loc, AutoTile>();
            AutoTile brushTile = stroke.GetBrush(stroke.CoveredRect.Start);
            Grid.FloodFill(new Rect(0, 0, ZoneManager.Instance.CurrentGround.Width, ZoneManager.Instance.CurrentGround.Height),
                    (Loc testLoc) =>
                    {
                        if (brush.ContainsKey(testLoc))
                            return true;
                        return !tile.Equals(ZoneManager.Instance.CurrentGround.Layers[Layers.ChosenLayer].Tiles[testLoc.X][testLoc.Y]);
                    },
                    (Loc testLoc) =>
                    {
                        return true;
                    },
                    (Loc testLoc) =>
                    {
                        bounds = Rect.FromPoints(new Loc(Math.Min(bounds.X, testLoc.X), Math.Min(bounds.Y, testLoc.Y)),
                            new Loc(Math.Max(bounds.End.X, testLoc.X+1), Math.Max(bounds.End.Y, testLoc.Y + 1)));

                        brush[testLoc] = brushTile.Copy();
                    },
                stroke.CoveredRect.Start);

            DiagManager.Instance.DevEditor.GroundEditor.Edits.Apply(new DrawGroundTexUndo(Layers.ChosenLayer, brush, bounds));
        }
    }

    public class DrawGroundTexUndo : DrawUndo<AutoTile>
    {
        private int layer;
        private Rect coveredRect;

        public DrawGroundTexUndo(int layer, Dictionary<Loc, AutoTile> brush, Rect coveredRect) : base(brush)
        {
            this.layer = layer;
            this.coveredRect = coveredRect;
        }

        protected override AutoTile GetValue(Loc loc)
        {
            return ZoneManager.Instance.CurrentGround.Layers[layer].Tiles[loc.X][loc.Y];
        }
        protected override void SetValue(Loc loc, AutoTile val)
        {
            ZoneManager.Instance.CurrentGround.Layers[layer].Tiles[loc.X][loc.Y] = val;
        }
        protected override void ValuesFinished()
        {
            //now recompute all tiles within the multiselect rectangle + 1
            Rect bounds = coveredRect;
            bounds.Inflate(1, 1);
            ZoneManager.Instance.CurrentGround.Layers[layer].CalculateAutotiles(ZoneManager.Instance.CurrentGround.Rand.FirstSeed, bounds.Start, bounds.Size, ZoneManager.Instance.CurrentGround.EdgeView == Map.ScrollEdge.Wrap);
        }
    }


    public class GroundTextureStateUndo : StateUndo<MapLayer>
    {
        private int layer;
        public GroundTextureStateUndo(int layer)
        {
            this.layer = layer;
        }

        public override MapLayer GetState()
        {
            return ZoneManager.Instance.CurrentGround.Layers[layer];
        }

        public override void SetState(MapLayer state)
        {
            ZoneManager.Instance.CurrentGround.Layers[layer] = state;
        }
    }
}
