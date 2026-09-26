using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Input;
using Avalonia.Interactivity;
using System;
using System.IO;
using RogueEssence;
using RogueEssence.Dev;
using Microsoft.Xna.Framework;
using Avalonia.Threading;
using System.Threading;
using RogueEssence.Data;
using RogueEssence.Content;
using System.Collections.Generic;
using RogueEssence.Dev.ViewModels;

namespace RogueEssence.Dev.Views
{
    public partial class TileEditWindowView : ChromelessWindow
    {

        public TileEditWindowView()
        {
            InitializeComponent();

        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            var vm = DataContext as TileEditWindowViewModel;
            vm.SelectedOKEvent += this.Close;
            vm.SelectedCancelEvent += this.Close;
        }

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            var vm = DataContext as TileEditWindowViewModel;
            vm.SelectedOKEvent -= this.Close;
            vm.SelectedCancelEvent -= this.Close;
        }
    }
}
