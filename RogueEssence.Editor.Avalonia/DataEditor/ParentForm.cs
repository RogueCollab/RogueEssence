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
        protected List<Window> children;
        protected bool OK;
        protected bool Cancel;

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

        public void FocusChildren()
        {
            for (int ii = children.Count - 1; ii >= 0; ii--)
            {
                children[ii].Activate();
                ParentForm dataEditor = children[ii] as ParentForm;
                if (dataEditor != null)
                {
                    dataEditor.FocusChildren();
                }
            }
        }

        public void CloseChildren()
        {
            for (int ii = children.Count - 1; ii >= 0; ii--)
            {
                ParentForm dataEditor = children[ii] as ParentForm;
                if (dataEditor != null)
                {
                    dataEditor.OK = this.OK;
                    dataEditor.Cancel = this.Cancel;
                }
                children[ii].Close();
            }
                
        }

    }
}
