using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using RogueEssence.Dev.Views;
using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Text;

namespace RogueEssence.Dev
{
    public class ObjectConverter : EditorConverter<object>
    {

    }
    public abstract class EditorConverter<T> : IEditorConverter
    {
        protected delegate void CreateMethod();

        public Type GetConvertingType() { return typeof(T); }

        public virtual void LoadClassControls(StackPanel control, string name, Type type, object[] attributes, T member, bool isWindow)
        {
            //if you want a class that is by default isolated to a classbox but has a custom UI when opened on its own/overridden to render,
            //override LoadWindowControls, which is called by those methods.

            //in all cases where the class itself isn't being rendered to the window, simply represent as an editable object
            DataEditor.LoadLabelControl(control, name);

            if (!isWindow && ReflectionExt.FindAttribute<SubGroupAttribute>(attributes) == null)
            {
                if (member == null)
                {
                    Type[] children = type.GetAssignableTypes();
                    //create an empty instance
                    member = (T)ReflectionExt.CreateMinimalInstance(children[0]);
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

                    DataEditor.StaticLoadMemberControl(frmData.ControlPanel, name, type, ReflectionExt.GetPassableAttributes(0, attributes), element, true);

                    frmData.SelectedOKEvent += () =>
                    {
                        DataEditor.StaticSaveMemberControl(frmData.ControlPanel, name, type, ReflectionExt.GetPassableAttributes(0, attributes), ref element, true);
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
                //if it's a class of its own, create a new panel
                //then pass it into the call
                //use the returned "ref" int to determine how big the panel should be
                //continue from there
                Type[] children = type.GetAssignableTypes();

                //handle null members by getting an instance of the FIRST instantiatable subclass (including itself) it can find
                if (member == null)
                    member = (T)ReflectionExt.CreateMinimalInstance(children[0]);

                if (children.Length < 1)
                    throw new Exception("Completely abstract field found for: " + name);
                else if (children.Length == 1)
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
                            DataEditor.SaveWindowControls(obj, groupBoxPanel);
                            DataEditor.setClipboardObj(obj);
                        };
                        pasteToolStripMenuItem.Click += async (object copySender, RoutedEventArgs copyE) =>
                        {
                            Type type1 = DataEditor.clipboardObj.GetType();
                            Type type2 = type;
                            if (type2.IsAssignableFrom(type1))
                            {
                                groupBoxPanel.Children.Clear();
                                DataEditor.LoadWindowControls(DataEditor.clipboardObj, groupBoxPanel);
                            }
                            else
                                await MessageBox.Show(control.GetOwningForm(), String.Format("Incompatible types:\n{0}\n{1}", type1.AssemblyQualifiedName, type2.AssemblyQualifiedName), "Invalid Operation", MessageBox.MessageBoxButtons.Ok);
                        };

                        groupBoxPanel.ContextMenu = copyPasteStrip;
                    }

                    DataEditor.LoadWindowControls(member, groupBoxPanel);
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


                    Avalonia.Controls.Grid sharedRowPanel = DataEditor.getSharedRowPanel(2);

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
                                DataEditor.LoadWindowControls(emptyMember, groupBoxPanel);//TODO: POTENTIAL INFINITE RECURSION?
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
                            DataEditor.SaveWindowControls(obj, groupBoxPanel);
                            DataEditor.setClipboardObj(obj);
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

                                groupBoxPanel.Children.Clear();
                                DataEditor.LoadWindowControls(DataEditor.clipboardObj, groupBoxPanel);
                            }
                            else
                                await MessageBox.Show(control.GetOwningForm(), String.Format("Incompatible types:\n{0}\n{1}", type1.AssemblyQualifiedName, type2.AssemblyQualifiedName), "Invalid Operation", MessageBox.MessageBoxButtons.Ok);
                        };

                        groupBoxPanel.ContextMenu = copyPasteStrip;
                    }

                    DataEditor.LoadWindowControls(member, groupBoxPanel);
                    border.Child = groupBoxPanel;
                    control.Children.Add(border);
                }
            }
        }

        public virtual void LoadWindowControls(T obj, StackPanel control)
        {
            DataEditor.StaticLoadClassControls(obj, control);
        }

        public virtual void LoadMemberControl(T obj, StackPanel control, string name, Type type, object[] attributes, object member, bool isWindow)
        {
            DataEditor.StaticLoadMemberControl(control, name, type, attributes, member, isWindow);
        }

        public virtual void SaveClassControls(StackPanel control, string name, Type type, object[] attributes, ref T member, bool isWindow)
        {
            int controlIndex = 0;
            if (!isWindow && ReflectionExt.FindAttribute<SubGroupAttribute>(attributes) == null)
            {
                controlIndex++;
                ClassBox cbxValue = (ClassBox)control.Children[controlIndex];
                member = (T)cbxValue.Object;
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
                    member = (T)ReflectionExt.CreateMinimalInstance(children[0]);
                else
                {

                    Avalonia.Controls.Grid subGrid = (Avalonia.Controls.Grid)control.Children[controlIndex];
                    ComboBox cbValue = (ComboBox)subGrid.Children[1];

                    member = (T)ReflectionExt.CreateMinimalInstance(children[cbValue.SelectedIndex]);
                    controlIndex++;
                }

                Border border = (Border)control.Children[controlIndex];
                DataEditor.SaveWindowControls(member, (StackPanel)border.Child);
                controlIndex++;
            }
        }
        public virtual void SaveWindowControls(T obj, StackPanel control)
        {
            DataEditor.StaticSaveClassControls(obj, control);
        }

        public virtual void SaveMemberControl(T obj, StackPanel control, string name, Type type, object[] attributes, ref object member, bool isWindow)
        {
            DataEditor.StaticSaveMemberControl(control, name, type, attributes, ref member, isWindow);
        }

        void IEditorConverter.LoadClassControls(StackPanel control, string name, Type type, object[] attributes, object member, bool isWindow)
        {
            LoadClassControls(control, name, type, attributes, (T)member, isWindow);
        }
        void IEditorConverter.LoadWindowControls(object obj, StackPanel control)
        {
            LoadWindowControls((T)obj, control);
        }

        void IEditorConverter.LoadMemberControl(object obj, StackPanel control, string name, Type type, object[] attributes, object member, bool isWindow)
        {
            LoadMemberControl((T)obj, control, name, type, attributes, member, isWindow);
        }
        void IEditorConverter.SaveClassControls(StackPanel control, string name, Type type, object[] attributes, ref object member, bool isWindow)
        {
            T mem = (T)member;
            SaveClassControls(control, name, type, attributes, ref mem, isWindow);
            member = mem;
        }

        void IEditorConverter.SaveWindowControls(object obj, StackPanel control)
        {
            SaveWindowControls((T)obj, control);
        }

        void IEditorConverter.SaveMemberControl(object obj, StackPanel control, string name, Type type, object[] attributes, ref object member, bool isWindow)
        {
            SaveMemberControl((T)obj, control, name, type, attributes, ref member, isWindow);
        }
    }
}
