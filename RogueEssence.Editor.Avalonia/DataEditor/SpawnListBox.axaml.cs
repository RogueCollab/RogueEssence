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
            Button button = this.FindControl<Button>("SpawnListBoxAddButton");
            button.AddHandler(PointerReleasedEvent, SpawnListBoxAddButton_OnPointerReleased, RoutingStrategies.Tunnel);
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

        public void SetListContextMenu(ContextMenu menu)
        {
            DataGrid lbx = this.FindControl<DataGrid>("gridItems");
            lbx.ContextMenu = menu;
        }

        private void SpawnListBoxAddButton_OnPointerReleased(object sender, PointerReleasedEventArgs e)
        {
            KeyModifiers modifiers = e.KeyModifiers;
            bool advancedEdit = modifiers.HasFlag(KeyModifiers.Shift);
            SpawnListBoxViewModel vm = (SpawnListBoxViewModel) DataContext;
            vm.btnAdd_Click(advancedEdit);
        }
    }
}
