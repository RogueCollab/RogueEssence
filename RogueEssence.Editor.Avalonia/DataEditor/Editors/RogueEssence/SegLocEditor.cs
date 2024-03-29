﻿using System;
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
    public class SegLocEditor : Editor<SegLoc>
    {
        public override bool DefaultSubgroup => true;
        public override bool DefaultDecoration => false;

        public override void LoadWindowControls(StackPanel control, string parent, Type parentType, string name, Type type, object[] attributes, SegLoc member, Type[] subGroupStack)
        {
            Avalonia.Controls.Grid innerPanel = getSharedRowPanel(4);

            TextBlock lblX = new TextBlock();
            lblX.Text = "Structure:";
            lblX.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
            lblX.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right;
            innerPanel.Children.Add(lblX);
            innerPanel.ColumnDefinitions[0].Width = new GridLength(40);
            lblX.SetValue(Avalonia.Controls.Grid.ColumnProperty, 0);

            NumericUpDown nudValueX = new NumericUpDown();
            nudValueX.Margin = new Thickness(4, 0, 0, 0);
            nudValueX.Minimum = Int32.MinValue;
            nudValueX.Maximum = Int32.MaxValue;
            nudValueX.Value = member.Segment;
            innerPanel.Children.Add(nudValueX);
            nudValueX.SetValue(Avalonia.Controls.Grid.ColumnProperty, 1);

            TextBlock lblY = new TextBlock();
            lblY.Margin = new Thickness(8, 0, 0, 0);
            lblY.Text = "Map:";
            lblY.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
            lblY.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right;
            innerPanel.Children.Add(lblY);
            innerPanel.ColumnDefinitions[2].Width = new GridLength(40);
            lblY.SetValue(Avalonia.Controls.Grid.ColumnProperty, 2);

            NumericUpDown nudValueY = new NumericUpDown();
            nudValueY.Margin = new Thickness(4, 0, 0, 0);
            nudValueY.Minimum = Int32.MinValue;
            nudValueY.Maximum = Int32.MaxValue;
            nudValueY.Value = member.ID;
            innerPanel.Children.Add(nudValueY);
            nudValueY.SetValue(Avalonia.Controls.Grid.ColumnProperty, 3);

            control.Children.Add(innerPanel);
        }


        public override SegLoc SaveWindowControls(StackPanel control, string name, Type type, object[] attributes, Type[] subGroupStack)
        {
            int controlIndex = 0;

            Avalonia.Controls.Grid innerControl = (Avalonia.Controls.Grid)control.Children[controlIndex];
            int innerControlIndex = 0;

            innerControlIndex++;
            NumericUpDown nudValueX = (NumericUpDown)innerControl.Children[innerControlIndex];
            innerControlIndex++;
            innerControlIndex++;
            NumericUpDown nudValueY = (NumericUpDown)innerControl.Children[innerControlIndex];
            return new SegLoc((int)nudValueX.Value, (int)nudValueY.Value);
        }
    }
}
