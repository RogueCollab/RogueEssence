using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using RogueEssence.Content;
using RogueEssence.Dungeon;
using RogueEssence.Data;
using System.Drawing;
using RogueElements;
using Avalonia.Controls;
using RogueEssence.Dev.Utility;
using RogueEssence.Dev.Views;
using RogueEssence.Dev.ViewModels;

namespace RogueEssence.Dev
{
    //TODO: there is no parameterless interface for hashset
    //so instead we have to do the painful process of manually adding every hashset of every type we actually use.  ugh
    public class HashSetEditor<T> : Editor<HashSet<T>>
    {
        public HashSetEditor(EditorContext context) : base(context) { }
        public override bool DefaultSubgroup => true;
        public override bool DefaultDecoration => false;
        public override bool DefaultType => true;

        public override void LoadWindowControls(StackPanel control, string parent, Type parentType, string name, Type type, object[] attributes, HashSet<T> member, Type[] subGroupStack)
        {
            Type elementType = typeof(T);

            CollectionBox lbxValue = new CollectionBox();
            EditorHeightAttribute heightAtt = ReflectionExt.FindAttribute<EditorHeightAttribute>(attributes);
            if (heightAtt != null)
                lbxValue.MaxHeight = heightAtt.Height;
            else
                lbxValue.MaxHeight = 200;

            CollectionBoxViewModel vm = new CollectionBoxViewModel(_context.DialogService, new StringConv(elementType, ReflectionExt.GetPassableAttributes(1, attributes)));

            CollectionAttribute confirmAtt = ReflectionExt.FindAttribute<CollectionAttribute>(attributes);
            if (confirmAtt != null)
                vm.ConfirmDelete = confirmAtt.ConfirmDelete;

            lbxValue.DataContext = vm;
            lbxValue.MinHeight = lbxValue.MaxHeight;//TODO: Uptake Avalonia fix for improperly updating Grid control dimensions

            //add lambda expression for editing a single element
            vm.OnEditItem += (int index, object element, bool advancedEdit, CollectionBoxViewModel.EditElementOp op) =>
            {
                EditorPageViewModel pageViewModel = control.FindAncestorViewModel<EditorPageViewModel>();
                string elementName = name + "[" + index + "]";
                string title = DataEditor.GetWindowTitle(parent, elementName, element, elementType, ReflectionExt.GetPassableAttributes(1, attributes));

                NodeBase node = _context.NodeFactory.CreateReflectedDataNode<ReflectedDataPageViewModel>(elementName, pageViewModel.Node, pageViewModel.Node.Icon);
                pageViewModel.Node.AddNodeIfNotExists(node);

                NodeHelper.ExpandParents(node, true);
                ReflectedDataPageViewModel newEditor = _context.PageFactory.CreatePage<ReflectedDataPageViewModel>(node);
                newEditor.SetPageTitle(title, pageViewModel.Node.Icon);

                newEditor.OnLoadAction = (StackPanel stack) =>
                {
                    DataEditor.LoadClassControls(stack, parent, parentType, elementName, elementType, ReflectionExt.GetPassableAttributes(1, attributes), element, true, new Type[0], advancedEdit);
                };

                newEditor.OnOKAction = async (StackPanel stack) =>
                {
                    object newElement = DataEditor.SaveClassControls(stack, elementName, elementType, ReflectionExt.GetPassableAttributes(1, attributes), true, new Type[0], advancedEdit);

                    bool itemExists = false;
                    List<object> states = (List<object>)vm.GetList(typeof(List<object>));
                    for (int ii = 0; ii < states.Count; ii++)
                    {
                        if (ii != index || element == null)
                        {
                            if (states[ii].Equals(newElement))
                                itemExists = true;
                        }
                    }

                    if (itemExists)
                    {
                        await MessageBoxWindowView.Show(_context.DialogService, "Cannot add duplicate items.", "Entry already exists.", MessageBoxWindowView.MessageBoxButtons.Ok);
                        return false;
                    }
                    else
                    {
                        op(index, newElement);
                        return true;
                    }
                };
                _context.TabEvents.AddChildPage(pageViewModel, newEditor);
            };

            List<object> states = new List<object>();
            foreach (object state in member)
                states.Add(state);
            vm.LoadFromList(states);
            control.Children.Add(lbxValue);
        }


        public override HashSet<T> SaveWindowControls(StackPanel control, string name, Type type, object[] attributes, Type[] subGroupStack)
        {
            int controlIndex = 0;

            Control lbxValue = control.Children[controlIndex];
            HashSet<T> member = (HashSet<T>)Activator.CreateInstance(type);
            CollectionBoxViewModel mv = (CollectionBoxViewModel)lbxValue.DataContext;
            List<object> states = (List<object>)mv.GetList(typeof(List<object>));
            for (int ii = 0; ii < states.Count; ii++)
                member.Add((T)states[ii]);
            return member;


        }
    }
}
