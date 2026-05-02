using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Avalonia.VisualTree;
using RogueEssence.Dev.ViewModels;

namespace RogueEssence.Dev.Views;

public partial class TabSwitcherView : UserControl
{
    public TabSwitcherView()
    {
        InitializeComponent();

        
        
        TreeViewTabSwitcher.AddHandler(KeyDownEvent, TreeViewTabSwitcher_OnKeyDown,
            RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
        TreeViewTabSwitcher.AddHandler(PointerPressedEvent, TreeViewTabSwitcher_OnPointerPressed,
            RoutingStrategies.Tunnel);

        TreeViewTabSwitcher.AttachedToVisualTree += (_, _) =>
        {
            
            Dispatcher.UIThread.Post(() =>
            {

            
                ExpandAllNodes();
                if (DataContext is TabSwitcherViewModel vm && vm.HasTemporaryTab())
                {
                    TabSwitcherTextBox.Focus();
                    var allItems = GetAllVisibleTreeViewItems(TreeViewTabSwitcher).ToList();

                    foreach (var item in allItems)
                    {
                        item.IsSelected = false;
                    }
                }
                else
                {
                    SetFocusToSelectedPage();
                    
                }
               
            },  DispatcherPriority.Loaded);
        };
        
        PagesListBox.AttachedToVisualTree += (_, _) =>
        {
            
            Dispatcher.UIThread.Post(() =>
            {
                if (DataContext is TabSwitcherViewModel vm && vm.HasTemporaryTab())
                {
                    (DataContext as TabSwitcherViewModel).SelectedPage = null;
                    TabSwitcherTextBox.Focus();
                }
                else
                {
                    PagesListBox.Focus();
                }
            },  DispatcherPriority.Loaded);
        };
    }
    
    private void ExpandAllNodes()
    {
        if (TreeViewTabSwitcher.Items == null) return;
    
        foreach (var item in TreeViewTabSwitcher.Items)
        {
            if (item is PageNode pageNode)
            {
                ExpandAllPageNodesRecursive(pageNode);
            }
        }
    }

    private void ExpandAllPageNodesRecursive(NodeBase pageNode)
    {
        pageNode.IsExpanded = true;
    
        if (pageNode.SubNodes != null && pageNode.SubNodes.Count > 0)
        {
            foreach (var child in pageNode.SubNodes)
            {
                ExpandAllPageNodesRecursive(child);
            }
        }
    }

    private void SetFocusToSelectedPage()
    {
        if (DataContext is TabSwitcherViewModel switcher)
        {
            ExpandToPage(TreeViewTabSwitcher, switcher.SelectedPage);
            
            Dispatcher.UIThread.Post(() =>
            {
                var allItems = GetAllVisibleTreeViewItems(TreeViewTabSwitcher).ToList();

                foreach (var item in allItems)
                {
                    item.IsSelected = false;
                }

                foreach (var item in allItems)
                {
                    if ((item.DataContext as PageNode)?.Page == switcher.SelectedPage)
                    {
                        item.IsSelected = true;
                        item.Focus();
                        item.BringIntoView();
                        break;
                    }
                }
            }, DispatcherPriority.Background);
        }
    }

    private void ExpandToPage(TreeView treeView, object targetPage)
    {
        foreach (var child in treeView.GetLogicalChildren().OfType<TreeViewItem>())
        {
            if (ExpandToPageRecursive(child, targetPage))
            {
                break;
            }
        }
    }

    private bool ExpandToPageRecursive(TreeViewItem item, object targetPage)
    {
        var pageNode = item.DataContext as PageNode;
        
        if (pageNode?.Page == targetPage)
        {
            return true;
        }
        
        if (pageNode != null)
        {
            bool wasExpanded = item.IsExpanded;
            item.IsExpanded = true;
            
            item.UpdateLayout();

            foreach (var child in item.GetLogicalChildren().OfType<TreeViewItem>())
            {
                if (ExpandToPageRecursive(child, targetPage))
                {
                    return true;
                }
            }
            item.IsExpanded = wasExpanded;
        }

        return false;
    }


    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (e.Key == Key.Enter && DataContext is ViewModels.TabSwitcherViewModel switcher)
        {
            switcher.Switch();
            e.Handled = true;
        }
    }

    private void OnItemTapped(object sender, TappedEventArgs e)
    {
        if (DataContext is TabSwitcherViewModel switcher)
        {
            switcher.Switch();
            e.Handled = true;
        }
    }


    private void PagesListBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (DataContext is not TabSwitcherViewModel switcher)
            return;

        if (PagesListBox.ItemCount == 0)
            return;

        if (!switcher.IsTreeView)
        {
            if (e.Key == Key.Down)
            {
                if (PagesListBox.SelectedIndex < PagesListBox.ItemCount - 1)
                {
                    PagesListBox.SelectedIndex++;
                    PagesListBox.Focus(NavigationMethod.Directional);
                }
                else
                {
                    TabSwitcherTextBox.Focus(NavigationMethod.Directional);
                    PagesListBox.SelectedIndex = -1;
                }

                e.Handled = true;
            }
            else if (e.Key == Key.Up)
            {
                if (PagesListBox.SelectedIndex > 0)
                {
                    PagesListBox.Focus(NavigationMethod.Directional);
                    PagesListBox.SelectedIndex--;
                }
                else
                {
                    TabSwitcherTextBox.Focus(NavigationMethod.Directional);
                    PagesListBox.SelectedIndex = -1;
                }

                e.Handled = true;
            }
        }
    }

    private void TabSwitcherTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (DataContext is not TabSwitcherViewModel switcher)
            return;

        if (PagesListBox.ItemCount == 0)
            return;

        if (!switcher.IsTreeView)
        {
            if (e.Key == Key.Down)
            {
                PagesListBox.Focus(NavigationMethod.Directional);
                PagesListBox.SelectedIndex = 0;
            }
            else if (e.Key == Key.Up)
            {
                PagesListBox.Focus(NavigationMethod.Directional);
                PagesListBox.SelectedIndex = PagesListBox.ItemCount - 1;
            }
        }
        else
        {
            var allItems = GetAllVisibleTreeViewItems(TreeViewTabSwitcher).ToList();
    
            if (allItems.Count > 0)
            {
                if (e.Key == Key.Down)
                {
                    TreeViewTabSwitcher.Focus(NavigationMethod.Directional);
                    allItems[0].IsSelected = true;
                    allItems[0].Focus();
                    switcher.SelectedPage = (allItems[0].DataContext as PageNode)?.Page;
                }
                else if (e.Key == Key.Up)
                {
                    TreeViewTabSwitcher.Focus(NavigationMethod.Directional);
                    allItems[allItems.Count - 1].IsSelected = true;
                    allItems[allItems.Count - 1].Focus();
                    switcher.SelectedPage = (allItems[allItems.Count - 1].DataContext as PageNode)?.Page;
                }
            }
        }
        
    }

    private void TreeViewTabSwitcher_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is ViewModels.TabSwitcherViewModel switcher)
        {
            var toggleButton = (e.Source as Control)?.FindAncestorOfType<ToggleButton>();
            if (toggleButton != null)
            {
                return;
            }

            var treeViewItem = (e.Source as Control)?.FindAncestorOfType<TreeViewItem>();

            if (treeViewItem != null)
            {
                var dataContext = treeViewItem.DataContext as PageNode;
                switcher.SelectedPage = dataContext?.Page;
                switcher.Switch();
                e.Handled = true;
            }
        }
    }

    private void ToggleButtonOnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is TabSwitcherViewModel switcher)
        {
            switcher.ToggleSearchMode();
            SetFocusToSelectedPage();
            TabSwitcherTextBox.Focus();
        }
    }

    private void ClearFilterButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is TabSwitcherViewModel switcher)
        {
            switcher.ClearFilter();
            TabSwitcherTextBox.Focus();
        }
    }

    private void TreeViewTabSwitcher_OnKeyDown(object sender, KeyEventArgs e)
{
    if (e.Handled) return;

    if (e.Key == Key.Enter && DataContext is ViewModels.TabSwitcherViewModel switcher)
    {
        switcher.Switch();
        e.Handled = true;
        return;
    }

    var treeView = sender as TreeView;
    if (treeView == null) return;

    if (e.Key == Key.Down)
    {
        SelectNextItem(treeView);
        e.Handled = true;
    }
    else if (e.Key == Key.Up)
    {
        SelectPreviousItem(treeView);
        e.Handled = true;
    }
}

private void SelectNextItem(TreeView treeView)
{
    if (DataContext is not TabSwitcherViewModel switcher) return;

    var items = GetAllVisibleTreeViewItems(treeView).ToList();
    if (items.Count == 0) return;

    var currentItem = items.FirstOrDefault(item => item.IsSelected);
    int currentIndex = currentItem != null ? items.IndexOf(currentItem) : -1;

    if (currentIndex < items.Count - 1)
    {
        if (currentItem != null) currentItem.IsSelected = false;
        items[currentIndex + 1].IsSelected = true;
        items[currentIndex + 1].Focus();
        switcher.SelectedPage = (items[currentIndex + 1].DataContext as PageNode).Page;
    }
    else if (currentIndex == items.Count - 1)
    {
        if (currentItem != null) currentItem.IsSelected = false;
        TabSwitcherTextBox.Focus(NavigationMethod.Directional);
    }
    else if (currentIndex == -1 && items.Count > 0)
    {
        items[0].IsSelected = true;
        items[0].Focus();
        switcher.SelectedPage = (items[0].DataContext as PageNode).Page;
    }
}

private void SelectPreviousItem(TreeView treeView)
{
    if (DataContext is not TabSwitcherViewModel switcher) return;

    var items = GetAllVisibleTreeViewItems(treeView).ToList();
    if (items.Count == 0) return;

    var currentItem = items.FirstOrDefault(item => item.IsSelected);
    int currentIndex = currentItem != null ? items.IndexOf(currentItem) : -1;

    if (currentIndex > 0)
    {
        if (currentItem != null) currentItem.IsSelected = false;
        items[currentIndex - 1].IsSelected = true;
        items[currentIndex - 1].Focus();
        switcher.SelectedPage = (items[currentIndex - 1].DataContext as PageNode).Page;
    }
    else if (currentIndex == 0)
    {
        if (currentItem != null) currentItem.IsSelected = false;
        TabSwitcherTextBox.Focus(NavigationMethod.Directional);
    }
    else if (currentIndex == -1 && items.Count > 0)
    {
        items[items.Count - 1].IsSelected = true;
        items[items.Count - 1].Focus();
        switcher.SelectedPage = (items[items.Count - 1].DataContext as PageNode).Page;
    }
}
    private IEnumerable<TreeViewItem> GetAllVisibleTreeViewItems(TreeView treeView)
    {
        foreach (var child in treeView.GetLogicalChildren().OfType<TreeViewItem>())
        {
            yield return child;

            if (child.IsExpanded)
            {
                foreach (var descendant in GetVisibleTreeViewItemsRecursive(child))
                {
                    yield return descendant;
                }
            }
        }
    }
    
    private IEnumerable<TreeViewItem> GetVisibleTreeViewItemsRecursive(TreeViewItem parent)
    {
        foreach (var child in parent.GetLogicalChildren().OfType<TreeViewItem>())
        {
            yield return child;

            if (child.IsExpanded)
            {
                foreach (var descendant in GetVisibleTreeViewItemsRecursive(child))
                {
                    yield return descendant;
                }
            }
        }
    }
}