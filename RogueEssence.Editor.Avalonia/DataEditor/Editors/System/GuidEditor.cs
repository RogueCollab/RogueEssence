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
    public class GuidEditor : Editor<Guid>
    {
        public override bool DefaultSubgroup => true;

        public override bool DefaultDecoration => false;

        public override void LoadWindowControls(StackPanel control, string parent, Type parentType, string name, Type type, object[] attributes, Guid member, Type[] subGroupStack)
        {
            if (true)
            {
                TextBox txtValue = new TextBox();
                txtValue.Text = member.ToString().ToUpper();
                control.Children.Add(txtValue);
            }
            else
            {
                string[] stringVals = member.ToString().ToUpper().Split('-');

                Avalonia.Controls.Grid innerPanel = getSharedRowPanel(9);
                innerPanel.ColumnDefinitions[0].Width = new GridLength(2, GridUnitType.Star);
                innerPanel.ColumnDefinitions[1].Width = new GridLength(8);
                innerPanel.ColumnDefinitions[2].Width = new GridLength(1, GridUnitType.Star);
                innerPanel.ColumnDefinitions[3].Width = new GridLength(8);
                innerPanel.ColumnDefinitions[4].Width = new GridLength(1, GridUnitType.Star);
                innerPanel.ColumnDefinitions[5].Width = new GridLength(8);
                innerPanel.ColumnDefinitions[6].Width = new GridLength(1, GridUnitType.Star);
                innerPanel.ColumnDefinitions[7].Width = new GridLength(8);
                innerPanel.ColumnDefinitions[8].Width = new GridLength(3, GridUnitType.Star);

                for (int ii = 0; ii < stringVals.Length; ii++)
                {
                    if (ii > 0)
                    {
                        TextBlock lblDash = new TextBlock();
                        lblDash.Text = "-";
                        lblDash.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
                        lblDash.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center;
                        innerPanel.Children.Add(lblDash);
                        lblDash.SetValue(Avalonia.Controls.Grid.ColumnProperty, 1);
                    }

                    TextBox txtValue = new TextBox();
                    txtValue.Text = stringVals[ii];
                    innerPanel.Children.Add(txtValue);
                    txtValue.SetValue(Avalonia.Controls.Grid.ColumnProperty, ii * 2);
                }
                control.Children.Add(innerPanel);
            }
        }

        public override Guid SaveWindowControls(StackPanel control, string name, Type type, object[] attributes, Type[] subGroupStack)
        {
            int controlIndex = 0;
            if (true)
            {
                TextBox txtValue = (TextBox)control.Children[controlIndex];
                return new Guid(txtValue.Text);
            }
            else
            {
                StringBuilder str = new StringBuilder();
                Avalonia.Controls.Grid innerControl = (Avalonia.Controls.Grid)control.Children[controlIndex];
                int innerControlIndex = 0;

                for (int ii = 0; ii < 9; ii++)
                {
                    if (ii % 2 == 0)
                    {
                        TextBlock txt = (TextBlock)innerControl.Children[innerControlIndex];
                        str.Append(txt.Text);
                    }
                    else
                        str.Append("-");
                    innerControlIndex++;
                }
                return new Guid(str.ToString());
            }
        }
    }
}
