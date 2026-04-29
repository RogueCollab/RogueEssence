using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using RogueEssence.Dev.Utility;
using RogueEssence.Dev.ViewModels;

namespace RogueEssence.Dev.Views;

public partial class DataListPageView : UserControl
{
    public DataListPageView()
    {
        InitializeComponent();
    }


    private void DataListPageListBox_OnDoubleTapped(object sender, TappedEventArgs e)
    {
        if (DataContext is DataListPageViewModel vm)
            vm.AddUnderParentNode(vm.SelectedItem);
    }


    private void DataListPageListBox_OnContextRequested(object sender, ContextRequestedEventArgs e)
    {
        if (e.Source is not Visual)
            return;

        if (sender is not ListBox listBox || listBox.SelectedItem is not DataListEntry entry ||
            listBox.DataContext is not DataListPageViewModel vm)
            return;

        var contextMenu = ContextMenuHelper.CreateDataItemMenu(vm.Node, entry.Key);
        contextMenu.Open(this);
    }

    private void DataListPageViewEditMenu_OnContextRequested(object sender, ContextRequestedEventArgs e)
    {
        if (e.Source is not Visual)
            return;

        if (sender is not ListBox listBox || listBox.SelectedItem is not DataListEntry entry ||
            listBox.DataContext is not DataListPageViewModel vm)
            return;

        var contextMenu = ContextMenuHelper.CreateDataItemMenu(vm.Node, entry.Key);
        contextMenu.Open(this);
    }
}