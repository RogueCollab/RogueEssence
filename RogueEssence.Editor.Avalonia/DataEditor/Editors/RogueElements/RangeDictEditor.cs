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
        /// <summary>
        /// Default display behavior of whether to treat 0s as 1s
        /// </summary>
        public bool Index1;

        /// <summary>
        /// Default display behavior of whether to treat end borders exclsusively
        /// </summary>
        public bool Inclusive;

        public RangeDictEditor(bool index1, bool inclusive)
        {
            Index1 = index1;
            Inclusive = inclusive;
        }

        public override bool DefaultSubgroup => true;
        public override bool DefaultDecoration => false;
        public override bool DefaultType => true;

        public override void LoadWindowControls(StackPanel control, string parent, string name, Type type, object[] attributes, IRangeDict member, Type[] subGroupStack)
        {
            LoadLabelControl(control, name);

            Type keyType = typeof(IntRange);
            Type elementType = ReflectionExt.GetBaseTypeArg(typeof(IRangeDict<>), type, 0);


            RangeDictBox lbxValue = new RangeDictBox();

            EditorHeightAttribute heightAtt = ReflectionExt.FindAttribute<EditorHeightAttribute>(attributes);
            if (heightAtt != null)
                lbxValue.MaxHeight = heightAtt.Height;
            else
                lbxValue.MaxHeight = 180;

            RangeDictBoxViewModel mv = new RangeDictBoxViewModel(control.GetOwningForm(), new StringConv(elementType, ReflectionExt.GetPassableAttributes(1, attributes)));

            mv.Index1 = Index1;
            mv.Inclusive = Inclusive;
            RangeBorderAttribute rangeAtt = ReflectionExt.FindAttribute<RangeBorderAttribute>(attributes);
            if (rangeAtt != null)
            {
                mv.Index1 = rangeAtt.Index1;
                mv.Inclusive = rangeAtt.Inclusive;
            }

            lbxValue.DataContext = mv;

            //add lambda expression for editing a single element
            mv.OnEditItem += (IntRange key, object element, RangeDictBoxViewModel.EditElementOp op) =>
            {
                string elementName = name + "[" + key.ToString() + "]";
                DataEditForm frmData = new DataEditForm();
                frmData.Title = DataEditor.GetWindowTitle(parent, elementName, element, elementType, ReflectionExt.GetPassableAttributes(1, attributes));

                DataEditor.LoadClassControls(frmData.ControlPanel, parent, elementName, elementType, ReflectionExt.GetPassableAttributes(1, attributes), element, true, new Type[0]);

                frmData.SelectedOKEvent += () =>
                {
                    element = DataEditor.SaveClassControls(frmData.ControlPanel, elementName, elementType, ReflectionExt.GetPassableAttributes(1, attributes), true, new Type[0]);
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
                string elementName = name + "<Range>";
                DataEditForm frmKey = new DataEditForm();

                List<object> attrList = new List<object>();
                if (rangeAtt != null)
                    attrList.Add(rangeAtt);
                frmKey.Title = DataEditor.GetWindowTitle(parent, elementName, key, keyType, attrList.ToArray());

                DataEditor.LoadClassControls(frmKey.ControlPanel, parent, elementName, keyType, attrList.ToArray(), key, true, new Type[0]);

                frmKey.SelectedOKEvent += () =>
                {
                    key = (IntRange)DataEditor.SaveClassControls(frmKey.ControlPanel, elementName, keyType, attrList.ToArray(), true, new Type[0]);
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


        public override IRangeDict SaveWindowControls(StackPanel control, string name, Type type, object[] attributes, Type[] subGroupStack)
        {
            int controlIndex = 0;
            controlIndex++;
            RangeDictBox lbxValue = (RangeDictBox)control.Children[controlIndex];
            RangeDictBoxViewModel mv = (RangeDictBoxViewModel)lbxValue.DataContext;
            return mv.GetDict(type);
        }
    }
}
