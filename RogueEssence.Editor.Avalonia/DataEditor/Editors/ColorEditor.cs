using System;
using System.Collections.Generic;
using System.Text;
using RogueEssence.Content;
using RogueEssence.Dungeon;
using RogueEssence.Data;
using RogueElements;
using Avalonia.Controls;
using RogueEssence.Dev.Views;
using System.Collections;
using Avalonia;
using System.Reactive.Subjects;
using Microsoft.Xna.Framework;

namespace RogueEssence.Dev
{
    public class ColorEditor : Editor<Color>
    {
        public override void LoadClassControls(StackPanel control, string name, Type type, object[] attributes, Color member, bool isWindow)
        {
            LoadLabelControl(control, name);

            Avalonia.Controls.Grid innerPanel = DataEditor.getSharedRowPanel(8);

            TextBlock lblR = new TextBlock();
            lblR.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
            lblR.Text = "R:";
            innerPanel.Children.Add(lblR);
            lblR.SetValue(Avalonia.Controls.Grid.ColumnProperty, 0);

            NumericUpDown nudValueR = new NumericUpDown();
            nudValueR.Margin = new Thickness(4, 0, 0, 0);
            nudValueR.Minimum = byte.MinValue;
            nudValueR.Maximum = byte.MaxValue;
            nudValueR.Value = (member == null) ? 0 : member.R;
            innerPanel.Children.Add(nudValueR);
            nudValueR.SetValue(Avalonia.Controls.Grid.ColumnProperty, 1);

            TextBlock lblG = new TextBlock();
            lblG.Margin = new Thickness(8, 0, 0, 0);
            lblG.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
            lblG.Text = "G:";
            innerPanel.Children.Add(lblG);
            lblG.SetValue(Avalonia.Controls.Grid.ColumnProperty, 2);

            NumericUpDown nudValueG = new NumericUpDown();
            nudValueG.Margin = new Thickness(4, 0, 0, 0);
            nudValueG.Minimum = byte.MinValue;
            nudValueG.Maximum = byte.MaxValue;
            nudValueG.Value = (member == null) ? 0 : member.G;
            innerPanel.Children.Add(nudValueG);
            nudValueG.SetValue(Avalonia.Controls.Grid.ColumnProperty, 3);

            TextBlock lblB = new TextBlock();
            lblB.Margin = new Thickness(8, 0, 0, 0);
            lblB.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
            lblB.Text = "B:";
            innerPanel.Children.Add(lblB);
            lblB.SetValue(Avalonia.Controls.Grid.ColumnProperty, 4);

            NumericUpDown nudValueB = new NumericUpDown();
            nudValueB.Margin = new Thickness(4, 0, 0, 0);
            nudValueB.Minimum = byte.MinValue;
            nudValueB.Maximum = byte.MaxValue;
            nudValueB.Value = (member == null) ? 0 : member.B;
            innerPanel.Children.Add(nudValueB);
            nudValueB.SetValue(Avalonia.Controls.Grid.ColumnProperty, 5);

            TextBlock lblA = new TextBlock();
            lblA.Margin = new Thickness(8, 0, 0, 0);
            lblA.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
            lblA.Text = "A:";
            innerPanel.Children.Add(lblA);
            lblA.SetValue(Avalonia.Controls.Grid.ColumnProperty, 6);

            NumericUpDown nudValueA = new NumericUpDown();
            nudValueA.Margin = new Thickness(4, 0, 0, 0);
            nudValueA.Minimum = byte.MinValue;
            nudValueA.Maximum = byte.MaxValue;
            nudValueA.Value = (member == null) ? 0 : member.A;
            innerPanel.Children.Add(nudValueA);
            nudValueA.SetValue(Avalonia.Controls.Grid.ColumnProperty, 7);

            control.Children.Add(innerPanel);
        }


        public override void SaveClassControls(StackPanel control, string name, Type type, object[] attributes, ref Color member, bool isWindow)
        {
            int controlIndex = 0;
            controlIndex++;
            Avalonia.Controls.Grid innerControl = (Avalonia.Controls.Grid)control.Children[controlIndex];
            int innerControlIndex = 0;

            innerControlIndex++;
            NumericUpDown nudValueR = (NumericUpDown)innerControl.Children[innerControlIndex];
            innerControlIndex++;
            innerControlIndex++;
            NumericUpDown nudValueG = (NumericUpDown)innerControl.Children[innerControlIndex];
            innerControlIndex++;
            innerControlIndex++;
            NumericUpDown nudValueB = (NumericUpDown)innerControl.Children[innerControlIndex];
            innerControlIndex++;
            innerControlIndex++;
            NumericUpDown nudValueA = (NumericUpDown)innerControl.Children[innerControlIndex];
            innerControlIndex++;
            member = new Color((int)nudValueR.Value, (int)nudValueG.Value, (int)nudValueB.Value, (int)nudValueA.Value);
        }
    }
}
