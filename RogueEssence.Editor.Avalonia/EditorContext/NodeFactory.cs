

using System;
using Microsoft.Extensions.DependencyInjection;
using RogueEssence.Content;
using RogueEssence.Data;
using RogueEssence.Dev.Services;
using RogueEssence.Dev.ViewModels;

public class NodeFactory
{
    private readonly IServiceProvider _serviceProvider;

    public NodeFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public T Create<T>(params object[] args)
        where T : NodeBase
    {
        return (T)ActivatorUtilities.CreateInstance(_serviceProvider, typeof(T), args);
    }

    private Action<EditorPageViewModel>? CreateCallback<TEditor>(Action<TEditor>? onOpen)
        where TEditor : EditorPageViewModel
    {
        
        Action<EditorPageViewModel> callback = vm => { };
        if (onOpen != null)
        {
            callback = vm => onOpen((TEditor)vm);
        }
        return callback;
    }

    public OpenEditorNode CreateOpenEditorNode<TEditor>(
        string title,
        string? icon = null,
        Action<TEditor>? onOpen = null)
        where TEditor : EditorPageViewModel
        => Create<OpenEditorNode>(title, typeof(TEditor), icon, CreateCallback(onOpen));

    public OpenEditorNode CreateUniversalNode<TEditor>(
        string title,
        string? icon = null,
        Action<TEditor>? onOpen = null)
        where TEditor : EditorPageViewModel
        => Create<UniversalNode>(title, typeof(TEditor), icon, CreateCallback(onOpen));
    
    public ReflectedDataNode CreateReflectedDataNode<TEditor>(
        string title,
        string? icon = null,
        NodeBase? parent = null,
        Action<TEditor>? onOpen = null)
        where TEditor : EditorPageViewModel
        => Create<ReflectedDataNode>(title, typeof(TEditor), icon, parent, CreateCallback(onOpen));

    public OpenEditorNodeWithParams CreateOpenEditorNodeWithParams<TEditor>(
        string title,
        object[] extraParams,
        string? icon = null,
        Action<TEditor>? onOpen = null)
        where TEditor : EditorPageViewModel
        => Create<OpenEditorNodeWithParams>(title, typeof(TEditor), extraParams, icon, CreateCallback(onOpen));

    public DataRootNode CreateDataRootNode<TEditor>(
        DataManager.DataType dataType,
        string title,
        string? icon = null,
        Action<TEditor>? onOpen = null)
        where TEditor : EditorPageViewModel
        => Create<DataRootNode>(dataType, typeof(TEditor), title, icon, CreateCallback(onOpen));

    // public ModRootNode CreateModRootNode<TEditor>(
    //     string title,
    //     string? icon = null,
    //     Action<TEditor>? onOpen = null)
    //     where TEditor : EditorPageViewModel
    //     => Create<ModRootNode>(typeof(TEditor), title, icon, CreateCallback(onOpen));

    public DataItemNode CreateDataItemNode<TEditor>(
        string key,
        string title,
        string? icon = null,
        Action<TEditor>? onOpen = null)
        where TEditor : EditorPageViewModel
        => Create<DataItemNode>(key, typeof(TEditor), title, icon, CreateCallback(onOpen));

    
    public ModItemNode CreatModItemNode<TEditor>(
        string path,
        string title,
        string? icon = null,
        Action<TEditor>? onOpen = null)
        where TEditor : EditorPageViewModel
        => Create<ModItemNode>(path, typeof(TEditor), title, icon, CreateCallback(onOpen));

    
    // _serviceProvider.GetService<IDialogService>()
    public PageNode CreatePageNode(EditorPageViewModel childPage, PageNode? parentNode = null)
        => new PageNode(this, childPage, parentNode);

    public SpriteRootNode CreateSpriteRootNode<TEditor>(
        GraphicsManager.AssetType assetType,
        string title,
        string? icon = null,
        Action<TEditor>? onOpen = null)
        where TEditor : EditorPageViewModel
        => Create<SpriteRootNode>(assetType, typeof(TEditor), title, icon, CreateCallback(onOpen));
    
    
        
    public SpriteTileRootNode CreateSpriteTileRootNode<TEditor>(
        string title,
        string? icon = null,
        Action<TEditor>? onOpen = null)
        where TEditor : EditorPageViewModel
        => Create<SpriteTileRootNode>(typeof(TEditor), title, icon, CreateCallback(onOpen));
}