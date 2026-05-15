using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using RogueEssence.Dev.ViewModels;

namespace RogueEssence.Dev.Views;

public partial class ModListPageView : UserControl
{
    public ModListPageView()
    {
        InitializeComponent();
    }
    
    private async void ModListPageListBox_OnDoubleTapped(object sender, TappedEventArgs e)
    {
        if (DataContext is ModListPageViewModel vm)
            await vm.AddChildItemUnderParent(vm.SelectedItem);
    }
    
}
