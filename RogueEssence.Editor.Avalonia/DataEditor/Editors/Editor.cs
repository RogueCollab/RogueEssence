using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using RogueEssence.Dev.ViewModels;
using RogueEssence.Dev.Views;
using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Reflection;
using System.Text;

namespace RogueEssence.Dev
{
    public abstract class Editor<T> : IEditor
    {
        protected delegate void CreateMethod();

        public virtual bool DefaultSubgroup => false;
        public virtual bool DefaultDecoration => true;
        public virtual bool DefaultType => false;

        public static void LoadLabelControl(StackPanel control, string name)
        {
            TextBlock lblName = new TextBlock();
            lblName.Margin = new Thickness(0, 4, 0, 0);
            lblName.Text = DataEditor.GetMemberTitle(name) + ":";
            control.Children.Add(lblName);
        }


        protected static Grid getSharedRowPanel(int cols)
        {
            Grid sharedRowPanel = new Grid();
            for (int ii = 0; ii < cols; ii++)
                sharedRowPanel.ColumnDefinitions.Add(new ColumnDefinition());

            return sharedRowPanel;
        }

        public virtual Type GetAttributeType() { return null; }
        public Type GetConvertingType() { return typeof(T); }

        public virtual void LoadWindowControls(StackPanel control, string parent, string name, Type type, object[] attributes, T obj)
        {
            //go through all members and add for them
            //control starts off clean; this is the control that will have all member controls on it
            try
            {
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
                        DataEditor.LoadMemberControl(name, obj, stack, myInfo.Name, myInfo.GetMemberInfoType(), myInfo.GetCustomAttributes(false), myInfo.GetValue(obj), false);
                    }
                    else
                    {
                        Grid sharedRowPanel = getSharedRowPanel(tieredFields[ii].Count);
                        control.Children.Add(sharedRowPanel);
                        for (int jj = 0; jj < tieredFields[ii].Count; jj++)
                        {
                            MemberInfo myInfo = tieredFields[ii][jj];
                            StackPanel stack = new StackPanel();
                            sharedRowPanel.Children.Add(stack);
                            stack.SetValue(Grid.ColumnProperty, jj);
                            DataEditor.LoadMemberControl(name, obj, stack, myInfo.Name, myInfo.GetMemberInfoType(), myInfo.GetCustomAttributes(false), myInfo.GetValue(obj), false);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(e);
            }
        }

        //TODO: add the ability to tag- using attributes- a specific member with a specific editor
        public virtual void LoadMemberControl(string parent, T obj, StackPanel control, string name, Type type, object[] attributes, object member, bool isWindow)
        {
            DataEditor.LoadClassControls(control, parent, name, type, attributes, member, isWindow);
        }

        public virtual T SaveWindowControls(StackPanel control, string name, Type type, object[] attributes)
        {
            try
            {
                //create instance of type here; object always starts null?
                T obj = (T)ReflectionExt.CreateMinimalInstance(type);

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
                        object member = DataEditor.SaveMemberControl(obj, (StackPanel)control.Children[controlIndex], myInfo.Name, myInfo.GetMemberInfoType(), myInfo.GetCustomAttributes(false), false);
                        myInfo.SetValue(obj, member);
                        controlIndex++;
                    }
                    else
                    {
                        Grid sharedRowControl = (Grid)control.Children[controlIndex];
                        int sharedRowControlIndex = 0;
                        for (int jj = 0; jj < tieredFields[ii].Count; jj++)
                        {
                            MemberInfo myInfo = tieredFields[ii][jj];
                            object member = DataEditor.SaveMemberControl(obj, (StackPanel)sharedRowControl.Children[jj], myInfo.Name, myInfo.GetMemberInfoType(), myInfo.GetCustomAttributes(false), false);
                            myInfo.SetValue(obj, member);
                            sharedRowControlIndex++;
                        }
                        controlIndex++;
                    }
                }
                return obj;
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(e);
                return default(T);
            }
        }

        public virtual object SaveMemberControl(T obj, StackPanel control, string name, Type type, object[] attributes, bool isWindow)
        {
            return DataEditor.SaveClassControls(control, name, type, attributes, isWindow);
        }


        public virtual string GetString(T obj, Type type, object[] attributes)
        {
            return obj == null ? "NULL" : obj.ToString();
        }

        void IEditor.LoadClassControls(StackPanel control, string parent, string name, Type type, object[] attributes, object member, bool isWindow)
        {
            //if you want a class that is by default isolated to a classbox but has a custom UI when opened on its own/overridden to render,
            //override LoadWindowControls, which is called by those methods.

            //in all cases where the class itself isn't being rendered to the window, simply represent as an editable object

            //isWindow will force subgroup automatically
            //otherwise, the presence of a Subgroup attribute will force it (or the presence of a Separation attribute will force it into its own classbox) 
            //then defaultSubgroup will force it.

            bool subGroup = DefaultSubgroup;
            if (ReflectionExt.FindAttribute<SepGroupAttribute>(attributes) != null)
                subGroup = false;
            if (ReflectionExt.FindAttribute<SubGroupAttribute>(attributes) != null)
                subGroup = true;
            if (isWindow)
                subGroup = true;

            if (!subGroup)
            {
                LoadLabelControl(control, name);
                if (member == null)
                {
                    Type[] children;
                    if (DefaultType)
                        children = new Type[1] { type };
                    else
                        children = type.GetAssignableTypes();
                    //create an empty instance
                    member = ReflectionExt.CreateMinimalInstance(children[0]);
                }

                ClassBox cbxValue = new ClassBox();
                MultilineAttribute attribute = ReflectionExt.FindAttribute<MultilineAttribute>(attributes);
                if (attribute != null)
                {
                    //txtValue.Multiline = true;
                    cbxValue.Height = 80;
                    //txtValue.Size = new Size(0, 80);
                }
                //else
                //    txtValue.Size = new Size(0, 20);
                ClassBoxViewModel mv = new ClassBoxViewModel(new StringConv(type, ReflectionExt.GetPassableAttributes(0, attributes)));
                mv.LoadFromSource(member);
                cbxValue.DataContext = mv;
                control.Children.Add(cbxValue);

                //add lambda expression for editing a single element
                mv.OnEditItem += (object element, ClassBoxViewModel.EditElementOp op) =>
                {
                    DataEditForm frmData = new DataEditForm();
                    frmData.Title = DataEditor.GetWindowTitle(parent, name, element, type, ReflectionExt.GetPassableAttributes(0, attributes));

                    DataEditor.LoadClassControls(frmData.ControlPanel, parent, name, type, ReflectionExt.GetPassableAttributes(0, attributes), element, true);

                    frmData.SelectedOKEvent += () =>
                    {
                        element = DataEditor.SaveClassControls(frmData.ControlPanel, name, type, ReflectionExt.GetPassableAttributes(0, attributes), true);
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
                        DataEditor.SetClipboardObj(mv.Object);
                    };
                    pasteToolStripMenuItem.Click += async (object copySender, RoutedEventArgs copyE) =>
                    {
                        Type type1 = DataEditor.clipboardObj.GetType();
                        Type type2 = type;
                        if (type2.IsAssignableFrom(type1))
                            mv.LoadFromSource(DataEditor.clipboardObj);
                        else
                            await MessageBox.Show(control.GetOwningForm(), String.Format("Incompatible types:\n{0}\n{1}", type1.AssemblyQualifiedName, type2.AssemblyQualifiedName), "Invalid Operation", MessageBox.MessageBoxButtons.Ok);
                    };

                    control.ContextMenu = copyPasteStrip;
                }
            }
            else
            {
                //when being drawn as a subgroup, we have 2 options:
                //(A) include a label and border, OR
                //(B) add controls directly to the current stackpanel?

                //additionally, there will be complications when the Type for a member has child classes
                //in this case, (A) choosing to have a label and border will result in label over the type dropdown, and border in the chosen class
                //(B) choosing not to have a label and border will remove the label, but still have a dropdown and contain it a child stackpanel, without border or margin

                //when isWindow is true, we never want option A. No label, no border, no margin
                //when isWindow is false, (which means either subgroup or defaultsubgroup is active) it's up to the editor itself to decide 

                bool includeDecoration = DefaultDecoration;
                if (isWindow)
                    includeDecoration = false;


                //if it's a class of its own, create a new panel
                //then pass it into the call
                //use the returned "ref" int to determine how big the panel should be
                //continue from there
                Type[] children;
                if (DefaultType)
                    children = new Type[1] { type };
                else
                    children = type.GetAssignableTypes();

                //handle null members by getting an instance of the FIRST instantiatable subclass (including itself) it can find
                if (member == null)
                    member = ReflectionExt.CreateMinimalInstance(children[0]);

                if (children.Length < 1)
                    throw new Exception("Completely abstract field found for: " + name);
                else if (children.Length == 1)
                {
                    Type memberType = member.GetType();
                    if (children[0] != memberType)
                        throw new TargetException("Types do not match.");

                    StackPanel controlParent = control;
                    if (includeDecoration)
                    {
                        LoadLabelControl(control, name);

                        Border border = new Border();
                        border.BorderThickness = new Thickness(1);
                        border.BorderBrush = Avalonia.Media.Brushes.LightGray;
                        border.Margin = new Thickness(2);
                        control.Children.Add(border);

                        StackPanel groupBoxPanel = new StackPanel();
                        groupBoxPanel.Margin = new Thickness(2);
                        border.Child = groupBoxPanel;

                        controlParent = groupBoxPanel;
                    }

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
                            object obj = DataEditor.SaveWindowControls(controlParent, name, children[0], attributes);
                            DataEditor.SetClipboardObj(obj);
                        };
                        pasteToolStripMenuItem.Click += async (object copySender, RoutedEventArgs copyE) =>
                        {
                            Type type1 = DataEditor.clipboardObj.GetType();
                            Type type2 = type;
                            if (type2.IsAssignableFrom(type1))
                            {
                                controlParent.Children.Clear();
                                DataEditor.LoadWindowControls(controlParent, parent, name, type1, attributes, DataEditor.clipboardObj);
                            }
                            else
                                await MessageBox.Show(control.GetOwningForm(), String.Format("Incompatible types:\n{0}\n{1}", type1.AssemblyQualifiedName, type2.AssemblyQualifiedName), "Invalid Operation", MessageBox.MessageBoxButtons.Ok);
                        };

                        control.ContextMenu = copyPasteStrip;
                    }
                    controlParent.Background = Avalonia.Media.Brushes.Transparent;
                    DataEditor.LoadWindowControls(controlParent, parent, name, children[0], attributes, member);

                }
                else
                {
                    //note: considerations must be made when dealing with inheritance/polymorphism
                    //eg: find all children in this assembly that can be instantiated,
                    //add them to different panels
                    //show the one that is active right now
                    //include a combobox for switching children

                    StackPanel controlParent = null;
                    if (includeDecoration)
                        LoadLabelControl(control, name);

                    Grid sharedRowPanel = getSharedRowPanel(2);

                    TextBlock lblType = new TextBlock();
                    lblType.Text = "Type:";
                    lblType.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
                    sharedRowPanel.Children.Add(lblType);
                    sharedRowPanel.ColumnDefinitions[0].Width = new GridLength(30);
                    lblType.SetValue(Grid.ColumnProperty, 0);

                    ComboBox cbValue = new ComboBox();
                    cbValue.Margin = new Thickness(4, 0, 0, 0);
                    cbValue.VirtualizationMode = ItemVirtualizationMode.Simple;
                    sharedRowPanel.Children.Add(cbValue);
                    cbValue.SetValue(Grid.ColumnProperty, 1);

                    control.Children.Add(sharedRowPanel);


                    if (includeDecoration)
                    {
                        Border border = new Border();
                        border.BorderThickness = new Thickness(1);
                        border.BorderBrush = Avalonia.Media.Brushes.LightGray;
                        border.Margin = new Thickness(2);
                        control.Children.Add(border);

                        StackPanel groupBoxPanel = new StackPanel();
                        groupBoxPanel.Margin = new Thickness(2);
                        border.Child = groupBoxPanel;

                        controlParent = groupBoxPanel;
                    }
                    else
                    {
                        StackPanel groupBoxPanel = new StackPanel();
                        control.Children.Add(groupBoxPanel);
                        controlParent = groupBoxPanel;
                    }



                    List<CreateMethod> createMethods = new List<CreateMethod>();

                    bool refreshPanel = true;
                    List<string> items = new List<string>();
                    int selection = -1;
                    for (int ii = 0; ii < children.Length; ii++)
                    {
                        Type childType = children[ii];
                        items.Add(childType.GetDisplayName());

                        createMethods.Add(() =>
                        {
                            if (refreshPanel)
                            {
                                controlParent.Children.Clear();
                                object emptyMember = ReflectionExt.CreateMinimalInstance(childType);
                                DataEditor.LoadWindowControls(controlParent, parent, name, childType, attributes, emptyMember);//TODO: POTENTIAL INFINITE RECURSION?
                            }
                        });
                        if (childType == member.GetType())
                            selection = ii;
                    }
                    if (selection == -1)
                       throw new TargetException("Types do not match.");

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
                            object obj = DataEditor.SaveWindowControls(controlParent, name, children[cbValue.SelectedIndex], attributes);
                            DataEditor.SetClipboardObj(obj);
                        };
                        pasteToolStripMenuItem.Click += async (object copySender, RoutedEventArgs copyE) =>
                        {
                            Type type1 = DataEditor.clipboardObj.GetType();
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

                                controlParent.Children.Clear();
                                DataEditor.LoadWindowControls(controlParent, parent, name, type1, attributes, DataEditor.clipboardObj);
                            }
                            else
                                await MessageBox.Show(control.GetOwningForm(), String.Format("Incompatible types:\n{0}\n{1}", type1.AssemblyQualifiedName, type2.AssemblyQualifiedName), "Invalid Operation", MessageBox.MessageBoxButtons.Ok);
                        };

                        control.ContextMenu = copyPasteStrip;
                    }
                    controlParent.Background = Avalonia.Media.Brushes.Transparent;
                    DataEditor.LoadWindowControls(controlParent, parent, name, children[selection], attributes, member);
                }
            }
        }

        void IEditor.LoadWindowControls(StackPanel control, string parent, string name, Type type, object[] attributes, object obj)
        {
            LoadWindowControls(control, parent, name, type, attributes, (T)obj);
        }

        void IEditor.LoadMemberControl(string parent, object obj, StackPanel control, string name, Type type, object[] attributes, object member, bool isWindow)
        {
            LoadMemberControl(parent, (T)obj, control, name, type, attributes, member, isWindow);
        }
        object IEditor.SaveClassControls(StackPanel control, string name, Type type, object[] attributes, bool isWindow)
        {
            int controlIndex = 0;

            bool subGroup = DefaultSubgroup;
            if (ReflectionExt.FindAttribute<SepGroupAttribute>(attributes) != null)
                subGroup = false;
            if (ReflectionExt.FindAttribute<SubGroupAttribute>(attributes) != null)
                subGroup = true;
            if (isWindow)
                subGroup = true;

            if (!subGroup)
            {
                controlIndex++;
                ClassBox cbxValue = (ClassBox)control.Children[controlIndex];
                ClassBoxViewModel mv = (ClassBoxViewModel)cbxValue.DataContext;
                return mv.Object;
            }
            else
            {
                Type[] children;
                if (DefaultType)
                    children = new Type[1] { type };
                else
                    children = type.GetAssignableTypes();

                //need to create a new instance
                //note: considerations must be made when dealing with inheritance/polymorphism
                //eg: check to see if there are children of the type,
                //and if so, do this instead:
                //get the combobox index determining the type
                //instantiate the type
                //get the panel for the index
                //save using THAT panel

                bool includeDecoration = DefaultDecoration;
                if (isWindow)
                    includeDecoration = false;


                if (children.Length == 1)
                {
                    StackPanel chosenParent = control;
                    if (includeDecoration)
                    {
                        controlIndex++;

                        Border border = (Border)control.Children[controlIndex];
                        chosenParent = (StackPanel)border.Child;
                    }
                    return DataEditor.SaveWindowControls(chosenParent, name, children[0], attributes);
                }
                else
                {
                    StackPanel chosenParent = null;
                    if (includeDecoration)
                        controlIndex++;

                    Grid subGrid = (Grid)control.Children[controlIndex];
                    ComboBox cbValue = (ComboBox)subGrid.Children[1];

                    controlIndex++;

                    if (includeDecoration)
                    {
                        Border border = (Border)control.Children[controlIndex];
                        chosenParent = (StackPanel)border.Child;
                    }
                    else
                        chosenParent = (StackPanel)control.Children[controlIndex];

                    return DataEditor.SaveWindowControls(chosenParent, name, children[cbValue.SelectedIndex], attributes);
                }

            }
        }

        object IEditor.SaveWindowControls(StackPanel control, string name, Type type, object[] attributes)
        {
            return SaveWindowControls(control, name, type, attributes);
        }

        object IEditor.SaveMemberControl(object obj, StackPanel control, string name, Type type, object[] attributes, bool isWindow)
        {
            return SaveMemberControl((T)obj, control, name, type, attributes, isWindow);
        }

        string IEditor.GetString(object obj, Type type, object[] attributes)
        {
            return GetString((T)obj, type, attributes);
        }
    }
}
