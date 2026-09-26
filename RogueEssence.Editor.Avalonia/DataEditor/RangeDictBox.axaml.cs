using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Reactive.Subjects;
using Avalonia.VisualTree;
using Avalonia.Input;
using RogueEssence.Dev.ViewModels;

namespace RogueEssence.Dev.Views
{
    public partial class RangeDictBox : UserControl
    {
        public RangeDictBox()
        {
            this.InitializeComponent();
        }
        
        
        public void SetListContextMenu(ContextMenu menu)
        {
            RangeDictBoxDataGrid.ContextMenu = menu;
        }

        private void RangeDictBoxDataGrid_OnCellPointerPressed(object sender, DataGridCellPointerPressedEventArgs e)
        {
            if (e.PointerPressedEventArgs.ClickCount != 2) return;
            if (e.Column.DisplayIndex != 2) return; // Value column only

            RangeDictBoxViewModel viewModel = (RangeDictBoxViewModel)DataContext;
            if (viewModel == null) return;
            viewModel.lbxCollection_DoubleClick(sender, e);
        }

        private void RangeDictBoxDataGrid_OnCellEditEnded(object sender, DataGridCellEditEndedEventArgs e)
        {
            if (e.EditAction != DataGridEditAction.Commit) return;

            var element = (RangeDictElement)e.Row.DataContext;
            var columnIndex = e.Column.DisplayIndex;

            RangeDictBoxViewModel viewModel = (RangeDictBoxViewModel)DataContext;
            if (viewModel == null) return;

            // Start
            if (columnIndex == 0) 
                viewModel.AdjustOtherLimit(element.DisplayStart, false);
            // End
            else if (columnIndex == 1)
                viewModel.AdjustOtherLimit(element.DisplayEnd, true);
        }
    }
}
