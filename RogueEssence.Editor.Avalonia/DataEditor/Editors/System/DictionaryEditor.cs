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
    public class DictionaryEditor : Editor<IDictionary>
    {
        public override bool DefaultSubgroup => true;
        public override bool DefaultDecoration => false;
        public override bool DefaultType => true;

        public override void LoadWindowControls(StackPanel control, string parent, string name, Type type, object[] attributes, IDictionary member)
        {
            LoadLabelControl(control, name);

            Type keyType = ReflectionExt.GetBaseTypeArg(typeof(IDictionary<,>), type, 0);
            Type elementType = ReflectionExt.GetBaseTypeArg(typeof(IDictionary<,>), type, 1);

            DictionaryBox lbxValue = new DictionaryBox();
            
            EditorHeightAttribute heightAtt = ReflectionExt.FindAttribute<EditorHeightAttribute>(attributes);
            if (heightAtt != null)
                lbxValue.MaxHeight = heightAtt.Height;
            else
                lbxValue.MaxHeight = 180;

            DictionaryBoxViewModel mv = new DictionaryBoxViewModel(control.GetOwningForm(), new StringConv(elementType, ReflectionExt.GetPassableAttributes(2, attributes)));
            lbxValue.DataContext = mv;
            lbxValue.MinHeight = lbxValue.MaxHeight;//TODO: Uptake Avalonia fix for improperly updating Grid control dimensions

            //add lambda expression for editing a single element
            mv.OnEditItem += (object key, object element, DictionaryBoxViewModel.EditElementOp op) =>
            {
                string elementName = name + "[" + key.ToString() + "]";
                DataEditForm frmData = new DataEditForm();
                frmData.Title = DataEditor.GetWindowTitle(parent, elementName, element, elementType, ReflectionExt.GetPassableAttributes(2, attributes));

                DataEditor.LoadClassControls(frmData.ControlPanel, parent, elementName, elementType, ReflectionExt.GetPassableAttributes(2, attributes), element, true);

                frmData.SelectedOKEvent += () =>
                {
                    element = DataEditor.SaveClassControls(frmData.ControlPanel, elementName, elementType, ReflectionExt.GetPassableAttributes(2, attributes), true);
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

            mv.OnEditKey += (object key, object element, DictionaryBoxViewModel.EditElementOp op) =>
            {
                string elementName = name + "<Key>";
                DataEditForm frmData = new DataEditForm();
                frmData.Title = DataEditor.GetWindowTitle(parent, elementName, key, keyType, ReflectionExt.GetPassableAttributes(1, attributes));

                DataEditor.LoadClassControls(frmData.ControlPanel, parent, elementName, keyType, ReflectionExt.GetPassableAttributes(1, attributes), key, true);

                frmData.SelectedOKEvent += () =>
                {
                    key = DataEditor.SaveClassControls(frmData.ControlPanel, elementName, keyType, ReflectionExt.GetPassableAttributes(1, attributes), true);
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

            mv.LoadFromDict(member);
            control.Children.Add(lbxValue);
        }


        public override IDictionary SaveWindowControls(StackPanel control, string name, Type type, object[] attributes)
        {
            int controlIndex = 0;
            controlIndex++;
            DictionaryBox lbxValue = (DictionaryBox)control.Children[controlIndex];
            DictionaryBoxViewModel mv = (DictionaryBoxViewModel)lbxValue.DataContext;
            return mv.GetDict(type);
        }
    }
}
