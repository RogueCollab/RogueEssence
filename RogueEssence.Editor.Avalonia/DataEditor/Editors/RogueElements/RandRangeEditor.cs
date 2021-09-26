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
    public class RandRangeEditor : Editor<RandRange>
    {
        /// <summary>
        /// Default display behavior of whether to treat 0s as 1s
        /// </summary>
        public bool Index1;

        /// <summary>
        /// Default display behavior of whether to treat end borders exclsusively
        /// </summary>
        public bool Inclusive;

        public RandRangeEditor(bool index1, bool inclusive)
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

        public override void LoadWindowControls(StackPanel control, string parent, string name, Type type, object[] attributes, RandRange member, Type[] subGroupStack)
        {
            LoadLabelControl(control, name);

            int addMin, addMax;
            getMinMaxOffsets(attributes, out addMin, out addMax);

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
            nudValueY.Value = Math.Max(member.Min + 1, member.Max) + addMax;
            innerPanel.Children.Add(nudValueY);
            nudValueY.SetValue(Avalonia.Controls.Grid.ColumnProperty, 3);

            control.Children.Add(innerPanel);
        }


        public override RandRange SaveWindowControls(StackPanel control, string name, Type type, object[] attributes, Type[] subGroupStack)
        {
            int addMin, addMax;
            getMinMaxOffsets(attributes, out addMin, out addMax);

            int controlIndex = 0;
            controlIndex++;
            Avalonia.Controls.Grid innerControl = (Avalonia.Controls.Grid)control.Children[controlIndex];
            int innerControlIndex = 0;

            innerControlIndex++;
            NumericUpDown nudValueX = (NumericUpDown)innerControl.Children[innerControlIndex];
            innerControlIndex++;
            innerControlIndex++;
            NumericUpDown nudValueY = (NumericUpDown)innerControl.Children[innerControlIndex];
            return new RandRange((int)nudValueX.Value - addMin, (int)nudValueY.Value - addMax);
        }

        public override string GetString(RandRange obj, Type type, object[] attributes)
        {
            int addMin, addMax;
            getMinMaxOffsets(attributes, out addMin, out addMax);

            if (obj.Min + addMin >= obj.Max + addMax)
                return obj.Min.ToString();
            else
                return string.Format("{0}-{1}", obj.Min + addMin, obj.Max + addMax);
        }
    }
}
