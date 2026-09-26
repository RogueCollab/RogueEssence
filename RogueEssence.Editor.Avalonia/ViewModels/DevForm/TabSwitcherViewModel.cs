using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Text.RegularExpressions;
using Avalonia.Media;
using DynamicData;
using ReactiveUI;
using RogueEssence.Dev.Utility;

namespace RogueEssence.Dev.ViewModels;

public class TabSwitcherViewModel: ViewModelBase
{
    public ObservableCollection<EditorPageViewModel> VisiblePages { get; } = new ObservableCollection<EditorPageViewModel>();
    
    private EditorPageViewModel? _selectedPage;

    public EditorPageViewModel? SelectedPage
    {
        get => _selectedPage;
        set => this.RaiseAndSetIfChanged(ref _selectedPage, value);
    }

    private readonly DevFormViewModel _mainWindow;

    private string _searchFilter = string.Empty;

    public string SearchFilter
    {
        get => _searchFilter;
        set { this.RaiseAndSetIfChanged(ref _searchFilter, value); }
    }

    
    public TabSwitcherViewModel(DevFormViewModel mainWindow)
    {
        _mainWindow = mainWindow;
        UpdateVisiblePages(_searchFilter);
        
        VisiblePages = new ObservableCollection<EditorPageViewModel>(_mainWindow.Pages);
        SelectedPage = _mainWindow.ActivePage;
        this.WhenAnyValue(x => x.SearchFilter).Subscribe(UpdateVisiblePages);
        
        _isTreeView = mainWindow
            .WhenAnyValue(x => x.IsTreeView)
            .ToProperty(this, x => x.IsTreeView);
    }

    // public TabSwitcherViewModel() : this(new DevFormViewModel()) {}
    
    public void ClearFilter()
    {
        SearchFilter = string.Empty;
    }

    
    private readonly ObservableAsPropertyHelper<bool> _isTreeView;
    public bool IsTreeView => _isTreeView.Value;
    
    public void ToggleSearchMode()
    {
        _mainWindow.IsTreeView = !_mainWindow.IsTreeView;
    }
    
    public void Switch()
    {
        _mainWindow.ActivePage = _selectedPage;
        _mainWindow.CloseTabSwitcher();
    }
    
    public ObservableCollection<PageNode> TopLevelPages 
        => _mainWindow.TopLevelPages;
    
    private void UpdateVisiblePages(string filter)
    {
        var strategy = new BeginningTitleFilterStrategy();
        foreach (var node in _mainWindow.TopLevelPages)
        {
            NodeHelper.FilterRecursive(node, filter, strategy);
        }
        
        UpdateVisiblePagesList(filter, strategy);
    }
    
    public bool HasTemporaryTab()
    {
        return _mainWindow?.TemporaryTab != null;
    }
    
    private void UpdateVisiblePagesList(string filter, ITitleFilterStrategy titleStrategy)
    {
        VisiblePages.Clear();

        foreach (var page in _mainWindow.Pages)
        {
            if (titleStrategy.Matches(page.Title, filter))
                VisiblePages.Add(page);
        }
        
        SelectedPage = VisiblePages.Count > 0 ? VisiblePages[0] : null;
    }
    
 

}