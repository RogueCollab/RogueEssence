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
using System.IO;

namespace RogueEssence.Dev
{
    public class SoundEditor : StringEditor
    {
        public override bool DefaultSubgroup => true;

        public override bool DefaultDecoration => false;

        public override Type GetAttributeType() { return typeof(SoundAttribute); }

        public override void LoadWindowControls(StackPanel control, string parent, Type parentType, string name, Type type, object[] attributes, String member, Type[] subGroupStack)
        {
            ComboBox cbValue = new SearchComboBox();
            string choice = member;

            List<string> items = new List<string>();
            items.Add("**EMPTY**");
            int chosenIndex = 0;

            string[] dirs = PathMod.GetModFiles(GraphicsManager.CONTENT_PATH + "Sound/Battle");

            for (int ii = 0; ii < dirs.Length; ii++)
            {
                string filename = Path.GetFileNameWithoutExtension(dirs[ii]);
                if (filename == choice)
                    chosenIndex = items.Count;
                items.Add(filename);
            }

            var subject = new Subject<List<string>>();
            cbValue.Bind(ComboBox.ItemsSourceProperty, subject);
            subject.OnNext(items);
            cbValue.SelectionChanged += CbValue_PlaySound;
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



        private static void CbValue_PlaySound(object sender, SelectionChangedEventArgs e)
        {
            ComboBox box = (ComboBox)sender;
            if (box.SelectedIndex > 0)
            {
                lock (GameBase.lockObj)
                    GameManager.Instance.BattleSE((string)box.SelectedItem);
            }
        }
    }
}
