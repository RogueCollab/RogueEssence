using System;
using System.Collections.Generic;
using System.Text;
using RogueEssence.Content;
using RogueEssence.Dungeon;
using RogueEssence.Data;
using System.Drawing;
using Avalonia.Controls;
using RogueEssence.Dev.Views;

namespace RogueEssence.Dev
{
    public class AutoTileEditor : Editor<AutoTileBase>
    {
        public override bool DefaultSubgroup => true;
        public override bool DefaultDecoration => false;

        public override void LoadMemberControl(AutoTileBase obj, StackPanel control, string name, Type type, object[] attributes, object member, bool isWindow)
        {
            //if (type == typeof(List<TileLayer>))
            //{
            //    List<TileLayer> anims = (List<TileLayer>)member;
            //    LoadLabelControl(control, name);

            //    Dev.TilePreview preview = new Dev.TilePreview();
            //    preview.Size = new Size(GraphicsManager.TileSize, GraphicsManager.TileSize);
            //    preview.SetChosenAnim(anims.Count > 0 ? anims[0] : new TileLayer());
            //    preview.Tag = member;
            //    control.Controls.Add(preview);
            //    preview.TileClick += (object sender, EventArgs e) =>
            //    {
            //        Dev.ElementForm frmData = new Dev.ElementForm();
            //        frmData.Text = "Edit Tile";
            //        Rectangle boxRect = new Rectangle(new Point(), frmData.Size);
            //        DataEditor.loadClassControls(frmData.ControlPanel, name, type, attributes, preview.Tag, true);

            //        if (frmData.ShowDialog() == DialogResult.OK)
            //        {
            //            object element = preview.Tag;
            //            DataEditor.saveClassControls(frmData.ControlPanel, name, type, attributes, ref element, true);
            //            List<TileLayer> new_anims = (List<TileLayer>)element;
            //            preview.SetChosenAnim(new_anims.Count > 0 ? new_anims[0] : new TileLayer());
            //            preview.Tag = element;
            //        }
            //    };
            //}
            //else
            //{
                base.LoadMemberControl(obj, control, name, type, attributes, member, isWindow);
            //}
        }

        public override object SaveMemberControl(AutoTileBase obj, StackPanel control, string name, Type type, object[] attributes, bool isWindow)
        {
            //if (type == typeof(List<TileLayer>))
            //{
            //    int controlIndex = 0;
            //    controlIndex++;
            //    Dev.TilePreview preview = (Dev.TilePreview)control.Controls[controlIndex];
            //    member = preview.Tag;
            //    controlIndex++;
            //}
            //else
            //{
                return base.SaveMemberControl(obj, control, name, type, attributes, isWindow);
            //}
        }
    }
}
