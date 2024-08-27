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
    public class CharEditor : Editor<Char>
    {
        public override bool DefaultSubgroup => true;

        public override bool DefaultDecoration => false;

        public override void LoadWindowControls(StackPanel control, string parent, Type parentType, string name, Type type, object[] attributes, Char member, Type[] subGroupStack)
        {
            //for strings, use an edit textbox
            TextBox txtValue = new TextBox();

            //TODO: set max length

            txtValue.Text = ((int)member == 0) ? "" : member.ToString();
            control.Children.Add(txtValue);
        }


        public override Char SaveWindowControls(StackPanel control, string name, Type type, object[] attributes, Type[] subGroupStack)
        {
            int controlIndex = 0;

            TextBox txtValue = (TextBox)control.Children[controlIndex];

            if (String.IsNullOrEmpty(txtValue.Text))
                return (Char)0;

            return txtValue.Text[0];
        }
    }
}
