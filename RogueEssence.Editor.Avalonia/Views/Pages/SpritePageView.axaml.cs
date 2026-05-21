using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using RogueEssence.Content;
using RogueEssence.Dev.ViewModels;
using RogueEssence.Dungeon;

namespace RogueEssence.Dev.Views;

public partial class SpritePageView : UserControl
{
    public SpritePageView()
    {
        InitializeComponent();
    }
    
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        if (DataContext is SpritePageViewModel vm)
        {
            vm.OnPageRemovedAction = () =>
            {
                lock (GameBase.lockObj)
                {
                    if (DungeonScene.Instance != null && vm.AssetType == DungeonScene.Instance.DebugAsset)
                    {
                        DungeonScene.Instance.DebugAsset = GraphicsManager.AssetType.None;
                        DungeonScene.Instance.DebugAnim = null;
                    }
                }
            };
        }
    }
    

    private void SpritePageListBox_OnContextRequested(object sender, ContextRequestedEventArgs e)
    {
        // if (e.Source is not Visual)
        //     return;
        //
        // if (sender is not ListBox listBox || listBox.SelectedItem is not DataListEntry entry ||
        //     listBox.DataContext is not DataListPageViewModel vm)
        //     return;
        //
        // var contextMenu = ContextMenuHelper.CreateDataItemMenu(vm.Node, entry.Key);
        // contextMenu.Open(this);
        //
    }
    
    private void SpritePageListBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is not ListBox list)
            return;

        if (list.SelectedItem is not string entry || DataContext is not SpritePageViewModel vm)
        {
            lock (GameBase.lockObj)
            {
                if (DungeonScene.Instance != null)
                {
                    DungeonScene.Instance.DebugAsset = GraphicsManager.AssetType.None;
                    DungeonScene.Instance.DebugAnim = null;
                }
            }
            return;
        }

        if (DungeonScene.Instance == null)
            return;

        GraphicsManager.AssetType debugAsset = vm.AssetType;
        string? debugAnim = null;

        if (debugAsset.IsAnimEdit())
            debugAnim = entry;

        lock (GameBase.lockObj)
        {
            DungeonScene.Instance.DebugAsset = debugAsset;
            DungeonScene.Instance.DebugAnim = debugAnim;
        }
    }
}