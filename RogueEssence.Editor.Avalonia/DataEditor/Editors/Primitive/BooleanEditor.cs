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
        public override bool DefaultSubgroup => true;
        public override bool DefaultDecoration => false;
        public override bool DefaultLabel => false;

        public override void LoadWindowControls(StackPanel control, string parent, string name, Type type, object[] attributes, Boolean member, Type[] subGroupStack)
        {
            CheckBox chkValue = new CheckBox();
            chkValue.Margin = new Thickness(0, 4, 0, 0);
            chkValue.Content = DataEditor.GetMemberTitle(name);
            chkValue.IsChecked = member;

            string desc = DevDataManager.GetDoc(subGroupStack[subGroupStack.Length - 2], name);
            ToolTip.SetTip(chkValue, desc);

            control.Children.Add(chkValue);
        }


        public override Boolean SaveWindowControls(StackPanel control, string name, Type type, object[] attributes, Type[] subGroupStack)
        {
            int controlIndex = 0;
            CheckBox chkValue = (CheckBox)control.Children[controlIndex];
            return chkValue.IsChecked.HasValue && chkValue.IsChecked.Value;
        }

    }
}
