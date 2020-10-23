using System;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using RogueEssence.Dungeon;
using RogueElements;
using RogueEssence.Data;
using RogueEssence.Dev;
using RogueEssence.Ground;
using System.Resources;
using System.Collections;
using System.Collections.Generic;
using RogueEssence.Script;
using RogueEssence.Content;
using System.Diagnostics;

namespace RogueEssence.Dev
{
    public partial class GroundEditor : Form, IGroundEditor
    {
        public enum EntEditMode
        {
            SelectEntity = 0,
            PlaceEntity = 1,
            MoveEntity = 2,
        }

        public bool Active { get; private set; }

        private string CurrentFile;

        private List<string> objectAnimIndex;

        public bool ShowDataLayer;

        private EntEditMode EntMode;
        private TileEditMode BlockMode;

        /// <summary>
        /// The currently selected entity.
        /// The entity is either selected using select mode, or move mode!
        /// </summary>
        private GroundEntity selectedEntity;

        public GroundEditor()
        {
            SetupLayerVisibility();

            InitializeComponent();

            lbxLayers.LoadFromList(ZoneManager.Instance.CurrentGround.Layers, IsLayerChecked);
            tileBrowser.SetTileSize(ZoneManager.Instance.CurrentGround.TileSize);

            UpdateHasScriptFolder();

            saveMapFileDialog.Filter = "map files (*" + DataManager.GROUND_EXT + ")|*" + DataManager.GROUND_EXT;

            for (int ii = 0; ii < (int)GroundEntity.EEntTypes.Count; ii++)
                cmbEntityType.Items.Add(((GroundEntity.EEntTypes)ii).ToLocal());

            for(int ii = 0; ii <= (int)Gender.Female; ii++)
                cmbEntCharGender.Items.Add(((Gender)ii).ToLocal());

            string[] names = DataManager.Instance.DataIndices[DataManager.DataType.Monster].GetLocalStringArray();
            for (int ii = 0; ii < names.Length; ii++)
                cmbEntKind.Items.Add(ii + " - " + names[ii]);

            foreach (string s in TemplateManager.TemplateTypeNames)
                cmbTemplateType.Items.Add(s);

            foreach (LuaEngine.EMapCallbacks v in LuaEngine.EnumerateCallbackTypes())
                chklstScriptMapCallbacks.Items.Add(v.ToString());


            for (int ii = 0; ii <= (int)Map.ScrollEdge.Clamp; ii++)
                cbScrollEdge.Items.Add(((Map.ScrollEdge)ii).ToLocal());


            tabctrlEntData.TabPages.Clear();
            cmbEntityType.SelectedIndex = 0;
            cmbEntCharGender.SelectedIndex = 0;
            cmbEntKind.SelectedIndex = 0;
            cmbTemplateType.SelectedIndex = 0;
            cmbSpawnerType.SelectedIndex = 0;

            objectAnimIndex = new List<string>();

            objectAnimIndex.Add("");
            cbEntObjSpriteID.Items.Add("---");

            string[] dirs = Directory.GetFiles(DiagManager.CONTENT_PATH + "Object/");

            for (int ii = 0; ii < dirs.Length; ii++)
            {
                string filename = Path.GetFileNameWithoutExtension(dirs[ii]);
                cbEntObjSpriteID.Items.Add(filename);
                objectAnimIndex.Add(filename);
            }
            cbEntObjSpriteID.SelectedIndex = 0;


            ReloadDirections();
            cmbEntityDir.SelectedIndex = 0;

            selectedEntity = null;
        }


        public void ProcessInput(InputManager input)
        {
            GroundEditScene.Instance.MouseLoc = input.MouseLoc;
            if (Collision.InBounds(GraphicsManager.WindowWidth, GraphicsManager.WindowHeight, input.MouseLoc))
            {
                if (tabMapOptions.SelectedTab == tabTextures)
                {
                    Loc tileCoords = GroundEditScene.Instance.ScreenCoordsToMapCoords(input.MouseLoc);
                    switch (tileBrowser.TexMode)
                    {
                        case TileEditMode.Draw:
                            {
                                if (input[FrameInput.InputType.LeftMouse])
                                    PaintTile(tileCoords, GetBrush());
                                else if (input[FrameInput.InputType.RightMouse])
                                    PaintTile(tileCoords, new TileBrush(new TileLayer(), Loc.One));
                            }
                            break;
                        case TileEditMode.Rectangle:
                            {
                                Loc groundCoords = GroundEditScene.Instance.ScreenCoordsToGroundCoords(input.MouseLoc);
                                if (input.JustPressed(FrameInput.InputType.LeftMouse))
                                {
                                    GroundEditScene.Instance.AutoTileInProgress = new AutoTile(GetBrush().Layer);
                                    GroundEditScene.Instance.RectInProgress = new Rect(groundCoords, Loc.Zero);
                                }
                                else if (input[FrameInput.InputType.LeftMouse])
                                    GroundEditScene.Instance.RectInProgress.Size = (groundCoords - GroundEditScene.Instance.RectInProgress.Start);
                                else if (input.JustReleased(FrameInput.InputType.LeftMouse))
                                {
                                    RectTile(GroundEditScene.Instance.TileRectPreview(), GetBrush().Layer);
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
                                    RectTile(GroundEditScene.Instance.TileRectPreview(), new TileLayer());
                                    GroundEditScene.Instance.AutoTileInProgress = null;
                                }
                            }
                            break;
                        case TileEditMode.Fill:
                            {
                                if (input.JustReleased(FrameInput.InputType.LeftMouse))
                                    FillTile(tileCoords, GetBrush().Layer);
                                else if (input.JustReleased(FrameInput.InputType.RightMouse))
                                    FillTile(tileCoords, new TileLayer());
                            }
                            break;
                        case TileEditMode.Eyedrop:
                            {
                                if (input[FrameInput.InputType.LeftMouse])
                                    EyedropTile(tileCoords);
                            }
                            break;
                    }

                }
                else if (tabMapOptions.SelectedTab == tabBlock)
                {
                    Loc tileCoords = GroundEditScene.Instance.ScreenCoordsToBlockCoords(input.MouseLoc);
                    switch (BlockMode)
                    {
                        case TileEditMode.Draw:
                            {
                                if (input[FrameInput.InputType.LeftMouse])
                                    PaintBlockTile(tileCoords, true);
                                else if (input[FrameInput.InputType.RightMouse])
                                    PaintBlockTile(tileCoords, false);
                            }
                            break;
                        case TileEditMode.Rectangle:
                            {
                                Loc groundCoords = GroundEditScene.Instance.ScreenCoordsToGroundCoords(input.MouseLoc);
                                if (input.JustPressed(FrameInput.InputType.LeftMouse))
                                {
                                    GroundEditScene.Instance.BlockInProgress = true;
                                    GroundEditScene.Instance.RectInProgress = new Rect(groundCoords, Loc.Zero);
                                }
                                else if (input[FrameInput.InputType.LeftMouse])
                                    GroundEditScene.Instance.RectInProgress.Size = (groundCoords - GroundEditScene.Instance.RectInProgress.Start);
                                else if (input.JustReleased(FrameInput.InputType.LeftMouse))
                                {
                                    RectBlockTile(GroundEditScene.Instance.BlockRectPreview(), true);
                                    GroundEditScene.Instance.BlockInProgress = null;
                                }
                                else if (input.JustPressed(FrameInput.InputType.RightMouse))
                                {
                                    GroundEditScene.Instance.BlockInProgress = false;
                                    GroundEditScene.Instance.RectInProgress = new Rect(groundCoords, Loc.Zero);
                                }
                                else if (input[FrameInput.InputType.RightMouse])
                                    GroundEditScene.Instance.RectInProgress.Size = (groundCoords - GroundEditScene.Instance.RectInProgress.Start);
                                else if (input.JustReleased(FrameInput.InputType.RightMouse))
                                {
                                    RectBlockTile(GroundEditScene.Instance.BlockRectPreview(), false);
                                    GroundEditScene.Instance.BlockInProgress = null;
                                }
                            }
                            break;
                        case TileEditMode.Fill:
                            {
                                if (input.JustReleased(FrameInput.InputType.LeftMouse))
                                    FillBlockTile(tileCoords, true);
                                else if (input.JustReleased(FrameInput.InputType.RightMouse))
                                    FillBlockTile(tileCoords, false);
                            }
                            break;
                    }
                }
                else if (tabMapOptions.SelectedTab == tabEntities)
                {
                    Loc groundCoords = GroundEditScene.Instance.ScreenCoordsToGroundCoords(input.MouseLoc);
                    switch (EntMode)
                    {
                        case EntEditMode.PlaceEntity:
                            {
                                if (input.JustReleased(FrameInput.InputType.LeftMouse))
                                    PlaceEntity(groundCoords);
                                else if (input.JustReleased(FrameInput.InputType.RightMouse))
                                    RemoveEntityAt(groundCoords);
                                break;
                            }
                        case EntEditMode.SelectEntity:
                            {
                                if (input.JustReleased(FrameInput.InputType.LeftMouse))
                                    SelectEntityAt(groundCoords);
                                else if (input.JustReleased(FrameInput.InputType.RightMouse))
                                    EntityContext(input.MouseLoc, groundCoords);
                                break;
                            }
                        case EntEditMode.MoveEntity:
                            {
                                if (input.JustReleased(FrameInput.InputType.LeftMouse))
                                    MoveEntity(groundCoords);
                                break;
                            }
                    }
                }
            }
        }

        #region MENU_STRIP
        //===============================
        // Menu Strip
        //===============================


        private IEnumerator<YieldInstruction> DoNew()
        {
            //take all the necessary steps before and after moving to the map

            DiagManager.Instance.LoadMsg = "Loading Map...";
            DevForm.EnterLoadPhase(GameBase.LoadPhase.Content);
            GameManager.Instance.ForceReady();


            ZoneManager.Instance.CurrentZone.DevNewGround();

            loadEditorSettings();

            DevForm.EnterLoadPhase(GameBase.LoadPhase.Ready);

            yield break;
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Check all callbacks by default
            for (int ii = 0; ii < chklstScriptMapCallbacks.Items.Count; ii++)
                chklstScriptMapCallbacks.SetItemChecked(ii, true);

            CurrentFile = "";

            //Schedule the map creation
            GroundEditScene.Instance.PendingDevEvent = DoNew();
        }

        private IEnumerator<YieldInstruction> DoLoad(string mapName)
        {
            //take all the necessary steps before and after moving to the map

            DiagManager.Instance.LoadMsg = "Loading Map...";
            DevForm.EnterLoadPhase(GameBase.LoadPhase.Content);
            GameManager.Instance.ForceReady();

            ZoneManager.Instance.CurrentZone.DevLoadGround(mapName);

            loadEditorSettings();

            DevForm.EnterLoadPhase(GameBase.LoadPhase.Ready);

            yield break;
        }

        public void LoadFromCurrentGround()
        {
            if (ZoneManager.Instance.CurrentGround.AssetName != "")
                CurrentFile = Path.Join(Directory.GetCurrentDirectory(), DataManager.GROUND_PATH, ZoneManager.Instance.CurrentGround.AssetName + DataManager.GROUND_EXT);
            else
                CurrentFile = "";

            loadEditorSettings();
        }

        private void loadEditorSettings()
        {
            lbxLayers.LoadFromList(ZoneManager.Instance.CurrentGround.Layers, IsLayerChecked);
            tileBrowser.SetTileSize(ZoneManager.Instance.CurrentGround.TileSize);

            RefreshTitle();
            UpdateHasScriptFolder();
            LoadMapProperties();
            SetupLayerVisibility();
            LoadAndSetupStrings();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog.Filter = "map files (*" + DataManager.GROUND_EXT + ")|*" + DataManager.GROUND_EXT;
            DialogResult result = openFileDialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                if (!ComparePaths(Path.Join(Directory.GetCurrentDirectory(), DataManager.GROUND_PATH), Path.GetDirectoryName(openFileDialog.FileName)))
                    MessageBox.Show(String.Format("Map can only be loaded from {0}!", Path.Join(Directory.GetCurrentDirectory(), DataManager.GROUND_PATH)), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else
                {
                    CurrentFile = openFileDialog.FileName;

                    //Schedule the map load
                    GroundEditScene.Instance.PendingDevEvent = DoLoad(Path.GetFileNameWithoutExtension(openFileDialog.FileName));
                }
            }
        }


        private IEnumerator<YieldInstruction> DoImportPng(string filePath)
        {
            string sheetName = Path.GetFileNameWithoutExtension(filePath);

            string outputFile = String.Format(GraphicsManager.TILE_PATTERN, sheetName);


            //load into tilesets
            using (BaseSheet tileset = BaseSheet.Import(filePath))
            {
                List<BaseSheet> tileList = new List<BaseSheet>();
                tileList.Add(tileset);
                ImportHelper.SaveTileSheet(tileList, outputFile, ZoneManager.Instance.CurrentGround.TileSize);
            }


            //update the index
            using (FileStream stream = File.OpenRead(outputFile))
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    TileIndexNode guide = TileIndexNode.Load(reader);
                    GraphicsManager.TileIndex.Nodes[sheetName] = guide;
                }
            }
            
            string search = Path.GetDirectoryName(String.Format(GraphicsManager.TILE_PATTERN, '*'));
            using (FileStream stream = new FileStream(search + "/index.idx", FileMode.Create, FileAccess.Write))
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                    GraphicsManager.TileIndex.Save(writer);
            }

            GraphicsManager.ClearCaches(GraphicsManager.AssetType.Tile);

            tileBrowser.UpdateTilesList();
            tileBrowser.SelectTileset(sheetName);
            yield break;
        }

        private IEnumerator<YieldInstruction> DoImportTileset(string sheetName)
        {
            Loc newSize = GraphicsManager.TileIndex.GetTileDims(sheetName);

            DiagManager.Instance.LoadMsg = "Loading Map...";
            DevForm.EnterLoadPhase(GameBase.LoadPhase.Content);

            ZoneManager.Instance.CurrentGround.ResizeJustified(newSize.X, newSize.Y, Dir8.UpLeft);

            //set tilesets
            for (int yy = 0; yy < newSize.Y; yy++)
            {
                for (int xx = 0; xx < newSize.X; xx++)
                    ZoneManager.Instance.CurrentGround.Layers[lbxLayers.SelectedIndex].Tiles[xx][yy] = new AutoTile(new TileLayer(new Loc(xx, yy), sheetName));
            }

            DevForm.EnterLoadPhase(GameBase.LoadPhase.Ready);

            yield break;
        }

        private void importFromTilesetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tileBrowser.CurrentTileset == "")
                MessageBox.Show(String.Format("No tileset to import!"), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else
                GroundEditScene.Instance.PendingDevEvent = DoImportTileset(tileBrowser.CurrentTileset);
        }

        private void importFromPngToolStripMenuItem_Click(object sender, EventArgs e)
        {

            openFileDialog.Filter = "PNG files (*.png)|*.png";
            DialogResult result = openFileDialog.ShowDialog();

            if (result == DialogResult.OK)
                GroundEditScene.Instance.PendingDevEvent = DoImportPng(openFileDialog.FileName);
        }

        private IEnumerator<YieldInstruction> DoSave(GroundMap curgrnd, string filepath, string oldfname)
        {

            DataManager.SaveData(filepath, curgrnd);

            //Actually create the script folder, and default script file.
            CreateOrCopyScriptData(oldfname, CurrentFile);
            //Strings will have to be created on demand!
            UpdateHasScriptFolder();

            RefreshTitle();

            yield break;
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result = saveMapFileDialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                if (!ComparePaths(Directory.GetCurrentDirectory() + "/" + DataManager.GROUND_PATH, Path.GetDirectoryName(saveMapFileDialog.FileName)))
                    MessageBox.Show(String.Format("Map can only be saved to {0}!", Directory.GetCurrentDirectory() + "/" + DataManager.GROUND_PATH), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else
                {
                    string oldFilename = CurrentFile;
                    ZoneManager.Instance.CurrentGround.AssetName = Path.GetFileNameWithoutExtension(saveMapFileDialog.FileName); //Set the assetname to the file name!
                    CurrentFile = saveMapFileDialog.FileName;

                    //Schedule saving the map
                    GroundEditScene.Instance.PendingDevEvent = DoSave(ZoneManager.Instance.CurrentGround, CurrentFile, oldFilename);
                }
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CurrentFile == "")
                saveAsToolStripMenuItem_Click(sender, e); //Since its the same thing, might as well re-use the function! It makes everyone's lives easier!
            else
                GroundEditScene.Instance.PendingDevEvent = DoSave(ZoneManager.Instance.CurrentGround, CurrentFile, CurrentFile);
        }


        #endregion

        void RefreshTitle()
        {
            if (String.IsNullOrWhiteSpace(CurrentFile))
                CurrentFile = "";

            if (CurrentFile == "")
                this.Text = "New Ground";
            else
                this.Text = Path.GetFileNameWithoutExtension(CurrentFile);
        }


        private void ReloadMusic()
        {
            lbxMusic.Items.Clear();

            string[] files = Directory.GetFiles(DataManager.MUSIC_PATH, "*.ogg", SearchOption.TopDirectoryOnly);

            lbxMusic.Items.Add("None");
            for (int ii = 0; ii < files.Length; ii++)
            {
                string song = files[ii].Substring((DataManager.MUSIC_PATH).Length);
                lbxMusic.Items.Add(song);
                if (song == ZoneManager.Instance.CurrentGround.Music)
                    lbxMusic.SelectedIndex = ii + 1;
            }
            if (lbxMusic.SelectedIndex < 0)
                lbxMusic.SelectedIndex = 0;
        }

        private void LoadMapProperties()
        {
            txtMapName.Text = ZoneManager.Instance.CurrentGround.Name.DefaultText;
            cbScrollEdge.SelectedIndex = (int)ZoneManager.Instance.CurrentGround.EdgeView;

            bool foundSong = false;
            for (int ii = 0; ii < lbxMusic.Items.Count; ii++)
            {
                string song = (string)lbxMusic.Items[ii];
                if (song == ZoneManager.Instance.CurrentGround.Music)
                {
                    lbxMusic.SelectedIndex = ii;
                    foundSong = true;
                    break;
                }
            }
            if (!foundSong)
                lbxMusic.SelectedIndex = -1;

            FillInMapScriptData(ZoneManager.Instance.CurrentGround);
        }

        void SetupLayerVisibility()
        {

            ShowDataLayer = false;
        }

        private void GroundEditor_Load(object sender, EventArgs e)
        {

            RefreshTitle();

            //string mapDir = (string)Registry.GetValue(DiagManager.REG_PATH, "MapDir", "");
            //if (String.IsNullOrEmpty(mapDir))
            string mapDir = Path.Join(Directory.GetCurrentDirectory(), DataManager.GROUND_PATH);
            openFileDialog.InitialDirectory = mapDir;
            saveMapFileDialog.InitialDirectory = mapDir;

            ReloadMusic();

            LoadMapProperties();
            LoadAndSetupStrings();

            Active = true;
        }

        private void GroundEditor_FormClosed(object sender, FormClosedEventArgs e)
        {
            Active = false;

            //move to the previous scene or the title, if there was none
            if (DataManager.Instance.Save != null && DataManager.Instance.Save.NextDest.IsValid())
                GameManager.Instance.SceneOutcome = GameManager.Instance.MoveToZone(DataManager.Instance.Save.NextDest);
            else
                GameManager.Instance.SceneOutcome = GameManager.Instance.RestartToTitle();
        }

        private void btnReloadSongs_Click(object sender, EventArgs e)
        {
            ReloadMusic();
        }

        //!#NOTE: This seems to set the map's localized name, and not its asset name. It might be a bit confusing?
        private void txtMapName_TextChanged(object sender, EventArgs e)
        {
            ZoneManager.Instance.CurrentGround.Name.DefaultText = txtMapName.Text;
        }


        private void cbScrollEdge_SelectedIndexChanged(object sender, EventArgs e)
        {
            ZoneManager.Instance.CurrentGround.EdgeView = (Map.ScrollEdge)cbScrollEdge.SelectedIndex;
        }

        private void lbxMusic_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lbxMusic.SelectedIndex <= 0)
            {
                ZoneManager.Instance.CurrentGround.Music = "";
            }
            else
            {
                string fileName = (string)lbxMusic.Items[lbxMusic.SelectedIndex];
                ZoneManager.Instance.CurrentGround.Music = fileName;
            }

            GameManager.Instance.BGM(ZoneManager.Instance.CurrentGround.Music, false);
        }

        private void resizeMapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MapResizeWindow window = new MapResizeWindow(ZoneManager.Instance.CurrentGround.Width, ZoneManager.Instance.CurrentGround.Height);

            if (window.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                DiagManager.Instance.LoadMsg = "Resizing Map...";
                DevForm.EnterLoadPhase(GameBase.LoadPhase.Content);

                ZoneManager.Instance.CurrentGround.ResizeJustified(window.MapWidth, window.MapHeight, window.ResizeDir);

                DevForm.EnterLoadPhase(GameBase.LoadPhase.Ready);
            }
        }

        private void retileMapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MapRetileWindow window = new MapRetileWindow(ZoneManager.Instance.CurrentGround.TileSize);

            if (window.ShowDialog() == DialogResult.OK)
            {
                DiagManager.Instance.LoadMsg = "Retiling Map...";
                DevForm.EnterLoadPhase(GameBase.LoadPhase.Content);

                ZoneManager.Instance.CurrentGround.Retile(window.TileSize / GraphicsManager.TEX_SIZE);

                tileBrowser.SetTileSize(window.TileSize);

                DevForm.EnterLoadPhase(GameBase.LoadPhase.Ready);
            }
        }

        public void PaintTile(Loc loc, TileBrush brush)
        {
            if (!Collision.InBounds(ZoneManager.Instance.CurrentGround.Width, ZoneManager.Instance.CurrentGround.Height, loc))
                return;

            if (brush.MultiSelect == Loc.One)
                ZoneManager.Instance.CurrentGround.Layers[lbxLayers.SelectedIndex].Tiles[loc.X][loc.Y] = new AutoTile(brush.Layer);
            else
            {
                for (int xx = 0; xx < brush.MultiSelect.X; xx++)
                {
                    for (int yy = 0; yy < brush.MultiSelect.Y; yy++)
                    {
                        TileFrame frame = brush.Layer.Frames[0];
                        ZoneManager.Instance.CurrentGround.Layers[lbxLayers.SelectedIndex].Tiles[loc.X + xx][loc.Y + yy] = new AutoTile(new TileLayer(frame.TexLoc + new Loc(xx, yy), frame.Sheet));
                    }
                }
            }
        }

        public void RectTile(Rect rect, TileLayer anim)
        {
            for (int xx = rect.X; xx < rect.End.X; xx++)
            {
                for (int yy = rect.Y; yy < rect.End.Y; yy++)
                {
                    if (!Collision.InBounds(ZoneManager.Instance.CurrentGround.Width, ZoneManager.Instance.CurrentGround.Height, new Loc(xx, yy)))
                        continue;

                    ZoneManager.Instance.CurrentGround.Layers[lbxLayers.SelectedIndex].Tiles[xx][yy] = new AutoTile(anim);
                }
            }
        }

        public void EyedropTile(Loc loc)
        {
            if (!Collision.InBounds(ZoneManager.Instance.CurrentGround.Width, ZoneManager.Instance.CurrentGround.Height, loc))
                return;

            tileBrowser.SetBrush(ZoneManager.Instance.CurrentGround.Layers[lbxLayers.SelectedIndex].Tiles[loc.X][loc.Y].Layers[0]);//TODO: an anim can have multiple layers if they're an autotile, we nee to somehow pick up autotiles
        }


        public void FillTile(Loc loc, TileLayer anim)
        {
            if (!Collision.InBounds(ZoneManager.Instance.CurrentGround.Width, ZoneManager.Instance.CurrentGround.Height, loc))
                return;

            AutoTile tile = ZoneManager.Instance.CurrentGround.Layers[lbxLayers.SelectedIndex].Tiles[loc.X][loc.Y].Copy();

            Grid.FloodFill(new Rect(0, 0, ZoneManager.Instance.CurrentGround.Width, ZoneManager.Instance.CurrentGround.Height),
                    (Loc testLoc) =>
                    {
                        return !tile.Equals(ZoneManager.Instance.CurrentGround.Layers[lbxLayers.SelectedIndex].Tiles[testLoc.X][testLoc.Y]);
                    },
                    (Loc testLoc) =>
                    {
                        return true;
                    },
                    (Loc testLoc) =>
                    {
                        ZoneManager.Instance.CurrentGround.Layers[lbxLayers.SelectedIndex].Tiles[testLoc.X][testLoc.Y] = new AutoTile(anim);
                    },
                loc);
        }

        public TileBrush GetBrush()
        {
            return tileBrowser.GetBrush();
        }


        public void PaintBlockTile(Loc loc, bool block)
        {
            if (!Collision.InBounds(ZoneManager.Instance.CurrentGround.TexWidth, ZoneManager.Instance.CurrentGround.TexHeight, loc))
                return;

            ZoneManager.Instance.CurrentGround.SetObstacle(loc.X, loc.Y, block ? 1u : 0u);
        }

        public void RectBlockTile(Rect rect, bool block)
        {
            for (int xx = rect.X; xx < rect.End.X; xx++)
            {
                for (int yy = rect.Y; yy < rect.End.Y; yy++)
                {
                    if (!Collision.InBounds(ZoneManager.Instance.CurrentGround.TexWidth, ZoneManager.Instance.CurrentGround.TexHeight, new Loc(xx, yy)))
                        continue;

                    ZoneManager.Instance.CurrentGround.SetObstacle(xx, yy, block ? 1u : 0u);
                }
            }
        }


        public void FillBlockTile(Loc loc, bool block)
        {
            if (!Collision.InBounds(ZoneManager.Instance.CurrentGround.TexWidth, ZoneManager.Instance.CurrentGround.TexHeight, loc))
                return;

            uint tile = ZoneManager.Instance.CurrentGround.GetObstacle(loc.X, loc.Y);

            Grid.FloodFill(new Rect(0, 0, ZoneManager.Instance.CurrentGround.TexWidth, ZoneManager.Instance.CurrentGround.TexHeight),
                    (Loc testLoc) =>
                    {
                        return tile != ZoneManager.Instance.CurrentGround.GetObstacle(testLoc.X, testLoc.Y);
                    },
                    (Loc testLoc) =>
                    {
                        return true;
                    },
                    (Loc testLoc) =>
                    {
                        ZoneManager.Instance.CurrentGround.SetObstacle(testLoc.X, testLoc.Y, block ? 1u : 0u);
                    },
                loc);
        }


        /// <summary>
        /// Meant to handle transition between editor modes all in one place
        /// </summary>
        /// <param name="ty"></param>
        private void ChangeEditorMode(EntEditMode ty)
        {
            EntMode = ty;

            //Then handle mode specific stuff here
            switch (ty)
            {
                case EntEditMode.PlaceEntity:
                    {
                        DeselectEntity();
                        break;
                    }
                case EntEditMode.SelectEntity:
                    {
                        break;
                    }
                case EntEditMode.MoveEntity:
                    {
                        break;
                    }
                default:
                    {
                        throw new Exception("GroundEditor.ChangeEditorMode(): Invalid mode!");
                    }
            }
        }


        private void lbxLayers_OnAddItem(int index, object element, LayerBox.EditCheckElementOp op)
        {
            MapLayer layer = new MapLayer("New Layer");
            layer.CreateNew(ZoneManager.Instance.CurrentGround.Width, ZoneManager.Instance.CurrentGround.Height);
            op(index, layer, layer.Visible);
        }

        private void lbxLayers_OnDuplicateItem(int index, object element, LayerBox.EditCheckElementOp op)
        {
            MapLayer layer = element as MapLayer;
            op(index, layer.Clone(), layer.Visible);
        }

        private void lbxLayers_OnEditItem(int index, object element, LayerBox.EditElementOp op)
        {
            MapLayer layer = element as MapLayer;

            MapLayerWindow window = new MapLayerWindow(layer.Name, layer.Front);

            if (window.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                layer.Name = window.LayerName;
                layer.Front = window.Front;
                op(index, layer);
            }
        }

        private void lbxLayers_CheckChanged(object sender, ItemCheckEventArgs e)
        {
            ZoneManager.Instance.CurrentGround.Layers[e.Index].Visible = (e.NewValue == CheckState.Checked);
        }

        public bool IsLayerChecked(object element)
        {
            return ((MapLayer)element).Visible;
        }


        #region MAP_SCRIPT_TAB
        //=========================================================================
        //  Script Stuff
        //=========================================================================

        /// <summary>
        /// Call this when saving as, so that if the previous map name has a script folder with data in it,
        /// we can copy it. And if it doesn't, we create a "blank slate" one!
        /// </summary>
        /// <param name="oldfilepath"></param>
        /// <param name="newfilepath"></param>
        private void CreateOrCopyScriptData(string oldfilepath, string newfilepath)
        {
            string oldmapscriptdir = LuaEngine.Instance._MakeMapScriptPath(Path.GetFileNameWithoutExtension(oldfilepath));
            string newmapscriptdir = LuaEngine.Instance._MakeMapScriptPath(Path.GetFileNameWithoutExtension(newfilepath));

            //Check if we have anything to copy at all!
            if (oldfilepath != newfilepath && !String.IsNullOrEmpty(oldfilepath) && Directory.Exists(oldfilepath) )
            {
                Directory.CreateDirectory(newmapscriptdir);
                foreach(string f in Directory.GetFiles(oldmapscriptdir, "*.*", SearchOption.AllDirectories) ) //This lists all subfiles recursively
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

        /// <summary>
        /// Takes the info from the specified map to populate the script tab
        /// </summary>
        /// <param name="m"></param>
        private void FillInMapScriptData(GroundMap m)
        {
            //Setup callback display without triggering events
            chklstScriptMapCallbacks.ItemCheck -= chklstScriptMapCallbacks_ItemCheck;
            var scev = ZoneManager.Instance.CurrentGround.ActiveScriptEvent();
            foreach (LuaEngine.EMapCallbacks s in scev)
            {
                chklstScriptMapCallbacks.SetItemChecked((int)s, true);
            }
            chklstScriptMapCallbacks.ItemCheck += chklstScriptMapCallbacks_ItemCheck;
        }

        /// <summary>
        /// Tell the scipt engine do do a full reload
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnMapReloadScripts_Click(object sender, EventArgs e)
        {
            LuaEngine.Instance.Reset();
            LuaEngine.Instance.ReInit();
        }


        /// <summary>
        /// Open script dir
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOpenScriptDir_Click(object sender, EventArgs e)
        {
            if (CurrentFile == "")
            {

                return;
            }
            string mapscriptdir = LuaEngine.Instance._MakeMapScriptPath(Path.GetFileNameWithoutExtension(CurrentFile));
            mapscriptdir = Path.GetFullPath(mapscriptdir);
            Process.Start("explorer.exe", mapscriptdir);
        }

        /// <summary>
        /// Called when callbacks for the map are added or removed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void chklstScriptMapCallbacks_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (e.Index < 0)
                return;

            if (e.NewValue == CheckState.Checked)
                ZoneManager.Instance.CurrentGround.AddMapScriptEvent((LuaEngine.EMapCallbacks)e.Index);
            else if (e.NewValue == CheckState.Unchecked)
                ZoneManager.Instance.CurrentGround.RemoveMapScriptEvent((LuaEngine.EMapCallbacks)e.Index);

        }
        #endregion

        #region MAP_STRINGS_TAB
        //=========================================================================
        //  Strings Tab
        //=========================================================================

        /// <summary>
        /// Calls this when the map's status as a new, unsaved map has changed.
        /// </summary>
        private void UpdateHasScriptFolder()
        {
            bool hasFolder = CurrentFile != "";
            btnCommitStrings.Enabled = hasFolder;
            btnReloadStrings.Enabled = hasFolder;
            btnOpenScriptDir.Enabled = hasFolder;
            btnMapReloadScripts.Enabled = hasFolder;
            chklstEntScriptCallbacks.Enabled = hasFolder;
        }


        private void gvStrings_CellValidating(object sender, System.Windows.Forms.DataGridViewCellValidatingEventArgs e)
        {
            if (gvStrings.Rows[e.RowIndex].IsNewRow)
                return;
            if (e.ColumnIndex != 0)
                return;

            string newVal = e.FormattedValue.ToString();

            for (int ii = 0; ii < gvStrings.Rows.Count;ii++)
            {
                if (ii == e.RowIndex)
                    continue;

                string key = gvStrings.Rows[ii].Cells[0].Value as string;

                if (newVal == key)
                {
                    e.Cancel = true;
                    break;
                }
            }
        }

        /// <summary>
        /// Prepares the view for displaying the current map's localized strings.
        /// </summary>
        private void SetupStringsDisplay(Dictionary<string, Dictionary<string, string>> CurrentStrings)
        {
            //Create the rows for each individual strings, and the columns for each individual languages

            // Initialize the DataGridView.
            gvStrings.Columns.Clear();
            gvStrings.Rows.Clear();

            //First column is always the name
            gvStrings.Columns.Add("name", "String Name");
            gvStrings.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            gvStrings.Columns[0].FillWeight = 1.5f;

            //Make language columns
            foreach (var code in RogueEssence.Text.SupportedLangs)
            {
                gvStrings.Columns.Add("col_" + code, code);
            }

            //Add the strings row by row
            foreach( var str in CurrentStrings )
            {
                //Create a new row to insert
                DataGridViewRow curow = new DataGridViewRow();

                //First add name
                DataGridViewTextBoxCell namecell = new DataGridViewTextBoxCell();
                namecell.Value = str.Key;
                curow.Cells.Add(namecell);

                //Then the values
                foreach (var code in RogueEssence.Text.SupportedLangs)
                {
                    DataGridViewTextBoxCell datacell = new DataGridViewTextBoxCell();
                    string codestr = code;
                    if (str.Value.ContainsKey(codestr))
                    {
                        datacell.Value = str.Value[codestr];
                    }
                    else
                    {
                        datacell.Value = null;
                    }
                    curow.Cells.Add(datacell);
                }
                gvStrings.Rows.Add(curow);
            }
        }

        /// <summary>
        /// Call this to do the full initialization of the localized string tab.
        /// </summary>
        private void LoadAndSetupStrings()
        {
            SetupStringsDisplay(LoadStrings(MakeCurrentStringsPath()));
        }

        /// <summary>
        /// Loads the strings files content into the data grid view for editing
        /// </summary>
        /// <param name="stringsdir">Directory in which string resx files are stored!</param>
        private Dictionary<string, Dictionary<string, string>> LoadStrings(string stringsdir)
        {
            //Clear old strings
            Dictionary<string, Dictionary<string, string>> CurrentStrings = new Dictionary<string, Dictionary<string, string>>();

            string FMTStr = String.Format("{0}{1}.{2}", Script.ScriptStrings.STRINGS_FILE_NAME, "{0}", Script.ScriptStrings.STRINGS_FILE_EXT);
            foreach(string code in RogueEssence.Text.SupportedLangs)
            {
                string fname = String.Format(FMTStr, code == "en" ? "" : ("." + code));//special case for english, which is default
                string path = Path.Combine(stringsdir, fname);

                if (File.Exists(path))
                {
                    using (ResXResourceReader rsxr = new ResXResourceReader(path))
                    {
                        foreach (DictionaryEntry d in rsxr)
                        {
                            string curkey = (string)d.Key;

                            if (!CurrentStrings.ContainsKey(curkey))
                                CurrentStrings.Add(curkey, new Dictionary<string, string>());

                            if (!CurrentStrings[curkey].ContainsKey(code))
                                CurrentStrings[curkey].Add(code, d.Value as string);
                            else
                                CurrentStrings[curkey][code] = d.Value as string;
                        }
                        rsxr.Close();
                    };

                    DiagManager.Instance.LogInfo(String.Format("GroundEditor.LoadStrings({0}): Loaded succesfully the \"{1}\" strings file for this map!", stringsdir, fname));
                }
                else
                {
                    DiagManager.Instance.LogInfo(String.Format("GroundEditor.LoadStrings({0}): Couldn't open the \"{1}\" strings file for this map!", stringsdir, fname));
                }

            }

            return CurrentStrings;
        }

        private Dictionary<string, Dictionary<string, string>> SaveStrings()
        {
            //Clear old strings
            Dictionary<string, Dictionary<string, string>> CurrentStrings = new Dictionary<string, Dictionary<string, string>>();

            foreach (DataGridViewRow row in gvStrings.Rows)
            {
                string key = row.Cells[0].Value as string;
                Dictionary<string, string> translations = new Dictionary<string, string>();
                for (int ii = 0; ii < RogueEssence.Text.SupportedLangs.Length; ii++)
                {
                    string val = row.Cells[ii + 1].Value as string;
                    if (val != null)
                        translations[RogueEssence.Text.SupportedLangs[ii]] = val;
                }
                CurrentStrings[key] = translations;
            }

            return CurrentStrings;
        }

        /// <summary>
        /// Writes the content of the dataview into a set of resx files for each languages
        /// </summary>
        /// <param name="stringsdir">Directory into which we save the string files</param>
        private void CommitStrings(string stringsdir, Dictionary<string, Dictionary<string, string>> CurrentStrings)
        {

            string FMTStr = String.Format("{0}{1}.{2}", Script.ScriptStrings.STRINGS_FILE_NAME, "{0}", Script.ScriptStrings.STRINGS_FILE_EXT);
            foreach (string code in RogueEssence.Text.SupportedLangs)
            {
                string fname = String.Format(FMTStr, code == "en" ? "" : ("." + code));//special case for english, which is default
                string path = Path.Combine(stringsdir, fname);
                using (ResXResourceWriter resx = new ResXResourceWriter(path))
                {
                    //Add all strings matching the specified language code
                    foreach (var str in CurrentStrings)
                    {
                        if (str.Value.ContainsKey(code))
                            resx.AddResource(new ResXDataNode(str.Key, str.Value[code]) { Comment = "" });
                        else
                            resx.AddResource(new ResXDataNode(str.Key, "") { Comment = "" }); //Put an empty string by default
                    }
                    resx.Generate();
                    resx.Close();
                }
            }
        }

        private void btnCommitStrings_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(CurrentFile))
                CommitStrings(MakeCurrentStringsPath(), SaveStrings());
            else
                MessageBox.Show(this, "Please save the map at least once before trying to commit the strings!!", "Please save first!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void btnReloadStrings_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(CurrentFile))
            {
                LoadAndSetupStrings();
            }
            else
                MessageBox.Show(this, "Please save the map and commit the strings at least once before trying to reload the strings!!", "Please save and commit strings first!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private string MakeCurrentStringsPath()
        {
            return String.Format("{0}{1}", Script.LuaEngine.MapScriptDirectory, ZoneManager.Instance.CurrentGround.AssetName);
        }

        private void btnStringAdd_Click(object sender, EventArgs e)
        {
            string defname = String.Format("String_{0}", gvStrings.Rows.Count);
            gvStrings.Rows.Add(defname);
            gvStrings.AutoResizeColumn(0);
            int lastrow = gvStrings.Rows.GetLastRow(DataGridViewElementStates.Visible);
            gvStrings.CurrentCell = gvStrings.Rows[lastrow].Cells[0];
        }

        private void btnStringRem_Click(object sender, EventArgs e)
        {
            string currowname = gvStrings.CurrentRow.Cells[0].Value as string;
            gvStrings.Rows.Remove(gvStrings.CurrentRow);
            gvStrings.AutoResizeColumn(0);
        }
        #endregion


        public delegate void EntityOp(GroundEntity ent);


        //=========================================================================
        //  Entities Tab
        //=========================================================================
        /// <summary>
        /// Assemble a default name for an entity, if the user hasn't supplied one.
        /// </summary>
        /// <param name="id">Number to be appended to the default name, so all entities have a different name.</param>
        /// <returns>The name.</returns>
        private string MakeDefaultEntName()
        {
            string prefix = "NewEntity";
            switch (cmbEntityType.SelectedIndex)
            {
                case (int)GroundEntity.EEntTypes.Character:
                case (int)GroundEntity.EEntTypes.Object:
                case (int)GroundEntity.EEntTypes.Marker:
                case (int)GroundEntity.EEntTypes.Spawner:
                    prefix = String.Format("New{0}", ((GroundEntity.EEntTypes)cmbEntityType.SelectedIndex).ToString());
                    break;
            }
            return prefix;
        }

        /// <summary>
        /// Rebuild the direction combobox to either include or exclude the "None" direction.
        /// Since its invalid for GroundChar to have a "None" direction.
        /// </summary>
        private void ReloadDirections()
        {
            //Clear directions list
            cmbEntityDir.Items.Clear();

            //Pick appropriate direction list
            List<Dir8> dirlist = new List<Dir8>();
            switch (cmbEntityType.SelectedIndex)
            {
                //Entities with non-NONE direction
                case (int)GroundEntity.EEntTypes.Character:
                case (int)GroundEntity.EEntTypes.Spawner:
                    {
                        for (int ii = 0; ii <= (int)Dir8.DownRight; ii++)
                            dirlist.Add((Dir8)ii);
                    }
                    break;
                //Entities that accept NONE as direction
                default:
                    {
                        for (int ii = -1; ii <= (int)Dir8.DownRight; ii++)
                            dirlist.Add((Dir8)ii);
                    }
                    break;
            }

            //Update valid directions
            for (int ii = 0; ii < dirlist.Count; ii++)
            {
                cmbEntityDir.Items.Add(dirlist[ii].ToLocal());
            }

            cmbEntityDir.SelectedIndex = 0;
        }

        /// <summary>
        /// Called whenever the entity type changes.
        /// It mainly reorganizes the displayed tabs, and display what is appropriate for the new entity type.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmbEntityType_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Hide all tabs
            tabctrlEntData.TabPages.Clear();

            //Show the appropriate tabs
            switch (cmbEntityType.SelectedIndex)
            {
                case (int)GroundEntity.EEntTypes.Character:
                    {
                        //Add valid tabs
                        tabctrlEntData.TabPages.Add(tabEntScript);
                        tabctrlEntData.TabPages.Add(tabEntCharData);
                        tabctrlEntData.TabPages.Add(tabEntCharDisplay);

                        btnAddToTemplates.Enabled = true;
                        numEntHeight.Enabled = false;
                        numEntWidth.Enabled = false;
                        break;
                    }
                case (int)GroundEntity.EEntTypes.Object:
                    {
                        //Add valid tabs
                        tabctrlEntData.TabPages.Add(tabEntScript);
                        tabctrlEntData.TabPages.Add(tabEntObjDisplay);

                        btnAddToTemplates.Enabled = true;
                        numEntHeight.Enabled = true;
                        numEntWidth.Enabled = true;
                        break;
                    }
                case (int)GroundEntity.EEntTypes.Marker:
                    {
                        btnAddToTemplates.Enabled = false;
                        numEntHeight.Enabled = false;
                        numEntWidth.Enabled = false;
                        break;
                    }
                case (int)GroundEntity.EEntTypes.Spawner:
                    {
                        tabctrlEntData.TabPages.Add(tabEntSpawner);
                        tabctrlEntData.TabPages.Add(tabEntScript);
                        cmbSpawnerType.SelectedIndex = 0;

                        btnAddToTemplates.Enabled = false;
                        numEntHeight.Enabled = false;
                        numEntWidth.Enabled = false;
                        break;
                    }
                default:
                    {
                        numEntHeight.Enabled = false;
                        numEntWidth.Enabled = false;
                        btnAddToTemplates.Enabled = false;
                        throw new Exception("GroundEditor.cmbEntityType_SelectedIndexChanged(): Invalid entity type!!!");
                    }
            }

            //Update valid directions
            ReloadDirections();

            //Setup the script page
            SetupEntityScriptTab((GroundEntity.EEntTypes)cmbEntityType.SelectedIndex);

            cmbEntTriggerType.SelectedIndex = 0;

            if ( String.IsNullOrEmpty(txtEntityName.Text) )
                txtEntityName.Text = MakeDefaultEntName(); //Set a name by default, if there are no names yet
        }

        /// <summary>
        /// Called whenever the entity direction is changed.
        /// Mainly meant to update the selected entity or the placeable one.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmbEntityDir_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (selectedEntity != null)
                selectedEntity.Direction = GetCurrentEntityDirection();
        }

        private void txtEntityName_Leave(object sender, EventArgs e)
        {
            string resultName = txtEntityName.Text;
            if (selectedEntity != null)
            {
                //We changed the selected entity's name
                selectedEntity.EntName = resultName;
            }
            txtEntityName.Text = resultName;
        }

        /// <summary>
        /// Determine the correct entity direction value to assign to the entity, based on the entity type.
        /// This is meant to handle transparently how GroundChar don't have a "None" direction, while objects and markers do.
        /// </summary>
        /// <returns></returns>
        private Dir8 GetCurrentEntityDirection()
        {
            if (cmbEntityDir.Items.Count == 8) //If the  "None" direction is disabled
            {
                return (Dir8)cmbEntityDir.SelectedIndex;
            }
            else
            {
                return (cmbEntityDir.SelectedIndex > 0) ? (Dir8)(cmbEntityDir.SelectedIndex - 1) : Dir8.None;
            }
        }

        /// <summary>
        /// Returns a validated character specie
        /// </summary>
        /// <returns></returns>
        private int GetCurrentEntityKindID()
        {
            return cmbEntKind.SelectedIndex;
        }

        /// <summary>
        /// Returns a validated character form
        /// </summary>
        /// <returns></returns>
        private int GetCurrentEntityFormID()
        {
            return cmbEntForm.SelectedIndex;
        }

        /// <summary>
        /// Returns a validated MonsterID for the current GroundChar
        /// </summary>
        /// <returns></returns>
        private MonsterID GetCurrentEntityMonsterID()
        {
            if (cmbEntityType.SelectedIndex != (int)GroundEntity.EEntTypes.Character)
                return new MonsterID();

            return new Dungeon.MonsterID(   GetCurrentEntityKindID(),
                                        GetCurrentEntityFormID(),
                                        chkEntRare.Checked ? 1 : 0,
                                        (Gender)cmbEntCharGender.SelectedIndex);
        }

        /// <summary>
        /// Assemble a GroundChar's Character entity from the form's data.
        /// </summary>
        /// <returns></returns>
        private Character MakeCharacterFromEditor()
        {
            CharData chdata = new CharData();
            chdata.Nickname = txtEntCharNickname.Text;
            chdata.BaseForm = GetCurrentEntityMonsterID();
            chdata.Level = (int)numEntCharLevel.Value;
            Character ch = new Character(chdata, null);
            AITactic tactic = DataManager.Instance.GetAITactic(0);
            ch.Tactic = new AITactic(tactic);
            return ch;
        }

        private GroundEntity MakePlaceableEntity()
        {
            GroundEntity placeableEntity = null;
            //We want to assemble the placeable entity
            switch (cmbEntityType.SelectedIndex)
            {
                case (int)GroundEntity.EEntTypes.Character:
                    {
                        GroundChar ch = new GroundChar(MakeCharacterFromEditor(),
                                                         new Loc(0, 0),
                                                         GetCurrentEntityDirection(),
                                                         txtEntityName.Text);
                        GiveEntityCallbacks(ch);
                        ch.Data.BaseForm.Form = cmbEntForm.SelectedIndex;
                        placeableEntity = ch;
                        break;
                    }
                case (int)GroundEntity.EEntTypes.Object:
                    {
                        GroundObject obj = new GroundObject(MakeAnimDataFromEditor(),
                                                           new Rect(0, 0, (int)numEntWidth.Value, (int)numEntHeight.Value),
                                                           GetCurrentTriggerActivationType(),
                                                           txtEntityName.Text);
                        GiveEntityCallbacks(obj);
                        placeableEntity = obj;
                        break;
                    }
                case (int)GroundEntity.EEntTypes.Marker:
                    {
                        placeableEntity =  new GroundMarker(txtEntityName.Text, new Loc(0, 0), GetCurrentEntityDirection());
                        break;
                    }
                case (int)GroundEntity.EEntTypes.Spawner:
                    {
                        GroundSpawner spwn = new GroundSpawner(txtEntityName.Text, txtSpawnedEntName.Text, new Character());
                        spwn.Direction = GetCurrentEntityDirection();
                        GiveEntityCallbacks(spwn);
                        GiveSpawnerSpawnedEntityCallbacks(spwn);
                        placeableEntity = spwn;
                        break;
                    }
            }
            placeableEntity.EntName = txtEntityName.Text;
            placeableEntity.EntEnabled = chkEntEnabled.Checked;
            placeableEntity.Direction = GetCurrentEntityDirection();
            placeableEntity.SetTriggerType(GetCurrentTriggerActivationType());
            return placeableEntity;
        }

        private GroundEntity.EEntityTriggerTypes GetCurrentTriggerActivationType()
        {
            return (GroundEntity.EEntityTriggerTypes)cmbEntTriggerType.SelectedIndex;
        }


        /// <summary>
        /// Select the entity at that position and displays its data for editing
        /// </summary>
        /// <param name="position"></param>
        public void RemoveEntityAt(Loc position)
        {
            OperateOnEntityAt(position, RemoveEntity);
        }

        public void RemoveEntity(GroundEntity ent)
        {
            if (ent == null)
                return;

            if (ent.GetEntityType() == GroundEntity.EEntTypes.Character)
                ZoneManager.Instance.CurrentGround.RemoveMapChar((GroundChar)ent);
            else if (ent.GetEntityType() == GroundEntity.EEntTypes.Object)
                ZoneManager.Instance.CurrentGround.RemoveObject((GroundObject)ent);
            else if (ent.GetEntityType() == GroundEntity.EEntTypes.Marker)
                ZoneManager.Instance.CurrentGround.RemoveMarker((GroundMarker)ent);
            else if (ent.GetEntityType() == GroundEntity.EEntTypes.Spawner)
                ZoneManager.Instance.CurrentGround.RemoveSpawner((GroundSpawner)ent);
        }

        public void PlaceEntity(Loc position)
        {
            GroundEntity placeableEntity = MakePlaceableEntity();

            if (placeableEntity == null)
                return;

            placeableEntity.Position = position;


            placeableEntity.EntName = ZoneManager.Instance.CurrentGround.FindNonConflictingName(placeableEntity.EntName);

            if (placeableEntity.GetEntityType() == GroundEntity.EEntTypes.Character)
                ZoneManager.Instance.CurrentGround.AddMapChar((GroundChar)placeableEntity);
            else if (placeableEntity.GetEntityType() == GroundEntity.EEntTypes.Object)
                ZoneManager.Instance.CurrentGround.AddObject((GroundObject)placeableEntity);
            else if (placeableEntity.GetEntityType() == GroundEntity.EEntTypes.Marker)
                ZoneManager.Instance.CurrentGround.AddMarker((GroundMarker)placeableEntity);
            else if (placeableEntity.GetEntityType() == GroundEntity.EEntTypes.Spawner)
                ZoneManager.Instance.CurrentGround.AddSpawner((GroundSpawner)placeableEntity);

        }

        /// <summary>
        /// Select the entity at that position and displays its data for editing
        /// </summary>
        /// <param name="position"></param>
        public void SelectEntityAt(Loc position)
        {
            OperateOnEntityAt(position, SelectEntity);
        }

        public void OperateOnEntityAt(Loc position, EntityOp op)
        {
            List<GroundEntity> found = ZoneManager.Instance.CurrentGround.FindEntitiesAtPosition(position);
            if (found.Count > 0)
            {
                if (found.Count == 1)
                    op(found.First());
                else if (found.Count > 1)
                {
                    List<MenuItem> items = new List<MenuItem>();
                    MenuItem menuinfo = new MenuItem("Pick one of the overlapping entities :");
                    menuinfo.Enabled = false;
                    items.Add(menuinfo);
                    foreach (GroundEntity e in found)
                    {
                        MenuItem it = new MenuItem(e.EntName,
                                                   new EventHandler(new Action<object, EventArgs>((object sender, EventArgs args) => { op(e); }))
                                                   );
                        items.Add(it);
                    }

                    //Display a little popup menu to pick an entity to select
                    ContextMenu cm = new ContextMenu();
                    cm.MenuItems.AddRange(items.ToArray());
                    cm.Show(this, Cursor.Position);
                }
            }
            else
                op(null);
        }

        /// <summary>
        /// Select the specified entity
        /// </summary>
        /// <param name="ent"></param>
        private void SelectEntity(GroundEntity ent)
        {
            if (selectedEntity != null)
                selectedEntity.DevOnEntityUnSelected();

            selectedEntity = null;

            if (ent != null)
            {
                cmbEntityType.Enabled = false;

                LoadInEntityData(ent);

                selectedEntity = ent;
                selectedEntity.DevOnEntitySelected();
            }
            else
                cmbEntityType.Enabled = true;

        }

        /// <summary>
        /// Deselect the currently selected entity if applicable
        /// </summary>
        private void DeselectEntity()
        {
            SelectEntity(null);
        }

        /// <summary>
        /// Show context menu for this entity.
        /// TODO
        /// </summary>
        /// <param name="position"></param>
        public void EntityContext(Loc mousepos, Loc mappos)
        {
            List<GroundEntity> found = ZoneManager.Instance.CurrentGround.FindEntitiesAtPosition(mappos);
            if (found.Count > 0)
            {
                //Display context menu
            }
        }

        private void MoveEntity(Loc loc)
        {
            if (selectedEntity != null)
            {
                selectedEntity.Bounds = new Rect(loc, selectedEntity.Bounds.Size);
            }
        }

        /// <summary>
        /// Takes the specified entity's data and displays it in the
        /// entity form.
        /// </summary>
        /// <param name="ent"></param>
        private void LoadInEntityData(GroundEntity ent)
        {
            cmbEntityType.SelectedIndex = (int)ent.GetEntityType();
            txtEntityName.Text = ent.EntName;
            cmbEntityDir.SelectedIndex = (int)ent.Direction;
            numEntWidth.Value = ent.Width;
            numEntHeight.Value = ent.Height;
            chkEntEnabled.Checked = ent.EntEnabled;

            switch (ent.GetEntityType())
            {
                case GroundEntity.EEntTypes.Character:
                    {
                        GroundChar ch = (GroundChar)ent;
                        cmbEntKind.SelectedIndex = ch.CurrentForm.Species;
                        cmbEntForm.SelectedIndex = ch.CurrentForm.Form;
                        cmbEntCharGender.SelectedIndex = ch.CurrentForm.Gender != Gender.Unknown ? (int)ch.CurrentForm.Gender : 0;
                        chkEntRare.Checked = ch.CurrentForm.Skin == 0 ? false : true;

                        txtEntCharNickname.Text = ch.Nickname;
                        //numEntCharLevel.Value = ch.Level; // NOTE: level commented out until we find a place where we need it
                        break;
                    }

                case GroundEntity.EEntTypes.Object:
                    {
                        GroundObject obj = (GroundObject)ent;
                        cbEntObjSpriteID.SelectedIndex = objectAnimIndex.FindIndex(x => x == obj.ObjectAnim.AnimIndex);
                        numEntObjStartFrame.Value = obj.ObjectAnim.StartFrame;
                        numEntObjEndFrame.Value = obj.ObjectAnim.EndFrame;
                        numEntObjFrameTime.Value = obj.ObjectAnim.FrameTime;
                        numEntObjAlpha.Value = obj.ObjectAnim.Alpha;
                        break;
                    }
                case GroundEntity.EEntTypes.Spawner:
                    {
                        GroundSpawner spwn = (GroundSpawner)ent;
                        txtSpawnedEntName.Text = spwn.NPCName;
                        cmbSpawnerType.SelectedIndex = 0; //!TODO: If we add new spawner types change this
                        break;
                    }
                case GroundEntity.EEntTypes.Marker: //No callbacks
                default:
                    break;
            }

            //Handle lua callbacks if necessary

            //Activation type set
            cmbEntTriggerType.SelectedIndex = (int)ent.GetTriggerType();

            //Check all active lua callbacks in the list!
            chklstEntScriptCallbacks.ItemCheck -= this.chklstEntScriptCallbacks_ItemCheck;
            foreach (int e in ent.ActiveLuaCallbacks())
            {
                int idx = chklstEntScriptCallbacks.Items.IndexOf(LuaEngine.EntLuaEventTypeNames[e]);
                if (idx != -1)
                    chklstEntScriptCallbacks.SetItemChecked(idx, true);
            }
            chklstEntScriptCallbacks.ItemCheck += this.chklstEntScriptCallbacks_ItemCheck;

            //Extra handling for spawners only
            //(Check events inherited by spawned entity)
            if (ent.GetEntityType() == GroundEntity.EEntTypes.Spawner)
            {
                GroundSpawner spwn = (GroundSpawner)ent;
                chklstScriptSecondaryCallbacks.ItemCheck -= this.chklstScriptSecondaryCallbacks_ItemCheck;
                foreach (int e in spwn.IterateSpawnedEntScriptEvents())
                {
                    int idx = chklstScriptSecondaryCallbacks.Items.IndexOf(LuaEngine.EntLuaEventTypeNames[e]);
                    if (idx != -1)
                        chklstScriptSecondaryCallbacks.SetItemChecked(idx, true);
                }
                chklstScriptSecondaryCallbacks.ItemCheck += this.chklstScriptSecondaryCallbacks_ItemCheck;
            }
        }

        /// <summary>
        /// When the specie field is changed, this is called
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmbEntKind_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Update the placeable entity if possible
            if (selectedEntity != null && selectedEntity.GetEntityType() == GroundEntity.EEntTypes.Character)
            {
                GroundChar ch = (GroundChar)selectedEntity;
                ch.Data.BaseForm.Species = cmbEntKind.SelectedIndex;
            }

            cmbEntForm.Items.Clear();
            for (int ii = 0; ii < DataManager.Instance.GetMonster(cmbEntKind.SelectedIndex).Forms.Count; ii++)
            {
                cmbEntForm.Items.Add(ii + " - " + DataManager.Instance.GetMonster(cmbEntKind.SelectedIndex).Forms[ii].FormName.ToLocal());
            }
            cmbEntForm.SelectedIndex = 0;
        }

        /// <summary>
        /// Updates the templates list for the currently selected kind of template
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmbTemplateType_SelectedIndexChanged(object sender, EventArgs e)
        {
            lstTemplates.Items.Clear();
            foreach( var tmpl in TemplateManager.Instance.IterateSpecified((ETemplateType)cmbTemplateType.SelectedIndex) )
            {
                lstTemplates.Items.Add( tmpl.Name );
            }
            lstTemplates.ClearSelected();
        }

        /// <summary>
        /// Assembles an AnimData object for a GroundObject based on the data of the
        /// Object display tab.
        /// </summary>
        /// <returns></returns>
        private Content.ObjAnimData MakeAnimDataFromEditor()
        {
            Content.ObjAnimData dat = new Content.ObjAnimData();

            //Normally, the user would be able to pick from a list of sprites
            dat.AnimDir     = GetCurrentEntityDirection();
            dat.AnimIndex   = objectAnimIndex[cbEntObjSpriteID.SelectedIndex];
            dat.Alpha       = (byte)numEntObjAlpha.Value;
            dat.StartFrame  = (int)numEntObjStartFrame.Value;
            dat.EndFrame    = (int)numEntObjEndFrame.Value;
            dat.FrameTime   = (int)numEntObjFrameTime.Value;

            return dat;
        }

        /// <summary>
        /// Creates a new template based on the entity data inside the editor form.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAddToTemplates_Click(object sender, EventArgs e)
        {
            //!TODO: Depending on the type, add to templates!
            GroundEntity.EEntTypes ty = (GroundEntity.EEntTypes)cmbEntityType.SelectedIndex;

            if( ty == GroundEntity.EEntTypes.Character )
            {
                CharacterTemplate tmpl = new CharacterTemplate();
                tmpl.Name  = txtEntityName.Text;
                tmpl.Chara = MakeCharacterFromEditor();
                TemplateManager.Instance.SetTemplate(tmpl.Name, tmpl);
            }
            else if(ty == GroundEntity.EEntTypes.Object)
            {
                ObjectTemplate tmpl = new ObjectTemplate();
                tmpl.Name = txtEntityName.Text;
                tmpl.Rect = new Rect(0,0, (int)numEntWidth.Value, (int)numEntHeight.Value);
                tmpl.Contact = cmbEntTriggerType.SelectedIndex != 0 ? true : false;
                tmpl.Anim = MakeAnimDataFromEditor();
                TemplateManager.Instance.SetTemplate(tmpl.Name, tmpl);
            }

        }

        /// <summary>
        /// Update the bounds of the selected or placeable entity
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void numEntWidth_ValueChanged(object sender, EventArgs e)
        {
            //Update the placeable entity if possible
            if (selectedEntity != null && (selectedEntity.GetEntityType() == GroundEntity.EEntTypes.Character || selectedEntity.GetEntityType() == GroundEntity.EEntTypes.Object))
            {
                Rect bounds = selectedEntity.Bounds;
                bounds.Width = (int)numEntWidth.Value;
                selectedEntity.Bounds = bounds;
            }
        }

        /// <summary>
        /// Update the bounds of the selected or placeable entity
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void numEntHeight_ValueChanged(object sender, EventArgs e)
        {
            //Update the placeable entity if possible
            if(selectedEntity != null && (selectedEntity.GetEntityType() == GroundEntity.EEntTypes.Character || selectedEntity.GetEntityType() == GroundEntity.EEntTypes.Object))
            {
                Rect bounds = selectedEntity.Bounds;
                bounds.Height = (int)numEntHeight.Value;
                selectedEntity.Bounds = bounds;
            }

        }

        /// <summary>
        /// Change the form of the selected or placeable entity
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmbEntForm_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Update the placeable entity if possible
            if (selectedEntity != null && selectedEntity.GetEntityType() == GroundEntity.EEntTypes.Character)
            {
                GroundChar ch = (GroundChar)selectedEntity;
                ch.Data.BaseForm.Form = cmbEntForm.SelectedIndex;
            }
        }


        #region ENTITY_SCRIPT_TAB
        //===========================================
        // Entity Script Tab
        //===========================================

        /// <summary>
        /// Assign the curently selected callbacks in the editor form, to the specified character
        /// </summary>
        /// <param name="ch"></param>
        private void GiveEntityCallbacks(GroundEntity ch)
        {
            for (int i = 0; i < chklstEntScriptCallbacks.Items.Count; ++i)
            {
                int idx = LuaEngine.EntLuaEventTypeNames.IndexOf((string)chklstEntScriptCallbacks.Items[i]);
                if (chklstEntScriptCallbacks.GetItemChecked(i))
                    ch.AddScriptEvent((LuaEngine.EEntLuaEventTypes)idx);
                else
                    ch.RemoveScriptEvent((LuaEngine.EEntLuaEventTypes)idx);
            }
        }


        /// <summary>
        /// Assign the curently selected callbacks in the editor form, to the specified spawner
        /// </summary>
        /// <param name="spwn"></param>
        private void GiveSpawnerSpawnedEntityCallbacks(GroundSpawner spwn)
        {
            for (int i = 0; i < chklstScriptSecondaryCallbacks.Items.Count; ++i)
            {
                int idx = LuaEngine.EntLuaEventTypeNames.IndexOf((string)chklstScriptSecondaryCallbacks.Items[i]);
                if (chklstScriptSecondaryCallbacks.GetItemChecked(i))
                    spwn.AddSpawnedEntScriptEvent((LuaEngine.EEntLuaEventTypes)idx);
                else
                    spwn.RemoveSpawnedEntScriptEvent((LuaEngine.EEntLuaEventTypes)idx);
            }
        }

        /// <summary>
        /// Based on the entity type and the trigger type, update the list of available callbacks for this entity
        /// </summary>
        /// <param name="curentty"></param>
        private void SetupEntityScriptCallbacks()
        {
            //First add the ones based on the Activation type
            chklstEntScriptCallbacks.Items.Clear();
            chklstScriptSecondaryCallbacks.Items.Clear();
            switch ((GroundEntity.EEntityTriggerTypes)cmbEntTriggerType.SelectedIndex)
            {
                case GroundEntity.EEntityTriggerTypes.None:
                    {
                        break;
                    }
                case GroundEntity.EEntityTriggerTypes.Action:
                    {
                        chklstEntScriptCallbacks.Items.Add(LuaEngine.ActionFun);
                        break;
                    }
                case GroundEntity.EEntityTriggerTypes.Touch:
                    {
                        chklstEntScriptCallbacks.Items.Add(LuaEngine.TouchFun);
                        break;
                    }
                default:
                    {
                        break;
                    }
            }


            //Then add any extra ones that comes with the entity type
            switch (cmbEntityType.SelectedIndex)
            {
                case (int)GroundEntity.EEntTypes.Character:
                    {
                        chklstEntScriptCallbacks.Items.Add(LuaEngine.ThinkFun);
                        break;
                    }
                case (int)GroundEntity.EEntTypes.Object:
                    {
                        chklstEntScriptCallbacks.Items.Add(LuaEngine.UpdateFun);
                        break;
                    }
                case (int)GroundEntity.EEntTypes.Spawner:
                    {
                        chklstEntScriptCallbacks.Items.Add(LuaEngine.EntSpawnedFun);
                        chklstScriptSecondaryCallbacks.Items.Add(LuaEngine.ActionFun);
                        chklstScriptSecondaryCallbacks.Items.Add(LuaEngine.ThinkFun);
                        break;
                    }
                case (int)GroundEntity.EEntTypes.Marker:
                default:
                    {
                        break;
                    }
            }
        }

        /// <summary>
        /// Depending on the entity type, updates the Trigger type combo box with the possible ways to interact with an entity
        /// </summary>
        /// <param name="curentty"></param>
        private void SetupEntityScriptTab(GroundEntity.EEntTypes curentty)
        {
            cmbEntTriggerType.Items.Clear();
            cmbEntTriggerType.Items.Add(GroundEntity.EEntityTriggerTypes.None.ToLocal());
            switch (curentty)
            {
                case GroundEntity.EEntTypes.Character:
                    {
                        cmbEntTriggerType.Enabled = true;
                        cmbEntTriggerType.Items.Add(GroundEntity.EEntityTriggerTypes.Action.ToLocal());
                        ToggleVisibleSecondaryEntityScriptCallbacks(false);
                        break;
                    }
                case GroundEntity.EEntTypes.Object:
                    {
                        cmbEntTriggerType.Enabled = true;
                        cmbEntTriggerType.Items.Add(GroundEntity.EEntityTriggerTypes.Action.ToLocal());
                        cmbEntTriggerType.Items.Add(GroundEntity.EEntityTriggerTypes.Touch.ToLocal());
                        ToggleVisibleSecondaryEntityScriptCallbacks(false);
                        break;
                    }
                case GroundEntity.EEntTypes.Spawner:
                    {
                        cmbEntTriggerType.Enabled = false;
                        ToggleVisibleSecondaryEntityScriptCallbacks(true);
                        break;
                    }
                case GroundEntity.EEntTypes.Marker:
                default:
                    {
                        cmbEntTriggerType.Enabled = false;
                        ToggleVisibleSecondaryEntityScriptCallbacks(false);
                        break;
                    }
            }
        }

        /// <summary>
        /// Set whether the secondary list of entity callbacks should be displayed on the script tab for the current entity
        /// </summary>
        /// <param name="state"></param>
        private void ToggleVisibleSecondaryEntityScriptCallbacks(bool state)
        {
            lblScriptSecondaryCallbacks.Visible = state;
            lblScriptSecondaryCallbacks.Enabled = state;
            chklstScriptSecondaryCallbacks.Enabled = state;
            chklstScriptSecondaryCallbacks.Visible = state;
        }

        /// <summary>
        /// Called when the activation trigger is changed for an object
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmbEntTriggerType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (selectedEntity != null)
                selectedEntity.SetTriggerType(GetCurrentTriggerActivationType());

            //Update the placeable entity if possible
            SetupEntityScriptCallbacks();
        }

        private void chklstEntScriptCallbacks_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (selectedEntity != null && chklstEntScriptCallbacks.SelectedItem != null)
            {
                GiveEntityCallbacks(selectedEntity);
            }
        }


        private void chklstScriptSecondaryCallbacks_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (selectedEntity != null && selectedEntity.GetEntityType() == GroundEntity.EEntTypes.Spawner && e.Index != -1)
            {
                GiveSpawnerSpawnedEntityCallbacks((GroundSpawner)selectedEntity);
            }
        }

        #endregion


        private void txtSpawnedEntName_TextChanged(object sender, EventArgs e)
        {
            if (selectedEntity != null && selectedEntity.GetEntityType() == GroundEntity.EEntTypes.Spawner)
            {
                GroundSpawner spwn = (GroundSpawner)selectedEntity;
                spwn.NPCName = txtSpawnedEntName.Text;
            }
        }

        #region TOOLBAR
        //===========================================
        // TOOLBAR
        //===========================================


        private void rbEntSelect_CheckedChanged(object sender, EventArgs e)
        {
            ChangeEditorMode(EntEditMode.SelectEntity);
        }

        private void rbEntPlace_CheckedChanged(object sender, EventArgs e)
        {
            ChangeEditorMode(EntEditMode.PlaceEntity);
        }

        private void rbEntMove_CheckedChanged(object sender, EventArgs e)
        {
            ChangeEditorMode(EntEditMode.MoveEntity);
        }

        #endregion

        private void cbEntObjSpriteID_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (selectedEntity != null && selectedEntity.GetEntityType() == GroundEntity.EEntTypes.Object)
            {
                GroundObject obj = (GroundObject)selectedEntity;
                obj.ObjectAnim = MakeAnimDataFromEditor();
            }
        }

        private void chkEntEnabled_CheckedChanged(object sender, EventArgs e)
        {
            if (selectedEntity != null)
                selectedEntity.EntEnabled = chkEntEnabled.Checked;
        }


        private static bool ComparePaths(string path1, string path2)
        {
            return String.Compare(Path.GetFullPath(path1).TrimEnd('\\'),
                Path.GetFullPath(path2).TrimEnd('\\'),
                StringComparison.InvariantCultureIgnoreCase) == 0;
        }

        #region BLOCKS

        private void rbBlockDraw_CheckedChanged(object sender, EventArgs e)
        {
            BlockMode = TileEditMode.Draw;
        }

        private void rbBlockRectangle_CheckedChanged(object sender, EventArgs e)
        {
            BlockMode = TileEditMode.Rectangle;
        }

        private void rbBlockFill_CheckedChanged(object sender, EventArgs e)
        {
            BlockMode = TileEditMode.Fill;
        }

        #endregion

    }
}
