using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text;
using RogueEssence.Content;
using RogueEssence.Dungeon;
using RogueEssence.Data;
using System.Drawing;
using RogueElements;

namespace RogueEssence.Dev
{
    public class TypeDictConverter : EditorConverter<ITypeDict>
    {
        public override void LoadClassControls(ITypeDict obj, TableLayoutPanel control)
        {
            CollectionBox lbxValue = new CollectionBox();
            lbxValue.Dock = DockStyle.Fill;
            lbxValue.Size = new Size(0, 150);
            List<object> states = new List<object>();
            foreach (object state in obj)
                states.Add(state);
            lbxValue.LoadFromList(typeof(List<object>), states);
            control.Controls.Add(lbxValue);

            Type elementType = ReflectionExt.GetBaseTypeArg(typeof(ITypeDict<>), obj.GetType(), 0);
            //add lambda expression for editing a single element
            lbxValue.OnEditItem = (int index, object element, CollectionBox.EditElementOp op) =>
            {
                ElementForm frmData = new ElementForm();
                if (element == null)
                    frmData.Text = "New " + elementType.Name;
                else
                    frmData.Text = element.ToString();

                DataEditor.StaticLoadMemberControl(frmData.ControlPanel, "(StateCollection) [" + index + "]", elementType, new object[0] { }, element, true);

                frmData.OnCopy += (object copySender, EventArgs copyE) => {
                    object obj = null;
                    DataEditor.StaticSaveMemberControl(frmData.ControlPanel, "StateCollection", elementType, new object[0] { }, ref obj, true);
                    Clipboard.SetDataObject(obj);
                };
                frmData.OnPaste += (object copySender, EventArgs copyE) => {
                    IDataObject clip = Clipboard.GetDataObject();
                    string[] formats = clip.GetFormats();
                    object clipObj = clip.GetData(formats[0]);
                    Type type1 = clipObj.GetType();
                    Type type2 = elementType;
                    if (type2.IsAssignableFrom(type1))
                    {
                        frmData.ControlPanel.Controls.Clear();
                        DataEditor.StaticLoadMemberControl(frmData.ControlPanel, "(StateCollection) [" + index + "]", elementType, new object[0] { }, clipObj, true);
                    }
                    else
                        MessageBox.Show(String.Format("Incompatible types:\n{0}\n{1}", type1.AssemblyQualifiedName, type2.AssemblyQualifiedName), "Invalid Operation", MessageBoxButtons.OK, MessageBoxIcon.Error);
                };

                frmData.OnOK += (object okSender, EventArgs okE) =>
                {
                    DataEditor.StaticSaveMemberControl(frmData.ControlPanel, "StateCollection", elementType, new object[0] { }, ref element, true);

                    bool itemExists = false;
                    for (int ii = 0; ii < lbxValue.Collection.Count; ii++)
                    {
                        if (ii != index)
                        {
                            if (lbxValue.Collection[ii].GetType() == element.GetType())
                                itemExists = true;
                        }
                    }

                    if (itemExists)
                        MessageBox.Show("Cannot add duplicate states.", "Entry already exists.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    else
                    {
                        op(index, element);
                        frmData.Close();
                    }
                };
                frmData.OnCancel += (object okSender, EventArgs okE) =>
                {
                    frmData.Close();
                };

                frmData.Show();
            };
        }


        public override void SaveClassControls(ITypeDict obj, TableLayoutPanel control)
        {
            CollectionBox lbxValue = (CollectionBox)control.Controls[0];

            for (int ii = 0; ii < lbxValue.Collection.Count; ii++)
                obj.Set(lbxValue.Collection[ii]);
        }
    }
}
