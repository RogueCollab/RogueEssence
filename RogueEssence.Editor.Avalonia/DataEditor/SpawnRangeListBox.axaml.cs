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
using RogueEssence.Dev.ViewModels;

namespace RogueEssence.Dev.Views
{
    public partial class SpawnRangeListBox : UserControl
    {
        
        public SpawnRangeListBox()
        {
            this.InitializeComponent();
            SpawnRangeListBoxAddButton.AddHandler(PointerReleasedEvent, SpawnRangeListBoxAddButton_OnPointerReleased, RoutingStrategies.Tunnel);
        }
  

        // public void gridCollection_DoubleClick(object sender, PointerReleasedEventArgs e)
        // {
        //     if (!doubleclick)
        //         return;
        //     doubleclick = false;
        //
        //     ViewModels.SpawnRangeListBoxViewModel viewModel = (ViewModels.SpawnRangeListBoxViewModel)DataContext;
        //     if (viewModel == null)
        //         return;
        //     viewModel.gridCollection_DoubleClick(sender, e);
        // }

        public void nudStart_ValueChanged(object sender, NumericUpDownValueChangedEventArgs e)
        {
            ViewModels.SpawnRangeListBoxViewModel viewModel = (ViewModels.SpawnRangeListBoxViewModel)DataContext;
            viewModel.AdjustOtherLimit((int)e.NewValue, false);
        }

        public void nudEnd_ValueChanged(object sender, NumericUpDownValueChangedEventArgs e)
        {
            ViewModels.SpawnRangeListBoxViewModel viewModel = (ViewModels.SpawnRangeListBoxViewModel)DataContext;
            viewModel.AdjustOtherLimit((int)e.NewValue, true);
        }

        public void SetListContextMenu(ContextMenu menu)
        {
            SpawnRangeListDataGrid.ContextMenu = menu;
        }

        private void SpawnRangeListBoxAddButton_OnPointerReleased(object sender, PointerReleasedEventArgs e)
        {
            KeyModifiers modifiers = e.KeyModifiers;
            bool advancedEdit = modifiers.HasFlag(KeyModifiers.Shift);
            SpawnRangeListBoxViewModel vm = (SpawnRangeListBoxViewModel) DataContext;
            vm.btnAdd_Click(advancedEdit);
        }

       

        private void SpawnRangeListDataGrid_OnCellPointerPressed(object sender, DataGridCellPointerPressedEventArgs e)
        {
            if (e.PointerPressedEventArgs.ClickCount != 2) return;
            if (e.Column.DisplayIndex != 3) return;

            ViewModels.SpawnRangeListBoxViewModel viewModel = (ViewModels.SpawnRangeListBoxViewModel)DataContext;
            if (viewModel == null)
                return;
            viewModel.gridCollection_DoubleClick(sender, e);
        }

        private void SpawnRangeListDataGrid_OnCellEditEnded(object sender, DataGridCellEditEndedEventArgs e)
        {
            if (e.EditAction != DataGridEditAction.Commit) return;
    
            var element = (SpawnRangeListElement)e.Row.DataContext;
            var columnIndex = e.Column.DisplayIndex;

            ViewModels.SpawnRangeListBoxViewModel viewModel = (ViewModels.SpawnRangeListBoxViewModel)DataContext;
            if (viewModel == null) return;

            
            // Start Column
            if (columnIndex == 0)
                viewModel.AdjustOtherLimit(element.DisplayStart, false);
            
            // End Column
            else if (columnIndex == 1) 
                viewModel.AdjustOtherLimit(element.DisplayEnd, true);
        }
    }
}
