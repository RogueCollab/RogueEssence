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
    public class MultiplierEditor : Editor<Multiplier>
    {
        public override bool DefaultSubgroup => true;
        public override bool DefaultDecoration => false;

        public override void LoadWindowControls(StackPanel control, string parent, Type parentType, string name, Type type, object[] attributes, Multiplier member, Type[] subGroupStack)
        {
            FractionLimitAttribute rangeAtt = ReflectionExt.FindAttribute<FractionLimitAttribute>(attributes);
            int numMin = Int32.MinValue;
            int denMin = Int32.MinValue;
            if (rangeAtt != null)
            {
                numMin = rangeAtt.NumSign;
                denMin = rangeAtt.DenSign;
            }

            Avalonia.Controls.Grid innerPanel = getSharedRowPanel(3);
            innerPanel.ColumnDefinitions[1].Width = new GridLength(20);

            NumericUpDown nudValueMin = new NumericUpDown();
            nudValueMin.Margin = new Thickness(4, 0, 0, 0);
            nudValueMin.Minimum = numMin;
            nudValueMin.Maximum = Int32.MaxValue;
            nudValueMin.Value = member.Numerator;
            innerPanel.Children.Add(nudValueMin);
            nudValueMin.SetValue(Avalonia.Controls.Grid.ColumnProperty, 0);

            TextBlock lblMax = new TextBlock();
            lblMax.Margin = new Thickness(8, 0, 0, 0);
            lblMax.FontSize = 16;
            lblMax.Text = "/";
            lblMax.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
            lblMax.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center;
            innerPanel.Children.Add(lblMax);
            lblMax.SetValue(Avalonia.Controls.Grid.ColumnProperty, 1);

            NumericUpDown nudValueMax = new NumericUpDown();
            nudValueMax.Margin = new Thickness(4, 0, 0, 0);
            nudValueMax.Minimum = denMin;
            nudValueMax.Maximum = Int32.MaxValue;
            nudValueMax.Value = member.Denominator;
            innerPanel.Children.Add(nudValueMax);
            nudValueMax.SetValue(Avalonia.Controls.Grid.ColumnProperty, 3);

            control.Children.Add(innerPanel);
        }

        public override Multiplier SaveWindowControls(StackPanel control, string name, Type type, object[] attributes, Type[] subGroupStack)
        {
            int controlIndex = 0;

            Avalonia.Controls.Grid innerControl = (Avalonia.Controls.Grid)control.Children[controlIndex];
            int innerControlIndex = 0;

            NumericUpDown nudValueNum = (NumericUpDown)innerControl.Children[innerControlIndex];
            innerControlIndex++;
            innerControlIndex++;
            NumericUpDown nudValueDen = (NumericUpDown)innerControl.Children[innerControlIndex];
            return new Multiplier((int)nudValueNum.Value, (int)nudValueDen.Value);
        }
    }
}
