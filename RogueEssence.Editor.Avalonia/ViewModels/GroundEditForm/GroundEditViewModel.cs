using System;
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

namespace RogueEssence.Dev.ViewModels
{
    public class GroundEditViewModel : ViewModelBase
    {
        public GroundEditViewModel(Window dialogParent)
        {
            Textures = new GroundTabTexturesViewModel();
            Walls = new GroundTabWallsViewModel();
            Entities = new GroundTabEntitiesViewModel();
            Properties = new GroundTabPropertiesViewModel();
            Script = new GroundTabScriptViewModel();
            Strings = new GroundTabStringsViewModel();
            DialogParent = dialogParent;
        }

        public GroundTabTexturesViewModel Textures { get; set; }
        public GroundTabWallsViewModel Walls { get; set; }
        public GroundTabEntitiesViewModel Entities { get; set; }
        public GroundTabPropertiesViewModel Properties { get; set; }
        public GroundTabScriptViewModel Script { get; set; }
        public GroundTabStringsViewModel Strings { get; set; }


        public string CurrentFile;
        public Window DialogParent;


        public void New_Click()
        {

            //Check all callbacks by default
            for (int ii = 0; ii < Script.ScriptItems.Count; ii++)
                Script.ScriptItems[ii].IsChecked = true;

            CurrentFile = "";

            lock (GameBase.lockObj)
            {
                //Schedule the map creation
                GroundEditScene.Instance.PendingDevEvent = DoNew();
            }
        }

        public async void Open_Click()
        {
            string mapDir = Path.Join(Directory.GetCurrentDirectory(), DataManager.GROUND_PATH);
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Directory = mapDir;

            FileDialogFilter filter = new FileDialogFilter();
            filter.Name = "Ground Files";
            filter.Extensions.Add(DataManager.GROUND_EXT.Substring(1));
            openFileDialog.Filters.Add(filter);

            string[] results = await openFileDialog.ShowAsync(DialogParent);

            if (results.Length > 0)
            {
                if (!comparePaths(Path.Join(Directory.GetCurrentDirectory(), DataManager.GROUND_PATH), Path.GetDirectoryName(results[0])))
                    await MessageBox.Show(DialogParent, String.Format("Map can only be loaded from:\n{0}", Path.Join(Directory.GetCurrentDirectory(), DataManager.GROUND_PATH)), "Error", MessageBox.MessageBoxButtons.Ok);
                else
                {
                    lock (GameBase.lockObj)
                    {
                        CurrentFile = results[0];

                        //Schedule the map load
                        GroundEditScene.Instance.PendingDevEvent = DoLoad(Path.GetFileNameWithoutExtension(results[0]));
                    }
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
                    GroundEditScene.Instance.PendingDevEvent = DoSave(ZoneManager.Instance.CurrentGround, CurrentFile, CurrentFile);
            }
        }
        public async void SaveAs_Click()
        {
            string mapDir = Path.Join(Directory.GetCurrentDirectory(), DataManager.GROUND_PATH);
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Directory = mapDir;

            FileDialogFilter filter = new FileDialogFilter();
            filter.Name = "Ground Files";
            filter.Extensions.Add(DataManager.GROUND_EXT.Substring(1));
            saveFileDialog.Filters.Add(filter);

            string result = await saveFileDialog.ShowAsync(DialogParent);

            if (result != null)
            {
                if (!comparePaths(Directory.GetCurrentDirectory() + "/" + DataManager.GROUND_PATH, Path.GetDirectoryName(result)))
                    await MessageBox.Show(DialogParent, String.Format("Map can only be saved to:\n{0}", Directory.GetCurrentDirectory() + "/" + DataManager.GROUND_PATH), "Error", MessageBox.MessageBoxButtons.Ok);
                else
                {
                    lock (GameBase.lockObj)
                    {
                        string oldFilename = CurrentFile;
                        ZoneManager.Instance.CurrentGround.AssetName = Path.GetFileNameWithoutExtension(result); //Set the assetname to the file name!
                        CurrentFile = result;

                        //Schedule saving the map
                        GroundEditScene.Instance.PendingDevEvent = DoSave(ZoneManager.Instance.CurrentGround, CurrentFile, oldFilename);
                    }
                }
            }
        }

        public async void ImportFromPng_Click()
        {
            string mapDir = Path.Join(Directory.GetCurrentDirectory(), DataManager.GROUND_PATH);
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Directory = mapDir;

            FileDialogFilter filter = new FileDialogFilter();
            filter.Name = "PNG Files";
            filter.Extensions.Add("png");
            openFileDialog.Filters.Add(filter);

            string[] results = await openFileDialog.ShowAsync(DialogParent);

            lock (GameBase.lockObj)
            {
                if (results.Length > 0)
                    GroundEditScene.Instance.PendingDevEvent = DoImportPng(results[0]);
            }
        }


        public async void ImportFromTileset_Click()
        {
            if (Textures.TileBrowser.CurrentTileset == "")
                await MessageBox.Show(DialogParent, String.Format("No tileset to import!"), "Error", MessageBox.MessageBoxButtons.Ok);
            else
                GroundEditScene.Instance.PendingDevEvent = DoImportTileset(Textures.TileBrowser.CurrentTileset);
        }


        public async void ReSize_Click()
        {

            MapResizeWindow window = new MapResizeWindow();
            MapResizeViewModel viewModel = new MapResizeViewModel(ZoneManager.Instance.CurrentGround.Width, ZoneManager.Instance.CurrentGround.Height);
            window.DataContext = viewModel;

            bool result = await window.ShowDialog<bool>(DialogParent);

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

            bool result = await window.ShowDialog<bool>(DialogParent);

            lock (GameBase.lockObj)
            {
                if (result)
                {
                    DiagManager.Instance.LoadMsg = "Retiling Map...";
                    DevForm.EnterLoadPhase(GameBase.LoadPhase.Content);

                    ZoneManager.Instance.CurrentGround.Retile(viewModel.TileSize / GraphicsManager.TEX_SIZE);

                    Textures.TileBrowser.SetTileSize(viewModel.TileSize);

                    DevForm.EnterLoadPhase(GameBase.LoadPhase.Ready);
                }
            }
        }


        public void Undo_Click()
        {

        }

        public void Redo_Click()
        {

        }



        private IEnumerator<YieldInstruction> DoNew()
        {
            ////take all the necessary steps before and after moving to the map

            //DiagManager.Instance.LoadMsg = "Loading Map...";
            //DevForm.EnterLoadPhase(GameBase.LoadPhase.Content);
            //GameManager.Instance.ForceReady();


            //ZoneManager.Instance.CurrentZone.DevNewGround();

            //loadEditorSettings();

            //DevForm.EnterLoadPhase(GameBase.LoadPhase.Ready);

            yield break;
        }
        private IEnumerator<YieldInstruction> DoLoad(string mapName)
        {
            ////take all the necessary steps before and after moving to the map

            //DiagManager.Instance.LoadMsg = "Loading Map...";
            //DevForm.EnterLoadPhase(GameBase.LoadPhase.Content);
            //GameManager.Instance.ForceReady();

            //ZoneManager.Instance.CurrentZone.DevLoadGround(mapName);

            //loadEditorSettings();

            //DevForm.EnterLoadPhase(GameBase.LoadPhase.Ready);

            yield break;
        }

        public void LoadFromCurrentGround()
        {
            //if (ZoneManager.Instance.CurrentGround.AssetName != "")
            //    CurrentFile = Path.Join(Directory.GetCurrentDirectory(), DataManager.GROUND_PATH, ZoneManager.Instance.CurrentGround.AssetName + DataManager.GROUND_EXT);
            //else
            //    CurrentFile = "";

            //loadEditorSettings();
        }

        private void loadEditorSettings()
        {
            //lbxLayers.LoadFromList(ZoneManager.Instance.CurrentGround.Layers, IsLayerChecked);
            //tileBrowser.SetTileSize(ZoneManager.Instance.CurrentGround.TileSize);

            //RefreshTitle();
            //UpdateHasScriptFolder();
            //LoadMapProperties();
            //SetupLayerVisibility();
            //LoadAndSetupStrings();
        }

        private IEnumerator<YieldInstruction> DoImportPng(string filePath)
        {
            //string sheetName = Path.GetFileNameWithoutExtension(filePath);

            //string outputFile = String.Format(GraphicsManager.TILE_PATTERN, sheetName);


            ////load into tilesets
            //using (BaseSheet tileset = BaseSheet.Import(filePath))
            //{
            //    List<BaseSheet> tileList = new List<BaseSheet>();
            //    tileList.Add(tileset);
            //    ImportHelper.SaveTileSheet(tileList, outputFile, ZoneManager.Instance.CurrentGround.TileSize);
            //}


            ////update the index
            //using (FileStream stream = File.OpenRead(outputFile))
            //{
            //    using (BinaryReader reader = new BinaryReader(stream))
            //    {
            //        TileIndexNode guide = TileIndexNode.Load(reader);
            //        GraphicsManager.TileIndex.Nodes[sheetName] = guide;
            //    }
            //}

            //string search = Path.GetDirectoryName(String.Format(GraphicsManager.TILE_PATTERN, '*'));
            //using (FileStream stream = new FileStream(search + "/index.idx", FileMode.Create, FileAccess.Write))
            //{
            //    using (BinaryWriter writer = new BinaryWriter(stream))
            //        GraphicsManager.TileIndex.Save(writer);
            //}

            //GraphicsManager.ClearCaches(GraphicsManager.AssetType.Tile);

            //tileBrowser.UpdateTilesList();
            //tileBrowser.SelectTileset(sheetName);
            yield break;
        }

        private IEnumerator<YieldInstruction> DoImportTileset(string sheetName)
        {
            //Loc newSize = GraphicsManager.TileIndex.GetTileDims(sheetName);

            //DiagManager.Instance.LoadMsg = "Loading Map...";
            //DevForm.EnterLoadPhase(GameBase.LoadPhase.Content);

            //ZoneManager.Instance.CurrentGround.ResizeJustified(newSize.X, newSize.Y, Dir8.UpLeft);

            ////set tilesets
            //for (int yy = 0; yy < newSize.Y; yy++)
            //{
            //    for (int xx = 0; xx < newSize.X; xx++)
            //        ZoneManager.Instance.CurrentGround.Layers[lbxLayers.SelectedIndex].Tiles[xx][yy] = new AutoTile(new TileLayer(new Loc(xx, yy), sheetName));
            //}

            //DevForm.EnterLoadPhase(GameBase.LoadPhase.Ready);

            yield break;
        }

        private IEnumerator<YieldInstruction> DoSave(GroundMap curgrnd, string filepath, string oldfname)
        {
            //DataManager.SaveData(filepath, curgrnd);

            ////Actually create the script folder, and default script file.
            //CreateOrCopyScriptData(oldfname, CurrentFile);
            ////Strings will have to be created on demand!
            //UpdateHasScriptFolder();

            //RefreshTitle();

            yield break;
        }


        private static bool comparePaths(string path1, string path2)
        {
            return String.Compare(Path.GetFullPath(path1).TrimEnd('\\'),
                Path.GetFullPath(path2).TrimEnd('\\'),
                StringComparison.InvariantCultureIgnoreCase) == 0;
        }

    }
}
