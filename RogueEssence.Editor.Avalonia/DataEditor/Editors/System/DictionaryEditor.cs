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
    public class DictionaryEditor : Editor<IDictionary>
    {
        public override bool DefaultSubgroup => true;
        public override bool DefaultDecoration => false;

        public override void LoadWindowControls(StackPanel control, string name, Type type, object[] attributes, IDictionary member)
        {
            LoadLabelControl(control, name);

            DictionaryBox lbxValue = new DictionaryBox();

            Type keyType = ReflectionExt.GetBaseTypeArg(typeof(IDictionary<,>), type, 0);
            Type elementType = ReflectionExt.GetBaseTypeArg(typeof(IDictionary<,>), type, 1);

            //lbxValue.StringConv = GetStringRep(elementType, ReflectionExt.GetPassableAttributes(2, attributes));
            //add lambda expression for editing a single element
            lbxValue.OnEditItem = (object key, object element, DictionaryBox.EditElementOp op) =>
            {
                DataEditForm frmData = new DataEditForm();
                if (element == null)
                    frmData.Title = name + "/" + "New " + elementType.Name;
                else
                    frmData.Title = name + "/" + element.ToString();

                DataEditor.loadClassControls(frmData.ControlPanel, "(Dict) " + name + "[" + key.ToString() + "]", elementType, ReflectionExt.GetPassableAttributes(2, attributes), element, true);

                frmData.SelectedOKEvent += () =>
                {
                    element = DataEditor.saveClassControls(frmData.ControlPanel, name, elementType, ReflectionExt.GetPassableAttributes(2, attributes), true);
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

            lbxValue.OnEditKey = (object key, object element, DictionaryBox.EditElementOp op) =>
            {
                DataEditForm frmKey = new DataEditForm();
                if (element == null)
                    frmKey.Title = name + "/" + "New Key:" + keyType.Name;
                else
                    frmKey.Title = name + "/" + element.ToString();

                DataEditor.loadClassControls(frmKey.ControlPanel, "(Dict) " + name + "<New Key>", keyType, new object[0] { }, null, true);

                frmKey.SelectedOKEvent += () =>
                {
                    key = DataEditor.saveClassControls(frmKey.ControlPanel, name, keyType, new object[0] { }, true);
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

            lbxValue.LoadFromDict(member);
            control.Children.Add(lbxValue);
        }


        public override IDictionary SaveWindowControls(StackPanel control, string name, Type type, object[] attributes)
        {
            int controlIndex = 0;
            controlIndex++;
            DictionaryBox lbxValue = (DictionaryBox)control.Children[controlIndex];
            return lbxValue.GetDict(type);
        }
    }
}
