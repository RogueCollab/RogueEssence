using System;
using System.Collections.Generic;
using System.Text;
using RogueEssence.Content;
using RogueEssence.Dungeon;
using RogueEssence.Data;
using System.Drawing;
using Avalonia.Controls;

namespace RogueEssence.Dev
{
    public class ItemDataConverter : EditorConverter<ItemData>
    {
        public override void LoadMemberControl(ItemData obj, StackPanel control, string name, Type type, object[] attributes, object member, bool isWindow)
        {
            //if (name == "Sprite")
            //{
            //    DataEditor.LoadLabelControl(control, name);
            //    //for strings, use an edit textbox
            //    Dev.SpriteBrowser browser = new Dev.SpriteBrowser();
            //    browser.Size = new Size(210, 256);
            //    browser.ChosenPic = (int)member;
            //    control.Controls.Add(browser);
            //}
            //else
            //{
            //    base.LoadMemberControl(obj, control, name, type, attributes, member, isWindow);
            //}
        }

        public override void SaveMemberControl(ItemData obj, StackPanel control, string name, Type type, object[] attributes, ref object member, bool isWindow)
        {
            //if (name == "Sprite")
            //{
            //    int controlIndex = 0;
            //    controlIndex++;
            //    Dev.SpriteBrowser browser = (Dev.SpriteBrowser)control.Controls[controlIndex];
            //    member = browser.ChosenPic;
            //    controlIndex++;
            //}
            //else
            //{
            //    base.SaveMemberControl(obj, control, name, type, attributes, ref member, isWindow);
            //}
        }
    }
}
