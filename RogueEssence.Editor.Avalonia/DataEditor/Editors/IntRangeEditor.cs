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
    public class IntRangeEditor : Editor<IntRange>
    {
        public override bool DefaultSubgroup => true;
        public override bool DefaultDecoration => false;

        public override void LoadWindowControls(StackPanel control, string name, Type type, object[] attributes, IntRange member)
        {
            LoadLabelControl(control, name);

            RangeBorderAttribute rangeAtt = ReflectionExt.FindAttribute<RangeBorderAttribute>(attributes);
            int addMin = 0;
            int addMax = 0;
            if (rangeAtt != null)
                rangeAtt.GetAddVals(out addMin, out addMax);

            Avalonia.Controls.Grid innerPanel = getSharedRowPanel(4);
            innerPanel.ColumnDefinitions[0].Width = new GridLength(30);
            innerPanel.ColumnDefinitions[2].Width = new GridLength(30);

            TextBlock lblX = new TextBlock();
            lblX.Text = "Min:";
            lblX.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
            lblX.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right;
            innerPanel.Children.Add(lblX);
            lblX.SetValue(Avalonia.Controls.Grid.ColumnProperty, 0);

            NumericUpDown nudValueX = new NumericUpDown();
            nudValueX.Margin = new Thickness(4, 0, 0, 0);
            nudValueX.Minimum = Int32.MinValue;
            nudValueX.Maximum = Int32.MaxValue;
            nudValueX.Value = member.Min + addMin;
            innerPanel.Children.Add(nudValueX);
            nudValueX.SetValue(Avalonia.Controls.Grid.ColumnProperty, 1);

            TextBlock lblY = new TextBlock();
            lblY.Margin = new Thickness(8, 0, 0, 0);
            lblY.Text = "Max:";
            lblY.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
            lblY.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right;
            innerPanel.Children.Add(lblY);
            lblY.SetValue(Avalonia.Controls.Grid.ColumnProperty, 2);

            NumericUpDown nudValueY = new NumericUpDown();
            nudValueY.Margin = new Thickness(4, 0, 0, 0);
            nudValueY.Minimum = Int32.MinValue;
            nudValueY.Maximum = Int32.MaxValue;
            nudValueY.Value = member.Max + addMax;
            innerPanel.Children.Add(nudValueY);
            nudValueY.SetValue(Avalonia.Controls.Grid.ColumnProperty, 3);

            control.Children.Add(innerPanel);
        }


        public override IntRange SaveWindowControls(StackPanel control, string name, Type type, object[] attributes)
        {
            RangeBorderAttribute rangeAtt = ReflectionExt.FindAttribute<RangeBorderAttribute>(attributes);
            int addMin = 0;
            int addMax = 0;
            if (rangeAtt != null)
                rangeAtt.GetAddVals(out addMin, out addMax);

            int controlIndex = 0;
            controlIndex++;
            Avalonia.Controls.Grid innerControl = (Avalonia.Controls.Grid)control.Children[controlIndex];
            int innerControlIndex = 0;

            innerControlIndex++;
            NumericUpDown nudValueX = (NumericUpDown)innerControl.Children[innerControlIndex];
            innerControlIndex++;
            innerControlIndex++;
            NumericUpDown nudValueY = (NumericUpDown)innerControl.Children[innerControlIndex];
            return new IntRange((int)nudValueX.Value - addMin, (int)nudValueY.Value - addMax);
        }

        public override string GetString(IntRange obj, Type type, object[] attributes)
        {
            RangeBorderAttribute rangeAtt = ReflectionExt.FindAttribute<RangeBorderAttribute>(attributes);
            int addMin = 0;
            int addMax = 0;
            if (rangeAtt != null)
                rangeAtt.GetAddVals(out addMin, out addMax);

            if (obj.Min + addMin + 1 >= obj.Max + addMax)
                return obj.Min.ToString();
            else
                return string.Format("{0}-{1}", obj.Min + addMin, obj.Max + addMax);
        }
    }
}
