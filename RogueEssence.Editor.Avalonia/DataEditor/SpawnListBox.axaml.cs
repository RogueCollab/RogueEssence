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
    public partial class SpawnListBox : UserControl
    {
        
        public SpawnListBox()
        {
            this.InitializeComponent();
            SpawnListBoxAddButton.AddHandler(PointerReleasedEvent, SpawnListBoxAddButton_OnPointerReleased, RoutingStrategies.Tunnel);
        }
        
        public void SetListContextMenu(ContextMenu menu)
        {
            gridItems.ContextMenu = menu;
        }

        private void SpawnListBoxAddButton_OnPointerReleased(object sender, PointerReleasedEventArgs e)
        {
            KeyModifiers modifiers = e.KeyModifiers;
            bool advancedEdit = modifiers.HasFlag(KeyModifiers.Shift);
            SpawnListBoxViewModel vm = (SpawnListBoxViewModel) DataContext;
            vm.btnAdd_Click(advancedEdit);
        }

        private void SpawnListBoxDataGrid_OnCellEditEnded(object sender, DataGridCellEditEndedEventArgs e)
        {
            if (e.EditAction != DataGridEditAction.Commit) return;
    
            var element = (SpawnListElement)e.Row.DataContext;
            var columnIndex = e.Column.DisplayIndex;

            ViewModels.SpawnListBoxViewModel viewModel = (ViewModels.SpawnListBoxViewModel)DataContext;
            if (viewModel == null) return;
            viewModel.CurrentWeight = element.Weight;
        }

        private void SpawnListBoxDataGrid_OnCellPointerPressed(object sender, DataGridCellPointerPressedEventArgs e)
        {
            if (e.PointerPressedEventArgs.ClickCount != 2) return;
            if (e.Column.DisplayIndex != 2) return;

            ViewModels.SpawnListBoxViewModel viewModel = (ViewModels.SpawnListBoxViewModel)DataContext;
            if (viewModel == null)
                return;
            viewModel.gridCollection_DoubleClick(sender, e);
        }
    }
}
