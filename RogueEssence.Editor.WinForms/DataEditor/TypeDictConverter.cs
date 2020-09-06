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

            Type elementType = ReflectionExt.GetBaseTypeArg(typeof(ITypeDict<>), obj.GetType(), 0);
            lbxValue.StringConv = DataEditor.GetStringRep(elementType, new object[0] { });
            //add lambda expression for editing a single element
            lbxValue.OnEditItem = (int index, object element, CollectionBox.EditElementOp op) =>
            {
                ElementForm frmData = new ElementForm();
                if (element == null)
                    frmData.Text = "New " + elementType.Name;
                else
                    frmData.Text = element.ToString();

                DataEditor.StaticLoadMemberControl(frmData.ControlPanel, "(StateCollection) [" + index + "]", elementType, new object[0] { }, element, true);

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

            List<object> states = new List<object>();
            foreach (object state in obj)
                states.Add(state);
            lbxValue.LoadFromList(typeof(List<object>), states);
            control.Controls.Add(lbxValue);
        }


        public override void SaveClassControls(ITypeDict obj, TableLayoutPanel control)
        {
            CollectionBox lbxValue = (CollectionBox)control.Controls[0];

            for (int ii = 0; ii < lbxValue.Collection.Count; ii++)
                obj.Set(lbxValue.Collection[ii]);
        }
    }
}
