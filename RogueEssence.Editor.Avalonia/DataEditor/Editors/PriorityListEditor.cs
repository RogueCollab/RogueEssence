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
    public class PriorityListEditor : Editor<IPriorityList>
    {
        public override bool DefaultSubgroup => true;
        public override bool DefaultDecoration => false;

        public override void LoadWindowControls(StackPanel control, string name, Type type, object[] attributes, IPriorityList member)
        {
            LoadLabelControl(control, name);

            PriorityListBox lbxValue = new PriorityListBox();

            Type elementType = ReflectionExt.GetBaseTypeArg(typeof(IPriorityList<>), type, 0);
            //lbxValue.StringConv = GetStringRep(elementType, ReflectionExt.GetPassableAttributes(2, attributes));
            //add lambda expression for editing a single element
            lbxValue.OnEditItem = (Priority priority, int index, object element, PriorityListBox.EditElementOp op) =>
            {
                DataEditForm frmData = new DataEditForm();
                if (element == null)
                    frmData.Title = name + "/" + "New " + elementType.Name;
                else
                    frmData.Title = name + "/" + element.ToString();

                DataEditor.LoadClassControls(frmData.ControlPanel, "(PriorityList) " + name + "[" + index + "]", elementType, ReflectionExt.GetPassableAttributes(2, attributes), element, true);

                frmData.SelectedOKEvent += () =>
                {
                    element = DataEditor.SaveClassControls(frmData.ControlPanel, name, elementType, ReflectionExt.GetPassableAttributes(2, attributes), true);
                    op(priority, index, element);
                    frmData.Close();
                };
                frmData.SelectedCancelEvent += () =>
                {
                    frmData.Close();
                };

                control.GetOwningForm().RegisterChild(frmData);
                frmData.Show();
            };
            lbxValue.OnEditPriority = (Priority priority, int index, PriorityListBox.EditPriorityOp op) =>
            {
                DataEditForm frmData = new DataEditForm();
                frmData.Title = name + "/" + "New Priority";

                DataEditor.LoadClassControls(frmData.ControlPanel, "(PriorityList) " + name + "[" + index + "]", typeof(Priority), new object[0] { }, priority, true);

                frmData.SelectedOKEvent += () =>
                {
                    object priorityObj = DataEditor.SaveClassControls(frmData.ControlPanel, name, typeof(Priority), ReflectionExt.GetPassableAttributes(2, attributes), true);
                    op(priority, index, (Priority)priorityObj);
                    frmData.Close();
                };
                frmData.SelectedCancelEvent += () =>
                {
                    frmData.Close();
                };

                control.GetOwningForm().RegisterChild(frmData);
                frmData.Show();
            };

            lbxValue.LoadFromList((IPriorityList)member);
            control.Children.Add(lbxValue);
        }


        public override IPriorityList SaveWindowControls(StackPanel control, string name, Type type, object[] attributes)
        {
            int controlIndex = 0;
            controlIndex++;
            PriorityListBox lbxValue = (PriorityListBox)control.Children[controlIndex];
            return lbxValue.GetList(type);
        }
    }
}
