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
    public class MusicEditor : StringEditor
    {
        public override bool DefaultSubgroup => true;

        public override bool DefaultDecoration => false;

        public override Type GetAttributeType() { return typeof(MusicAttribute); }

        public override void LoadWindowControls(StackPanel control, string parent, string name, Type type, object[] attributes, String member, Type[] subGroupStack)
        {
            ComboBox cbValue = new ComboBox();
            cbValue.VirtualizationMode = ItemVirtualizationMode.Simple;
            string choice = member;

            List<string> items = new List<string>();
            items.Add("---");
            int chosenIndex = 0;

            string[] dirs = PathMod.GetModFiles(GraphicsManager.CONTENT_PATH + "Music");

            for (int ii = 0; ii < dirs.Length; ii++)
            {
                string filename = Path.GetFileName(dirs[ii]);
                if (filename == choice)
                    chosenIndex = items.Count;
                items.Add(filename);
            }

            var subject = new Subject<List<string>>();
            cbValue.Bind(ComboBox.ItemsProperty, subject);
            subject.OnNext(items);
            cbValue.SelectedIndex = chosenIndex;
            control.Children.Add(cbValue);
        }


        public override String SaveWindowControls(StackPanel control, string name, Type type, object[] attributes, Type[] subGroupStack)
        {
            int controlIndex = 0;

            ComboBox cbValue = (ComboBox)control.Children[controlIndex];
            if (cbValue.SelectedIndex == 0)
                return "";
            else
                return (string)cbValue.SelectedItem;
        }
    }
}
