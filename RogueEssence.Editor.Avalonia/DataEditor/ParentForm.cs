using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Input;
using Avalonia.Interactivity;
using System;
using RogueEssence;
using RogueEssence.Dev;
using Microsoft.Xna.Framework;
using Avalonia.Threading;
using System.Threading;
using System.Collections.Generic;
using System.ComponentModel;

namespace RogueEssence.Dev.Views
{
    public class ParentForm : Window
    {
        private List<Window> children;

        public ParentForm()
        {
            children = new List<Window>();
        }

        public void RegisterChild(Window child)
        {
            children.Add(child);
            child.Closed += (object sender, EventArgs e) =>
            {
                children.Remove(child);
            };
        }

        public void CloseChildren()
        {
            for (int ii = children.Count - 1; ii >= 0; ii--)
                children[ii].Close();
        }

    }
}
