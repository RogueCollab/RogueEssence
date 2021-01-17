using System;
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
using RogueEssence.Dev.Models;
using System.Diagnostics;

namespace RogueEssence.Dev.ViewModels
{
    public class GroundEditViewModel : ViewModelBase
    {
        public GroundEditViewModel()
        {
            Textures = new GroundTabTexturesViewModel();
            Decorations = new GroundTabDecorationsViewModel();
            Walls = new GroundTabWallsViewModel();
            Entities = new GroundTabEntitiesViewModel();
            Properties = new GroundTabPropertiesViewModel();
            Strings = new GroundTabStringsViewModel();
            CurrentFile = "";


            ScriptItems = new ObservableCollection<ScriptItem>();
            foreach (LuaEngine.EMapCallbacks v in LuaEngine.EnumerateCallbackTypes())
                ScriptItems.Add(new ScriptItem(v.ToString(), false));

        }

        public GroundTabTexturesViewModel Textures { get; set; }
        public GroundTabDecorationsViewModel Decorations { get; set; }
        public GroundTabWallsViewModel Walls { get; set; }
        public GroundTabEntitiesViewModel Entities { get; set; }
        public GroundTabPropertiesViewModel Properties { get; set; }
        public GroundTabStringsViewModel Strings { get; set; }

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


        public void New_Click()
        {

            //Check all callbacks by default
            for (int ii = 0; ii < ScriptItems.Count; ii++)
                ScriptItems[ii].IsChecked = true;

            CurrentFile = "";

            lock (GameBase.lockObj) //Schedule the map creation
                DoNew();
        }

        public async void Open_Click()
        {
            string mapDir = PathMod.ModPath(DataManager.GROUND_PATH);
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Directory = mapDir;

            FileDialogFilter filter = new FileDialogFilter();
            filter.Name = "Ground Files";
            filter.Extensions.Add(DataManager.GROUND_EXT.Substring(1));
            openFileDialog.Filters.Add(filter);

            DevForm form = (DevForm)DiagManager.Instance.DevEditor;

            string[] results = await openFileDialog.ShowAsync(form.GroundEditForm);

            if (results.Length > 0)
            {
                bool legalPath = false;
                foreach (string proposedPath in PathMod.FallbackPaths(DataManager.GROUND_PATH))
                {
                    if (comparePaths(proposedPath, Path.GetDirectoryName(results[0])))
                        legalPath = true;
                }
                if (!legalPath)
                    await MessageBox.Show(form.GroundEditForm, String.Format("Map can only be loaded from:\n{0}\nOr one of its parents.", PathMod.ModPath(DataManager.GROUND_PATH)), "Error", MessageBox.MessageBoxButtons.Ok);
                else
                {
                    lock (GameBase.lockObj)
                        DoLoad(Path.GetFileNameWithoutExtension(results[0]));
                }
            }
        }

        public void Save_Click()
        {
            if (CurrentFile == "")
                SaveAs_Click(); //Since its the same thing, might as well re-use the function! It makes everyone's lives easier!
            else
            {
                lock (GameBase.lockObj)
                    DoSave(ZoneManager.Instance.CurrentGround, CurrentFile, CurrentFile);
            }
        }
        public async void SaveAs_Click()
        {
            string mapDir = PathMod.ModPath(DataManager.GROUND_PATH);
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Directory = mapDir;

            FileDialogFilter filter = new FileDialogFilter();
            filter.Name = "Ground Files";
            filter.Extensions.Add(DataManager.GROUND_EXT.Substring(1));
            saveFileDialog.Filters.Add(filter);

            DevForm form = (DevForm)DiagManager.Instance.DevEditor;

            string result = await saveFileDialog.ShowAsync(form.GroundEditForm);

            if (result != null)
            {
                if (!comparePaths(PathMod.ModPath(DataManager.GROUND_PATH), Path.GetDirectoryName(result)))
                    await MessageBox.Show(form.GroundEditForm, String.Format("Map can only be saved to:\n{0}", Directory.GetCurrentDirectory() + "/" + DataManager.GROUND_PATH), "Error", MessageBox.MessageBoxButtons.Ok);
                else
                {
                    lock (GameBase.lockObj)
                    {
                        string oldFilename = CurrentFile;
                        ZoneManager.Instance.CurrentGround.AssetName = Path.GetFileNameWithoutExtension(result); //Set the assetname to the file name!

                        //Schedule saving the map
                        DoSave(ZoneManager.Instance.CurrentGround, result, oldFilename);
                    }
                }
            }
        }

        public async void ImportFromPng_Click()
        {
            string mapDir = PathMod.ModPath(DataManager.GROUND_PATH);
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Directory = mapDir;

            FileDialogFilter filter = new FileDialogFilter();
            filter.Name = "PNG Files";
            filter.Extensions.Add("png");
            openFileDialog.Filters.Add(filter);

            DevForm form = (DevForm)DiagManager.Instance.DevEditor;

            string[] results = await openFileDialog.ShowAsync(form.GroundEditForm);

            lock (GameBase.lockObj)
            {
                if (results.Length > 0)
                    DoImportPng(results[0]);
            }
        }


        public async void ImportFromTileset_Click()
        {
            DevForm form = (DevForm)DiagManager.Instance.DevEditor;

            if (Textures.TileBrowser.CurrentTileset == "")
                await MessageBox.Show(form.GroundEditForm, String.Format("No tileset to import!"), "Error", MessageBox.MessageBoxButtons.Ok);
            else
            {
                lock (GameBase.lockObj)
                    DoImportTileset(Textures.TileBrowser.CurrentTileset);
            }
        }


        public async void ReSize_Click()
        {

            MapResizeWindow window = new MapResizeWindow();
            MapResizeViewModel viewModel = new MapResizeViewModel(ZoneManager.Instance.CurrentGround.Width, ZoneManager.Instance.CurrentGround.Height);
            window.DataContext = viewModel;

            DevForm form = (DevForm)DiagManager.Instance.DevEditor;

            bool result = await window.ShowDialog<bool>(form.GroundEditForm);

            lock (GameBase.lockObj)
            {
                if (result)
                {
                    DiagManager.Instance.LoadMsg = "Resizing Map...";
                    DevForm.EnterLoadPhase(GameBase.LoadPhase.Content);

                    ZoneManager.Instance.CurrentGround.ResizeJustified(viewModel.MapWidth, viewModel.MapHeight, viewModel.ResizeDir);

                    DevForm.EnterLoadPhase(GameBase.LoadPhase.Ready);
                }
            }
        }

        public async void ReTile_Click()
        {
            MapRetileWindow window = new MapRetileWindow();
            MapRetileViewModel viewModel = new MapRetileViewModel(ZoneManager.Instance.CurrentGround.TileSize);
            window.DataContext = viewModel;

            DevForm form = (DevForm)DiagManager.Instance.DevEditor;

            bool result = await window.ShowDialog<bool>(form.GroundEditForm);

            lock (GameBase.lockObj)
            {
                if (result)
                {
                    DiagManager.Instance.LoadMsg = "Retiling Map...";
                    DevForm.EnterLoadPhase(GameBase.LoadPhase.Content);

                    ZoneManager.Instance.CurrentGround.Retile(viewModel.TileSize / GraphicsManager.TEX_SIZE);

                    Textures.TileBrowser.TileSize = Textures.TileBrowser.TileSize;

                    DevForm.EnterLoadPhase(GameBase.LoadPhase.Ready);
                }
            }
        }

        //public void Undo_Click()
        //{

        //}

        //public void Redo_Click()
        //{

        //}

        private void DoNew()
        {
            //take all the necessary steps before and after moving to the map

            DiagManager.Instance.LoadMsg = "Loading Map...";
            DevForm.EnterLoadPhase(GameBase.LoadPhase.Content);
            GameManager.Instance.ForceReady();

            ZoneManager.Instance.CurrentZone.DevNewGround();

            loadEditorSettings();
            DevForm.EnterLoadPhase(GameBase.LoadPhase.Ready);
        }
        private void DoLoad(string mapName)
        {
            //take all the necessary steps before and after moving to the map

            DiagManager.Instance.LoadMsg = "Loading Map...";
            DevForm.EnterLoadPhase(GameBase.LoadPhase.Content);
            GameManager.Instance.ForceReady();

            ZoneManager.Instance.CurrentZone.DevLoadGround(mapName);

            CurrentFile = PathMod.ModPath(Path.Combine(DataManager.GROUND_PATH, mapName + DataManager.GROUND_EXT));
            loadEditorSettings();
            DevForm.EnterLoadPhase(GameBase.LoadPhase.Ready);

        }

        public void LoadFromCurrentGround()
        {
            if (ZoneManager.Instance.CurrentGround.AssetName != "")
                CurrentFile = PathMod.ModPath(Path.Combine(DataManager.GROUND_PATH, ZoneManager.Instance.CurrentGround.AssetName + DataManager.GROUND_EXT));
            else
                CurrentFile = "";

            loadEditorSettings();
        }

        private void loadEditorSettings()
        {
            Textures.Layers.LoadLayers();
            Textures.TileBrowser.TileSize = Textures.TileBrowser.TileSize;
            Decorations.Layers.LoadLayers();

            Walls.SetupLayerVisibility();
            Properties.LoadMapProperties();
            LoadScriptData();
            Strings.LoadStrings();
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
                ImportHelper.SaveTileSheet(tileList, outputFile, ZoneManager.Instance.CurrentGround.TileSize);
            }

            GraphicsManager.RebuildIndices(GraphicsManager.AssetType.Tile);
            GraphicsManager.ClearCaches(GraphicsManager.AssetType.Tile);
            DevGraphicsManager.ClearCaches();

            Textures.TileBrowser.UpdateTilesList();
            Textures.TileBrowser.SelectTileset(sheetName);
        }

        private void DoImportTileset(string sheetName)
        {
            Loc newSize = GraphicsManager.TileIndex.GetTileDims(sheetName);

            DiagManager.Instance.LoadMsg = "Loading Map...";
            DevForm.EnterLoadPhase(GameBase.LoadPhase.Content);

            ZoneManager.Instance.CurrentGround.ResizeJustified(newSize.X, newSize.Y, Dir8.UpLeft);

            //set tilesets
            for (int yy = 0; yy < newSize.Y; yy++)
            {
                for (int xx = 0; xx < newSize.X; xx++)
                    ZoneManager.Instance.CurrentGround.Layers[Textures.Layers.ChosenLayer].Tiles[xx][yy] = new AutoTile(new TileLayer(new Loc(xx, yy), sheetName));
            }

            DevForm.EnterLoadPhase(GameBase.LoadPhase.Ready);
        }

        private void DoSave(GroundMap curgrnd, string filepath, string oldfname)
        {
            DataManager.SaveData(filepath, curgrnd);

            //Actually create the script folder, and default script file.
            createOrCopyScriptData(oldfname, filepath);
            //create or update the strings
            Strings.SaveStrings();

            CurrentFile = filepath;
        }


        private static bool comparePaths(string path1, string path2)
        {
            return String.Compare(Path.GetFullPath(path1).TrimEnd('\\'),
                Path.GetFullPath(path2).TrimEnd('\\'),
                StringComparison.InvariantCultureIgnoreCase) == 0;
        }

        /// <summary>
        /// Call this when saving as, so that if the previous map name has a script folder with data in it,
        /// we can copy it. And if it doesn't, we create a "blank slate" one!
        /// </summary>
        /// <param name="oldfilepath"></param>
        /// <param name="newfilepath"></param>
        private void createOrCopyScriptData(string oldfilepath, string newfilepath)
        {
            string oldmapscriptdir = LuaEngine.Instance._MakeMapScriptPath(Path.GetFileNameWithoutExtension(oldfilepath));
            string newmapscriptdir = LuaEngine.Instance._MakeMapScriptPath(Path.GetFileNameWithoutExtension(newfilepath));

            //Check if we have anything to copy at all!
            if (oldfilepath != newfilepath && !String.IsNullOrEmpty(oldfilepath) && Directory.Exists(oldfilepath))
            {
                Directory.CreateDirectory(newmapscriptdir);
                foreach (string f in Directory.GetFiles(oldmapscriptdir, "*.*", SearchOption.AllDirectories)) //This lists all subfiles recursively
                {
                    //Path to the sub-directory within the script folder containing this file
                    string subdirpath = f.Remove(oldmapscriptdir.Count()); //Not Count - 1 because of the last path separator!
                    //Path to the sub-directory within the new script folder where we'll copy this file!
                    string destpath = Path.Combine(newmapscriptdir, subdirpath);

                    //Ensure all subdirectories are created recursively, if there are any!
                    Directory.CreateDirectory(Path.GetDirectoryName(destpath));

                    //Copy the file itself
                    File.Copy(f, destpath, false);
                }
            }
            else
            {
                //We just create a new one straight away!
                LuaEngine.Instance.CreateNewMapScriptDir(Path.GetFileNameWithoutExtension(newfilepath));
            }
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
                    case 1://Decorations
                        Decorations.ProcessInput(input);
                        break;
                    case 2://Walls
                        Walls.ProcessInput(input);
                        break;
                    case 3://Entities
                        Entities.ProcessInput(input);
                        break;
                }
            }
        }







        public ObservableCollection<ScriptItem> ScriptItems { get; }

        public void btnOpenScriptDir_Click()
        {
            lock (GameBase.lockObj)
            {
                string mapscriptdir = LuaEngine.Instance._MakeMapScriptPath(Path.GetFileNameWithoutExtension(CurrentFile));
                mapscriptdir = Path.GetFullPath(mapscriptdir);
                Process.Start("explorer.exe", mapscriptdir);
            }
        }
        public void btnReloadScripts_Click()
        {
            lock (GameBase.lockObj)
            {
                LuaEngine.Instance.Reset();
                LuaEngine.Instance.ReInit();
            }
        }

        private void LoadScriptData()
        {
            lock (GameBase.lockObj)
            {
                //Setup callback display without triggering events
                var scev = ZoneManager.Instance.CurrentGround.ActiveScriptEvent();
                foreach (LuaEngine.EMapCallbacks s in scev)
                {
                    ScriptItems[(int)s].IsChecked = true;
                }
            }
        }



    }
}
