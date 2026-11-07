using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using RogueElements;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Reactive.Subjects;
using Avalonia.Controls.Presenters;
using Avalonia.Input;
using Avalonia.VisualTree;
using RogueEssence.Dev.ViewModels;

namespace RogueEssence.Dev.Views
{
    public partial class PriorityListBox : UserControl
    {
        public PriorityListBox()
        {
            this.InitializeComponent();
            PriorityListBoxAddButton.AddHandler(PointerReleasedEvent, PriorityListBoxAddButton_OnPointerReleased, RoutingStrategies.Tunnel);
        }
        

     
        // public void lbxCollection_DoubleClick(object sender, PointerReleasedEventArgs e)
        // {
        //     if (!doubleclick)
        //         return;
        //     doubleclick = false;
        //
        //     ViewModels.PriorityListBoxViewModel viewModel = (ViewModels.PriorityListBoxViewModel)DataContext;
        //     if (viewModel == null)
        //         return;
        //     viewModel.lbxCollection_DoubleClick(sender, e);
        // }

        public void SetListContextMenu(ContextMenu menu)
        {
            PriorityListBoxDataGrid.ContextMenu = menu;
        }

        private void PriorityListBoxAddButton_OnPointerReleased(object sender, PointerReleasedEventArgs e)
        {
            KeyModifiers modifiers = e.KeyModifiers;
            bool advancedEdit = modifiers.HasFlag(KeyModifiers.Shift);
            PriorityListBoxViewModel vm = (PriorityListBoxViewModel) DataContext;
            vm.btnAdd_Click(advancedEdit);
        }
        
        private Priority? ParsePriority(string text)
        {
            string[] divText = text.Trim().Split('.');
            int[] divNums = new int[divText.Length];
            for (int ii = 0; ii < divText.Length; ii++)
            {
                if (!int.TryParse(divText[ii], out divNums[ii]))
                    return null;
            }
            return new Priority(divNums);
        }
        
        private void PriorityListBoxDataGrid_OnCellPointerPressed(object sender, DataGridCellPointerPressedEventArgs e)
        {
            ViewModels.PriorityListBoxViewModel viewModel = (ViewModels.PriorityListBoxViewModel)DataContext;
            if (e.PointerPressedEventArgs.ClickCount != 2) return;
            if (e.Column.DisplayIndex != 1) return;
            if (viewModel == null)
                return;
            viewModel.lbxCollection_DoubleClick(sender, e);
        }
        
        // This is a really hacky way to prevent the priority from being edited twice.
        // I think OnCellEditEnding is being called twice since it is removed and then added which focuses the textbox again and retriggers
        private bool _isEditingPriority;
        
        private void PriorityListBoxDataGrid_OnCellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (_isEditingPriority) return;
            if (e.EditAction != DataGridEditAction.Commit) return;
            if (e.Column.DisplayIndex != 0) return;
            
            var textBox = e.EditingElement as TextBox;
            if (textBox == null) return;
        
            Priority? newPriority = ParsePriority(textBox.Text);
            if (newPriority == null) return;
        
            e.Cancel = true;
        
            _isEditingPriority = true;
            PriorityListBoxViewModel viewModel = (PriorityListBoxViewModel)DataContext;
            viewModel?.ChangePriority(newPriority.Value);
            _isEditingPriority = false;
        }
    }
}
