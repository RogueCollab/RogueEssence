using Avalonia.Interactivity;
using System;
using System.Collections.Generic;
using System.Text;
using ReactiveUI;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using RogueEssence.Dungeon;
using RogueEssence.Data;
using RogueEssence.Content;
using System.IO;
using Avalonia.Media.Imaging;
using RogueElements;
using RogueEssence.Dev.Views;
using RogueEssence.Menu;
using RogueEssence.Ground;

namespace RogueEssence.Dev.ViewModels
{
    public class TextTestViewModel : ViewModelBase
    {
        private string text;
        public string Text
        {
            get { return text; }
            set { this.SetIfChanged(ref text, value); }
        }


        public TextTestViewModel()
        {


        }


        public void btnDlg_Click()
        {
            lock (GameBase.lockObj)
            {
                if (text != "")
                {
                    if (GameManager.Instance.CurrentScene == DungeonScene.Instance)
                        DungeonScene.Instance.PendingDevEvent = MenuManager.Instance.SetDialogue(text);
                    else if (GameManager.Instance.CurrentScene == GroundScene.Instance)
                        GroundScene.Instance.PendingDevEvent = MenuManager.Instance.SetDialogue(text);
                }
            }
        }
    }
}
