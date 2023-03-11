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
using System.Diagnostics;
using System.Threading.Tasks;

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
            Script = new GroundTabScriptViewModel();
            CurrentFile = "";
        }

        public GroundTabTexturesViewModel Textures { get; set; }
        public GroundTabDecorationsViewModel Decorations { get; set; }
        public GroundTabWallsViewModel Walls { get; set; }
        public GroundTabEntitiesViewModel Entities { get; set; }
        public GroundTabPropertiesViewModel Properties { get; set; }
        public GroundTabStringsViewModel Strings { get; set; }
        public GroundTabScriptViewModel Script { get; set; }

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
            set
            {
                this.SetIfChanged(ref selectedTabIndex, value);
                TabChanged();
            }
        }


        public void mnuNew_Click()
        {
            CurrentFile = "";

            lock (GameBase.lockObj) //Schedule the map creation
                DoNew();
        }

        public async void mnuOpen_Click()
        {
            string mapDir = Path.GetFullPath(PathMod.ModPath(DataManager.GROUND_PATH));
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Directory = mapDir;

            FileDialogFilter filter = new FileDialogFilter();
            filter.Name = "Ground Files";
            filter.Extensions.Add(DataManager.GROUND_EXT.Substring(1));
            openFileDialog.Filters.Add(filter);

            DevForm form = (DevForm)DiagManager.Instance.DevEditor;

            string[] results = await openFileDialog.ShowAsync(form.GroundEditForm);

            if (results != null && results.Length > 0)
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

        public async Task<bool> mnuSave_Click()
        {
            if (CurrentFile == "")
                return await mnuSaveAs_Click(); //Since its the same thing, might as well re-use the function! It makes everyone's lives easier!
            else
            {
                string reqDir = PathMod.HardMod(DataManager.GROUND_PATH);
                string result = Path.Join(reqDir, Path.GetFileName(CurrentFile));
                lock (GameBase.lockObj)
                {
                    string oldFilename = CurrentFile;
                    DoSave(ZoneManager.Instance.CurrentGround, result, oldFilename);
                }
                return true;
            }
        }
        public async Task<bool> mnuSaveAs_Click()
        {
            string mapDir = Path.GetFullPath(PathMod.ModPath(DataManager.GROUND_PATH));
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Directory = mapDir;

            FileDialogFilter filter = new FileDialogFilter();
            filter.Name = "Ground Files";
            filter.Extensions.Add(DataManager.GROUND_EXT.Substring(1));
            saveFileDialog.Filters.Add(filter);

            DevForm form = (DevForm)DiagManager.Instance.DevEditor;

            string result = await saveFileDialog.ShowAsync(form.GroundEditForm);

            if (!String.IsNullOrEmpty(result))
            {
                string reqDir = PathMod.HardMod(DataManager.GROUND_PATH);
                if (!comparePaths(reqDir, Path.GetDirectoryName(result)))
                    await MessageBox.Show(form.GroundEditForm, String.Format("Map can only be saved to:\n{0}", reqDir), "Error", MessageBox.MessageBoxButtons.Ok);
                else if (Path.GetFileName(result).Contains(" "))
                    await MessageBox.Show(form.GroundEditForm, String.Format("Save file should not contain white space:\n{0}", Path.GetFileName(result)), "Error", MessageBox.MessageBoxButtons.Ok);
                else
                {
                    lock (GameBase.lockObj)
                    {
                        string oldFilename = CurrentFile;

                        //Schedule saving the map
                        DoSave(ZoneManager.Instance.CurrentGround, result, oldFilename);
                    }
                    return true;
                }
            }
            return false;
        }

        public async void mnuTest_Click()
        {
            bool saved = await mnuSave_Click();
            if (saved)
            {
                lock (GameBase.lockObj)
                {
                    DevForm form = (DevForm)DiagManager.Instance.DevEditor;
                    form.GroundEditForm.SilentClose();
                    form.GroundEditForm = null;
                    GameManager.Instance.SceneOutcome = GameManager.Instance.TestWarp(ZoneManager.Instance.CurrentGround.AssetName, true, MathUtils.Rand.NextUInt64());
                }
            }
        }

        public async void mnuImportFromPng_Click()
        {
            string mapDir = Path.GetFullPath(PathMod.ModPath(DataManager.GROUND_PATH));
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Directory = mapDir;

            FileDialogFilter filter = new FileDialogFilter();
            filter.Name = "PNG Files";
            filter.Extensions.Add("png");
            openFileDialog.Filters.Add(filter);

            DevForm form = (DevForm)DiagManager.Instance.DevEditor;

            string[] results = await openFileDialog.ShowAsync(form.GroundEditForm);

            if (results != null && results.Length > 0)
                DoImportPng(results[0]);
        }


        public void mnuClearLayer_Click()
        {
            lock (GameBase.lockObj)
                DoClearLayer();
        }


        public async void mnuImportFromTileset_Click()
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


        public async void mnuReSize_Click()
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
                    //TODO: support undo for this
                    DiagManager.Instance.DevEditor.GroundEditor.Edits.Clear();

                    DiagManager.Instance.LoadMsg = "Resizing Map...";
                    DevForm.EnterLoadPhase(GameBase.LoadPhase.Content);

                    ZoneManager.Instance.CurrentGround.ResizeJustified(viewModel.MapWidth, viewModel.MapHeight, viewModel.ResizeDir);

                    DevForm.EnterLoadPhase(GameBase.LoadPhase.Ready);
                }
            }
        }

        public async void mnuReTile_Click()
        {
            MapRetileWindow window = new MapRetileWindow();
            MapRetileViewModel viewModel = new MapRetileViewModel(ZoneManager.Instance.CurrentGround.TileSize, "Tile size must be divisible by 8. All textures will be erased from all layers upon completing this operation.");
            window.DataContext = viewModel;

            DevForm form = (DevForm)DiagManager.Instance.DevEditor;

            bool result = await window.ShowDialog<bool>(form.GroundEditForm);

            lock (GameBase.lockObj)
            {
                bool sizeChanged = viewModel.TileSize != ZoneManager.Instance.CurrentGround.TileSize;
                if (result && sizeChanged)
                {
                    //TODO: support undo for this
                    DiagManager.Instance.DevEditor.GroundEditor.Edits.Clear();

                    DiagManager.Instance.LoadMsg = "Retiling Map...";
                    DevForm.EnterLoadPhase(GameBase.LoadPhase.Content);

                    ZoneManager.Instance.CurrentGround.Retile(viewModel.TileSize / GraphicsManager.TEX_SIZE);

                    Textures.TileBrowser.TileSize = ZoneManager.Instance.CurrentGround.TileSize;
                    Textures.AutotileBrowser.TileSize = ZoneManager.Instance.CurrentGround.TileSize;
                    ZoneManager.Instance.CurrentGround.BlankBG = new AutoTile();
                    Properties.BlankBG.LoadFromSource(ZoneManager.Instance.CurrentGround.BlankBG);

                    DevForm.EnterLoadPhase(GameBase.LoadPhase.Ready);
                }
            }
        }

        public void mnuUndo_Click()
        {
            if (DiagManager.Instance.DevEditor.GroundEditor.Edits.CanUndo)
            {
                DiagManager.Instance.DevEditor.GroundEditor.Edits.Undo();
                ProcessUndo();
            }
        }

        public void mnuRedo_Click()
        {
            if (DiagManager.Instance.DevEditor.GroundEditor.Edits.CanRedo)
                DiagManager.Instance.DevEditor.GroundEditor.Edits.Redo();
        }

        private void DoNew()
        {
            DiagManager.Instance.DevEditor.GroundEditor.Edits.Clear();
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
            DiagManager.Instance.DevEditor.GroundEditor.Edits.Clear();
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
            Textures.TileBrowser.TileSize = ZoneManager.Instance.CurrentGround.TileSize;
            Textures.AutotileBrowser.TileSize = ZoneManager.Instance.CurrentGround.TileSize;
            Decorations.Layers.LoadLayers();
            Entities.Layers.LoadLayers();

            Walls.SetupLayerVisibility();
            Properties.LoadMapProperties();
            Script.LoadScripts();
            Strings.LoadStrings();
        }

        private void DoImportPng(string filePath)
        {
            DevForm.ExecuteOrPend(() => { tryImportPng(filePath); });

            string sheetName = Path.GetFileNameWithoutExtension(filePath);
            lock (GameBase.lockObj)
            {
                Textures.TileBrowser.UpdateTilesList();
            }
            Textures.TileBrowser.SelectTileset(sheetName);
        }

        private void tryImportPng(string filePath)
        {
            lock (GameBase.lockObj)
            {
                string sheetName = Path.GetFileNameWithoutExtension(filePath);
                string outputFile = PathMod.HardMod(String.Format(GraphicsManager.TILE_PATTERN, sheetName));

                //load into tilesets
                using (BaseSheet tileset = BaseSheet.Import(filePath))
                {
                    List<BaseSheet[]> tileList = new List<BaseSheet[]>();
                    tileList.Add(new BaseSheet[] { tileset });
                    ImportHelper.SaveTileSheet(tileList, outputFile, ZoneManager.Instance.CurrentGround.TileSize);
                }

                GraphicsManager.RebuildIndices(GraphicsManager.AssetType.Tile);
                GraphicsManager.ClearCaches(GraphicsManager.AssetType.Tile);
                DevDataManager.ClearCaches();
            }
        }

        private void DoClearLayer()
        {
            //TODO: support undo for this
            DiagManager.Instance.DevEditor.GroundEditor.Edits.Clear();

            DiagManager.Instance.LoadMsg = "Loading Map...";
            DevForm.EnterLoadPhase(GameBase.LoadPhase.Content);

            //set tilesets
            for (int yy = 0; yy < ZoneManager.Instance.CurrentGround.Height; yy++)
            {
                for (int xx = 0; xx < ZoneManager.Instance.CurrentGround.Width; xx++)
                    ZoneManager.Instance.CurrentGround.Layers[Textures.Layers.ChosenLayer].Tiles[xx][yy] = new AutoTile();
            }

            DevForm.EnterLoadPhase(GameBase.LoadPhase.Ready);
        }

        private void DoImportTileset(string sheetName)
        {
            //TODO: support undo for this
            DiagManager.Instance.DevEditor.GroundEditor.Edits.Clear();

            Loc newSize = GraphicsManager.TileIndex.GetTileDims(sheetName);

            DiagManager.Instance.LoadMsg = "Loading Map...";
            DevForm.EnterLoadPhase(GameBase.LoadPhase.Content);

            ZoneManager.Instance.CurrentGround.ResizeJustified(Math.Max(newSize.X, ZoneManager.Instance.CurrentGround.Width), Math.Max(newSize.Y, ZoneManager.Instance.CurrentGround.Height), Dir8.UpLeft);

            //set tilesets
            for (int yy = 0; yy < newSize.Y; yy++)
            {
                for (int xx = 0; xx < newSize.X; xx++)
                {
                    AutoTile tile = new AutoTile();
                    TileFrame newFrame = new TileFrame(new Loc(xx, yy), sheetName);
                    //check for emptiness
                    long tilePos = GraphicsManager.TileIndex.GetPosition(newFrame.Sheet, newFrame.TexLoc);
                    if (tilePos > 0)
                        tile.Layers.Add(new TileLayer(newFrame));

                    ZoneManager.Instance.CurrentGround.Layers[Textures.Layers.ChosenLayer].Tiles[xx][yy] = tile;
                }
            }

            DevForm.EnterLoadPhase(GameBase.LoadPhase.Ready);
        }

        //TODO: standardize adding of frames
        private void DoImportTilesetToFrames(string sheetName)
        {
            //TODO: support undo for this
            DiagManager.Instance.DevEditor.GroundEditor.Edits.Clear();

            Loc newSize = GraphicsManager.TileIndex.GetTileDims(sheetName);

            DiagManager.Instance.LoadMsg = "Loading Map...";
            DevForm.EnterLoadPhase(GameBase.LoadPhase.Content);

            ZoneManager.Instance.CurrentGround.ResizeJustified(Math.Max(newSize.X, ZoneManager.Instance.CurrentGround.Width), Math.Max(newSize.Y, ZoneManager.Instance.CurrentGround.Height), Dir8.UpLeft);

            //count highest frames
            int maxFrames = 0;
            for (int yy = 0; yy < newSize.Y; yy++)
            {
                for (int xx = 0; xx < newSize.X; xx++)
                {
                    AutoTile tile = ZoneManager.Instance.CurrentGround.Layers[Textures.Layers.ChosenLayer].Tiles[xx][yy];
                    if (tile.Layers.Count > 0)
                        maxFrames = Math.Max(tile.Layers[0].Frames.Count, maxFrames);
                }
            }

            //set tilesets
            for (int yy = 0; yy < newSize.Y; yy++)
            {
                for (int xx = 0; xx < newSize.X; xx++)
                {
                    AutoTile tile = ZoneManager.Instance.CurrentGround.Layers[Textures.Layers.ChosenLayer].Tiles[xx][yy];
                    TileFrame newFrame = new TileFrame(new Loc(xx, yy), sheetName);
                    //check for emptiness
                    long tilePos = GraphicsManager.TileIndex.GetPosition(newFrame.Sheet, newFrame.TexLoc);
                    if (tilePos > 0)
                    {
                        if (tile.Layers.Count == 0)
                            tile.Layers.Add(new TileLayer(60));
                        while (tile.Layers[0].Frames.Count < maxFrames)
                            tile.Layers[0].Frames.Add(TileFrame.Empty);
                        tile.Layers[0].Frames.Add(newFrame);
                    }
                }
            }

            DevForm.EnterLoadPhase(GameBase.LoadPhase.Ready);
        }

        private void DoSave(GroundMap curgrnd, string filepath, string oldfname)
        {
            ZoneManager.Instance.CurrentGround.AssetName = Path.GetFileNameWithoutExtension(filepath); //Set the assetname to the file name!
            DataManager.SaveData(filepath, curgrnd);

            //Actually create the script folder, and default script file.
            createOrCopyScriptData(oldfname, filepath);
            //create or update the strings
            Strings.SaveStrings();

            CurrentFile = filepath;
        }


        private static bool comparePaths(string path1, string path2)
        {
            return String.Compare(Path.GetFullPath(path1).TrimEnd('\\').TrimEnd('/'),
                Path.GetFullPath(path2).TrimEnd('\\').TrimEnd('/'),
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
            string oldmapscriptdir = Path.GetDirectoryName(LuaEngine.MakeGroundMapScriptPath(false, Path.GetFileNameWithoutExtension(oldfilepath), "/init.lua"));
            string newmapscriptdir = LuaEngine.MakeGroundMapScriptPath(true, Path.GetFileNameWithoutExtension(newfilepath), "");

            //Check if we have anything to copy at all!
            if (oldmapscriptdir != newmapscriptdir && !String.IsNullOrEmpty(oldfilepath))
            {
                Directory.CreateDirectory(newmapscriptdir);
                foreach (string f in Directory.GetFiles(oldmapscriptdir, "*.*", SearchOption.AllDirectories)) //This lists all subfiles recursively
                {
                    //Path to the sub-directory within the script folder containing this file
                    string subdirpath = f.Substring(oldmapscriptdir.Length + 1); //Count + 1 because of the last path separator!
                    //Path to the sub-directory within the new script folder where we'll copy this file!
                    string destpath = Path.Combine(newmapscriptdir, subdirpath);

                    //Ensure all subdirectories are created recursively, if there are any!
                    Directory.CreateDirectory(Path.GetDirectoryName(destpath));

                    //Copy the file itself
                    if (File.Exists(f))
                        File.Copy(f, destpath, false);
                }
            }
            else
            {
                //We just create a new one straight away!
                LuaEngine.Instance.CreateGroundMapScriptDir(Path.GetFileNameWithoutExtension(newfilepath));
            }
        }

        public void TabChanged()
        {
            switch (selectedTabIndex)
            {
                case 0://Textures
                    GroundEditScene.Instance.EditMode = GroundEditScene.EditorMode.Texture;
                    break;
                case 1://Decorations
                    GroundEditScene.Instance.EditMode = GroundEditScene.EditorMode.Decoration;
                    break;
                case 2://Walls
                    GroundEditScene.Instance.EditMode = GroundEditScene.EditorMode.Wall;
                    break;
                case 3://Entities
                    GroundEditScene.Instance.EditMode = GroundEditScene.EditorMode.Entity;
                    break;
                default:
                    GroundEditScene.Instance.EditMode = GroundEditScene.EditorMode.Other;
                    break;
            }
        }

        public void ProcessInput(InputManager input)
        {
            lock (GameBase.lockObj)
            {
                if (input.BaseKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftControl))
                {
                    if (input.BaseKeyPressed(Microsoft.Xna.Framework.Input.Keys.Z) && input.BaseKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftShift))
                    {
                        mnuRedo_Click();
                        return;
                    }
                    else if (input.BaseKeyPressed(Microsoft.Xna.Framework.Input.Keys.Z))
                    {
                        mnuUndo_Click();
                        return;
                    }
                }
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

        public void ProcessUndo()
        {
            lock (GameBase.lockObj)
            {
                switch (selectedTabIndex)
                {
                    case 3://Entities
                        Entities.ProcessUndo();
                        break;
                }
            }
        }
    }
}
