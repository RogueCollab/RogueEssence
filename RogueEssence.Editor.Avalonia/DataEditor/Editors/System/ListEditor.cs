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
using System.Collections;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;
using DynamicData;
using RogueEssence.Dev.Services;
using RogueEssence.Dev.Utility;

namespace RogueEssence.Dev
{
    public class ListEditor : Editor<IList>
    {
        public ListEditor(EditorContext context) : base(context) { }
        public override bool DefaultSubgroup => true;
        public override bool DefaultDecoration => false;
        public override bool DefaultType => true;

        public override void LoadWindowControls(StackPanel control, string parent, Type parentType, string name, Type type, object[] attributes, IList member, Type[] subGroupStack)
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
                lbxValue.SetListContextMenu(CreateContextMenu(_context.DialogService, control, type, vm));
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
                lbxValue.SetListContextMenu(CreateContextMenu(_context.DialogService, control, type, vm));
                lbxValue.MinHeight = lbxValue.MaxHeight;//TODO: Uptake Avalonia fix for improperly updating Grid control dimensions
                control.Children.Add(lbxValue);
            }
        }

        public static ContextMenu CreateContextMenu(IDialogService dialogService, StackPanel control, Type type, CollectionBoxViewModel vm)
        {
            Type elementType = ReflectionExt.GetBaseTypeArg(typeof(IList<>), type, 0);

            ContextMenu copyPasteStrip = new ContextMenu();

            MenuItem copyToolStripMenuItem = new MenuItem();
            MenuItem pasteToolStripMenuItem = new MenuItem();

            Avalonia.Collections.AvaloniaList<object> list = new Avalonia.Collections.AvaloniaList<object>();
            list.AddRange(new MenuItem[] {
                            copyToolStripMenuItem,
                            pasteToolStripMenuItem});

            copyPasteStrip.ItemsSource = list;
            copyToolStripMenuItem.Header = "Copy List Element: " + elementType.Name;
            pasteToolStripMenuItem.Header = "Insert List Element: " + elementType.Name;

            copyToolStripMenuItem.Click += async (object copySender, RoutedEventArgs copyE) =>
            {
                if (vm.SelectedIndex > -1)
                {
                    object obj = vm.Collection[vm.SelectedIndex].Value;
                    DataEditor.SetClipboardObj(obj, null);
                }
                else
                    await MessageBoxWindowView.Show(dialogService, String.Format("No index selected!"), "Invalid Operation", MessageBoxWindowView.MessageBoxButtons.Ok);
            };
            pasteToolStripMenuItem.Click += async (object copySender, RoutedEventArgs copyE) =>
            {
                Type type1 = DataEditor.clipboardObj.GetType();
                Type type2 = elementType;
                if (type2.IsAssignableFrom(type1))
                {
                    int idx = vm.SelectedIndex;
                    if (idx < 0)
                        idx = vm.Collection.Count;
                    vm.InsertItem(idx, DataEditor.clipboardObj);
                }
                else
                    await MessageBoxWindowView.Show(dialogService, String.Format("Incompatible types:\n{0}\n{1}", type1.AssemblyQualifiedName, type2.AssemblyQualifiedName), "Invalid Operation", MessageBoxWindowView.MessageBoxButtons.Ok);
            };
            return copyPasteStrip;
        }


        private CollectionBoxViewModel createViewModel(StackPanel control, string parent, string name, Type type, object[] attributes, IList member, bool index1)
        {
            Type elementType = ReflectionExt.GetBaseTypeArg(typeof(IList<>), type, 0);

            CollectionBoxViewModel vm = new CollectionBoxViewModel(_context.DialogService, new StringConv(elementType, ReflectionExt.GetPassableAttributes(1, attributes)));
            vm.Index1 = index1;

            CollectionAttribute confirmAtt = ReflectionExt.FindAttribute<CollectionAttribute>(attributes);
            if (confirmAtt != null)
                vm.ConfirmDelete = confirmAtt.ConfirmDelete;

          
            
            // //add lambda expression for editing a single element
            // vm.OnEditItem += (int index, object element, bool advancedEdit, CollectionBoxViewModel.EditElementOp op) =>
            // {
            //     Console.WriteLine("TRY EDIT");
            //     // public void TestMethod()
            //     // {
            //     //     NodeBase node = NodeFactory.CreateReflectedDataNode<ReflectedDataPageViewModel>("New Node [TODO Change Title]", "Icons.PaintBrushFill");
            //     //     Node.SubNodes.Add(node);
            //     //     var editor = PageFactory.CreatePage<ReflectedDataPageViewModel>(node);
            //     //     if (editor != null)
            //     //     {
            //     //         editor.SetPageTitle("New Node [TODO Change Title]", Node.Icon);
            //     //         TabEvents.AddChildPage(this, editor);
            //     //     }
            //     // }
            //     
            //     
            //     
            //     var new_editor = pageViewModel.PageFactory.CreatePage<ReflectedDataPageViewModel>(pageViewModel.Node);
            //     var locator = new ViewLocator();
            //     ReflectedDataPageView view = locator.Build(new_editor) as ReflectedDataPageView;
            //     
            //     Console.WriteLine(view + "VIEW DONE");
            //  
            //
            //     
            //     string elementName = name + "[" + index + "]";
            //     string title = DataEditor.GetWindowTitle(parent, elementName, element, elementType, ReflectionExt.GetPassableAttributes(1, attributes));
            //     DataEditor.LoadClassControlsReflected(view.ControlPanel, parent, null, elementName, elementType, ReflectionExt.GetPassableAttributes(1, attributes), element, true, new Type[0], advancedEdit);
            //
            //     NodeBase node = pageViewModel.NodeFactory.CreateReflectedDataNode<ReflectedDataPageViewModel>(title, pageViewModel.Node.Icon);
            //     
            //     pageViewModel.Node.SubNodes.Add(node);
            //     // ReflectedDataPageViewModel editor = PageFactory.CreatePage<ReflectedDataPageViewModel>(node);
            //     if (new_editor != null)
            //     {
            //         new_editor.SetPageTitle(title, pageViewModel.Node.Icon);
            //         pageViewModel.TabEvents.AddChildPage(pageViewModel, new_editor);
            //     }
            //     
            //     view.SelectedOKEvent += async () =>
            //     {
            //         element = DataEditor.SaveClassControls(view.ControlPanel, elementName, elementType, ReflectionExt.GetPassableAttributes(1, attributes), true, new Type[0], advancedEdit);
            //         op(index, element);
            //         return true;
            //     };
            //
            //     
            //     // DataEditor.TrackTypeSize(frmData, elementType);
            //     //
            //     // frmData.SelectedOKEvent += async () =>
            //     // {
            //     //     element = DataEditor.SaveClassControls(frmData.ControlPanel, elementName, elementType, ReflectionExt.GetPassableAttributes(1, attributes), true, new Type[0], advancedEdit);
            //     //     op(index, element);
            //     //     return true;
            //     // };
            //
            //     // control.GetOwningForm().RegisterChild(frmData);
            //     // // control.FindAncestorOfType<TreeDataGr>()
            //
            //     // frmData.Show();
            // };
            
            //add lambda expression for editing a single element
            vm.OnEditItem += (int index, object element, bool advancedEdit, CollectionBoxViewModel.EditElementOp op) =>
            {
                EditorPageViewModel pageViewModel = control.FindAncestorViewModel<EditorPageViewModel>();
                string elementName = name + "[" + index + "]";
                // string title = DataEditor.GetWindowTitle(parent, elementName, element, elementType, ReflectionExt.GetPassableAttributes(1, attributes));

                NodeBase node = _context.NodeFactory.CreateReflectedDataNode<ReflectedDataPageViewModel>(elementName, pageViewModel.Node, pageViewModel.Node.Icon);
                
                pageViewModel.Node.AddNodeIfNotExists(node);
                
                NodeHelper.ExpandParents(node, true);
                ReflectedDataPageViewModel newEditor = _context.PageFactory.CreatePage<ReflectedDataPageViewModel>(node);
                newEditor.SetPageTitle(elementName, pageViewModel.Node.Icon);
                
                newEditor.OnLoadAction = (StackPanel stack) =>
                {
                    DataEditor.LoadClassControls(stack, elementName, null, elementName, elementType, ReflectionExt.GetPassableAttributes(1, attributes), element, true, new Type[0], advancedEdit);
                };

                newEditor.OnOKAction = async (StackPanel stack) =>
                {
                    element = DataEditor.SaveClassControls(stack, elementName, elementType, ReflectionExt.GetPassableAttributes(1, attributes), true, new Type[0], advancedEdit);
                    op(index, element);
                    return true;
                };

                _context.TabEvents.AddChildPage(pageViewModel, newEditor);
            };
            vm.LoadFromList(member);
            return vm;
        }

        public override IList SaveWindowControls(StackPanel control, string name, Type type, object[] attributes, Type[] subGroupStack)
        {
            int controlIndex = 0;

            Control lbxValue = control.Children[controlIndex];
            CollectionBoxViewModel mv = (CollectionBoxViewModel)lbxValue.DataContext;
            return mv.GetList(type);
        }
    }
}
