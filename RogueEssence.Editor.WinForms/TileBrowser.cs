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
    public partial class TileBrowser : UserControl
    {
        bool inAnimMode;
        string currentTileset;
        string chosenTileset;
        Loc chosenTile;
        public TileEditMode TexMode { get; private set; }

        List<string> tileIndices;

        public delegate void RefreshCallback();

        public TileBrowser()
        {
            inAnimMode = false;
            chosenTile = new Loc();
            tileIndices = new List<string>();

            InitializeComponent();

            slbTilesets.SetName("Tilesets");
        }

        public TileLayer GetBrush()
        {
            return tilePreview.GetChosenAnim();
        }

        public void SetBrush(TileLayer anim)
        {
            if (anim.Frames.Count > 0)
            {
                chosenTileset = anim.Frames[0].Sheet;
                currentTileset = chosenTileset;
                chosenTile = anim.Frames[0].TexLoc;

                inAnimMode = (anim.Frames.Count > 1);

                tilePreview.SetChosenAnim(anim);

                //refresh
                RefreshTileSelect();
            }
        }

        public void UpdateTilesList()
        {
            tileIndices.Clear();
            slbTilesets.Clear();

            foreach (string name in GraphicsManager.TileIndex.Nodes.Keys)
            {
                tileIndices.Add(name);
                slbTilesets.AddItem(name);
            }
            chosenTileset = tileIndices[0];
            currentTileset = tileIndices[0];

            slbTilesets.SelectedIndex = 0;

            RefreshAnimControls();

            tilePreview.SetChosenAnim(new TileLayer(chosenTile, chosenTileset));

            //refresh
            RefreshTileSelect();
        }

        void RefreshTileset()
        {
            RefreshScrollMaximums();

            RefreshPic();
        }


        void RefreshPic()
        {
            int picX = picTileset.Size.Width / GraphicsManager.TileSize;
            int picY = picTileset.Size.Height / GraphicsManager.TileSize;

            Loc tilePos = GraphicsManager.TileIndex.GetTileDims(currentTileset);

            if (inAnimMode)
                lblTileInfo.Visible = false;
            else
            {
                lblTileInfo.Visible = true;
                lblTileInfo.Text = "X:" + chosenTile.X + "  Y:" + chosenTile.Y + "  Tile:" + chosenTileset;
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
                                graphics.DrawImage(img, x * GraphicsManager.TileSize, y * GraphicsManager.TileSize);
                        }
                    }

                    //draw red square
                    if (currentTileset == chosenTileset &&
                        chosenTile.X >= hScroll.Value && chosenTile.X < hScroll.Value + picX &&
                        chosenTile.Y >= vScroll.Value && chosenTile.Y < vScroll.Value + picY)
                    {
                        graphics.DrawRectangle(new Pen(Color.Red, 2), new Rectangle((chosenTile.X - hScroll.Value) * GraphicsManager.TileSize + 1,
                            (chosenTile.Y - vScroll.Value) * GraphicsManager.TileSize + 1,
                            GraphicsManager.TileSize - 2, GraphicsManager.TileSize - 2));
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


            int picX = picTileset.Size.Width / GraphicsManager.TileSize;
            int picY = picTileset.Size.Height / GraphicsManager.TileSize;

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
                RefreshPic();

        }

        void RefreshScrollMaximums()
        {

            int picX = picTileset.Size.Width / GraphicsManager.TileSize;
            int picY = picTileset.Size.Height / GraphicsManager.TileSize;

            Loc tilePos = GraphicsManager.TileIndex.GetTileDims(currentTileset);

            if (tilePos.X - picX <= 0)
            {
                hScroll.Maximum = 0;
                hScroll.Enabled = false;
            }
            else
            {
                hScroll.Enabled = true;
                hScroll.Maximum = tilePos.X - picX + 1;
            }

            if (tilePos.Y - picY <= 0)
            {
                vScroll.Maximum = 0;
                vScroll.Enabled = false;
            }
            else
            {
                vScroll.Enabled = true;
                vScroll.Maximum = tilePos.Y - picY + 1;
            }
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
            RefreshPic();
        }

        private void hScroll_Scroll(object sender, ScrollEventArgs e)
        {
            RefreshPic();
        }


        private void picTileset_Click(object sender, EventArgs e)
        {
            MouseEventArgs args = e as MouseEventArgs;


            int clickedX = args.X / GraphicsManager.TileSize + hScroll.Value;
            int clickedY = args.Y / GraphicsManager.TileSize + vScroll.Value;

            if (GraphicsManager.TileIndex.GetPosition(currentTileset, new Loc(clickedX, clickedY)) > 0)
            {
                chosenTileset = currentTileset;
                chosenTile = new Loc(clickedX, clickedY);
                RefreshPic();

                if (!inAnimMode)
                    tilePreview.SetChosenAnim(new TileLayer(chosenTile, chosenTileset));
                else
                {
                    TileLayer chosenAnim = tilePreview.GetChosenAnim();
                    if (lbxFrames.SelectedIndex > -1)
                        chosenAnim.Frames[lbxFrames.SelectedIndex] = new TileFrame(chosenTile, chosenTileset);
                    else
                        chosenAnim.Frames.Add(new TileFrame(chosenTile, chosenTileset));
                    tilePreview.SetChosenAnim(chosenAnim);
                    UpdateAnimFrames();
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
                    TileLayer chosenAnim = tilePreview.GetChosenAnim();
                    chosenAnim.Frames.RemoveRange(1, chosenAnim.Frames.Count - 1);

                    tilePreview.SetChosenAnim(chosenAnim);

                    chosenTile = chosenAnim.Frames[0].TexLoc;
                    chosenTileset = chosenAnim.Frames[0].Sheet;
                    currentTileset = chosenTileset;
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
            tilePreview.SetChosenAnim(chosenAnim);
        }

        private void btnAddFrame_Click(object sender, EventArgs e)
        {
            TileLayer chosenAnim = tilePreview.GetChosenAnim();
            if (lbxFrames.SelectedIndex > -1)
                chosenAnim.Frames.Insert(lbxFrames.SelectedIndex, new TileFrame(chosenTile, chosenTileset));
            else
                chosenAnim.Frames.Add(new TileFrame(chosenTile, chosenTileset));
            tilePreview.SetChosenAnim(chosenAnim);

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
                tilePreview.SetChosenAnim(chosenAnim);
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
                chosenTile = chosenAnim.Frames[lbxFrames.SelectedIndex].TexLoc;
                RefreshTileSelect();
            }
        }

        private void slbTilesets_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (slbTilesets.SelectedIndex > -1)
            {
                currentTileset = tileIndices[slbTilesets.InternalIndex];

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


            nudFrameLength.Maximum = Int32.MaxValue;

            foreach (string name in GraphicsManager.TileIndex.Nodes.Keys)
            {
                tileIndices.Add(name);
                slbTilesets.AddItem(name);
            }
            chosenTileset = tileIndices[0];
            currentTileset = tileIndices[0];

            slbTilesets.SelectedIndex = 0;

            RefreshAnimControls();

            tilePreview.SetChosenAnim(new TileLayer(chosenTile, chosenTileset));

            //refresh
            RefreshTileSelect();
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
