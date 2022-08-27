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

namespace RogueEssence.Dev
{
    public class DictionaryEditor : Editor<IDictionary>
    {
        public override bool DefaultSubgroup => true;
        public override bool DefaultDecoration => false;
        public override bool DefaultType => true;

        public override void LoadWindowControls(StackPanel control, string parent, Type parentType, string name, Type type, object[] attributes, IDictionary member, Type[] subGroupStack)
        {
            Type keyType = ReflectionExt.GetBaseTypeArg(typeof(IDictionary<,>), type, 0);
            Type elementType = ReflectionExt.GetBaseTypeArg(typeof(IDictionary<,>), type, 1);

            DictionaryBox lbxValue = new DictionaryBox();
            
            EditorHeightAttribute heightAtt = ReflectionExt.FindAttribute<EditorHeightAttribute>(attributes);
            if (heightAtt != null)
                lbxValue.MaxHeight = heightAtt.Height;
            else
                lbxValue.MaxHeight = 180;

            DictionaryBoxViewModel vm = new DictionaryBoxViewModel(control.GetOwningForm(), new StringConv(elementType, ReflectionExt.GetPassableAttributes(2, attributes)));
            lbxValue.DataContext = vm;
            lbxValue.MinHeight = lbxValue.MaxHeight;//TODO: Uptake Avalonia fix for improperly updating Grid control dimensions

            //add lambda expression for editing a single element
            vm.OnEditItem += (object key, object element, DictionaryBoxViewModel.EditElementOp op) =>
            {
                string elementName = name + "[" + key.ToString() + "]";
                DataEditForm frmData = new DataEditForm();
                frmData.Title = DataEditor.GetWindowTitle(parent, elementName, element, elementType, ReflectionExt.GetPassableAttributes(2, attributes));

                DataEditor.LoadClassControls(frmData.ControlPanel, parent, null, elementName, elementType, ReflectionExt.GetPassableAttributes(2, attributes), element, true, new Type[0]);
                DataEditor.TrackTypeSize(frmData, elementType);

                frmData.SelectedOKEvent += async () =>
                {
                    element = DataEditor.SaveClassControls(frmData.ControlPanel, elementName, elementType, ReflectionExt.GetPassableAttributes(2, attributes), true, new Type[0]);
                    op(key, key, element);
                    return true;
                };

                control.GetOwningForm().RegisterChild(frmData);
                frmData.Show();
            };

            vm.OnEditKey += (object key, object element, DictionaryBoxViewModel.EditElementOp op) =>
            {
                string elementName = name + "<Key>";
                DataEditForm frmData = new DataEditForm();
                frmData.Title = DataEditor.GetWindowTitle(parent, elementName, key, keyType, ReflectionExt.GetPassableAttributes(1, attributes));

                DataEditor.LoadClassControls(frmData.ControlPanel, parent, null, elementName, keyType, ReflectionExt.GetPassableAttributes(1, attributes), key, true, new Type[0]);
                DataEditor.TrackTypeSize(frmData, keyType);

                frmData.SelectedOKEvent += async () =>
                {
                    object newKey = DataEditor.SaveClassControls(frmData.ControlPanel, elementName, keyType, ReflectionExt.GetPassableAttributes(1, attributes), true, new Type[0]);
                    op(key, newKey, element);
                    return true;
                };

                control.GetOwningForm().RegisterChild(frmData);
                frmData.Show();
            };

            vm.LoadFromDict(member);
            lbxValue.SetListContextMenu(createContextMenu(control, type, vm));
            control.Children.Add(lbxValue);
        }

        private ContextMenu createContextMenu(StackPanel control, Type type, DictionaryBoxViewModel vm)
        {
            Type keyType = ReflectionExt.GetBaseTypeArg(typeof(IDictionary<,>), type, 0);
            Type elementType = ReflectionExt.GetBaseTypeArg(typeof(IDictionary<,>), type, 1);

            ContextMenu copyPasteStrip = new ContextMenu();

            MenuItem renameToolStripMenuItem = new MenuItem();

            Avalonia.Collections.AvaloniaList<object> list = (Avalonia.Collections.AvaloniaList<object>)copyPasteStrip.Items;
            list.AddRange(new MenuItem[] {
                            renameToolStripMenuItem});

            renameToolStripMenuItem.Header = "Rename " + elementType.Name;

            renameToolStripMenuItem.Click += async (object copySender, RoutedEventArgs copyE) =>
            {
                if (vm.SelectedIndex > -1)
                    vm.EditKey(vm.SelectedIndex);
                else
                    await MessageBox.Show(control.GetOwningForm(), String.Format("No index selected!"), "Invalid Operation", MessageBox.MessageBoxButtons.Ok);
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
