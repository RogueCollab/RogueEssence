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
        SpritePagePreviewScrollViewer.AddHandler(Gestures.PinchEvent, OnPinchUpdated);
        
        SpritePagePreviewImage.AddHandler(Gestures.PinchEvent, (s, e) => { Console.WriteLine("Pinch"); });
        SpritePagePreviewImage.AddHandler(Gestures.PinchEndedEvent, (s, e) => { Console.WriteLine("PinchEnded"); });

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
    private void OnPinchUpdated(object? sender, PinchEventArgs e)
    {
        Console.WriteLine("Pinch: " + e.Scale);
        // var vm = DataContext as SpritePageViewModel;
        // if (vm == null) return;
        //
        // if (e.Scale == 1)
        //     _pinchStartZoom = vm.ZoomLevel;
        //
        // vm.ZoomLevel = Math.Clamp(_pinchStartZoom * e.Scale, 0.25, 8);
    }
    

    
}