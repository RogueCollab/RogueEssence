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
    public partial class DictionaryBox : UserControl
    {
        public DictionaryBox()
        {
            this.InitializeComponent();
            Button button = this.FindControl<Button>("DictionaryBoxAddButton");
            button.AddHandler(PointerReleasedEvent, DictionaryBoxAddButton_OnPointerReleased, RoutingStrategies.Tunnel);
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

            ViewModels.DictionaryBoxViewModel viewModel = (ViewModels.DictionaryBoxViewModel)DataContext;
            if (viewModel == null)
                return;
            viewModel.lbxCollection_DoubleClick(sender, e);
        }

        public void SetListContextMenu(ContextMenu menu)
        {
            DataGrid lbx = this.FindControl<DataGrid>("gridItems");
            lbx.ContextMenu = menu;
        }

        private void DictionaryBoxAddButton_OnPointerReleased(object sender, PointerReleasedEventArgs e)
        {
            KeyModifiers modifiers = e.KeyModifiers;
            bool advancedEdit = modifiers.HasFlag(KeyModifiers.Shift);
            DictionaryBoxViewModel vm = (DictionaryBoxViewModel) DataContext;
            vm.btnAdd_Click(advancedEdit);
        }
    }
}
