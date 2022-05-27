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
using RogueEssence.Dev.ViewModels;

namespace RogueEssence.Dev
{
    public class ArrayEditor : Editor<Array>
    {
        public override bool DefaultSubgroup => true;
        public override bool DefaultDecoration => false;

        public override void LoadWindowControls(StackPanel control, string parent, Type parentType, string name, Type type, object[] attributes, Array member, Type[] subGroupStack)
        {
            RankedListAttribute rangeAtt = ReflectionExt.FindAttribute<RankedListAttribute>(attributes);

            if (rangeAtt != null)
            {
                RankedCollectionBox lbxValue = new RankedCollectionBox();

                EditorHeightAttribute heightAtt = ReflectionExt.FindAttribute<EditorHeightAttribute>(attributes);
                if (heightAtt != null)
                    lbxValue.MaxHeight = heightAtt.Height;
                else
                    lbxValue.MaxHeight = 180;

                lbxValue.DataContext = createViewModel(control, parent, name, type, attributes, member, rangeAtt.Index1);
                lbxValue.MinHeight = lbxValue.MaxHeight;//TODO: Uptake Avalonia fix for improperly updating Grid control dimensions
                control.Children.Add(lbxValue);
            }
            else
            {
                CollectionBox lbxValue = new CollectionBox();

                EditorHeightAttribute heightAtt = ReflectionExt.FindAttribute<EditorHeightAttribute>(attributes);
                if (heightAtt != null)
                    lbxValue.MaxHeight = heightAtt.Height;
                else
                    lbxValue.MaxHeight = 180;

                lbxValue.DataContext = createViewModel(control, parent, name, type, attributes, member, false);
                control.Children.Add(lbxValue);
            }

        }

        private CollectionBoxViewModel createViewModel(StackPanel control, string parent, string name, Type type, object[] attributes, Array member, bool index1)
        {
            Type elementType = type.GetElementType();

            CollectionBoxViewModel mv = new CollectionBoxViewModel(new StringConv(elementType, ReflectionExt.GetPassableAttributes(1, attributes)));
            mv.Index1 = index1;
            //add lambda expression for editing a single element
            mv.OnEditItem += (int index, object element, CollectionBoxViewModel.EditElementOp op) =>
            {
                string elementName = name + "[" + index + "]";
                DataEditForm frmData = new DataEditForm();
                frmData.Title = DataEditor.GetWindowTitle(parent, elementName, element, elementType, ReflectionExt.GetPassableAttributes(0, attributes));

                DataEditor.LoadClassControls(frmData.ControlPanel, parent, null, elementName, elementType, ReflectionExt.GetPassableAttributes(0, attributes), element, true, new Type[0]);

                frmData.SelectedOKEvent += async () =>
                {
                    element = DataEditor.SaveClassControls(frmData.ControlPanel, elementName, elementType, ReflectionExt.GetPassableAttributes(0, attributes), true, new Type[0]);
                    op(index, element);
                    return true;
                };

                control.GetOwningForm().RegisterChild(frmData);
                frmData.Show();
            };


            List<object> objList = new List<object>();
            for (int ii = 0; ii < member.Length; ii++)
                objList.Add(member.GetValue(ii));

            mv.LoadFromList(objList);
            return mv;
        }

        public override Array SaveWindowControls(StackPanel control, string name, Type type, object[] attributes, Type[] subGroupStack)
        {
            int controlIndex = 0;
            //TODO: 2D array grid support
            //if (type.GetElementType().IsArray)

            IControl lbxValue = control.Children[controlIndex];
            CollectionBoxViewModel mv = (CollectionBoxViewModel)lbxValue.DataContext;
            List<object> objList = (List<object>)mv.GetList(typeof(List<object>));

            Array array = Array.CreateInstance(type.GetElementType(), objList.Count);
            for (int ii = 0; ii < objList.Count; ii++)
                array.SetValue(objList[ii], ii);

            return array;
        }
    }
}
