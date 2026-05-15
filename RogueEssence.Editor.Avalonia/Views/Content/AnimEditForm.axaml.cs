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
using RogueEssence.Dungeon;

namespace RogueEssence.Dev.Views
{
    public partial class AnimEditForm : Window
    {

        public AnimEditForm()
        {
            InitializeComponent();
        }
        
        public void Window_Closed(object sender, EventArgs e)
        {
            lock (GameBase.lockObj)
            {
                if (DungeonScene.Instance != null)
                {
                    DungeonScene.Instance.DebugAsset = RogueEssence.Content.GraphicsManager.AssetType.None;
                    DungeonScene.Instance.DebugAnim = "";
                }
            }
        }
    }
}
