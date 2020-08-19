using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if EDITORS
using System.Windows.Forms;
#endif
using System.Reflection;
using System.Drawing;
using RogueEssence.Content;
using RogueEssence.Dungeon;
using RogueElements;
using System.IO;

namespace RogueEssence.Dev
{
    [Serializable]
    public abstract class EditorData
    {
        protected const int LABEL_HEIGHT = 14;

        public override string ToString()
        {
            return GetType().Name;
        }

#if EDITORS


        public static void LoadDataControls(object obj, TableLayoutPanel control)
        {
            loadMemberControl(obj, control, obj.ToString(), obj.GetType(), null, obj, true);
        }

        private static void loadClassControls(object obj, TableLayoutPanel control)
        {
            if (obj.GetType().IsSubclassOf(typeof(EditorData)))
                ((EditorData)obj).LoadClassControls(control);
            else
                staticLoadClassControls(obj, control);
        }

        protected virtual void LoadClassControls(TableLayoutPanel control)
        {
            staticLoadClassControls(this, control);
        }

        private static void staticLoadClassControls(object obj, TableLayoutPanel control)
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

                        for (int jj = 0; jj < tieredFields[ii].Count; jj++)
                            staticLoadClassControl(obj, sharedRowPanel, tieredFields[ii][jj]);
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
     
        protected static void loadLabelControl(TableLayoutPanel control, string name)
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


        protected static void loadMemberControl(object obj, TableLayoutPanel control, string name, Type type, object[] attributes, object member, bool isWindow)
        {
            if (obj.GetType().IsSubclassOf(typeof(EditorData)))
                ((EditorData)obj).LoadMemberControl(control, name, type, attributes, member, isWindow);
            else
                staticLoadMemberControl(control, name, type, attributes, member, isWindow);
        }

        protected virtual void LoadMemberControl(TableLayoutPanel control, string name, Type type, object[] attributes, object member, bool isWindow)
        {
            staticLoadMemberControl(control, name, type, attributes, member, isWindow);
        }

        //overload this method in children to account for structs such as loc
        protected static void staticLoadMemberControl(TableLayoutPanel control, string name, Type type, object[] attributes, object member, bool isWindow)
        {

            //members are set when control values are changed?
            try
            {
                if (type.IsEnum)
                {
                    loadLabelControl(control, name);


                    Array enums = type.GetEnumValues();
                    if (type.GetCustomAttributes(typeof(FlagsAttribute), false).Length > 0)
                    {
                        TableLayoutPanel innerPanel = getSharedRowPanel(2);
                        control.Controls.Add(innerPanel);

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
                    loadLabelControl(control, name);

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
                    loadLabelControl(control, name);

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
                    loadLabelControl(control, name);

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
                    loadLabelControl(control, name);

                    NumericUpDown nudValue = new NumericUpDown();
                    nudValue.Dock = DockStyle.Fill;
                    nudValue.Size = new Size(0, 21);
                    nudValue.DecimalPlaces = 3;
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
                    loadLabelControl(control, name);

                    NumericUpDown nudValue = new NumericUpDown();
                    nudValue.Dock = DockStyle.Fill;
                    nudValue.Size = new Size(0, 21);
                    nudValue.DecimalPlaces = 3;
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
                        control.Controls.Add(sharedRowPanel);

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

                            if (childType.FullName == ((FlagType)member).Type && childType.Assembly.FullName == ((FlagType)member).Assembly)
                                cbValue.SelectedIndex = ii;
                        }
                        if (cbValue.SelectedIndex == -1)
                            cbValue.SelectedIndex = 0;
                    }
                }
                else if (type == typeof(String))
                {
                    loadLabelControl(control, name);

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
                    loadLabelControl(control, name);

                    TableLayoutPanel innerPanel = getSharedRowPanel(4);
                    control.Controls.Add(innerPanel);

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
                }
                else if (type == typeof(Loc))
                {
                    loadLabelControl(control, name);

                    TableLayoutPanel innerPanel = getSharedRowPanel(2);
                    control.Controls.Add(innerPanel);

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
                }
                else if (type == typeof(SegLoc))
                {
                    loadLabelControl(control, name);

                    TableLayoutPanel innerPanel = getSharedRowPanel(2);
                    control.Controls.Add(innerPanel);

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

                }
                else if (type == typeof(IntRange))
                {
                    loadLabelControl(control, name);

                    TableLayoutPanel innerPanel = getSharedRowPanel(2);
                    control.Controls.Add(innerPanel);

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

                }
                else if (type == typeof(TileLayer))
                {
                    loadLabelControl(control, name);

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
                        loadLabelControl(frmData.ControlPanel, name);
                        box_down += 16;
                        //for enums, use a combobox
                        TileBrowser browser = new TileBrowser();
                        browser.Location = new Point(boxRect.Left, box_down);
                        browser.Size = new Size(boxRect.Width, boxRect.Height);
                        browser.SetBrush(preview.GetChosenAnim());
                        frmData.ControlPanel.Controls.Add(browser);

                        if (frmData.ShowDialog() == DialogResult.OK)
                            preview.SetChosenAnim(browser.GetBrush());
                    };
                }
                else if (type.GetInterfaces().Contains(typeof(IList<TileLayer>)))
                {
                    loadLabelControl(control, name);

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

                    Type elementType = type.GetGenericArguments()[0];
                    //add lambda expression for editing a single element
                    lbxValue.OnEditItem = (int index, object element, CollectionBox.EditElementOp op) =>
                    {
                        ElementForm frmData = new ElementForm();
                        frmData.Text = name + "/" + "Tile #" + index;

                        Rectangle boxRect = new Rectangle(new Point(), new Size(654, 502 + LABEL_HEIGHT));
                        int box_down = 0;
                        loadLabelControl(frmData.ControlPanel, name);
                        box_down += 16;
                        //for enums, use a combobox
                        TileBrowser browser = new TileBrowser();
                        browser.Location = new Point(boxRect.Left, box_down);
                        browser.Size = new Size(boxRect.Width, boxRect.Height);
                        browser.SetBrush(element != null ? (TileLayer)element : new TileLayer());
                        frmData.ControlPanel.Controls.Add(browser);


                        frmData.OnOK += (object okSender, EventArgs okE) =>
                        {
                            element = browser.GetBrush();
                            frmData.Close();
                        };
                        frmData.OnCancel += (object okSender, EventArgs okE) =>
                        {
                            frmData.Close();
                        };

                        frmData.Show();
                    };

                }
                else if (type.Equals(typeof(Type)))
                {
                    TypeConstraintAttribute dataAtt = ReflectionExt.FindAttribute<TypeConstraintAttribute>(attributes);
                    Type baseType = dataAtt.BaseClass;

                    Type[] children = baseType.GetAssignableTypes();

                    TableLayoutPanel sharedRowPanel = new TableLayoutPanel();
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
                    control.Controls.Add(sharedRowPanel);

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
                }
                else if (type.IsArray)
                {
                    //TODO: 2D array grid support
                    //if (type.GetElementType().IsArray)

                    Array array = ((Array)member);
                    List<object> objList = new List<object>();
                    for(int ii = 0; ii < array.Length; ii++)
                        objList.Add(array.GetValue(ii));

                    loadLabelControl(control, name);

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

                        staticLoadMemberControl(frmData.ControlPanel, "(Array) " + name + "[" + index + "]", elementType, ReflectionExt.GetPassableAttributes(0, attributes), element, true);

                        frmData.OnOK += (object okSender, EventArgs okE) =>
                        {
                            staticSaveMemberControl(frmData.ControlPanel, name, elementType, ReflectionExt.GetPassableAttributes(0, attributes), ref element, true);
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
                    loadLabelControl(control, name);

                    CollectionBox lbxValue = new CollectionBox();
                    lbxValue.Dock = DockStyle.Fill;
                    lbxValue.Size = new Size(0, 150);
                    lbxValue.LoadFromList(type, (IList)member);
                    control.Controls.Add(lbxValue);

                    Type elementType = type.GetGenericArguments()[0];
                    //add lambda expression for editing a single element
                    lbxValue.OnEditItem = (int index, object element, CollectionBox.EditElementOp op) =>
                    {
                        ElementForm frmData = new ElementForm();
                        if (element == null)
                            frmData.Text = name + "/" + "New " + elementType.Name;
                        else
                            frmData.Text = name + "/" + element.ToString();

                        staticLoadMemberControl(frmData.ControlPanel, "(List) " + name + "[" + index + "]", elementType, ReflectionExt.GetPassableAttributes(1, attributes), element, true);

                        frmData.OnOK += (object okSender, EventArgs okE) =>
                        {
                            staticSaveMemberControl(frmData.ControlPanel, name, elementType, ReflectionExt.GetPassableAttributes(1, attributes), ref element, true);
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
                    loadLabelControl(control, name);

                    DictionaryBox lbxValue = new DictionaryBox();
                    lbxValue.Dock = DockStyle.Fill;
                    lbxValue.Size = new Size(0, 150);
                    lbxValue.LoadFromDictionary(type, (IDictionary)member);
                    control.Controls.Add(lbxValue);

                    Type keyType = type.GetGenericArguments()[0];
                    Type elementType = type.GetGenericArguments()[1];

                    //add lambda expression for editing a single element
                    lbxValue.OnEditItem = (object key, object element, DictionaryBox.EditElementOp op) =>
                    {

                        ElementForm frmData = new ElementForm();
                        if (element == null)
                            frmData.Text = name + "/" + "New " + elementType.Name;
                        else
                            frmData.Text = name + "/" + element.ToString();

                        staticLoadMemberControl(frmData.ControlPanel, "(Dict) " + name + "[" + key.ToString() + "]", elementType, ReflectionExt.GetPassableAttributes(2, attributes), element, true);


                        frmData.OnOK += (object okSender, EventArgs okE) =>
                        {
                            staticSaveMemberControl(frmData.ControlPanel, name, elementType, ReflectionExt.GetPassableAttributes(2, attributes), ref element, true);
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

                        staticLoadMemberControl(frmKey.ControlPanel, "(Dict) " + name + "<New Key>", keyType, new object[0] { }, null, true);


                        frmKey.OnOK += (object okSender, EventArgs okE) =>
                        {
                            staticSaveMemberControl(frmKey.ControlPanel, name, keyType, new object[0] { }, ref key, true);
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
                    loadLabelControl(control, name);

                    PriorityListBox lbxValue = new PriorityListBox();
                    lbxValue.Dock = DockStyle.Fill;
                    lbxValue.Size = new Size(0, 150);
                    lbxValue.LoadFromList(type, (IPriorityList)member);
                    control.Controls.Add(lbxValue);

                    Type elementType = type.GetGenericArguments()[0];
                    //add lambda expression for editing a single element
                    lbxValue.OnEditItem = (int priority, int index, object element, PriorityListBox.EditElementOp op) =>
                    {
                        ElementForm frmData = new ElementForm();
                        if (element == null)
                            frmData.Text = name + "/" + "New " + elementType.Name;
                        else
                            frmData.Text = name + "/" + element.ToString();

                        staticLoadMemberControl(frmData.ControlPanel, "(PriorityList) " + name + "[" + index + "]", elementType, ReflectionExt.GetPassableAttributes(2, attributes), element, true);

                        frmData.OnOK += (object okSender, EventArgs okE) =>
                        {
                            staticSaveMemberControl(frmData.ControlPanel, name, elementType, ReflectionExt.GetPassableAttributes(2, attributes), ref element, true);
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
                    loadLabelControl(control, name);

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

                        staticLoadMemberControl(frmData.ControlPanel, name, type, ReflectionExt.GetPassableAttributes(0, attributes), element, true);


                        frmData.OnOK += (object okSender, EventArgs okE) =>
                        {
                            staticSaveMemberControl(frmData.ControlPanel, name, type, ReflectionExt.GetPassableAttributes(0, attributes), ref element, true);
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
                        groupBox.AutoSize = true;
                        groupBox.AutoSizeMode = AutoSizeMode.GrowAndShrink;
                        groupBox.Dock = DockStyle.Fill;
                        groupBox.Text = name;
                        control.Controls.Add(groupBox);

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

                        groupBox.Controls.Add(groupBoxPanel);

                        loadClassControls(member, groupBoxPanel);
                    }
                    else
                    {
                        //note: considerations must be made when dealing with inheritance/polymorphism
                        //eg: find all children in this assembly that can be instantiated,
                        //add them to different panels
                        //show the one that is active right now
                        //include a combobox for switching children


                        TableLayoutPanel sharedRowPanel = new TableLayoutPanel();
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
                        control.Controls.Add(sharedRowPanel);

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

                        GroupBox groupBox = new GroupBox();
                        groupBox.AutoSize = true;
                        groupBox.AutoSizeMode = AutoSizeMode.GrowAndShrink;
                        groupBox.Dock = DockStyle.Fill;
                        groupBox.Text = name;
                        control.Controls.Add(groupBox);


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
                        

                        groupBox.Controls.Add(groupBoxPanel);

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
            if (obj.GetType().IsSubclassOf(typeof(EditorData)))
                ((EditorData)obj).SaveClassControls(control);
            else
                staticSaveClassControls(obj, control);
        }

        protected virtual void SaveClassControls(TableLayoutPanel control)
        {
            staticSaveClassControls(this, control);
        }

        private static void staticSaveClassControls(object obj, TableLayoutPanel control)
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



        protected static void saveMemberControl(object obj, TableLayoutPanel control, string name, Type type, object[] attributes, ref object member, bool isWindow)
        {
            if (obj.GetType().IsSubclassOf(typeof(EditorData)))
                ((EditorData)obj).SaveMemberControl(control, name, type, attributes, ref member, isWindow);
            else
                staticSaveMemberControl(control, name, type, attributes, ref member, isWindow);
        }

        protected virtual void SaveMemberControl(TableLayoutPanel control, string name, Type type, object[] attributes, ref object member, bool isWindow)
        {
            staticSaveMemberControl(control, name, type, attributes, ref member, isWindow);
        }

        protected static void staticSaveMemberControl(TableLayoutPanel control, string name, Type type, object[] attributes, ref object member, bool isWindow)
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
#endif



        //SpawnList
        //#if EDITORS
        //        public override void LoadClassControls(Control control, Rectangle rect)
        //        {
        //            //go through all members and add for them
        //            //control starts off clean; this is the control that will have all member controls on it
        //            try
        //            {
        //                //create panel and place in rect
        //                Panel main_panel = new Panel();
        //                main_panel.AutoScroll = true;
        //                main_panel.Location = rect.Location;
        //                main_panel.Size = rect.Size;
        //                //set panel's tag to a new list
        //                List<SpawnRate> tag_list = new List<SpawnRate>();
        //                foreach (SpawnRate tuple in spawns)
        //                    tag_list.Add(new SpawnRate(tuple.Spawn, tuple.Rate));
        //                //populate list with tuples created from tuples in the current list
        //                main_panel.Tag = tag_list;
        //                control.Controls.Add(main_panel);
        //                Button add_button = new Button();

        //                for (int ii = 0; ii < tag_list.Count; ii++)
        //                {
        //                    AddPanel(main_panel, add_button, tag_list, ii);
        //                }
        //                //add "add" button
        //                add_button.Text = "Add";
        //                add_button.Location = new Point(0, 70 * tag_list.Count);
        //                add_button.Size = new Size(50, 23);
        //                //clicking "add" creates an element form to choose with
        //                add_button.Click += (object sender, EventArgs e) =>
        //                {
        //                    Dev.ElementForm frmData = new Dev.ElementForm();
        //                    frmData.Text = "Edit Element";
        //                    int index = tag_list.Count;
        //                    Rectangle boxRect = new Rectangle(new Point(), GetMemberControlSize(typeof(T), null, null));
        //                    LoadMemberControl(frmData.ControlPanel, "(SpawnRate) " /*+ name*/ + "[" + index + "]", typeof(T), null, null, boxRect);
        //                    frmData.ResizeForPanelSize(boxRect.Size);

        //                    //if selected OK, adds object to list, inserts new panel, and loads it
        //                    if (frmData.ShowDialog() == DialogResult.OK)
        //                    {
        //                        int controlIndex = 0;
        //                        object spawn = null;
        //                        SaveMemberControl(frmData.ControlPanel, typeof(T), null, ref spawn, ref controlIndex);
        //                        SpawnRate tuple = new SpawnRate((T)spawn, 10);
        //                        tag_list.Add(tuple);
        //                        AddPanel(main_panel, add_button, tag_list, tag_list.Count - 1);
        //                        add_button.Location = new Point(0, tag_list.Count * 70);
        //                        //then modify chances of all the other panels
        //                        UpdateTotalChance(main_panel, tag_list);
        //                    }
        //                };
        //                main_panel.Controls.Add(add_button);
        //                UpdateTotalChance(main_panel, tag_list);
        //            }
        //            catch (Exception e)
        //            {
        //                DiagManager.Instance.LogError(e);
        //            }
        //        }

        //        public override int PanelWidth { get { return 260; } }
        //        public override int GetClassHeight() { return 340; }


        //        public override void SaveClassControls(Control control)
        //        {
        //            try
        //            {
        //                //set list to panel tag
        //                Panel panel = (Panel)control.Controls[0];
        //                spawns = (List<SpawnRate>)panel.Tag;
        //                //update spawn total
        //                spawnTotal = ListTotal(spawns);
        //            }
        //            catch (Exception e)
        //            {
        //                DiagManager.Instance.LogError(e);
        //            }
        //        }

        //        void AddPanel(Panel main_panel, Button add_button, List<SpawnRate> tag_list, int ii)
        //        {
        //            //add panel to main panel
        //            Panel panel = new Panel();
        //            panel.Location = new Point(0, ii * 70);
        //            panel.Size = new Size(main_panel.Size.Width - 18, 70);

        //            SpawnRate tuple = tag_list[ii];

        //            //set up name and calculated percent
        //            Label spawn_name = new Label();
        //            spawn_name.Location = new System.Drawing.Point(0, 5);
        //            spawn_name.AutoEllipsis = true;
        //            spawn_name.Size = new Size(panel.Size.Width - 60, 13);
        //            spawn_name.Text = tuple.Spawn.ToString();

        //            Label spawn_weight = new Label();
        //            spawn_weight.Location = new System.Drawing.Point(panel.Size.Width - 60, 53);
        //            spawn_weight.AutoSize = true;
        //            spawn_weight.Text = "Weight: " + tuple.Rate;

        //            Label spawn_chance = new Label();
        //            spawn_chance.Location = new System.Drawing.Point(panel.Size.Width - 50, 5);
        //            spawn_chance.AutoSize = true;
        //            spawn_chance.Text = "%";
        //            spawn_chance.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

        //            //set up buttons:
        //            Button edit_button = new Button();
        //            edit_button.Text = "Edit";
        //            edit_button.Location = new Point(0, 48);
        //            edit_button.Size = new Size(34, 21);
        //            //clicking edit opens the element form
        //            edit_button.Click += (object sender, EventArgs e) =>
        //            {
        //                Dev.ElementForm frmData = new Dev.ElementForm();
        //                frmData.Text = "Edit Element";
        //                int index = tag_list.IndexOf(tuple);
        //                Rectangle boxRect = new Rectangle(new Point(), GetMemberControlSize(typeof(T), null, tuple.Spawn));
        //                LoadMemberControl(frmData.ControlPanel, "(SpawnRate) " /*+ name*/ + "[" + index + "]", typeof(T), null, tuple.Spawn, boxRect);
        //                frmData.ResizeForPanelSize(boxRect.Size);
        //                //if selected OK, sets object in list, modifies label name
        //                if (frmData.ShowDialog() == DialogResult.OK)
        //                {
        //                    int controlIndex = 0;
        //                    object spawn = tuple.Spawn;
        //                    SaveMemberControl(frmData.ControlPanel, typeof(T), null, ref spawn, ref controlIndex);
        //                    tuple.Spawn = (T)spawn;
        //                }
        //            };

        //            Button delete_button = new Button();
        //            delete_button.Text = "Delete";
        //            delete_button.Location = new Point(34, 48);
        //            delete_button.Size = new Size(34, 21);
        //            //clicking delete removes the object, and removes panel and shifts all controls after this one up
        //            delete_button.Click += (object sender, EventArgs e) =>
        //            {
        //                int index = tag_list.IndexOf(tuple);
        //                tag_list.RemoveAt(index);
        //                main_panel.Controls.RemoveAt(index);
        //                for (int jj = index; jj < tag_list.Count; jj++)
        //                {
        //                    Panel shift_panel = (Panel)main_panel.Controls[jj];
        //                    shift_panel.Location = new Point(0, jj * 70);
        //                }
        //                UpdateTotalChance(main_panel, tag_list);
        //                add_button.Location = new Point(0, tag_list.Count * 70);
        //            };

        //            //set up trackbar to spawn rate
        //            //when trackbar changes, set appearance rate for all panels
        //            TrackBar trackBar = new TrackBar();
        //            trackBar.LargeChange = 10;
        //            trackBar.Location = new System.Drawing.Point(0, 20);
        //            trackBar.Maximum = 100;
        //            trackBar.Minimum = 1;
        //            trackBar.Size = new System.Drawing.Size(panel.Size.Width, 45);
        //            trackBar.TickFrequency = 10;
        //            trackBar.Value = tuple.Rate;
        //            trackBar.ValueChanged += (object sender, EventArgs e) =>
        //            {
        //                spawnTotal = spawnTotal - tuple.Rate + trackBar.Value;//MUST UPDATE THE SPAWNTOTAL TO STAY IN SYNC
        //                tuple.Rate = trackBar.Value;
        //                spawn_weight.Text = "Weight: " + trackBar.Value;
        //                UpdateTotalChance(main_panel, tag_list);
        //            };

        //            panel.Controls.Add(delete_button);
        //            panel.Controls.Add(edit_button);
        //            panel.Controls.Add(spawn_weight);
        //            panel.Controls.Add(spawn_chance);
        //            panel.Controls.Add(spawn_name);
        //            panel.Controls.Add(trackBar);
        //            main_panel.Controls.Add(panel);
        //            main_panel.Controls.SetChildIndex(panel, ii);
        //        }

        //        void UpdateTotalChance(Panel main_panel, List<SpawnRate> tag_list)
        //        {
        //            int total = ListTotal(tag_list);
        //            for (int jj = 0; jj < tag_list.Count; jj++)
        //            {
        //                Label label = (Label)main_panel.Controls[jj].Controls[3];
        //                label.Text = ((decimal)tag_list[jj].Rate / total).ToString("P2");
        //            }
        //        }
        //#endif

    }
}

