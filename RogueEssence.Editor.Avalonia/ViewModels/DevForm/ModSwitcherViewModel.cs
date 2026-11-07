using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using RogueEssence.Dev;
using RogueEssence.Dev.Services;
using ReactiveUI;
using RogueEssence.Dev.Views;
using RogueEssence.Menu;
using RogueEssence.Script;

namespace RogueEssence.Dev.ViewModels;

public class ModSwitcherViewModel : ViewModelBase
{
    private readonly DevFormViewModel _mainWindow;
    private ModsEntryViewModel _selectedMod;
    private ModManagerViewModel _modManager;
    public ModsEntryViewModel SelectedMod
    {
        get => _selectedMod;
        set { this.RaiseAndSetIfChanged(ref _selectedMod, value); }
    }
    
    public ObservableCollection<ModsEntryViewModel> Mods { get; }
    public ModSwitcherViewModel(DevFormViewModel mainWindowViewModel, ModManagerViewModel modManager)
    {
        _modManager = modManager;
        _mainWindow = mainWindowViewModel;
        

        Mods = new ObservableCollection<ModsEntryViewModel>(mainWindowViewModel.ModsManager.ModsList);
    }
    
    
    public void CloseSwitcher()
    {
        _mainWindow.OnModSwitcherClosed();
    }
    
    public async Task ConfirmModSwitchAsync()
    {
        await _modManager.AskSwitchTo(SelectedMod);
    }
    

    public async Task<bool> CheckIsCurrentMod(ModsEntryViewModel mod)
    { 
        return await _modManager.CheckIsCurrentMod(mod);
    }
    
    
    
}


