using System;

using System.Drawing;
using System.Windows.Forms;
using System.IO;
using Microsoft.Win32;
using RogueEssence.Dungeon;
using RogueElements;
using RogueEssence.Content;
using System.Collections.Generic;

namespace RogueEssence.Dev
{
    public class TileBrush
    {
        public TileLayer Layer;
        public Loc MultiSelect;

        public TileBrush(TileLayer layer, Loc multiSelect)
        {
            Layer = layer;
            MultiSelect = multiSelect;
        }
    }

    public partial class TileBrowser : UserControl
    {
        bool inAnimMode;
        string currentTileset;
        string chosenTileset;
        Loc chosenTile;
        Loc multiSelect;
        int tileSize;
        public TileEditMode TexMode { get; private set; }
        public string CurrentTileset { get { return currentTileset; } }

        List<string> tileIndices;

        public delegate void RefreshCallback();

        public TileBrowser()
        {
            inAnimMode = false;
            chosenTile = Loc.Zero;
            multiSelect = Loc.One;
            tileIndices = new List<string>();

            InitializeComponent();
            nudFrameLength.Maximum = Int32.MaxValue;

            slbTilesets.SetName("Tilesets");
        }

        public TileBrush GetBrush()
        {
            return new TileBrush(tilePreview.GetChosenAnim(), multiSelect);
        }

        public void SetBrush(TileLayer anim)
        {
            if (anim.Frames.Count > 0)
            {
                chosenTileset = anim.Frames[0].Sheet;
                currentTileset = chosenTileset;
                chosenTile = anim.Frames[0].TexLoc;
                multiSelect = Loc.One;
                inAnimMode = (anim.Frames.Count > 1);

                tilePreview.SetChosenAnim(new TileBrush(anim, multiSelect));

                //refresh
                RefreshTileSelect();
            }
        }

        public void SelectTileset(string sheetName)
        {
            slbTilesets.SearchText = "";
            slbTilesets.SelectedIndex = tileIndices.FindIndex(str => (str == sheetName));
        }

        public void SetTileSize(int tileSize)
        {
            this.tileSize = tileSize;
            UpdateTilesList();
        }

        public void UpdateTilesList()
        {
            tileIndices.Clear();
            slbTilesets.Clear();

            foreach (string name in GraphicsManager.TileIndex.Nodes.Keys)
            {
                if (GraphicsManager.TileIndex.GetTileSize(name) == tileSize)
                {
                    tileIndices.Add(name);
                    slbTilesets.AddItem(name);
                }
            }

            if (tileIndices.Count > 0)
            {

                chosenTileset = tileIndices[0];
                currentTileset = tileIndices[0];

                slbTilesets.SelectedIndex = 0;
            }
            else
            {
                chosenTileset = "";
                currentTileset = "";
            }
            chosenTile = Loc.Zero;
            multiSelect = Loc.One;

            RefreshAnimControls();

            tilePreview.SetChosenAnim(new TileBrush(new TileLayer(chosenTile, chosenTileset), multiSelect));

            //refresh
            RefreshTileSelect();


        }

        void RefreshTileset()
        {
            RefreshScrollMaximums();

            RefreshTilesetPic();
        }


        void RefreshTilesetPic()
        {
            int picX = picTileset.Size.Width / tileSize;
            int picY = picTileset.Size.Height / tileSize;

            Loc tilePos = GraphicsManager.TileIndex.GetTileDims(currentTileset);

            if (inAnimMode)
                lblTileInfo.Visible = false;
            else
            {
                lblTileInfo.Visible = true;
                lblTileInfo.Text = "X:" + chosenTile.X + "  Y:" + chosenTile.Y + "  Set:" + chosenTileset;
                if (multiSelect != Loc.One)
                    lblTileInfo.Text += "\n" + multiSelect.X + "x" + multiSelect.Y;
            }

            if (tilePos.X > 0 && tilePos.Y > 0)
            {
                picTileset.Visible = true;

                Image endImage = new Bitmap(picTileset.Width, picTileset.Height);
                using (System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(endImage))
                {
                    int width = picX;
                    if (tilePos.X - hScroll.Value < width)
                        width = tilePos.X - hScroll.Value;

                    int height = picY;
                    if (tilePos.Y - vScroll.Value < height)
                        height = tilePos.Y - vScroll.Value;

                    for (int x = 0; x < width; x++)
                    {
                        for (int y = 0; y < height; y++)
                        {
                            Bitmap img = DevTileManager.Instance.GetTile(new TileFrame(new Loc(x + hScroll.Value, y + vScroll.Value), currentTileset));
                            if (img != null)
                                graphics.DrawImage(img, x * tileSize, y * tileSize);
                        }
                    }

                    //draw red square
                    if (currentTileset == chosenTileset && Collision.Collides(new Rect(hScroll.Value, vScroll.Value, picX, picY), new Rect(chosenTile, multiSelect)))
                    {
                        graphics.DrawRectangle(new Pen(Color.Red, 2), new Rectangle((chosenTile.X - hScroll.Value) * tileSize + 1,
                            (chosenTile.Y - vScroll.Value) * tileSize + 1,
                            tileSize * multiSelect.X - 2, tileSize * multiSelect.Y - 2));
                    }
                }
                picTileset.Image = endImage;
            }
            else
            {
                picTileset.Visible = false;
            }
        }

        void RefreshTileSelect()
        {
            chkAnimationMode.Checked = inAnimMode;
            slbTilesets.SearchText = "";
            slbTilesets.SelectedIndex = tileIndices.FindIndex(str => (str == currentTileset));

            RefreshScrollMaximums();

            int picX = picTileset.Size.Width / tileSize;
            int picY = picTileset.Size.Height / tileSize;

            bool refreshedPic = false;

            //check if selected tile is within current scroll view
            if (chosenTile.X < hScroll.Value || chosenTile.X >= hScroll.Value + picX)
            {
                //if not, move it within scroll view
                int newVal = chosenTile.X - picX / 2;
                refreshedPic |= (hScroll.Value == newVal);
                hScroll.Value = Math.Max(newVal, 0);
            }

            //check if selected tile is within current scroll view
            if (chosenTile.Y < vScroll.Value || chosenTile.Y >= vScroll.Value + picY)
            {
                //if not, move it within scroll view
                int newVal = chosenTile.Y - picY / 2;
                refreshedPic |= (vScroll.Value == newVal);
                vScroll.Value = Math.Max(newVal, 0);
            }

            if (!refreshedPic)
                RefreshTilesetPic();

        }

        void RefreshScrollMaximums()
        {
            int picX = picTileset.Size.Width / tileSize;
            int picY = picTileset.Size.Height / tileSize;

            Loc tileDims = GraphicsManager.TileIndex.GetTileDims(currentTileset);

            if (tileDims.X - picX <= 0)
            {
                hScroll.Maximum = 0;
                hScroll.Enabled = false;
            }
            else
            {
                hScroll.Enabled = true;
                hScroll.Maximum = tileDims.X - picX + 1;
            }

            if (tileDims.Y - picY <= 0)
            {
                vScroll.Maximum = 0;
                vScroll.Enabled = false;
            }
            else
            {
                vScroll.Enabled = true;
                vScroll.Maximum = tileDims.Y - picY + 1;
            }
        }

        void RefreshChosenTile()
        {
            Loc tileDims = GraphicsManager.TileIndex.GetTileDims(currentTileset);

            if (chosenTile.X >= tileDims.X)
                chosenTile.X = tileDims.X - 1;
            if (chosenTile.Y >= tileDims.Y)
                chosenTile.Y = tileDims.Y - 1;

            if (chosenTile.X + multiSelect.X > tileDims.X)
                multiSelect.X = tileDims.X - chosenTile.X;
            if (chosenTile.Y + multiSelect.Y > tileDims.Y)
                multiSelect.Y = tileDims.Y - chosenTile.Y;
        }


        void RefreshAnimControls()
        {
            bool show = inAnimMode;

            pnlAnimation.Visible = show;
            nudFrameLength.Value = tilePreview.GetChosenAnim().FrameLength;
        }


        void UpdateAnimFrames()
        {
            TileLayer chosenAnim = tilePreview.GetChosenAnim();
            int selection = lbxFrames.SelectedIndex;
            lbxFrames.Items.Clear();
            for (int ii = 0; ii < chosenAnim.Frames.Count; ii++)
            {
                lbxFrames.Items.Add(chosenAnim.Frames[ii].ToString());
            }
            if (selection < lbxFrames.Items.Count)
                lbxFrames.SelectedIndex = selection;

        }



        private void vScroll_Scroll(object sender, ScrollEventArgs e)
        {
            RefreshTilesetPic();
        }

        private void hScroll_Scroll(object sender, ScrollEventArgs e)
        {
            RefreshTilesetPic();
        }


        private void picTileset_Click(object sender, EventArgs e)
        {
            MouseEventArgs args = e as MouseEventArgs;

            int clickedX = args.X / tileSize + hScroll.Value;
            int clickedY = args.Y / tileSize + vScroll.Value;

            if (GraphicsManager.TileIndex.GetPosition(currentTileset, new Loc(clickedX, clickedY)) > 0)
            {
                chosenTileset = currentTileset;

                if ((Control.ModifierKeys & Keys.Shift) != 0)
                {
                    chkAnimationMode.Checked = false;

                    //multiselect
                    Loc endTile = new Loc(clickedX, clickedY);
                    multiSelect = endTile - chosenTile + Loc.One;
                    RefreshTilesetPic();

                    tilePreview.SetChosenAnim(new TileBrush(new TileLayer(chosenTile, chosenTileset), multiSelect));
                    
                }
                else
                {
                    chosenTile = new Loc(clickedX, clickedY);
                    multiSelect = Loc.One;
                    RefreshTilesetPic();

                    if (!inAnimMode)
                        tilePreview.SetChosenAnim(new TileBrush(new TileLayer(chosenTile, chosenTileset), multiSelect));
                    else
                    {
                        TileLayer chosenAnim = tilePreview.GetChosenAnim();
                        if (lbxFrames.SelectedIndex > -1)
                            chosenAnim.Frames[lbxFrames.SelectedIndex] = new TileFrame(chosenTile, chosenTileset);
                        else
                            chosenAnim.Frames.Add(new TileFrame(chosenTile, chosenTileset));
                        tilePreview.SetChosenAnim(new TileBrush(chosenAnim, multiSelect));
                        UpdateAnimFrames();
                    }
                }
            }
        }

        private void chkAnimationMode_CheckedChanged(object sender, EventArgs e)
        {
            if (inAnimMode != chkAnimationMode.Checked)
            {
                inAnimMode = chkAnimationMode.Checked;
                if (!inAnimMode)
                {
                    multiSelect = Loc.One;
                    tilePreview.SetChosenAnim(new TileBrush(new TileLayer(chosenTile, chosenTileset), multiSelect));
                }
            }
            UpdateAnimFrames();
            RefreshTileSelect();

            RefreshAnimControls();
        }

        private void nudFrameLength_TextChanged(object sender, EventArgs e)
        {
            int frames = nudFrameLength.GetIntFromNumeric();
            TileLayer chosenAnim = tilePreview.GetChosenAnim();
            chosenAnim.FrameLength = frames;
            tilePreview.SetChosenAnim(new TileBrush(chosenAnim, multiSelect));
        }

        private void btnAddFrame_Click(object sender, EventArgs e)
        {
            TileLayer chosenAnim = tilePreview.GetChosenAnim();
            if (lbxFrames.SelectedIndex > -1)
                chosenAnim.Frames.Insert(lbxFrames.SelectedIndex, new TileFrame(chosenTile, chosenTileset));
            else
                chosenAnim.Frames.Add(new TileFrame(chosenTile, chosenTileset));
            tilePreview.SetChosenAnim(new TileBrush(chosenAnim, multiSelect));

            UpdateAnimFrames();
        }

        private void btnRemoveFrame_Click(object sender, EventArgs e)
        {
            TileLayer chosenAnim = tilePreview.GetChosenAnim();
            if (chosenAnim.Frames.Count > 1)
            {
                if (lbxFrames.SelectedIndex > -1)
                    chosenAnim.Frames.RemoveAt(lbxFrames.SelectedIndex);
                else
                    chosenAnim.Frames.RemoveAt(lbxFrames.Items.Count - 1);
                tilePreview.SetChosenAnim(new TileBrush(chosenAnim, multiSelect));
            }

            UpdateAnimFrames();
        }

        private void lbxFrames_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lbxFrames.SelectedIndex > -1)
            {
                TileLayer chosenAnim = tilePreview.GetChosenAnim();
                chosenTileset = chosenAnim.Frames[lbxFrames.SelectedIndex].Sheet;
                currentTileset = chosenTileset;
                multiSelect = Loc.One;
                chosenTile = chosenAnim.Frames[lbxFrames.SelectedIndex].TexLoc;
                RefreshTileSelect();
            }
        }

        private void slbTilesets_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (slbTilesets.SelectedIndex > -1)
            {
                currentTileset = tileIndices[slbTilesets.InternalIndex];
                multiSelect = Loc.One;

                RefreshTileset();
            }
        }

        private void TileBrowser_Load(object sender, EventArgs e)
        {
            if (Site != null && Site.DesignMode)
                return;

            string tileDir = (string)Registry.GetValue(DiagManager.REG_PATH, "TileDir", "");
            if (String.IsNullOrEmpty(tileDir))
                tileDir = Directory.GetCurrentDirectory();
            openTileFileDialog.InitialDirectory = tileDir;
            saveTileFileDialog.InitialDirectory = tileDir;


            UpdateTilesList();
        }


        private void rbDraw_CheckedChanged(object sender, EventArgs e)
        {
            TexMode = TileEditMode.Draw;
        }

        private void rbRectangle_CheckedChanged(object sender, EventArgs e)
        {
            TexMode = TileEditMode.Rectangle;
        }

        private void rbFill_CheckedChanged(object sender, EventArgs e)
        {
            TexMode = TileEditMode.Fill;
        }

        private void rbEyedrop_CheckedChanged(object sender, EventArgs e)
        {
            TexMode = TileEditMode.Eyedrop;
        }
    }
}
