using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using RogueEssence.Dev.Services;
using RogueEssence.Dev.ViewModels;

namespace RogueEssence.Dev.Views
{
    public partial class MessageBoxWindowView : ChromelessWindow
    {
        public enum MessageBoxButtons
        {
            Ok,
            OkCancel,
            YesNo,
            YesNoCancel
        }

        public enum MessageBoxResult
        {
            Ok,
            Cancel,
            Yes,
            No
        }

        public MessageBoxWindowView()
        {
            InitializeComponent();
        }

        public static async Task<MessageBoxResult> Show(
            string text,
            string title,
            MessageBoxButtons buttons,
            IDialogService dialogService, 
            bool danger = false)
        {
            var vm = new MessageBoxWindowViewModel(title, text);

            void AddButton(string caption, MessageBoxResult result, bool _danger = false)
            {
                vm.Buttons.Add(new MessageBoxWindowViewModel.ButtonViewModel(caption, result, vm, _danger));
            }

            switch (buttons)
            {
                case MessageBoxButtons.Ok:
                    AddButton("Ok", MessageBoxResult.Ok, danger);
                    break;
                case MessageBoxButtons.OkCancel:
                    AddButton("Cancel", MessageBoxResult.Cancel, false);
                    AddButton("Ok", MessageBoxResult.Ok, danger);
                    break;
                case MessageBoxButtons.YesNo:
                    AddButton("No", MessageBoxResult.No, false);
                    AddButton("Yes", MessageBoxResult.Yes, danger);
                    break;
                case MessageBoxButtons.YesNoCancel:
                    AddButton("No", MessageBoxResult.No, false);
                    AddButton("Cancel", MessageBoxResult.Cancel, false);
                    AddButton("Yes", MessageBoxResult.Yes, danger);
                    break;
            }
            
            vm.CloseRequested += (_, result) => dialogService.Close(vm, result);

            var resultValue = await dialogService.ShowDialogAsync<MessageBoxWindowViewModel, MessageBoxResult>(vm, title);
            return resultValue;
        }
    }
}
