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
using System.Threading.Tasks;

namespace RogueEssence.Dev
{
    public class TypeDictEditor : Editor<ITypeDict>
    {
        public override bool DefaultSubgroup => true;

        public override bool DefaultDecoration => false;

        public override void LoadWindowControls(StackPanel control, string parent, Type parentType, string name, Type type, object[] attributes, ITypeDict member, Type[] subGroupStack)
        {
            Type elementType = ReflectionExt.GetBaseTypeArg(typeof(ITypeDict<>), member.GetType(), 0);

            CollectionBox lbxValue = new CollectionBox();

            EditorHeightAttribute heightAtt = ReflectionExt.FindAttribute<EditorHeightAttribute>(attributes);
            if (heightAtt != null)
                lbxValue.MaxHeight = heightAtt.Height;
            else
                lbxValue.MaxHeight = 180;

            CollectionBoxViewModel mv = new CollectionBoxViewModel(new StringConv(elementType, ReflectionExt.GetPassableAttributes(1, attributes)));
            lbxValue.DataContext = mv;

            //add lambda expression for editing a single element
            mv.OnEditItem += (int index, object element, CollectionBoxViewModel.EditElementOp op) =>
            {
                string elementName = name + "[" + index + "]";
                DataEditForm frmData = new DataEditForm();
                frmData.Title = DataEditor.GetWindowTitle(parent, elementName, element, elementType, ReflectionExt.GetPassableAttributes(1, attributes));

                //TODO: make this a member and reference it that way
                DataEditor.LoadClassControls(frmData.ControlPanel, parent, null, elementName, elementType, ReflectionExt.GetPassableAttributes(1, attributes), element, true, new Type[0]);
                DataEditor.TrackTypeSize(frmData, elementType);

                frmData.SelectedOKEvent += async () =>
                {
                    object newElement = DataEditor.SaveClassControls(frmData.ControlPanel, elementName, elementType, ReflectionExt.GetPassableAttributes(1, attributes), true, new Type[0]);

                    bool itemExists = false;

                    List<object> states = (List<object>)mv.GetList(typeof(List<object>));
                    for (int ii = 0; ii < states.Count; ii++)
                    {
                        //ignore the current index being edited
                        //if the element is null, then we are editing a new object, so skip
                        if (ii != index || element == null)
                        {
                            if (states[ii].GetType() == newElement.GetType())
                                itemExists = true;
                        }
                    }

                    if (itemExists)
                    {
                        await MessageBox.Show(control.GetOwningForm(), "Cannot add duplicate states.", "Entry already exists.", MessageBox.MessageBoxButtons.Ok);
                        return false;
                    }
                    else
                    {
                        op(index, newElement);
                        return true;
                    }
                };

                control.GetOwningForm().RegisterChild(frmData);
                frmData.Show();
            };

            List<object> states = new List<object>();
            foreach (object state in member)
                states.Add(state);
            mv.LoadFromList(states);
            control.Children.Add(lbxValue);
        }

        public override ITypeDict SaveWindowControls(StackPanel control, string name, Type type, object[] attributes, Type[] subGroupStack)
        {
            int controlIndex = 0;

            CollectionBox lbxValue = (CollectionBox)control.Children[controlIndex];

            ITypeDict member = (ITypeDict)Activator.CreateInstance(type);
            CollectionBoxViewModel mv = (CollectionBoxViewModel)lbxValue.DataContext;
            List<object> states = (List<object>)mv.GetList(typeof(List<object>));
            for (int ii = 0; ii < states.Count; ii++)
                member.Set(states[ii]);
            return member;
        }
    }
}
