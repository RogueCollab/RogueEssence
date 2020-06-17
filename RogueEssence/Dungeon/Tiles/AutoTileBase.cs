using System;
using System.Collections.Generic;
using RogueElements;
using System.Drawing;
#if EDITORS
using System.Windows.Forms;
#endif
using RogueEssence.Content;

namespace RogueEssence.Dungeon
{
    [Serializable]
    public abstract class AutoTileBase : Dev.EditorData
    {
        public delegate void PlacementMethod(int x, int y, List<TileLayer> tile);
        public delegate bool QueryMethod(int x, int y);
        public abstract void AutoTileArea(ReRandom rand, Loc rectStart, Loc rectSize, PlacementMethod placementMethod, QueryMethod queryMethod);
        public abstract TileLayer[] Generic { get; }

        protected bool IsBlocked(QueryMethod queryMethod, int x, int y, Dir8 dir)
        {
            Loc loc = new Loc(x,y) + dir.GetLoc();

            return queryMethod(loc.X, loc.Y);
        }
        
#if EDITORS
        protected override void LoadMemberControl(TableLayoutPanel control, string name, Type type, object[] attributes, object member, bool isWindow)
        {
            if (type == typeof(List<TileLayer>))
            {
                List<TileLayer> anims = (List<TileLayer>)member;
                loadLabelControl(control, name);

                Dev.TilePreview preview = new Dev.TilePreview();
                preview.Size = new Size(GraphicsManager.TileSize, GraphicsManager.TileSize);
                preview.SetChosenAnim(anims.Count > 0 ? anims[0] : new TileLayer());
                preview.Tag = member;
                control.Controls.Add(preview);
                preview.TileClick += (object sender, EventArgs e) =>
                {
                    Dev.ElementForm frmData = new Dev.ElementForm();
                    frmData.Text = "Edit Tile";
                    Rectangle boxRect = new Rectangle(new Point(), frmData.Size);
                    staticLoadMemberControl(frmData.ControlPanel, name, type, attributes, preview.Tag, true);

                    if (frmData.ShowDialog() == DialogResult.OK)
                    {
                        object element = preview.Tag;
                        staticSaveMemberControl(frmData.ControlPanel, name, type, attributes, ref element, true);
                        List<TileLayer> new_anims = (List<TileLayer>)element;
                        preview.SetChosenAnim(new_anims.Count > 0 ? new_anims[0] : new TileLayer());
                        preview.Tag = element;
                    }
                };
            }
            else
            {
                base.LoadMemberControl(control, name, type, attributes, member, isWindow);
            }
        }

        protected override void SaveMemberControl(TableLayoutPanel control, string name, Type type, object[] attributes, ref object member, bool isWindow)
        {
            if (type == typeof(List<TileLayer>))
            {
                int controlIndex = 0;
                controlIndex++;
                Dev.TilePreview preview = (Dev.TilePreview)control.Controls[controlIndex];
                member = preview.Tag;
                controlIndex++;
            }
            else
            {
                base.SaveMemberControl(control, name, type, attributes, ref member, isWindow);
            }
        }
#endif


        protected TileLayer SelectTile(ReRandom rand, List<TileLayer> anims)
        {

            if (anims.Count > 0)
            {
                int index = 0;
                for (int ii = 0; ii < anims.Count - 1; ii++)
                {
                    if (rand.Next() % 2 == 0)
                        index++;
                    else
                        break;
                }
                return anims[index];
            }
            return new TileLayer();
        }
    }
}
