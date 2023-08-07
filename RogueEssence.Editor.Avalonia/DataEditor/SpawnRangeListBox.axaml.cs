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

            ViewModels.SpawnRangeListBoxViewModel viewModel = (ViewModels.SpawnRangeListBoxViewModel)DataContext;
            if (viewModel == null)
                return;
            viewModel.gridCollection_DoubleClick(sender, e);
        }

        public void nudStart_ValueChanged(object sender, NumericUpDownValueChangedEventArgs e)
        {
            ViewModels.SpawnRangeListBoxViewModel viewModel = (ViewModels.SpawnRangeListBoxViewModel)DataContext;
            if (viewModel.CurrentEnd < e.NewValue)
                viewModel.CurrentEnd = (int)e.NewValue;
        }

        public void nudEnd_ValueChanged(object sender, NumericUpDownValueChangedEventArgs e)
        {
            ViewModels.SpawnRangeListBoxViewModel viewModel = (ViewModels.SpawnRangeListBoxViewModel)DataContext;
            if (viewModel.CurrentStart > e.NewValue)
                viewModel.CurrentStart = (int)e.NewValue;
        }

        public void SetListContextMenu(ContextMenu menu)
        {
            DataGrid lbx = this.FindControl<DataGrid>("gridItems");
            lbx.ContextMenu = menu;
        }
    }
}
