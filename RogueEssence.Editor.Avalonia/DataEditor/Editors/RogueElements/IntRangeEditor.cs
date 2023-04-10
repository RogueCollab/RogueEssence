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
        /// <summary>
        /// Default display behavior of whether to treat 0s as 1s
        /// </summary>
        public bool Index1;

        /// <summary>
        /// Default display behavior of whether to treat end borders exclsusively
        /// </summary>
        public bool Inclusive;

        public IntRangeEditor(bool index1, bool inclusive)
        {
            Index1 = index1;
            Inclusive = inclusive;
        }

        public override bool DefaultSubgroup => true;
        public override bool DefaultDecoration => false;

        private void getMinMaxOffsets(object[] attributes, out int addMin, out int addMax)
        {
            RangeBorderAttribute rangeAtt = ReflectionExt.FindAttribute<RangeBorderAttribute>(attributes);
            bool index1 = Index1;
            bool inclusive = Inclusive;
            if (rangeAtt != null)
            {
                index1 = rangeAtt.Index1;
                inclusive = rangeAtt.Inclusive;
            }
            RangeBorderAttribute.GetAddVals(index1, inclusive, out addMin, out addMax);
        }

        public override void LoadWindowControls(StackPanel control, string parent, Type parentType, string name, Type type, object[] attributes, IntRange member, Type[] subGroupStack)
        {
            int addMin, addMax;
            getMinMaxOffsets(attributes, out addMin, out addMax);

            Avalonia.Controls.Grid innerPanel = getSharedRowPanel(4);
            innerPanel.ColumnDefinitions[0].Width = new GridLength(30);
            innerPanel.ColumnDefinitions[2].Width = new GridLength(30);

            TextBlock lblMin = new TextBlock();
            lblMin.Text = "Min:";
            lblMin.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
            lblMin.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right;
            innerPanel.Children.Add(lblMin);
            lblMin.SetValue(Avalonia.Controls.Grid.ColumnProperty, 0);

            NumericUpDown nudValueMin = new NumericUpDown();
            nudValueMin.Margin = new Thickness(4, 0, 0, 0);
            nudValueMin.Minimum = Int32.MinValue;
            nudValueMin.Maximum = Int32.MaxValue;
            nudValueMin.Value = member.Min + addMin;
            innerPanel.Children.Add(nudValueMin);
            nudValueMin.SetValue(Avalonia.Controls.Grid.ColumnProperty, 1);

            TextBlock lblMax = new TextBlock();
            lblMax.Margin = new Thickness(8, 0, 0, 0);
            lblMax.Text = "Max:";
            lblMax.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
            lblMax.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right;
            innerPanel.Children.Add(lblMax);
            lblMax.SetValue(Avalonia.Controls.Grid.ColumnProperty, 2);

            NumericUpDown nudValueMax = new NumericUpDown();
            nudValueMax.Margin = new Thickness(4, 0, 0, 0);
            nudValueMax.Minimum = Int32.MinValue;
            nudValueMax.Maximum = Int32.MaxValue;
            nudValueMax.Value = member.Max + addMax;
            innerPanel.Children.Add(nudValueMax);
            nudValueMax.SetValue(Avalonia.Controls.Grid.ColumnProperty, 3);

            nudValueMin.ValueChanged += (object sender, NumericUpDownValueChangedEventArgs e) =>
            {
                if (nudValueMin.Value > nudValueMax.Value)
                    nudValueMax.Value = nudValueMin.Value;
            };
            nudValueMax.ValueChanged += (object sender, NumericUpDownValueChangedEventArgs e) =>
            {
                if (nudValueMin.Value > nudValueMax.Value)
                    nudValueMin.Value = nudValueMax.Value;
            };

            control.Children.Add(innerPanel);
        }


        public override IntRange SaveWindowControls(StackPanel control, string name, Type type, object[] attributes, Type[] subGroupStack)
        {
            int addMin, addMax;
            getMinMaxOffsets(attributes, out addMin, out addMax);

            int controlIndex = 0;

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
            int addMin, addMax;
            getMinMaxOffsets(attributes, out addMin, out addMax);

            if (obj.Min + addMin + 1 >= obj.Max + addMax)
                return (obj.Min + addMin).ToString();
            else
                return string.Format("{0}-{1}", obj.Min + addMin, obj.Max + addMax);
        }
    }
}
