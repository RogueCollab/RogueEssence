using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using RogueEssence.Dev.Services;
using ReactiveUI;
using RogueEssence.Data;
using RogueEssence.Dev.Views;

namespace RogueEssence.Dev.ViewModels;

using System.Collections.ObjectModel;

public class NodeBase : ViewModelBase
{
    public ObservableCollection<NodeBase> SubNodes { get; set; }

    public NodeBase? Parent { get; internal set; }

    private string _title = "";

    public virtual string Title
    {
        get => _title;
        set => this.RaiseAndSetIfChanged(ref _title, value);
    }

    public string _icon = "";

    public string Icon
    {
        get => _icon;
        set => this.RaiseAndSetIfChanged(ref _icon, value);
    }


    public NodeBase(string title = "", string? icon = null)
    {
        Title = title;
        _icon = icon ?? "";
        SubNodes = new ObservableCollection<NodeBase>();
        IsExpanded = false;
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

    private void OnSubNodesChanged(object? s, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
            foreach (NodeBase child in e.NewItems)
                child.Parent = this;
        if (e.OldItems != null)
            foreach (NodeBase child in e.OldItems)
                child.Parent = null;
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
}

public class OpenEditorNode : NodeBase
{
    public string EditorKey { get; }

    public OpenEditorNode(string title, string? icon = null, string editorKey = "")
        : base(title, icon ?? "")
    {
        EditorKey = editorKey;
    }
}

public abstract class ItemRootNode : OpenEditorNode
{
    protected readonly IDialogService _dialogService;

    public abstract ReactiveCommand<Unit, Unit> AddCommand { get; }
    public abstract ReactiveCommand<DataItemNode, Unit> DeleteCommand { get; }

    protected ItemRootNode(string title, string icon, string editorKey)
        : base(title, icon, editorKey)
    {
    }
}

public class DataRootNode : ItemRootNode
{
    public DataManager.DataType DataType { get; }

    private readonly NodeFactory _nodeFactory;

    private readonly IDialogService _dialogService;


    public override ReactiveCommand<Unit, Unit> AddCommand { get; }
    public override ReactiveCommand<DataItemNode, Unit> DeleteCommand { get; }

    public ReactiveCommand<Unit, Unit> ReIndexCommand { get; }
    public ReactiveCommand<Unit, Unit> ResaveAllAsFileCommand { get; }
    public ReactiveCommand<Unit, Unit> ResaveAllAsDiffCommand { get; }

    public ReactiveCommand<DataItemNode, Unit> ResaveAsFile { get; }
    public ReactiveCommand<DataItemNode, Unit> ResaveAsPatch { get; }

    public DataRootNode(NodeFactory nodeFactory, IDialogService dialogService, DataManager.DataType dataType,
        string editorKey, string title, string? icon = null)
        : base(title, icon ?? "", editorKey)
    {
        _nodeFactory = nodeFactory;
        _dialogService = dialogService;
        DataType = dataType;

        AddCommand = ReactiveCommand.CreateFromTask(AddItemAsync);
        AddCommand.CanExecute.Subscribe(canExecute =>
            Console.WriteLine($"Add Command - CanExecute: {canExecute}"));

        DeleteCommand = ReactiveCommand.CreateFromTask<DataItemNode>(DeleteItemAsync);

        ReIndexCommand = ReactiveCommand.CreateFromTask(ReIndexAsync);
        ResaveAllAsFileCommand = ReactiveCommand.CreateFromTask(ResaveAllAsFileAsync);
        ResaveAllAsDiffCommand = ReactiveCommand.CreateFromTask(ResaveAllAsDiffAsync);

        ResaveAsFile = ReactiveCommand.CreateFromTask<DataItemNode>(ResaveItemAsFileAsync);
        ResaveAsPatch = ReactiveCommand.CreateFromTask<DataItemNode>(ResaveItemAsPatchAsync);
    }

    private async Task AddItemAsync()
    {
        var vm = new RenameWindowViewModel();
        bool result = await _dialogService.ShowDialogAsync<RenameWindowViewModel, bool>(vm, $"Add new {DataType}");

        if (!result)
            return;

        SubNodes.Add(_nodeFactory.CreateDataItemNode(vm.Name, "MonsterEditor", $"{vm.Name}:", "Icons.GhostFill"));
        Console.WriteLine($"Added {DataType} item: {vm.Name}");
    }

    private async Task DeleteItemAsync(DataItemNode? node)
    {
        if (node is null)
            return;

        var res = await MessageBoxWindowView.Show(
            $"Deleting {node.ItemKey} will reset it back to the base game.",
            $"Delete {node.ItemKey}",
            MessageBoxWindowView.MessageBoxButtons.YesNo,
            _dialogService,
            true
        );

        if (res != MessageBoxWindowView.MessageBoxResult.Yes)
            return;

        SubNodes.Remove(node);
        Console.WriteLine($"Deleted {node.Title} of type {DataType}");
    }

    private async Task ReIndexAsync()
    {
        Console.WriteLine($"Reindexing {DataType}...");
        await Task.Delay(100);
    }

    private async Task ResaveAllAsFileAsync()
    {
        Console.WriteLine($"Resaving all {DataType} as file...");
        await Task.Delay(100);
    }

    private async Task ResaveAllAsDiffAsync()
    {
        Console.WriteLine($"Resaving all {DataType} as diff...");
        await Task.Delay(100);
    }


    private async Task ResaveItemAsFileAsync(DataItemNode node)
    {
        // GetEntry entryOp, CreateEntry createOp
        // string entryNum = choices.ChosenAsset;
        // if (DataManager.GetEntryDataModStatus(entryNum, dataType.ToString()) == DataManager.ModStatus.Base)
        // {
        //     await MessageBox.Show(form, String.Format("{0} must have saved edits first!", entryNum), "Error", MessageBox.MessageBoxButtons.Ok);
        //     return;
        // }
        //
        // lock (GameBase.lockObj)
        // {
        //     IEntryData data = entryOp(entryNum);
        //     DataManager.Instance.ContentResaved(dataType, entryNum, data, false);
        //
        //     string newName = DataManager.Instance.DataIndices[dataType].Get(entryNum).GetLocalString(true);
        //     choices.ModifyEntry(entryNum, newName);
        // }
        //
        // await MessageBox.Show(form, String.Format("{0} is now saved as a file.", entryNum), "Complete", MessageBox.MessageBoxButtons.Ok);
    }

    private async Task ResaveItemAsPatchAsync(DataItemNode node)
    {
        Console.WriteLine($"Resaving {node.Title} as patch...");
        await Task.Delay(100);
    }
}

public class DataItemNode : OpenEditorNode
{
    public string ItemKey { get; }

    private string _value = "";

    public string Value
    {
        get => _value;
        set => this.RaiseAndSetIfChanged(ref _value, value);
    }

    // private bool _editorKey = false;
    // public bool EditorKey
    // {
    //     get => _editorKey;
    //     set => this.RaiseAndSetIfChanged(ref _editorKey, value);
    // }

    public DataItemNode(string itemKey, string editorKey, string? title, string? icon = null)
        : base(title ?? "", icon ?? "", editorKey)
    {
        ItemKey = itemKey;
    }
}

public class SpriteRootNode : ItemRootNode
{
    private readonly string _dataType;
    private readonly string _key;
    private readonly NodeFactory _nodeFactory;
    private readonly IDialogService _dialogService;

    public override ReactiveCommand<Unit, Unit> AddCommand { get; }
    public override ReactiveCommand<DataItemNode, Unit> DeleteCommand { get; }

    public ReactiveCommand<Unit, Unit> MassExportCommand { get; }
    public ReactiveCommand<Unit, Unit> MassImportCommand { get; }
    public ReactiveCommand<DataItemNode, Unit> ExportCommand { get; }
    public ReactiveCommand<DataItemNode, Unit> ImportCommand { get; }
    public ReactiveCommand<DataItemNode, Unit> ReImportCommand { get; }

    public SpriteRootNode(
        IDialogService dialogService,
        NodeFactory nodeFactory,
        string dataType,
        string editorKey,
        string title,
        string? icon = null)
        : base(title, icon ?? "", editorKey)
    {
        _dataType = dataType;
        _key = editorKey;
        _dialogService = dialogService;
        _nodeFactory = nodeFactory;

        AddCommand = ReactiveCommand.CreateFromTask(AddSpriteAsync);
        DeleteCommand = ReactiveCommand.CreateFromTask<DataItemNode>(DeleteSpriteAsync);
        MassExportCommand = ReactiveCommand.CreateFromTask(MassExportAsync);
        MassImportCommand = ReactiveCommand.CreateFromTask(MassImportAsync);
        ExportCommand = ReactiveCommand.CreateFromTask<DataItemNode>(ExportSpriteAsync);
        ImportCommand = ReactiveCommand.CreateFromTask<DataItemNode>(ImportSpriteAsync);
        ReImportCommand = ReactiveCommand.CreateFromTask<DataItemNode>(ReImportSpriteAsync);
    }

    private async Task AddSpriteAsync()
    {
        var vm = new RenameWindowViewModel();
        bool result = await _dialogService.ShowDialogAsync<RenameWindowViewModel, bool>(vm, "Add sprite ID");

        if (!result)
            return;

        var node = _nodeFactory.CreateDataItemNode(vm.Name, "SpriteEditor", vm.Name + ":", "Icons.PaintBrushFill");
        SubNodes.Add(node);

        Console.WriteLine($"[SpriteRootNode] Added sprite: {vm.Name}");
    }

    private async Task DeleteSpriteAsync(DataItemNode node)
    {
        var res = await MessageBoxWindowView.Show(
            $"Delete sprite '{node.Title}'?",
            "Deleting Sprite",
            MessageBoxWindowView.MessageBoxButtons.YesNo,
            _dialogService, true);

        if (res != MessageBoxWindowView.MessageBoxResult.Yes)
            return;

        SubNodes.Remove(node);
        Console.WriteLine($"[SpriteRootNode] Deleted sprite: {node.Title}");
    }

    private async Task MassExportAsync()
    {
        var x = await _dialogService.ShowFolderPickerAsync(new FolderPickerOpenOptions()
        {
            AllowMultiple = true,
            Title = "Select a folder to mass export to"
        });
        Console.WriteLine("[SpriteRootNode] Mass export triggered (TODO: implement export logic).");
        await Task.CompletedTask;
    }

    private async Task MassImportAsync()
    {
        Console.WriteLine("[SpriteRootNode] Mass import triggered (TODO: implement import logic).");
        await Task.CompletedTask;
    }

    private async Task ExportSpriteAsync(DataItemNode node)
    {
        Console.WriteLine($"[SpriteRootNode] Exporting sprite: {node.Title}");
        await Task.CompletedTask;
    }

    private async Task ImportSpriteAsync(DataItemNode node)
    {
        Console.WriteLine($"[SpriteRootNode] Importing sprite: {node.Title}");
        await Task.CompletedTask;
    }

    private async Task ReImportSpriteAsync(DataItemNode node)
    {
        Console.WriteLine($"[SpriteRootNode] Re-importing sprite: {node.Title}");
        await Task.CompletedTask;
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