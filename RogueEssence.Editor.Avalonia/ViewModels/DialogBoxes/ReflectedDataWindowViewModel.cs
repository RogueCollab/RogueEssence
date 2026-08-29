using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using ReactiveUI;

namespace RogueEssence.Dev.ViewModels
{
    public class ReflectedDataWindowViewModel : ViewModelBase
    {
        public Action<StackPanel> OnLoadAction;
        public Func<StackPanel, Task<bool>> OnOKAction;

        public ReflectedDataWindowViewModel()
        {
            
        }
    }
}
