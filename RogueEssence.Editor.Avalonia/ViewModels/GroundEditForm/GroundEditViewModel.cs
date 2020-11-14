using System;
using System.Collections.Generic;
using System.Text;
using RogueEssence;
using RogueEssence.Dungeon;
using RogueEssence.Ground;
using RogueEssence.Data;

namespace RogueEssence.Dev.ViewModels
{
    public class GroundEditViewModel : ViewModelBase
    {
        public GroundEditViewModel()
        {
            Textures = new GroundTabTexturesViewModel();
            Walls = new GroundTabWallsViewModel();
            Entities = new GroundTabEntitiesViewModel();
            Properties = new GroundTabPropertiesViewModel();
            Script = new GroundTabScriptViewModel();
            Strings = new GroundTabStringsViewModel();
        }

        public GroundTabTexturesViewModel Textures { get; set; }
        public GroundTabWallsViewModel Walls { get; set; }
        public GroundTabEntitiesViewModel Entities { get; set; }
        public GroundTabPropertiesViewModel Properties { get; set; }
        public GroundTabScriptViewModel Script { get; set; }
        public GroundTabStringsViewModel Strings { get; set; }


        private void New_Click()
        {
            ////Check all callbacks by default
            //for (int ii = 0; ii < chklstScriptMapCallbacks.Items.Count; ii++)
            //    chklstScriptMapCallbacks.SetItemChecked(ii, true);

            //CurrentFile = "";

            ////Schedule the map creation
            //GroundEditScene.Instance.PendingDevEvent = DoNew();
        }

        private void Open_Click()
        {
            //openFileDialog.Filter = "map files (*" + DataManager.GROUND_EXT + ")|*" + DataManager.GROUND_EXT;
            //DialogResult result = openFileDialog.ShowDialog();

            //if (result == DialogResult.OK)
            //{
            //    if (!ComparePaths(Path.Join(Directory.GetCurrentDirectory(), DataManager.GROUND_PATH), Path.GetDirectoryName(openFileDialog.FileName)))
            //        MessageBox.Show(String.Format("Map can only be loaded from {0}!", Path.Join(Directory.GetCurrentDirectory(), DataManager.GROUND_PATH)), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //    else
            //    {
            //        CurrentFile = openFileDialog.FileName;

            //        //Schedule the map load
            //        GroundEditScene.Instance.PendingDevEvent = DoLoad(Path.GetFileNameWithoutExtension(openFileDialog.FileName));
            //    }
            //}
        }

        private void Save_Click()
        {
            //if (CurrentFile == "")
            //    saveAsToolStripMenuItem_Click(sender, e); //Since its the same thing, might as well re-use the function! It makes everyone's lives easier!
            //else
            //    GroundEditScene.Instance.PendingDevEvent = DoSave(ZoneManager.Instance.CurrentGround, CurrentFile, CurrentFile);
        }
        private void SaveAs_Click()
        {
            //DialogResult result = saveMapFileDialog.ShowDialog();

            //if (result == System.Windows.Forms.DialogResult.OK)
            //{
            //    if (!ComparePaths(Directory.GetCurrentDirectory() + "/" + DataManager.GROUND_PATH, Path.GetDirectoryName(saveMapFileDialog.FileName)))
            //        MessageBox.Show(String.Format("Map can only be saved to {0}!", Directory.GetCurrentDirectory() + "/" + DataManager.GROUND_PATH), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //    else
            //    {
            //        string oldFilename = CurrentFile;
            //        ZoneManager.Instance.CurrentGround.AssetName = Path.GetFileNameWithoutExtension(saveMapFileDialog.FileName); //Set the assetname to the file name!
            //        CurrentFile = saveMapFileDialog.FileName;

            //        //Schedule saving the map
            //        GroundEditScene.Instance.PendingDevEvent = DoSave(ZoneManager.Instance.CurrentGround, CurrentFile, oldFilename);
            //    }
            //}
        }


        private void ImportFromTileset_Click()
        {
            //if (tileBrowser.CurrentTileset == "")
            //    MessageBox.Show(String.Format("No tileset to import!"), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //else
            //    GroundEditScene.Instance.PendingDevEvent = DoImportTileset(tileBrowser.CurrentTileset);
        }

        private void ImportFromPng_Click()
        {
            //openFileDialog.Filter = "PNG files (*.png)|*.png";
            //DialogResult result = openFileDialog.ShowDialog();

            //if (result == DialogResult.OK)
            //    GroundEditScene.Instance.PendingDevEvent = DoImportPng(openFileDialog.FileName);
        }


        private void ReSize_Click()
        {
            //MapResizeWindow window = new MapResizeWindow(ZoneManager.Instance.CurrentGround.Width, ZoneManager.Instance.CurrentGround.Height);

            //if (window.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            //{
            //    DiagManager.Instance.LoadMsg = "Resizing Map...";
            //    DevForm.EnterLoadPhase(GameBase.LoadPhase.Content);

            //    ZoneManager.Instance.CurrentGround.ResizeJustified(window.MapWidth, window.MapHeight, window.ResizeDir);

            //    DevForm.EnterLoadPhase(GameBase.LoadPhase.Ready);
            //}
        }

        private void ReTile_Click()
        {
            //MapRetileWindow window = new MapRetileWindow(ZoneManager.Instance.CurrentGround.TileSize);

            //if (window.ShowDialog() == DialogResult.OK)
            //{
            //    DiagManager.Instance.LoadMsg = "Retiling Map...";
            //    DevForm.EnterLoadPhase(GameBase.LoadPhase.Content);

            //    ZoneManager.Instance.CurrentGround.Retile(window.TileSize / GraphicsManager.TEX_SIZE);

            //    tileBrowser.SetTileSize(window.TileSize);

            //    DevForm.EnterLoadPhase(GameBase.LoadPhase.Ready);
            //}
        }


        private void Undo_Click()
        {

        }

        private void Redo_Click()
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

    }
}
