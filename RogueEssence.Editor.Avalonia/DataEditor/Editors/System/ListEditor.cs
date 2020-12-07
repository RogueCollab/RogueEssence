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
    public class ListEditor : Editor<IList>
    {
        public override bool DefaultSubgroup => true;
        public override bool DefaultDecoration => false;

        public override void LoadWindowControls(StackPanel control, string name, Type type, object[] attributes, IList member)
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

                DataEditor.loadClassControls(frmData.ControlPanel, "(List) " + name + "[" + index + "]", elementType, ReflectionExt.GetPassableAttributes(1, attributes), element, true);

                frmData.SelectedOKEvent += () =>
                {
                    element = DataEditor.saveClassControls(frmData.ControlPanel, name, elementType, ReflectionExt.GetPassableAttributes(1, attributes), true);
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

            lbxValue.LoadFromList(member);
            control.Children.Add(lbxValue);
        }


        public override IList SaveWindowControls(StackPanel control, string name, Type type, object[] attributes)
        {
            int controlIndex = 0;
            controlIndex++;
            CollectionBox lbxValue = (CollectionBox)control.Children[controlIndex];
            return lbxValue.GetList(type);
        }
    }
}
