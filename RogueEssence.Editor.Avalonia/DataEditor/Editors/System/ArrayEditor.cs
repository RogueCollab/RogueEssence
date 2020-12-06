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
using Avalonia;
using System.Reactive.Subjects;

namespace RogueEssence.Dev
{
    public class ArrayEditor : Editor<Array>
    {
        public override void LoadClassControls(StackPanel control, string name, Type type, object[] attributes, Array member, bool isWindow)
        {
            //TODO: 2D array grid support
            //if (type.GetElementType().IsArray)

            LoadLabelControl(control, name);

            CollectionBox lbxValue = new CollectionBox();

            Type elementType = type.GetElementType();
            //lbxValue.StringConv = GetStringRep(elementType, ReflectionExt.GetPassableAttributes(1, attributes));
            //add lambda expression for editing a single element
            lbxValue.OnEditItem = (int index, object element, CollectionBox.EditElementOp op) =>
            {
                DataEditForm frmData = new DataEditForm();
                if (element == null)
                    frmData.Title = name + "/" + "New " + elementType.Name;
                else
                    frmData.Title = name + "/" + element.ToString();

                DataEditor.StaticLoadMemberControl(frmData.ControlPanel, "(Array) " + name + "[" + index + "]", elementType, ReflectionExt.GetPassableAttributes(0, attributes), element, true);

                frmData.SelectedOKEvent += () =>
                {
                    DataEditor.StaticSaveMemberControl(frmData.ControlPanel, name, elementType, ReflectionExt.GetPassableAttributes(0, attributes), ref element, true);
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


            List<object> objList = new List<object>();
            for (int ii = 0; ii < member.Length; ii++)
                objList.Add(member.GetValue(ii));

            lbxValue.LoadFromList(objList);
            control.Children.Add(lbxValue);
        }


        public override void SaveClassControls(StackPanel control, string name, Type type, object[] attributes, ref Array member, bool isWindow)
        {
            int controlIndex = 0;
            //TODO: 2D array grid support
            //if (type.GetElementType().IsArray)

            controlIndex++;
            CollectionBox lbxValue = (CollectionBox)control.Children[controlIndex];
            List<object> objList = (List<object>)lbxValue.GetList(typeof(List<object>));

            Array array = Array.CreateInstance(type.GetElementType(), objList.Count);
            for (int ii = 0; ii < objList.Count; ii++)
                array.SetValue(objList[ii], ii);

            member = array;
            controlIndex++;
        }
    }
}
