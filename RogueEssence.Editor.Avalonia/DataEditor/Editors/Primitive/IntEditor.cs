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
using System.IO;

namespace RogueEssence.Dev
{
    public class IntEditor : Editor<Int32>
    {
        public override bool DefaultSubgroup => true;
        public override bool DefaultDecoration => false;

        public override void LoadWindowControls(StackPanel control, string name, Type type, object[] attributes, Int32 member)
        {
            LoadLabelControl(control, name);

            DataTypeAttribute dataAtt = ReflectionExt.FindAttribute<DataTypeAttribute>(attributes);
            FrameTypeAttribute frameAtt = ReflectionExt.FindAttribute<FrameTypeAttribute>(attributes);
            if (dataAtt != null)
            {
                ComboBox cbValue = new ComboBox();
                cbValue.VirtualizationMode = ItemVirtualizationMode.Simple;
                int chosenIndex = member;
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
                nudValue.Value = member;

                control.Children.Add(nudValue);
            }
        }


        public override Int32 SaveWindowControls(StackPanel control, string name, Type type, object[] attributes)
        {
            int controlIndex = 0;
            controlIndex++;
            DataTypeAttribute dataAtt = ReflectionExt.FindAttribute<DataTypeAttribute>(attributes);
            FrameTypeAttribute frameAtt = ReflectionExt.FindAttribute<FrameTypeAttribute>(attributes);
            if (dataAtt != null)
            {
                ComboBox cbValue = (ComboBox)control.Children[controlIndex];
                int returnValue = cbValue.SelectedIndex;
                if (dataAtt.IncludeInvalid)
                    returnValue--;
                return returnValue;
            }
            else if (frameAtt != null)
            {
                ComboBox cbValue = (ComboBox)control.Children[controlIndex];
                if (!frameAtt.DashOnly)
                    return cbValue.SelectedIndex;
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
                                return ii;
                            }
                        }
                    }
                }
                return 0;
            }
            else
            {
                NumericUpDown nudValue = (NumericUpDown)control.Children[controlIndex];
                return (Int32)nudValue.Value;
            }
        }

    }
}
