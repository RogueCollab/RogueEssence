using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Avalonia.Platform;
using RogueEssence.Dev.ViewModels;

namespace RogueEssence.Dev.Views;

public partial class MainWindow : ChromelessWindow
{
    public static readonly StyledProperty<GridLength> CaptionHeightProperty =
        AvaloniaProperty.Register<MainWindow, GridLength>(nameof(CaptionHeight));

    public GridLength CaptionHeight
    {
        get => GetValue(CaptionHeightProperty);
        set => SetValue(CaptionHeightProperty, value);
    }

    public static readonly StyledProperty<bool> HasLeftCaptionButtonProperty =
        AvaloniaProperty.Register<MainWindow, bool>(nameof(HasLeftCaptionButton));

    public bool HasLeftCaptionButton
    {
        get => GetValue(HasLeftCaptionButtonProperty);
        set => SetValue(HasLeftCaptionButtonProperty, value);
    }

    public bool HasRightCaptionButton
    {
        get
        {
            if (OperatingSystem.IsLinux())
                return !Native.OS.UseSystemWindowFrame;

            return OperatingSystem.IsWindows();
        }
    }
    
    
    
    public MainWindow()
    {
        
        if (OperatingSystem.IsMacOS())
        {
            HasLeftCaptionButton = true;
            CaptionHeight = new GridLength(34);
            ExtendClientAreaChromeHints =
                ExtendClientAreaChromeHints.SystemChrome | ExtendClientAreaChromeHints.OSXThickTitleBar;
        }
        else if (UseSystemWindowFrame)
        {
            CaptionHeight = new GridLength(30);
        }
        else
        {
            CaptionHeight = new GridLength(38);
        }
        
        InitializeComponent();
    }
    
    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        if (DataContext is MainWindowViewModel vm)
        {
            // vm.ModSwitcherClosed += () => ModSwitcherFlyoutButton.Flyout?.Hide();;
        }
    }
    
    private void ModSwitcherFlyout_OnOpened(object? sender, EventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            vm.OnModSwitcherOpened();
        }
    }
    
    private void ModSwitcherFlyout_OnClosed(object? sender, EventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            vm.OnModSwitcherClosed();
            // ModSwitcherFlyoutButton.Flyout?.Hide();
        }
    }
    
    protected override void OnClosing(WindowClosingEventArgs e)
    {
        base.OnClosing(e);

        if (!Design.IsDesignMode && DataContext is ViewModels.MainWindowViewModel)
        {
            PreferencesWindowViewModel.Instance.Save();
        }
    }

  
 
    private void MasterTreeView_OnDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm && sender is TreeView { SelectedItem: not null } treeView)
        {
            var selectedItem = (OpenEditorNode)treeView.SelectedItem;
            vm.AddPageFromPageNode(selectedItem);
        }
    }
    
    private void MasterTreeView_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        // Deselect the item after MasterTreeView_OnDoubleTapped
        Dispatcher.UIThread.Post(() =>
        {
            if (sender is TreeView tree)
            {
                tree.SelectedItem = null;
            }
        });
    }
    
    private void MasterTreeView_OnContextRequested(object? sender, ContextRequestedEventArgs e)
    {
        if (e.Source is not Visual visual)
            return;

        var current = visual.GetSelfAndVisualAncestors().OfType<TreeViewItem>().FirstOrDefault();
        var parent = visual.GetSelfAndVisualAncestors().OfType<TreeViewItem>().Skip(1).FirstOrDefault();

        if (current == null)
            return;

        if (current.DataContext is DataItemNode itemNode &&
            parent?.DataContext is DataRootNode parentRoot)
        {
            ShowDataItemNodeMenu(current, itemNode, parentRoot, e);
        }
        
        else if (current.DataContext is DataItemNode itemNode2 &&
            parent?.DataContext is SpriteRootNode spriteRootMode)
        {
            ShowSpriteItemNodeMenu(current, itemNode2, spriteRootMode, e);
        }
        else if (current.DataContext is DataRootNode rootNode)
        {
            ShowRootNodeMenu(current, rootNode, e);
        } 
        else if (current.DataContext is SpriteRootNode spriteRoot)
        {
            ShowSpriteRootNodeMenu(current, spriteRoot, e);
            
        }
    }

    private void ShowDataItemNodeMenu(TreeViewItem current, DataItemNode node, DataRootNode parentNode,
        ContextRequestedEventArgs e)
    {
        var menu = new ContextMenu
        {
            Items =
            {
                new MenuItem { Header = "Resave as File", Command = parentNode.ResaveAsFile, CommandParameter = node, Icon = App.CreateMenuIcon("Icons.FileFill") },
                new MenuItem
                    { Header = "Resave as Patch", Command = parentNode.ResaveAsPatch, CommandParameter = node, Icon = App.CreateMenuIcon("Icons.FileTextFill") },
                new Separator(),
                new MenuItem { Header = "Edit", Icon = App.CreateMenuIcon("Icons.PencilFill") },
                new MenuItem { Header = "Delete", Command = parentNode.DeleteCommand, CommandParameter = node,  Icon = App.CreateMenuIcon("Icons.TrashFill") }
            }
        };

        AttachAndOpenMenu(current, menu, e);
    }

    private void ShowSpriteItemNodeMenu(TreeViewItem current, DataItemNode node, SpriteRootNode parentNode,
        ContextRequestedEventArgs e)
    {
        var menu = new ContextMenu
        {
            Items =
            {
                new MenuItem { Header = "Import", Command = parentNode.ImportCommand, CommandParameter = node, Icon = App.CreateMenuIcon("Icons.DownloadSimpleFill") },
                new MenuItem { Header = "Re-Import", Command = parentNode.ReImportCommand, CommandParameter = node, Icon = App.CreateMenuIcon("Icons.RepeatFill") },
                new MenuItem { Header = "Export", Command = parentNode.ExportCommand, CommandParameter = node, Icon = App.CreateMenuIcon("Icons.ExportFill") },
                new Separator(),
                new MenuItem { Header = "Delete", Command = parentNode.DeleteCommand, CommandParameter = node, Icon =  App.CreateMenuIcon("Icons.TrashFill") }
            }
        };

        AttachAndOpenMenu(current, menu, e);
    }

    private void ShowRootNodeMenu(TreeViewItem current, DataRootNode root, ContextRequestedEventArgs e)
    {
        var menu = new ContextMenu
        {
            Items =
            {
                new MenuItem { Header = "Re-Index", Command = root.ReIndexCommand, Icon = App.CreateMenuIcon("Icons.ListNumbersFill") },
                new MenuItem { Header = "Resave all as File", Command = root.ResaveAllAsFileCommand, Icon = App.CreateMenuIcon("Icons.FileFill") },
                new MenuItem { Header = "Resave all as Diff", Command = root.ResaveAllAsDiffCommand, Icon = App.CreateMenuIcon("Icons.PlusMinusFill") },
                new Separator(),
                new MenuItem { Header = "Add", Command = root.AddCommand, Icon = App.CreateMenuIcon("Icons.Plus") }
            }
        };

        AttachAndOpenMenu(current, menu, e);
    }
    
    private void ShowSpriteRootNodeMenu(TreeViewItem current, SpriteRootNode root, ContextRequestedEventArgs e)
    {
        var menu = new ContextMenu
        {
            Items =
            {
                new MenuItem { Header = "Mass Import", Command = root.MassImportCommand, Icon = App.CreateMenuIcon("Icons.DownloadSimpleFill") },
                new MenuItem { Header = "Mass Export", Command = root.MassExportCommand, Icon = App.CreateMenuIcon("Icons.ExportFill") },
                new Separator(),
                new MenuItem { Header = "Add", Command = root.AddCommand,  Icon = App.CreateMenuIcon("Icons.Plus") }
            }
        };

        AttachAndOpenMenu(current, menu, e);
    }

    private static void AttachAndOpenMenu(TreeViewItem current, ContextMenu menu, ContextRequestedEventArgs e)
    {
        menu.Closed += (_, _) => current.ContextMenu = null;
        current.ContextMenu = menu;
        menu.Open(current);
        e.Handled = true;
    }
}