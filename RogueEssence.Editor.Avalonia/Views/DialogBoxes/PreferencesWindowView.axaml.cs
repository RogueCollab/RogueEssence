using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace RogueEssence.Dev.Views;

public partial class PreferencesWindowView: ChromelessWindow
{
    public PreferencesWindowView()
    {
        var pref = ViewModels.PreferencesWindowViewModel.Instance;
        DataContext = pref;
        CloseOnESC = true;
        InitializeComponent();
    }
    
    protected override void OnClosing(WindowClosingEventArgs e)
    {
        base.OnClosing(e);

        if (Design.IsDesignMode)
            return;

        ViewModels.PreferencesWindowViewModel.Instance.Save();
    }

}