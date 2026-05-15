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



public interface ISpriteItemOperationStrategy
{
    Task<string?> ImportAsync(ObservableCollection<string> items);
    Task ReImportAsync(string cachedPath);
    Task DeleteAsync(string path);
    Task ExportAsync(string path);
}

public class SpriteItemAssetTypeStrategy : ISpriteItemOperationStrategy
{
    private readonly IDialogService _dialogService;
    private readonly GraphicsManager.AssetType _assetType;
    private readonly Action _onReload;


    public async Task<string?> ImportAsync(ObservableCollection<string> items)
    {
        string name = _assetType.ToString();
        string folderName = DevForm.GetConfig(name + "Dir", Directory.GetCurrentDirectory());
        var options = new FilePickerOpenOptions
        {
            Title = "Open .png or .xml File",
            AllowMultiple = false,
            FileTypeFilter =
            [
                new FilePickerFileType("PNG Files") { Patterns = ["*.png"] },
                new FilePickerFileType("DirData XML") { Patterns = ["*.xml"] }
            ]
        };

        string? result = null;
        await Dispatcher.UIThread.InvokeAsync((Func<Task>)(async () =>
        {
            var filePath = await _dialogService.ShowFilePickerAsync(options, folderName);

            if (filePath == null)
                return;

            string animName = Path.GetFileNameWithoutExtension(filePath);

            if (items.Any(item => item == animName))
            {
                MessageBoxWindowView.MessageBoxResult res = await MessageBoxWindowView.Show(_dialogService, "Are you sure you want to overwrite the existing sheet:\n" + animName, "Sprite Sheet already exists.", MessageBoxWindowView.MessageBoxButtons.YesNo);
                if (res == MessageBoxWindowView.MessageBoxResult.No)
                    return;
            }
            else
            {
                items.Add(animName);
            }

            DevForm.SetConfig(name + "Dir", Path.GetDirectoryName(filePath));
            result = Path.GetExtension(filePath) == ".xml"
                ? Path.GetDirectoryName(filePath)
                : filePath;

            try
            {
                _import(result);
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex, false);
                await MessageBoxWindowView.Show(_dialogService, "Error importing from\n" + result + "\n\n" + ex.Message, "Import Failed", MessageBoxWindowView.MessageBoxButtons.Ok);
                result = null;
            }
        }));

        return result;
    }
    
    
    public async Task DeleteAsync(string key)
    {
        var res = await MessageBoxWindowView.Show(_dialogService, $"Delete sprite '{key}'?", "Deleting Sprite", MessageBoxWindowView.MessageBoxButtons.YesNo, true);
        
        if (res == MessageBoxWindowView.MessageBoxResult.Yes)
        {
            lock (GameBase.lockObj)
            {
                string anim = key;
                string animPath = PathMod.ModPath(String.Format(GraphicsManager.GetPattern(_assetType), anim));
                if (File.Exists(animPath))
                    File.Delete(animPath);

                GraphicsManager.RebuildIndices(_assetType);
                GraphicsManager.ClearCaches(_assetType);

                DiagManager.Instance.LogInfo("Deleted frames for:" + anim);
                _onReload?.Invoke();
            }
            
        }
    }
    
    public async Task ExportAsync(string key)
    {
        string name = _assetType.ToString();
        string folderName = DevForm.GetConfig(name + "Dir", Directory.GetCurrentDirectory());
        var options = new FilePickerSaveOptions
        {
            Title = $"Export {key} as PNG",
            DefaultExtension = "png",
            FileTypeChoices =
            [
                new FilePickerFileType("PNG Files")
                {
                    Patterns = new[] { "*.png" }
                }
            ]
        };

        await Dispatcher.UIThread.InvokeAsync((Func<Task>)(async () =>
        {

            var filePath = await _dialogService.TryGetSaveFileAsync(options, folderName);
            if (!String.IsNullOrEmpty(filePath))
            {
                DevForm.SetConfig(name + "Dir", Path.GetDirectoryName(filePath));

                try
                {
                    DevForm.ExecuteOrPend(() => { _export(filePath, key); });
                }
                catch (Exception ex)
                {
                    DiagManager.Instance.LogError(ex, false);


                    await MessageBoxWindowView.Show(_dialogService, $"Error exporting to\n{filePath}\n\n{ex.Message}", "Export Failed", MessageBoxWindowView.MessageBoxButtons.Ok);
                }
            }
        }));

    }
    
    public async Task ReImportAsync(string cachedPatg)
    {
        await Dispatcher.UIThread.InvokeAsync((Func<Task>)(async () =>
        {
            try
            {
                _import(cachedPatg);
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex, false);
                await MessageBoxWindowView.Show(_dialogService, "Error importing from\n" + cachedPatg + "\n\n" + ex.Message, "Import Failed", MessageBoxWindowView.MessageBoxButtons.Ok);
                return;
            }

        }));
    }

    private void _export(string currentPath, string anim)
    {
        lock (GameBase.lockObj)
        {
            string animPath = PathMod.ModPath(String.Format(GraphicsManager.GetPattern(_assetType), anim));
            if (File.Exists(animPath))
            {
                //read file and read binary data
                using (FileStream fileStream = File.OpenRead(animPath))
                {
                    using (BinaryReader reader = new BinaryReader(fileStream))
                    {
                        DirSheet sheet = DirSheet.Load(reader);

                        string filename = DirSheet.GetExportString(sheet, Path.GetFileNameWithoutExtension(currentPath));
                        string dirname = Path.GetDirectoryName(currentPath);
                        DirSheet.Export(sheet, Path.Combine(dirname, filename + ".png"));
                    }
                }
            }

            DiagManager.Instance.LogInfo("Frames from:\n" +
                                         anim +
                                         "\nhave been exported to:" + currentPath);
        }
    }
    
    
    private void _import(string currentPath)
    {
        DevForm.ExecuteOrPend(() => { _tryImport(currentPath); });

        //recompute
        _onReload?.Invoke();
    }

    private void _tryImport(string currentPath)
    {
        lock (GameBase.lockObj)
        {
            string assetPattern = GraphicsManager.GetPattern(_assetType);
            string destFile;
            string animName = Path.GetFileNameWithoutExtension(currentPath);
            if (Directory.Exists(currentPath))
                destFile = PathMod.HardMod(String.Format(assetPattern, animName));
            else
            {
                string[] components = animName.Split('.');
                if (components.Length != 2)
                    throw new ArgumentException(
                        "The input filename does not fit the convention of \"<Anim Name>.<Anim Type>.png\"!");
                destFile = PathMod.HardMod(String.Format(assetPattern, components[0]));
            }

            if (!Directory.Exists(Path.GetDirectoryName(destFile)))
                Directory.CreateDirectory(Path.GetDirectoryName(destFile));

            //write sprite data
            using (DirSheet sheet = DirSheet.Import(currentPath))
            {
                using (FileStream stream = File.OpenWrite(destFile))
                {
                    //save data
                    using (BinaryWriter writer = new BinaryWriter(stream))
                        sheet.Save(writer);
                }
            }

            GraphicsManager.RebuildIndices(_assetType);
            GraphicsManager.ClearCaches(_assetType);

            DiagManager.Instance.LogInfo("Frames from:\n" +
                                         currentPath + "\nhave been imported.");
        }
    }

    public SpriteItemAssetTypeStrategy(IDialogService dialogService, GraphicsManager.AssetType assetType, Action onReload)
    {
        _dialogService = dialogService;
        _assetType = assetType;
        _onReload = onReload;
    }
}





public class SpriteItemTileStrategy : ISpriteItemOperationStrategy
{
    private int _cachedSize;

    private readonly IDialogService _dialogService;
    
    private Action _onReload;
    public SpriteItemTileStrategy(IDialogService dialogService, Action onReload)
    {
        _dialogService = dialogService;
        _onReload = onReload;
    }
    
    public async Task DeleteAsync(string key)
    {
        var res = await MessageBoxWindowView.Show(_dialogService, $"Delete sprite '{key}'?", "Deleting Sprite",
            MessageBoxWindowView.MessageBoxButtons.YesNo, true);

        if (res == MessageBoxWindowView.MessageBoxResult.Yes)
        {
            lock (GameBase.lockObj)
            {
                string anim = key;

                string animPath = PathMod.ModPath(String.Format(GraphicsManager.TILE_PATTERN, anim));
                if (File.Exists(animPath))
                    File.Delete(animPath);

                GraphicsManager.RebuildIndices(GraphicsManager.AssetType.Tile);
                GraphicsManager.ClearCaches(GraphicsManager.AssetType.Tile);

                DiagManager.Instance.LogInfo("Deleted frames for:" + anim);
                _onReload?.Invoke();
            }
        }
    }
    
    public async Task ExportAsync(string key)
    {
        //get current sprite
        string animData = key;

        //remember addresses in registry
        string folderName = DevForm.GetConfig("TilesetDir", Directory.GetCurrentDirectory());

        var options = new FilePickerSaveOptions
        {
            Title = "Export PNG",
            DefaultExtension = "png",
            FileTypeChoices =
            [
                new FilePickerFileType("PNG Files")
                {
                    Patterns = new[] { "*.png" }
                }
            ]
        };
        var folder = await _dialogService.TryGetSaveFileAsync(options, folderName);
        if (!String.IsNullOrEmpty(folder))
        {
            DevForm.SetConfig("TilesetDir", Path.GetDirectoryName(folder));

            try
            {
                DevForm.ExecuteOrPend(() => { _export(folder, animData); });
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex, false);
                await MessageBoxWindowView.Show(_dialogService,
                    "Error exporting to\n" + folderName + "\n\n" + ex.Message, "Export Failed",
                    MessageBoxWindowView.MessageBoxButtons.Ok);
                return;
            }
        }
    }
    
    
    public async Task<string?> ImportAsync(ObservableCollection<string> items)
    {
        //remember addresses in registry
        string folderName = DevForm.GetConfig("TilesetDir", Directory.GetCurrentDirectory());

        var options = new FilePickerOpenOptions
        {
            Title = "Open .png File",
            AllowMultiple = false,
            FileTypeFilter =
            [
                new FilePickerFileType("PNG Files") { Patterns = ["*.PNG"] }
            ]
        };

        var file = await _dialogService.ShowFilePickerAsync(options, folderName);
        if (file == null) return null;

        string animName = Path.GetFileNameWithoutExtension(file);

        if (items.Any(item => item == animName))
        {
            MessageBoxWindowView.MessageBoxResult result = await MessageBoxWindowView.Show(_dialogService,
                "Are you sure you want to overwrite the existing sheet:\n" + animName,
                "Sprite Sheet already exists.",
                MessageBoxWindowView.MessageBoxButtons.YesNo);
            if (result == MessageBoxWindowView.MessageBoxResult.No)
                return null;
        }
        else
        {
            items.Add(animName);
        }

        MapRetileViewModel viewModel = new MapRetileViewModel(GraphicsManager.TileSize, "Tile size must be divisible by 8.");
        bool sizeResult = await _dialogService.ShowDialogAsync<MapRetileViewModel, bool>(viewModel, "");

        int size = viewModel.TileSize;

        if (!sizeResult || size == 0)
            return null;

        //open window to choose directory
        DevForm.SetConfig("TilesetDir", Path.GetDirectoryName(file));
        _cachedSize = size;

        try
        {
            _import(file, _cachedSize);
            return file;
        }
        catch (Exception ex)
        {
            DiagManager.Instance.LogError(ex, false);
            await MessageBoxWindowView.Show(_dialogService,
                "Error importing from\n" + file + "\n\n" + ex.Message,
                "Import Failed", MessageBoxWindowView.MessageBoxButtons.Ok);
            return null;
        }
    }
    
    public async Task ReImportAsync(string cachedPath)
    {
        try
        {
            _import(cachedPath, _cachedSize);
        }
        catch (Exception ex)
        {
            DiagManager.Instance.LogError(ex, false);
            await MessageBoxWindowView.Show(_dialogService,
                "Error importing from\n" + cachedPath + "\n\n" + ex.Message, "Import Failed",
                MessageBoxWindowView.MessageBoxButtons.Ok);
            return;
        }
    }
    
    private void _import(string currentPath, int tileSize)
    {
        DevForm.ExecuteOrPend(() => { _tryImport(currentPath, tileSize); });

        //recompute
        _onReload?.Invoke();
    }

    
    private void _tryImport(string currentPath, int tileSize)
    {
        lock (GameBase.lockObj)
        {
            string sheetName = Path.GetFileNameWithoutExtension(currentPath);
            string outputFile = PathMod.HardMod(String.Format(GraphicsManager.TILE_PATTERN, sheetName));

            if (!Directory.Exists(Path.GetDirectoryName(outputFile)))
                Directory.CreateDirectory(Path.GetDirectoryName(outputFile));

            //load into tilesets
            using (BaseSheet tileset = BaseSheet.Import(currentPath))
            {
                List<BaseSheet[]> tileList = new List<BaseSheet[]>();
                tileList.Add(new BaseSheet[] { tileset });
                ImportHelper.SaveTileSheet(tileList, outputFile, tileSize);
            }

            GraphicsManager.RebuildIndices(GraphicsManager.AssetType.Tile);
            GraphicsManager.ClearCaches(GraphicsManager.AssetType.Tile);
            DevDataManager.ClearCaches();

            DiagManager.Instance.LogInfo("Tiles from:\n" +
                                         currentPath + "\nhave been imported.");
        }
    }

    
    private void _export(string currentPath, string anim)
    {
        lock (GameBase.lockObj)
        {
            string animPath = PathMod.ModPath(String.Format(GraphicsManager.TILE_PATTERN, anim));
            ImportHelper.ExportTileSheet(animPath, currentPath);

            DiagManager.Instance.LogInfo("Frames from:\n" +
                                         anim + "\nhave been exported to:" + currentPath);
        }
    }
    
    


}



public class SpritePageViewModel : EditorPageViewModel<SpriteRootNode>
{
    
    private string _searchFilter = string.Empty;

    public string SearchFilter
    {
        get => _searchFilter;
        set => this.RaiseAndSetIfChanged(ref _searchFilter, value);
    }
    
    // private List<string> anims;
    // public SearchListBoxViewModel Anims { get; set; }

    private string cachedPath;
    public string CachedPath
    {
        get => cachedPath;
        set => this.RaiseAndSetIfChanged(ref cachedPath, value);
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
    
    public string Name { get { return AssetType.ToString(); } }

    private void UpdateVisibleItems(string filter)
    {
        FilteredItems.Clear();
        var strategy = new BeginningTitleFilterStrategy();
        foreach (var item in Items.Where(key => strategy.Matches(key, filter)))
            FilteredItems.Add(item);
    }

    
    public GraphicsManager.AssetType AssetType => Node.AssetType;
    
    private ISpriteItemOperationStrategy _strategy;
    
    public SpritePageViewModel(EditorContext context, SpriteRootNode node, Action<EditorPageViewModel> onPageOpen = null) : base(context, node, onPageOpen)
    {

        if (AssetType == GraphicsManager.AssetType.Tile)
        {
            _strategy = new SpriteItemTileStrategy(context.DialogService, LoadDataEntriesForTiles);
        }
        else
        {
            _strategy = new SpriteItemAssetTypeStrategy(context.DialogService, AssetType,
                () => LoadDataEntries(AssetType));
        }
    }
    
    public Action? OnPageRemovedAction { get; set; }

    public override void OnPageRemoved()
    {
        OnPageRemovedAction?.Invoke();
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
        
      
        this.WhenAnyValue(x => x.SearchFilter).Subscribe(UpdateVisibleItems);
        this.WhenAnyValue(x => x.SelectedItem).Subscribe(_ => CachedPath = null);
        CachedPath = null;
        // EditMenuItems.Add(new DataOpContainer("Re-Index", ReIndexAsync));
        // if (DataType != DataManager.DataType.AutoTile)
        // {
        //     EditMenuItems.Add(new DataOpContainer("Resave all as File", () => ResaveAllAsync(false)));
        //     EditMenuItems.Add(new DataOpContainer("Resave all as Diff", () => ResaveAllAsync(true)));
        // }
        // else
        // {
        //     EditMenuItems.Add(new DataOpContainer("Import DTEF", ImportDtefAsync));
        //     // EditMenuItems.Add(new DataOpContainer("Export as DTEF", ExportDtefAsync));
        // }
    }
    
    // private void Delete(int animIdx)
    // {
    //     lock (GameBase.lockObj)
    //     {
    //         string anim = anims[animIdx];
    //         string animPath = PathMod.ModPath(String.Format(GraphicsManager.GetPattern(AssetType), anim));
    //         if (File.Exists(animPath))
    //             File.Delete(animPath);
    //
    //         GraphicsManager.RebuildIndices(AssetType);
    //         GraphicsManager.ClearCaches(AssetType);
    //
    //         DiagManager.Instance.LogInfo("Deleted frames for:" + anim);
    //
    //         anims.RemoveAt(animIdx);
    //         Anims.RemoveInternalAt(animIdx);
    //     }
    // }
    
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
        var path = await _strategy.ImportAsync(Items);
        if (path != null)
            CachedPath = path;
    }
    

    public async void btnReImport_Click()
    {
        await _strategy.ReImportAsync(CachedPath);
    }
    
    public async void btnExport_Click()
    {
        await _strategy.ExportAsync(SelectedItem);
    }
    
    public async void btnDelete_Click()
    {
        await _strategy.DeleteAsync(SelectedItem);
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