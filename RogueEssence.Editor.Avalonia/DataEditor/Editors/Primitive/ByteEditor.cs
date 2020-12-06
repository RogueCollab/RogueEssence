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
    public class ByteEditor : Editor<Byte>
    {
        public override void LoadClassControls(StackPanel control, string name, Type type, object[] attributes, Byte member, bool isWindow)
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
            nudValue.Value = (byte)member;

            control.Children.Add(nudValue);
        }


        public override void SaveClassControls(StackPanel control, string name, Type type, object[] attributes, ref Byte member, bool isWindow)
        {
            int controlIndex = 0;
            controlIndex++;
            NumericUpDown nudValue = (NumericUpDown)control.Children[controlIndex];
            member = (byte)nudValue.Value;
            controlIndex++;
        }

    }
}
