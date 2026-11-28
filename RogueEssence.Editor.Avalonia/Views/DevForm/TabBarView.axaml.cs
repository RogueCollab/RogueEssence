using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.VisualTree;
using RogueEssence.Dev.ViewModels;

public static class ControlExtensions
{

    public static Path CreateMenuIcon(this Control control, string iconKey)
    {
        if (control?.FindResource(iconKey) is StreamGeometry geo)
        {
            return new Path()
            {
                Data = geo,
                Width = 12,
                Height = 12,
                Stretch = Stretch.Uniform
            };
        }

        return null;
    }
}

namespace RogueEssence.Dev.Views
{
    public partial class TabBarView : UserControl
    {
        private ContextMenu _tabContextMenu;
        private EditorPageViewModel? _contextMenuPage;

        public TabBarView()
        {
            InitializeComponent();

            _tabContextMenu = new ContextMenu();
            TabsControl.ContextMenu = _tabContextMenu;
            TabsControl.AddHandler(InputElement.PointerPressedEvent, OnTabBarPointerPressed, RoutingStrategies.Tunnel);
        }

        private void OnTabBarPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (!e.GetCurrentPoint(this).Properties.IsRightButtonPressed) return;
        
            var current = e.Source as Visual;
            EditorPageViewModel? page = null;
        
            while (current != null)
            {
                if (current.DataContext is EditorPageViewModel vm)
                {
                    page = vm;
                    break;
                }
                current = current.GetVisualParent();
            }
        
            if (page == null) return;
            if (DataContext is not DevFormViewModel devVm) return;
        
            _contextMenuPage = page;
            BuildContextMenu(devVm, page);
            _tabContextMenu.Open(TabsControl);
            e.Handled = true;
        }

        private void BuildContextMenu(DevFormViewModel vm, EditorPageViewModel page)
        {
            _tabContextMenu.Items.Clear();

            var close = new MenuItem { Header = "Close" };
            close.InputGesture =
                new KeyGesture(Key.W, OperatingSystem.IsMacOS() ? KeyModifiers.Meta : KeyModifiers.Control);
            close.Click += (_, _) => vm.TryCloseTabAsync(page, false);
            _tabContextMenu.Items.Add(close);

            bool canLeft = vm.CanCloseLeft(page);
            bool canRight = vm.CanCloseRight(page);

            if (vm.CanCloseOthers())
            {
                var closeOthers = new MenuItem { Header = "Close Others" };
                closeOthers.Click += (_, _) => vm.CloseOthersAsync(page);
                _tabContextMenu.Items.Add(closeOthers);

                var closeAll = new MenuItem { Header = "Close All" };
                closeAll.Click += (_, _) => vm.CloseAllAsync();
                _tabContextMenu.Items.Add(closeAll);
            }

            if (canLeft && canRight)
            {
                var closeLeft = new MenuItem { Header = "Close Left" };
                closeLeft.Click += (_, _) => vm.CloseLeftAsync(page);
                _tabContextMenu.Items.Add(closeLeft);

                var closeRight = new MenuItem { Header = "Close Right" };
                closeRight.Click += (_, _) => vm.CloseRightAsync(page);
                _tabContextMenu.Items.Add(closeRight);
            }
        }

        protected override void OnDataContextChanged(EventArgs e)
        {
            base.OnDataContextChanged(e);
            if (DataContext is DevFormViewModel vm)
                vm.TabSwitcherClosed += () => TabSwitcherFlyoutButton.Flyout?.Hide();
        }

        private async void OnCloseTab(object? sender, RoutedEventArgs e)
        {
            if (DataContext is not DevFormViewModel vm) return;
            if (sender is not Button { DataContext: EditorPageViewModel page }) return;

            bool closed = await vm.TryCloseTabAsync(page, false);
            if (!closed)
                e.Handled = true;
        }

        private void TabSwitcherFlyout_OnOpened(object? sender, EventArgs e)
        {
            if (DataContext is DevFormViewModel vm)
                vm.OpenTabSwitcher();
        }

        private void TabSwitcherFlyout_OnClosed(object? sender, EventArgs e)
        {
            if (DataContext is DevFormViewModel vm)
                vm.CloseTabSwitcher();
        }


        private void TabBarViewScrollViewer_OnPointerWheelChanged(object sender, PointerWheelEventArgs e)
        {
            
            if (sender is ScrollViewer scrollViewer)
            {

                double speed = 40;

                double totalHorizontalDelta = Math.Abs(e.Delta.X) > Math.Abs(e.Delta.Y) ? e.Delta.X : -e.Delta.Y;

                double newOffset = scrollViewer.Offset.X - (totalHorizontalDelta * speed);


                double maxOffset = scrollViewer.Extent.Width - scrollViewer.Viewport.Width;
                newOffset = Math.Clamp(newOffset, 0, maxOffset);

                scrollViewer.Offset = new Vector(newOffset, scrollViewer.Offset.Y);

                e.Handled = true;
            }
        }
    }
}