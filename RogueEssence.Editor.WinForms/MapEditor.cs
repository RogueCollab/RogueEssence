using System;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using Microsoft.Win32;
using RogueEssence.Dev;
using RogueEssence.Dungeon;
using RogueElements;
using RogueEssence.Data;
using System.Collections.Generic;

namespace RogueEssence.Dev
{
    public partial class MapEditor : Form, IMapEditor
    {

        public bool Active { get; private set; }

        private string CurrentFile;

        public bool ShowDataLayer;

        public IMapEditor.TileEditMode Mode { get; private set; }

        public MapEditor()
        {
            SetupLayerVisibility();

            InitializeComponent();

        }


        private IEnumerator<YieldInstruction> DoNew()
        {
            //take all the necessary steps before and after moving to the map

            DiagManager.Instance.LoadMsg = "Loading Map...";
            DevForm.EnterLoadPhase(GameBase.LoadPhase.Content);

            GameManager.Instance.ForceReady();

            ZoneManager.Instance.CurrentZone.DevNewMap();


            ZoneManager.Instance.CurrentMap.EnterMap(DataManager.Instance.Save.ActiveTeam, ZoneManager.Instance.CurrentMap.EntryPoints[0]);

            DungeonScene.Instance.ResetFloor();


            RefreshTitle();
            LoadMapProperties();
            SetupLayerVisibility();
            DevForm.EnterLoadPhase(GameBase.LoadPhase.Ready);

            yield break;
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CurrentFile = "";

            //Schedule the map creation
            DungeonScene.Instance.PendingDevEvent = DoNew();
        }


        private IEnumerator<YieldInstruction> DoLoad(string mapName)
        {
            //take all the necessary steps before and after moving to the map

            DiagManager.Instance.LoadMsg = "Loading Map...";
            DevForm.EnterLoadPhase(GameBase.LoadPhase.Content);
            
            GameManager.Instance.ForceReady();

            ZoneManager.Instance.CurrentZone.DevLoadMap(mapName);

            
            ZoneManager.Instance.CurrentMap.EnterMap(DataManager.Instance.Save.ActiveTeam, ZoneManager.Instance.CurrentMap.EntryPoints[0]);
            
            DungeonScene.Instance.ResetFloor();
            

            RefreshTitle();
            LoadMapProperties();
            SetupLayerVisibility();
            DevForm.EnterLoadPhase(GameBase.LoadPhase.Ready);

            yield break;
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result = openMapFileDialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                CurrentFile = openMapFileDialog.FileName;
                //Schedule the map load
                DungeonScene.Instance.PendingDevEvent = DoLoad(Path.GetFileNameWithoutExtension(openMapFileDialog.FileName));

            }
        }


        private IEnumerator<YieldInstruction> DoSave(Map currentMap, string filePath)
        {
            DataManager.SaveData(filePath, currentMap);
            //NOTE: Status effect refs are unaccounted for here!!
            //TODO: fix these
            //Not high priority because it is easy to just avoid adding targeted status effects in the first place...
            //just don't add them for now!

            RefreshTitle();
            yield break;
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result = saveMapFileDialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                if (!ComparePaths(Directory.GetCurrentDirectory() + "/" + DataManager.MAP_PATH, Path.GetDirectoryName(saveMapFileDialog.FileName)))
                    MessageBox.Show(String.Format("Map can only be saved to {0}!", Directory.GetCurrentDirectory() + "/" + DataManager.MAP_PATH), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else
                {
                    CurrentFile = saveMapFileDialog.FileName;
                    DungeonScene.Instance.PendingDevEvent = DoSave(ZoneManager.Instance.CurrentMap, CurrentFile);
                }
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CurrentFile == "")
                saveAsToolStripMenuItem_Click(sender, e); //Since its the same thing, might as well re-use the function! It makes everyone's lives easier!
            else
                DungeonScene.Instance.PendingDevEvent = DoSave(ZoneManager.Instance.CurrentMap, CurrentFile);

        }

        void RefreshTitle()
        {
            if (String.IsNullOrWhiteSpace(CurrentFile))
                CurrentFile = "";

            if (CurrentFile == "")
                this.Text = "New Map";
            else
            {
                string[] fileEnd = CurrentFile.Split('/');
                this.Text = fileEnd[fileEnd.Length - 1];
            }
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
                if (song == ZoneManager.Instance.CurrentMap.Music)
                    lbxMusic.SelectedIndex = ii + 1;
            }
            if (lbxMusic.SelectedIndex < 0)
                lbxMusic.SelectedIndex = 0;
        }

        private void LoadMapProperties()
        {
            txtMapName.Text = ZoneManager.Instance.CurrentMap.Name.DefaultText;

            cbTileSight.SelectedIndex = (int)ZoneManager.Instance.CurrentMap.TileSight;
            cbCharSight.SelectedIndex = (int)ZoneManager.Instance.CurrentMap.CharSight;

            for (int ii = 0; ii < lbxMusic.Items.Count; ii++)
            {
                string song = (string)lbxMusic.Items[ii];
                if (song == ZoneManager.Instance.CurrentMap.Music)
                {
                    lbxMusic.SelectedIndex = ii;
                    break;
                }
            }
        }

        void SetupLayerVisibility()
        {

            ShowDataLayer = false;
        }

        private void MapEditor_Load(object sender, EventArgs e)
        {

            RefreshTitle();

            string mapDir = (string)Registry.GetValue(DiagManager.REG_PATH, "MapDir", "");
            if (String.IsNullOrEmpty(mapDir))
                mapDir = Directory.GetCurrentDirectory() + "/" + DataManager.MAP_PATH;
            openMapFileDialog.InitialDirectory = mapDir;
            saveMapFileDialog.InitialDirectory = mapDir;

            chkFill.Checked = Mode == IMapEditor.TileEditMode.Fill;
            chkTexEyeDropper.Checked = Mode == IMapEditor.TileEditMode.Eyedrop;
            nudTimeLimit.Maximum = Int32.MaxValue;

            for (int ii = 0; ii < 4; ii++)
            {
                cbTileSight.Items.Add(((Map.SightRange)ii).ToString());
                cbCharSight.Items.Add(((Map.SightRange)ii).ToString());
            }

            ReloadMusic();

            LoadMapProperties();

            Active = true;
        }

        private void MapEditor_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void MapEditor_FormClosed(object sender, FormClosedEventArgs e)
        {
            Active = false;
        }


        private void btnReloadSongs_Click(object sender, EventArgs e)
        {
            ReloadMusic();
        }

        private void txtMapName_TextChanged(object sender, EventArgs e)
        {
            ZoneManager.Instance.CurrentMap.Name.DefaultText = txtMapName.Text;
        }

        private void cbTileSight_SelectedIndexChanged(object sender, EventArgs e)
        {
            ZoneManager.Instance.CurrentMap.TileSight = (Map.SightRange)cbTileSight.SelectedIndex;
        }

        private void cbCharSight_SelectedIndexChanged(object sender, EventArgs e)
        {
            ZoneManager.Instance.CurrentMap.CharSight = (Map.SightRange)cbCharSight.SelectedIndex;
        }
        
        private void lbxMusic_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lbxMusic.SelectedIndex <= 0)
            {
                ZoneManager.Instance.CurrentMap.Music = "";
            }
            else
            {
                string fileName = (string)lbxMusic.Items[lbxMusic.SelectedIndex];
                ZoneManager.Instance.CurrentMap.Music = fileName;
            }

            GameManager.Instance.BGM(ZoneManager.Instance.CurrentMap.Music, true);
        }

        private void resizeMapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MapResizeWindow window = new MapResizeWindow(ZoneManager.Instance.CurrentMap.Width, ZoneManager.Instance.CurrentMap.Height);

            if (window.ShowDialog() == DialogResult.OK)
            {
                DiagManager.Instance.LoadMsg = "Resizing Map...";
                DevForm.EnterLoadPhase(GameBase.LoadPhase.Content);

                //TODO: move this into map class
                Grid.LocAction changeOp = (Loc effectLoc) => { ZoneManager.Instance.CurrentMap.Tiles[effectLoc.X][effectLoc.Y].Effect.UpdateTileLoc(effectLoc); };
                Grid.LocAction newOp = (Loc effectLoc) => { ZoneManager.Instance.CurrentMap.Tiles[effectLoc.X][effectLoc.Y] = new Tile(0, effectLoc); };

                Loc diff = Grid.ResizeJustified<Tile>(ref ZoneManager.Instance.CurrentMap.Tiles, window.MapWidth, window.MapHeight, window.ResizeDir.Reverse(), changeOp, newOp);
                
                foreach(Character character in ZoneManager.Instance.CurrentMap.IterateCharacters())
                {
                    Loc newLoc = character.CharLoc + diff;
                    if (newLoc.X < 0)
                        newLoc.X = 0;
                    else if (newLoc.X >= window.MapWidth)
                        newLoc.X = window.MapWidth - 1;
                    if (newLoc.Y < 0)
                        newLoc.Y = 0;
                    else if (newLoc.Y >= window.MapHeight)
                        newLoc.Y = window.MapHeight - 1;

                    character.CharLoc = newLoc;
                    character.UpdateFrame();
                    ZoneManager.Instance.CurrentMap.UpdateExploration(character);
                }

                DevForm.EnterLoadPhase(GameBase.LoadPhase.Ready);
            }
        }

        private void chkTexEyeDropper_CheckedChanged(object sender, EventArgs e)
        {
            if (chkTexEyeDropper.Checked)
            {
                chkFill.Checked = false;
                Mode = IMapEditor.TileEditMode.Eyedrop;
            }
            else
            {
                Mode = IMapEditor.TileEditMode.Draw;
            }
        }

        private void chkFill_CheckedChanged(object sender, EventArgs e)
        {
            if (chkFill.Checked)
            {
                chkTexEyeDropper.Checked = false;
                Mode = IMapEditor.TileEditMode.Fill;
            }
            else
            {
                Mode = IMapEditor.TileEditMode.Draw;
            }
        }

        public void PaintTile(Loc loc, TileLayer anim)
        {
            if (!Collision.InBounds(ZoneManager.Instance.CurrentMap.Width, ZoneManager.Instance.CurrentMap.Height, loc))
                return;


        }

        public TileLayer GetBrush()
        {
            return tileBrowser.GetBrush().Layer;
        }

        public void EyedropTile(Loc loc)
        {
            if (!Collision.InBounds(ZoneManager.Instance.CurrentMap.Width, ZoneManager.Instance.CurrentMap.Height, loc))
                return;

        }


        public void FillTile(Loc loc, TileLayer anim)
        {
            if (!Collision.InBounds(ZoneManager.Instance.CurrentMap.Width, ZoneManager.Instance.CurrentMap.Height, loc))
                return;

        }

        private static bool ComparePaths(string path1, string path2)
        {
            return String.Compare(Path.GetFullPath(path1).TrimEnd('\\'),
                Path.GetFullPath(path2).TrimEnd('\\'),
                StringComparison.InvariantCultureIgnoreCase) == 0;
        }
    }
}
