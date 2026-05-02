using System;
using System.Collections.ObjectModel;
using System.Reactive;
using ReactiveUI;
using RogueEssence.Dev.Views;

namespace RogueEssence.Dev.ViewModels
{
    public class MessageBoxWindowViewModel : ReactiveObject
    {
        public string Title { get; }
        public string Text { get; }
        public ObservableCollection<ButtonViewModel> Buttons { get; } = new();

        
        public event EventHandler<MessageBoxWindowView.MessageBoxResult>? CloseRequested;

        public MessageBoxWindowViewModel(string title, string text)
        {
            Title = title;
            Text = text;
        }

        public void RequestClose(MessageBoxWindowView.MessageBoxResult result)
        {
            CloseRequested?.Invoke(this, result);
        }

        public class ButtonViewModel
        {
            public bool IsPrimary => (Caption == "Yes" || Caption == "Ok") && !Danger;
            public bool Danger { get; }
            public string Caption { get; }
            public MessageBoxWindowView.MessageBoxResult Result { get; }
            public ReactiveCommand<Unit, Unit> Command { get; }

            public ButtonViewModel(string caption, MessageBoxWindowView.MessageBoxResult result, MessageBoxWindowViewModel owner, bool danger)
            {
                Caption = caption;
                Result = result;
                Danger = danger;
                Command = ReactiveCommand.Create(() => owner.RequestClose(result));
            }
        }
    }
}