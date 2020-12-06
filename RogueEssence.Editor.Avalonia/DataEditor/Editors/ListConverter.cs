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
using System.Collections;

namespace RogueEssence.Dev
{
    public class ListConverter : EditorConverter<IList>
    {
        public override void LoadClassControls(StackPanel control, string name, Type type, object[] attributes, IList member, bool isWindow)
        {
            LoadLabelControl(control, name);

            CollectionBox lbxValue = new CollectionBox();

            Type elementType = ReflectionExt.GetBaseTypeArg(typeof(IList<>), type, 0);
            //lbxValue.StringConv = GetStringRep(elementType, ReflectionExt.GetPassableAttributes(1, attributes));
            //add lambda expression for editing a single element
            lbxValue.OnEditItem = (int index, object element, CollectionBox.EditElementOp op) =>
            {
                DataEditForm frmData = new DataEditForm();
                if (element == null)
                    frmData.Title = name + "/" + "New " + elementType.Name;
                else
                    frmData.Title = name + "/" + element.ToString();

                DataEditor.StaticLoadMemberControl(frmData.ControlPanel, "(List) " + name + "[" + index + "]", elementType, ReflectionExt.GetPassableAttributes(1, attributes), element, true);

                frmData.SelectedOKEvent += () =>
                {
                    DataEditor.StaticSaveMemberControl(frmData.ControlPanel, name, elementType, ReflectionExt.GetPassableAttributes(1, attributes), ref element, true);
                    op(index, element);
                    frmData.Close();
                };
                frmData.SelectedCancelEvent += () =>
                {
                    frmData.Close();
                };

                control.GetOwningForm().RegisterChild(frmData);
                frmData.Show();
            };

            lbxValue.LoadFromList((IList)member);
            control.Children.Add(lbxValue);
        }


        public override void SaveClassControls(StackPanel control, string name, Type type, object[] attributes, ref IList member, bool isWindow)
        {
            int controlIndex = 0;
            controlIndex++;
            CollectionBox lbxValue = (CollectionBox)control.Children[controlIndex];
            member = lbxValue.GetList(type);
            controlIndex++;
        }
    }
}
