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
using RogueEssence.Dev.ViewModels;

namespace RogueEssence.Dev
{
    public class SpawnListEditor : Editor<ISpawnList>
    {
        public override bool DefaultSubgroup => true;
        public override bool DefaultDecoration => false;

        public override void LoadWindowControls(StackPanel control, string name, Type type, object[] attributes, ISpawnList member)
        {
            LoadLabelControl(control, name);

            Type elementType = ReflectionExt.GetBaseTypeArg(typeof(ISpawnList<>), type, 0);

            SpawnListBox lbxValue = new SpawnListBox();
            lbxValue.MaxHeight = 220;
            SpawnListBoxViewModel mv = new SpawnListBoxViewModel(new StringConv(elementType, ReflectionExt.GetPassableAttributes(1, attributes)));
            lbxValue.DataContext = mv;

            //add lambda expression for editing a single element
            mv.OnEditItem += (int index, object element, SpawnListBoxViewModel.EditElementOp op) =>
            {
                DataEditForm frmData = new DataEditForm();
                if (element == null)
                    frmData.Title = name + "/" + "New " + elementType.Name;
                else
                    frmData.Title = name + "/" + element.ToString();

                DataEditor.LoadClassControls(frmData.ControlPanel, "(SpawnList) " + name + "[" + index + "]", elementType, ReflectionExt.GetPassableAttributes(2, attributes), element, true);

                frmData.SelectedOKEvent += () =>
                {
                    element = DataEditor.SaveClassControls(frmData.ControlPanel, name, elementType, ReflectionExt.GetPassableAttributes(2, attributes), true);
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

            mv.LoadFromList(member);
            control.Children.Add(lbxValue);
        }

        public override ISpawnList SaveWindowControls(StackPanel control, string name, Type type, object[] attributes)
        {
            int controlIndex = 0;
            controlIndex++;
            SpawnListBox lbxValue = (SpawnListBox)control.Children[controlIndex];
            SpawnListBoxViewModel mv = (SpawnListBoxViewModel)lbxValue.DataContext;
            return mv.GetList(type);
        }
    }
}
