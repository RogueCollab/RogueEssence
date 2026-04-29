using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using Avalonia.ReactiveUI;
using Avalonia.Threading;
using RogueEssence.Dev.Services;
using ReactiveUI;
using RogueEssence.Content;
using RogueEssence.Data;
using RogueEssence.Dev.Views;
using RogueEssence.Script;


namespace RogueEssence.Dev.ViewModels;

using System.Collections.ObjectModel;

public class NodeBase : ViewModelBase, IEquatable<NodeBase>
{
    public ObservableCollection<NodeBase> SubNodes { get; set; }

    public NodeBase? Parent { get; internal set; }

    private string _title = "";

    public virtual string Title
    {
        get => _title;
        set => this.RaiseAndSetIfChanged(ref _title, value);
    }

    private string _icon = "";

    public string Icon
    {
        get => _icon;
        set => this.RaiseAndSetIfChanged(ref _icon, value);
    }

    public event Action? SubNodesChanged;
    

    public NodeBase(string title = "", string? icon = null)
    {
        Title = title;
        _icon = icon ?? "";
        SubNodes = new ObservableCollection<NodeBase>();
        IsExpanded = false;
        IsVisible = true;
        SubNodes.CollectionChanged += OnSubNodesChanged;
        this.WhenAnyValue(x => x.IsExpanded)
            .Where(expanded => !expanded)
            .Subscribe(_ => CollapseChildren());
    }

    private bool _isExpanded = false;

    public bool IsExpanded
    {
        get => _isExpanded;
        set => this.RaiseAndSetIfChanged(ref _isExpanded, value);
    }

    private bool _isVisible = true;

    public bool IsVisible
    {
        get => _isVisible;
        set => this.RaiseAndSetIfChanged(ref _isVisible, value);
    }
    
    public bool Equals(NodeBase? other)
    {
        if (other is null) return false;
        if (GetType() != other.GetType()) return false;
        return EqualsCore(other);
    }

    public override bool Equals(object? obj) => (obj is NodeBase) && this.Equals((NodeBase) obj);
    public override int GetHashCode() => GetHashCodeCore();

    protected virtual bool EqualsCore(NodeBase other) => ReferenceEquals(this, other);
    protected virtual int GetHashCodeCore() => RuntimeHelpers.GetHashCode(this);
    

    private void OnSubNodesChanged(object? s, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (NodeBase child in e.NewItems)
            {
                child.Parent = this;
                child.SubNodesChanged += SubNodesChanged;
            }
        }
        
        if (e.OldItems != null)
        {
            foreach (NodeBase child in e.OldItems)
            {
                child.Parent = null;
                child.SubNodesChanged -= SubNodesChanged;
            }
        }

        SubNodesChanged?.Invoke();
    }


    public void CollapseChildren()
    {
        if (SubNodes == null)
            return;

        foreach (var child in SubNodes)
        {
            child.IsExpanded = false;
            child.CollapseChildren();
        }
    }
    public void AddNodeIfNotExists(NodeBase node)
    {
        if (!SubNodes.Contains(node))
            SubNodes.Add(node);
    }
    
    public void RemoveNode(NodeBase node)
    {
        Console.WriteLine("hi");
        Console.WriteLine($"Removing node {node.Title}");
        SubNodes.Remove(node);
    }
    
}

public class OpenEditorNode : NodeBase
{
    public Type EditorType { get; }

    public OpenEditorNode(string title, Type? editorType, string? icon = null)
        : base(title, icon ?? "")
    {
        EditorType = editorType;
    }
    
    protected override bool EqualsCore(NodeBase other)
    {
        if (other is not OpenEditorNode node)
            return false;
        return EditorType == node.EditorType;
    }
    protected override int GetHashCodeCore()
    {
        return EditorType.GetHashCode();
    }
}


// TODO: Remove
public class OpenEditorNodeWithParams : OpenEditorNode
{
    public object[] ExtraParams { get; }

    public OpenEditorNodeWithParams(
        string title,
        Type? editorType,
        object[] extraParams,
        string? icon = null)
        : base(title, editorType, icon)
    {
        ExtraParams = extraParams;
    }
}

public class OpenEditorNodeFX<T> : OpenEditorNode
{
    private readonly Func<T> _getter;
    private readonly Action<T> _setter;

    public OpenEditorNodeFX(
        string title, 
        Type? editorType,
        Func<T> getter, 
        Action<T> setter,
        string? icon = null)
        : base(title, editorType, icon)
    {
        _getter = getter;
        _setter = setter;
    }

    public async Task OnClickAsync()
    {

    }
}

public abstract class ItemRootNode : OpenEditorNode
{
    
    public ReactiveCommand<NodeBase, Unit> DeleteCommand { get; }

// In constructor:

    public abstract Task AddItem();
    public abstract Task DeleteItem(string key);

    protected ItemRootNode(string title, Type? editorKey, string icon = null)
        : base(title, editorKey, icon)
    {
        DeleteCommand = ReactiveCommand.Create<NodeBase>(RemoveNode);
        
    }
}

public class DataRootNode : ItemRootNode
{
    public DataManager.DataType DataType { get; }

    private readonly NodeFactory _nodeFactory;
    private readonly IDialogService _dialogService;

    public DataRootNode(NodeFactory nodeFactory, IDialogService dialogService, DataManager.DataType dataType,
        Type? editorType, string title, string? icon = null)
        : base(title, editorType, icon ?? "")
    {
        _nodeFactory = nodeFactory;
        _dialogService = dialogService;
        DataType = dataType;
    }

    protected override bool EqualsCore(NodeBase other)
    {
        if (other is not DataRootNode node)
            return false;
        return DataType == node.DataType && EditorType == node.EditorType;
    }

    protected override int GetHashCodeCore()
    {
        return EditorType.GetHashCode() ^ DataType.GetHashCode();
    }

    public override async Task AddItem()
    {
        var vm = new RenameWindowViewModel();
        bool result = await _dialogService.ShowDialogAsync<RenameWindowViewModel, bool>(vm, $"Add new {DataType}");

        if (!result)
            return;

        SubNodes.Add(_nodeFactory.CreateDataItemNode<DevEditPageViewModel>(vm.Name, $"{vm.Name}:", "Icons.GhostFill"));
        IsExpanded = true;
        Console.WriteLine($"Added {DataType} item: {vm.Name}");
    }

    public override async Task DeleteItem(string key)
    {
        // if (node is null)
        //     return;

        // var res = await MessageBoxWindowView.Show(_dialogService,
        //     $"Deleting {key} will reset it back to the base game.", $"Delete {key}", MessageBoxWindowView.MessageBoxButtons.YesNo, true);

        // if (res != MessageBoxWindowView.MessageBoxResult.Yes)
        //     return;

        // SubNodes.Remove(node);
        // Console.WriteLine($"Deleted {key} of type {DataType}");
        
        var modStatus = DataManager.GetEntryDataModStatus(key, DataType.ToString());

        if (PathMod.Quest.IsValid() && modStatus == DataManager.ModStatus.Base)
        {
            await MessageBoxWindowView.Show(_dialogService,
                string.Format("The {0} {1} is not a part of the currently edited mod and cannot be deleted.", DataType.ToString(), key),
                "Delete " + DataType.ToString(), MessageBoxWindowView.MessageBoxButtons.Ok);
            return;
        }

        MessageBoxWindowView.MessageBoxResult result;
        if (modStatus == DataManager.ModStatus.Base || modStatus == DataManager.ModStatus.Added)
            result = await MessageBoxWindowView.Show(_dialogService,
                string.Format("Are you sure you want to delete the following {0}:\n{1}", DataType.ToString(), key),
                "Delete " + DataType.ToString(), MessageBoxWindowView.MessageBoxButtons.YesNo);
        else
            result = await MessageBoxWindowView.Show(_dialogService,
                string.Format("The following {0} will be reset back to the base game:\n{1}", DataType.ToString(), key),
                "Delete " + DataType.ToString(), MessageBoxWindowView.MessageBoxButtons.YesNo);

        if (result == MessageBoxWindowView.MessageBoxResult.No)
            return;

        lock (GameBase.lockObj)
        {
            DataManager.Instance.ContentChanged(DataType, key, null);
            // choices.DeleteEntry(key);
        
            if (DataManager.Instance.DataIndices[DataType].ContainsKey(key))
            {
                string newName = DataManager.Instance.DataIndices[DataType].Get(key).GetLocalString(true);
                // choices.AddEntry(key, newName);
            }

            if (DataType == DataManager.DataType.Zone)
            {
                string str = LuaEngine.MakeZoneScriptPath(key, "");
                if (Directory.Exists(str))
                    Directory.Delete(str, true);
            }
        }
    }

    public async Task ReIndex()
    {
        Console.WriteLine($"Reindexing {DataType}...");
        await Task.Delay(100);
    }

    public async Task ResaveAllAsFile()
    {
        Console.WriteLine($"Resaving all {DataType} as file...");
        await Task.Delay(100);
    }

    public async Task ResaveAllAsDiff()
    {
        Console.WriteLine($"Resaving all {DataType} as diff...");
        await Task.Delay(100);
    }

    public async Task ResaveItemAsFile(string key)
    {
        if (DataManager.GetEntryDataModStatus(key, DataType.ToString()) == DataManager.ModStatus.Base)
        {
            await MessageBoxWindowView.Show(_dialogService, string.Format("{0} must have saved edits first!", key), "Error", MessageBoxWindowView.MessageBoxButtons.Ok);
            return;
        }

        lock (GameBase.lockObj)
        {
            var entry = DataRegistry.Map[DataType];
            IEntryData data = entry.GetEntry(key);
            DataManager.Instance.ContentResaved(DataType, key, data, false);
            string newName = DataManager.Instance.DataIndices[DataType].Get(key).GetLocalString(true);
        }

        await MessageBoxWindowView.Show(_dialogService, string.Format("{0} is now saved as a file.", key), "Complete", MessageBoxWindowView.MessageBoxButtons.Ok);
    }

    public async Task ResaveItemAsPatch(string key)
    {
        var modStatus = DataManager.GetEntryDataModStatus(key, DataType.ToString());
    
        if (modStatus == DataManager.ModStatus.Base)
        {
            await MessageBoxWindowView.Show(_dialogService, string.Format("{0} must have saved edits first!", key), "Error", MessageBoxWindowView.MessageBoxButtons.Ok);
            return;
        }
    
        if (modStatus == DataManager.ModStatus.Added)
        {
            await MessageBoxWindowView.Show(_dialogService, string.Format("{0} is newly added in this mod and cannot be saved as patch.", key), "Error", MessageBoxWindowView.MessageBoxButtons.Ok);
            return;
        }

        lock (GameBase.lockObj)
        {
            var entry = DataRegistry.Map[DataType];
            IEntryData data = entry.GetEntry(key);
            DataManager.Instance.ContentResaved(DataType, key, data, true);
        }

        if (DataManager.GetEntryDataModStatus(key, DataType.ToString()) == DataManager.ModStatus.Base)
            await MessageBoxWindowView.Show(_dialogService, string.Format("Modded {0} was identical to base. Unneeded patch removed.", key), "Complete", MessageBoxWindowView.MessageBoxButtons.Ok);
        else
            await MessageBoxWindowView.Show(_dialogService, string.Format("{0} is now saved as a patch.", key), "Complete", MessageBoxWindowView.MessageBoxButtons.Ok);
    }
    
    
}


// For Skills, Zones, Monsters, Items, etc.
// public class DataRootNode : ItemRootNode
// {
//     public DataManager.DataType DataType { get; }
//
//     private readonly NodeFactory _nodeFactory;
//
//     private readonly IDialogService _dialogService;
//     
//     public override ReactiveCommand<Unit, Unit> AddCommand { get; }
//     public override ReactiveCommand<DataItemNode, Unit> DeleteCommand { get; }
//
//     public ReactiveCommand<Unit, Unit> ReIndexCommand { get; }
//     public ReactiveCommand<Unit, Unit> ResaveAllAsFileCommand { get; }
//     public ReactiveCommand<Unit, Unit> ResaveAllAsDiffCommand { get; }
//
//     public ReactiveCommand<string, Unit> ResaveAsFile { get; }
//     public ReactiveCommand<string, Unit> ResaveAsPatch { get; }
//
//     public DataRootNode(NodeFactory nodeFactory, IDialogService dialogService, DataManager.DataType dataType,
//         Type? editorType, string title, string? icon = null)
//         : base(title, editorType, icon ?? "")
//     {
//         _nodeFactory = nodeFactory;
//         _dialogService = dialogService;
//         DataType = dataType;
//
//         AddCommand = ReactiveCommand.CreateFromTask(AddItemAsync);
//         AddCommand.CanExecute.Subscribe(canExecute =>
//             Console.WriteLine($"Add Command - CanExecute: {canExecute}"));
//
//         DeleteCommand = ReactiveCommand.CreateFromTask<DataItemNode>(DeleteItemAsync);
//
//         ReIndexCommand = ReactiveCommand.CreateFromTask(ReIndexAsync);
//         ResaveAllAsFileCommand = ReactiveCommand.CreateFromTask(ResaveAllAsFileAsync);
//         ResaveAllAsDiffCommand = ReactiveCommand.CreateFromTask(ResaveAllAsDiffAsync);
//
//
//         ResaveAsFile = ReactiveCommand.CreateFromTask<string>(
//             execute: async (key) => await ResaveItemAsFileAsync(key),
//             canExecute: Observable.Return(true)
//         );
//         ResaveAsFile.ThrownExceptions.Subscribe(ex => Console.WriteLine(ex.Message));
//         ResaveAsFile.IsExecuting.Subscribe(x => Console.WriteLine($"IsExecuting: {x}"));
//         ResaveAsPatch = ReactiveCommand.CreateFromTask<string>(ResaveItemAsPatchAsync);
//     }
//
//     protected override bool EqualsCore(NodeBase other)
//     {
//         if (other is not DataRootNode node)
//             return false;
//         return DataType == node.DataType && EditorType == node.EditorType;
//     }
//     protected override int GetHashCodeCore()
//     {
//         return EditorType.GetHashCode() ^ DataType.GetHashCode();;
//     }
//     
//     private async Task AddItemAsync()
//     {
//         var vm = new RenameWindowViewModel();
//         bool result = await _dialogService.ShowDialogAsync<RenameWindowViewModel, bool>(vm, $"Add new {DataType}");
//
//         if (!result)
//             return;
//
//         SubNodes.Add(_nodeFactory.CreateDataItemNode<DevEditPageViewModel>(vm.Name, $"{vm.Name}:", "Icons.GhostFill"));
//         IsExpanded = true;
//         Console.WriteLine($"Added {DataType} item: {vm.Name}");
//     }
//
//     private async Task DeleteItemAsync(DataItemNode? node)
//     {
//         if (node is null)
//             return;
//
//         var res = await MessageBoxWindowView.Show(_dialogService,
//             $"Deleting {node.ItemKey} will reset it back to the base game.", $"Delete {node.ItemKey}", MessageBoxWindowView.MessageBoxButtons.YesNo, true);
//
//         if (res != MessageBoxWindowView.MessageBoxResult.Yes)
//             return;
//
//         SubNodes.Remove(node);
//         Console.WriteLine($"Deleted {node.Title} of type {DataType}");
//     }
//
//     private async Task ReIndexAsync()
//     {
//         Console.WriteLine($"Reindexing {DataType}...");
//         await Task.Delay(100);
//     }
//
//     private async Task ResaveAllAsFileAsync()
//     {
//         Console.WriteLine($"Resaving all {DataType} as file...");
//         await Task.Delay(100);
//     }
//
//     private async Task ResaveAllAsDiffAsync()
//     {
//         Console.WriteLine($"Resaving all {DataType} as diff...");
//         await Task.Delay(100);
//     }
//     
//     public async Task ResaveItemAsFileAsync(string key)
//     {
//         await Task.Run(async () =>
//         {
//             if (DataManager.GetEntryDataModStatus(key, DataType.ToString()) == DataManager.ModStatus.Base)
//             {
//                 await Dispatcher.UIThread.InvokeAsync(() =>
//                     MessageBoxWindowView.Show(_dialogService,
//                         string.Format("{0} must have saved edits first!", key), "Error", MessageBoxWindowView.MessageBoxButtons.Ok));
//                 return;
//             }
//
//             lock (GameBase.lockObj)
//             {
//                 var entry = DataRegistry.Map[DataType];
//                 IEntryData data = entry.GetEntry(key);
//                 DataManager.Instance.ContentResaved(DataType, key, data, false);
//                 string newName = DataManager.Instance.DataIndices[DataType].Get(key).GetLocalString(true);
//             }
//
//             await Dispatcher.UIThread.InvokeAsync(() =>
//                 MessageBoxWindowView.Show(_dialogService,
//                     string.Format("{0} is now saved as a file.", key), "Complete", MessageBoxWindowView.MessageBoxButtons.Ok));
//         });
//     }
    // private async Task ResaveItemAsFileAsync(string key)
    // {
    //     Console.WriteLine("3");
    //     
    //     await Task.Yield(); // force off the calling context
    //
    //     Console.WriteLine("4");
    //     await Task.Delay(1000).ConfigureAwait(false);
    //     Console.WriteLine("5");
        // if (DataManager.GetEntryDataModStatus(key, DataType.ToString()) == DataManager.ModStatus.Base)
        // {
        //     await MessageBoxWindowView.Show(_dialogService,
        //         string.Format("{0} must have saved edits first!", key), "Error", MessageBoxWindowView.MessageBoxButtons.Ok);
        // }
        // else
        // {
        //     await Task.Run(() =>
        //     {
        //         lock (GameBase.lockObj)
        //         {
        //             var entry = DataRegistry.Map[DataType];
        //             IEntryData data = entry.GetEntry(key);
        //             DataManager.Instance.ContentResaved(DataType, key, data, false);
        //             string newName = DataManager.Instance.DataIndices[DataType].Get(key).GetLocalString(true);
        //         }
        //     });
        //
        //     await MessageBoxWindowView.Show(_dialogService,
        //         string.Format("{0} is now saved as a file.", key), "Complete", MessageBoxWindowView.MessageBoxButtons.Ok);
        // }
    // }


    // private async Task ResaveItemAsFileAsync(string key)
    // {
    //     if (DataManager.GetEntryDataModStatus(key, DataType.ToString()) == DataManager.ModStatus.Base)
    //     {
    //         
    //         
    //        await MessageBoxWindowView.Show(_dialogService,
    //             String.Format("{0} must have saved edits first!", key), "Errir", MessageBoxWindowView.MessageBoxButtons.Ok);
    //        
    //        return;
    //     }
    //     
    //     // Do the locked work synchronously, outside of any await
    //     await Task.Run(() =>
    //     {
    //         lock (GameBase.lockObj)
    //         {
    //             var entry = DataRegistry.Map[DataType];
    //             IEntryData data = entry.GetEntry(key);
    //             DataManager.Instance.ContentResaved(DataType, key, data, false);
    //
    //             string newName = DataManager.Instance.DataIndices[DataType].Get(key).GetLocalString(true);
    //             // choices.ModifyEntry(entryNum, newName);
    //         }
    //     });
    //
    //     // lock (GameBase.lockObj)
    //     // {
    //     //
    //     //     var entry = DataRegistry.Map[DataType];
    //     //     IEntryData data = entry.GetEntry(key);
    //     //     DataManager.Instance.ContentResaved(DataType, key, data, false);
    //     //
    //     //     string newName = DataManager.Instance.DataIndices[DataType].Get(key).GetLocalString(true);
    //     //
    //     // }
    //
    //                
    //     await MessageBoxWindowView.Show(_dialogService,
    //         String.Format("{0} is now saved as a file.", key), "Complete", MessageBoxWindowView.MessageBoxButtons.Ok);
    // }


// }

// Children of the DataRootNode

public class DataItemNode : OpenEditorNode
{
    public string ItemKey { get; }

    // private string _value = "";
    //
    // public string Value
    // {
    //     get => _value;
    //     set => this.RaiseAndSetIfChanged(ref _value, value);
    // }

    // private bool _editorKey = false;
    // public bool EditorKey
    // {
    //     get => _editorKey;
    //     set => this.RaiseAndSetIfChanged(ref _editorKey, value);
    // }

    public DataItemNode(string itemKey, Type? editorType, string? title, string? icon = null)
        : base(title ?? "", editorType, icon ?? "")
    {
        ItemKey = itemKey;
    }
    
    protected override bool EqualsCore(NodeBase other)
    {
        if (other is not DataItemNode node)
            return false;
        return ItemKey == node.ItemKey && EditorType == node.EditorType;
    }
    protected override int GetHashCodeCore()
    {
        return EditorType.GetHashCode() ^ ItemKey.GetHashCode();
    }
}

// SpriteRootNode have additional properties like mass exporting
public class SpriteRootNode : ItemRootNode
{
    private readonly ISpriteOperationStrategy _strategy;
    
    public readonly GraphicsManager.AssetType AssetType;

    
    public ReactiveCommand<Unit, Unit> MassExportCommand { get; }
    public ReactiveCommand<Unit, Unit> MassImportCommand { get; }
    public ReactiveCommand<DataItemNode, Unit> ExportCommand { get; }
    public ReactiveCommand<Unit, Unit> ImportCommand { get; }
    public ReactiveCommand<Unit, Unit> ReImportCommand { get; }

    private string _cachedPath;
    public string CachedPath
    {
        get => _cachedPath;
        set => this.RaiseAndSetIfChanged(ref _cachedPath, value);
    }

    
    public SpriteRootNode(
        IDialogService dialogService,
        NodeFactory nodeFactory,
        GraphicsManager.AssetType assetType,
        Type? editorType,
        string title,
        string? icon = null)
        : base(title, editorType, icon ?? "")
    {
        AssetType = assetType;
        // _dialogService = dialogService;
        // _nodeFactory = nodeFactory;

        if (AssetType == GraphicsManager.AssetType.Tile)
        {
            _strategy = new SpriteTileStrategy(dialogService, nodeFactory, this);
        }
        else
        {
            _strategy = new SpriteAssetTypeStrategy(dialogService, nodeFactory, this);
            
        }
        
        MassExportCommand = ReactiveCommand.CreateFromTask(MassExportAsync);
        MassImportCommand = ReactiveCommand.CreateFromTask(MassImportAsync);
        ExportCommand = ReactiveCommand.CreateFromTask<DataItemNode>(ExportSpriteAsync);
        ImportCommand = ReactiveCommand.CreateFromTask(ImportSpriteAsync);
        ReImportCommand = ReactiveCommand.CreateFromTask(ReImportSpriteAsync);
    }

    public override async Task AddItem()
    {
        await _strategy.ImportAsync();
    }

    public override async Task DeleteItem(string key)
    {
        // await _strategy.DeleteAsync(node);
    }

    private async Task MassExportAsync()
    {
        await _strategy.MassExportAsync();
    }

    private async Task MassImportAsync()
    {
        await _strategy.MassImportAsync();
    }

    private async Task ExportSpriteAsync(DataItemNode node)
    {
        await _strategy.ExportAsync(node);
    }

    private async Task ImportSpriteAsync()
    {
        await _strategy.ImportAsync();
    }

    private async Task ReImportSpriteAsync()
    {
        await _strategy.ReImportAsync();
    }
}

public class SpriteTileRootNode : SpriteRootNode
{
    private readonly NodeFactory _nodeFactory;
    private readonly IDialogService _dialogService;
    public ReactiveCommand<Unit, Unit> ReIndexCommand { get; }

    public SpriteTileRootNode(IDialogService dialogService, NodeFactory nodeFactory, Type? editorType, string title, string? icon = null)
        : base(dialogService, nodeFactory, GraphicsManager.AssetType.Tile, editorType, title, icon)
    {
        _nodeFactory = nodeFactory;
        _dialogService = dialogService;
        ReIndexCommand = ReactiveCommand.CreateFromTask(ReIndexAsync);
    }
    
    private async Task ReIndexAsync()
    {
        try
        {
            _reIndex();
        }
        catch (Exception ex)
        {
            DiagManager.Instance.LogError(ex, false);
            await MessageBoxWindowView.Show(_dialogService, "Error when reindexing.\n\n" + ex.Message, "Reindex Failed", MessageBoxWindowView.MessageBoxButtons.Ok);
            return;
        }
    }
    
    private void _reloadFullList()
    {
        Console.WriteLine("TODO: Reuse this... This is already identical to SpriteTileStrategy...");
        lock (GameBase.lockObj)
        {
            SubNodes.Clear();
            foreach (string name in GraphicsManager.TileIndex.Nodes.Keys)
            {
                SubNodes.Add(
                    _nodeFactory.CreateDataItemNode<DevEditPageViewModel>(name, name + ":", this.AssetType.GetIcon())
                );
            }
        }
    }

    private void _reIndex()
    {
        DevForm.ExecuteOrPend(() => { _tryReIndex(); });

        _reloadFullList();
    }

    private void _tryReIndex()
    {
        lock (GameBase.lockObj)
        {
            GraphicsManager.RebuildIndices(GraphicsManager.AssetType.Tile);
            GraphicsManager.ClearCaches(GraphicsManager.AssetType.Tile);

            DiagManager.Instance.LogInfo("All files re-indexed.");
        }
    }
}


public class ModRootNode : ItemRootNode
{
    private readonly NodeFactory _nodeFactory;

    private readonly IDialogService _dialogService;
    public ModRootNode(NodeFactory nodeFactory, IDialogService dialogService,
        Type? editorType, string title, string? icon = null)
        : base(title, editorType, icon ?? "")
    {
        _nodeFactory = nodeFactory;
        _dialogService = dialogService;
        
    }

    public override async Task AddItem()
    {
        ModHeader header = new ModHeader("", "", "", "", "", Guid.NewGuid(), new Version(), new Version(), PathMod.ModType.Mod, new RelatedMod[0] { });
        var vm = new ModConfigWindowViewModel(header);
        bool result = await _dialogService.ShowDialogAsync<ModConfigWindowViewModel, bool>(vm, "Mod Config");
        
        if (!result)
            return;
        
        // SubNodes.Add(_nodeFactory.CreateDataItemNode(vm.Name, "MonsterEditor", $"{vm.Name}:", "Icons.GhostFill"));
        // IsExpanded = true;
        // Console.WriteLine($"Added {DataType} item: {vm.Name}");
    }

    public override async Task DeleteItem(string key)
    {
        // if (node is null)
        //     return;
        //
        // var res = await MessageBoxWindowView.Show(_dialogService,
        //     $"Deleting {node.ItemKey} will reset it back to the base game.", $"Delete {node.ItemKey}", MessageBoxWindowView.MessageBoxButtons.YesNo, true);
        //
        // if (res != MessageBoxWindowView.MessageBoxResult.Yes)
        //     return;
        //
        // SubNodes.Remove(node);
        // Console.WriteLine($"Deleted {node.Title} of type {DataType}");
    }
}

// Used by TabSwitcher
public class PageNode : NodeBase
{
    private readonly NodeFactory _nodeFactory;
    private readonly IDialogService _dialogService;
    public EditorPageViewModel Page { get; }
    public PageNode? Parent { get; set; }
    public bool IsTopLevel => Parent == null;

    public PageNode(IDialogService dialogService, NodeFactory nodeFactory, EditorPageViewModel page,
        PageNode? parent = null)
        : base(page.Title, page.Icon)
    {
        _dialogService = dialogService;
        _nodeFactory = nodeFactory;
        Page = page;
        Parent = parent;
    }

    public override string Title => Page.Title;

    public PageNode AddChild(EditorPageViewModel childPage)
    {
        var childNode = _nodeFactory.CreatePageNode(childPage, this);
        SubNodes.Add(childNode);
        return childNode;
    }

    public void RemoveChild(PageNode childNode)
    {
        SubNodes.Remove(childNode);
        childNode.Parent = null;
    }
}

// public interface IItemCommands
// {
//     ReactiveCommand<Unit, Unit> AddCommand { get; }
//     ReactiveCommand<DataItemNode, Unit> DeleteCommand { get; }
// }

// public class DefaultItemCommands : IItemCommands
// {
//     public ReactiveCommand<Unit, Unit> AddCommand { get; }
//     public ReactiveCommand<Unit, Unit> DeleteCommand { get; }
//
//     public DefaultItemCommands(Action onAdd, Action onDelete)
//     {
//         // AddCommand = ReactiveCommand.CreateFromTask(onAdd);
//         // DeleteCommand = ReactiveCommand.Create(onDelete);
//     }
// }
//


// public class DataRootItemCommands : IItemCommands
// {
//     public ReactiveCommand<Unit, Unit> AddCommand { get; }
//     public ReactiveCommand<DataItemNode, Unit> DeleteCommand { get; }
//     
//     public DataRootItemCommands(ItemRootNode node)
//     {
//         AddCommand = ReactiveCommand.CreateFromTask(async () =>
//         {
//             Console.WriteLine($"[DATA] Adding {dataType}...");
//             await Task.Delay(500); // simulate work
//             Console.WriteLine($"[DATA] Done adding {dataType}");
//         });
//
//         DeleteCommand = ReactiveCommand.CreateFromTask(async () =>
//         {
//             Console.WriteLine($"[DATA] Deleting {dataType}...");
//             await Task.Delay(500);
//             Console.WriteLine($"[DATA] Done deleting {dataType}");
//         });
//     }
//
// }