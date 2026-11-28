using System;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using RogueEssence.Dev.ViewModels;

namespace RogueEssence.Dev.Views;

public partial class SpeciesEditPageView : UserControl
{
    public SpeciesEditPageView()
    {
        InitializeComponent();
    }
    
    private void SpeciesEditTreeDataGrid_OnSelectionChanging(object sender, CancelEventArgs e)
    {
        if (sender is not TreeDataGrid grid || DataContext is not SpeciesEditPageViewModel vm) return;
        Dispatcher.UIThread.Post(() =>
        {
            vm.ChosenMonster = grid.RowSelection?.SelectedItem as MonsterNodeViewModel;
            grid.InvalidateMeasure();
        });
    }
    
}