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
    public class SpawnRangeListBox : UserControl
    {
        
        public SpawnRangeListBox()
        {
            this.InitializeComponent();
            Button button = this.FindControl<Button>("SpawnRangeListBoxAddButton");
            button.AddHandler(PointerReleasedEvent, SpawnRangeListBoxAddButton_OnPointerReleased, RoutingStrategies.Tunnel);
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
            viewModel.AdjustOtherLimit((int)e.NewValue, false);
        }

        public void nudEnd_ValueChanged(object sender, NumericUpDownValueChangedEventArgs e)
        {
            ViewModels.SpawnRangeListBoxViewModel viewModel = (ViewModels.SpawnRangeListBoxViewModel)DataContext;
            viewModel.AdjustOtherLimit((int)e.NewValue, true);
        }

        public void SetListContextMenu(ContextMenu menu)
        {
            DataGrid lbx = this.FindControl<DataGrid>("gridItems");
            lbx.ContextMenu = menu;
        }

        private void SpawnRangeListBoxAddButton_OnPointerReleased(object sender, PointerReleasedEventArgs e)
        {
            KeyModifiers modifiers = e.KeyModifiers;
            bool advancedEdit = modifiers.HasFlag(KeyModifiers.Shift);
            SpawnRangeListBoxViewModel vm = (SpawnRangeListBoxViewModel) DataContext;
            vm.btnAdd_Click(advancedEdit);
        }
    }
}
