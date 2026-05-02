

using System;
using Microsoft.Extensions.DependencyInjection;
using RogueEssence.Content;
using RogueEssence.Data;
using RogueEssence.Dev.Services;
using RogueEssence.Dev.ViewModels;

namespace RogueEssence.Dev
{
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
        
        public OpenEditorNode CreateOpenEditorNode<TEditor>(string title, string? icon = null)
            where TEditor : EditorPageViewModel
            => Create<OpenEditorNode>(title, typeof(TEditor), icon);
        
        public OpenEditorNodeWithParams CreateOpenEditorNodeWithParams<TEditor>(
            string title,
            object[] extraParams,
            string? icon = null)
            where TEditor : EditorPageViewModel
            => Create<OpenEditorNodeWithParams>(title, typeof(TEditor), extraParams, icon);
        
        public DataRootNode CreateDataRootNode<TEditor>(DataManager.DataType dataType, string title, string? icon = null)
            where TEditor : EditorPageViewModel
            => Create<DataRootNode>(dataType, typeof(TEditor), title, icon);
        
        public ModRootNode CreateModRootNode<TEditor>(string title, string? icon = null)
            where TEditor : EditorPageViewModel
            => Create<ModRootNode>(typeof(TEditor), title, icon);
        
        public DataItemNode CreateDataItemNode<TEditor>(string key, string title, string? icon = null)
            where TEditor : EditorPageViewModel
            => Create<DataItemNode>(key, typeof(TEditor), title, icon);

        public PageNode CreatePageNode(EditorPageViewModel childPage, PageNode? parentNode = null)
            => new PageNode(_serviceProvider.GetService<IDialogService>(), this, childPage, parentNode);
        
        public SpriteRootNode CreateSpriteRootNode<TEditor>(GraphicsManager.AssetType assetType, string title, string? icon = null)
            where TEditor : EditorPageViewModel
            => Create<SpriteRootNode>(assetType, typeof(TEditor), title, icon);
        
        public SpriteTileRootNode CreateSpriteTileRootNode<TEditor>(string title, string? icon = null)
            where TEditor : EditorPageViewModel
            => Create<SpriteTileRootNode>(typeof(TEditor), title, icon);
        
        public OpenEditorNodeFX<T> CreateOpenEditorNodeFX<T, TEditor>(
            string title, 
            Func<T> getter, 
            Action<T> setter,
            string? icon = null)
            where TEditor : EditorPageViewModel
            => Create<OpenEditorNodeFX<T>>(title, typeof(TEditor), getter, setter, icon);
    }
}