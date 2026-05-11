using System;
using Avalonia.Controls;
using RogueEssence.Dev.Services;
using RogueEssence.Dev.ViewModels;

namespace RogueEssence.Dev;

public static class EditorContextExtensions
{
    public static void OpenElementEditor(
        this EditorContext context,
        EditorPageViewModel pageViewModel,
        int index,
        string title,
        string elementName,
        Type elementType,
        object[] attributes,
        object element,
        bool advancedEdit,
        CollectionBoxViewModel.EditElementOp op)
    {
        NodeBase node = context.NodeFactory.CreateReflectedDataNode<ReflectedDataPageViewModel>(title, pageViewModel.Node.Icon);
        pageViewModel.Node.SubNodes.Add(node);
        
        ReflectedDataPageViewModel new_editor = context.PageFactory.CreatePage<ReflectedDataPageViewModel>(node);
        if (new_editor == null) return;
        
        new_editor.SetPageTitle(title, pageViewModel.Node.Icon);
        
        new_editor.OnLoadAction = (StackPanel stack) =>
        {
            DataEditor.LoadClassControls(stack, elementName, null, elementName, elementType, ReflectionExt.GetPassableAttributes(1, attributes), element, true, new Type[0], advancedEdit);
        };

        new_editor.OnOKAction = async (StackPanel stack) =>
        {
            element = DataEditor.SaveClassControls(stack, elementName, elementType, ReflectionExt.GetPassableAttributes(1, attributes), true, new Type[0], advancedEdit);
            op(index, element);
            return true;
        };
        
        context.TabEvents.AddChildPage(pageViewModel, new_editor);
    }
}
public class EditorContext
{
    public NodeFactory NodeFactory { get; }
    public PageFactory PageFactory { get; }
    public TabEvents TabEvents { get; }
    public IDialogService DialogService { get; }

    public EditorContext(NodeFactory nodeFactory, PageFactory pageFactory, TabEvents tabEvents, IDialogService dialogService)
    {
        NodeFactory = nodeFactory;
        PageFactory = pageFactory;
        TabEvents = tabEvents;
        DialogService = dialogService;
    }
    
    
}