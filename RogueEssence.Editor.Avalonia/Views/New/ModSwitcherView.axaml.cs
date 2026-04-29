using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using RogueEssence.Dev.Models;
using RogueEssence.Dev.ViewModels;
using RogueEssence.Menu;
using RogueEssence.Script;

namespace RogueEssence.Dev.Views;

public partial class ModSwitcherView : UserControl
{
    public ModSwitcherView()
    {
        InitializeComponent();
        ModsListBox.AttachedToVisualTree += (_, _) =>
        {
            
            Dispatcher.UIThread.Post(() =>
            {
                ModsListBox.Focus();
            },  DispatcherPriority.Loaded);
        };
    }
    
    protected override async void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (e.Key != Key.Enter || DataContext is not ViewModels.ModSwitcherViewModel switcher)
            return;

        var selected = ModsListBox.SelectedItem as ModsNodeViewModel;
        if (selected is null)
            return;
        
        if (switcher.IsCurrent(selected))
        {
            switcher.CloseSwitcher();
            e.Handled = true;
            return;
        }
        
        switcher.CloseSwitcher();
        await switcher.ConfirmModSwitchAsync();
        // if (result == MessageBoxWindowView.MessageBoxResult.Cancel)
        //     return;
        
        // switcher.CurrentMod = selected;
      

        // DevForm.ExecuteOrPend(() => _doSwitch(selected));
        e.Handled = true;
    }

    
    private void ModsListBox_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (DataContext is not ModSwitcherViewModel switcher)
            return;
        
        if (e.Key == Key.Down)
        {
            ModsListBox.SelectedIndex = (ModsListBox.SelectedIndex + 1 + ModsListBox.ItemCount) % ModsListBox.ItemCount;
            ModsListBox.Focus(NavigationMethod.Directional);
            e.Handled = true;
        }
        else if (e.Key == Key.Up)
        {
            ModsListBox.SelectedIndex = (ModsListBox.SelectedIndex - 1 + ModsListBox.ItemCount) % ModsListBox.ItemCount;
            ModsListBox.Focus(NavigationMethod.Directional);
            e.Handled = true;
        }
    }
    
    private async void OnItemTapped(object sender, TappedEventArgs e)
    {
        if (DataContext is not ModSwitcherViewModel switcher)
            return;

        var selected = ModsListBox.SelectedItem as ModsNodeViewModel;
        if (selected is null)
            return;
        ;
        if (switcher.IsCurrent(selected))
        {
            Console.WriteLine("Same");
            switcher.CloseSwitcher();
            e.Handled = true;
            return;
        }
        
        switcher.CloseSwitcher();
        await switcher.ConfirmModSwitchAsync();
        
        // switcher.CurrentMod = selected;
        // DevForm.ExecuteOrPend(() => _doSwitch(selected));
  

        e.Handled = true;
    }


}