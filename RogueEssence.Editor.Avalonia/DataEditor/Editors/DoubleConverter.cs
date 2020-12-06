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
    public class DoubleConverter : EditorConverter<Double>
    {
        public override void LoadClassControls(StackPanel control, string name, Type type, object[] attributes, Double member, bool isWindow)
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
            nudValue.Value = (double)member;
            control.Children.Add(nudValue);
        }


        public override void SaveClassControls(StackPanel control, string name, Type type, object[] attributes, ref Double member, bool isWindow)
        {
            int controlIndex = 0;
            controlIndex++;
            NumericUpDown nudValue = (NumericUpDown)control.Children[controlIndex];
            member = (Double)nudValue.Value;
            controlIndex++;
        }

    }
}
