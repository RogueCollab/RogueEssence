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
using Avalonia;
using System.Reactive.Subjects;

namespace RogueEssence.Dev
{
    public class EnumEditor : Editor<Enum>
    {
        public override bool DefaultSubgroup => true;
        public override bool DefaultDecoration => false;
        public override bool DefaultType => true;

        public override void LoadWindowControls(StackPanel control, string parent, Type parentType, string name, Type type, object[] attributes, Enum member, Type[] subGroupStack)
        {
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
                        chkValue.IsChecked = member.HasFlag((Enum)enums.GetValue(ii));
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
                {
                    string typeDesc = DevDataManager.GetMemberDoc(type, enums.GetValue(cbValue.SelectedIndex).ToString());
                    if (typeDesc != null)
                        ToolTip.SetTip(cbValue, typeDesc);
                }
                cbValue.SelectionChanged += (object sender, SelectionChangedEventArgs e) =>
                {
                    string typeDesc = DevDataManager.GetMemberDoc(type, enums.GetValue(cbValue.SelectedIndex).ToString());
                    if (typeDesc != null)
                        ToolTip.SetTip(cbValue, typeDesc);
                };
                control.Children.Add(cbValue);
            }
        }


        public override Enum SaveWindowControls(StackPanel control, string name, Type type, object[] attributes, Type[] subGroupStack)
        {
            int controlIndex = 0;

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
                return (Enum)Enum.ToObject(type, pending);
            }
            else
            {
                ComboBox cbValue = (ComboBox)control.Children[controlIndex];
                return (Enum)Enum.ToObject(type, cbValue.SelectedIndex);
            }
        }
    }
}
