using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text;
using RogueEssence.Content;
using RogueEssence.Dungeon;
using RogueEssence.Data;
using System.Drawing;

namespace RogueEssence.Dev
{
    public class AutoTileBaseConverter : EditorConverter<AutoTileBase>
    {
        public override void LoadMemberControl(AutoTileBase obj, TableLayoutPanel control, string name, Type type, object[] attributes, object member, bool isWindow)
        {
            if (type == typeof(List<TileLayer>))
            {
                List<TileLayer> anims = (List<TileLayer>)member;
                DataEditor.LoadLabelControl(control, name);

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
                    DataEditor.StaticLoadMemberControl(frmData.ControlPanel, name, type, attributes, preview.Tag, true);

                    frmData.SetObjectName(type.Name);
                    frmData.OnCopy += (object copySender, EventArgs copyE) => {
                        object obj = null;
                        DataEditor.StaticSaveMemberControl(frmData.ControlPanel, name, type, attributes, ref obj, true);
                        Clipboard.SetDataObject(obj);
                    };
                    frmData.OnPaste += (object copySender, EventArgs copyE) => {
                        IDataObject clip = Clipboard.GetDataObject();
                        string[] formats = clip.GetFormats();
                        object clipObj = clip.GetData(formats[0]);
                        Type type1 = clipObj.GetType();
                        Type type2 = type;
                        if (type1 == type2)
                        {
                            frmData.ControlPanel.Controls.Clear();
                            DataEditor.StaticLoadMemberControl(frmData.ControlPanel, name, type, attributes, clipObj, true);
                        }
                        else
                            MessageBox.Show(String.Format("Incompatible types:\n{0}\n{1}", type1.AssemblyQualifiedName, type2.AssemblyQualifiedName), "Invalid Operation", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    };

                    if (frmData.ShowDialog() == DialogResult.OK)
                    {
                        object element = preview.Tag;
                        DataEditor.StaticSaveMemberControl(frmData.ControlPanel, name, type, attributes, ref element, true);
                        List<TileLayer> new_anims = (List<TileLayer>)element;
                        preview.SetChosenAnim(new_anims.Count > 0 ? new_anims[0] : new TileLayer());
                        preview.Tag = element;
                    }
                };
            }
            else
            {
                base.LoadMemberControl(obj, control, name, type, attributes, member, isWindow);
            }
        }

        public override void SaveMemberControl(AutoTileBase obj, TableLayoutPanel control, string name, Type type, object[] attributes, ref object member, bool isWindow)
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
                base.SaveMemberControl(obj, control, name, type, attributes, ref member, isWindow);
            }
        }
    }
}
