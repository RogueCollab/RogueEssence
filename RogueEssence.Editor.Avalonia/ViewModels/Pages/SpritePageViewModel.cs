using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using DynamicData;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RogueEssence.Dev.Services;
using ReactiveUI;
using RogueEssence.Content;
using RogueEssence.Dev.Utility;
using RogueEssence.Dev.Views;
using RogueEssence.Dungeon;

namespace RogueEssence.Dev.ViewModels;

public static class PreviewHelper
{
    public static Bitmap FromColors(Color[] data, int width, int height)
    {
        var bitmap = new WriteableBitmap(
            new PixelSize(width, height),
            new Vector(96, 96),
            PixelFormat.Bgra8888,
            AlphaFormat.Premul);

        using var fb = bitmap.Lock();
    
        byte[] bytes = new byte[data.Length * 4];
        for (int i = 0; i < data.Length; i++)
        {
            bytes[i * 4 + 0] = data[i].B;
            bytes[i * 4 + 1] = data[i].G;
            bytes[i * 4 + 2] = data[i].R;
            bytes[i * 4 + 3] = data[i].A;
        }

        Marshal.Copy(bytes, 0, fb.Address, bytes.Length);
        return bitmap;
    }
    public static Bitmap FromSheet(BaseSheet sheet)
    {
        Color[] data = BaseSheet.GetData(sheet);
        return FromColors(data, sheet.Width, sheet.Height);
    }

    public static Bitmap FromSheet(BaseSheet sheet, int frameX, int frameY, int width, int height)
    {
        Color[] data = BaseSheet.GetData(sheet, frameX, frameY, width, height);
        return FromColors(data, width, height);
    }
}

public class SpritePageViewModel : EditorPageViewModel<SpriteRootNode>
{
    
    // 
    
    private double _zoomLevel = 1.0;
    public double ZoomLevel
    {
        get => _zoomLevel;
        set => this.RaiseAndSetIfChanged(ref _zoomLevel, value);
    }
    
    
    public double PreviewWidth => (PreviewBitmap?.PixelSize.Width ?? 0) * ZoomLevel;
    public double PreviewHeight => (PreviewBitmap?.PixelSize.Height ?? 0) * ZoomLevel;
    
    
    private string _searchFilter = string.Empty;

    public string SearchFilter
    {
        get => _searchFilter;
        set => this.RaiseAndSetIfChanged(ref _searchFilter, value);
    }

    private string? _selectedItem;

    public string? SelectedItem
    {
        get => _selectedItem;
        set => this.RaiseAndSetIfChanged(ref _selectedItem, value);
    }


    public ObservableCollection<string> Items { get; } = new();
    public ObservableCollection<string> FilteredItems { get; } = new();

    public ObservableCollection<DataOpContainer> EditMenuItems { get; } = new();

    private void UpdateVisibleItems(string filter)
    {
        FilteredItems.Clear();
        var strategy = new BeginningTitleFilterStrategy();
        foreach (var item in Items.Where(key => strategy.Matches(key, filter)))
            FilteredItems.Add(item);
    }


    public GraphicsManager.AssetType AssetType => Node.AssetType;


    public SpritePageViewModel(EditorContext context, SpriteRootNode node,
        Action<EditorPageViewModel> onPageOpen = null) : base(context, node, onPageOpen)
    {
    }

    public Action? OnPageRemovedAction { get; set; }

    // public override void OnPageRemoved()
    // {
    //     OnPageRemovedAction?.Invoke();
    //     Node.Strategy.OnReload -= () => LoadDataEntries(AssetType);
    //     base.OnPageRemoved();
    // }
    public override void OnPageRemoved()
    {
        StopPreview();
        OnPageRemovedAction?.Invoke();
        Node.Strategy.OnReload -= () => LoadDataEntries(AssetType);
        base.OnPageRemoved();
    }
    


    public override void OnPageLoad()
    {
        if (AssetType == GraphicsManager.AssetType.Tile)
        {
            LoadDataEntriesForTiles();
        }
        else
        {
            LoadDataEntries(AssetType);
        }

        Node.Strategy.OnReload += () => LoadDataEntries(AssetType);

        this.WhenAnyValue(x => x.SearchFilter).Subscribe(UpdateVisibleItems);
        this.WhenAnyValue(x => x.SelectedItem).Subscribe(item =>
        {
            Node.CachedPath = null;
            StartPreview(item);
        });
        this.WhenAnyValue(x => x.ZoomLevel, x => x.PreviewBitmap)
            .Subscribe(_ =>
            {
                this.RaisePropertyChanged(nameof(PreviewWidth));
                this.RaisePropertyChanged(nameof(PreviewHeight));
            });
        
        this.WhenAnyValue(x => x.IsPaused).Subscribe(paused =>
        {
            if (paused)
                _animTimer?.Stop();
            else
                _animTimer?.Start();
        });
        
        this.WhenAnyValue(x => x.PreviewFps).Subscribe(fps =>
        {
            if (_animTimer != null)
                _animTimer.Interval = TimeSpan.FromSeconds(1.0 / fps);
        });
    }


    public async void mnuMassExport_Click()
    {
        await Node.MassExportAsync();
    }

    public async void mnuMassImport_Click()
    {
        await Node.MassImportAsync();
    }


    public void LoadDataEntries(GraphicsManager.AssetType assetType)
    {
        lock (GameBase.lockObj)
        {
            Dispatcher.UIThread.Post(() =>
            {
                Items.Clear();
                string assetPattern = GraphicsManager.GetPattern(assetType);
                string[] dirs = PathMod.GetModFiles(Path.GetDirectoryName(assetPattern),
                    String.Format(Path.GetFileName(assetPattern), "*"));
                for (int ii = 0; ii < dirs.Length; ii++)
                {
                    string filename = Path.GetFileNameWithoutExtension(dirs[ii]);
                    Items.Add(filename);
                }

                UpdateVisibleItems(SearchFilter);
            });
        }
    }

    public void LoadDataEntriesForTiles()
    {
        lock (GameBase.lockObj)
        {
            Dispatcher.UIThread.Post(() =>
            {
                Items.Clear();

                foreach (string name in GraphicsManager.TileIndex.Nodes.Keys)
                {
                    Items.Add(name);
                }

                UpdateVisibleItems(SearchFilter);
            });
        }
    }

    public async void btnImport_Click()
    {
        await Node.ImportAsync(Items);
    }


    public async void btnReImport_Click()
    {
        await Node.ReImportAsync();
    }

    public async void btnExport_Click()
    {
        await Node.ExportAsync(SelectedItem);
    }

    public async void btnDelete_Click()
    {
        await Node.DeleteAsync(SelectedItem);
    }

    protected override bool IsSamePage(EditorPageViewModel other)
    {
        var page = other as SpritePageViewModel;
        return AssetType == page?.AssetType;
    }


    public async void mnuReIndex_Click()
    {
        try
        {
            ReIndex();
        }
        catch (Exception ex)
        {
            DiagManager.Instance.LogError(ex, false);
            await MessageBoxWindowView.Show(_context.DialogService, "Error when reindexing.\n\n" + ex.Message,
                "Reindex Failed", MessageBoxWindowView.MessageBoxButtons.Ok);
            return;
        }
    }

    public void ReIndex()
    {
        DevForm.ExecuteOrPend(() => { tryReIndex(); });

        LoadDataEntriesForTiles();
    }


    private void tryReIndex()
    {
        lock (GameBase.lockObj)
        {
            GraphicsManager.RebuildIndices(GraphicsManager.AssetType.Tile);
            GraphicsManager.ClearCaches(GraphicsManager.AssetType.Tile);

            DiagManager.Instance.LogInfo("All files re-indexed.");
        }
    }

    public bool IsTileAsset => AssetType == GraphicsManager.AssetType.Tile;

    private Bitmap? _previewBitmap;

    public Bitmap? PreviewBitmap
    {
        get => _previewBitmap;
        set => this.RaiseAndSetIfChanged(ref _previewBitmap, value);
    }

    private DispatcherTimer? _animTimer;
    private DirSheet? _currentSheet;
    private int _currentFrame;
    
    
    
    
    private BaseSheet _currentBaseSheet;
    private BeamSheet _currentBeamSheet;
    

    private void StartPreview(string? item)
    {
        StopPreview();
        if (item == null) return;

        DevForm.ExecuteOrPend(() =>
        {
            lock (GameBase.lockObj)
            {
                try
                {
                    if (AssetType == GraphicsManager.AssetType.Tile)
                    {
                        var bmp = DevDataManager.GetTileset(item);
                        Dispatcher.UIThread.Post(() => PreviewBitmap = bmp);
                    }
                    else if (AssetType == GraphicsManager.AssetType.Beam)
                    {
                        var beamSheet = GraphicsManager.GetBeam(item);
                        Dispatcher.UIThread.Post(() => PreviewBitmap = PreviewHelper.FromSheet(beamSheet));
                    }
                    else
                    {
                        var sheet = AssetType switch
                        {
                            GraphicsManager.AssetType.Particle => GraphicsManager.GetAttackSheet(item),
                            _ => GraphicsManager.GetDirSheet(AssetType, item)
                        };
                        _currentSheet = sheet;
                        _currentFrame = 0;
                        Dispatcher.UIThread.Post(() =>
                        {
                            PreviewBitmap = PreviewHelper.FromSheet(sheet, 0, 0, sheet.TileWidth, sheet.TileHeight);
                            HasAnimation = sheet.TotalX > 1;
    
                            if (sheet.TotalX > 1)
                            {
                                _animTimer = new DispatcherTimer(DispatcherPriority.Render);
                                _animTimer.Interval = TimeSpan.FromSeconds(1.0 / PreviewFps);
                           
                                _animTimer.Tick += (_, _) =>
                                {
                                  
                                    _currentFrame = (_currentFrame + 1) % sheet.TotalX;
                                    int frameX = _currentFrame * sheet.TileWidth;
                                    PreviewBitmap = PreviewHelper.FromSheet(sheet, frameX, 0, sheet.TileWidth, sheet.TileHeight);
                                    this.RaisePropertyChanged(nameof(CurrentFrameDisplay));
                                };
                                _animTimer.Start();
                     
                            }
                        });
                    }
                }
                catch (Exception ex)
                {
                    DiagManager.Instance.LogError(ex, false);
                }
            }
        });
    }

    /*private void UpdatePreviewFrame()
    {
        if (_currentSheet == null)
        {
            Console.WriteLine("UpdatePreviewFrame: _currentSheet is null");
            return;
        }
        Console.WriteLine($"UpdatePreviewFrame: frame={_currentFrame} TotalX={_currentSheet.TotalX} TileWidth={_currentSheet.TileWidth} TileHeight={_currentSheet.TileHeight} SheetW={_currentSheet.Width} SheetH={_currentSheet.Height}");
        int frameX = (_currentFrame % _currentSheet.TotalX) * _currentSheet.TileWidth;
        int frameY = 0;
        PreviewBitmap = PreviewHelper.FromSheet(_currentSheet, frameX, frameY, _currentSheet.TileWidth, _currentSheet.TileHeight);
        Console.WriteLine($"UpdatePreviewFrame: PreviewBitmap set to {PreviewBitmap?.Size}");
    }*/
    
    private void UpdatePreviewFrame()
    {
        if (_currentSheet == null)
        {
            Console.WriteLine("UpdatePreviewFrame: _currentSheet is null");
            return;
        }
        Console.WriteLine($"UpdatePreviewFrame: frame={_currentFrame}");
        int frameX = (_currentFrame % _currentSheet.TotalX) * _currentSheet.TileWidth;
        PreviewBitmap = PreviewHelper.FromSheet(_currentSheet, frameX, 0, _currentSheet.TileWidth, _currentSheet.TileHeight);
    }

    private void StopPreview()
    {
        _animTimer?.Stop();
        _animTimer = null;
        _currentSheet = null;
        _currentFrame = 0;
    }
    
    public override void OnPageActivated()
    {
        ResumePreview();
    }

    public override void OnPageDeactivated()
    {
        PausePreview();
    }
    
    private double _previewFps = 12;
    public double PreviewFps
    {
        get => _previewFps;
        set => this.RaiseAndSetIfChanged(ref _previewFps, value);
    }
    
    private bool _isPaused;
    public bool IsPaused
    {
        get => _isPaused;
        set => this.RaiseAndSetIfChanged(ref _isPaused, value);
    }

    private bool _hasAnimation;
    public bool HasAnimation
    {
        get => _hasAnimation;
        set => this.RaiseAndSetIfChanged(ref _hasAnimation, value);
    }
  
    public void TogglePreview()
    {
        IsPaused = !IsPaused;
    }
    
    public void ResumePreview() => IsPaused = false;

    public void PausePreview() => IsPaused = true;
    
    public string CurrentFrameDisplay => _currentSheet != null 
        ? $"Frame: {_currentFrame + 1} / {_currentSheet.TotalX}" 
        : "Frame: -";
    
}