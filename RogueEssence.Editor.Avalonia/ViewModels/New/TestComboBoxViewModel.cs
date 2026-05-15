using System.Collections.Generic;
using System.Collections.ObjectModel;
using ReactiveUI;

namespace RogueEssence.Dev.ViewModels;

public class TestComboBoxViewModel : ViewModelBase
{
    private ObservableCollection<string> _fruits = new ObservableCollection<string>
    {
        "Pineapple", "Banana", "Cherry", "Dragonfruit",
        "Elderberry", "Fig", "Grapes", "Honeydew"
    };
    
    public ObservableCollection<string> Fruits
    {
        get => _fruits;
        set => this.RaiseAndSetIfChanged(ref _fruits, value);
    }
    
    private string? _selectedFruit;
    public string? SelectedFruit
    {
        get => _selectedFruit;
        set => this.RaiseAndSetIfChanged(ref _selectedFruit, value);
    }
    
     

    public TestComboBoxViewModel()
    {
        Fruits = new ObservableCollection<string>
        {
            "Pineapple", "Banana", "Cherry", "Dragonfruit",
            "Elderberry", "Fig", "Grapes", "Honeydew"
        };
    }
        
    
    
}