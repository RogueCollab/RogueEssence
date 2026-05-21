using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using RogueEssence.Dev.ViewModels;

namespace RogueEssence.Dev.Views;

public partial class ModConfigPageView : UserControl
{
    public ModConfigPageView()
    {
        InitializeComponent();
    }
    
    public async void btnOK_Click(object sender, RoutedEventArgs e)
    {
        ModConfigPageViewModel vm = (ModConfigPageViewModel)DataContext;

        bool isValid = await vm.Validate();

        if (isValid)
        {
            vm.OnOKValidAction.Invoke();
            vm.Close();
        }
           
    }


    public void btnCancel_Click(object sender, RoutedEventArgs e)
    {
        
        ModConfigPageViewModel vm = (ModConfigPageViewModel)DataContext;
        vm.Close();
    }
}