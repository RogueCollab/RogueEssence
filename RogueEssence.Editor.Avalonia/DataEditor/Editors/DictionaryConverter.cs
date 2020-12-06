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
    public class DictionaryConverter : EditorConverter<IDictionary>
    {
        public override void LoadClassControls(StackPanel control, string name, Type type, object[] attributes, IDictionary member, bool isWindow)
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

                DataEditor.StaticLoadMemberControl(frmData.ControlPanel, "(Dict) " + name + "[" + key.ToString() + "]", elementType, ReflectionExt.GetPassableAttributes(2, attributes), element, true);

                frmData.SelectedOKEvent += () =>
                {
                    DataEditor.StaticSaveMemberControl(frmData.ControlPanel, name, elementType, ReflectionExt.GetPassableAttributes(2, attributes), ref element, true);
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

                DataEditor.StaticLoadMemberControl(frmKey.ControlPanel, "(Dict) " + name + "<New Key>", keyType, new object[0] { }, null, true);

                frmKey.SelectedOKEvent += () =>
                {
                    DataEditor.StaticSaveMemberControl(frmKey.ControlPanel, name, keyType, new object[0] { }, ref key, true);
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

            lbxValue.LoadFromDict((IDictionary)member);
            control.Children.Add(lbxValue);
        }


        public override void SaveClassControls(StackPanel control, string name, Type type, object[] attributes, ref IDictionary member, bool isWindow)
        {
            int controlIndex = 0;
            controlIndex++;
            DictionaryBox lbxValue = (DictionaryBox)control.Children[controlIndex];
            member = lbxValue.GetDict(type);
            controlIndex++;
        }
    }
}
