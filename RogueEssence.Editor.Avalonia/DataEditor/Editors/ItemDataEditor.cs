﻿using System;
using System.Collections.Generic;
using System.Text;
using RogueEssence.Content;
using RogueEssence.Dungeon;
using RogueEssence.Data;
using System.Drawing;
using Avalonia.Controls;
using RogueEssence.Dev.Views;

namespace RogueEssence.Dev
{
    public class ItemDataEditor : Editor<ItemData>
    {
        public override bool DefaultSubgroup => false;
        public override bool DefaultDecoration => true;

        public override void LoadMemberControl(ItemData obj, StackPanel control, string name, Type type, object[] attributes, object member, bool isWindow)
        {
            //if (name == "Sprite")
            //{
            //    LoadLabelControl(control, name);
            //    //for strings, use an edit textbox
            //    Dev.SpriteBrowser browser = new Dev.SpriteBrowser();
            //    browser.Size = new Size(210, 256);
            //    browser.ChosenPic = (int)member;
            //    control.Controls.Add(browser);
            //}
            //else
            //{
                base.LoadMemberControl(obj, control, name, type, attributes, member, isWindow);
            //}
        }

        public override object SaveMemberControl(ItemData obj, StackPanel control, string name, Type type, object[] attributes, bool isWindow)
        {
            //Here, we can just inject an attribute and now it's as if that member had an attribute assigned to it,
            //even when we didn't have access to the member itself! (via it being a part of another library, etc)

            //if (name == "Sprite")
            //{
            //    int controlIndex = 0;
            //    controlIndex++;
            //    Dev.SpriteBrowser browser = (Dev.SpriteBrowser)control.Controls[controlIndex];
            //    member = browser.ChosenPic;
            //    controlIndex++;
            //
            //}
            //else
            //{
            return base.SaveMemberControl(obj, control, name, type, attributes, isWindow);
            //}
        }
    }
}
