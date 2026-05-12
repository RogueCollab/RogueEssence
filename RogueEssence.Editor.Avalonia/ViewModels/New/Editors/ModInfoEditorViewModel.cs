using System;

namespace RogueEssence.Dev.ViewModels;

using Dev.Services;

public class ModInfoEditorViewModel : EditorPageViewModel
{
    // public override string? Title => "Mod Info";
    
    // public override bool AddNewTab => true;

    public TestComboBoxViewModel Fruits { get; }
    
    public ModInfoEditorViewModel(EditorContext context, NodeBase node, Action<EditorPageViewModel> onPageOpen = null) : base(context, node, onPageOpen) 
    {
       
        Fruits = new TestComboBoxViewModel();
    }
    
    // public ModInfoEditorViewModel() : base(new PageFactory(new DesignServiceProvider()), new TabEvents(new PageFactory(new DesignServiceProvider())), new DialogService())
    // {
    //     // Title = "Dev Control";
    //     Fruits = new TestComboBoxViewModel();
    // }
    
}