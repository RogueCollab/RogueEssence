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
    public class SpawnRangeListEditor : Editor<ISpawnRangeList>
    {
        public override bool DefaultSubgroup => true;
        public override bool DefaultDecoration => false;

        public override void LoadWindowControls(StackPanel control, string parent, string name, Type type, object[] attributes, ISpawnRangeList member)
        {
            LoadLabelControl(control, name);

            Type elementType = ReflectionExt.GetBaseTypeArg(typeof(ISpawnRangeList<>), type, 0);

            RangeBorderAttribute rangeAtt = ReflectionExt.FindAttribute<RangeBorderAttribute>(attributes);

            SpawnRangeListBox lbxValue = new SpawnRangeListBox();
            lbxValue.MaxHeight = 260;
            SpawnRangeListBoxViewModel mv = new SpawnRangeListBoxViewModel(new StringConv(elementType, ReflectionExt.GetPassableAttributes(1, attributes)));
            if (rangeAtt != null)
            {
                mv.Index1 = rangeAtt.Index1;
                mv.Inclusive = rangeAtt.Inclusive;
            }
            lbxValue.DataContext = mv;


            //add lambda expression for editing a single element
            mv.OnEditItem += (int index, object element, SpawnRangeListBoxViewModel.EditElementOp op) =>
            {
                string elementName = name + "[" + index + "]";
                DataEditForm frmData = new DataEditForm();
                frmData.Title = DataEditor.GetWindowTitle(parent, elementName, element, elementType, ReflectionExt.GetPassableAttributes(2, attributes));

                DataEditor.LoadClassControls(frmData.ControlPanel, parent, elementName, elementType, ReflectionExt.GetPassableAttributes(2, attributes), element, true);

                frmData.SelectedOKEvent += () =>
                {
                    element = DataEditor.SaveClassControls(frmData.ControlPanel, elementName, elementType, ReflectionExt.GetPassableAttributes(2, attributes), true);
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

        public override ISpawnRangeList SaveWindowControls(StackPanel control, string name, Type type, object[] attributes)
        {
            int controlIndex = 0;
            controlIndex++;
            SpawnRangeListBox lbxValue = (SpawnRangeListBox)control.Children[controlIndex];
            SpawnRangeListBoxViewModel mv = (SpawnRangeListBoxViewModel)lbxValue.DataContext;
            return mv.GetList(type);
        }
    }
}
