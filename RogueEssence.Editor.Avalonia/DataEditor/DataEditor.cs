using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RogueEssence.Content;
using RogueEssence.Dungeon;
using RogueElements;
using System.IO;
using Avalonia.Controls;
using System.Collections.ObjectModel;
using System.Reactive.Subjects;
using RogueEssence.Dev.Views;
using Microsoft.Xna.Framework;
using Avalonia.Interactivity;
using Avalonia;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

namespace RogueEssence.Dev
{
    public static class DataEditor
    {
        private const int LABEL_HEIGHT = 14;

        private static List<IEditorConverter> converters;

        private static object clipboardObj;

        public static void Init()
        {
            clipboardObj = new object();
            converters = new List<IEditorConverter>();
            //AddConverter(new AutoTileBaseConverter());
            AddConverter(new BaseEmitterConverter());
            AddConverter(new BattleDataConverter());
            AddConverter(new BattleFXConverter());
            AddConverter(new CircleSquareEmitterConverter());
            AddConverter(new CombatActionConverter());
            AddConverter(new ExplosionDataConverter());
            //AddConverter(new ItemDataConverter());
            AddConverter(new ShootingEmitterConverter());
            AddConverter(new SkillDataConverter());
            //AddConverter(new SpawnListConverter());
            //AddConverter(new SpawnRangeListConverter());
            AddConverter(new TypeDictConverter());
            AddConverter(new ColumnAnimConverter());
            AddConverter(new StaticAnimConverter());
        }

        public static void AddConverter(IEditorConverter converter)
        {
            //maintain inheritance order
            for (int ii = 0; ii < converters.Count; ii++)
            {
                if (converter.GetConvertingType().IsSubclassOf(converters[ii].GetConvertingType()))
                {
                    converters.Insert(ii, converter);
                    return;
                }
            }
            converters.Add(converter);
        }


        public static void LoadDataControls(object obj, StackPanel control)
        {
            loadMemberControl(obj, control, obj.ToString(), obj.GetType(), null, obj, true);
        }

        private static void loadClassControls(object obj, StackPanel control)
        {
            Type objType = obj.GetType();
            Type[] interfaces = objType.GetInterfaces();
            foreach (IEditorConverter converter in converters)
            {
                Type convertType = converter.GetConvertingType();
                if (convertType == objType || objType.IsSubclassOf(convertType) || interfaces.Contains(convertType))
                {
                    converter.LoadClassControls(obj, control);
                    return;
                }
            }

            StaticLoadClassControls(obj, control);
        }

        public static void StaticLoadClassControls(object obj, StackPanel control)
        {
            //go through all members and add for them
            //control starts off clean; this is the control that will have all member controls on it
            try
            {
                Type type = obj.GetType();

                List<MemberInfo> myFields = type.GetEditableMembers();

                List<List<MemberInfo>> tieredFields = new List<List<MemberInfo>>();
                for (int ii = 0; ii < myFields.Count; ii++)
                {
                    if (myFields[ii].GetCustomAttributes(typeof(NonEditedAttribute), false).Length > 0)
                        continue;
                    if (myFields[ii].GetCustomAttributes(typeof(NonSerializedAttribute), false).Length > 0)
                        continue;

                    object member = myFields[ii].GetValue(obj);
                    if (member == null && myFields[ii].GetCustomAttributes(typeof(NonNullAttribute), false).Length > 0)
                        throw new Exception("Null class member found in " + type.ToString() + ": " + myFields[ii].Name);

                    if (myFields[ii].GetCustomAttributes(typeof(SharedRowAttribute), false).Length == 0)
                        tieredFields.Add(new List<MemberInfo>());
                    tieredFields[tieredFields.Count - 1].Add(myFields[ii]);
                }

                for (int ii = 0; ii < tieredFields.Count; ii++)
                {
                    if (tieredFields[ii].Count == 1)
                    {
                        MemberInfo myInfo = tieredFields[ii][0];
                        StackPanel stack = new StackPanel();
                        control.Children.Add(stack);
                        loadMemberControl(obj, stack, myInfo.Name, myInfo.GetMemberInfoType(), myInfo.GetCustomAttributes(false), myInfo.GetValue(obj), false);
                    }
                    else
                    {
                        Avalonia.Controls.Grid sharedRowPanel = getSharedRowPanel(tieredFields[ii].Count);
                        control.Children.Add(sharedRowPanel);
                        for (int jj = 0; jj < tieredFields[ii].Count; jj++)
                        {
                            MemberInfo myInfo = tieredFields[ii][jj];
                            StackPanel stack = new StackPanel();
                            sharedRowPanel.Children.Add(stack);
                            stack.SetValue(Avalonia.Controls.Grid.ColumnProperty, jj);
                            loadMemberControl(obj, stack, myInfo.Name, myInfo.GetMemberInfoType(), myInfo.GetCustomAttributes(false), myInfo.GetValue(obj), false);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(e);
            }
        }

        public static Avalonia.Controls.Grid getSharedRowPanel(int cols)
        {
            Avalonia.Controls.Grid sharedRowPanel = new Avalonia.Controls.Grid();
            for(int ii = 0; ii < cols; ii++)
                sharedRowPanel.ColumnDefinitions.Add(new ColumnDefinition());

            return sharedRowPanel;
        }


        private delegate void CreateMethod();

        public static void LoadLabelControl(StackPanel control, string name)
        {
            TextBlock lblName = new TextBlock();
            lblName.Margin = new Thickness(0, 4, 0, 0);
            //StringBuilder separatedName = new StringBuilder();
            //for (int ii = 0; ii < name.Length; ii++)
            //{
            //    if (ii > 0 && (char.IsUpper(name[ii]) && !char.IsLower(name[ii-1]) || char.IsDigit(name[ii])))
            //        separatedName.Append(' ');
            //    separatedName.Append(name[ii]);
            //}
            //separatedName.Append(":");
            lblName.Text = name + ":";
            control.Children.Add(lblName);
        }


        private static void loadMemberControl(object obj, StackPanel control, string name, Type type, object[] attributes, object member, bool isWindow)
        {
            Type objType = obj.GetType();
            Type[] interfaces = objType.GetInterfaces();
            foreach (IEditorConverter converter in converters)
            {
                Type convertType = converter.GetConvertingType();
                if (convertType == objType || objType.IsSubclassOf(convertType) || interfaces.Contains(convertType))
                {
                    converter.LoadMemberControl(obj, control, name, type, attributes, member, isWindow);
                    return;
                }
            }
            StaticLoadMemberControl(control, name, type, attributes, member, isWindow);
        }
        //TODO: move loadClassControls to be another layer of searching just after loadMemberControl
        //loadClassControls will have to honor isWindow and SubGroupAttribute by themselves.
        //the code for that can be written into EditorConverter to make it easy, possibly
        //meanwhile other classes like color will not honor isWindow and SubGroupAttribute because they are meant to be treated like the primitives
        //finally, the last-resort cases in StaticLoadMemberControl will remain unchanged?  they will only call the generic class UI creators straight on
        //instead of loadClassControls

        //overload this method in children to account for structs such as loc
        public static void StaticLoadMemberControl(StackPanel control, string name, Type type, object[] attributes, object member, bool isWindow)
        {
            try
            {
                if (type.IsEnum)
                {
                    LoadLabelControl(control, name);

                    Array enums = type.GetEnumValues();
                    if (type.GetCustomAttributes(typeof(FlagsAttribute), false).Length > 0)
                    {
                        List<CheckBox> checkboxes = new List<CheckBox>();
                        for (int ii = 0; ii < enums.Length; ii++)
                        {
                            int numeric = (int)enums.GetValue(ii);
                            int num1s = 0;
                            for (int jj = 0; jj < 32; jj++)
                            {
                                if ((numeric & 0x1) == 1)
                                    num1s++;
                                numeric = numeric >> 1;
                            }
                            if (num1s == 1)
                            {
                                CheckBox chkValue = new CheckBox();
                                if (checkboxes.Count > 0)
                                    chkValue.Margin = new Thickness(4, 0, 0, 0);
                                chkValue.Content = enums.GetValue(ii).ToString();
                                chkValue.IsChecked = ((int)member & (int)enums.GetValue(ii)) > 0;
                                checkboxes.Add(chkValue);
                            }
                        }

                        Avalonia.Controls.Grid innerPanel = getSharedRowPanel(checkboxes.Count);
                        for (int ii = 0; ii < checkboxes.Count; ii++)
                        {
                            innerPanel.Children.Add(checkboxes[ii]);
                            checkboxes[ii].SetValue(Avalonia.Controls.Grid.ColumnProperty, ii);
                        }

                        control.Children.Add(innerPanel);
                    }
                    else
                    {
                        //for enums, use a combobox
                        ComboBox cbValue = new ComboBox();
                        cbValue.VirtualizationMode = ItemVirtualizationMode.Simple;

                        List<string> items = new List<string>();
                        int selection = 0;
                        for (int ii = 0; ii < enums.Length; ii++)
                        {
                            items.Add(enums.GetValue(ii).ToString());
                            if (Enum.Equals(enums.GetValue(ii), member))
                                selection = ii;
                        }

                        var subject = new Subject<List<string>>();
                        cbValue.Bind(ComboBox.ItemsProperty, subject);
                        subject.OnNext(items);
                        cbValue.SelectedIndex = selection;
                        control.Children.Add(cbValue);
                    }
                }
                else if (type == typeof(Boolean))
                {
                    CheckBox chkValue = new CheckBox();
                    chkValue.Margin = new Thickness(0, 4, 0, 0);
                    chkValue.Content = name;
                    chkValue.IsChecked = (member == null) ? false : (bool)member;
                    control.Children.Add(chkValue);
                }
                else if (type == typeof(Int32))
                {
                    LoadLabelControl(control, name);

                    DataTypeAttribute dataAtt = ReflectionExt.FindAttribute<DataTypeAttribute>(attributes);
                    FrameTypeAttribute frameAtt = ReflectionExt.FindAttribute<FrameTypeAttribute>(attributes);
                    if (dataAtt != null)
                    {
                        ComboBox cbValue = new ComboBox();
                        cbValue.VirtualizationMode = ItemVirtualizationMode.Simple;
                        int chosenIndex = (member == null) ? 0 : (Int32)member;
                        Data.EntryDataIndex nameIndex = Data.DataManager.Instance.DataIndices[dataAtt.DataType];

                        List<string> items = new List<string>();
                        if (dataAtt.IncludeInvalid)
                        {
                            items.Add("---");
                            chosenIndex++;
                        }

                        for (int ii = 0; ii < nameIndex.Count; ii++)
                            items.Add(ii.ToString() + ": " + nameIndex.Entries[ii].GetLocalString(false));

                        var subject = new Subject<List<string>>();
                        cbValue.Bind(ComboBox.ItemsProperty, subject);
                        subject.OnNext(items);
                        cbValue.SelectedIndex = Math.Min(Math.Max(0, chosenIndex), items.Count - 1);
                        control.Children.Add(cbValue);
                    }
                    else if (frameAtt != null)
                    {
                        ComboBox cbValue = new ComboBox();
                        cbValue.VirtualizationMode = ItemVirtualizationMode.Simple;
                        int chosenIndex = 0;

                        List<string> items = new List<string>();
                        for (int ii = 0; ii < GraphicsManager.Actions.Count; ii++)
                        {
                            if (!frameAtt.DashOnly || GraphicsManager.Actions[ii].IsDash)
                            {
                                if (ii == (int)member)
                                    chosenIndex = items.Count;
                                items.Add(GraphicsManager.Actions[ii].Name);
                            }
                        }

                        var subject = new Subject<List<string>>();
                        cbValue.Bind(ComboBox.ItemsProperty, subject);
                        subject.OnNext(items);
                        cbValue.SelectedIndex = Math.Min(Math.Max(0, chosenIndex), items.Count - 1);
                        control.Children.Add(cbValue);
                    }
                    else
                    {
                        NumericUpDown nudValue = new NumericUpDown();
                        nudValue.Minimum = Int32.MinValue;
                        nudValue.Maximum = Int32.MaxValue;
                        NumberRangeAttribute rangeAtt = ReflectionExt.FindAttribute<NumberRangeAttribute>(attributes);
                        if (rangeAtt != null)
                        {
                            nudValue.Minimum = rangeAtt.Min;
                            nudValue.Maximum = rangeAtt.Max;
                        }
                        nudValue.Value = (member == null) ? (nudValue.Minimum > 0 ? nudValue.Minimum : 0) : (Int32)member;

                        control.Children.Add(nudValue);
                    }
                }
                else if (type == typeof(byte))
                {
                    LoadLabelControl(control, name);

                    NumericUpDown nudValue = new NumericUpDown();
                    nudValue.Minimum = byte.MinValue;
                    nudValue.Maximum = byte.MaxValue;
                    NumberRangeAttribute rangeAtt = ReflectionExt.FindAttribute<NumberRangeAttribute>(attributes);
                    if (rangeAtt != null)
                    {
                        nudValue.Minimum = rangeAtt.Min;
                        nudValue.Maximum = rangeAtt.Max;
                    }
                    nudValue.Value = (member == null) ? (nudValue.Minimum > 0 ? nudValue.Minimum : 0) : (byte)member;

                    control.Children.Add(nudValue);
                }
                else if (type == typeof(Single))
                {
                    LoadLabelControl(control, name);

                    NumericUpDown nudValue = new NumericUpDown();
                    nudValue.Minimum = Int32.MinValue;
                    nudValue.Maximum = Int32.MaxValue;
                    NumberRangeAttribute attribute = ReflectionExt.FindAttribute<NumberRangeAttribute>(attributes);
                    if (attribute != null)
                    {
                        nudValue.Minimum = attribute.Min;
                        nudValue.Maximum = attribute.Max;
                    }
                    float value = (member == null) ? (float)(nudValue.Minimum > 0 ? nudValue.Minimum : 0) : (float)member;
                    nudValue.Value = (double)value;
                    control.Children.Add(nudValue);
                }
                else if (type == typeof(Double))
                {
                    LoadLabelControl(control, name);

                    NumericUpDown nudValue = new NumericUpDown();
                    nudValue.Minimum = Int32.MinValue;
                    nudValue.Maximum = Int32.MaxValue;
                    NumberRangeAttribute attribute = ReflectionExt.FindAttribute<NumberRangeAttribute>(attributes);
                    if (attribute != null)
                    {
                        nudValue.Minimum = attribute.Min;
                        nudValue.Maximum = attribute.Max;
                    }
                    double value = (member == null) ? (double)(nudValue.Minimum > 0 ? nudValue.Minimum : 0) : (double)member;
                    nudValue.Value = (double)value;
                    control.Children.Add(nudValue);
                }
                else if (type == typeof(Color))
                {
                    LoadLabelControl(control, name);

                    Avalonia.Controls.Grid innerPanel = getSharedRowPanel(8);

                    TextBlock lblR = new TextBlock();
                    lblR.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
                    lblR.Text = "R:";
                    innerPanel.Children.Add(lblR);
                    lblR.SetValue(Avalonia.Controls.Grid.ColumnProperty, 0);

                    NumericUpDown nudValueR = new NumericUpDown();
                    nudValueR.Margin = new Thickness(4, 0, 0, 0);
                    nudValueR.Minimum = byte.MinValue;
                    nudValueR.Maximum = byte.MaxValue;
                    nudValueR.Value = (member == null) ? 0 : ((Microsoft.Xna.Framework.Color)member).R;
                    innerPanel.Children.Add(nudValueR);
                    nudValueR.SetValue(Avalonia.Controls.Grid.ColumnProperty, 1);

                    TextBlock lblG = new TextBlock();
                    lblG.Margin = new Thickness(8, 0, 0, 0);
                    lblG.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
                    lblG.Text = "G:";
                    innerPanel.Children.Add(lblG);
                    lblG.SetValue(Avalonia.Controls.Grid.ColumnProperty, 2);

                    NumericUpDown nudValueG = new NumericUpDown();
                    nudValueG.Margin = new Thickness(4, 0, 0, 0);
                    nudValueG.Minimum = byte.MinValue;
                    nudValueG.Maximum = byte.MaxValue;
                    nudValueG.Value = (member == null) ? 0 : ((Microsoft.Xna.Framework.Color)member).G;
                    innerPanel.Children.Add(nudValueG);
                    nudValueG.SetValue(Avalonia.Controls.Grid.ColumnProperty, 3);

                    TextBlock lblB = new TextBlock();
                    lblB.Margin = new Thickness(8, 0, 0, 0);
                    lblB.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
                    lblB.Text = "B:";
                    innerPanel.Children.Add(lblB);
                    lblB.SetValue(Avalonia.Controls.Grid.ColumnProperty, 4);

                    NumericUpDown nudValueB = new NumericUpDown();
                    nudValueB.Margin = new Thickness(4, 0, 0, 0);
                    nudValueB.Minimum = byte.MinValue;
                    nudValueB.Maximum = byte.MaxValue;
                    nudValueB.Value = (member == null) ? 0 : ((Microsoft.Xna.Framework.Color)member).B;
                    innerPanel.Children.Add(nudValueB);
                    nudValueB.SetValue(Avalonia.Controls.Grid.ColumnProperty, 5);

                    TextBlock lblA = new TextBlock();
                    lblA.Margin = new Thickness(8, 0, 0, 0);
                    lblA.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
                    lblA.Text = "A:";
                    innerPanel.Children.Add(lblA);
                    lblA.SetValue(Avalonia.Controls.Grid.ColumnProperty, 6);

                    NumericUpDown nudValueA = new NumericUpDown();
                    nudValueA.Margin = new Thickness(4, 0, 0, 0);
                    nudValueA.Minimum = byte.MinValue;
                    nudValueA.Maximum = byte.MaxValue;
                    nudValueA.Value = (member == null) ? 0 : ((Microsoft.Xna.Framework.Color)member).A;
                    innerPanel.Children.Add(nudValueA);
                    nudValueA.SetValue(Avalonia.Controls.Grid.ColumnProperty, 7);

                    control.Children.Add(innerPanel);
                }
                else if (type == typeof(FlagType))
                {
                    StringTypeConstraintAttribute dataAtt = ReflectionExt.FindAttribute<StringTypeConstraintAttribute>(attributes);

                    if (dataAtt != null)
                    {
                        Type baseType = dataAtt.BaseClass;

                        Type[] children = baseType.GetAssignableTypes();

                        Avalonia.Controls.Grid sharedRowPanel = getSharedRowPanel(2);

                        TextBlock lblType = new TextBlock();
                        lblType.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
                        lblType.Text = "Type:";
                        sharedRowPanel.Children.Add(lblType);
                        sharedRowPanel.ColumnDefinitions[0].Width = new GridLength(30);
                        lblType.SetValue(Avalonia.Controls.Grid.ColumnProperty, 0);

                        ComboBox cbValue = new ComboBox();
                        cbValue.Margin = new Thickness(4, 0, 0, 0);
                        sharedRowPanel.Children.Add(cbValue);
                        cbValue.SetValue(Avalonia.Controls.Grid.ColumnProperty, 1);

                        List<string> items = new List<string>();
                        int selection = 0;
                        for (int ii = 0; ii < children.Length; ii++)
                        {
                            Type childType = children[ii];
                            items.Add(childType.GetDisplayName());

                            if (childType == ((FlagType)member).FullType)
                                selection = ii;
                        }

                        var subject = new Subject<List<string>>();
                        cbValue.Bind(ComboBox.ItemsProperty, subject);
                        subject.OnNext(items);
                        cbValue.SelectedIndex = selection;

                        control.Children.Add(sharedRowPanel);
                    }
                }
                else if (type == typeof(String))
                {
                    LoadLabelControl(control, name);

                    AnimAttribute animAtt = ReflectionExt.FindAttribute<AnimAttribute>(attributes);
                    if (animAtt != null)
                    {
                        ComboBox cbValue = new ComboBox();
                        cbValue.VirtualizationMode = ItemVirtualizationMode.Simple;
                        string choice = (string)member;

                        List<string> items = new List<string>();
                        items.Add("---");
                        int chosenIndex = 0;

                        string[] dirs = Directory.GetFiles(animAtt.FolderPath);

                        for (int ii = 0; ii < dirs.Length; ii++)
                        {
                            string filename = Path.GetFileNameWithoutExtension(dirs[ii]);
                            if (filename == choice)
                                chosenIndex = items.Count;
                            items.Add(filename);
                        }

                        var subject = new Subject<List<string>>();
                        cbValue.Bind(ComboBox.ItemsProperty, subject);
                        subject.OnNext(items);
                        cbValue.SelectedIndex = chosenIndex;
                        control.Children.Add(cbValue);
                    }
                    else if (ReflectionExt.FindAttribute<SoundAttribute>(attributes) != null)
                    {
                        //is it a sound effect?

                        ComboBox cbValue = new ComboBox();
                        cbValue.VirtualizationMode = ItemVirtualizationMode.Simple;
                        string choice = (string)member;

                        List<string> items = new List<string>();
                        items.Add("---");
                        int chosenIndex = 0;

                        string[] dirs = Directory.GetFiles(DiagManager.CONTENT_PATH + "Sound/Battle");

                        for (int ii = 0; ii < dirs.Length; ii++)
                        {
                            string filename = Path.GetFileNameWithoutExtension(dirs[ii]);
                            if (filename == choice)
                                chosenIndex = items.Count;
                            items.Add(filename);
                        }

                        var subject = new Subject<List<string>>();
                        cbValue.Bind(ComboBox.ItemsProperty, subject);
                        subject.OnNext(items);
                        cbValue.SelectionChanged += CbValue_PlaySound;
                        cbValue.SelectedIndex = chosenIndex;
                        control.Children.Add(cbValue);

                    }
                    else
                    {
                        //for strings, use an edit textbox
                        TextBox txtValue = new TextBox();
                        //txtValue.Dock = DockStyle.Fill;
                        MultilineAttribute attribute = ReflectionExt.FindAttribute<MultilineAttribute>(attributes);
                        if (attribute != null)
                        {
                            //txtValue.Multiline = true;
                            //txtValue.Size = new Size(0, 80);
                        }
                        //else
                        //    txtValue.Size = new Size(0, 20);
                        txtValue.Text = (member == null) ? "" : (String)member;
                        control.Children.Add(txtValue);
                    }
                }
                else if (type == typeof(Priority))
                {
                    LoadLabelControl(control, name);

                    //for strings, use an edit textbox
                    TextBox txtValue = new TextBox();
                    txtValue.Text = (member == null) ? "" : member.ToString();
                    control.Children.Add(txtValue);
                }
                else if (type == typeof(Loc))
                {
                    LoadLabelControl(control, name);

                    Avalonia.Controls.Grid innerPanel = getSharedRowPanel(4);

                    TextBlock lblX = new TextBlock();
                    lblX.Text = "X:";
                    innerPanel.Children.Add(lblX);
                    lblX.SetValue(Avalonia.Controls.Grid.ColumnProperty, 0);

                    NumericUpDown nudValueX = new NumericUpDown();
                    nudValueX.Margin = new Thickness(4, 0, 0, 0);
                    nudValueX.Minimum = Int32.MinValue;
                    nudValueX.Maximum = Int32.MaxValue;
                    nudValueX.Value = (member == null) ? 0 : ((Loc)member).X;
                    innerPanel.Children.Add(nudValueX);
                    nudValueX.SetValue(Avalonia.Controls.Grid.ColumnProperty, 1);

                    TextBlock lblY = new TextBlock();
                    lblY.Margin = new Thickness(8, 0, 0, 0);
                    lblY.Text = "Y:";
                    innerPanel.Children.Add(lblY);
                    lblY.SetValue(Avalonia.Controls.Grid.ColumnProperty, 2);


                    NumericUpDown nudValueY = new NumericUpDown();
                    nudValueY.Margin = new Thickness(4, 0, 0, 0);
                    nudValueY.Minimum = Int32.MinValue;
                    nudValueY.Maximum = Int32.MaxValue;
                    nudValueY.Value = (member == null) ? 0 : ((Loc)member).Y;
                    innerPanel.Children.Add(nudValueY);
                    nudValueY.SetValue(Avalonia.Controls.Grid.ColumnProperty, 3);

                    control.Children.Add(innerPanel);
                }
                else if (type == typeof(SegLoc))
                {
                    LoadLabelControl(control, name);

                    Avalonia.Controls.Grid innerPanel = getSharedRowPanel(4);

                    TextBlock lblX = new TextBlock();
                    lblX.Text = "Structure:";
                    innerPanel.Children.Add(lblX);
                    lblX.SetValue(Avalonia.Controls.Grid.ColumnProperty, 0);

                    NumericUpDown nudValueX = new NumericUpDown();
                    nudValueX.Margin = new Thickness(4, 0, 0, 0);
                    nudValueX.Minimum = Int32.MinValue;
                    nudValueX.Maximum = Int32.MaxValue;
                    nudValueX.Value = (member == null) ? 0 : ((SegLoc)member).Segment;
                    innerPanel.Children.Add(nudValueX);
                    nudValueX.SetValue(Avalonia.Controls.Grid.ColumnProperty, 1);

                    TextBlock lblY = new TextBlock();
                    lblY.Margin = new Thickness(8, 0, 0, 0);
                    lblY.Text = "Map:";
                    innerPanel.Children.Add(lblY);
                    lblY.SetValue(Avalonia.Controls.Grid.ColumnProperty, 2);

                    NumericUpDown nudValueY = new NumericUpDown();
                    nudValueY.Margin = new Thickness(4, 0, 0, 0);
                    nudValueY.Minimum = Int32.MinValue;
                    nudValueY.Maximum = Int32.MaxValue;
                    nudValueY.Value = (member == null) ? 0 : ((SegLoc)member).ID;
                    innerPanel.Children.Add(nudValueY);
                    nudValueY.SetValue(Avalonia.Controls.Grid.ColumnProperty, 3);

                    control.Children.Add(innerPanel);

                }
                else if (type == typeof(IntRange))
                {
                    LoadLabelControl(control, name);

                    Avalonia.Controls.Grid innerPanel = getSharedRowPanel(4);

                    TextBlock lblX = new TextBlock();
                    lblX.Text = "Min:";
                    innerPanel.Children.Add(lblX);
                    lblX.SetValue(Avalonia.Controls.Grid.ColumnProperty, 0);

                    NumericUpDown nudValueX = new NumericUpDown();
                    nudValueX.Margin = new Thickness(4, 0, 0, 0);
                    nudValueX.Minimum = Int32.MinValue;
                    nudValueX.Maximum = Int32.MaxValue;
                    nudValueX.Value = (member == null) ? 0 : ((IntRange)member).Min;
                    innerPanel.Children.Add(nudValueX);
                    nudValueX.SetValue(Avalonia.Controls.Grid.ColumnProperty, 1);

                    TextBlock lblY = new TextBlock();
                    lblY.Margin = new Thickness(8, 0, 0, 0);
                    lblY.Text = "Map:";
                    innerPanel.Children.Add(lblY);
                    lblY.SetValue(Avalonia.Controls.Grid.ColumnProperty, 2);

                    NumericUpDown nudValueY = new NumericUpDown();
                    nudValueY.Margin = new Thickness(4, 0, 0, 0);
                    nudValueY.Minimum = Int32.MinValue;
                    nudValueY.Maximum = Int32.MaxValue;
                    nudValueY.Value = (member == null) ? 0 : ((IntRange)member).Max;
                    innerPanel.Children.Add(nudValueY);
                    nudValueY.SetValue(Avalonia.Controls.Grid.ColumnProperty, 3);

                    control.Children.Add(innerPanel);
                }
                //else if (type == typeof(TileLayer))
                //{
                //    LoadLabelControl(control, name);

                //    TilePreview preview = new TilePreview();
                //    preview.Dock = DockStyle.Fill;
                //    preview.Size = new Size(GraphicsManager.TileSize, GraphicsManager.TileSize);
                //    preview.SetChosenAnim((TileLayer)member);
                //    control.Controls.Add(preview);
                //    preview.TileClick += (object sender, EventArgs e) =>
                //    {
                //        ElementForm frmData = new ElementForm();
                //        frmData.Text = name + "/" + "Tile";

                //        Rectangle boxRect = new Rectangle(new Point(), new Size(654, 502 + LABEL_HEIGHT));
                //        int box_down = 0;
                //        LoadLabelControl(frmData.ControlPanel, name);
                //        box_down += 16;
                //        //for enums, use a combobox
                //        TileBrowser browser = new TileBrowser();
                //        browser.Location = new Point(boxRect.Left, box_down);
                //        browser.Size = new Size(boxRect.Width, boxRect.Height);
                //        browser.SetBrush(preview.GetChosenAnim());
                //        frmData.ControlPanel.Controls.Add(browser);

                //        if (frmData.ShowDialog() == DialogResult.OK)
                //            preview.SetChosenAnim(browser.GetBrush());
                //    };
                //}
                //else if (type.GetInterfaces().Contains(typeof(IList<TileLayer>)))
                //{
                //    LoadLabelControl(control, name);

                //    TilePreview preview = new TilePreview();
                //    preview.Dock = DockStyle.Fill;
                //    preview.Size = new Size(GraphicsManager.TileSize, GraphicsManager.TileSize);
                //    preview.SetChosenAnim(((IList<TileLayer>)member).Count > 0 ? ((IList<TileLayer>)member)[0] : new TileLayer());
                //    control.Controls.Add(preview);

                //    CollectionBox lbxValue = new CollectionBox();
                //    lbxValue.Dock = DockStyle.Fill;
                //    lbxValue.Size = new Size(0, 175);
                //    lbxValue.LoadFromList(type, (IList)member);
                //    control.Controls.Add(lbxValue);

                //    lbxValue.SelectedIndexChanged += (object sender, EventArgs e) =>
                //    {
                //        if (lbxValue.SelectedIndex > -1)
                //            preview.SetChosenAnim(((IList<TileLayer>)lbxValue.Collection)[lbxValue.SelectedIndex]);
                //        else
                //            preview.SetChosenAnim(((IList<TileLayer>)lbxValue.Collection).Count > 0 ? ((IList<TileLayer>)lbxValue.Collection)[0] : new TileLayer());
                //    };


                //    //add lambda expression for editing a single element
                //    lbxValue.OnEditItem = (int index, object element, CollectionBox.EditElementOp op) =>
                //    {
                //        ElementForm frmData = new ElementForm();
                //        frmData.Text = name + "/" + "Tile #" + index;

                //        Rectangle boxRect = new Rectangle(new Point(), new Size(654, 502 + LABEL_HEIGHT));
                //        int box_down = 0;
                //        LoadLabelControl(frmData.ControlPanel, name);
                //        box_down += 16;
                //        //for enums, use a combobox
                //        TileBrowser browser = new TileBrowser();
                //        browser.Location = new Point(boxRect.Left, box_down);
                //        browser.Size = new Size(boxRect.Width, boxRect.Height);
                //        browser.SetBrush(element != null ? (TileLayer)element : new TileLayer());

                //        frmData.OnOK += (object okSender, EventArgs okE) =>
                //        {
                //            element = browser.GetBrush();
                //            frmData.Close();
                //        };
                //        frmData.OnCancel += (object okSender, EventArgs okE) =>
                //        {
                //            frmData.Close();
                //        };

                //        frmData.ControlPanel.Controls.Add(browser);

                //        frmData.Show();
                //    };

                //}
                else if (type.Equals(typeof(Type)))
                {
                    TypeConstraintAttribute dataAtt = ReflectionExt.FindAttribute<TypeConstraintAttribute>(attributes);
                    Type baseType = dataAtt.BaseClass;

                    Type[] children = baseType.GetAssignableTypes();

                    Avalonia.Controls.Grid sharedRowPanel = getSharedRowPanel(2);

                    TextBlock lblType = new TextBlock();
                    lblType.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
                    lblType.Text = "Type:";
                    sharedRowPanel.Children.Add(lblType);
                    sharedRowPanel.ColumnDefinitions[0].Width = new GridLength(30);
                    lblType.SetValue(Avalonia.Controls.Grid.ColumnProperty, 0);

                    ComboBox cbValue = new ComboBox();
                    cbValue.Margin = new Thickness(4, 0, 0, 0);
                    sharedRowPanel.Children.Add(cbValue);
                    cbValue.SetValue(Avalonia.Controls.Grid.ColumnProperty, 1);

                    List<string> items = new List<string>();
                    int selection = 0;
                    for (int ii = 0; ii < children.Length; ii++)
                    {
                        Type childType = children[ii];
                        items.Add(childType.GetDisplayName());

                        if (childType == (Type)member)
                            selection = ii;
                    }

                    var subject = new Subject<List<string>>();
                    cbValue.Bind(ComboBox.ItemsProperty, subject);
                    subject.OnNext(items);
                    cbValue.SelectedIndex = selection;

                    control.Children.Add(sharedRowPanel);

                }
                else if (type.IsArray)
                {
                    //TODO: 2D array grid support
                    //if (type.GetElementType().IsArray)

                    LoadLabelControl(control, name);

                    CollectionBox lbxValue = new CollectionBox();

                    Type elementType = type.GetElementType();
                    //lbxValue.StringConv = GetStringRep(elementType, ReflectionExt.GetPassableAttributes(1, attributes));
                    //add lambda expression for editing a single element
                    lbxValue.OnEditItem = (int index, object element, CollectionBox.EditElementOp op) =>
                    {
                        DataEditForm frmData = new DataEditForm();
                        if (element == null)
                            frmData.Title = name + "/" + "New " + elementType.Name;
                        else
                            frmData.Title = name + "/" + element.ToString();

                        StaticLoadMemberControl(frmData.ControlPanel, "(Array) " + name + "[" + index + "]", elementType, ReflectionExt.GetPassableAttributes(0, attributes), element, true);

                        frmData.SelectedOKEvent += () =>
                        {
                            StaticSaveMemberControl(frmData.ControlPanel, name, elementType, ReflectionExt.GetPassableAttributes(0, attributes), ref element, true);
                            op(index, element);
                            frmData.Close();
                        };
                        frmData.SelectedCancelEvent += () =>
                        {
                            frmData.Close();
                        };
                        control.GetOwningForm().RegisterChild(frmData);
                        frmData.Show();
                    };


                    Array array = ((Array)member);
                    List<object> objList = new List<object>();
                    for (int ii = 0; ii < array.Length; ii++)
                        objList.Add(array.GetValue(ii));

                    lbxValue.LoadFromList(objList);
                    control.Children.Add(lbxValue);
                }
                else if (type.GetInterfaces().Contains(typeof(IList)))
                {
                    LoadLabelControl(control, name);

                    CollectionBox lbxValue = new CollectionBox();

                    Type elementType = ReflectionExt.GetBaseTypeArg(typeof(IList<>), type, 0);
                    //lbxValue.StringConv = GetStringRep(elementType, ReflectionExt.GetPassableAttributes(1, attributes));
                    //add lambda expression for editing a single element
                    lbxValue.OnEditItem = (int index, object element, CollectionBox.EditElementOp op) =>
                    {
                        DataEditForm frmData = new DataEditForm();
                        if (element == null)
                            frmData.Title = name + "/" + "New " + elementType.Name;
                        else
                            frmData.Title = name + "/" + element.ToString();

                        StaticLoadMemberControl(frmData.ControlPanel, "(List) " + name + "[" + index + "]", elementType, ReflectionExt.GetPassableAttributes(1, attributes), element, true);

                        frmData.SelectedOKEvent += () =>
                        {
                            StaticSaveMemberControl(frmData.ControlPanel, name, elementType, ReflectionExt.GetPassableAttributes(1, attributes), ref element, true);
                            op(index, element);
                            frmData.Close();
                        };
                        frmData.SelectedCancelEvent += () =>
                        {
                            frmData.Close();
                        };

                        control.GetOwningForm().RegisterChild(frmData);
                        frmData.Show();
                    };

                    lbxValue.LoadFromList((IList)member);
                    control.Children.Add(lbxValue);
                }
                else if (type.GetInterfaces().Contains(typeof(IDictionary)))
                {
                    LoadLabelControl(control, name);

                    DictionaryBox lbxValue = new DictionaryBox();

                    Type keyType = ReflectionExt.GetBaseTypeArg(typeof(IDictionary<,>), type, 0);
                    Type elementType = ReflectionExt.GetBaseTypeArg(typeof(IDictionary<,>), type, 1);

                    //lbxValue.StringConv = GetStringRep(elementType, ReflectionExt.GetPassableAttributes(2, attributes));
                    //add lambda expression for editing a single element
                    lbxValue.OnEditItem = (object key, object element, DictionaryBox.EditElementOp op) =>
                    {
                        DataEditForm frmData = new DataEditForm();
                        if (element == null)
                            frmData.Title = name + "/" + "New " + elementType.Name;
                        else
                            frmData.Title = name + "/" + element.ToString();

                        StaticLoadMemberControl(frmData.ControlPanel, "(Dict) " + name + "[" + key.ToString() + "]", elementType, ReflectionExt.GetPassableAttributes(2, attributes), element, true);

                        frmData.SelectedOKEvent += () =>
                        {
                            StaticSaveMemberControl(frmData.ControlPanel, name, elementType, ReflectionExt.GetPassableAttributes(2, attributes), ref element, true);
                            op(key, element);
                            frmData.Close();
                        };
                        frmData.SelectedCancelEvent += () =>
                        {
                            frmData.Close();
                        };

                        control.GetOwningForm().RegisterChild(frmData);
                        frmData.Show();
                    };

                    lbxValue.OnEditKey = (object key, object element, DictionaryBox.EditElementOp op) =>
                    {
                        DataEditForm frmKey = new DataEditForm();
                        if (element == null)
                            frmKey.Title = name + "/" + "New Key:" + keyType.Name;
                        else
                            frmKey.Title = name + "/" + element.ToString();

                        StaticLoadMemberControl(frmKey.ControlPanel, "(Dict) " + name + "<New Key>", keyType, new object[0] { }, null, true);

                        frmKey.SelectedOKEvent += () =>
                        {
                            StaticSaveMemberControl(frmKey.ControlPanel, name, keyType, new object[0] { }, ref key, true);
                            op(key, element);
                            frmKey.Close();
                        };
                        frmKey.SelectedCancelEvent += () =>
                        {
                            frmKey.Close();
                        };

                        control.GetOwningForm().RegisterChild(frmKey);
                        frmKey.Show();
                    };

                    lbxValue.LoadFromDict((IDictionary)member);
                    control.Children.Add(lbxValue);
                }
                else if (type.GetInterfaces().Contains(typeof(IPriorityList)))
                {
                    LoadLabelControl(control, name);

                    PriorityListBox lbxValue = new PriorityListBox();

                    Type elementType = ReflectionExt.GetBaseTypeArg(typeof(IPriorityList<>), type, 0);
                    //lbxValue.StringConv = GetStringRep(elementType, ReflectionExt.GetPassableAttributes(2, attributes));
                    //add lambda expression for editing a single element
                    lbxValue.OnEditItem = (Priority priority, int index, object element, PriorityListBox.EditElementOp op) =>
                    {
                        DataEditForm frmData = new DataEditForm();
                        if (element == null)
                            frmData.Title = name + "/" + "New " + elementType.Name;
                        else
                            frmData.Title = name + "/" + element.ToString();

                        StaticLoadMemberControl(frmData.ControlPanel, "(PriorityList) " + name + "[" + index + "]", elementType, ReflectionExt.GetPassableAttributes(2, attributes), element, true);

                        frmData.SelectedOKEvent += () =>
                        {
                            StaticSaveMemberControl(frmData.ControlPanel, name, elementType, ReflectionExt.GetPassableAttributes(2, attributes), ref element, true);
                            op(priority, index, element);
                            frmData.Close();
                        };
                        frmData.SelectedCancelEvent += () =>
                        {
                            frmData.Close();
                        };

                        control.GetOwningForm().RegisterChild(frmData);
                        frmData.Show();
                    };
                    lbxValue.OnEditPriority = (Priority priority, int index, PriorityListBox.EditPriorityOp op) =>
                    {
                        DataEditForm frmData = new DataEditForm();
                        frmData.Title = name + "/" + "New Priority";

                        StaticLoadMemberControl(frmData.ControlPanel, "(PriorityList) " + name + "[" + index + "]", typeof(Priority), new object[0] { }, priority, true);

                        frmData.SelectedOKEvent += () =>
                        {
                            object priorityObj = priority;
                            StaticSaveMemberControl(frmData.ControlPanel, name, typeof(Priority), ReflectionExt.GetPassableAttributes(2, attributes), ref priorityObj, true);
                            op(priority, index, (Priority)priorityObj);
                            frmData.Close();
                        };
                        frmData.SelectedCancelEvent += () =>
                        {
                            frmData.Close();
                        };

                        control.GetOwningForm().RegisterChild(frmData);
                        frmData.Show();
                    };

                    lbxValue.LoadFromList((IPriorityList)member);
                    control.Children.Add(lbxValue);
                }
                else if (!isWindow && ReflectionExt.FindAttribute<SubGroupAttribute>(attributes) == null)
                {
                    //in all cases where the class itself isn't being rendered to the window, simply represent as an editable object
                    LoadLabelControl(control, name);

                    if (member == null)
                    {
                        Type[] children = type.GetAssignableTypes();
                        //create an empty instance
                        member = ReflectionExt.CreateMinimalInstance(children[0]);
                    }

                    ClassBox cbxValue = new ClassBox();
                    MultilineAttribute attribute = ReflectionExt.FindAttribute<MultilineAttribute>(attributes);
                    //if (attribute != null)
                    //    cbxValue.Size = new Size(0, 80);
                    //else
                    //    cbxValue.Size = new Size(0, 29);
                    cbxValue.LoadFromSource(member);
                    control.Children.Add(cbxValue);

                    //add lambda expression for editing a single element
                    cbxValue.OnEditItem = (object element, ClassBox.EditElementOp op) =>
                    {
                        DataEditForm frmData = new DataEditForm();
                        frmData.Title = name + "/" + type.Name;

                        StaticLoadMemberControl(frmData.ControlPanel, name, type, ReflectionExt.GetPassableAttributes(0, attributes), element, true);

                        frmData.SelectedOKEvent += () =>
                        {
                            StaticSaveMemberControl(frmData.ControlPanel, name, type, ReflectionExt.GetPassableAttributes(0, attributes), ref element, true);
                            op(element);
                            frmData.Close();
                        };
                        frmData.SelectedCancelEvent += () =>
                        {
                            frmData.Close();
                        };

                        control.GetOwningForm().RegisterChild(frmData);
                        frmData.Show();
                    };
                }
                else
                {
                    LoadLabelControl(control, name);
                    //if it's a class of its own, create a new panel
                    //then pass it into the call
                    //use the returned "ref" int to determine how big the panel should be
                    //continue from there
                    Type[] children = type.GetAssignableTypes();

                    //handle null members by getting an instance of the FIRST instantiatable subclass (including itself) it can find
                    if (member == null)
                        member = ReflectionExt.CreateMinimalInstance(children[0]);

                    if (children.Length < 1)
                        throw new Exception("Completely abstract field found for: " + name);
                    else if (children.Count() == 1)
                    {
                        Border border = new Border();
                        border.BorderThickness = new Thickness(1);
                        border.BorderBrush = Avalonia.Media.Brushes.LightGray;
                        border.Margin = new Thickness(2);

                        StackPanel groupBoxPanel = new StackPanel();
                        groupBoxPanel.Margin = new Thickness(2);

                        {
                            ContextMenu copyPasteStrip = new ContextMenu();

                            MenuItem copyToolStripMenuItem = new MenuItem();
                            MenuItem pasteToolStripMenuItem = new MenuItem();

                            Avalonia.Collections.AvaloniaList<object> list = (Avalonia.Collections.AvaloniaList<object>)copyPasteStrip.Items;
                            list.AddRange(new MenuItem[] {
                            copyToolStripMenuItem,
                            pasteToolStripMenuItem});

                            copyToolStripMenuItem.Header = "Copy " + type.Name;
                            pasteToolStripMenuItem.Header = "Paste " + type.Name;

                            copyToolStripMenuItem.Click += (object copySender, RoutedEventArgs copyE) =>
                            {
                                object obj = ReflectionExt.CreateMinimalInstance(children[0]);
                                saveClassControls(obj, groupBoxPanel);
                                setClipboardObj(obj);
                            };
                            pasteToolStripMenuItem.Click += async (object copySender, RoutedEventArgs copyE) =>
                            {
                                Type type1 = clipboardObj.GetType();
                                Type type2 = type;
                                if (type2.IsAssignableFrom(type1))
                                {
                                    groupBoxPanel.Children.Clear();
                                    loadClassControls(clipboardObj, groupBoxPanel);
                                }
                                else
                                    await MessageBox.Show(control.GetOwningForm(), String.Format("Incompatible types:\n{0}\n{1}", type1.AssemblyQualifiedName, type2.AssemblyQualifiedName), "Invalid Operation", MessageBox.MessageBoxButtons.Ok);
                            };

                            groupBoxPanel.ContextMenu = copyPasteStrip;
                        }

                        loadClassControls(member, groupBoxPanel);
                        border.Child = groupBoxPanel;
                        control.Children.Add(border);
                    }
                    else
                    {
                        //note: considerations must be made when dealing with inheritance/polymorphism
                        //eg: find all children in this assembly that can be instantiated,
                        //add them to different panels
                        //show the one that is active right now
                        //include a combobox for switching children


                        Avalonia.Controls.Grid sharedRowPanel = getSharedRowPanel(2);

                        TextBlock lblType = new TextBlock();
                        lblType.Text = "Type:";
                        lblType.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
                        sharedRowPanel.Children.Add(lblType);
                        sharedRowPanel.ColumnDefinitions[0].Width = new GridLength(30);
                        lblType.SetValue(Avalonia.Controls.Grid.ColumnProperty, 0);

                        ComboBox cbValue = new ComboBox();
                        cbValue.Margin = new Thickness(4, 0, 0, 0);
                        cbValue.VirtualizationMode = ItemVirtualizationMode.Simple;
                        sharedRowPanel.Children.Add(cbValue);
                        cbValue.SetValue(Avalonia.Controls.Grid.ColumnProperty, 1);

                        control.Children.Add(sharedRowPanel);

                        Border border = new Border();
                        border.BorderThickness = new Thickness(1);
                        border.BorderBrush = Avalonia.Media.Brushes.LightGray;
                        border.Margin = new Thickness(2);

                        StackPanel groupBoxPanel = new StackPanel();
                        groupBoxPanel.Margin = new Thickness(2);

                        List<CreateMethod> createMethods = new List<CreateMethod>();

                        bool refreshPanel = true;
                        List<string> items = new List<string>();
                        int selection = 0;
                        for (int ii = 0; ii < children.Length; ii++)
                        {
                            Type childType = children[ii];
                            items.Add(childType.GetDisplayName());

                            createMethods.Add(() =>
                            {
                                if (refreshPanel)
                                {
                                    groupBoxPanel.Children.Clear();
                                    object emptyMember = ReflectionExt.CreateMinimalInstance(childType);
                                    loadClassControls(emptyMember, groupBoxPanel);//TODO: POTENTIAL INFINITE RECURSION
                                }
                            });
                            if (childType == member.GetType())
                                selection = ii;
                        }

                        var subject = new Subject<List<string>>();
                        cbValue.Bind(ComboBox.ItemsProperty, subject);
                        subject.OnNext(items);
                        cbValue.SelectedIndex = selection;

                        cbValue.SelectionChanged += (object sender, SelectionChangedEventArgs e) =>
                        {
                            createMethods[cbValue.SelectedIndex]();
                        };

                        {
                            ContextMenu copyPasteStrip = new ContextMenu();

                            MenuItem copyToolStripMenuItem = new MenuItem();
                            MenuItem pasteToolStripMenuItem = new MenuItem();

                            Avalonia.Collections.AvaloniaList<object> list = (Avalonia.Collections.AvaloniaList<object>)copyPasteStrip.Items;
                            list.AddRange(new MenuItem[] {
                            copyToolStripMenuItem,
                            pasteToolStripMenuItem});

                            copyToolStripMenuItem.Header = "Copy " + type.Name;
                            pasteToolStripMenuItem.Header = "Paste " + type.Name;

                            copyToolStripMenuItem.Click += (object copySender, RoutedEventArgs copyE) =>
                            {
                                object obj = ReflectionExt.CreateMinimalInstance(children[cbValue.SelectedIndex]);
                                saveClassControls(obj, groupBoxPanel);
                                setClipboardObj(obj);
                            };
                            pasteToolStripMenuItem.Click += async (object copySender, RoutedEventArgs copyE) =>
                            {
                                Type type1 = clipboardObj.GetType();
                                Type type2 = type;
                                int type_idx = -1;
                                for (int ii = 0; ii < children.Length; ii++)
                                {
                                    if (children[ii] == type1)
                                    {
                                        type_idx = ii;
                                        break;
                                    }
                                }
                                if (type_idx > -1)
                                {
                                    refreshPanel = false;
                                    cbValue.SelectedIndex = type_idx;
                                    refreshPanel = true;

                                    groupBoxPanel.Children.Clear();
                                    loadClassControls(clipboardObj, groupBoxPanel);
                                }
                                else
                                    await MessageBox.Show(control.GetOwningForm(), String.Format("Incompatible types:\n{0}\n{1}", type1.AssemblyQualifiedName, type2.AssemblyQualifiedName), "Invalid Operation", MessageBox.MessageBoxButtons.Ok);
                            };

                            groupBoxPanel.ContextMenu = copyPasteStrip;
                        }

                        loadClassControls(member, groupBoxPanel);
                        border.Child = groupBoxPanel;
                        control.Children.Add(border);
                    }
                }
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(e);
            }
        }

        public static void SaveDataControls(ref object obj, StackPanel control)
        {
            saveMemberControl(obj, control, obj.ToString(), obj.GetType(), null, ref obj, true);
        }

        private static void saveClassControls(object obj, StackPanel control)
        {
            Type objType = obj.GetType();
            Type[] interfaces = objType.GetInterfaces();
            foreach (IEditorConverter converter in converters)
            {
                Type convertType = converter.GetConvertingType();
                if (convertType == objType || objType.IsSubclassOf(convertType) || interfaces.Contains(convertType))
                {
                    converter.SaveClassControls(obj, control);
                    return;
                }
            }
            StaticSaveClassControls(obj, control);
        }

        public static void StaticSaveClassControls(object obj, StackPanel control)
        {
            try
            {
                Type type = obj.GetType();

                List<MemberInfo> myFields = type.GetEditableMembers();


                List<List<MemberInfo>> tieredFields = new List<List<MemberInfo>>();
                for (int ii = 0; ii < myFields.Count; ii++)
                {
                    if (myFields[ii].GetCustomAttributes(typeof(NonEditedAttribute), false).Length > 0)
                        continue;
                    if (myFields[ii].GetCustomAttributes(typeof(NonSerializedAttribute), false).Length > 0)
                        continue;

                    object member = myFields[ii].GetValue(obj);
                    if (member == null && myFields[ii].GetCustomAttributes(typeof(NonNullAttribute), false).Length > 0)
                        throw new Exception("Null class member found in " + type.ToString() + ": " + myFields[ii].Name);

                    if (myFields[ii].GetCustomAttributes(typeof(SharedRowAttribute), false).Length == 0)
                        tieredFields.Add(new List<MemberInfo>());

                    tieredFields[tieredFields.Count - 1].Add(myFields[ii]);
                }

                int controlIndex = 0;
                for (int ii = 0; ii < tieredFields.Count; ii++)
                {
                    if (tieredFields[ii].Count == 1)
                    {
                        MemberInfo myInfo = tieredFields[ii][0];
                        object member = myInfo.GetValue(obj);

                        saveMemberControl(obj, (StackPanel)control.Children[controlIndex], myInfo.Name, myInfo.GetMemberInfoType(), myInfo.GetCustomAttributes(false), ref member, false);
                        myInfo.SetValue(obj, member);
                        controlIndex++;
                    }
                    else
                    {
                        StackPanel sharedRowControl = (StackPanel)control.Children[controlIndex];
                        int sharedRowControlIndex = 0;
                        for (int jj = 0; jj < tieredFields[ii].Count; jj++)
                        {
                            MemberInfo myInfo = tieredFields[ii][jj];
                            object member = myInfo.GetValue(obj);

                            saveMemberControl(obj, (StackPanel)sharedRowControl.Children[jj], myInfo.Name, myInfo.GetMemberInfoType(), myInfo.GetCustomAttributes(false), ref member, false);
                            myInfo.SetValue(obj, member);
                            sharedRowControlIndex++;
                        }
                        controlIndex++;
                    }
                }
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(e);
            }
        }



        private static void saveMemberControl(object obj, StackPanel control, string name, Type type, object[] attributes, ref object member, bool isWindow)
        {
            Type objType = obj.GetType();
            Type[] interfaces = objType.GetInterfaces();
            foreach (IEditorConverter converter in converters)
            {
                Type convertType = converter.GetConvertingType();
                if (convertType == objType || objType.IsSubclassOf(convertType) || interfaces.Contains(convertType))
                {
                    converter.SaveMemberControl(obj, control, name, type, attributes, ref member, isWindow);
                    return;
                }
            }

            StaticSaveMemberControl(control, name, type, attributes, ref member, isWindow);
        }

        public static void StaticSaveMemberControl(StackPanel control, string name, Type type, object[] attributes, ref object member, bool isWindow)
        {
            //do not set anything
            //on save, write value to object
            //use completely clean controls for iterating child controls
            //must invoke save and load for structs
            //must use the attribute tag
            //does not need members; can be static methods then
            int controlIndex = 0;
            try
            {
                if (type.IsEnum)
                {
                    controlIndex++;

                    Array enums = type.GetEnumValues();
                    if (type.GetCustomAttributes(typeof(FlagsAttribute), false).Length > 0)
                    {
                        Avalonia.Controls.Grid innerControl = (Avalonia.Controls.Grid)control.Children[controlIndex];
                        int innerControlIndex = 0;

                        int pending = 0;
                        for (int ii = 0; ii < enums.Length; ii++)
                        {
                            int numeric = (int)enums.GetValue(ii);
                            int num1s = 0;
                            for (int jj = 0; jj < 32; jj++)
                            {
                                if ((numeric & 0x1) == 1)
                                    num1s++;
                                numeric = numeric >> 1;
                            }
                            if (num1s == 1)
                            {
                                CheckBox chkValue = (CheckBox)innerControl.Children[innerControlIndex];
                                pending |= ((chkValue.IsChecked.HasValue && chkValue.IsChecked.Value) ? 1 : 0) * (int)enums.GetValue(ii);
                                innerControlIndex++;
                            }
                        }
                        member = Enum.ToObject(type, pending);
                    }
                    else
                    {
                        ComboBox cbValue = (ComboBox)control.Children[controlIndex];
                        Array array = Enum.GetValues(type);
                        member = array.GetValue(cbValue.SelectedIndex);
                        controlIndex++;
                    }
                }
                else if (type == typeof(Boolean))
                {
                    CheckBox chkValue = (CheckBox)control.Children[controlIndex];
                    member = chkValue.IsChecked;
                    controlIndex++;
                }
                else if (type == typeof(Int32))
                {
                    controlIndex++;
                    DataTypeAttribute dataAtt = ReflectionExt.FindAttribute<DataTypeAttribute>(attributes);
                    FrameTypeAttribute frameAtt = ReflectionExt.FindAttribute<FrameTypeAttribute>(attributes);
                    if (dataAtt != null)
                    {
                        ComboBox cbValue = (ComboBox)control.Children[controlIndex];
                        int returnValue = cbValue.SelectedIndex;
                        if (dataAtt.IncludeInvalid)
                            returnValue--;
                        member = returnValue;
                    }
                    else if (frameAtt != null)
                    {
                        ComboBox cbValue = (ComboBox)control.Children[controlIndex];
                        if (!frameAtt.DashOnly)
                            member = cbValue.SelectedIndex;
                        else
                        {
                            int currentDashValue = -1;
                            for (int ii = 0; ii < GraphicsManager.Actions.Count; ii++)
                            {
                                if (GraphicsManager.Actions[ii].IsDash)
                                {
                                    currentDashValue++;
                                    if (currentDashValue == cbValue.SelectedIndex)
                                    {
                                        member = ii;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        NumericUpDown nudValue = (NumericUpDown)control.Children[controlIndex];
                        member = (Int32)nudValue.Value;
                    }
                    controlIndex++;
                }
                else if (type == typeof(byte))
                {
                    controlIndex++;
                    NumericUpDown nudValue = (NumericUpDown)control.Children[controlIndex];
                    member = (byte)nudValue.Value;
                    controlIndex++;
                }
                else if (type == typeof(Single))
                {
                    controlIndex++;
                    NumericUpDown nudValue = (NumericUpDown)control.Children[controlIndex];
                    member = (Single)nudValue.Value;
                    controlIndex++;
                }
                else if (type == typeof(Double))
                {
                    controlIndex++;
                    NumericUpDown nudValue = (NumericUpDown)control.Children[controlIndex];
                    member = (Double)nudValue.Value;
                    controlIndex++;
                }
                else if (type == typeof(Color))
                {
                    controlIndex++;
                    Avalonia.Controls.Grid innerControl = (Avalonia.Controls.Grid)control.Children[controlIndex];
                    int innerControlIndex = 0;

                    innerControlIndex++;
                    NumericUpDown nudValueR = (NumericUpDown)innerControl.Children[innerControlIndex];
                    innerControlIndex++;
                    innerControlIndex++;
                    NumericUpDown nudValueG = (NumericUpDown)innerControl.Children[innerControlIndex];
                    innerControlIndex++;
                    innerControlIndex++;
                    NumericUpDown nudValueB = (NumericUpDown)innerControl.Children[innerControlIndex];
                    innerControlIndex++;
                    innerControlIndex++;
                    NumericUpDown nudValueA = (NumericUpDown)innerControl.Children[innerControlIndex];
                    innerControlIndex++;
                    member = new Color((int)nudValueR.Value, (int)nudValueG.Value, (int)nudValueB.Value, (int)nudValueA.Value);
                }
                else if (type == typeof(FlagType))
                {
                    StringTypeConstraintAttribute dataAtt = ReflectionExt.FindAttribute<StringTypeConstraintAttribute>(attributes);
                    if (dataAtt != null)
                    {
                        Type baseType = dataAtt.BaseClass;

                        Type[] children = baseType.GetAssignableTypes();

                        Avalonia.Controls.Grid subGrid = (Avalonia.Controls.Grid)control.Children[controlIndex];
                        ComboBox cbValue = (ComboBox)subGrid.Children[1];
                        member = new FlagType(children[cbValue.SelectedIndex]);
                        controlIndex++;
                    }
                }
                else if (type == typeof(String))
                {
                    controlIndex++;
                    //for strings, use an edit textbox
                    if (ReflectionExt.FindAttribute<AnimAttribute>(attributes) != null || ReflectionExt.FindAttribute<SoundAttribute>(attributes) != null)
                    {
                        ComboBox cbValue = (ComboBox)control.Children[controlIndex];
                        if (cbValue.SelectedIndex == 0)
                            member = "";
                        else
                            member = (string)cbValue.SelectedItem;
                    }
                    else
                    {
                        TextBox txtValue = (TextBox)control.Children[controlIndex];
                        member = (String)txtValue.Text;
                    }
                    controlIndex++;
                }
                else if (type == typeof(Priority))
                {
                    controlIndex++;
                    //attempt to parse
                    //TODO: enforce validation
                    TextBox txtValue = (TextBox)control.Children[controlIndex];
                    string[] divText = txtValue.Text.Split('.');
                    int[] divNums = new int[divText.Length];
                    for (int ii = 0; ii < divText.Length; ii++)
                    {
                        int res;
                        if (int.TryParse(divText[ii], out res))
                            divNums[ii] = res;
                        else
                        {
                            divNums = null;
                            break;
                        }
                    }
                    member = new Priority(divNums);
                    controlIndex++;
                }
                else if (type == typeof(Loc))
                {
                    controlIndex++;
                    Avalonia.Controls.Grid innerControl = (Avalonia.Controls.Grid)control.Children[controlIndex];
                    int innerControlIndex = 0;

                    innerControlIndex++;
                    NumericUpDown nudValueX = (NumericUpDown)innerControl.Children[innerControlIndex];
                    innerControlIndex++;
                    innerControlIndex++;
                    NumericUpDown nudValueY = (NumericUpDown)innerControl.Children[innerControlIndex];
                    member = new Loc((int)nudValueX.Value, (int)nudValueY.Value);
                    innerControlIndex++;
                }
                else if (type == typeof(SegLoc))
                {
                    controlIndex++;
                    Avalonia.Controls.Grid innerControl = (Avalonia.Controls.Grid)control.Children[controlIndex];
                    int innerControlIndex = 0;

                    innerControlIndex++;
                    NumericUpDown nudValueX = (NumericUpDown)innerControl.Children[innerControlIndex];
                    innerControlIndex++;
                    innerControlIndex++;
                    NumericUpDown nudValueY = (NumericUpDown)innerControl.Children[innerControlIndex];
                    member = new SegLoc((int)nudValueX.Value, (int)nudValueY.Value);
                    innerControlIndex++;
                }
                else if (type == typeof(IntRange))
                {
                    controlIndex++;
                    Avalonia.Controls.Grid innerControl = (Avalonia.Controls.Grid)control.Children[controlIndex];
                    int innerControlIndex = 0;

                    innerControlIndex++;
                    NumericUpDown nudValueX = (NumericUpDown)innerControl.Children[innerControlIndex];
                    innerControlIndex++;
                    innerControlIndex++;
                    NumericUpDown nudValueY = (NumericUpDown)innerControl.Children[innerControlIndex];
                    member = new IntRange((int)nudValueX.Value, (int)nudValueY.Value);
                    innerControlIndex++;
                }
                //else if (type == typeof(TileLayer))
                //{
                //    controlIndex++;
                //    TilePreview preview = (TilePreview)control.Children[controlIndex];
                //    member = preview.GetChosenAnim();
                //    controlIndex++;
                //}
                //else if (type.GetInterfaces().Contains(typeof(IList<TileLayer>)))
                //{
                //    controlIndex++;
                //    controlIndex++;
                //    CollectionBox lbxValue = (CollectionBox)control.Children[controlIndex];
                //    member = lbxValue.Collection;
                //    controlIndex++;
                //}
                else if (type.Equals(typeof(Type)))
                {
                    TypeConstraintAttribute dataAtt = ReflectionExt.FindAttribute<TypeConstraintAttribute>(attributes);
                    Type baseType = dataAtt.BaseClass;

                    Type[] children = baseType.GetAssignableTypes();

                    Avalonia.Controls.Grid subGrid = (Avalonia.Controls.Grid)control.Children[controlIndex];
                    ComboBox cbValue = (ComboBox)subGrid.Children[1];
                    member = children[cbValue.SelectedIndex];
                    controlIndex++;
                }
                else if (type.IsArray)
                {
                    //TODO: 2D array grid support
                    //if (type.GetElementType().IsArray)

                    controlIndex++;
                    CollectionBox lbxValue = (CollectionBox)control.Children[controlIndex];
                    List<object> objList = (List<object>)lbxValue.GetList(typeof(List<object>));

                    Array array = Array.CreateInstance(type.GetElementType(), objList.Count);
                    for (int ii = 0; ii < objList.Count; ii++)
                        array.SetValue(objList[ii], ii);

                    member = array;
                    controlIndex++;

                }
                else if (type.GetInterfaces().Contains(typeof(IList)))
                {
                    controlIndex++;
                    CollectionBox lbxValue = (CollectionBox)control.Children[controlIndex];
                    member = lbxValue.GetList(type);
                    controlIndex++;
                }
                else if (type.GetInterfaces().Contains(typeof(IDictionary)))
                {
                    controlIndex++;
                    DictionaryBox lbxValue = (DictionaryBox)control.Children[controlIndex];
                    member = lbxValue.GetDict(type);
                    controlIndex++;
                }
                else if (type.GetInterfaces().Contains(typeof(IPriorityList)))
                {
                    controlIndex++;
                    PriorityListBox lbxValue = (PriorityListBox)control.Children[controlIndex];
                    member = lbxValue.GetList(type);
                    controlIndex++;
                }
                else if (!isWindow && ReflectionExt.FindAttribute<SubGroupAttribute>(attributes) == null)
                {
                    controlIndex++;
                    ClassBox cbxValue = (ClassBox)control.Children[controlIndex];
                    member = cbxValue.Object;
                    controlIndex++;
                }
                else
                {
                    controlIndex++;
                    Type[] children = type.GetAssignableTypes();

                    //need to create a new instance
                    //note: considerations must be made when dealing with inheritance/polymorphism
                    //eg: check to see if there are children of the type,
                    //and if so, do this instead:
                    //get the combobox index determining the type
                    //instantiate the type
                    //get the panel for the index
                    //save using THAT panel

                    if (children.Length == 1)
                        member = ReflectionExt.CreateMinimalInstance(children[0]);
                    else
                    {

                        Avalonia.Controls.Grid subGrid = (Avalonia.Controls.Grid)control.Children[controlIndex];
                        ComboBox cbValue = (ComboBox)subGrid.Children[1];

                        member = ReflectionExt.CreateMinimalInstance(children[cbValue.SelectedIndex]);
                        controlIndex++;
                    }

                    Border border = (Border)control.Children[controlIndex];
                    saveClassControls(member, (StackPanel)border.Child);
                    controlIndex++;
                }
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(e);
            }
        }

        private static void CbValue_PlaySound(object sender, SelectionChangedEventArgs e)
        {
            ComboBox box = (ComboBox)sender;
            if (box.SelectedIndex > 0)
            {
                lock (GameBase.lockObj)
                    GameManager.Instance.BattleSE((string)box.SelectedItem);
            }
        }

        //TODO: WPF data binding would invalidate this

        public static ReflectionExt.TypeStringConv GetStringRep(Type type, object[] attributes)
        {
            if (type == typeof(Int32))
            {
                DataTypeAttribute dataAtt = ReflectionExt.FindAttribute<DataTypeAttribute>(attributes);
                FrameTypeAttribute frameAtt = ReflectionExt.FindAttribute<FrameTypeAttribute>(attributes);
                if (dataAtt != null)
                {
                    Data.EntryDataIndex nameIndex = Data.DataManager.Instance.DataIndices[dataAtt.DataType];
                    return (obj) => { return ((int)obj >= 0 & (int)obj < nameIndex.Count) ? nameIndex.Entries[(int)obj].GetLocalString(false) : "---"; };
                }
                else if (frameAtt != null)
                {
                    return (obj) => { return ((int)obj >= 0 & (int)obj < GraphicsManager.Actions.Count) ? GraphicsManager.Actions[(int)obj].Name : "---"; };
                }
            }
            return (obj) => { return obj == null ? "[NULL]" : obj.ToString(); };
        }

        private static void setClipboardObj(object obj)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, obj);

                stream.Flush();
                stream.Position = 0;

                clipboardObj = formatter.Deserialize(stream);
            }
        }
    }
}

