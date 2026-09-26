using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using RogueEssence.Dev.ViewModels;

namespace RogueEssence.Dev.Views;


public partial class StringEditPageView : UserControl
{
    public StringEditPageView()
    {
        InitializeComponent();
    }

    private void StringDataGridButtonAdd_OnClick(object sender, RoutedEventArgs e)
    {
        if (DataContext is StringEditPageViewModel vm)
        {
            vm.btnAdd_Click();
            var lastItem = vm.GameStrings[vm.GameStrings.Count - 1];
            StringDataGrid.ScrollIntoView(lastItem, null);
            StringDataGrid.SelectedItem = lastItem;
        }
    }
}