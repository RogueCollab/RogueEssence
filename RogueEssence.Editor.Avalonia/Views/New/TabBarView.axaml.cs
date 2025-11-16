using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using RogueEssence.Dev.ViewModels;

namespace RogueEssence.Dev.Views
{
    public partial class TabBarView : UserControl
    {
        public TabBarView()
        {
            InitializeComponent();
        }
        
        protected override void OnDataContextChanged(EventArgs e)
        {
            base.OnDataContextChanged(e);
            if (DataContext is MainWindowViewModel vm)
            {
                vm.TabSwitcherClosed += () => TabSwitcherFlyoutButton.Flyout?.Hide();;
            }
        }

        private async void OnCloseTab(object? sender, RoutedEventArgs e)
        {
            if (DataContext is not MainWindowViewModel vm) return;
            if (sender is not Button { DataContext: EditorPageViewModel page }) return;
            
            bool closed = await vm.TryCloseTabAsync(page);
            if (!closed)
                e.Handled = true;
        }
        
        
        private void TabSwitcherFlyout_OnOpened(object? sender, EventArgs e)
        {
            if (DataContext is MainWindowViewModel vm)
            {
                vm.OpenTabSwitcher();
            }
        }

        private void TabSwitcherFlyout_OnClosed(object? sender, EventArgs e)
        {
            if (DataContext is MainWindowViewModel vm)
            {
                vm.CloseTabSwitcher();
            }
        }
    }
}