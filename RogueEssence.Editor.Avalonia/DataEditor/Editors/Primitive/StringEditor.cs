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
        public override void LoadClassControls(StackPanel control, string name, Type type, object[] attributes, String member, bool isWindow)
        {
            LoadLabelControl(control, name);

            AnimAttribute animAtt = ReflectionExt.FindAttribute<AnimAttribute>(attributes);
            if (animAtt != null)
            {
                ComboBox cbValue = new ComboBox();
                cbValue.VirtualizationMode = ItemVirtualizationMode.Simple;
                string choice = member;

                List<string> items = new List<string>();
                items.Add("---");
                int chosenIndex = 0;

                string[] dirs = Directory.GetFiles(animAtt.FolderPath);

                for (int ii = 0; ii < dirs.Length; ii++)
                {
                    string filename = Path.GetFileNameWithoutExtension(dirs[ii]);
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
            else if (ReflectionExt.FindAttribute<SoundAttribute>(attributes) != null)
            {
                //is it a sound effect?

                ComboBox cbValue = new ComboBox();
                cbValue.VirtualizationMode = ItemVirtualizationMode.Simple;
                string choice = member;

                List<string> items = new List<string>();
                items.Add("---");
                int chosenIndex = 0;

                string[] dirs = Directory.GetFiles(DiagManager.CONTENT_PATH + "Sound/Battle");

                for (int ii = 0; ii < dirs.Length; ii++)
                {
                    string filename = Path.GetFileNameWithoutExtension(dirs[ii]);
                    if (filename == choice)
                        chosenIndex = items.Count;
                    items.Add(filename);
                }

                var subject = new Subject<List<string>>();
                cbValue.Bind(ComboBox.ItemsProperty, subject);
                subject.OnNext(items);
                cbValue.SelectionChanged += CbValue_PlaySound;
                cbValue.SelectedIndex = chosenIndex;
                control.Children.Add(cbValue);

            }
            else
            {
                //for strings, use an edit textbox
                TextBox txtValue = new TextBox();
                //txtValue.Dock = DockStyle.Fill;
                MultilineAttribute attribute = ReflectionExt.FindAttribute<MultilineAttribute>(attributes);
                if (attribute != null)
                {
                    //txtValue.Multiline = true;
                    //txtValue.Size = new Size(0, 80);
                }
                //else
                //    txtValue.Size = new Size(0, 20);
                txtValue.Text = (member == null) ? "" : member;
                control.Children.Add(txtValue);
            }
        }


        public override void SaveClassControls(StackPanel control, string name, Type type, object[] attributes, ref String member, bool isWindow)
        {
            int controlIndex = 0;
            controlIndex++;
            //for strings, use an edit textbox
            if (ReflectionExt.FindAttribute<AnimAttribute>(attributes) != null || ReflectionExt.FindAttribute<SoundAttribute>(attributes) != null)
            {
                ComboBox cbValue = (ComboBox)control.Children[controlIndex];
                if (cbValue.SelectedIndex == 0)
                    member = "";
                else
                    member = (string)cbValue.SelectedItem;
            }
            else
            {
                TextBox txtValue = (TextBox)control.Children[controlIndex];
                member = (String)txtValue.Text;
            }
            controlIndex++;
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
