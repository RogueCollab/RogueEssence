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

        public override void LoadWindowControls(StackPanel control, string parent, Type parentType, string name, Type type, object[] attributes, Int32 member, Type[] subGroupStack)
        {
            NumericUpDown nudValue = new NumericUpDown();
            int minimum = Int32.MinValue;
            int maximum = Int32.MaxValue;
            NumberRangeAttribute rangeAtt = ReflectionExt.FindAttribute<NumberRangeAttribute>(attributes);
            if (rangeAtt != null)
            {
                minimum = rangeAtt.Min;
                maximum = rangeAtt.Max;
            }
            IntRangeAttribute intAtt = ReflectionExt.FindAttribute<IntRangeAttribute>(attributes);
            if (intAtt != null)
            {
                if (intAtt.Index1)
                {
                    minimum += 1;
                    if (maximum < Int32.MaxValue)
                        maximum += 1;
                    member += 1;
                }
            }
            nudValue.Minimum = minimum;
            nudValue.Maximum = maximum;
            nudValue.Value = member;

            control.Children.Add(nudValue);
        }


        public override Int32 SaveWindowControls(StackPanel control, string name, Type type, object[] attributes, Type[] subGroupStack)
        {
            int controlIndex = 0;

            NumericUpDown nudValue = (NumericUpDown)control.Children[controlIndex];
            int member = (Int32)nudValue.Value;

            IntRangeAttribute intAtt = ReflectionExt.FindAttribute<IntRangeAttribute>(attributes);
            if (intAtt != null && intAtt.Index1)
                member -= 1;
            return member;
        }

    }
}
