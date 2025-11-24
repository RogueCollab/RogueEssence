using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using RogueEssence.Dev;
using RogueEssence.Dev.Services;
using ReactiveUI;
using RogueEssence.Dev.Views;

namespace RogueEssence.Dev.ViewModels;

public class ModSwitcherViewModel : ViewModelBase
{

    private readonly IDialogService _dialogService;
    private readonly DevFormViewModel _mainWindow;
    private ModsNodeViewModel _selectedMod;
    
    public ModsNodeViewModel SelectedMod
    {
        get => _selectedMod;
        set { this.RaiseAndSetIfChanged(ref _selectedMod, value); }
    }
    
    public ObservableCollection<ModsNodeViewModel> Mods { get; }
    public ModSwitcherViewModel(DevFormViewModel mainWindowViewModel, IDialogService dialogService)
    {
        _mainWindow = mainWindowViewModel;
        _dialogService = dialogService;

        Mods = new ObservableCollection<ModsNodeViewModel>(mainWindowViewModel.Mods.Mods);
    }
    
    
    public void CloseSwitcher()
    {
        _mainWindow.OnModSwitcherClosed();
    }
    
    public async Task<MessageBoxWindowView.MessageBoxResult> ConfirmModSwitchAsync()
    {
        return await MessageBoxWindowView.Show(_dialogService, "The game will be reloaded to use content from the new path. Click OK to proceed. Your changes will not be saved.", "Are you sure?", MessageBoxWindowView.MessageBoxButtons.OkCancel);
    }
    
    public ModsNodeViewModel CurrentMod
    {
        get => _mainWindow.Mods.ChosenMod;
        set => _mainWindow.Mods.ChosenMod = value;
    }

    public bool IsCurrent(ModsNodeViewModel head)
    { 
        return head.Namespace == _mainWindow.Mods.ChosenMod.Namespace;
    }
}


