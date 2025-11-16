

using System;
using Microsoft.Extensions.DependencyInjection;
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

        public OpenEditorNode CreateOpenEditorNode(string title, string? icon = null, string editorKey = "")
            => Create<OpenEditorNode>(title, icon, editorKey);

        public DataRootNode CreateDataRootNode(string dataType, string editorKey, string title, string? icon = null)
        {
            return Create<DataRootNode>(dataType, editorKey, title, icon);
        }

        public DataItemNode CreateDataItemNode(string key, string editorKey, string title, string? icon = null)
            => Create<DataItemNode>(key, editorKey, title, icon);

        // I cannot for the life of figure out of why none of the create methods work... but I guess this will do
        public PageNode CreatePageNode(EditorPageViewModel childPage, PageNode? parentNode = null)

            => new PageNode(_serviceProvider.GetService<IDialogService>(), this, childPage, parentNode);
        // => Create<PageNode>(childPage, parentNode);
        // => Create<PageNode>(this, childPage, parentNode);


        public SpriteRootNode CreateSpriteRootNode(string dataType, string editorKey, string title, string? icon = null)
            => Create<SpriteRootNode>(dataType, editorKey, title, icon);

    }
}
