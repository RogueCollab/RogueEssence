using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using DynamicData;
using ReactiveUI;
using RogueEssence.Content;
using RogueEssence.Data;
using RogueEssence.Dev.Services;
using RogueEssence.Dev.Utility;
using RogueEssence.Dev.Views;
using RogueEssence.Script;

namespace RogueEssence.Dev.ViewModels;

public record DataListEntry(string Key, string Value)
{
    public string Title => $"{Key}: {Value}";
}



public class DataListPageViewModel : EditorPageViewModel<DataRootNode>
{
    private string _searchFilter = string.Empty;

    public string SearchFilter
    {
        get => _searchFilter;
        set => this.RaiseAndSetIfChanged(ref _searchFilter, value);
    }


    private DataListEntry? _selectedItem;

    public DataListEntry? SelectedItem
    {
        get => _selectedItem;
        set => this.RaiseAndSetIfChanged(ref _selectedItem, value);
    }


    public ObservableCollection<DataListEntry> Items { get; } = new();
    public ObservableCollection<DataListEntry> FilteredItems { get; } = new();

    public ObservableCollection<DataOpContainer> EditMenuItems { get; } = new();

    public DataManager.DataType DataType => Node.DataType;


    public DataListPageViewModel(EditorContext context, NodeBase node, Action<EditorPageViewModel> onPageOpen = null) : base(context, node, onPageOpen)
    {
    }


    private void ReloadEntries()
    {
        var entries = DataManager.Instance.DataIndices[DataType].GetLocalStringArray(true);
        Dispatcher.UIThread.Post(() =>
        {
            Items.Clear();
            foreach (var (key, value) in entries)
                Items.Add(new DataListEntry(key, value));
            UpdateVisibleItems(SearchFilter);
        });
    }


    public override void OnPageLoad()
    {
        base.OnPageLoad();
        ReloadEntries();
        Node.OnReload += ReloadEntries;

        // ResaveAsFileCommand = Node.ResaveAsFile;

        this.WhenAnyValue(x => x.SearchFilter).Subscribe(UpdateVisibleItems);

        EditMenuItems.Clear();

        EditMenuItems.Add(new DataOpContainer("Re-Index", Node.ReIndexAsync));
        if (DataType != DataManager.DataType.AutoTile)
        {
            EditMenuItems.Add(new DataOpContainer("Resave all as File", () => Node.ResaveAllAsync(false)));
            EditMenuItems.Add(new DataOpContainer("Resave all as Diff", () => Node.ResaveAllAsync(true)));
        }
        else
        {
            EditMenuItems.Add(new DataOpContainer("Import DTEF", ImportDtefAsync));
            // EditMenuItems.Add(new DataOpContainer("Export as DTEF", ExportDtefAsync));
        }
    }

    private void tryImportDtef(string folder, string animName)
    {
        lock (GameBase.lockObj)
        {
            string destFile = PathMod.HardMod(string.Format(Content.GraphicsManager.TILE_PATTERN, animName));
            DtefImportHelper.ImportDtef(folder, destFile);

            //reindex graphics
            GraphicsManager.RebuildIndices(GraphicsManager.AssetType.Tile);
            GraphicsManager.ClearCaches(GraphicsManager.AssetType.Tile);
            DevDataManager.ClearCaches();

            //reindex data
            DevHelper.RunIndexing(DataManager.DataType.AutoTile);
            DevHelper.RunExtraIndexing(DataManager.DataType.AutoTile);
            DataManager.Instance.LoadIndex(DataManager.DataType.AutoTile);
            DataManager.Instance.LoadUniversalIndices();
            DataManager.Instance.ClearCache(DataManager.DataType.AutoTile);
            DiagManager.Instance.DevEditor.ReloadData(DataManager.DataType.AutoTile);
            ReloadEntries();
        }
    }

    private async Task ImportDtefAsync()
    {
        //remember addresses in registry
        string folderName = DevForm.GetConfig("TilesetDir", Directory.GetCurrentDirectory());

        string? folder = await _context.DialogService.ShowFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Select DTEF folder",
            AllowMultiple = false,
        }, folderName);

        if (folder is null)
            return;

        string animName = Path.GetFileNameWithoutExtension(folder);

        bool conflict = false;
        foreach (string name in GraphicsManager.TileIndex.Nodes.Keys)
        {
            if (name.ToLower() == animName.ToLower())
            {
                conflict = true;
                break;
            }
        }

        if (conflict)
        {
            var result = await MessageBoxWindowView.Show(_context.DialogService,
                $"Are you sure you want to overwrite the existing sheet:\n{animName}",
                "Tileset already exists.", MessageBoxWindowView.MessageBoxButtons.YesNo);

            if (result == MessageBoxWindowView.MessageBoxResult.No)
                return;
        }

        DevForm.SetConfig("TilesetDir", Path.GetDirectoryName(folder));

        try
        {
            DevForm.ExecuteOrPend(() => { tryImportDtef(folder, animName); });
        }
        catch (Exception ex)
        {
            DiagManager.Instance.LogError(ex, false);
            await MessageBoxWindowView.Show(_context.DialogService, $"Error importing from\n{folder}\n\n{ex.Message}",
                "Import Failed", MessageBoxWindowView.MessageBoxButtons.Ok);
            return;
        }
    }

    private async Task ExportDtefAsync()
    {
        if (SelectedItem != null)
        {
            //remember addresses in registry
            string folderName = DevForm.GetConfig("TilesetDir", Directory.GetCurrentDirectory());

            string? folder = await _context.DialogService.ShowFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "Select folder to export the DTEF to",
                AllowMultiple = false,
            }, folderName);

            if (folder is null)
                return;
            DevForm.SetConfig("TilesetDir", Path.GetDirectoryName(folder));

            // TODO: Implement export Dtef function
            // DevForm.ExecuteOrPend(() => { tryExportDtef(entryIndex, folder); });
        }
    }
    // private void tryExportDtef(int entryIndex, string folder)
    // {
    //     lock (GameBase.lockObj)
    //     {
    //         DtefImportHelper.ExportDtefTile(entryIndex, folder);
    //     }
    // }






    public override void OnPageRemoved()
    {
        Node.OnReload -= ReloadEntries;
        base.OnPageRemoved();

    }

 
    protected override bool IsSamePage(EditorPageViewModel other)
    {
        var page = other as DataListPageViewModel;
        return DataType == page?.DataType;
    }

    private void UpdateVisibleItems(string filter)
    {
        FilteredItems.Clear();
        var strategy = new BeginningTitleFilterStrategy();
        foreach (var item in Items.Where(e => strategy.Matches(e.Title, filter) || strategy.Matches(e.Key, filter)))
            FilteredItems.Add(item);
    }
    
    public void AddChildItemUnderParent(DataListEntry entry)
    {
        Node.AddNodeIfNotExists(
            _context.NodeFactory.CreateDataItemNode<ReflectedDataPageViewModel>(entry.Key, entry.Title, Node.Icon, _configurePage));
        NodeHelper.ExpandParents(Node, true);
    }

    private void _configurePage(EditorPageViewModel vm)
    {
        ReflectedDataPageViewModel pg = (ReflectedDataPageViewModel)vm;
    
        pg.SetIsRootPage(true);
    
        var dataRoot = pg.Node.FindNode<DataRootNode>();
        var dataItem = pg.Node.FindNode<DataItemNode>();

        DataManager.DataType dataType = dataRoot.DataType;
        string key = dataItem.ItemKey;

        var regis = DataRegistry.Map[dataType];
        IEntryData data = regis.GetEntry(key);

        string title = DataEditor.GetWindowTitle(String.Format("{0} #{1}", dataType.ToString(), key),
            data.Name.ToLocal(), data, data.GetType());

        pg.SetPageTitle(title, pg.Node.Icon);

        pg.OnLoadAction = (StackPanel stack) =>
        {
            DataEditor.LoadDataControls(key, data, stack);
        };

        pg.OnOKAction = async (StackPanel stack) =>
        {
            lock (GameBase.lockObj)
            {
                object obj = data;
                DataEditor.SaveDataControls(ref obj, stack, new Type[0]);
                DataManager.Instance.ContentChanged(dataType, key, (IEntryData)obj);

                string newName = DataManager.Instance.DataIndices[dataType].Get(key).GetLocalString(true);
                pg.SetPageTitle(
                    DataEditor.GetWindowTitle(String.Format("{0} #{1}", dataType.ToString(), key), newName, obj,
                        obj.GetType()), pg.Node.Icon);
                
                // TODO: This needs to update after pressing OK on the default name too...
                
                pg.Node.Title = $"{key}: {newName}";
                pg.Title = $"{key}: {newName}";
                ModifyEntry(key, newName);
            
            }

            return true;
        };
    }
    
    
    
    public void ModifyEntry(string key, string newValue)
    {
        var item = Items.FirstOrDefault(x => x.Key == key);
        if (item == null) return;
        int index = Items.IndexOf(item);
        Items[index] = item with { Value = newValue };
        UpdateVisibleItems(SearchFilter);
    }

    public async Task AddItem()
    {
        var entry = DataRegistry.Map[DataType];

        var vm = new RenameWindowViewModel();
        bool result = await _context.DialogService.ShowDialogAsync<RenameWindowViewModel, bool>(vm, $"Add new {DataType}");

        if (!result)
            return;

        lock (GameBase.lockObj)
        {
            string assetName = Text.GetNonConflictingName(Text.Sanitize(vm.Name).ToLower(),
                DataManager.Instance.DataIndices[DataType].ContainsKey);
            DataManager.Instance.ContentChanged(DataType, assetName, entry.CreateEntry());
            string newName = DataManager.Instance.DataIndices[DataType].Get(assetName).GetLocalString(true);
            // choices.AddEntry(assetName, newName);

            if (DataType == DataManager.DataType.Zone)
                LuaEngine.Instance.CreateZoneScriptDir(assetName);
        }

        DataListEntry data = new(vm.Name, "");
        Items.Add(data);
        UpdateVisibleItems(SearchFilter);


        // TODO: Determine whether to add the new item to the tree
        // SubNodes.Add(_nodeFactory.CreateDataItemNode<DevEditPageViewModel>(assetName, $"{assetName}:", "Icons.GhostFill"));
        // IsExpanded = true;
        // Console.WriteLine($"Added {DataType} item: {vm.Name}");
    }

    // private async Task ReIndexAsync()
    // {
    //     await Task.Run(() =>
    //     {
    //         lock (GameBase.lockObj)
    //         {
    //             DevHelper.RunIndexing(DataType);
    //             DevHelper.RunExtraIndexing(DataType);
    //             DataManager.Instance.LoadIndex(DataType);
    //             DataManager.Instance.LoadUniversalIndices();
    //             DataManager.Instance.ClearCache(DataType);
    //             DiagManager.Instance.DevEditor.ReloadData(DataType);
    //             ReloadEntries();
    //         }
    //     });
    // }


    public async Task DeleteItem()
    {
        var key = SelectedItem?.Key;
        if (key is null) return;


        var datatype = Node.DataType;
        var modStatus = DataManager.GetEntryDataModStatus(key, datatype.ToString());

        if (PathMod.Quest.IsValid() && modStatus == DataManager.ModStatus.Base)
        {
            await MessageBoxWindowView.Show(_context.DialogService,
                string.Format("The {0} {1} is not a part of the currently edited mod and cannot be deleted.",
                    datatype.ToString(), key),
                "Delete " + datatype.ToString(), MessageBoxWindowView.MessageBoxButtons.Ok);
            return;
        }

        MessageBoxWindowView.MessageBoxResult result;
        if (modStatus == DataManager.ModStatus.Base || modStatus == DataManager.ModStatus.Added)
            result = await MessageBoxWindowView.Show(_context.DialogService,
                string.Format("Are you sure you want to delete the following {0}:\n{1}", datatype.ToString(), key),
                "Delete " + datatype.ToString(), MessageBoxWindowView.MessageBoxButtons.YesNo);
        else
            result = await MessageBoxWindowView.Show(_context.DialogService,
                string.Format("The following {0} will be reset back to the base game:\n{1}", datatype.ToString(), key),
                "Delete " + datatype.ToString(), MessageBoxWindowView.MessageBoxButtons.YesNo);

        if (result == MessageBoxWindowView.MessageBoxResult.No)
            return;


        Items.Remove(SelectedItem);
        UpdateVisibleItems(SearchFilter);
        _context.TabEvents.RequestCloseTabsForEntry(key, datatype);
        var nodeToRemove = Node.SubNodes
            .OfType<DataItemNode>()
            .FirstOrDefault(n => n.ItemKey == key);

        if (nodeToRemove != null)
            Node.SubNodes.Remove(nodeToRemove);
        lock (GameBase.lockObj)
        {
            DataManager.Instance.ContentChanged(datatype, key, null);
            // choices.DeleteEntry(key);


            if (DataManager.Instance.DataIndices[datatype].ContainsKey(key))
            {
                string newName = DataManager.Instance.DataIndices[datatype].Get(key).GetLocalString(true);
                // choices.AddEntry(key, newName);
            }

            if (datatype == DataManager.DataType.Zone)
            {
                string str = LuaEngine.MakeZoneScriptPath(key, "");
                if (Directory.Exists(str))
                    Directory.Delete(str, true);
            }
        }
    }
}