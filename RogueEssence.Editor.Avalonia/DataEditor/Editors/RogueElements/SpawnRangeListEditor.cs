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
using Avalonia.Interactivity;

namespace RogueEssence.Dev
{
    public class SpawnRangeListEditor : Editor<ISpawnRangeList>
    {
        /// <summary>
        /// Default display behavior of whether to treat 0s as 1s
        /// </summary>
        public bool Index1;

        /// <summary>
        /// Default display behavior of whether to treat end borders exclsusively
        /// </summary>
        public bool Inclusive;

        public SpawnRangeListEditor(bool index1, bool inclusive)
        {
            Index1 = index1;
            Inclusive = inclusive;
        }

        public override bool DefaultSubgroup => true;
        public override bool DefaultDecoration => false;

        public override void LoadWindowControls(StackPanel control, string parent, Type parentType, string name, Type type, object[] attributes, ISpawnRangeList member, Type[] subGroupStack)
        {
            Type elementType = ReflectionExt.GetBaseTypeArg(typeof(ISpawnRangeList<>), type, 0);

            SpawnRangeListBox lbxValue = new SpawnRangeListBox();

            EditorHeightAttribute heightAtt = ReflectionExt.FindAttribute<EditorHeightAttribute>(attributes);
            if (heightAtt != null)
                lbxValue.MaxHeight = heightAtt.Height;
            else
                lbxValue.MaxHeight = 260;

            SpawnRangeListBoxViewModel vm = new SpawnRangeListBoxViewModel(control.GetOwningForm(), new StringConv(elementType, ReflectionExt.GetPassableAttributes(1, attributes)));

            vm.Index1 = Index1;
            vm.Inclusive = Inclusive;

            CollectionAttribute confirmAtt = ReflectionExt.FindAttribute<CollectionAttribute>(attributes);
            if (confirmAtt != null)
                vm.ConfirmDelete = confirmAtt.ConfirmDelete;

            RangeBorderAttribute rangeAtt = ReflectionExt.FindAttribute<RangeBorderAttribute>(attributes);
            if (rangeAtt != null)
            {
                vm.Index1 = rangeAtt.Index1;
                vm.Inclusive = rangeAtt.Inclusive;
            }

            lbxValue.DataContext = vm;
            lbxValue.SetListContextMenu(createContextMenu(control, type, vm));
            lbxValue.MinHeight = lbxValue.MaxHeight;//TODO: Uptake Avalonia fix for improperly updating Grid control dimensions


            //add lambda expression for editing a single element
            vm.OnEditItem += (int index, object element, bool advancedEdit, SpawnRangeListBoxViewModel.EditElementOp op) =>
            {
                string elementName = name + "[" + index + "]";
                DataEditForm frmData = new DataEditForm();
                frmData.Title = DataEditor.GetWindowTitle(parent, elementName, element, elementType, ReflectionExt.GetPassableAttributes(2, attributes));

                DataEditor.LoadClassControls(frmData.ControlPanel, parent, null, elementName, elementType, ReflectionExt.GetPassableAttributes(2, attributes), element, true, new Type[0], advancedEdit);
                DataEditor.TrackTypeSize(frmData, elementType);

                frmData.SelectedOKEvent += async () =>
                {
                    element = DataEditor.SaveClassControls(frmData.ControlPanel, elementName, elementType, ReflectionExt.GetPassableAttributes(2, attributes), true, new Type[0], advancedEdit);
                    op(index, element);
                    return true;
                };

                control.GetOwningForm().RegisterChild(frmData);
                frmData.Show();
            };

            vm.LoadFromList(member);
            control.Children.Add(lbxValue);
        }


        private static ContextMenu createContextMenu(StackPanel control, Type type, SpawnRangeListBoxViewModel vm)
        {
            Type elementType = ReflectionExt.GetBaseTypeArg(typeof(ISpawnRangeList<>), type, 0);

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
                if (vm.CurrentElement > -1)
                {
                    object obj = vm.Collection[vm.CurrentElement].Value;
                    DataEditor.SetClipboardObj(obj, null);
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
                    int idx = vm.CurrentElement;
                    if (idx < 0)
                        idx = vm.Collection.Count;
                    vm.InsertOnKey(idx, DataEditor.clipboardObj);
                }
                else
                    await MessageBox.Show(control.GetOwningForm(), String.Format("Incompatible types:\n{0}\n{1}", type1.AssemblyQualifiedName, type2.AssemblyQualifiedName), "Invalid Operation", MessageBox.MessageBoxButtons.Ok);
            };
            return copyPasteStrip;
        }

        public override ISpawnRangeList SaveWindowControls(StackPanel control, string name, Type type, object[] attributes, Type[] subGroupStack)
        {
            int controlIndex = 0;

            SpawnRangeListBox lbxValue = (SpawnRangeListBox)control.Children[controlIndex];
            SpawnRangeListBoxViewModel mv = (SpawnRangeListBoxViewModel)lbxValue.DataContext;
            return mv.GetList(type);
        }
    }
}
