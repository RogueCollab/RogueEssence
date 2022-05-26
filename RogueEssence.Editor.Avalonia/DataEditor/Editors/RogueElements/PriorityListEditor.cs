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
using RogueEssence.Dev.ViewModels;

namespace RogueEssence.Dev
{
    public class PriorityListEditor : Editor<IPriorityList>
    {
        public override bool DefaultSubgroup => true;
        public override bool DefaultDecoration => false;

        public override void LoadWindowControls(StackPanel control, string parent, string name, Type type, object[] attributes, IPriorityList member, Type[] subGroupStack)
        {
            Type elementType = ReflectionExt.GetBaseTypeArg(typeof(IPriorityList<>), type, 0);

            PriorityListBox lbxValue = new PriorityListBox();

            EditorHeightAttribute heightAtt = ReflectionExt.FindAttribute<EditorHeightAttribute>(attributes);
            if (heightAtt != null)
                lbxValue.MaxHeight = heightAtt.Height;
            else
                lbxValue.MaxHeight = 220;

            PriorityListBoxViewModel mv = new PriorityListBoxViewModel(new StringConv(elementType, ReflectionExt.GetPassableAttributes(2, attributes)));
            lbxValue.DataContext = mv;
            lbxValue.MinHeight = lbxValue.MaxHeight;//TODO: Uptake Avalonia fix for improperly updating Grid control dimensions

            //add lambda expression for editing a single element
            mv.OnEditItem = (Priority priority, int index, object element, PriorityListBoxViewModel.EditElementOp op) =>
            {
                string elementName = name + "[" + priority.ToString() + "]";
                DataEditForm frmData = new DataEditForm();
                frmData.Title = DataEditor.GetWindowTitle(parent, elementName, element, elementType, ReflectionExt.GetPassableAttributes(2, attributes));

                DataEditor.LoadClassControls(frmData.ControlPanel, parent, elementName, elementType, ReflectionExt.GetPassableAttributes(2, attributes), element, true, new Type[0]);

                frmData.SelectedOKEvent += async () =>
                {
                    element = DataEditor.SaveClassControls(frmData.ControlPanel, elementName, elementType, ReflectionExt.GetPassableAttributes(2, attributes), true, new Type[0]);
                    op(priority, index, element);
                    return true;
                };

                control.GetOwningForm().RegisterChild(frmData);
                frmData.Show();
            };
            mv.OnEditPriority = (Priority priority, int index, PriorityListBoxViewModel.EditPriorityOp op) =>
            {
                string elementName = name + "<Priority>";
                DataEditForm frmData = new DataEditForm();
                frmData.Title = DataEditor.GetWindowTitle(parent, elementName, priority, typeof(Priority), ReflectionExt.GetPassableAttributes(1, attributes));

                DataEditor.LoadClassControls(frmData.ControlPanel, parent, elementName, typeof(Priority), ReflectionExt.GetPassableAttributes(1, attributes), priority, true, new Type[0]);

                frmData.SelectedOKEvent += async () =>
                {
                    object priorityObj = DataEditor.SaveClassControls(frmData.ControlPanel, elementName, typeof(Priority), ReflectionExt.GetPassableAttributes(1, attributes), true, new Type[0]);
                    op(priority, index, (Priority)priorityObj);
                    return true;
                };

                control.GetOwningForm().RegisterChild(frmData);
                frmData.Show();
            };

            mv.LoadFromList(member);
            control.Children.Add(lbxValue);
        }


        public override IPriorityList SaveWindowControls(StackPanel control, string name, Type type, object[] attributes, Type[] subGroupStack)
        {
            int controlIndex = 0;

            PriorityListBox lbxValue = (PriorityListBox)control.Children[controlIndex];
            PriorityListBoxViewModel mv = (PriorityListBoxViewModel)lbxValue.DataContext;
            return mv.GetList(type);
        }
    }
}
