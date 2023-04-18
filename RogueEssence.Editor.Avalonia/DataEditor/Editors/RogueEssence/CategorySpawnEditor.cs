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
using RogueEssence.LevelGen;
using Avalonia;
using System.Reflection;

namespace RogueEssence.Dev
{
    //TODO: Category zone spawns need to be refactored into their own class with their own interface
    // from Dictionary<string, CategorySpawn<InvItem>> to ICategoryZoneSpawn.  This way it can catch other spawn methods
    public class CategorySpawnEditor : Editor<Dictionary<string, CategorySpawn<InvItem>>>
    {
        /// <summary>
        /// Default display behavior of whether to treat 0s as 1s
        /// </summary>
        public bool Index1;


        public CategorySpawnEditor(bool index1)
        {
            Index1 = index1;
        }



        public override bool DefaultSubgroup => true;
        public override bool DefaultDecoration => false;
        public override bool DefaultType => true;

        public override void LoadWindowControls(StackPanel control, string parent, Type parentType, string name, Type type, object[] attributes, Dictionary<string, CategorySpawn<InvItem>> member, Type[] subGroupStack)
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

            CollectionAttribute confirmAtt = ReflectionExt.FindAttribute<CollectionAttribute>(attributes);
            if (confirmAtt != null)
                vm.ConfirmDelete = confirmAtt.ConfirmDelete;

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
            lbxValue.SetListContextMenu(DictionaryEditor.CreateContextMenu(control, type, vm));
            control.Children.Add(lbxValue);



            Avalonia.Controls.Grid innerPanel = getSharedRowPanel(3);

            TextBlock lblX = new TextBlock();
            lblX.Text = "Floor:";
            lblX.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
            lblX.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right;
            innerPanel.Children.Add(lblX);
            innerPanel.ColumnDefinitions[0].Width = new GridLength(40);
            lblX.SetValue(Avalonia.Controls.Grid.ColumnProperty, 0);

            NumericUpDown nudValueTestFloor = new NumericUpDown();
            nudValueTestFloor.Margin = new Thickness(4, 4, 0, 0);
            nudValueTestFloor.Minimum = Int32.MinValue;
            nudValueTestFloor.Maximum = Int32.MaxValue;
            nudValueTestFloor.Value = Index1 ? 1 : 0;
            innerPanel.Children.Add(nudValueTestFloor);
            nudValueTestFloor.SetValue(Avalonia.Controls.Grid.ColumnProperty, 1);


            Button btnTest = new Button();
            btnTest.Margin = new Thickness(4, 4, 0, 0);
            btnTest.Content = "Get Summary";
            btnTest.Click += (object sender, RoutedEventArgs e) =>
            {
                int floor = (int)nudValueTestFloor.Value;
                if (Index1)
                    floor--;
                Dictionary<string, CategorySpawn<InvItem>> curSave = (Dictionary<string, CategorySpawn<InvItem>>)vm.GetDict(type);
                SpawnDict<string, SpawnList<InvItem>> spawns = new SpawnDict<string, SpawnList<InvItem>>();
                //contains all LISTS that intersect the current ID
                foreach (string key in curSave.Keys)
                {
                    //get all items within the spawnrangelist that intersect the current ID
                    SpawnList<InvItem> slicedList = curSave[key].Spawns.GetSpawnList(floor);

                    // add the spawnlist under the current key, with the key having the spawnrate for this id
                    if (slicedList.CanPick && curSave[key].SpawnRates.ContainsItem(floor) && curSave[key].SpawnRates[floor] > 0)
                        spawns.Add(key, slicedList, curSave[key].SpawnRates[floor]);
                }

                List<(object, double)> flatSpawns = CategorySpawnHelper.CollapseSpawnDict<string, InvItem>(spawns);

                DataEditForm frmData = new DataEditForm();
                frmData.Title = "Spawn Summary";
                StackPanel viewPanel = frmData.ControlPanel;

                SpawnListViewBox lbxValue = new SpawnListViewBox();
                SpawnListBoxViewModel mv = new SpawnListBoxViewModel(control.GetOwningForm(), new StringConv(elementType, ReflectionExt.GetPassableAttributes(1, attributes)));

                CollectionAttribute confirmAtt = ReflectionExt.FindAttribute<CollectionAttribute>(attributes);
                if (confirmAtt != null)
                    mv.ConfirmDelete = confirmAtt.ConfirmDelete;

                lbxValue.DataContext = mv;
                lbxValue.MaxHeight = 400;
                lbxValue.MinHeight = lbxValue.MaxHeight;//TODO: Uptake Avalonia fix for improperly updating Grid control dimensions

                mv.LoadFromTupleList(flatSpawns);
                viewPanel.Children.Add(lbxValue);

                frmData.SetViewOnly();

                control.GetOwningForm().RegisterChild(frmData);
                frmData.Show();
            };
            btnTest.SetValue(Avalonia.Controls.Grid.ColumnProperty, 2);
            innerPanel.ColumnDefinitions[2].Width = new GridLength(120);
            innerPanel.Children.Add(btnTest);

            control.Children.Add(innerPanel);
        }


        public override Dictionary<string, CategorySpawn<InvItem>> SaveWindowControls(StackPanel control, string name, Type type, object[] attributes, Type[] subGroupStack)
        {
            int controlIndex = 0;

            DictionaryBox lbxValue = (DictionaryBox)control.Children[controlIndex];
            DictionaryBoxViewModel mv = (DictionaryBoxViewModel)lbxValue.DataContext;
            return (Dictionary<string, CategorySpawn<InvItem>>)mv.GetDict(type);
        }
    }



    public class DictSpawnEditor : Editor<SpawnDict<string, SpawnList<InvItem>>>
    {
        public override bool DefaultSubgroup => true;
        public override bool DefaultDecoration => false;
        public override bool DefaultType => true;

        public override void LoadWindowControls(StackPanel control, string parent, Type parentType, string name, Type type, object[] attributes, SpawnDict<string, SpawnList<InvItem>> member, Type[] subGroupStack)
        {
            Type keyType = ReflectionExt.GetBaseTypeArg(typeof(ISpawnDict<,>), type, 0);
            Type listType = ReflectionExt.GetBaseTypeArg(typeof(ISpawnDict<,>), type, 1);
            Type elementType = ReflectionExt.GetBaseTypeArg(typeof(ISpawnList<>), listType, 0);

            CategorySpawnBox lbxValue = new CategorySpawnBox();

            EditorHeightAttribute heightAtt = ReflectionExt.FindAttribute<EditorHeightAttribute>(attributes);
            if (heightAtt != null)
                lbxValue.MaxHeight = heightAtt.Height;
            else
                lbxValue.MaxHeight = 180;

            CategorySpawnBoxViewModel vm = new CategorySpawnBoxViewModel(control.GetOwningForm(), new StringConv(keyType, ReflectionExt.GetPassableAttributes(1, attributes)), new StringConv(elementType, ReflectionExt.GetPassableAttributes(1, ReflectionExt.GetPassableAttributes(2, attributes))));
            lbxValue.DataContext = vm;
            lbxValue.MinHeight = lbxValue.MaxHeight;//TODO: Uptake Avalonia fix for improperly updating Grid control dimensions

            //add lambda expression for editing a single element
            vm.OnEditItem += (int index, object element, CategorySpawnBoxViewModel.EditElementOp op) =>
            {
                string elementName = name + "[" + index + "]";
                DataEditForm frmData = new DataEditForm();
                frmData.Title = DataEditor.GetWindowTitle(parent, elementName, element, elementType, ReflectionExt.GetPassableAttributes(1, ReflectionExt.GetPassableAttributes(2, attributes)));

                DataEditor.LoadClassControls(frmData.ControlPanel, parent, null, elementName, elementType, ReflectionExt.GetPassableAttributes(1, ReflectionExt.GetPassableAttributes(2, attributes)), element, true, new Type[0]);
                DataEditor.TrackTypeSize(frmData, elementType);

                frmData.SelectedOKEvent += async () =>
                {
                    element = DataEditor.SaveClassControls(frmData.ControlPanel, elementName, elementType, ReflectionExt.GetPassableAttributes(1, ReflectionExt.GetPassableAttributes(2, attributes)), true, new Type[0]);
                    op(index, element);
                    return true;
                };

                control.GetOwningForm().RegisterChild(frmData);
                frmData.Show();
            };

            vm.OnEditKey += (int index, object key, CategorySpawnBoxViewModel.EditElementOp op) =>
            {
                string elementName = name + "[" + index + "]";
                DataEditForm frmData = new DataEditForm();
                frmData.Title = DataEditor.GetWindowTitle(parent, elementName, key, keyType, ReflectionExt.GetPassableAttributes(1, attributes));

                DataEditor.LoadClassControls(frmData.ControlPanel, parent, null, elementName, keyType, ReflectionExt.GetPassableAttributes(1, attributes), key, true, new Type[0]);
                DataEditor.TrackTypeSize(frmData, keyType);

                frmData.SelectedOKEvent += async () =>
                {
                    key = DataEditor.SaveClassControls(frmData.ControlPanel, elementName, keyType, ReflectionExt.GetPassableAttributes(1, attributes), true, new Type[0]);
                    op(index, key);
                    return true;
                };

                control.GetOwningForm().RegisterChild(frmData);
                frmData.Show();
            };

            vm.LoadFromDict(member);
            control.Children.Add(lbxValue);

        }


        public override SpawnDict<string, SpawnList<InvItem>> SaveWindowControls(StackPanel control, string name, Type type, object[] attributes, Type[] subGroupStack)
        {
            int controlIndex = 0;

            CategorySpawnBox lbxValue = (CategorySpawnBox)control.Children[controlIndex];
            CategorySpawnBoxViewModel mv = (CategorySpawnBoxViewModel)lbxValue.DataContext;
            return (SpawnDict<string, SpawnList<InvItem>>)mv.GetDict(type);
        }

        public override string GetString(SpawnDict<string, SpawnList<InvItem>> obj, Type type, object[] attributes)
        {
            return string.Format("{0}[{1}]", obj.GetType().GetFormattedTypeName(), obj.Count);
        }
    }
}
