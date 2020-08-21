using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.IO;

namespace RogueEssence.Dev
{
    public partial class SpriteBrowser : UserControl
    {
        const int ITEM_SPACING = 32;
        List<Image> items;
        private int chosenPic;
        public int ChosenPic {
            get { return chosenPic; }
            set
            {
                chosenPic = value;
                RefreshPic();
            }
        }

        public SpriteBrowser()
        {
            InitializeComponent();

            picSprite.BackColor = Color.Black;

            int count = 0;
            items = new List<Image>();
            while (File.Exists(Content.GraphicsManager.ITEM_PATTERN + count + ".png"))
            {
                items.Add(new Bitmap(Content.GraphicsManager.ITEM_PATTERN + count + ".png"));
                count++;
            }
            if (items.Count / (picSprite.Width / ITEM_SPACING) > picSprite.Height / ITEM_SPACING)
                vsItemScroll.Maximum = items.Count / (picSprite.Width / ITEM_SPACING) - picSprite.Height / ITEM_SPACING;
            else
                vsItemScroll.Enabled = false;
            
        }

        void RefreshPic()
        {
            Image endImage = new Bitmap(picSprite.Width, picSprite.Height);
            using (var graphics = System.Drawing.Graphics.FromImage(endImage))
            {
                for (int x = 0; x < picSprite.Width / ITEM_SPACING; x++)
                {
                    for (int y = 0; y < picSprite.Height / ITEM_SPACING; y++)
                    {
                        int picIndex = x + (y + vsItemScroll.Value) * picSprite.Width / ITEM_SPACING;
                        if (picIndex < items.Count)
                        {
                            //blit at given location
                            graphics.DrawImage(items[picIndex], new Point(x * ITEM_SPACING + (ITEM_SPACING - items[picIndex].Width) / 2, y * ITEM_SPACING + (ITEM_SPACING - items[picIndex].Height) / 2));

                            //draw red square
                            if (ChosenPic == picIndex)
                            {
                                graphics.DrawRectangle(new Pen(Color.Red, 2), new Rectangle(x * ITEM_SPACING + 1, y * ITEM_SPACING + 1,
                                    ITEM_SPACING - 2, ITEM_SPACING - 2));
                            }
                        }
                    }
                }
            }
            picSprite.Image = endImage;
        }

        private void ItemBrowser_Load(object sender, EventArgs e)
        {
            RefreshPic();
        }

        private void picSprite_Click(object sender, EventArgs e)
        {
            int picIndex = (((MouseEventArgs)e).Y / ITEM_SPACING + vsItemScroll.Value) * (picSprite.Width / ITEM_SPACING) + ((MouseEventArgs)e).X / ITEM_SPACING;
            if (picIndex < items.Count)
                ChosenPic = picIndex;
        }

        private void vsItemScroll_ValueChanged(object sender, EventArgs e)
        {
            RefreshPic();
        }
    }
}
