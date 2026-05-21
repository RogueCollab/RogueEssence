using System.Threading.Tasks;
using ReactiveUI;

namespace RogueEssence.Dev.ViewModels;


public class DialogViewModel : ViewModelBase
{
    private bool _isDialogOpen;

    public bool IsDialogOpen
    {
        get => _isDialogOpen;
        set { this.RaiseAndSetIfChanged(ref _isDialogOpen, value); }
    }

    protected TaskCompletionSource closeTask = new TaskCompletionSource();

    public async Task WaitAsnyc()
    {
        await closeTask.Task;
    }
    
    public void Show()
    {
        if (closeTask.Task.IsCompleted)
            closeTask = new TaskCompletionSource();
    
        IsDialogOpen = true;
    }

    public void Close()
    {
        IsDialogOpen = false;
        
        closeTask.TrySetResult();
    }
}