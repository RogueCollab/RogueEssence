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

            FrameTypeAttribute frameAtt = ReflectionExt.FindAttribute<FrameTypeAttribute>(attributes);
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


        public override Int32 SaveWindowControls(StackPanel control, string name, Type type, object[] attributes)
        {
            int controlIndex = 0;
            controlIndex++;

            NumericUpDown nudValue = (NumericUpDown)control.Children[controlIndex];
            return (Int32)nudValue.Value;
        }

    }
}
