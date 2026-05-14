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
using DynamicData;
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
    public ObservableCollection<NodeBase> SubNodes { get; }

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
    public bool AddNodeIfNotExists(NodeBase node)
    {
        bool added = false;

        if (!SubNodes.Contains(node))
        {
            SubNodes.Add(node);
            added = true;
        }
        
        return added;
    }
    
    public void RemoveNode(NodeBase node)
    {
        SubNodes.Remove(node);
    }
    
    // NOTE: Include itself while looking for a match
    public T FindNode<T>() where T : NodeBase
    {
        var currentNode = this;

        while (currentNode != null)
        {
            if (currentNode is T match)
                return match;

            currentNode = currentNode.Parent;
        }

        return null;
    }
}

public class OpenEditorNode : NodeBase
{
    public Type EditorType { get; }
    
    public readonly Action<EditorPageViewModel>? OnPageLoad;
    
    public OpenEditorNode(string title, Type? editorType, string? icon = null, Action<EditorPageViewModel>? onPageLoad = null)
        : base(title, icon ?? "")
    {
        OnPageLoad = onPageLoad;
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

public class ReflectedDataNode : OpenEditorNode
{
    private string BuildIdentifier(NodeBase? parent)
    {
        var parts = new List<string> { Title };
        NodeBase? current = parent;

        while (current != null)
        {
            parts.Add(current.Title);
            current = current.Parent;
        }

        parts.Reverse();
        return string.Join("/", parts);
    }

    private readonly string _identifier;

    public ReflectedDataNode(string title, Type editorType,
        string icon = null, NodeBase? parent = null, Action<EditorPageViewModel>? onPageLoad = null)
        : base(title, editorType, icon, onPageLoad)
    {
        _identifier = BuildIdentifier(parent);
    }

    protected override bool EqualsCore(NodeBase other)
        => other is ReflectedDataNode o && _identifier == o._identifier;

    protected override int GetHashCodeCore() => _identifier.GetHashCode();
}


// TODO: Remove
public class OpenEditorNodeWithParams : OpenEditorNode
{
    public object[] ExtraParams { get; }

    public OpenEditorNodeWithParams(
        string title,
        Type? editorType,
        object[] extraParams,
        string? icon = null, 
        Action<EditorPageViewModel>? onPageLoad = null)
        : base(title, editorType, icon, onPageLoad)
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
        string? icon = null,
        Action<EditorPageViewModel>? onPageLoad = null
        )
        : base(title, editorType, icon, onPageLoad)
    {
        _getter = getter;
        _setter = setter;
    }

    public async Task OnClickAsync()
    {

    }
}

// public abstract class ItemRootNode : OpenEditorNode
// {
//     
//     public ReactiveCommand<NodeBase, Unit> DeleteCommand { get; }
//
// // In constructor:
//
//     public abstract Task AddItem();
//     public abstract Task DeleteItem(string key);
//
//     protected ItemRootNode(string title, Type? editorKey, string icon = null, Action<EditorPageViewModel>? onPageLoad = null)
//         : base(title, editorKey, icon, onPageLoad)
//     {
//         DeleteCommand = ReactiveCommand.Create<NodeBase>(RemoveNode);
//         
//     }
// }

public class DataRootNode : OpenEditorNode
{
    public DataManager.DataType DataType { get; }

    private readonly NodeFactory _nodeFactory;
    private readonly IDialogService _dialogService;

    public DataRootNode(NodeFactory nodeFactory, IDialogService dialogService, DataManager.DataType dataType,
        Type? editorType, string title, string? icon = null, Action<EditorPageViewModel>? onPageLoad = null)
        : base(title, editorType, icon ?? "", onPageLoad)
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

// Children of the DataRootNode
public class DataItemNode : OpenEditorNode
{
    public string ItemKey { get; }
    
    public DataItemNode(string itemKey, Type? editorType, string? title, string? icon = null, Action<EditorPageViewModel>? onPageLoad = null)
        : base(title ?? "", editorType, icon ?? "", onPageLoad)
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
public class SpriteRootNode : OpenEditorNode
{
    private readonly ISpriteRootOperationStrategy _strategy;
    
    public readonly GraphicsManager.AssetType AssetType;

    
    public ReactiveCommand<Unit, Unit> MassExportCommand { get; }
    public ReactiveCommand<Unit, Unit> MassImportCommand { get; }
    // public ReactiveCommand<DataItemNode, Unit> ExportCommand { get; }
    // public ReactiveCommand<Unit, Unit> ImportCommand { get; }
    // public ReactiveCommand<Unit, Unit> ReImportCommand { get; }

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
        string? icon = null, Action<EditorPageViewModel>? onPageLoad = null)
        : base(title, editorType, icon ?? "", onPageLoad)
    {
        AssetType = assetType;
        // _dialogService = dialogService;
        // _nodeFactory = nodeFactory;

        if (AssetType == GraphicsManager.AssetType.Tile)
        {
            _strategy = new SpriteRootTileStrategy(dialogService, nodeFactory, this);
        }
        else
        {
            _strategy = new SpriteRootAssetTypeStrategy(dialogService, nodeFactory, this);
            
        }
        
        MassExportCommand = ReactiveCommand.CreateFromTask(MassExportAsync);
        MassImportCommand = ReactiveCommand.CreateFromTask(MassImportAsync);
        // ExportCommand = ReactiveCommand.CreateFromTask<DataItemNode>(ExportSpriteAsync);
        // ImportCommand = ReactiveCommand.CreateFromTask(ImportSpriteAsync);
        // ReImportCommand = ReactiveCommand.CreateFromTask(ReImportSpriteAsync);
    }

    public async Task AddItem()
    {
        await _strategy.ImportAsync();
    }

    public async Task DeleteItem(string key)
    {
        // await _strategy.DeleteAsync(node);
    }

    public async Task MassExportAsync()
    {
        await _strategy.MassExportAsync();
    }

    public async Task MassImportAsync()
    {
        await _strategy.MassImportAsync();
    }

    // private async Task ExportSpriteAsync(DataItemNode node)
    // {
    //     await _strategy.ExportAsync(node);
    // }

    // private async Task ImportSpriteAsync()
    // {
    //     await _strategy.ImportAsync();
    // }
    //
    // private async Task ReImportSpriteAsync()
    // {
    //     await _strategy.ReImportAsync();
    // }
}

// public class ModRootNode : ItemRootNode
// {
//     private readonly NodeFactory _nodeFactory;
//
//     private readonly IDialogService _dialogService;
//     public ModRootNode(NodeFactory nodeFactory, IDialogService dialogService,
//         Type? editorType, string title, string? icon = null, Action<EditorPageViewModel>? onPageLoad = null)
//         : base(title, editorType, icon ?? "", onPageLoad)
//     {
//         _nodeFactory = nodeFactory;
//         _dialogService = dialogService;
//         
//     }
//
//     public override async Task AddItem()
//     {
//         ModHeader header = new ModHeader("", "", "", "", "", Guid.NewGuid(), new Version(), new Version(), PathMod.ModType.Mod, new RelatedMod[0] { });
//         var vm = new ModConfigWindowViewModel(_dialogService, header);
//         bool result = await _dialogService.ShowDialogAsync<ModConfigWindowViewModel, bool>(vm, "Mod Config");
//         
//         if (!result)
//             return;
//         
//         // SubNodes.Add(_nodeFactory.CreateDataItemNode(vm.Name, "MonsterEditor", $"{vm.Name}:", "Icons.GhostFill"));
//         // IsExpanded = true;
//         // Console.WriteLine($"Added {DataType} item: {vm.Name}");
//     }
//
//     public override async Task DeleteItem(string key)
//     {
//         // if (node is null)
//         //     return;
//         //
//         // var res = await MessageBoxWindowView.Show(_dialogService,
//         //     $"Deleting {node.ItemKey} will reset it back to the base game.", $"Delete {node.ItemKey}", MessageBoxWindowView.MessageBoxButtons.YesNo, true);
//         //
//         // if (res != MessageBoxWindowView.MessageBoxResult.Yes)
//         //     return;
//         //
//         // SubNodes.Remove(node);
//         // Console.WriteLine($"Deleted {node.Title} of type {DataType}");
//     }
// }

// Used by TabSwitcher
public class PageNode : NodeBase
{
    private readonly NodeFactory _nodeFactory;
    // private readonly IDialogService _dialogService;
    public EditorPageViewModel Page { get; }
    public PageNode? Parent { get; set; }
    public bool IsTopLevel => Parent == null;

    public PageNode(NodeFactory nodeFactory, EditorPageViewModel page,
        PageNode? parent = null)
        : base(page.Title, page.Icon)
    {
        // _dialogService = dialogService;
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