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
using RogueEssence.LevelGen;

namespace RogueEssence.Dev
{
    public class RangeDictEditor : Editor<IRangeDict>
    {
        public override bool DefaultSubgroup => true;
        public override bool DefaultDecoration => false;
        public override bool DefaultType => true;

        public override void LoadWindowControls(StackPanel control, string name, Type type, object[] attributes, IRangeDict member)
        {
            LoadLabelControl(control, name);

            RangeBorderAttribute rangeAtt = ReflectionExt.FindAttribute<RangeBorderAttribute>(attributes);

            RangeDictBox lbxValue = new RangeDictBox();
            lbxValue.MaxHeight = 180;
            RangeDictBoxViewModel mv = new RangeDictBoxViewModel(control.GetOwningForm());
            if (rangeAtt != null)
            {
                mv.Index1 = rangeAtt.Index1;
                mv.Inclusive = rangeAtt.Inclusive;
            }
            lbxValue.DataContext = mv;

            Type keyType = typeof(IntRange);
            Type elementType = ReflectionExt.GetBaseTypeArg(typeof(IRangeDict<>), type, 0);

            //lbxValue.StringConv = GetStringRep(elementType, ReflectionExt.GetPassableAttributes(2, attributes));
            //add lambda expression for editing a single element
            mv.OnEditItem += (IntRange key, object element, RangeDictBoxViewModel.EditElementOp op) =>
            {
                DataEditForm frmData = new DataEditForm();
                if (element == null)
                    frmData.Title = name + "/" + "New " + elementType.Name;
                else
                    frmData.Title = name + "/" + element.ToString();

                DataEditor.LoadClassControls(frmData.ControlPanel, "(Dict) " + name + "[" + key.ToString() + "]", elementType, ReflectionExt.GetPassableAttributes(2, attributes), element, true);

                frmData.SelectedOKEvent += () =>
                {
                    element = DataEditor.SaveClassControls(frmData.ControlPanel, name, elementType, ReflectionExt.GetPassableAttributes(2, attributes), true);
                    op(key, element);
                    frmData.Close();
                };
                frmData.SelectedCancelEvent += () =>
                {
                    frmData.Close();
                };

                control.GetOwningForm().RegisterChild(frmData);
                frmData.Show();
            };

            mv.OnEditKey += (IntRange key, object element, RangeDictBoxViewModel.EditElementOp op) =>
            {
                DataEditForm frmKey = new DataEditForm();
                if (element == null)
                    frmKey.Title = name + "/" + "New Range:" + keyType.Name;
                else
                    frmKey.Title = name + "/" + element.ToString();

                List<object> attrList = new List<object>();
                if (rangeAtt != null)
                    attrList.Add(rangeAtt);

                DataEditor.LoadClassControls(frmKey.ControlPanel, "(RangeDict) " + name + "<New Range>", keyType, attrList.ToArray(), key, true);

                frmKey.SelectedOKEvent += () =>
                {
                    key = (IntRange)DataEditor.SaveClassControls(frmKey.ControlPanel, name, keyType, attrList.ToArray(), true);
                    op(key, element);
                    frmKey.Close();
                };
                frmKey.SelectedCancelEvent += () =>
                {
                    frmKey.Close();
                };

                control.GetOwningForm().RegisterChild(frmKey);
                frmKey.Show();
            };

            mv.LoadFromDict(member);
            control.Children.Add(lbxValue);
        }


        public override IRangeDict SaveWindowControls(StackPanel control, string name, Type type, object[] attributes)
        {
            int controlIndex = 0;
            controlIndex++;
            RangeDictBox lbxValue = (RangeDictBox)control.Children[controlIndex];
            RangeDictBoxViewModel mv = (RangeDictBoxViewModel)lbxValue.DataContext;
            return mv.GetDict(type);
        }
    }
}
