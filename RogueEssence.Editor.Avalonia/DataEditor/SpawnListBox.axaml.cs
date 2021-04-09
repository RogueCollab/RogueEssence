using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using RogueElements;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Reactive.Subjects;
using Avalonia.Input;

namespace RogueEssence.Dev.Views
{
    public class SpawnListBox : UserControl
    {
        
        public SpawnListBox()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        bool doubleclick;
        public void doubleClickStart(object sender, RoutedEventArgs e)
        {
            doubleclick = true;
        }

        public void gridCollection_DoubleClick(object sender, PointerReleasedEventArgs e)
        {
            if (!doubleclick)
                return;
            doubleclick = false;

            ViewModels.SpawnListBoxViewModel viewModel = (ViewModels.SpawnListBoxViewModel)DataContext;
            if (viewModel == null)
                return;
            viewModel.gridCollection_DoubleClick(sender, e);
        }
    }
}
