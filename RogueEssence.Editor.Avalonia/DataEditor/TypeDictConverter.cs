using System;
using System.Collections.Generic;
using System.Text;
using RogueEssence.Content;
using RogueEssence.Dungeon;
using RogueEssence.Data;
using System.Drawing;
using RogueElements;
using Avalonia.Controls;
using RogueEssence.Dev.Views;

namespace RogueEssence.Dev
{
    public class TypeDictConverter : EditorConverter<ITypeDict>
    {
        public override void LoadClassControls(StackPanel control, string name, Type type, object[] attributes, ITypeDict member, bool isWindow)
        {
            DataEditor.LoadLabelControl(control, name);

            CollectionBox lbxValue = new CollectionBox();

            Type elementType = ReflectionExt.GetBaseTypeArg(typeof(ITypeDict<>), member.GetType(), 0);
            //lbxValue.StringConv = DataEditor.GetStringRep(elementType, new object[0] { });
            //add lambda expression for editing a single element
            lbxValue.OnEditItem = (int index, object element, CollectionBox.EditElementOp op) =>
            {
                DataEditForm frmData = new DataEditForm();
                if (element == null)
                    frmData.Title = "New " + elementType.Name;
                else
                    frmData.Title = element.ToString();

                //TODO: make this a member and reference it that way
                DataEditor.StaticLoadMemberControl(frmData.ControlPanel, "(StateCollection) [" + index + "]", elementType, new object[0] { }, element, true);

                frmData.SelectedOKEvent += async () =>
                {
                    DataEditor.StaticSaveMemberControl(frmData.ControlPanel, "StateCollection", elementType, new object[0] { }, ref element, true);

                    bool itemExists = false;

                    List<object> states = (List<object>)lbxValue.GetList(typeof(List<object>));
                    for (int ii = 0; ii < states.Count; ii++)
                    {
                        if (ii != index)
                        {
                            if (states[ii].GetType() == element.GetType())
                                itemExists = true;
                        }
                    }

                    if (itemExists)
                    {
                        await MessageBox.Show(control.GetOwningForm(), "Cannot add duplicate states.", "Entry already exists.", MessageBox.MessageBoxButtons.Ok);
                    }
                    else
                    {
                        op(index, element);
                        frmData.Close();
                    }
                };
                frmData.SelectedCancelEvent += () =>
                {
                    frmData.Close();
                };

                control.GetOwningForm().RegisterChild(frmData);
                frmData.Show();
            };

            List<object> states = new List<object>();
            foreach (object state in member)
                states.Add(state);
            lbxValue.LoadFromList(states);
            control.Children.Add(lbxValue);
        }


        public override void SaveClassControls(StackPanel control, string name, Type type, object[] attributes, ref ITypeDict member, bool isWindow)
        {
            CollectionBox lbxValue = (CollectionBox)control.Children[1];

            member = (ITypeDict)Activator.CreateInstance(type);
            List<object> states = (List<object>)lbxValue.GetList(typeof(List<object>));
            for (int ii = 0; ii < states.Count; ii++)
                member.Set(states[ii]);
        }
    }
}
