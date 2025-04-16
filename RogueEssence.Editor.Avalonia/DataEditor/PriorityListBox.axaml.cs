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
    public partial class PriorityListBox : UserControl
    {
        public PriorityListBox()
        {
            this.InitializeComponent();
            Button addButton = this.FindControl<Button>("PriorityListBoxAddButton");
            addButton.AddHandler(PointerReleasedEvent, PriorityListBoxAddButton_OnPointerReleased, RoutingStrategies.Tunnel);
            
            Button editButton = this.FindControl<Button>("PriorityListBoxEditButton");
            editButton.AddHandler(PointerReleasedEvent, PriorityListBoxEditButton_OnPointerReleased, RoutingStrategies.Tunnel);
        }
        

        bool doubleclick;
        public void doubleClickStart(object sender, RoutedEventArgs e)
        {
            doubleclick = true;
        }

        public void lbxCollection_DoubleClick(object sender, PointerReleasedEventArgs e)
        {
            if (!doubleclick)
                return;
            doubleclick = false;

            ViewModels.PriorityListBoxViewModel viewModel = (ViewModels.PriorityListBoxViewModel)DataContext;
            if (viewModel == null)
                return;
            viewModel.lbxCollection_DoubleClick(sender, e);
        }

        public void SetListContextMenu(ContextMenu menu)
        {
            DataGrid lbx = this.FindControl<DataGrid>("gridItems");
            lbx.ContextMenu = menu;
        }

        private void PriorityListBoxAddButton_OnPointerReleased(object sender, PointerReleasedEventArgs e)
        {
            KeyModifiers modifiers = e.KeyModifiers;
            bool advancedEdit = modifiers.HasFlag(KeyModifiers.Shift);
            PriorityListBoxViewModel vm = (PriorityListBoxViewModel) DataContext;
            vm.btnAdd_Click(advancedEdit);
        }

        private void PriorityListBoxEditButton_OnPointerReleased(object sender, PointerReleasedEventArgs e)
        {
            KeyModifiers modifiers = e.KeyModifiers;
            bool advancedEdit = modifiers.HasFlag(KeyModifiers.Shift);
            PriorityListBoxViewModel vm = (PriorityListBoxViewModel) DataContext;
            vm.btnEditKey_Click(advancedEdit);
        }
    }
}
