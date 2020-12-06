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
    public class BooleanEditor : Editor<Boolean>
    {
        public override void LoadClassControls(StackPanel control, string name, Type type, object[] attributes, Boolean member, bool isWindow)
        {
            CheckBox chkValue = new CheckBox();
            chkValue.Margin = new Thickness(0, 4, 0, 0);
            chkValue.Content = name;
            chkValue.IsChecked = member;
            control.Children.Add(chkValue);
        }


        public override void SaveClassControls(StackPanel control, string name, Type type, object[] attributes, ref Boolean member, bool isWindow)
        {
            int controlIndex = 0;
            CheckBox chkValue = (CheckBox)control.Children[controlIndex];
            member = chkValue.IsChecked.HasValue && chkValue.IsChecked.Value;
            controlIndex++;
        }

    }
}
