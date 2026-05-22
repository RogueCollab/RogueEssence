using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using DynamicData;
using RogueEssence.Dev.Services;
using ReactiveUI;
using RogueEssence.Content;
using RogueEssence.Dev.Utility;
using RogueEssence.Dev.Views;
using RogueEssence.Dungeon;

namespace RogueEssence.Dev.ViewModels;

public class SpritePageViewModel : EditorPageViewModel<SpriteRootNode>
{
    
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
    
    
    public SpritePageViewModel(EditorContext context, SpriteRootNode node, Action<EditorPageViewModel> onPageOpen = null) : base(context, node, onPageOpen)
    {
        
    }
    
    public Action? OnPageRemovedAction { get; set; }

    public override void OnPageRemoved()
    {
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
        this.WhenAnyValue(x => x.SelectedItem).Subscribe(_ => Node.CachedPath = null);
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
                string[] dirs = PathMod.GetModFiles(Path.GetDirectoryName(assetPattern), String.Format(Path.GetFileName(assetPattern), "*"));
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
            await MessageBoxWindowView.Show(_context.DialogService, "Error when reindexing.\n\n" + ex.Message, "Reindex Failed", MessageBoxWindowView.MessageBoxButtons.Ok);
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
    
    
}