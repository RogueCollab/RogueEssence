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
using System.Collections;

namespace RogueEssence.Dev
{
    public class ListEditor : Editor<IList>
    {
        public override bool DefaultSubgroup => true;
        public override bool DefaultDecoration => false;
        public override bool DefaultType => true;

        public override void LoadWindowControls(StackPanel control, string parent, string name, Type type, object[] attributes, IList member)
        {
            LoadLabelControl(control, name);

            RankedListAttribute rangeAtt = ReflectionExt.FindAttribute<RankedListAttribute>(attributes);

            if (rangeAtt != null)
            {
                RankedCollectionBox lbxValue = new RankedCollectionBox();
                lbxValue.MaxHeight = 180;
                lbxValue.DataContext = createViewModel(control, parent, name, type, attributes, member, rangeAtt.Index1);
                control.Children.Add(lbxValue);
            }
            else
            {
                CollectionBox lbxValue = new CollectionBox();
                lbxValue.MaxHeight = 180;
                lbxValue.DataContext = createViewModel(control, parent, name, type, attributes, member, false);
                control.Children.Add(lbxValue);
            }
        }

        private CollectionBoxViewModel createViewModel(StackPanel control, string parent, string name, Type type, object[] attributes, IList member, bool index1)
        {
            Type elementType = ReflectionExt.GetBaseTypeArg(typeof(IList<>), type, 0);

            CollectionBoxViewModel mv = new CollectionBoxViewModel(new StringConv(elementType, ReflectionExt.GetPassableAttributes(1, attributes)));
            mv.Index1 = index1;
            //add lambda expression for editing a single element
            mv.OnEditItem += (int index, object element, CollectionBoxViewModel.EditElementOp op) =>
            {
                string elementName = name + "[" + index + "]";
                DataEditForm frmData = new DataEditForm();
                frmData.Title = DataEditor.GetWindowTitle(parent, elementName, element, elementType, ReflectionExt.GetPassableAttributes(1, attributes));

                DataEditor.LoadClassControls(frmData.ControlPanel, parent, elementName, elementType, ReflectionExt.GetPassableAttributes(1, attributes), element, true);

                frmData.SelectedOKEvent += () =>
                {
                    element = DataEditor.SaveClassControls(frmData.ControlPanel, elementName, elementType, ReflectionExt.GetPassableAttributes(1, attributes), true);
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
            return mv;
        }

        public override IList SaveWindowControls(StackPanel control, string name, Type type, object[] attributes)
        {
            int controlIndex = 0;
            controlIndex++;
            IControl lbxValue = control.Children[controlIndex];
            CollectionBoxViewModel mv = (CollectionBoxViewModel)lbxValue.DataContext;
            return mv.GetList(type);
        }
    }
}
