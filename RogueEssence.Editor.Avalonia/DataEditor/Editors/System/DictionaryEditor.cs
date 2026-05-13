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
using Avalonia.Interactivity;
using RogueEssence.Dev.Services;
using RogueEssence.Dev.Utility;

namespace RogueEssence.Dev
{
    public class DictionaryEditor : Editor<IDictionary>
    {
        public DictionaryEditor(EditorContext context) : base(context) { }
        public override bool DefaultSubgroup => true;
        public override bool DefaultDecoration => false;
        public override bool DefaultType => true;

        public override void LoadWindowControls(StackPanel control, string parent, Type parentType, string name,
            Type type, object[] attributes, IDictionary member, Type[] subGroupStack)
        {
            Type keyType = ReflectionExt.GetBaseTypeArg(typeof(IDictionary<,>), type, 0);
            Type elementType = ReflectionExt.GetBaseTypeArg(typeof(IDictionary<,>), type, 1);

            DictionaryBox lbxValue = new DictionaryBox();

            EditorHeightAttribute heightAtt = ReflectionExt.FindAttribute<EditorHeightAttribute>(attributes);
            if (heightAtt != null)
                lbxValue.MaxHeight = heightAtt.Height;
            else
                lbxValue.MaxHeight = 200;

            DictionaryBoxViewModel vm = new DictionaryBoxViewModel(_context.DialogService,
                new StringConv(elementType, ReflectionExt.GetPassableAttributes(2, attributes)));
            
            CollectionAttribute confirmAtt = ReflectionExt.FindAttribute<CollectionAttribute>(attributes);
            if (confirmAtt != null)
                vm.ConfirmDelete = confirmAtt.ConfirmDelete;

            lbxValue.DataContext = vm;
            lbxValue.MinHeight = lbxValue.MaxHeight; //TODO: Uptake Avalonia fix for improperly updating Grid control dimensions

            //add lambda expression for editing a single element
            vm.OnEditItem += (object key, object element, bool advancedEdit, DictionaryBoxViewModel.EditElementOp op) =>
            {
                EditorPageViewModel pageViewModel = control.FindAncestorViewModel<EditorPageViewModel>();
                string elementName = name + "[" + key.ToString() + "]";
                string title = DataEditor.GetWindowTitle(parent, elementName, element, elementType,
                    ReflectionExt.GetPassableAttributes(2, attributes));

                NodeBase node =
                    _context.NodeFactory.CreateReflectedDataNode<ReflectedDataPageViewModel>(title,
                        pageViewModel.Node.Icon, pageViewModel.Node);
                pageViewModel.Node.AddNodeIfNotExists(node);

                NodeHelper.ExpandParents(node, true);
                ReflectedDataPageViewModel
                    newEditor = _context.PageFactory.CreatePage<ReflectedDataPageViewModel>(node);
                newEditor.SetPageTitle(title, pageViewModel.Node.Icon);

                newEditor.OnLoadAction = (StackPanel stack) =>
                {
                    DataEditor.LoadClassControls(stack, elementName, null, elementName, elementType,
                        ReflectionExt.GetPassableAttributes(2, attributes), element, true, new Type[0],
                        advancedEdit);
                };

                newEditor.OnOKAction = async (StackPanel stack) =>
                {
                    element = DataEditor.SaveClassControls(stack, elementName, elementType,
                        ReflectionExt.GetPassableAttributes(2, attributes), true, new Type[0], advancedEdit);
                    op(key, key, element);
                    return true;
                };

                _context.TabEvents.AddChildPage(pageViewModel, newEditor);
            };

            vm.OnEditKey += (object key, object element, bool advancedEdit, DictionaryBoxViewModel.EditElementOp op) =>
            {
                EditorPageViewModel pageViewModel = control.FindAncestorViewModel<EditorPageViewModel>();
                string elementName = name + "<Key>";
                string title = DataEditor.GetWindowTitle(parent, elementName, key, keyType,
                    ReflectionExt.GetPassableAttributes(1, attributes));

                NodeBase node =
                    _context.NodeFactory.CreateReflectedDataNode<ReflectedDataPageViewModel>(elementName, pageViewModel.Node.Icon);
                pageViewModel.Node.AddNodeIfNotExists(node);

                NodeHelper.ExpandParents(node, true);
                ReflectedDataPageViewModel
                    newEditor = _context.PageFactory.CreatePage<ReflectedDataPageViewModel>(node);
                newEditor.SetPageTitle(title, pageViewModel.Node.Icon);

                newEditor.OnLoadAction = (StackPanel stack) =>
                {
                    DataEditor.LoadClassControls(stack, parent, null, elementName, keyType,
                        ReflectionExt.GetPassableAttributes(1, attributes), key, true, new Type[0], advancedEdit);
                };

                newEditor.OnOKAction = async (StackPanel stack) =>
                {
                    object newKey = DataEditor.SaveClassControls(stack, elementName, keyType,
                        ReflectionExt.GetPassableAttributes(1, attributes), true, new Type[0], advancedEdit);
                    op(key, newKey, element);
                    return true;
                };

                _context.TabEvents.AddChildPage(pageViewModel, newEditor);
            };
            control.Children.Add(lbxValue);
            vm.LoadFromDict(member);
            lbxValue.SetListContextMenu(CreateContextMenu(_context.DialogService, control, type, vm));
        }

        public static ContextMenu CreateContextMenu(IDialogService dialogService, StackPanel control, Type type, DictionaryBoxViewModel vm)
        {
            Type keyType = ReflectionExt.GetBaseTypeArg(typeof(IDictionary<,>), type, 0);
            Type elementType = ReflectionExt.GetBaseTypeArg(typeof(IDictionary<,>), type, 1);

            ContextMenu copyPasteStrip = new ContextMenu();

            MenuItem renameToolStripMenuItem = new MenuItem();

            Avalonia.Collections.AvaloniaList<object> list = new Avalonia.Collections.AvaloniaList<object>();
            list.AddRange(new MenuItem[] {
                            renameToolStripMenuItem});

            copyPasteStrip.ItemsSource = list;
            renameToolStripMenuItem.Header = "Rename " + elementType.Name;

            renameToolStripMenuItem.Click += async (object copySender, RoutedEventArgs copyE) =>
            {
                if (vm.SelectedIndex > -1)
                    vm.EditKey(vm.SelectedIndex);
                else
                    await MessageBoxWindowView.Show(dialogService, String.Format("No index selected!"), "Invalid Operation", MessageBoxWindowView.MessageBoxButtons.Ok);
            };
            return copyPasteStrip;
        }


        public override IDictionary SaveWindowControls(StackPanel control, string name, Type type, object[] attributes, Type[] subGroupStack)
        {
            int controlIndex = 0;

            DictionaryBox lbxValue = (DictionaryBox)control.Children[controlIndex];
            DictionaryBoxViewModel mv = (DictionaryBoxViewModel)lbxValue.DataContext;
            return mv.GetDict(type);
        }
    }
}
