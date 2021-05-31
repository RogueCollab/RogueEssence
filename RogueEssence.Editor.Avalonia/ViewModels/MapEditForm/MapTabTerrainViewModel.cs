using ReactiveUI;
using RogueElements;
using RogueEssence.Content;
using RogueEssence.Data;
using RogueEssence.Dungeon;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RogueEssence.Dev.ViewModels
{
    public class MapTabTerrainViewModel : ViewModelBase
    {
        public MapTabTerrainViewModel()
        {
            TileBrowser = new TileBrowserViewModel();
            TileBrowser.CanMultiSelect = true;
            AutotileBrowser = new AutotileBrowserViewModel();

            TerrainTypes = new ObservableCollection<string>();
            string[] terrain_names = DataManager.Instance.DataIndices[DataManager.DataType.Terrain].GetLocalStringArray();
            for (int ii = 0; ii < terrain_names.Length; ii++)
                TerrainTypes.Add(ii.ToString("D3") + ": " + terrain_names[ii]);
        }

        private TileEditMode terrainMode;
        public TileEditMode TerrainMode
        {
            get { return terrainMode; }
            set { this.SetIfChanged(ref terrainMode, value); }
        }

        public bool ShowTerrain
        {
            get { return DungeonEditScene.Instance.ShowTerrain; }
            set { this.RaiseAndSet(ref DungeonEditScene.Instance.ShowTerrain, value); }
        }


        public ObservableCollection<string> TerrainTypes { get; }

        private int chosenTerrain;
        public int ChosenTerrain
        {
            get { return chosenTerrain; }
            set { this.RaiseAndSet(ref chosenTerrain, value); }
        }


        public TileBrowserViewModel TileBrowser { get; set; }
        public AutotileBrowserViewModel AutotileBrowser { get; set; }



        private int tabIndex;
        public int TabIndex
        {
            get => tabIndex;
            set { this.SetIfChanged(ref tabIndex, value); }
        }


        public void SetupLayerVisibility()
        {
            ShowTerrain = ShowTerrain;
        }

        public void ProcessInput(InputManager input)
        {
            bool inWindow = Collision.InBounds(GraphicsManager.WindowWidth, GraphicsManager.WindowHeight, input.MouseLoc);

            Loc tileCoords = DungeonEditScene.Instance.ScreenCoordsToMapCoords(input.MouseLoc);
            switch (TerrainMode)
            {
                case TileEditMode.Draw:
                    {
                        TileBrush brush = getBrush();
                        if (brush.MultiSelect == Loc.One)
                        {
                            CanvasStroke<TerrainTile>.ProcessCanvasInput(input, tileCoords, inWindow,
                                () => new DrawStroke<TerrainTile>(tileCoords, new TerrainTile(ChosenTerrain, getBrush().GetSanitizedTile())),
                                () => new DrawStroke<TerrainTile>(tileCoords, new TerrainTile(0, new AutoTile())),
                                paintStroke, ref DungeonEditScene.Instance.TerrainInProgress);
                        }
                        else
                        {
                            CanvasStroke<TerrainTile>.ProcessCanvasInput(input, tileCoords, inWindow,
                                () => new ClusterStroke<TerrainTile>(tileCoords, getCluster(brush)),
                                () => new DrawStroke<TerrainTile>(tileCoords, new TerrainTile(0, new AutoTile())),
                                paintStroke, ref DungeonEditScene.Instance.TerrainInProgress);
                        }
                    }
                    break;
                case TileEditMode.Rectangle:
                    {
                        CanvasStroke<TerrainTile>.ProcessCanvasInput(input, tileCoords, inWindow,
                            () => new RectStroke<TerrainTile>(tileCoords, new TerrainTile(ChosenTerrain, getBrush().GetSanitizedTile())),
                            () => new RectStroke<TerrainTile>(tileCoords, new TerrainTile(0, new AutoTile())),
                            paintStroke, ref DungeonEditScene.Instance.TerrainInProgress);
                    }
                    break;
                case TileEditMode.Fill:
                    {
                        CanvasStroke<TerrainTile>.ProcessCanvasInput(input, tileCoords, inWindow,
                            () => new FillStroke<TerrainTile>(tileCoords, new TerrainTile(ChosenTerrain, getBrush().GetSanitizedTile())),
                            () => new FillStroke<TerrainTile>(tileCoords, new TerrainTile(0, new AutoTile())),
                            fillStroke, ref DungeonEditScene.Instance.TerrainInProgress);
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

        private TerrainTile[][] getCluster(TileBrush brush)
        {
            TerrainTile[][] tiles = new TerrainTile[brush.MultiSelect.X][];
            for (int xx = 0; xx < brush.MultiSelect.X; xx++)
            {
                tiles[xx] = new TerrainTile[brush.MultiSelect.Y];
                for (int yy = 0; yy < brush.MultiSelect.Y; yy++)
                    tiles[xx][yy] = new TerrainTile(ChosenTerrain, brush.GetSanitizedTile(new Loc(xx, yy)));
            }
            return tiles;
        }

        private void paintStroke(CanvasStroke<TerrainTile> stroke)
        {
            Dictionary<Loc, TerrainTile> brush = new Dictionary<Loc, TerrainTile>();
            foreach (Loc loc in stroke.GetLocs())
            {
                if (!Collision.InBounds(ZoneManager.Instance.CurrentMap.Width, ZoneManager.Instance.CurrentMap.Height, loc))
                    continue;

                brush[loc] = stroke.GetBrush(loc).Copy();
            }

            DiagManager.Instance.DevEditor.MapEditor.Edits.Apply(new DrawTerrainUndo(brush, stroke.CoveredRect));
        }

        private void eyedropTile(Loc loc)
        {
            if (!Collision.InBounds(ZoneManager.Instance.CurrentMap.Width, ZoneManager.Instance.CurrentMap.Height, loc))
                return;

            TerrainTile terrainTile = ZoneManager.Instance.CurrentMap.Tiles[loc.X][loc.Y].Data;

            ChosenTerrain = terrainTile.ID;
            if (terrainTile.TileTex.AutoTileset > -1)
            {
                //switch to autotile tab
                AutotileBrowser.SetBrush(terrainTile.TileTex);
            }
            else
            {
                TileLayer layer = (terrainTile.TileTex.Layers.Count > 0) ? terrainTile.TileTex.Layers[0] : new TileLayer();
                TileBrowser.SetBrush(layer);
            }
        }


        private void fillStroke(CanvasStroke<TerrainTile> stroke)
        {
            if (!Collision.InBounds(ZoneManager.Instance.CurrentMap.Width, ZoneManager.Instance.CurrentMap.Height, stroke.CoveredRect.Start))
                return;

            TerrainTile tile = ZoneManager.Instance.CurrentMap.Tiles[stroke.CoveredRect.Start.X][stroke.CoveredRect.Start.Y].Data.Copy();
            Rect bounds = new Rect(stroke.CoveredRect.Start, Loc.One);

            Dictionary<Loc, TerrainTile> brush = new Dictionary<Loc, TerrainTile>();
            TerrainTile brushTile = stroke.GetBrush(stroke.CoveredRect.Start);
            Grid.FloodFill(new Rect(0, 0, ZoneManager.Instance.CurrentMap.Width, ZoneManager.Instance.CurrentMap.Height),
                    (Loc testLoc) =>
                    {
                        if (brush.ContainsKey(testLoc))
                            return true;
                        return !tile.Equals(ZoneManager.Instance.CurrentMap.Tiles[testLoc.X][testLoc.Y].Data);
                    },
                    (Loc testLoc) =>
                    {
                        return true;
                    },
                    (Loc testLoc) =>
                    {
                        bounds = Rect.FromPoints(new Loc(Math.Min(bounds.X, testLoc.X), Math.Min(bounds.Y, testLoc.Y)),
                            new Loc(Math.Max(bounds.End.X, testLoc.X + 1), Math.Max(bounds.End.Y, testLoc.Y + 1)));
                        brush[testLoc] = brushTile.Copy();
                    },
                stroke.CoveredRect.Start);

            DiagManager.Instance.DevEditor.MapEditor.Edits.Apply(new DrawTerrainUndo(brush, bounds));
        }
    }

    public class DrawTerrainUndo : DrawUndo<TerrainTile>
    {
        private Rect coveredRect;

        public DrawTerrainUndo(Dictionary<Loc, TerrainTile> brush, Rect coveredRect) : base(brush)
        {
            this.coveredRect = coveredRect;
        }

        protected override TerrainTile GetValue(Loc loc)
        {
            return ZoneManager.Instance.CurrentMap.Tiles[loc.X][loc.Y].Data;
        }
        protected override void SetValue(Loc loc, TerrainTile val)
        {
            ZoneManager.Instance.CurrentMap.Tiles[loc.X][loc.Y].Data = val;
        }
        protected override void ValuesFinished()
        {
            //now recompute all tiles within the multiselect rectangle + 1
            Rect bounds = coveredRect;
            bounds.Inflate(1, 1);
            ZoneManager.Instance.CurrentMap.CalculateAutotiles(bounds.Start, bounds.Size);
        }
    }
}
