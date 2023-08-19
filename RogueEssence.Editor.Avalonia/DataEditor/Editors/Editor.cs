using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Newtonsoft.Json;
using RogueEssence.Dev.ViewModels;
using RogueEssence.Dev.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace RogueEssence.Dev
{
    public abstract class Editor<T> : IEditor
    {
        protected delegate void CreateMethod(PartialType type, Type fullType);

        /// <summary>
        /// Determines if the editor contents should be shown in the containing panel.
        /// If not, they will show up as a classbox that needs to be clicked on to show the contents.
        /// </summary>
        public virtual bool DefaultSubgroup => false;
        /// <summary>
        /// Detemines if the editor contents should be enclosed in a box.
        /// </summary>
        public virtual bool DefaultDecoration => true;

        /// <summary>
        /// Determines whether a label should be put above the object editor contents.
        /// </summary>
        public virtual bool DefaultLabel => true;
        public virtual bool DefaultType => false;

        /// <summary>
        /// Denotes that this is a simple editor that will open by default if opened in its own window, but fallback to another editor if opened via CTRL+Click
        /// </summary>
        public virtual bool SimpleEditor => false;

        public static void LoadLabelControl(StackPanel control, string name, string desc)
        {
            TextBlock lblName = new TextBlock();
            lblName.Margin = new Thickness(0, 4, 0, 0);
            lblName.Text = Text.GetMemberTitle(name) + ":";

            if (desc != null)
                ToolTip.SetTip(lblName, desc);

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

        public virtual void LoadWindowControls(StackPanel control, string parent, Type parentType, string name, Type type, object[] attributes, T obj, Type[] subGroupStack)
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
                        DataEditor.LoadMemberControl(name, obj, stack, myInfo.Name, myInfo.GetMemberInfoType(), myInfo.GetCustomAttributes(false), myInfo.GetValue(obj), false, subGroupStack);
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
                            DataEditor.LoadMemberControl(name, obj, stack, myInfo.Name, myInfo.GetMemberInfoType(), myInfo.GetCustomAttributes(false), myInfo.GetValue(obj), false, subGroupStack);
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
        public virtual void LoadMemberControl(string parent, T obj, StackPanel control, string name, Type type, object[] attributes, object member, bool isWindow, Type[] subGroupStack)
        {
            DataEditor.LoadClassControls(control, parent, obj.GetType(), name, type, attributes, member, isWindow, subGroupStack, false);
        }

        public virtual T SaveWindowControls(StackPanel control, string name, Type type, object[] attributes, Type[] subGroupStack)
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
                        object member = DataEditor.SaveMemberControl(obj, (StackPanel)control.Children[controlIndex], myInfo.Name, myInfo.GetMemberInfoType(), myInfo.GetCustomAttributes(false), false, subGroupStack);
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
                            object member = DataEditor.SaveMemberControl(obj, (StackPanel)sharedRowControl.Children[jj], myInfo.Name, myInfo.GetMemberInfoType(), myInfo.GetCustomAttributes(false), false, subGroupStack);
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

        public virtual object SaveMemberControl(T obj, StackPanel control, string name, Type type, object[] attributes, bool isWindow, Type[] subGroupStack)
        {
            return DataEditor.SaveClassControls(control, name, type, attributes, isWindow, subGroupStack, false);
        }


        public virtual string GetString(T obj, Type type, object[] attributes)
        {
            return obj == null ? "NULL" : obj.ToString();
        }

        public virtual string GetTypeString()
        {
            return null;
        }

        void IEditor.LoadClassControls(StackPanel control, string parent, Type parentType, string name, Type type, object[] attributes, object member, bool isWindow, Type[] subGroupStack)
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
            //force subgroup to be false if this type has appeared in window subgroup stack before
            //prevents infinite recursion from loading a subgroup for a type that has its own type in a subgroup
            if (subGroupStack.Contains(type))
                subGroup = false;

            JsonConverterAttribute jsonAttribute = ReflectionExt.FindAttribute<JsonConverterAttribute>(attributes);
            if (!subGroup)
            {
                string desc = DevDataManager.GetMemberDoc(parentType, name);
                LoadLabelControl(control, name, desc);
                if (member == null)
                {
                    Type[] children;
                    if (DefaultType)
                        children = new Type[1] { type };
                    else
                        children = type.GetAssignableTypes(1);
                    //create an empty instance
                    member = ReflectionExt.CreateMinimalInstance(children[0]);
                }

                ClassBox cbxValue = new ClassBox();
                MultilineAttribute attribute = ReflectionExt.FindAttribute<MultilineAttribute>(attributes);
                if (attribute != null)
                {
                    cbxValue.Height = 80;
                    //cbxValue.Size = new Size(0, 80);
                }
                //else
                //    cbxValue.Size = new Size(0, 20);
                ClassBoxViewModel mv = new ClassBoxViewModel(new StringConv(type, ReflectionExt.GetPassableAttributes(0, attributes)));
                mv.LoadFromSource(member);
                cbxValue.DataContext = mv;
                control.Children.Add(cbxValue);

                //add lambda expression for editing a single element
                mv.OnEditItem += (object element, bool advancedEdit, ClassBoxViewModel.EditElementOp op) =>
                {
                    DataEditForm frmData = new DataEditForm();
                    frmData.Title = DataEditor.GetWindowTitle(parent, name, element, type, ReflectionExt.GetPassableAttributes(0, attributes));

                    DataEditor.LoadClassControls(frmData.ControlPanel, parent, parentType, name, type, ReflectionExt.GetPassableAttributes(0, attributes), element, true, new Type[0], advancedEdit);
                    DataEditor.TrackTypeSize(frmData, type);

                    frmData.SelectedOKEvent += async () =>
                    {
                        element = DataEditor.SaveClassControls(frmData.ControlPanel, name, type, ReflectionExt.GetPassableAttributes(0, attributes), true, new Type[0], advancedEdit);
                        op(element);
                        return true;
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
                        DataEditor.SetClipboardObj(mv.Object, jsonAttribute?.ConverterType);
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
                bool includeLabel = DefaultLabel;
                if (isWindow)
                {
                    includeDecoration = false;
                    includeLabel = false;
                }


                //if it's a class of its own, create a new panel
                //then pass it into the call
                //use the returned "ref" int to determine how big the panel should be
                //continue from there
                Type[] children;
                if (DefaultType)
                    children = new Type[1] { type };
                else
                    children = type.GetAssignableTypes(2);

                //handle null members by getting an instance of the FIRST instantiatable subclass (including itself) it can find
                if (member == null)
                    member = ReflectionExt.CreateMinimalInstance(children[0]);

                if (children.Length < 1)
                    throw new Exception("Completely abstract field found for: " + name);
                else if (children.Length == 1)
                {
                    control.DataContext = children[0];
                    Type memberType = member.GetType();
                    if (!children[0].IsAssignableTo(memberType))
                        throw new TargetException("Types do not match.");

                    StackPanel controlParent = control;

                    if (includeLabel)
                    {
                        string desc = DevDataManager.GetMemberDoc(parentType, name);
                        LoadLabelControl(control, name, desc);
                    }

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
                            Type[] newStack = new Type[subGroupStack.Length + 1];
                            subGroupStack.CopyTo(newStack, 0);
                            newStack[newStack.Length-1] = type;
                            object obj = DataEditor.SaveWindowControls(controlParent, name, children[0], attributes, newStack);
                            DataEditor.SetClipboardObj(obj, jsonAttribute?.ConverterType);
                        };
                        pasteToolStripMenuItem.Click += async (object copySender, RoutedEventArgs copyE) =>
                        {
                            Type type1 = DataEditor.clipboardObj.GetType();
                            Type type2 = type;
                            if (type2.IsAssignableFrom(type1))
                            {
                                controlParent.Children.Clear();
                                Type[] newStack = new Type[subGroupStack.Length + 1];
                                subGroupStack.CopyTo(newStack, 0);
                                newStack[newStack.Length - 1] = type;
                                DataEditor.LoadWindowControls(controlParent, parent, parentType, name, type1, attributes, DataEditor.clipboardObj, newStack);
                            }
                            else
                                await MessageBox.Show(control.GetOwningForm(), String.Format("Incompatible types:\n{0}\n{1}", type1.AssemblyQualifiedName, type2.AssemblyQualifiedName), "Invalid Operation", MessageBox.MessageBoxButtons.Ok);
                        };

                        control.ContextMenu = copyPasteStrip;
                    }
                    controlParent.Background = Avalonia.Media.Brushes.Transparent;
                    Type[] newStack = new Type[subGroupStack.Length + 1];
                    subGroupStack.CopyTo(newStack, 0);
                    newStack[newStack.Length - 1] = type;
                    DataEditor.LoadWindowControls(controlParent, parent, parentType, name, children[0], attributes, member, newStack);

                }
                else
                {
                    control.DataContext = null;
                    //note: considerations must be made when dealing with inheritance/polymorphism
                    //eg: find all children in this assembly that can be instantiated,
                    //add them to different panels
                    //show the one that is active right now
                    //include a combobox for switching children

                    StackPanel controlParent = null;
                    if (includeLabel)
                    {
                        string desc = DevDataManager.GetMemberDoc(parentType, name);
                        LoadLabelControl(control, name, desc);
                    }


                    StackPanel typeArgsPanel = new StackPanel();
                    control.Children.Add(typeArgsPanel);


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

                    //populates the controlParent with a minimal instance
                    //this is fired in situations where the type has changed.
                    Action initNewConstructedType = () =>
                    {
                        controlParent.Children.Clear();
                        Type fullType = getChosenType(typeArgsPanel);
                        object emptyMember = ReflectionExt.CreateMinimalInstance(fullType);
                        Type[] newStack = new Type[subGroupStack.Length + 1];
                        subGroupStack.CopyTo(newStack, 0);
                        newStack[newStack.Length - 1] = type;
                        DataEditor.LoadWindowControls(controlParent, parent, parentType, name, fullType, attributes, emptyMember, newStack);
                    };

                    populateTypeChoice(typeArgsPanel, initNewConstructedType, member.GetType(), new Type[0], type);


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
                            Type[] newStack = new Type[subGroupStack.Length + 1];
                            subGroupStack.CopyTo(newStack, 0);
                            newStack[newStack.Length - 1] = type;
                            object obj = DataEditor.SaveWindowControls(controlParent, name, getChosenType(typeArgsPanel), attributes, newStack);
                            DataEditor.SetClipboardObj(obj, jsonAttribute?.ConverterType);
                        };
                        pasteToolStripMenuItem.Click += async (object copySender, RoutedEventArgs copyE) =>
                        {
                            Type type1 = DataEditor.clipboardObj.GetType();
                            Type type2 = type;

                            if (type2.IsAssignableFrom(type1))
                            {
                                populateTypeChoice(typeArgsPanel, initNewConstructedType, type1, new Type[0], type);

                                controlParent.Children.Clear();
                                Type[] newStack = new Type[subGroupStack.Length + 1];
                                subGroupStack.CopyTo(newStack, 0);
                                newStack[newStack.Length - 1] = type;
                                DataEditor.LoadWindowControls(controlParent, parent, parentType, name, type1, attributes, DataEditor.clipboardObj, newStack);
                            }
                            else
                                await MessageBox.Show(control.GetOwningForm(), String.Format("Incompatible types:\n{0}\n{1}", type1.AssemblyQualifiedName, type2.AssemblyQualifiedName), "Invalid Operation", MessageBox.MessageBoxButtons.Ok);
                        };

                        control.ContextMenu = copyPasteStrip;
                    }
                    controlParent.Background = Avalonia.Media.Brushes.Transparent;
                    Type[] newStack = new Type[subGroupStack.Length + 1];
                    subGroupStack.CopyTo(newStack, 0);
                    newStack[newStack.Length - 1] = type;
                    DataEditor.LoadWindowControls(controlParent, parent, parentType, name, getChosenType(typeArgsPanel), attributes, member, newStack);
                }
            }
        }

        void IEditor.LoadWindowControls(StackPanel control, string parent, Type parentType, string name, Type type, object[] attributes, object obj, Type[] subGroupStack)
        {
            LoadWindowControls(control, parent, parentType, name, type, attributes, (T)obj, subGroupStack);
        }

        void IEditor.LoadMemberControl(string parent, object obj, StackPanel control, string name, Type type, object[] attributes, object member, bool isWindow, Type[] subGroupStack)
        {
            LoadMemberControl(parent, (T)obj, control, name, type, attributes, member, isWindow, subGroupStack);
        }
        object IEditor.SaveClassControls(StackPanel control, string name, Type type, object[] attributes, bool isWindow, Type[] subGroupStack)
        {
            int controlIndex = 0;

            bool subGroup = DefaultSubgroup;
            if (ReflectionExt.FindAttribute<SepGroupAttribute>(attributes) != null)
                subGroup = false;
            if (ReflectionExt.FindAttribute<SubGroupAttribute>(attributes) != null)
                subGroup = true;
            if (isWindow)
                subGroup = true;
            //force subgroup to be false if this type has appeared in window subgroup stack before
            if (subGroupStack.Contains(type))
                subGroup = false;

            if (!subGroup)
            {
                controlIndex++;
                ClassBox cbxValue = (ClassBox)control.Children[controlIndex];
                ClassBoxViewModel mv = (ClassBoxViewModel)cbxValue.DataContext;
                return mv.Object;
            }
            else
            {
                //need to create a new instance
                //note: considerations must be made when dealing with inheritance/polymorphism
                //eg: check to see if there are children of the type,
                //and if so, do this instead:
                //get the combobox index determining the type
                //instantiate the type
                //get the panel for the index
                //save using THAT panel

                bool includeDecoration = DefaultDecoration;
                bool includeLabel = DefaultLabel;
                if (isWindow)
                {
                    includeDecoration = false;
                    includeLabel = false;
                }


                if (control.DataContext != null)
                {
                    Type childType = (Type)control.DataContext;

                    StackPanel chosenParent = control;
                    
                    if (includeLabel)
                        controlIndex++;
                    if (includeDecoration)
                    {
                        Border border = (Border)control.Children[controlIndex];
                        chosenParent = (StackPanel)border.Child;
                    }
                    else
                        chosenParent = (StackPanel)control.Children[controlIndex];

                    Type[] newStack = new Type[subGroupStack.Length + 1];
                    subGroupStack.CopyTo(newStack, 0);
                    newStack[newStack.Length - 1] = type;
                    return DataEditor.SaveWindowControls(chosenParent, name, childType, attributes, newStack);
                }
                else
                {
                    StackPanel chosenParent = null;

                    if (includeLabel)
                        controlIndex++;

                    StackPanel typeContainer = (StackPanel)control.Children[controlIndex];
                    Type chosenType = getChosenType(typeContainer);

                    controlIndex++;

                    if (includeDecoration)
                    {
                        Border border = (Border)control.Children[controlIndex];
                        chosenParent = (StackPanel)border.Child;
                    }
                    else
                        chosenParent = (StackPanel)control.Children[controlIndex];

                    Type[] newStack = new Type[subGroupStack.Length + 1];
                    subGroupStack.CopyTo(newStack, 0);
                    newStack[newStack.Length - 1] = type;
                    return DataEditor.SaveWindowControls(chosenParent, name, chosenType, attributes, newStack);
                }

            }
        }

        /// <summary>
        /// Creates a string that consists of a generic type and its single type argument.
        /// Used only for the editor display HACK
        /// </summary>
        /// <returns></returns>
        private static string encaseParentTypes(Type[] parentTemplateTypes, Type type)
        {
            StringBuilder str = new StringBuilder();
            for (int ii = 0; ii < parentTemplateTypes.Length; ii++)
            {
                Type parentType = parentTemplateTypes[ii];
                str.Append(parentType.Name.Substring(0, parentType.Name.LastIndexOf("`", StringComparison.InvariantCulture)));
                str.Append("<");
            }
            str.Append(type.GetFriendlyTypeString());
            for (int ii = 0; ii < parentTemplateTypes.Length; ii++)
            {
                str.Append(">");
            }
            return str.ToString();
        }

        /// <summary>
        /// Populates a stack panel with options to choose the type for the editor
        /// </summary>
        /// <param name="type">Type of variable to be assigned to</param>
        /// <param name="typeContainer"></param>
        /// <param name="initNewConstructedType"></param>
        /// <param name="chosenType">Assignining variable's type</param>
        private void populateTypeChoice(StackPanel typeContainer, Action initNewConstructedType, Type chosenType, Type[] parentTemplateTypes, params Type[] constraints)
        {
            //TODO: piecewise selection of individual type args of generic types, instead of all at once
            bool piecewiseTypes = false;

            PartialType[] baseList = ReflectionExt.GetTypesFromConstraints(parentTemplateTypes, constraints);

            typeContainer.Children.Clear();
            typeContainer.DataContext = baseList;

            //NOTE: this block is a HACK to make a certain common editor display case look nicer.
            //remove this when piecewise type construction actually works!!
            if (baseList.Length == 1)
            {
                PartialType childType = baseList[0];
                //recurse special cases where there is only one possible option with one template class
                if (childType.Type.IsGenericTypeDefinition)
                {
                    //and it has one GenericArg that is not specified

                    if (childType.GenericArgs.Length == 1 && childType.GenericArgs[0] == null)
                    {
                        StackPanel subPanel = new StackPanel();
                        typeContainer.Children.Add(subPanel);

                        Type[] checkArgs = childType.Type.GetGenericArguments();
                        Type argType = checkArgs[0];
                        Type[] tpConstraints = argType.GetGenericParameterConstraints();

                        Type[] childTemplateTypes = new Type[parentTemplateTypes.Length + 1];
                        for (int nn = 0; nn < parentTemplateTypes.Length; nn++)
                            childTemplateTypes[nn] = parentTemplateTypes[nn];
                        childTemplateTypes[parentTemplateTypes.Length] = childType.Type;

                        Type chosenTypeArg = null;
                        if (chosenType != null)
                        {
                            Type[] filledArgs = chosenType.GenericTypeArguments;
                            chosenTypeArg = filledArgs[0];
                        }
                        populateTypeChoice(subPanel, initNewConstructedType, chosenTypeArg, childTemplateTypes, tpConstraints);
                        return;
                    }
                }
            }

            Grid sharedRowPanel = getSharedRowPanel(2);

            TextBlock lblType = new TextBlock();
            lblType.Text = "Type:";
            lblType.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
            sharedRowPanel.Children.Add(lblType);
            sharedRowPanel.ColumnDefinitions[0].Width = new GridLength(30);
            lblType.SetValue(Grid.ColumnProperty, 0);

            ComboBox cbType = new SearchComboBox();
            cbType.Margin = new Thickness(4, 0, 0, 0);
            cbType.VirtualizationMode = ItemVirtualizationMode.Simple;
            sharedRowPanel.Children.Add(cbType);
            cbType.SetValue(Grid.ColumnProperty, 1);

            typeContainer.Children.Add(sharedRowPanel);

            StackPanel templatePanel = new StackPanel();
            templatePanel.Margin = new Thickness(8, 0, 0, 0);
            typeContainer.Children.Add(templatePanel);


            CreateMethod templateArgMethod = (PartialType childType, Type fullChildType) =>
            {
                //clear the sub-type panel
                templatePanel.Children.Clear();
                templatePanel.DataContext = null;

                if (piecewiseTypes)
                {
                    //TODO: treat constructed types differently
                    //NOTE: due to the fact that one type argument can be a constraint for another,
                    //types with multiple arguments must maintain an order of dependency
                    //and an update in one will require a check on every dependent argument

                    //populate the sub-panel
                    //Type[] generics = childType.Type.GenericTypeArguments;
                    Type[] checkArgs = childType.Type.GetGenericArguments();
                    Type[] filledArgs = new Type[checkArgs.Length];
                    if (fullChildType != null)
                        filledArgs = fullChildType.GetGenericArguments();

                    for (int ii = 0; ii < checkArgs.Length; ii++)
                    {
                        Type argType = checkArgs[ii];

                        StackPanel typeArgsPanel = new StackPanel();
                        templatePanel.Children.Add(typeArgsPanel);

                        Type[] tpConstraints = argType.GetGenericParameterConstraints();
                        Type[] childTemplateTypes = new Type[parentTemplateTypes.Length + 1];
                        for (int nn = 0; nn < parentTemplateTypes.Length; nn++)
                            childTemplateTypes[nn] = parentTemplateTypes[nn];
                        childTemplateTypes[parentTemplateTypes.Length] = childType.Type;
                        populateTypeChoice(typeArgsPanel, initNewConstructedType, filledArgs[ii], childTemplateTypes, tpConstraints);
                    }
                }
                else
                {
                    if (childType.Type.IsGenericTypeDefinition)
                        populateTypeArgChoice(templatePanel, initNewConstructedType, childType, fullChildType);
                }
            };

            List<string> items = new List<string>();
            int selection = -1;
            for (int ii = 0; ii < baseList.Length; ii++)
            {
                PartialType childType = baseList[ii];

                //NOTE: the hack uses parent template types different from how the piecewise types approach would do it.
                items.Add(encaseParentTypes(parentTemplateTypes, childType.Type));

                if (childType.Type.IsGenericEqual(chosenType))
                    selection = ii;
            }
            if (chosenType == null)
                selection = 0;
            if (selection == -1)
                throw new TargetException("Types do not match.");

            var subject = new Subject<List<string>>();
            cbType.Bind(ComboBox.ItemsProperty, subject);
            subject.OnNext(items);
            cbType.SelectedIndex = selection;

            {
                string typeDesc = DevDataManager.GetTypeDoc(baseList[cbType.SelectedIndex].Type);
                ToolTip.SetTip(cbType, typeDesc);
            }

            cbType.SelectionChanged += (object sender, SelectionChangedEventArgs e) =>
            {
                string typeDesc = DevDataManager.GetTypeDoc(baseList[cbType.SelectedIndex].Type);
                ToolTip.SetTip(cbType, typeDesc);

                // this will pass in an unconstructed generic type, if generic
                // otherwise, if non-generic, just passes the type
                PartialType childType = baseList[cbType.SelectedIndex];
                templateArgMethod(childType, null);

                //the distinction lies in the triggering of the selected index callback.

                //selected index changed will never be called as a part of initialization.
                //It will always be called because the user made a change
                //Therefore, we should always reload the class panel
                //the populateTypeChoice recursive call within createMethods will not trigger any further loadWindowControls
                //due to those calls being an initialization.
                //however, the creation process needs to take a whole type.
                //therefore this call must be made into a delegate that is passed in and then called.
                //this will help with the argument bloat too
                initNewConstructedType();
            };

            //default to first selection
            templateArgMethod(baseList[cbType.SelectedIndex], chosenType);
        }


        private static string getTypeArgString(Type genericType)
        {
            StringBuilder str = new StringBuilder("<");
            Type[] args = genericType.GetGenericArguments();
            for (int ii = 0; ii < args.Length; ii++)
            {
                if (ii > 0)
                    str.Append(",");
                str.Append(args[ii].GetFriendlyTypeString());
            }
            str.Append(">");
            return str.ToString();
        }
        private void populateTypeArgChoice(StackPanel templatePanel, Action initNewConstructedType, PartialType partialType, Type chosenType)
        {
            templatePanel.Children.Clear();

            Type[] baseList = ReflectionExt.GetClosedTypesFromPartial(partialType);
            templatePanel.DataContext = baseList;

            Grid sharedRowPanel = getSharedRowPanel(2);

            TextBlock lblType = new TextBlock();
            lblType.Text = "Args:";
            lblType.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
            sharedRowPanel.Children.Add(lblType);
            sharedRowPanel.ColumnDefinitions[0].Width = new GridLength(30);
            lblType.SetValue(Grid.ColumnProperty, 0);

            ComboBox cbArgType = new SearchComboBox();
            cbArgType.Margin = new Thickness(4, 0, 0, 0);
            cbArgType.VirtualizationMode = ItemVirtualizationMode.Simple;
            sharedRowPanel.Children.Add(cbArgType);
            cbArgType.SetValue(Grid.ColumnProperty, 1);

            templatePanel.Children.Add(sharedRowPanel);


            List<string> items = new List<string>();
            int selection = -1;
            for (int ii = 0; ii < baseList.Length; ii++)
            {
                Type childType = baseList[ii];
                items.Add(getTypeArgString(childType));

                if (childType == chosenType)
                    selection = ii;
            }
            if (chosenType == null)
                selection = 0;
            if (selection == -1)
                throw new TargetException("Types do not match.");



            var subject = new Subject<List<string>>();
            cbArgType.Bind(ComboBox.ItemsProperty, subject);
            subject.OnNext(items);
            cbArgType.SelectedIndex = selection;

            {
                string typeDesc = DevDataManager.GetTypeDoc(baseList[cbArgType.SelectedIndex]);
                ToolTip.SetTip(cbArgType, typeDesc);
            }

            cbArgType.SelectionChanged += (object sender, SelectionChangedEventArgs e) =>
            {
                string typeDesc = DevDataManager.GetTypeDoc(baseList[cbArgType.SelectedIndex]);
                ToolTip.SetTip(cbArgType, typeDesc);

                initNewConstructedType();
            };

        }

        private Type getChosenType(StackPanel typeContainer)
        {
            PartialType[] baseList = (PartialType[])typeContainer.DataContext;

            //NOTE: this block is a HACK to make a certain common editor display case look nicer.
            //remove this when piecewise type construction actually works!!
            if (baseList.Length == 1)
            {
                PartialType childType = baseList[0];
                //recurse special cases where there is only one possible option with one template class
                if (childType.Type.IsGenericTypeDefinition)
                {
                    //and it has one GenericArg that is not specified

                    if (childType.GenericArgs.Length == 1 && childType.GenericArgs[0] == null)
                    {
                        StackPanel subPanel = (StackPanel)typeContainer.Children[0];
                        Type chosenArgType = getChosenType(subPanel);
                        return childType.Type.MakeGenericType(new Type[1] { chosenArgType });
                    }
                }
            }

            Grid subGrid = (Grid)typeContainer.Children[0];
            ComboBox cbValue = (ComboBox)subGrid.Children[1];

            PartialType chosenType = baseList[cbValue.SelectedIndex];

            bool piecewiseTypes = false;
            Type returnType;
            if (chosenType.Type.IsGenericTypeDefinition)
            {
                StackPanel templatePanel = (StackPanel)typeContainer.Children[1];
                if (piecewiseTypes)
                {
                    Type[] argTypes = new Type[chosenType.Type.GetGenericArguments().Length];
                    for (int ii = 0; ii < argTypes.Length; ii++)
                    {
                        StackPanel typeArgsPanel = (StackPanel)templatePanel.Children[ii];

                        Type argType = getChosenType(typeArgsPanel);
                        argTypes[ii] = argType;
                    }
                    returnType = chosenType.Type.MakeGenericType(argTypes);
                }
                else
                    returnType = getChosenTypeArgs(templatePanel);

            }
            else
                returnType = chosenType.Type;



            return returnType;
        }

        private Type getChosenTypeArgs(StackPanel templatePanel)
        {
            Type[] baseList = (Type[])templatePanel.DataContext;
            Grid subGrid = (Grid)templatePanel.Children[0];
            ComboBox cbValue = (ComboBox)subGrid.Children[1];

            return baseList[cbValue.SelectedIndex];
        }

        object IEditor.SaveWindowControls(StackPanel control, string name, Type type, object[] attributes, Type[] subGroupStack)
        {
            try
            {
                return SaveWindowControls(control, name, type, attributes, subGroupStack);
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex);
            }
            return default(T);
        }

        object IEditor.SaveMemberControl(object obj, StackPanel control, string name, Type type, object[] attributes, bool isWindow, Type[] subGroupStack)
        {
            return SaveMemberControl((T)obj, control, name, type, attributes, isWindow, subGroupStack);
        }

        string IEditor.GetString(object obj, Type type, object[] attributes)
        {
            return GetString((T)obj, type, attributes);
        }
    }
}
