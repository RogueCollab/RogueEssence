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
        public override bool DefaultSubgroup => true;
        public override bool DefaultDecoration => false;

        public override void LoadWindowControls(StackPanel control, string parent, string name, Type type, object[] attributes, Priority member, Type[] subGroupStack)
        {
            LoadLabelControl(control, name);

            //for strings, use an edit textbox
            TextBox txtValue = new TextBox();
            txtValue.Text = member.ToString();
            control.Children.Add(txtValue);
        }


        public override Priority SaveWindowControls(StackPanel control, string name, Type type, object[] attributes, Type[] subGroupStack)
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
            return new Priority(divNums);
        }
    }
}
