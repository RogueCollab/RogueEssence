using System;
using Avalonia.Controls;
using RogueEssence.Dev.Services;
using RogueEssence.Dev.ViewModels;

namespace RogueEssence.Dev;
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