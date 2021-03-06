﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using RogueEssence;
using RogueEssence.Dungeon;
using RogueEssence.Ground;
using RogueEssence.Data;
using Avalonia.Controls;
using System.IO;
using RogueEssence.Dev.Views;
using RogueEssence.Content;
using RogueElements;
using RogueEssence.Script;
using System.Linq;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace RogueEssence.Dev.ViewModels
{
    public class MapEditViewModel : ViewModelBase
    {
        public MapEditViewModel()
        {
            Textures = new MapTabTexturesViewModel();
            Terrain = new MapTabTerrainViewModel();
            Tiles = new MapTabTilesViewModel();
            Items = new MapTabItemsViewModel();
            Entities = new MapTabEntitiesViewModel();
            Entrances = new MapTabEntrancesViewModel();
            Spawns = new MapTabSpawnsViewModel();
            Effects = new MapTabEffectsViewModel();
            Properties = new MapTabPropertiesViewModel();
            CurrentFile = "";
        }

        public MapTabTexturesViewModel Textures { get; set; }
        public MapTabTerrainViewModel Terrain { get; set; }
        public MapTabTilesViewModel Tiles { get; set; }
        public MapTabItemsViewModel Items { get; set; }
        public MapTabEntitiesViewModel Entities { get; set; }
        public MapTabEntrancesViewModel Entrances { get; set; }
        public MapTabSpawnsViewModel Spawns { get; set; }
        public MapTabEffectsViewModel Effects { get; set; }
        public MapTabPropertiesViewModel Properties { get; set; }

        private string currentFile;
        public string CurrentFile
        {
            get => currentFile;
            set => this.SetIfChanged(ref currentFile, value);
        }

        private int selectedTabIndex;
        public int SelectedTabIndex
        {
            get => selectedTabIndex;
            set => this.SetIfChanged(ref selectedTabIndex, value);
        }


        public void mnuNew_Click()
        {
            CurrentFile = "";

            lock (GameBase.lockObj) //Schedule the map creation
                DoNew();
        }

        public async void mnuOpen_Click()
        {
            string mapDir = PathMod.ModPath(DataManager.MAP_PATH);
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Directory = mapDir;

            FileDialogFilter filter = new FileDialogFilter();
            filter.Name = "Map Files";
            filter.Extensions.Add(DataManager.MAP_EXT.Substring(1));
            openFileDialog.Filters.Add(filter);

            DevForm form = (DevForm)DiagManager.Instance.DevEditor;

            string[] results = await openFileDialog.ShowAsync(form.MapEditForm);

            if (results.Length > 0)
            {
                bool legalPath = false;
                foreach (string proposedPath in PathMod.FallbackPaths(DataManager.MAP_PATH))
                {
                    if (comparePaths(proposedPath, Path.GetDirectoryName(results[0])))
                        legalPath = true;
                }
                if (!legalPath)
                    await MessageBox.Show(form.MapEditForm, String.Format("Map can only be loaded from:\n{0}\nOr one of its parents.", PathMod.ModPath(DataManager.MAP_PATH)), "Error", MessageBox.MessageBoxButtons.Ok);
                else
                {
                    lock (GameBase.lockObj)
                        DoLoad(Path.GetFileNameWithoutExtension(results[0]));
                }
            }
        }

        public void mnuSave_Click()
        {
            if (CurrentFile == "")
                mnuSaveAs_Click(); //Since its the same thing, might as well re-use the function! It makes everyone's lives easier!
            else
            {
                lock (GameBase.lockObj)
                    DoSave(ZoneManager.Instance.CurrentMap, CurrentFile, CurrentFile);
            }
        }
        public async void mnuSaveAs_Click()
        {
            string mapDir = PathMod.ModPath(DataManager.MAP_PATH);
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Directory = mapDir;

            FileDialogFilter filter = new FileDialogFilter();
            filter.Name = "Map Files";
            filter.Extensions.Add(DataManager.MAP_EXT.Substring(1));
            saveFileDialog.Filters.Add(filter);

            DevForm form = (DevForm)DiagManager.Instance.DevEditor;

            string result = await saveFileDialog.ShowAsync(form.MapEditForm);

            if (result != null)
            {
                if (!comparePaths(PathMod.ModPath(DataManager.MAP_PATH), Path.GetDirectoryName(result)))
                    await MessageBox.Show(form.MapEditForm, String.Format("Map can only be saved to:\n{0}", Directory.GetCurrentDirectory() + "/" + DataManager.MAP_PATH), "Error", MessageBox.MessageBoxButtons.Ok);
                else
                {
                    lock (GameBase.lockObj)
                    {
                        string oldFilename = CurrentFile;
                        ZoneManager.Instance.CurrentMap.AssetName = Path.GetFileNameWithoutExtension(result); //Set the assetname to the file name!

                        //Schedule saving the map
                        DoSave(ZoneManager.Instance.CurrentMap, result, oldFilename);
                    }
                }
            }
        }

        public async void mnuImportFromPng_Click()
        {
            string mapDir = PathMod.ModPath(DataManager.MAP_PATH);
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Directory = mapDir;

            FileDialogFilter filter = new FileDialogFilter();
            filter.Name = "PNG Files";
            filter.Extensions.Add("png");
            openFileDialog.Filters.Add(filter);

            DevForm form = (DevForm)DiagManager.Instance.DevEditor;

            string[] results = await openFileDialog.ShowAsync(form.MapEditForm);

            lock (GameBase.lockObj)
            {
                if (results.Length > 0)
                    DoImportPng(results[0]);
            }
        }


        public async void mnuImportFromTileset_Click()
        {
            DevForm form = (DevForm)DiagManager.Instance.DevEditor;

            if (Textures.TileBrowser.CurrentTileset == "")
                await MessageBox.Show(form.MapEditForm, String.Format("No tileset to import!"), "Error", MessageBox.MessageBoxButtons.Ok);
            else
            {
                lock (GameBase.lockObj)
                    DoImportTileset(Textures.TileBrowser.CurrentTileset);
            }
        }


        public async void mnuReSize_Click()
        {
            MapResizeWindow window = new MapResizeWindow();
            MapResizeViewModel viewModel = new MapResizeViewModel(ZoneManager.Instance.CurrentMap.Width, ZoneManager.Instance.CurrentMap.Height);
            window.DataContext = viewModel;

            DevForm form = (DevForm)DiagManager.Instance.DevEditor;

            bool result = await window.ShowDialog<bool>(form.MapEditForm);

            lock (GameBase.lockObj)
            {
                if (result)
                {
                    DiagManager.Instance.LoadMsg = "Resizing Map...";
                    DevForm.EnterLoadPhase(GameBase.LoadPhase.Content);

                    ZoneManager.Instance.CurrentMap.ResizeJustified(viewModel.MapWidth, viewModel.MapHeight, viewModel.ResizeDir);

                    DevForm.EnterLoadPhase(GameBase.LoadPhase.Ready);
                }
            }
        }

        //public void mnuUndo_Click()
        //{

        //}

        //public void mnuRedo_Click()
        //{

        //}

        private void DoNew()
        {
            //take all the necessary steps before and after moving to the map

            DiagManager.Instance.LoadMsg = "Loading Map...";
            DevForm.EnterLoadPhase(GameBase.LoadPhase.Content);
            GameManager.Instance.ForceReady();

            ZoneManager.Instance.CurrentZone.DevNewMap();

            loadEditorSettings();
            DevForm.EnterLoadPhase(GameBase.LoadPhase.Ready);
        }
        private void DoLoad(string mapName)
        {
            //take all the necessary steps before and after moving to the map

            DiagManager.Instance.LoadMsg = "Loading Map...";
            DevForm.EnterLoadPhase(GameBase.LoadPhase.Content);
            GameManager.Instance.ForceReady();

            ZoneManager.Instance.CurrentZone.DevLoadMap(mapName);

            CurrentFile = PathMod.ModPath(Path.Combine(DataManager.MAP_PATH, mapName + DataManager.MAP_EXT));
            loadEditorSettings();
            DevForm.EnterLoadPhase(GameBase.LoadPhase.Ready);

        }

        public void LoadFromCurrentMap()
        {
            if (ZoneManager.Instance.CurrentMap.AssetName != "")
                CurrentFile = PathMod.ModPath(Path.Combine(DataManager.MAP_PATH, ZoneManager.Instance.CurrentMap.AssetName + DataManager.MAP_EXT));
            else
                CurrentFile = "";

            loadEditorSettings();
        }

        private void loadEditorSettings()
        {
            Textures.TileBrowser.TileSize = GraphicsManager.TileSize;
            Textures.AutotileBrowser.TileSize = GraphicsManager.TileSize;
            Terrain.TileBrowser.TileSize = GraphicsManager.TileSize;
            Terrain.AutotileBrowser.TileSize = GraphicsManager.TileSize;

            Terrain.SetupLayerVisibility();
            Entrances.SetupLayerVisibility();
            Spawns.LoadMapSpawns();
            Effects.LoadMapEffects();
            Properties.LoadMapProperties();
        }

        private void DoImportPng(string filePath)
        {
            string sheetName = Path.GetFileNameWithoutExtension(filePath);
            string outputFile = PathMod.HardMod(String.Format(GraphicsManager.TILE_PATTERN, sheetName));


            //load into tilesets
            using (BaseSheet tileset = BaseSheet.Import(filePath))
            {
                List<BaseSheet> tileList = new List<BaseSheet>();
                tileList.Add(tileset);
                ImportHelper.SaveTileSheet(tileList, outputFile, GraphicsManager.TileSize);
            }

            GraphicsManager.RebuildIndices(GraphicsManager.AssetType.Tile);
            GraphicsManager.ClearCaches(GraphicsManager.AssetType.Tile);
            DevGraphicsManager.ClearCaches();

            Textures.TileBrowser.UpdateTilesList();
            Textures.TileBrowser.SelectTileset(sheetName);
            Terrain.TileBrowser.UpdateTilesList();
            Terrain.TileBrowser.SelectTileset(sheetName);
        }

        private void DoImportTileset(string sheetName)
        {
            Loc newSize = GraphicsManager.TileIndex.GetTileDims(sheetName);

            DiagManager.Instance.LoadMsg = "Loading Map...";
            DevForm.EnterLoadPhase(GameBase.LoadPhase.Content);

            ZoneManager.Instance.CurrentMap.ResizeJustified(newSize.X, newSize.Y, Dir8.UpLeft);

            //set tilesets
            for (int yy = 0; yy < newSize.Y; yy++)
            {
                for (int xx = 0; xx < newSize.X; xx++)
                    ZoneManager.Instance.CurrentMap.Tiles[xx][yy].FloorTile = new AutoTile(new TileLayer(new Loc(xx, yy), sheetName));
            }

            DevForm.EnterLoadPhase(GameBase.LoadPhase.Ready);
        }

        private void DoSave(Map curmap, string filepath, string oldfname)
        {
            DataManager.SaveData(filepath, curmap);

            CurrentFile = filepath;
        }


        private static bool comparePaths(string path1, string path2)
        {
            return String.Compare(Path.GetFullPath(path1).TrimEnd('\\'),
                Path.GetFullPath(path2).TrimEnd('\\'),
                StringComparison.InvariantCultureIgnoreCase) == 0;
        }


        public void ProcessInput(InputManager input)
        {
            lock (GameBase.lockObj)
            {
                switch (selectedTabIndex)
                {
                    case 0://Textures
                        Textures.ProcessInput(input);
                        break;
                    case 1://Terrain
                        Terrain.ProcessInput(input);
                        break;
                    case 2://Tiles
                        Tiles.ProcessInput(input);
                        break;
                    case 3://Items
                        Items.ProcessInput(input);
                        break;
                    case 4://Entities
                        Entities.ProcessInput(input);
                        break;
                    case 5://Entrances
                        Entrances.ProcessInput(input);
                        break;
                }
            }
        }

    }
}
