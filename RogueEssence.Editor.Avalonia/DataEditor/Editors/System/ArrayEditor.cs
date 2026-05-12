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
using RogueEssence.Dev.Utility;
using RogueEssence.Dev.ViewModels;

namespace RogueEssence.Dev
{
    public class ArrayEditor : Editor<Array>
    {
        public ArrayEditor(EditorContext context) : base(context) { }
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

                CollectionBoxViewModel vm = createViewModel(control, parent, name, type, attributes, member, rangeAtt.Index1);
                lbxValue.DataContext = vm;
                lbxValue.SetListContextMenu(ListEditor.CreateContextMenu(_context.DialogService, control, type, vm));
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

                CollectionBoxViewModel vm = createViewModel(control, parent, name, type, attributes, member, false);
                lbxValue.DataContext = vm;
                lbxValue.SetListContextMenu(ListEditor.CreateContextMenu(_context.DialogService, control, type, vm));
                lbxValue.MinHeight = lbxValue.MaxHeight;//TODO: Uptake Avalonia fix for improperly updating Grid control dimensions
                control.Children.Add(lbxValue);
            }

        }

        private CollectionBoxViewModel createViewModel(StackPanel control, string parent, string name, Type type, object[] attributes, Array member, bool index1)
        {
            Type elementType = type.GetElementType();

            CollectionBoxViewModel vm = new CollectionBoxViewModel(_context.DialogService, new StringConv(elementType, ReflectionExt.GetPassableAttributes(1, attributes)));
            vm.Index1 = index1;
            CollectionAttribute confirmAtt = ReflectionExt.FindAttribute<CollectionAttribute>(attributes);
            if (confirmAtt != null)
                vm.ConfirmDelete = confirmAtt.ConfirmDelete;

            //add lambda expression for editing a single element
            vm.OnEditItem += (int index, object element, bool advancedEdit, CollectionBoxViewModel.EditElementOp op) =>
            {
                EditorPageViewModel pageViewModel = control.FindAncestorViewModel<EditorPageViewModel>();
                string elementName = name + "[" + index + "]";
                string title = DataEditor.GetWindowTitle(parent, elementName, element, elementType, ReflectionExt.GetPassableAttributes(0, attributes));

                NodeBase node = _context.NodeFactory.CreateReflectedDataNode<ReflectedDataPageViewModel>(elementName, pageViewModel.Node.Icon);
                pageViewModel.Node.AddNodeIfNotExists(node);

                NodeHelper.ExpandParents(node, true);
                ReflectedDataPageViewModel newEditor = _context.PageFactory.CreatePage<ReflectedDataPageViewModel>(node);
                newEditor.SetPageTitle(title, pageViewModel.Node.Icon);

                newEditor.OnLoadAction = (StackPanel stack) =>
                {
                    DataEditor.LoadClassControls(stack, parent, null, elementName, elementType, ReflectionExt.GetPassableAttributes(0, attributes), element, true, new Type[0], advancedEdit);
                };

                newEditor.OnOKAction = async (StackPanel stack) =>
                {
                    element = DataEditor.SaveClassControls(stack, elementName, elementType, ReflectionExt.GetPassableAttributes(0, attributes), true, new Type[0], advancedEdit);
                    op(index, element);
                    return true;
                };

                _context.TabEvents.AddChildPage(pageViewModel, newEditor);
            };


            List<object> objList = new List<object>();
            for (int ii = 0; ii < member.Length; ii++)
                objList.Add(member.GetValue(ii));

            vm.LoadFromList(objList);
            return vm;
        }

        public override Array SaveWindowControls(StackPanel control, string name, Type type, object[] attributes, Type[] subGroupStack)
        {
            int controlIndex = 0;
            //TODO: 2D array grid support
            //if (type.GetElementType().IsArray)

            Control lbxValue = control.Children[controlIndex];
            CollectionBoxViewModel mv = (CollectionBoxViewModel)lbxValue.DataContext;
            List<object> objList = (List<object>)mv.GetList(typeof(List<object>));

            Array array = Array.CreateInstance(type.GetElementType(), objList.Count);
            for (int ii = 0; ii < objList.Count; ii++)
                array.SetValue(objList[ii], ii);

            return array;
        }
    }
}
