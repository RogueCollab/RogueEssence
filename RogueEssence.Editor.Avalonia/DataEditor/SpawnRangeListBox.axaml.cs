using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using RogueElements;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Reactive.Subjects;

namespace RogueEssence.Dev.Views
{
    public class SpawnRangeListBox : UserControl
    {
        
        public SpawnRangeListBox()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public void gridCollection_DoubleClick(object sender, RoutedEventArgs e)
        {
            ViewModels.SpawnRangeListBoxViewModel viewModel = (ViewModels.SpawnRangeListBoxViewModel)DataContext;
            if (viewModel == null)
                return;
            viewModel.gridCollection_DoubleClick(sender, e);
        }
    }
}
