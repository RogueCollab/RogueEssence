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
    public class DoubleEditor : Editor<Double>
    {
        public override bool DefaultSubgroup => true;
        public override bool DefaultDecoration => false;

        public override void LoadWindowControls(StackPanel control, string parent, Type parentType, string name, Type type, object[] attributes, Double member, Type[] subGroupStack)
        {
            NumericUpDown nudValue = new NumericUpDown();
            nudValue.Minimum = Int32.MinValue;
            nudValue.Maximum = Int32.MaxValue;
            NumberRangeAttribute attribute = ReflectionExt.FindAttribute<NumberRangeAttribute>(attributes);
            if (attribute != null)
            {
                nudValue.Minimum = attribute.Min;
                nudValue.Maximum = attribute.Max;
            }
            nudValue.Value = (decimal)member;
            control.Children.Add(nudValue);
        }


        public override Double SaveWindowControls(StackPanel control, string name, Type type, object[] attributes, Type[] subGroupStack)
        {
            int controlIndex = 0;
            NumericUpDown nudValue = (NumericUpDown)control.Children[controlIndex];
            return (Double)nudValue.Value;
        }

    }
}
