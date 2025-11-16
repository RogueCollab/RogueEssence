using ReactiveUI;

using System;
using System.Collections.ObjectModel;
using RogueEssence.Dev.Services;


namespace RogueEssence.Dev.ViewModels;

public class EditorPageViewModel : ViewModelBase
{
    public virtual string? UniqueId => null;
    
    public virtual bool AddNewTab => true;
    
    
    private string _data = "";

    private string _title = "";
    public virtual string Title
    {
        get { return _title; }
        set
        {
            this.RaiseAndSetIfChanged(ref _title, value);
        }
    }

    public string Data
    {
        get { return _data; }
        set
        {
            this.RaiseAndSetIfChanged(ref _data, value);
        }
    }
    
 

    
    private string? _icon = "";
    public string Icon
    {
        get { return _icon; }
        set
        {
            this.RaiseAndSetIfChanged(ref _icon, value);
        }
    }
    
    
       private bool _modified = false;
        public bool Modified
        {
            get { return _modified; }
            set
            {
                this.RaiseAndSetIfChanged(ref _modified, value);
            }
        }
    private readonly TabEvents _tabEvents;
    
    public EditorPageViewModel(PageFactory pageFactory, TabEvents tabEvents, IDialogService dialogService)
    {
        // Children = new ObservableCollection<EditorPageViewModel>();
        _tabEvents = tabEvents;
    }

    public void SetTabInfo(NodeBase node)
    {
        _icon = node._icon;
        _title = node.Title;
    }
    
    public void SetTitle(string title)
    {
        _title = title;
    }
    
    public void SetIcon(string icon)
    {
        _icon = icon;
    }
    
    // public RepositoryNode Node
    // {
    //     get => _node;
    //     set => SetProperty(ref _node, value);
    // }

    // public LauncherPage()
    // {
    // _node = new RepositoryNode() { Id = Guid.NewGuid().ToString() };
    // _data = Welcome.Instance;

    // New welcome page will clear the search filter before.
    // Welcome.Instance.ClearSearchFilter();
    // }


    // public class LauncherPage : ObservableObject
    // {
    //     public RepositoryNode Node
    //     {
    //         get => _node;
    //         set => SetProperty(ref _node, value);
    //     }
    //
    //     public object Data
    //     {
    //         get => _data;
    //         set => SetProperty(ref _data, value);
    //     }
    //
    //     public Models.DirtyState DirtyState
    //     {
    //         get => _dirtyState;
    //         private set => SetProperty(ref _dirtyState, value);
    //     }
    //
    //     public Popup Popup
    //     {
    //         get => _popup;
    //         set => SetProperty(ref _popup, value);
    //     }
    //
    //     public AvaloniaList<Models.Notification> Notifications
    //     {
    //         get;
    //         set;
    //     } = new AvaloniaList<Models.Notification>();
    //
    //     public LauncherPage()
    //     {
    //         _node = new RepositoryNode() { Id = Guid.NewGuid().ToString() };
    //         _data = Welcome.Instance;
    //
    //         // New welcome page will clear the search filter before.
    //         Welcome.Instance.ClearSearchFilter();
    //     }
    //
    //     public LauncherPage(RepositoryNode node, Repository repo)
    //     {
    //         _node = node;
    //         _data = repo;
    //     }
    //
    //     public void ClearNotifications()
    //     {
    //         Notifications.Clear();
    //     }
    //
    //     public async Task CopyPathAsync()
    //     {
    //         if (_node.IsRepository)
    //             await App.CopyTextAsync(_node.Id);
    //     }
    //
    //     public void ChangeDirtyState(Models.DirtyState flag, bool remove)
    //     {
    //         var state = _dirtyState;
    //         if (remove)
    //         {
    //             if (state.HasFlag(flag))
    //                 state -= flag;
    //         }
    //         else
    //         {
    //             state |= flag;
    //         }
    //
    //         DirtyState = state;
    //     }
    //
    //     public bool CanCreatePopup()
    //     {
    //         return _popup is not { InProgress: true };
    //     }
    //
    //     public async Task ProcessPopupAsync()
    //     {
    //         if (_popup is { InProgress: false } dump)
    //         {
    //             if (!dump.Check())
    //                 return;
    //
    //             dump.InProgress = true;
    //
    //             try
    //             {
    //                 var finished = await dump.Sure();
    //                 if (finished)
    //                 {
    //                     dump.Cleanup();
    //                     Popup = null;
    //                 }
    //             }
    //             catch (Exception e)
    //             {
    //                 App.LogException(e);
    //             }
    //
    //             dump.InProgress = false;
    //         }
    //     }
    //
    //     public void CancelPopup()
    //     {
    //         if (_popup == null || _popup.InProgress)
    //             return;
    //
    //         _popup?.Cleanup();
    //         Popup = null;
    //     }
    //
    //     private RepositoryNode _node = null;

    //     private Models.DirtyState _dirtyState = Models.DirtyState.None;
    //     private Popup _popup = null;
    // }

}