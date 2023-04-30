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
using RogueEssence.LevelGen;
using Avalonia.Interactivity;

namespace RogueEssence.Dev
{
    public class RangeDictEditor : Editor<IRangeDict>
    {
        /// <summary>
        /// Default display behavior of whether to treat 0s as 1s
        /// </summary>
        public bool Index1;

        /// <summary>
        /// Default display behavior of whether to treat end borders exclsusively
        /// </summary>
        public bool Inclusive;

        public RangeDictEditor(bool index1, bool inclusive)
        {
            Index1 = index1;
            Inclusive = inclusive;
        }

        public override bool DefaultSubgroup => true;
        public override bool DefaultDecoration => false;
        public override bool DefaultType => true;

        public override void LoadWindowControls(StackPanel control, string parent, Type parentType, string name, Type type, object[] attributes, IRangeDict member, Type[] subGroupStack)
        {
            Type keyType = typeof(IntRange);
            Type elementType = ReflectionExt.GetBaseTypeArg(typeof(IRangeDict<>), type, 0);


            RangeDictBox lbxValue = new RangeDictBox();

            EditorHeightAttribute heightAtt = ReflectionExt.FindAttribute<EditorHeightAttribute>(attributes);
            if (heightAtt != null)
                lbxValue.MaxHeight = heightAtt.Height;
            else
                lbxValue.MaxHeight = 180;

            RangeDictBoxViewModel vm = new RangeDictBoxViewModel(control.GetOwningForm(), new StringConv(elementType, ReflectionExt.GetPassableAttributes(1, attributes)));

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

            //add lambda expression for editing a single element
            vm.OnEditItem += (IntRange key, object element, RangeDictBoxViewModel.EditElementOp op) =>
            {
                string elementName = name + "[" + key.ToString() + "]";
                DataEditForm frmData = new DataEditForm();
                frmData.Title = DataEditor.GetWindowTitle(parent, elementName, element, elementType, ReflectionExt.GetPassableAttributes(1, attributes));

                DataEditor.LoadClassControls(frmData.ControlPanel, parent, null, elementName, elementType, ReflectionExt.GetPassableAttributes(1, attributes), element, true, new Type[0]);
                DataEditor.TrackTypeSize(frmData, elementType);

                frmData.SelectedOKEvent += async () =>
                {
                    element = DataEditor.SaveClassControls(frmData.ControlPanel, elementName, elementType, ReflectionExt.GetPassableAttributes(1, attributes), true, new Type[0]);
                    op(key, element);
                    return true;
                };

                control.GetOwningForm().RegisterChild(frmData);
                frmData.Show();
            };

            vm.OnEditKey += (IntRange key, object element, RangeDictBoxViewModel.EditElementOp op) =>
            {
                string elementName = name + "<Range>";
                DataEditForm frmKey = new DataEditForm();

                List<object> attrList = new List<object>();
                if (rangeAtt != null)
                    attrList.Add(rangeAtt);
                frmKey.Title = DataEditor.GetWindowTitle(parent, elementName, key, keyType, attrList.ToArray());

                DataEditor.LoadClassControls(frmKey.ControlPanel, parent, null, elementName, keyType, attrList.ToArray(), key, true, new Type[0]);
                DataEditor.TrackTypeSize(frmKey, keyType);

                frmKey.SelectedOKEvent += async () =>
                {
                    key = (IntRange)DataEditor.SaveClassControls(frmKey.ControlPanel, elementName, keyType, attrList.ToArray(), true, new Type[0]);
                    op(key, element);
                    return true;
                };

                control.GetOwningForm().RegisterChild(frmKey);
                frmKey.Show();
            };

            vm.LoadFromDict(member);
            control.Children.Add(lbxValue);
        }

        private static ContextMenu createContextMenu(StackPanel control, Type type, RangeDictBoxViewModel vm)
        {
            Type elementType = ReflectionExt.GetBaseTypeArg(typeof(IRangeDict<>), type, 0);

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


        public override IRangeDict SaveWindowControls(StackPanel control, string name, Type type, object[] attributes, Type[] subGroupStack)
        {
            int controlIndex = 0;

            RangeDictBox lbxValue = (RangeDictBox)control.Children[controlIndex];
            RangeDictBoxViewModel mv = (RangeDictBoxViewModel)lbxValue.DataContext;
            return mv.GetDict(type);
        }
    }
}
