using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
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
        
        SpritePagePreviewScrollViewer.AddHandler(
            Gestures.PinchEvent,
            SpritePagePreviewScrollViewer_OnPinch);
        
        SpritePagePreviewImage.AddHandler(Gestures.PinchEvent, SpritePagePreviewScrollViewer_OnPinch);
        // SpritePagePreviewImage.AddHandler(InputElement.P, (s, e) => { });
        // SpritePagePreviewImage.AddHandler(Gestures.PinchEvent, (s, e) => { Console.WriteLine("Pinch"); });
        // SpritePagePreviewImage.AddHandler(Gestures.PinchEndedEvent, (s, e) => { Console.WriteLine("PinchEnded"); });

    }
    
    private void SpritePagePreviewScrollViewer_OnPinch(object? sender, PinchEventArgs e)
    {
        Console.WriteLine("Pinch: " + e.Scale);
        // _currentScale = Math.Clamp(
        //     _currentScale * e.Scale,
        //     MinScale,
        //     MaxScale);
        //
        // SpritePagePreviewImage.Width  = PreviewBaseWidth  * _currentScale;
        // SpritePagePreviewImage.Height = PreviewBaseHeight * _currentScale;
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
        SpritePagePreviewScrollViewer.AddHandler(
            PointerWheelChangedEvent,
            SpritePagePreviewScrollViewer_OnPointerWheelChanged,
            RoutingStrategies.Tunnel); // tunnel fires before ScrollViewer's bubble handler
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


    
         
    // <!-- Size: 320x240: -->
    
    
    // <!-- Zoom 0.75x: -->
    // Mouse: 95.6015625, 190.68359375 Image: 117.1015625, 109.68359375
    // Scroll View Offset: 21.5, 0
    // Extent: 240, 180

        
    // <!-- Zoom 1x: -->
    // <!-- Mouse: 83.75, 195.1640625 Image: 158.25, 144.1640625 -->
    // <!-- Scroll View Offset: 74.5, 0 -->
    // <!-- Extent: 320, 240 -->
    //
    //                     
    // <!-- Zoom 2x -->
    // <!-- Mouse: 93.28515625, 184.52734375 Image: 331.28515625, 292.52734375 -->
    // <!-- Scroll View Offset: 238, 108 -->
    // <!-- Extent: 640, 480 -->
    //
    //                     
    // <!-- Zoom 3x -->
    // <!-- Mouse: 85.765625, 199.52734375 Image: 477.765625, 430.52734375 -->
    // <!-- Scroll View Offset: 392, 231 -->
    // <!-- Extent: 960, 720 -->
    //                     
    // <!-- Zoom 4x -->
    // <!-- Mouse: 90.7421875, 186.6796875 Image: 630.7421875, 573.6796875 -->
    // <!-- Scroll View Offset: 540, 387 -->
    // <!-- Extent: 1280, 960 -->
    // Note: there's probably a formula to figure the offset for the scrollviewer so that it stays zoomed in on the right pixel
    // private void SpritePagePreviewScrollViewer_OnPointerWheelChanged(object sender, PointerWheelEventArgs e)
    // {
    //     
    //     
    //     var sv = SpritePagePreviewScrollViewer;
    //     var vm = (SpritePageViewModel)DataContext!;
    //     // var mousePosInSv = e.GetPosition(sv);
    //     var image = SpritePagePreviewImage; // give your Image x:Name="SpritePagePreviewImage"
    //
    //
    //     if (!e.KeyModifiers.HasFlag(KeyModifiers.Control)) return;
    //
    //
    //     
    //
    //    
    //     double oldZoom = vm.ZoomLevel;
    //
    //
    //     double delta = e.Delta.Y > 0 ? 1.25 : 0.8;
    //     
    //     double rawZoom = Math.Max(oldZoom * delta, 0.25);
    //     double newZoom = Math.Clamp(Math.Round(rawZoom / 0.25, MidpointRounding.AwayFromZero) * 0.25, 0.25, 16.0);
    //     
    //
    //     
    //     vm.ZoomLevel = newZoom;
    //
    //
    //
    //     e.Handled = true;
    // }
    
    private void SpritePagePreviewScrollViewer_OnPointerWheelChanged(object sender, PointerWheelEventArgs e)
    {
        if (!e.KeyModifiers.HasFlag(KeyModifiers.Control)) return;


        SpritePageViewModel vm = DataContext as SpritePageViewModel;
        vm.ZoomLevel = GetNewValueFromScroll(e, vm.ZoomLevel, 0.25, 0.25, 10.0);
        e.Handled = true;
    }

    // private void SpritePageViewZoomTextBox_OnPointerWheelChanged(object sender, PointerWheelEventArgs e)
    // {
    //     var vm = (SpritePageViewModel)DataContext!;
    //     double oldZoom = vm.ZoomLevel;
    //     
    //     // We want the ability to scroll either horizontally or vertically... choose the one that's higher
    //     double delta = Math.Abs(e.Delta.Y) >= Math.Abs(e.Delta.X) ? e.Delta.Y : e.Delta.X;
    //
    //     // More sensitive
    //     if (Math.Abs(delta) < 0.1) return;
    //     
    //     // Step is 0.25x
    //     double step = delta > 0 ? 0.25 : -0.25;
    //
    //     // Make ensure it's between 0.25x and 10x
    //     double newZoom = Math.Clamp(oldZoom + step, 0.25, 10.0);
    //     
    //     vm.ZoomLevel = newZoom;
    //
    //     e.Handled = true;
    // }
    //
    // private void SpritePageViewFPSTextBox_OnPointerWheelChanged(object sender, PointerWheelEventArgs e)
    // {
    //    
    //     var vm = (SpritePageViewModel)DataContext!;
    //     double oldFps = vm.PreviewFps;
    //     
    //     // We want the ability to scroll either horizontally or vertically... choose the one that's higher
    //     double delta = Math.Abs(e.Delta.Y) >= Math.Abs(e.Delta.X) ? e.Delta.Y : e.Delta.X;
    //
    //     // More sensitive
    //     if (Math.Abs(delta) < 0.1) return;
    //     
    //     // Step is 0.25x
    //     int step = delta > 0 ? 1 : -1;
    //
    //     int newFps = (int)Math.Clamp(oldFps + step, 1, 60);
    //     
    //     vm.PreviewFps = newFps;
    //
    //     e.Handled = true;
    // }
    //
    
    public static double GetNewValueFromScroll(PointerWheelEventArgs e, double current, double step, double min, double max, double deltaThreshold = 0.1)
    {
        double delta = Math.Abs(e.Delta.Y) >= Math.Abs(e.Delta.X) ? e.Delta.Y : e.Delta.X;
        if (Math.Abs(delta) < deltaThreshold) return current;
        return Math.Clamp(current + (delta > 0 ? step : -step), min, max);
    }

    private void SpritePageViewZoomTextBox_OnPointerWheelChanged(object sender, PointerWheelEventArgs e)
    {
        SpritePageViewModel vm = DataContext as SpritePageViewModel;
        vm.ZoomLevel = GetNewValueFromScroll(e, vm.ZoomLevel, 0.25, 0.25, 10.0);
        e.Handled = true;
    }

    private void SpritePageViewFPSTextBox_OnPointerWheelChanged(object sender, PointerWheelEventArgs e)
    {
        SpritePageViewModel vm = DataContext as SpritePageViewModel;
        vm.PreviewFps = (int)GetNewValueFromScroll(e, vm.PreviewFps, 1, 1, 60);
        e.Handled = true;
    }
    
    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        if (DataContext is not SpritePageViewModel vm) return;

        // vm.RequestRender += () => SpritePagePreviewImage.InvalidateVisual();
    }
    

    private void SpritePageViewFrameTextBox_OnPointerWheelChanged(object sender, PointerWheelEventArgs e)
    {
        SpritePageViewModel vm = DataContext as SpritePageViewModel;

        if (vm.IsPaused)
        {
            vm.CurrentFrameDisplay = (int)GetNewValueFromScroll(e, vm.CurrentFrameDisplay, 1, 1, (double)vm.TotalFrames);
            e.Handled = true;
        }
    }
}
