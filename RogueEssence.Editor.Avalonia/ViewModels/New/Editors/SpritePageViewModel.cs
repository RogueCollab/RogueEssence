using System;
using System.Reactive;
using Avalonia.Platform.Storage;
using RogueEssence.Dev.Services;
using ReactiveUI;

namespace RogueEssence.Dev.ViewModels;

public class SpritePageViewModel : EditorPageViewModel
{
    public ReactiveCommand<Unit, Unit> CreateATab { get; }
    public ReactiveCommand<Unit, Unit> CreateATopTab { get; }
    public ReactiveCommand<Unit, Unit> TestDialog { get; }
    public ReactiveCommand<Unit, Unit> RemoveSelfTab { get; }
    
    // public override string Title => "Sprite Stuff";


    public SpritePageViewModel(EditorContext context, NodeBase node, Action<EditorPageViewModel> onPageOpen = null) : base(context, node, onPageOpen)
    {
        CreateATab = ReactiveCommand.Create(() => _context.TabEvents.AddChildPage(this, _context.PageFactory.CreatePage(typeof(SpritePageViewModel))));
        CreateATopTab = ReactiveCommand.Create(() => _context.TabEvents.AddTopLevelTab(_context.PageFactory.CreatePage(typeof(ModInfoEditorViewModel))));
        TestDialog = ReactiveCommand.CreateFromTask(async () =>
            {
                var rename = new RenameWindowViewModel();
                var result = await _context.DialogService.ShowFolderPickerAsync(new FolderPickerOpenOptions()
                {
                    AllowMultiple = false,
                    Title = "Select a folder"
                });
                Console.WriteLine(rename.Name);
            }
        );
        
        RemoveSelfTab = ReactiveCommand.Create( () => _context.TabEvents.RemoveTab(this));
            
        
    }
}