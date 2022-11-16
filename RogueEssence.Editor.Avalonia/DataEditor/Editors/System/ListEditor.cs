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

namespace RogueEssence.Dev
{
    public class ListEditor : Editor<IList>
    {
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
                    lbxValue.MaxHeight = 220;

                CollectionBoxViewModel vm = createViewModel(control, parent, name, type, attributes, member, rangeAtt.Index1);
                lbxValue.DataContext = vm;
                lbxValue.SetListContextMenu(createContextMenu(control, type, vm));
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
                lbxValue.SetListContextMenu(createContextMenu(control, type, vm));
                control.Children.Add(lbxValue);
            }
        }

        private ContextMenu createContextMenu(StackPanel control, Type type, CollectionBoxViewModel vm)
        {
            Type elementType = ReflectionExt.GetBaseTypeArg(typeof(IList<>), type, 0);

            ContextMenu copyPasteStrip = new ContextMenu();

            MenuItem copyToolStripMenuItem = new MenuItem();
            MenuItem pasteToolStripMenuItem = new MenuItem();

            Avalonia.Collections.AvaloniaList<object> list = (Avalonia.Collections.AvaloniaList<object>)copyPasteStrip.Items;
            list.AddRange(new MenuItem[] {
                            copyToolStripMenuItem,
                            pasteToolStripMenuItem});

            copyToolStripMenuItem.Header = "Copy List Element: " + elementType.Name;
            pasteToolStripMenuItem.Header = "Insert List Element: " + elementType.Name;

            copyToolStripMenuItem.Click += async (object copySender, RoutedEventArgs copyE) =>
            {
                if (vm.SelectedIndex > -1)
                {
                    object obj = vm.Collection[vm.SelectedIndex].Value;
                    DataEditor.SetClipboardObj(obj);
                }
                else
                    await MessageBox.Show(control.GetOwningForm(), String.Format("No index selected!"), "Invalid Operation", MessageBox.MessageBoxButtons.Ok);
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
                    await MessageBox.Show(control.GetOwningForm(), String.Format("Incompatible types:\n{0}\n{1}", type1.AssemblyQualifiedName, type2.AssemblyQualifiedName), "Invalid Operation", MessageBox.MessageBoxButtons.Ok);
            };
            return copyPasteStrip;
        }

        private CollectionBoxViewModel createViewModel(StackPanel control, string parent, string name, Type type, object[] attributes, IList member, bool index1)
        {
            Type elementType = ReflectionExt.GetBaseTypeArg(typeof(IList<>), type, 0);

            CollectionBoxViewModel mv = new CollectionBoxViewModel(new StringConv(elementType, ReflectionExt.GetPassableAttributes(1, attributes)));
            mv.Index1 = index1;
            //add lambda expression for editing a single element
            mv.OnEditItem += (int index, object element, CollectionBoxViewModel.EditElementOp op) =>
            {
                string elementName = name + "[" + index + "]";
                DataEditForm frmData = new DataEditForm();
                frmData.Title = DataEditor.GetWindowTitle(parent, elementName, element, elementType, ReflectionExt.GetPassableAttributes(1, attributes));

                DataEditor.LoadClassControls(frmData.ControlPanel, parent, null, elementName, elementType, ReflectionExt.GetPassableAttributes(1, attributes), element, true, new Type[0]);
                DataEditor.TrackTypeSize(frmData, elementType);

                frmData.SelectedOKEvent += async () =>
                {
                    element = DataEditor.SaveClassControls(frmData.ControlPanel, elementName, elementType, ReflectionExt.GetPassableAttributes(1, attributes), true, new Type[0]);
                    op(index, element);
                    return true;
                };

                control.GetOwningForm().RegisterChild(frmData);
                frmData.Show();
            };

            mv.LoadFromList(member);
            return mv;
        }

        public override IList SaveWindowControls(StackPanel control, string name, Type type, object[] attributes, Type[] subGroupStack)
        {
            int controlIndex = 0;

            IControl lbxValue = control.Children[controlIndex];
            CollectionBoxViewModel mv = (CollectionBoxViewModel)lbxValue.DataContext;
            return mv.GetList(type);
        }
    }
}
