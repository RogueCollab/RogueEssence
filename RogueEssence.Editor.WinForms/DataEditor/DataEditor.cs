using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Reflection;
using System.Drawing;
using RogueEssence.Content;
using RogueEssence.Dungeon;
using RogueElements;
using System.IO;

namespace RogueEssence.Dev
{
    public static class DataEditor
    {
        private const int LABEL_HEIGHT = 14;

        private static List<IEditorConverter> converters;

        public static void Init()
        {
            converters = new List<IEditorConverter>();
            AddConverter(new AutoTileBaseConverter());
            AddConverter(new BaseEmitterConverter());
            AddConverter(new BattleDataConverter());
            AddConverter(new BattleFXConverter());
            AddConverter(new CircleSquareEmitterConverter());
            AddConverter(new CombatActionConverter());
            AddConverter(new ExplosionDataConverter());
            //AddConverter(new ItemDataConverter());
            AddConverter(new ShootingEmitterConverter());
            AddConverter(new SkillDataConverter());
            AddConverter(new SpawnListConverter());
            AddConverter(new TypeDictConverter());
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


        public static void LoadDataControls(object obj, TableLayoutPanel control)
        {
            loadMemberControl(obj, control, obj.ToString(), obj.GetType(), null, obj, true);
        }

        private static void loadClassControls(object obj, TableLayoutPanel control)
        {
            control.SuspendLayout();
            Type objType = obj.GetType();
            Type[] interfaces = objType.GetInterfaces();
            foreach (IEditorConverter converter in converters)
            {
                Type convertType = converter.GetConvertingType();
                if (convertType == objType || objType.IsSubclassOf(convertType) || interfaces.Contains(convertType))
                {
                    converter.LoadClassControls(obj, control);
                    control.ResumeLayout(false);
                    return;
                }
            }
            
            StaticLoadClassControls(obj, control);
            control.ResumeLayout(false);
        }

        public static void StaticLoadClassControls(object obj, TableLayoutPanel control)
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
                        staticLoadClassControl(obj, control, tieredFields[ii][0]);
                    else
                    {
                        TableLayoutPanel sharedRowPanel = getSharedRowPanel(tieredFields[ii].Count);
                        control.Controls.Add(sharedRowPanel);
                        sharedRowPanel.SuspendLayout();
                        for (int jj = 0; jj < tieredFields[ii].Count; jj++)
                            staticLoadClassControl(obj, sharedRowPanel, tieredFields[ii][jj]);
                        sharedRowPanel.ResumeLayout(false);
                    }
                }
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(e);
            }
        }

        private static TableLayoutPanel getSharedRowPanel(int cols)
        {
            TableLayoutPanel sharedRowPanel = new TableLayoutPanel();
            sharedRowPanel.AutoSize = true;
            sharedRowPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            sharedRowPanel.ColumnCount = cols;
            for (int jj = 0; jj < cols; jj++)
                sharedRowPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F / cols));
            sharedRowPanel.Dock = DockStyle.Fill;
            sharedRowPanel.Padding = new Padding(0, 0, 0, 0);
            sharedRowPanel.Margin = new Padding(0, 0, 0, 0);
            sharedRowPanel.RowCount = 1;
            sharedRowPanel.RowStyles.Add(new RowStyle());
            sharedRowPanel.GrowStyle = TableLayoutPanelGrowStyle.AddRows;

            return sharedRowPanel;
        }

        private static void staticLoadClassControl(object obj, TableLayoutPanel parentControl, MemberInfo myInfo)
        {
            object member = myInfo.GetValue(obj);


            TableLayoutPanel control = new TableLayoutPanel();
            control.AutoSize = true;
            control.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            control.ColumnCount = 1;
            control.ColumnStyles.Add(new ColumnStyle());
            control.Dock = DockStyle.Fill;
            control.Padding = new Padding(0, 0, 0, 0);
            control.RowCount = 1;
            control.RowStyles.Add(new RowStyle());

            parentControl.Controls.Add(control);

            loadMemberControl(obj, control, myInfo.Name, myInfo.GetMemberInfoType(), myInfo.GetCustomAttributes(false), member, false);
        }


        private delegate void CreateMethod();

        public static void LoadLabelControl(TableLayoutPanel control, string name)
        {
            Label lblName = new Label();
            lblName.Dock = DockStyle.Fill;
            lblName.Size = new Size(0, 13);
            lblName.Margin = new Padding(0, 0, 0, 0);
            //StringBuilder separatedName = new StringBuilder();
            //for (int ii = 0; ii < name.Length; ii++)
            //{
            //    if (ii > 0 && (char.IsUpper(name[ii]) && !char.IsLower(name[ii-1]) || char.IsDigit(name[ii])))
            //        separatedName.Append(' ');
            //    separatedName.Append(name[ii]);
            //}
            //separatedName.Append(":");
            lblName.Text = name + ":";
            control.Controls.Add(lblName);
        }


        private static void loadMemberControl(object obj, TableLayoutPanel control, string name, Type type, object[] attributes, object member, bool isWindow)
        {
            control.SuspendLayout();
            Type objType = obj.GetType();
            Type[] interfaces = objType.GetInterfaces();
            foreach (IEditorConverter converter in converters)
            {
                Type convertType = converter.GetConvertingType();
                if (convertType == objType || objType.IsSubclassOf(convertType) || interfaces.Contains(convertType))
                {
                    converter.LoadMemberControl(obj, control, name, type, attributes, member, isWindow);
                    control.ResumeLayout(false);
                    return;
                }
            }
            StaticLoadMemberControl(control, name, type, attributes, member, isWindow);
            control.ResumeLayout(false);
        }


        //overload this method in children to account for structs such as loc
        public static void StaticLoadMemberControl(TableLayoutPanel control, string name, Type type, object[] attributes, object member, bool isWindow)
        {

            //members are set when control values are changed?
            try
            {
                if (type.IsEnum)
                {
                    LoadLabelControl(control, name);


                    Array enums = type.GetEnumValues();
                    if (type.GetCustomAttributes(typeof(FlagsAttribute), false).Length > 0)
                    {
                        TableLayoutPanel innerPanel = getSharedRowPanel(2);
                        innerPanel.SuspendLayout();

                        int amount = 0;
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
                                chkValue.Dock = DockStyle.Fill;
                                chkValue.Size = new Size(0, 17);
                                chkValue.Text = enums.GetValue(ii).ToString();
                                chkValue.Checked = ((int)member & (int)enums.GetValue(ii)) > 0;
                                innerPanel.Controls.Add(chkValue);
                                amount++;
                            }
                        }
                        control.Controls.Add(innerPanel);
                        innerPanel.ResumeLayout(false);
                    }
                    else
                    {
                        //for enums, use a combobox
                        ComboBox cbValue = new ComboBox();
                        cbValue.Dock = DockStyle.Fill;
                        cbValue.Size = new Size(0, 21);
                        cbValue.DropDownStyle = ComboBoxStyle.DropDownList;
                        cbValue.FormattingEnabled = true;
                        for (int ii = 0; ii < enums.Length; ii++)
                        {
                            cbValue.Items.Add(enums.GetValue(ii).ToString());
                            if (Enum.Equals(enums.GetValue(ii), member))
                                cbValue.SelectedIndex = ii;
                        }
                        control.Controls.Add(cbValue);
                    }
                }
                else if (type == typeof(Boolean))
                {
                    CheckBox chkValue = new CheckBox();
                    chkValue.Dock = DockStyle.Fill;
                    chkValue.Size = new Size(0, 17);
                    chkValue.Text = name;
                    chkValue.Checked = (member == null) ? false : (bool)member;
                    control.Controls.Add(chkValue);
                }
                else if (type == typeof(byte))
                {
                    LoadLabelControl(control, name);

                    IntNumericUpDown nudValue = new IntNumericUpDown();
                    nudValue.Dock = DockStyle.Fill;
                    nudValue.Size = new Size(0, 21);
                    nudValue.Minimum = byte.MinValue;
                    nudValue.Maximum = byte.MaxValue;
                    NumberRangeAttribute attribute = ReflectionExt.FindAttribute<NumberRangeAttribute>(attributes);
                    if (attribute != null)
                    {
                        nudValue.Minimum = attribute.Min;
                        nudValue.Maximum = attribute.Max;
                    }
                    nudValue.Value = (member == null) ? (nudValue.Minimum > 0 ? nudValue.Minimum : 0) : (byte)member;
                    control.Controls.Add(nudValue);
                }
                else if (type == typeof(Int32))
                {
                    LoadLabelControl(control, name);

                    DataTypeAttribute dataAtt = ReflectionExt.FindAttribute<DataTypeAttribute>(attributes);
                    FrameTypeAttribute frameAtt = ReflectionExt.FindAttribute<FrameTypeAttribute>(attributes);
                    if (dataAtt != null)
                    {
                        ComboBox cbValue = new ComboBox();
                        cbValue.Dock = DockStyle.Fill;
                        cbValue.Size = new Size(0, 21);
                        cbValue.DropDownStyle = ComboBoxStyle.DropDownList;
                        cbValue.FormattingEnabled = true;
                        int chosenIndex = (member == null) ? 0 : (Int32)member;
                        Data.EntryDataIndex nameIndex = Data.DataManager.Instance.DataIndices[dataAtt.DataType];
                        if (dataAtt.IncludeInvalid)
                        {
                            cbValue.Items.Add("---");
                            chosenIndex++;
                        }

                        for (int ii = 0; ii < nameIndex.Count; ii++)
                            cbValue.Items.Add(ii.ToString() + ": " + nameIndex.Entries[ii].GetLocalString(false));

                        cbValue.SelectedIndex = Math.Min(Math.Max(0, chosenIndex), cbValue.Items.Count - 1);
                        control.Controls.Add(cbValue);
                    }
                    else if (frameAtt != null)
                    {
                        ComboBox cbValue = new ComboBox();
                        cbValue.Dock = DockStyle.Fill;
                        cbValue.Size = new Size(0, 21);
                        cbValue.DropDownStyle = ComboBoxStyle.DropDownList;
                        cbValue.FormattingEnabled = true;
                        int chosenIndex = 0;

                        for (int ii = 0; ii < GraphicsManager.Actions.Count; ii++)
                        {
                            if (!frameAtt.DashOnly || GraphicsManager.Actions[ii].IsDash)
                            {
                                if (ii == (int)member)
                                    chosenIndex = cbValue.Items.Count;
                                cbValue.Items.Add(GraphicsManager.Actions[ii].Name);
                            }
                        }

                        cbValue.SelectedIndex = Math.Min(Math.Max(0, chosenIndex), cbValue.Items.Count - 1);
                        control.Controls.Add(cbValue);
                    }
                    else
                    {
                        IntNumericUpDown nudValue = new IntNumericUpDown();
                        nudValue.Dock = DockStyle.Fill;
                        nudValue.Size = new Size(0, 21);
                        nudValue.Minimum = Int32.MinValue;
                        nudValue.Maximum = Int32.MaxValue;
                        NumberRangeAttribute rangeAtt = ReflectionExt.FindAttribute<NumberRangeAttribute>(attributes);
                        if (rangeAtt != null)
                        {
                            nudValue.Minimum = rangeAtt.Min;
                            nudValue.Maximum = rangeAtt.Max;
                        }
                        nudValue.Value = (member == null) ? (nudValue.Minimum > 0 ? nudValue.Minimum : 0) : (Int32)member;

                        control.Controls.Add(nudValue);
                    }
                }
                else if (type == typeof(byte))
                {
                    LoadLabelControl(control, name);

                    IntNumericUpDown nudValue = new IntNumericUpDown();
                    nudValue.Dock = DockStyle.Fill;
                    nudValue.Size = new Size(0, 21);
                    nudValue.Minimum = byte.MinValue;
                    nudValue.Maximum = byte.MaxValue;
                    NumberRangeAttribute rangeAtt = ReflectionExt.FindAttribute<NumberRangeAttribute>(attributes);
                    if (rangeAtt != null)
                    {
                        nudValue.Minimum = rangeAtt.Min;
                        nudValue.Maximum = rangeAtt.Max;
                    }
                    nudValue.Value = (member == null) ? (nudValue.Minimum > 0 ? nudValue.Minimum : 0) : (Int32)member;

                    control.Controls.Add(nudValue);
                }
                else if (type == typeof(Single))
                {
                    LoadLabelControl(control, name);

                    NumericUpDown nudValue = new NumericUpDown();
                    nudValue.Dock = DockStyle.Fill;
                    nudValue.Size = new Size(0, 21);
                    nudValue.Minimum = Int32.MinValue;
                    nudValue.Maximum = Int32.MaxValue;
                    NumberRangeAttribute attribute = ReflectionExt.FindAttribute<NumberRangeAttribute>(attributes);
                    if (attribute != null)
                    {
                        nudValue.Minimum = attribute.Min;
                        nudValue.Maximum = attribute.Max;
                    }
                    float value = (member == null) ? (float)(nudValue.Minimum > 0 ? nudValue.Minimum : 0) : (float)member;
                    nudValue.Value = (decimal)value;
                    control.Controls.Add(nudValue);
                }
                else if (type == typeof(Double))
                {
                    LoadLabelControl(control, name);

                    NumericUpDown nudValue = new NumericUpDown();
                    nudValue.Dock = DockStyle.Fill;
                    nudValue.Size = new Size(0, 21);
                    nudValue.Minimum = Int32.MinValue;
                    nudValue.Maximum = Int32.MaxValue;
                    NumberRangeAttribute attribute = ReflectionExt.FindAttribute<NumberRangeAttribute>(attributes);
                    if (attribute != null)
                    {
                        nudValue.Minimum = attribute.Min;
                        nudValue.Maximum = attribute.Max;
                    }
                    double value = (member == null) ? (double)(nudValue.Minimum > 0 ? nudValue.Minimum : 0) : (double)member;
                    nudValue.Value = (decimal)value;
                    control.Controls.Add(nudValue);
                }
                else if (type == typeof(FlagType))
                {
                    StringTypeConstraintAttribute dataAtt = ReflectionExt.FindAttribute<StringTypeConstraintAttribute>(attributes);

                    if (dataAtt != null)
                    {
                        Type baseType = dataAtt.BaseClass;

                        Type[] children = baseType.GetAssignableTypes();

                        TableLayoutPanel sharedRowPanel = new TableLayoutPanel();
                        sharedRowPanel.SuspendLayout();

                        sharedRowPanel.AutoSize = true;
                        sharedRowPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
                        sharedRowPanel.ColumnCount = 2;
                        sharedRowPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
                        sharedRowPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
                        sharedRowPanel.Dock = DockStyle.Fill;
                        sharedRowPanel.Padding = new Padding(0, 0, 0, 0);
                        sharedRowPanel.Margin = new Padding(0, 0, 0, 0);
                        sharedRowPanel.RowCount = 1;
                        sharedRowPanel.RowStyles.Add(new RowStyle());


                        Label lblType = new Label();
                        lblType.Dock = DockStyle.Fill;
                        lblType.Size = new Size(36, 13);
                        lblType.Text = "Type: ";
                        lblType.TextAlign = ContentAlignment.MiddleLeft;
                        sharedRowPanel.Controls.Add(lblType);

                        ComboBox cbValue = new ComboBox();
                        cbValue.Dock = DockStyle.Fill;
                        cbValue.Size = new Size(0, 21);
                        cbValue.DropDownStyle = ComboBoxStyle.DropDownList;
                        cbValue.FormattingEnabled = true;
                        sharedRowPanel.Controls.Add(cbValue);

                        for (int ii = 0; ii < children.Length; ii++)
                        {
                            Type childType = children[ii];
                            cbValue.Items.Add(childType.GetDisplayName());

                            if (childType == ((FlagType)member).FullType)
                                cbValue.SelectedIndex = ii;
                        }
                        if (cbValue.SelectedIndex == -1)
                            cbValue.SelectedIndex = 0;

                        control.Controls.Add(sharedRowPanel);
                        sharedRowPanel.ResumeLayout(false);
                    }
                }
                else if (type == typeof(String))
                {
                    LoadLabelControl(control, name);

                    AnimAttribute animAtt = ReflectionExt.FindAttribute<AnimAttribute>(attributes);
                    if (animAtt != null)
                    {
                        ComboBox cbValue = new ComboBox();
                        cbValue.Dock = DockStyle.Fill;
                        cbValue.Size = new Size(0, 21);
                        cbValue.DropDownStyle = ComboBoxStyle.DropDownList;
                        cbValue.FormattingEnabled = true;
                        string choice = (string)member;

                        cbValue.Items.Add("---");
                        int chosenIndex = 0;

                        string[] dirs = Directory.GetFiles(animAtt.FolderPath);

                        for (int ii = 0; ii < dirs.Length; ii++)
                        {
                            string filename = Path.GetFileNameWithoutExtension(dirs[ii]);
                            if (filename == choice)
                                chosenIndex = cbValue.Items.Count;
                            cbValue.Items.Add(filename);
                        }

                        cbValue.SelectedIndex = chosenIndex;
                        control.Controls.Add(cbValue);
                    }
                    else if (ReflectionExt.FindAttribute<SoundAttribute>(attributes) != null)
                    {
                        //is it a sound effect?

                        ComboBox cbValue = new ComboBox();
                        cbValue.Dock = DockStyle.Fill;
                        cbValue.Size = new Size(0, 21);
                        cbValue.DropDownStyle = ComboBoxStyle.DropDownList;
                        cbValue.FormattingEnabled = true;
                        string choice = (string)member;

                        cbValue.Items.Add("---");
                        int chosenIndex = 0;

                        string[] dirs = Directory.GetFiles(DiagManager.CONTENT_PATH + "Sound/Battle");

                        for (int ii = 0; ii < dirs.Length; ii++)
                        {
                            string filename = Path.GetFileNameWithoutExtension(dirs[ii]);
                            if (filename == choice)
                                chosenIndex = cbValue.Items.Count;
                            cbValue.Items.Add(filename);
                        }

                        cbValue.SelectedIndexChanged += new System.EventHandler(CbValue_PlaySound);
                        cbValue.SelectedIndex = chosenIndex;
                        control.Controls.Add(cbValue);

                    }
                    else
                    {
                        //for strings, use an edit textbox
                        TextBox txtValue = new TextBox();
                        txtValue.Dock = DockStyle.Fill;
                        MultilineAttribute attribute = ReflectionExt.FindAttribute<MultilineAttribute>(attributes);
                        if (attribute != null)
                        {
                            txtValue.Multiline = true;
                            txtValue.Size = new Size(0, 80);
                        }
                        else
                            txtValue.Size = new Size(0, 20);
                        txtValue.Text = (member == null) ? "" : (String)member;
                        control.Controls.Add(txtValue);
                    }
                }
                else if (type == typeof(Microsoft.Xna.Framework.Color))
                {
                    LoadLabelControl(control, name);

                    TableLayoutPanel innerPanel = getSharedRowPanel(4);
                    innerPanel.SuspendLayout();

                    Label lblR = new Label();
                    lblR.Dock = DockStyle.Fill;
                    lblR.Size = new Size(0, 13);
                    lblR.Text = "R:";
                    innerPanel.Controls.Add(lblR);

                    Label lblG = new Label();
                    lblG.Dock = DockStyle.Fill;
                    lblG.Size = new Size(0, 13);
                    lblG.Text = "G:";
                    innerPanel.Controls.Add(lblG);

                    Label lblB = new Label();
                    lblB.Dock = DockStyle.Fill;
                    lblB.Size = new Size(0, 13);
                    lblB.Text = "B:";
                    innerPanel.Controls.Add(lblB);

                    Label lblA = new Label();
                    lblA.Dock = DockStyle.Fill;
                    lblA.Size = new Size(0, 13);
                    lblA.Text = "A:";
                    innerPanel.Controls.Add(lblA);

                    IntNumericUpDown nudValueR = new IntNumericUpDown();
                    nudValueR.Dock = DockStyle.Fill;
                    nudValueR.Size = new Size(0, 21);
                    nudValueR.Minimum = byte.MinValue;
                    nudValueR.Maximum = byte.MaxValue;
                    nudValueR.Value = (member == null) ? 0 : ((Microsoft.Xna.Framework.Color)member).R;
                    innerPanel.Controls.Add(nudValueR);
                    IntNumericUpDown nudValueG = new IntNumericUpDown();
                    nudValueG.Dock = DockStyle.Fill;
                    nudValueG.Size = new Size(0, 21);
                    nudValueG.Minimum = byte.MinValue;
                    nudValueG.Maximum = byte.MaxValue;
                    nudValueG.Value = (member == null) ? 0 : ((Microsoft.Xna.Framework.Color)member).G;
                    innerPanel.Controls.Add(nudValueG);
                    IntNumericUpDown nudValueB = new IntNumericUpDown();
                    nudValueB.Dock = DockStyle.Fill;
                    nudValueB.Size = new Size(0, 21);
                    nudValueB.Minimum = byte.MinValue;
                    nudValueB.Maximum = byte.MaxValue;
                    nudValueB.Value = (member == null) ? 0 : ((Microsoft.Xna.Framework.Color)member).B;
                    innerPanel.Controls.Add(nudValueB);
                    IntNumericUpDown nudValueA = new IntNumericUpDown();
                    nudValueA.Dock = DockStyle.Fill;
                    nudValueA.Size = new Size(0, 21);
                    nudValueA.Minimum = byte.MinValue;
                    nudValueA.Maximum = byte.MaxValue;
                    nudValueA.Value = (member == null) ? 0 : ((Microsoft.Xna.Framework.Color)member).A;
                    innerPanel.Controls.Add(nudValueA);

                    control.Controls.Add(innerPanel);
                    innerPanel.ResumeLayout(false);

                }
                else if (type == typeof(Loc))
                {
                    LoadLabelControl(control, name);

                    TableLayoutPanel innerPanel = getSharedRowPanel(2);
                    innerPanel.SuspendLayout();

                    Label lblX = new Label();
                    lblX.Dock = DockStyle.Fill;
                    lblX.Size = new Size(0, 13);
                    lblX.Text = "X:";
                    innerPanel.Controls.Add(lblX);

                    Label lblY = new Label();
                    lblY.Dock = DockStyle.Fill;
                    lblY.Size = new Size(0, 13);
                    lblY.Text = "Y:";
                    innerPanel.Controls.Add(lblY);

                    IntNumericUpDown nudValueX = new IntNumericUpDown();
                    nudValueX.Dock = DockStyle.Fill;
                    nudValueX.Size = new Size(0, 21);
                    nudValueX.Minimum = Int32.MinValue;
                    nudValueX.Maximum = Int32.MaxValue;
                    nudValueX.Value = (member == null) ? 0 : ((Loc)member).X;
                    innerPanel.Controls.Add(nudValueX);
                    IntNumericUpDown nudValueY = new IntNumericUpDown();
                    nudValueY.Dock = DockStyle.Fill;
                    nudValueY.Size = new Size(0, 21);
                    nudValueY.Minimum = Int32.MinValue;
                    nudValueY.Maximum = Int32.MaxValue;
                    nudValueY.Value = (member == null) ? 0 : ((Loc)member).Y;
                    innerPanel.Controls.Add(nudValueY);

                    control.Controls.Add(innerPanel);
                    innerPanel.ResumeLayout(false);
                }
                else if (type == typeof(SegLoc))
                {
                    LoadLabelControl(control, name);

                    TableLayoutPanel innerPanel = getSharedRowPanel(2);
                    innerPanel.SuspendLayout();

                    Label lblX = new Label();
                    lblX.Dock = DockStyle.Fill;
                    lblX.Size = new Size(0, 13);
                    lblX.Text = "Structure:";
                    innerPanel.Controls.Add(lblX);
                    Label lblY = new Label();
                    lblY.Dock = DockStyle.Fill;
                    lblY.Size = new Size(0, 13);
                    lblY.Text = "Map:";
                    innerPanel.Controls.Add(lblY);

                    IntNumericUpDown nudValueX = new IntNumericUpDown();
                    nudValueX.Dock = DockStyle.Fill;
                    nudValueX.Size = new Size(0, 21);
                    nudValueX.Minimum = Int32.MinValue;
                    nudValueX.Maximum = Int32.MaxValue;
                    nudValueX.Value = (member == null) ? 0 : ((SegLoc)member).Segment;
                    innerPanel.Controls.Add(nudValueX);
                    IntNumericUpDown nudValueY = new IntNumericUpDown();
                    nudValueY.Dock = DockStyle.Fill;
                    nudValueY.Size = new Size(0, 21);
                    nudValueY.Minimum = Int32.MinValue;
                    nudValueY.Maximum = Int32.MaxValue;
                    nudValueY.Value = (member == null) ? 0 : ((SegLoc)member).ID;
                    innerPanel.Controls.Add(nudValueY);

                    control.Controls.Add(innerPanel);
                    innerPanel.ResumeLayout(false);

                }
                else if (type == typeof(IntRange))
                {
                    LoadLabelControl(control, name);

                    TableLayoutPanel innerPanel = getSharedRowPanel(2);
                    innerPanel.SuspendLayout();

                    Label lblX = new Label();
                    lblX.Dock = DockStyle.Fill;
                    lblX.Size = new Size(0, 13);
                    lblX.Text = "Min:";
                    innerPanel.Controls.Add(lblX);
                    Label lblY = new Label();
                    lblY.Dock = DockStyle.Fill;
                    lblY.Size = new Size(0, 13);
                    lblY.Text = "Map:";
                    innerPanel.Controls.Add(lblY);

                    IntNumericUpDown nudValueX = new IntNumericUpDown();
                    nudValueX.Dock = DockStyle.Fill;
                    nudValueX.Size = new Size(0, 21);
                    nudValueX.Minimum = Int32.MinValue;
                    nudValueX.Maximum = Int32.MaxValue;
                    nudValueX.Value = (member == null) ? 0 : ((IntRange)member).Min;
                    innerPanel.Controls.Add(nudValueX);
                    IntNumericUpDown nudValueY = new IntNumericUpDown();
                    nudValueY.Dock = DockStyle.Fill;
                    nudValueY.Size = new Size(0, 21);
                    nudValueY.Minimum = Int32.MinValue;
                    nudValueY.Maximum = Int32.MaxValue;
                    nudValueY.Value = (member == null) ? 0 : ((IntRange)member).Max;
                    innerPanel.Controls.Add(nudValueY);

                    control.Controls.Add(innerPanel);
                    innerPanel.ResumeLayout(false);
                }
                else if (type == typeof(TileLayer))
                {
                    LoadLabelControl(control, name);

                    TilePreview preview = new TilePreview();
                    preview.Dock = DockStyle.Fill;
                    preview.Size = new Size(GraphicsManager.TileSize, GraphicsManager.TileSize);
                    preview.SetChosenAnim((TileLayer)member);
                    control.Controls.Add(preview);
                    preview.TileClick += (object sender, EventArgs e) =>
                    {
                        ElementForm frmData = new ElementForm();
                        frmData.Text = name + "/" + "Tile";

                        Rectangle boxRect = new Rectangle(new Point(), new Size(654, 502 + LABEL_HEIGHT));
                        int box_down = 0;
                        LoadLabelControl(frmData.ControlPanel, name);
                        box_down += 16;
                        //for enums, use a combobox
                        TileBrowser browser = new TileBrowser();
                        browser.Location = new Point(boxRect.Left, box_down);
                        browser.Size = new Size(boxRect.Width, boxRect.Height);
                        browser.SetBrush(preview.GetChosenAnim());
                        frmData.ControlPanel.Controls.Add(browser);

                        frmData.DisableClipboard();

                        if (frmData.ShowDialog() == DialogResult.OK)
                            preview.SetChosenAnim(browser.GetBrush());
                    };
                }
                else if (type.GetInterfaces().Contains(typeof(IList<TileLayer>)))
                {
                    LoadLabelControl(control, name);

                    TilePreview preview = new TilePreview();
                    preview.Dock = DockStyle.Fill;
                    preview.Size = new Size(GraphicsManager.TileSize, GraphicsManager.TileSize);
                    preview.SetChosenAnim(((IList<TileLayer>)member).Count > 0 ? ((IList<TileLayer>)member)[0] : new TileLayer());
                    control.Controls.Add(preview);

                    CollectionBox lbxValue = new CollectionBox();
                    lbxValue.Dock = DockStyle.Fill;
                    lbxValue.Size = new Size(0, 175);
                    lbxValue.LoadFromList(type, (IList)member);
                    control.Controls.Add(lbxValue);

                    lbxValue.SelectedIndexChanged += (object sender, EventArgs e) =>
                    {
                        if (lbxValue.SelectedIndex > -1)
                            preview.SetChosenAnim(((IList<TileLayer>)lbxValue.Collection)[lbxValue.SelectedIndex]);
                        else
                            preview.SetChosenAnim(((IList<TileLayer>)lbxValue.Collection).Count > 0 ? ((IList<TileLayer>)lbxValue.Collection)[0] : new TileLayer());
                    };


                    //add lambda expression for editing a single element
                    lbxValue.OnEditItem = (int index, object element, CollectionBox.EditElementOp op) =>
                    {
                        ElementForm frmData = new ElementForm();
                        frmData.Text = name + "/" + "Tile #" + index;

                        Rectangle boxRect = new Rectangle(new Point(), new Size(654, 502 + LABEL_HEIGHT));
                        int box_down = 0;
                        LoadLabelControl(frmData.ControlPanel, name);
                        box_down += 16;
                        //for enums, use a combobox
                        TileBrowser browser = new TileBrowser();
                        browser.Location = new Point(boxRect.Left, box_down);
                        browser.Size = new Size(boxRect.Width, boxRect.Height);
                        browser.SetBrush(element != null ? (TileLayer)element : new TileLayer());

                        frmData.DisableClipboard();

                        frmData.OnOK += (object okSender, EventArgs okE) =>
                        {
                            element = browser.GetBrush();
                            frmData.Close();
                        };
                        frmData.OnCancel += (object okSender, EventArgs okE) =>
                        {
                            frmData.Close();
                        };

                        frmData.ControlPanel.Controls.Add(browser);

                        frmData.Show();
                    };

                }
                else if (type.Equals(typeof(Type)))
                {
                    TypeConstraintAttribute dataAtt = ReflectionExt.FindAttribute<TypeConstraintAttribute>(attributes);
                    Type baseType = dataAtt.BaseClass;

                    Type[] children = baseType.GetAssignableTypes();

                    TableLayoutPanel sharedRowPanel = new TableLayoutPanel();
                    sharedRowPanel.SuspendLayout();

                    sharedRowPanel.AutoSize = true;
                    sharedRowPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
                    sharedRowPanel.ColumnCount = 2;
                    sharedRowPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
                    sharedRowPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
                    sharedRowPanel.Dock = DockStyle.Fill;
                    sharedRowPanel.Padding = new Padding(0, 0, 0, 0);
                    sharedRowPanel.Margin = new Padding(0, 0, 0, 0);
                    sharedRowPanel.RowCount = 1;
                    sharedRowPanel.RowStyles.Add(new RowStyle());

                    Label lblType = new Label();
                    lblType.Dock = DockStyle.Fill;
                    lblType.Size = new Size(36, 13);
                    lblType.Text = "Type: ";
                    lblType.TextAlign = ContentAlignment.MiddleLeft;
                    sharedRowPanel.Controls.Add(lblType);

                    ComboBox cbValue = new ComboBox();
                    cbValue.Dock = DockStyle.Fill;
                    cbValue.Size = new Size(0, 21);
                    cbValue.DropDownStyle = ComboBoxStyle.DropDownList;
                    cbValue.FormattingEnabled = true;
                    sharedRowPanel.Controls.Add(cbValue);

                    for (int ii = 0; ii < children.Length; ii++)
                    {
                        Type childType = children[ii];
                        cbValue.Items.Add(childType.GetDisplayName());

                        if (childType == (Type)member)
                            cbValue.SelectedIndex = ii;
                    }
                    if (cbValue.SelectedIndex == -1)
                        cbValue.SelectedIndex = 0;

                    control.Controls.Add(sharedRowPanel);
                    sharedRowPanel.ResumeLayout(false);

                }
                else if (type.IsArray)
                {
                    //TODO: 2D array grid support
                    //if (type.GetElementType().IsArray)

                    Array array = ((Array)member);
                    List<object> objList = new List<object>();
                    for(int ii = 0; ii < array.Length; ii++)
                        objList.Add(array.GetValue(ii));

                    LoadLabelControl(control, name);

                    CollectionBox lbxValue = new CollectionBox();
                    lbxValue.Dock = DockStyle.Fill;
                    lbxValue.Size = new Size(0, 150);
                    lbxValue.LoadFromList(objList.GetType(), objList);
                    control.Controls.Add(lbxValue);

                    Type elementType = type.GetElementType();
                    //add lambda expression for editing a single element
                    lbxValue.OnEditItem = (int index, object element, CollectionBox.EditElementOp op) =>
                    {
                        ElementForm frmData = new ElementForm();
                        if (element == null)
                            frmData.Text = name + "/" + "New " + elementType.Name;
                        else
                            frmData.Text = name + "/" + element.ToString();

                        StaticLoadMemberControl(frmData.ControlPanel, "(Array) " + name + "[" + index + "]", elementType, ReflectionExt.GetPassableAttributes(0, attributes), element, true);

                        frmData.SetObjectName(elementType.Name);
                        frmData.OnCopy += (object copySender, EventArgs copyE) => {
                            object obj = null;
                            StaticSaveMemberControl(frmData.ControlPanel, name, elementType, ReflectionExt.GetPassableAttributes(0, attributes), ref obj, true);
                            Clipboard.SetDataObject(obj);
                        };
                        frmData.OnPaste += (object copySender, EventArgs copyE) => {
                            IDataObject clip = Clipboard.GetDataObject();
                            string[] formats = clip.GetFormats();
                            object clipObj = clip.GetData(formats[0]);
                            Type type1 = clipObj.GetType();
                            Type type2 = elementType;
                            if (type2.IsAssignableFrom(type1))
                            {
                                frmData.ControlPanel.Controls.Clear();
                                StaticLoadMemberControl(frmData.ControlPanel, "(Array) " + name + "[" + index + "]", elementType, ReflectionExt.GetPassableAttributes(0, attributes), clipObj, true);
                            }
                            else
                                MessageBox.Show(String.Format("Incompatible types:\n{0}\n{1}", type1.AssemblyQualifiedName, type2.AssemblyQualifiedName), "Invalid Operation", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        };

                        frmData.OnOK += (object okSender, EventArgs okE) =>
                        {
                            StaticSaveMemberControl(frmData.ControlPanel, name, elementType, ReflectionExt.GetPassableAttributes(0, attributes), ref element, true);
                            op(index, element);
                            frmData.Close();
                        };
                        frmData.OnCancel += (object okSender, EventArgs okE) =>
                        {
                            frmData.Close();
                        };

                        frmData.Show();
                    };

                }
                else if (type.GetInterfaces().Contains(typeof(IList)))
                {
                    LoadLabelControl(control, name);

                    CollectionBox lbxValue = new CollectionBox();
                    lbxValue.Dock = DockStyle.Fill;
                    lbxValue.Size = new Size(0, 150);
                    lbxValue.LoadFromList(type, (IList)member);
                    control.Controls.Add(lbxValue);

                    Type elementType = ReflectionExt.GetBaseTypeArg(typeof(IList<>), type, 0);
                    //add lambda expression for editing a single element
                    lbxValue.OnEditItem = (int index, object element, CollectionBox.EditElementOp op) =>
                    {
                        ElementForm frmData = new ElementForm();
                        if (element == null)
                            frmData.Text = name + "/" + "New " + elementType.Name;
                        else
                            frmData.Text = name + "/" + element.ToString();

                        StaticLoadMemberControl(frmData.ControlPanel, "(List) " + name + "[" + index + "]", elementType, ReflectionExt.GetPassableAttributes(1, attributes), element, true);

                        frmData.SetObjectName(elementType.Name);
                        frmData.OnCopy += (object copySender, EventArgs copyE) => {
                            object obj = null;
                            StaticSaveMemberControl(frmData.ControlPanel, name, elementType, ReflectionExt.GetPassableAttributes(1, attributes), ref obj, true);
                            Clipboard.SetDataObject(obj);
                        };
                        frmData.OnPaste += (object copySender, EventArgs copyE) => {
                            IDataObject clip = Clipboard.GetDataObject();
                            string[] formats = clip.GetFormats();
                            object clipObj = clip.GetData(formats[0]);
                            Type type1 = clipObj.GetType();
                            Type type2 = elementType;
                            if (type2.IsAssignableFrom(type1))
                            {
                                frmData.ControlPanel.Controls.Clear();
                                StaticLoadMemberControl(frmData.ControlPanel, "(List) " + name + "[" + index + "]", elementType, ReflectionExt.GetPassableAttributes(1, attributes), clipObj, true);
                            }
                            else
                                MessageBox.Show(String.Format("Incompatible types:\n{0}\n{1}", type1.AssemblyQualifiedName, type2.AssemblyQualifiedName), "Invalid Operation", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        };

                        frmData.OnOK += (object okSender, EventArgs okE) =>
                        {
                            StaticSaveMemberControl(frmData.ControlPanel, name, elementType, ReflectionExt.GetPassableAttributes(1, attributes), ref element, true);
                            op(index, element);
                            frmData.Close();
                        };
                        frmData.OnCancel += (object okSender, EventArgs okE) =>
                        {
                            frmData.Close();
                        };

                        frmData.Show();
                    };
                }
                else if (type.GetInterfaces().Contains(typeof(IDictionary)))
                {
                    LoadLabelControl(control, name);

                    DictionaryBox lbxValue = new DictionaryBox();
                    lbxValue.Dock = DockStyle.Fill;
                    lbxValue.Size = new Size(0, 150);
                    lbxValue.LoadFromDictionary(type, (IDictionary)member);
                    control.Controls.Add(lbxValue);

                    Type keyType = ReflectionExt.GetBaseTypeArg(typeof(IDictionary<,>), type, 0);
                    Type elementType = ReflectionExt.GetBaseTypeArg(typeof(IDictionary<,>), type, 1);

                    //add lambda expression for editing a single element
                    lbxValue.OnEditItem = (object key, object element, DictionaryBox.EditElementOp op) =>
                    {

                        ElementForm frmData = new ElementForm();
                        if (element == null)
                            frmData.Text = name + "/" + "New " + elementType.Name;
                        else
                            frmData.Text = name + "/" + element.ToString();

                        StaticLoadMemberControl(frmData.ControlPanel, "(Dict) " + name + "[" + key.ToString() + "]", elementType, ReflectionExt.GetPassableAttributes(2, attributes), element, true);

                        frmData.SetObjectName(elementType.Name);
                        frmData.OnCopy += (object copySender, EventArgs copyE) => {
                            object obj = null;
                            StaticSaveMemberControl(frmData.ControlPanel, name, elementType, ReflectionExt.GetPassableAttributes(2, attributes), ref obj, true);
                            Clipboard.SetDataObject(obj);
                        };
                        frmData.OnPaste += (object copySender, EventArgs copyE) => {
                            IDataObject clip = Clipboard.GetDataObject();
                            string[] formats = clip.GetFormats();
                            object clipObj = clip.GetData(formats[0]);
                            Type type1 = clipObj.GetType();
                            Type type2 = elementType;
                            if (type2.IsAssignableFrom(type1))
                            {
                                frmData.ControlPanel.Controls.Clear();
                                StaticLoadMemberControl(frmData.ControlPanel, "(Dict) " + name + "[" + key.ToString() + "]", elementType, ReflectionExt.GetPassableAttributes(2, attributes), clipObj, true);
                            }
                            else
                                MessageBox.Show(String.Format("Incompatible types:\n{0}\n{1}", type1.AssemblyQualifiedName, type2.AssemblyQualifiedName), "Invalid Operation", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        };

                        frmData.OnOK += (object okSender, EventArgs okE) =>
                        {
                            StaticSaveMemberControl(frmData.ControlPanel, name, elementType, ReflectionExt.GetPassableAttributes(2, attributes), ref element, true);
                            op(key, element);
                            frmData.Close();
                        };
                        frmData.OnCancel += (object okSender, EventArgs okE) =>
                        {
                            frmData.Close();
                        };

                        frmData.Show();
                    };

                    lbxValue.OnEditKey = (object key, object element, DictionaryBox.EditElementOp op) =>
                    {
                        ElementForm frmKey = new ElementForm();
                        if (element == null)
                            frmKey.Text = name + "/" + "New Key:" + keyType.Name;
                        else
                            frmKey.Text = name + "/" + element.ToString();

                        StaticLoadMemberControl(frmKey.ControlPanel, "(Dict) " + name + "<New Key>", keyType, new object[0] { }, null, true);

                        frmKey.SetObjectName(keyType.Name);
                        frmKey.OnCopy += (object copySender, EventArgs copyE) => {
                            object obj = null;
                            StaticSaveMemberControl(frmKey.ControlPanel, name, keyType, new object[0] { }, ref obj, true);
                            Clipboard.SetDataObject(obj);
                        };
                        frmKey.OnPaste += (object copySender, EventArgs copyE) => {
                            IDataObject clip = Clipboard.GetDataObject();
                            string[] formats = clip.GetFormats();
                            object clipObj = clip.GetData(formats[0]);
                            Type type1 = clipObj.GetType();
                            Type type2 = keyType;
                            if (type2.IsAssignableFrom(type1))
                            {
                                frmKey.ControlPanel.Controls.Clear();
                                StaticLoadMemberControl(frmKey.ControlPanel, "(Dict) " + name + "<New Key>", keyType, new object[0] { }, clipObj, true);
                            }
                            else
                                MessageBox.Show(String.Format("Incompatible types:\n{0}\n{1}", type1.AssemblyQualifiedName, type2.AssemblyQualifiedName), "Invalid Operation", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        };

                        frmKey.OnOK += (object okSender, EventArgs okE) =>
                        {
                            StaticSaveMemberControl(frmKey.ControlPanel, name, keyType, new object[0] { }, ref key, true);
                            op(key, element);
                            frmKey.Close();
                        };
                        frmKey.OnCancel += (object okSender, EventArgs okE) =>
                        {
                            frmKey.Close();
                        };

                        frmKey.Show();
                    };
                }
                else if (type.GetInterfaces().Contains(typeof(IPriorityList)))
                {
                    LoadLabelControl(control, name);

                    PriorityListBox lbxValue = new PriorityListBox();
                    lbxValue.Dock = DockStyle.Fill;
                    lbxValue.Size = new Size(0, 150);
                    lbxValue.LoadFromList(type, (IPriorityList)member);
                    control.Controls.Add(lbxValue);

                    Type elementType = ReflectionExt.GetBaseTypeArg(typeof(IPriorityList<>), type, 0);
                    //add lambda expression for editing a single element
                    lbxValue.OnEditItem = (int priority, int index, object element, PriorityListBox.EditElementOp op) =>
                    {
                        ElementForm frmData = new ElementForm();
                        if (element == null)
                            frmData.Text = name + "/" + "New " + elementType.Name;
                        else
                            frmData.Text = name + "/" + element.ToString();

                        StaticLoadMemberControl(frmData.ControlPanel, "(PriorityList) " + name + "[" + index + "]", elementType, ReflectionExt.GetPassableAttributes(2, attributes), element, true);

                        frmData.SetObjectName(elementType.Name);
                        frmData.OnCopy += (object copySender, EventArgs copyE) => {
                            object obj = null;
                            StaticSaveMemberControl(frmData.ControlPanel, name, elementType, ReflectionExt.GetPassableAttributes(2, attributes), ref obj, true);
                            Clipboard.SetDataObject(obj);
                        };
                        frmData.OnPaste += (object copySender, EventArgs copyE) => {
                            IDataObject clip = Clipboard.GetDataObject();
                            string[] formats = clip.GetFormats();
                            object clipObj = clip.GetData(formats[0]);
                            Type type1 = clipObj.GetType();
                            Type type2 = elementType;
                            if (type2.IsAssignableFrom(type1))
                            {
                                frmData.ControlPanel.Controls.Clear();
                                StaticLoadMemberControl(frmData.ControlPanel, "(PriorityList) " + name + "[" + index + "]", elementType, ReflectionExt.GetPassableAttributes(2, attributes), clipObj, true);
                            }
                            else
                                MessageBox.Show(String.Format("Incompatible types:\n{0}\n{1}", type1.AssemblyQualifiedName, type2.AssemblyQualifiedName), "Invalid Operation", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        };

                        frmData.OnOK += (object okSender, EventArgs okE) =>
                        {
                            StaticSaveMemberControl(frmData.ControlPanel, name, elementType, ReflectionExt.GetPassableAttributes(2, attributes), ref element, true);
                            op(priority, index, element);
                            frmData.Close();
                        };
                        frmData.OnCancel += (object okSender, EventArgs okE) =>
                        {
                            frmData.Close();
                        };

                        frmData.Show();
                    };
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
                    cbxValue.Dock = DockStyle.Fill;
                    MultilineAttribute attribute = ReflectionExt.FindAttribute<MultilineAttribute>(attributes);
                    if (attribute != null)
                        cbxValue.Size = new Size(0, 80);
                    else
                        cbxValue.Size = new Size(0, 29);
                    cbxValue.LoadFromSource(member);
                    control.Controls.Add(cbxValue);

                    //add lambda expression for editing a single element
                    cbxValue.OnEditItem = (object element, ClassBox.EditElementOp op) =>
                    {
                        ElementForm frmData = new ElementForm();
                        frmData.Text = name + "/" + type.Name;

                        StaticLoadMemberControl(frmData.ControlPanel, name, type, ReflectionExt.GetPassableAttributes(0, attributes), element, true);

                        frmData.SetObjectName(type.Name);
                        frmData.OnCopy += (object copySender, EventArgs copyE) => {
                            object obj = null;
                            StaticSaveMemberControl(frmData.ControlPanel, name, type, ReflectionExt.GetPassableAttributes(0, attributes), ref obj, true);
                            Clipboard.SetDataObject(obj);
                        };
                        frmData.OnPaste += (object copySender, EventArgs copyE) => {
                            IDataObject clip = Clipboard.GetDataObject();
                            string[] formats = clip.GetFormats();
                            object clipObj = clip.GetData(formats[0]);
                            Type type1 = clipObj.GetType();
                            Type type2 = type;
                            if (type2.IsAssignableFrom(type1))
                            {
                                frmData.ControlPanel.Controls.Clear();
                                StaticLoadMemberControl(frmData.ControlPanel, name, type, ReflectionExt.GetPassableAttributes(0, attributes), clipObj, true);
                            }
                            else
                                MessageBox.Show(String.Format("Incompatible types:\n{0}\n{1}", type1.AssemblyQualifiedName, type2.AssemblyQualifiedName), "Invalid Operation", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        };

                        frmData.OnOK += (object okSender, EventArgs okE) =>
                        {
                            StaticSaveMemberControl(frmData.ControlPanel, name, type, ReflectionExt.GetPassableAttributes(0, attributes), ref element, true);
                            op(element);
                            frmData.Close();
                        };
                        frmData.OnCancel += (object okSender, EventArgs okE) =>
                        {
                            frmData.Close();
                        };

                        frmData.Show();
                    };
                }
                else
                {
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
                        GroupBox groupBox = new GroupBox();
                        groupBox.SuspendLayout();

                        groupBox.AutoSize = true;
                        groupBox.AutoSizeMode = AutoSizeMode.GrowAndShrink;
                        groupBox.Dock = DockStyle.Fill;
                        groupBox.Text = name;

                        TableLayoutPanel groupBoxPanel = new TableLayoutPanel();
                        groupBoxPanel.AutoSize = true;
                        groupBoxPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
                        groupBoxPanel.ColumnCount = 1;
                        groupBoxPanel.ColumnStyles.Add(new ColumnStyle());
                        groupBoxPanel.Dock = DockStyle.Fill;
                        groupBoxPanel.Padding = new Padding(0, 0, 0, 0);
                        groupBoxPanel.Margin = new Padding(0, 0, 0, 0);
                        groupBoxPanel.RowCount = 1;
                        groupBoxPanel.RowStyles.Add(new RowStyle());

                        //var copyPasteStrip = new System.Windows.Forms.ContextMenuStrip();
                        //copyPasteStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
                        //copyPasteStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                        //    copyToolStripMenuItem,
                        //    pasteToolStripMenuItem});
                        //copyPasteStrip.Name = "copyPasteStrip";
                        //copyPasteStrip.Size = new System.Drawing.Size(241, 101);


                        loadClassControls(member, groupBoxPanel);

                        groupBox.Controls.Add(groupBoxPanel);
                        control.Controls.Add(groupBox);
                        groupBox.ResumeLayout(false);

                    }
                    else
                    {
                        //note: considerations must be made when dealing with inheritance/polymorphism
                        //eg: find all children in this assembly that can be instantiated,
                        //add them to different panels
                        //show the one that is active right now
                        //include a combobox for switching children


                        TableLayoutPanel sharedRowPanel = new TableLayoutPanel();
                        sharedRowPanel.SuspendLayout();

                        sharedRowPanel.AutoSize = true;
                        sharedRowPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
                        sharedRowPanel.ColumnCount = 2;
                        sharedRowPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
                        sharedRowPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
                        sharedRowPanel.Dock = DockStyle.Fill;
                        sharedRowPanel.Padding = new Padding(0, 0, 0, 0);
                        sharedRowPanel.Margin = new Padding(0, 0, 0, 0);
                        sharedRowPanel.RowCount = 1;
                        sharedRowPanel.RowStyles.Add(new RowStyle());

                        Label lblType = new Label();
                        lblType.Dock = DockStyle.Fill;
                        lblType.Size = new Size(36, 13);
                        lblType.Text = "Type: ";
                        lblType.TextAlign = ContentAlignment.MiddleLeft;
                        sharedRowPanel.Controls.Add(lblType);

                        ComboBox cbValue = new ComboBox();
                        cbValue.Dock = DockStyle.Fill;
                        cbValue.Size = new Size(0, 21);
                        cbValue.DropDownStyle = ComboBoxStyle.DropDownList;
                        cbValue.FormattingEnabled = true;
                        sharedRowPanel.Controls.Add(cbValue);

                        control.Controls.Add(sharedRowPanel);
                        sharedRowPanel.ResumeLayout(false);

                        GroupBox groupBox = new GroupBox();
                        groupBox.SuspendLayout();

                        groupBox.AutoSize = true;
                        groupBox.AutoSizeMode = AutoSizeMode.GrowAndShrink;
                        groupBox.Dock = DockStyle.Fill;
                        groupBox.Text = name;


                        TableLayoutPanel groupBoxPanel = new TableLayoutPanel();
                        groupBoxPanel.AutoSize = true;
                        groupBoxPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
                        groupBoxPanel.ColumnCount = 1;
                        groupBoxPanel.ColumnStyles.Add(new ColumnStyle());
                        groupBoxPanel.Dock = DockStyle.Fill;
                        groupBoxPanel.Padding = new Padding(0, 0, 0, 0);
                        groupBoxPanel.Margin = new Padding(0, 0, 0, 0);
                        groupBoxPanel.RowCount = 1;
                        groupBoxPanel.RowStyles.Add(new RowStyle());
                        
                        loadClassControls(member, groupBoxPanel);

                        List<CreateMethod> createMethods = new List<CreateMethod>();

                        for (int ii = 0; ii < children.Length; ii++)
                        {
                            Type childType = children[ii];
                            cbValue.Items.Add(childType.GetDisplayName());

                            createMethods.Add(() =>
                            {
                                groupBoxPanel.Controls.Clear();
                                object emptyMember = ReflectionExt.CreateMinimalInstance(childType);
                                loadClassControls(emptyMember, groupBoxPanel);//TODO: POTENTIAL INFINITE RECURSION
                                //for some reason the parent control needs the following calls to prevent a strange autoscroll bug
                                control.AutoScroll = false;
                                control.AutoScroll = true;
                            });
                            if (childType == member.GetType())
                                cbValue.SelectedIndex = ii;
                        }
                        if (cbValue.SelectedIndex == -1)
                            cbValue.SelectedIndex = 0;

                        cbValue.SelectedIndexChanged += (object sender, EventArgs e) =>
                        {
                            createMethods[cbValue.SelectedIndex]();
                        };

                        groupBox.Controls.Add(groupBoxPanel);
                        control.Controls.Add(groupBox);

                        groupBox.ResumeLayout(false);
                    }
                }
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(e);
            }
        }

        public static void SaveDataControls(ref object obj, TableLayoutPanel control)
        {
            saveMemberControl(obj, control, obj.ToString(), obj.GetType(), null, ref obj, true);
        }

        private static void saveClassControls(object obj, TableLayoutPanel control)
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

        public static void StaticSaveClassControls(object obj, TableLayoutPanel control)
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

                        saveMemberControl(obj, (TableLayoutPanel)control.Controls[controlIndex], myInfo.Name, myInfo.GetMemberInfoType(), myInfo.GetCustomAttributes(false), ref member, false);
                        myInfo.SetValue(obj, member);
                        controlIndex++;
                    }
                    else
                    {
                        Control sharedRowControl = control.Controls[controlIndex];
                        int sharedRowControlIndex = 0;
                        for (int jj = 0; jj < tieredFields[ii].Count; jj++)
                        {
                            MemberInfo myInfo = tieredFields[ii][jj];
                            object member = myInfo.GetValue(obj);

                            saveMemberControl(obj, (TableLayoutPanel)sharedRowControl.Controls[jj], myInfo.Name, myInfo.GetMemberInfoType(), myInfo.GetCustomAttributes(false), ref member, false);
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



        private static void saveMemberControl(object obj, TableLayoutPanel control, string name, Type type, object[] attributes, ref object member, bool isWindow)
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

        public static void StaticSaveMemberControl(TableLayoutPanel control, string name, Type type, object[] attributes, ref object member, bool isWindow)
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
                        TableLayoutPanel innerControl = (TableLayoutPanel)control.Controls[controlIndex];
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
                                CheckBox chkValue = (CheckBox)innerControl.Controls[innerControlIndex];
                                pending |= (chkValue.Checked ? 1 : 0) * (int)enums.GetValue(ii);
                                innerControlIndex++;
                            }
                        }
                        member = Enum.ToObject(type, pending);
                    }
                    else
                    {
                        ComboBox cbValue = (ComboBox)control.Controls[controlIndex];
                        Array array = Enum.GetValues(type);
                        member = array.GetValue(cbValue.SelectedIndex);
                        controlIndex++;
                    }
                }
                else if (type == typeof(Boolean))
                {
                    CheckBox chkValue = (CheckBox)control.Controls[controlIndex];
                    member = chkValue.Checked;
                    controlIndex++;
                }
                else if (type == typeof(Int32))
                {
                    controlIndex++;
                    DataTypeAttribute dataAtt = ReflectionExt.FindAttribute<DataTypeAttribute>(attributes);
                    FrameTypeAttribute frameAtt = ReflectionExt.FindAttribute<FrameTypeAttribute>(attributes);
                    if (dataAtt != null)
                    {
                        ComboBox cbValue = (ComboBox)control.Controls[controlIndex];
                        int returnValue = cbValue.SelectedIndex;
                        if (dataAtt.IncludeInvalid)
                            returnValue--;
                        member = returnValue;
                    }
                    else if (frameAtt != null)
                    {
                        ComboBox cbValue = (ComboBox)control.Controls[controlIndex];
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
                        IntNumericUpDown nudValue = (IntNumericUpDown)control.Controls[controlIndex];
                        member = (Int32)nudValue.Value;
                    }
                    controlIndex++;
                }
                else if (type == typeof(byte))
                {
                    controlIndex++;
                    IntNumericUpDown nudValue = (IntNumericUpDown)control.Controls[controlIndex];
                    member = (byte)nudValue.Value;
                    controlIndex++;
                }
                else if (type == typeof(Single))
                {
                    controlIndex++;
                    NumericUpDown nudValue = (NumericUpDown)control.Controls[controlIndex];
                    member = (Single)nudValue.Value;
                    controlIndex++;
                }
                else if (type == typeof(Double))
                {
                    controlIndex++;
                    NumericUpDown nudValue = (NumericUpDown)control.Controls[controlIndex];
                    member = (Double)nudValue.Value;
                    controlIndex++;
                }
                else if (type == typeof(FlagType))
                {
                    StringTypeConstraintAttribute dataAtt = ReflectionExt.FindAttribute<StringTypeConstraintAttribute>(attributes);
                    if (dataAtt != null)
                    {
                        Type baseType = dataAtt.BaseClass;

                        Type[] children = baseType.GetAssignableTypes();

                        ComboBox cbValue = (ComboBox)control.Controls[controlIndex].Controls[1];
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
                        ComboBox cbValue = (ComboBox)control.Controls[controlIndex];
                        if (cbValue.SelectedIndex == 0)
                            member = "";
                        else
                            member = (string)cbValue.SelectedItem;
                    }
                    else
                    {
                        TextBox txtValue = (TextBox)control.Controls[controlIndex];
                        member = (String)txtValue.Text;
                    }
                    controlIndex++;
                }
                else if (type == typeof(Microsoft.Xna.Framework.Color))
                {
                    controlIndex++;
                    TableLayoutPanel innerControl = (TableLayoutPanel)control.Controls[controlIndex];
                    int innerControlIndex = 0;

                    innerControlIndex++;
                    innerControlIndex++;
                    innerControlIndex++;
                    innerControlIndex++;
                    IntNumericUpDown nudValueR = (IntNumericUpDown)innerControl.Controls[innerControlIndex];
                    innerControlIndex++;
                    IntNumericUpDown nudValueG = (IntNumericUpDown)innerControl.Controls[innerControlIndex];
                    innerControlIndex++;
                    IntNumericUpDown nudValueB = (IntNumericUpDown)innerControl.Controls[innerControlIndex];
                    innerControlIndex++;
                    IntNumericUpDown nudValueA = (IntNumericUpDown)innerControl.Controls[innerControlIndex];
                    member = new Microsoft.Xna.Framework.Color((int)nudValueR.Value, (int)nudValueG.Value, (int)nudValueB.Value, (int)nudValueA.Value);
                    innerControlIndex++;
                }
                else if (type == typeof(Loc))
                {
                    controlIndex++;
                    TableLayoutPanel innerControl = (TableLayoutPanel)control.Controls[controlIndex];
                    int innerControlIndex = 0;

                    innerControlIndex++;
                    innerControlIndex++;
                    IntNumericUpDown nudValueX = (IntNumericUpDown)innerControl.Controls[innerControlIndex];
                    innerControlIndex++;
                    IntNumericUpDown nudValueY = (IntNumericUpDown)innerControl.Controls[innerControlIndex];
                    member = new Loc((int)nudValueX.Value, (int)nudValueY.Value);
                    innerControlIndex++;
                }
                else if (type == typeof(SegLoc))
                {
                    controlIndex++;
                    TableLayoutPanel innerControl = (TableLayoutPanel)control.Controls[controlIndex];
                    int innerControlIndex = 0;

                    innerControlIndex++;
                    innerControlIndex++;
                    IntNumericUpDown nudValueX = (IntNumericUpDown)innerControl.Controls[innerControlIndex];
                    innerControlIndex++;
                    IntNumericUpDown nudValueY = (IntNumericUpDown)innerControl.Controls[innerControlIndex];
                    member = new SegLoc((int)nudValueX.Value, (int)nudValueY.Value);
                    innerControlIndex++;
                }
                else if (type == typeof(IntRange))
                {
                    controlIndex++;
                    TableLayoutPanel innerControl = (TableLayoutPanel)control.Controls[controlIndex];
                    int innerControlIndex = 0;

                    innerControlIndex++;
                    innerControlIndex++;
                    IntNumericUpDown nudValueX = (IntNumericUpDown)innerControl.Controls[innerControlIndex];
                    innerControlIndex++;
                    IntNumericUpDown nudValueY = (IntNumericUpDown)innerControl.Controls[innerControlIndex];
                    member = new IntRange((int)nudValueX.Value, (int)nudValueY.Value);
                    innerControlIndex++;
                }
                else if (type == typeof(TileLayer))
                {
                    controlIndex++;
                    TilePreview preview = (TilePreview)control.Controls[controlIndex];
                    member = preview.GetChosenAnim();
                    controlIndex++;
                }
                else if (type.GetInterfaces().Contains(typeof(IList<TileLayer>)))
                {
                    controlIndex++;
                    controlIndex++;
                    CollectionBox lbxValue = (CollectionBox)control.Controls[controlIndex];
                    member = lbxValue.Collection;
                    controlIndex++;
                }
                else if (type.Equals(typeof(Type)))
                {
                    TypeConstraintAttribute dataAtt = ReflectionExt.FindAttribute<TypeConstraintAttribute>(attributes);
                    Type baseType = dataAtt.BaseClass;

                    Type[] children = baseType.GetAssignableTypes();

                    ComboBox cbValue = (ComboBox)control.Controls[controlIndex].Controls[1];
                    member = children[cbValue.SelectedIndex];
                    controlIndex++;
                }
                else if (type.IsArray)
                {
                    //TODO: 2D array grid support
                    //if (type.GetElementType().IsArray)

                    controlIndex++;
                    CollectionBox lbxValue = (CollectionBox)control.Controls[controlIndex];
                    Array array = Array.CreateInstance(type.GetElementType(), lbxValue.Collection.Count);
                    for (int ii = 0; ii < lbxValue.Collection.Count; ii++)
                        array.SetValue(lbxValue.Collection[ii], ii);
                    member = array;
                    controlIndex++;

                }
                else if (type.GetInterfaces().Contains(typeof(IList)))
                {
                    controlIndex++;
                    CollectionBox lbxValue = (CollectionBox)control.Controls[controlIndex];
                    member = lbxValue.Collection;
                    controlIndex++;
                }
                else if (type.GetInterfaces().Contains(typeof(IDictionary)))
                {
                    controlIndex++;
                    DictionaryBox lbxValue = (DictionaryBox)control.Controls[controlIndex];
                    member = lbxValue.Dictionary;
                    controlIndex++;
                }
                else if (type.GetInterfaces().Contains(typeof(IPriorityList)))
                {
                    controlIndex++;
                    PriorityListBox lbxValue = (PriorityListBox)control.Controls[controlIndex];
                    member = lbxValue.Collection;
                    controlIndex++;
                }
                else if (!isWindow && ReflectionExt.FindAttribute<SubGroupAttribute>(attributes) == null)
                {
                    controlIndex++;
                    ClassBox cbxValue = (ClassBox)control.Controls[controlIndex];
                    member = cbxValue.Object;
                    controlIndex++;
                }
                else
                {
                    Type[] children = type.GetAssignableTypes();

                    //need to create a new instance
                    //note: considerations must be made when dealing with inheritance/polymorphism
                    //eg: check to see if there are children of the type,
                    //and if so, do this instead:
                    //get the combobox index determining the type
                    //instantiate the type
                    //get the panel for the index
                    //save using THAT panel

                    if (children.Length > 1)
                    {
                        ComboBox cbValue = (ComboBox)control.Controls[controlIndex].Controls[1];
                        member = ReflectionExt.CreateMinimalInstance(children[cbValue.SelectedIndex]);
                        controlIndex++;
                    }
                    else
                        member = ReflectionExt.CreateMinimalInstance(children[0]);

                    GroupBox groupBox = (GroupBox)control.Controls[controlIndex];
                    saveClassControls(member, (TableLayoutPanel)groupBox.Controls[0]);
                    controlIndex++;
                }
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(e);
            }
        }

        private static void CbValue_PlaySound(object sender, EventArgs e)
        {
            ComboBox box = (ComboBox)sender;
            if (box.SelectedIndex > 0)
                GameManager.Instance.BattleSE((string)box.SelectedItem);
        }


    }
}

