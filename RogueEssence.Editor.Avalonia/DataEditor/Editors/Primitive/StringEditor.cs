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
    public class StringEditor : Editor<String>
    {
        public override bool DefaultSubgroup => true;

        public override bool DefaultDecoration => false;

        public override void LoadWindowControls(StackPanel control, string parent, string name, Type type, object[] attributes, String member, Type[] subGroupStack)
        {
            LoadLabelControl(control, name);

            //for strings, use an edit textbox
            TextBox txtValue = new TextBox();
            //txtValue.Dock = DockStyle.Fill;
            MultilineAttribute attribute = ReflectionExt.FindAttribute<MultilineAttribute>(attributes);
            if (attribute != null)
            {
                txtValue.AcceptsReturn = true;
                txtValue.Height = 80;
                //txtValue.Size = new Size(0, 80);
            }
            //else
            //    txtValue.Size = new Size(0, 20);
            txtValue.Text = (member == null) ? "" : member;
            control.Children.Add(txtValue);
        }


        public override String SaveWindowControls(StackPanel control, string name, Type type, object[] attributes, Type[] subGroupStack)
        {
            int controlIndex = 0;
            controlIndex++;

            TextBox txtValue = (TextBox)control.Children[controlIndex];
            return txtValue.Text;
        }
    }
}
