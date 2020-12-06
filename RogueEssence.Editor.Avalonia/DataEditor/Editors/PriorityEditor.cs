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
    public class PriorityEditor : Editor<Priority>
    {
        public override void LoadClassControls(StackPanel control, string name, Type type, object[] attributes, Priority member, bool isWindow)
        {
            LoadLabelControl(control, name);

            //for strings, use an edit textbox
            TextBox txtValue = new TextBox();
            txtValue.Text = (member == null) ? "" : member.ToString();
            control.Children.Add(txtValue);
        }


        public override void SaveClassControls(StackPanel control, string name, Type type, object[] attributes, ref Priority member, bool isWindow)
        {
            int controlIndex = 0;
            controlIndex++;
            //attempt to parse
            //TODO: enforce validation
            TextBox txtValue = (TextBox)control.Children[controlIndex];
            string[] divText = txtValue.Text.Split('.');
            int[] divNums = new int[divText.Length];
            for (int ii = 0; ii < divText.Length; ii++)
            {
                int res;
                if (int.TryParse(divText[ii], out res))
                    divNums[ii] = res;
                else
                {
                    divNums = null;
                    break;
                }
            }
            member = new Priority(divNums);
            controlIndex++;
        }
    }
}
