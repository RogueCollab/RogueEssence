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


    public SpritePageViewModel(PageFactory pageFactory, TabEvents tabEvents, IDialogService dialogService) : base(pageFactory, tabEvents, dialogService)
    {
        CreateATab = ReactiveCommand.Create(() => tabEvents.AddChildPage(this, pageFactory.CreatePage(typeof(SpritePageViewModel))));
        CreateATopTab = ReactiveCommand.Create(() => tabEvents.AddTopLevelTab(pageFactory.CreatePage(typeof(ModInfoEditorViewModel))));
        TestDialog = ReactiveCommand.CreateFromTask(async () =>
            {
                var rename = new RenameWindowViewModel();
                var result = await dialogService.ShowFolderPickerAsync(new FolderPickerOpenOptions()
                {
                    AllowMultiple = false,
                    Title = "Select a folder"
                });
                Console.WriteLine(rename.Name);
            }
        );
        
        RemoveSelfTab = ReactiveCommand.Create( () => tabEvents.RemoveTab(this));
            
        
    }
}