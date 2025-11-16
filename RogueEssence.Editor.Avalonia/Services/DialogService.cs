using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;

namespace RogueEssence.Dev.Services
{
    public interface IDialogService
    {
        Task<TResult> ShowDialogAsync<TViewModel, TResult>(TViewModel viewModel, string title = "", bool initVm = true)
            where TViewModel : class;

        void Close<TViewModel, TResult>(TViewModel viewModel, TResult result)
            where TViewModel : class;

        Task<string?> ShowFolderPickerAsync(FolderPickerOpenOptions options);

    }

    public class DialogService : IDialogService
    {
        private readonly ViewLocator _viewLocator;
        private readonly Func<TopLevel> _topLevelFunc;

        public DialogService()
        {
            _viewLocator = new ViewLocator();
            _topLevelFunc = () =>
                (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
        }

        public DialogService(ViewLocator viewLocator, Func<TopLevel?> topLevelFunc)
        {
            _viewLocator = viewLocator;
            _topLevelFunc = topLevelFunc;
        }

        public async Task<TResult?> ShowDialogAsync<TViewModel, TResult>(TViewModel viewModel, string title = "",
            bool initVm = true)
            where TViewModel : class
        {
            var control = _viewLocator.Build(viewModel);
            if (control is not Window window)
                throw new InvalidOperationException($"View for {typeof(TViewModel).Name} must be a Window.");

            if (initVm)
            {
                // window.DataContext = viewModel;
            }

            window.Title = title;

            var mainWindow = (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)
                ?.MainWindow;
            if (mainWindow == null)
                throw new InvalidOperationException("Main window not found for dialog parent.");

            return await window.ShowDialog<TResult?>(mainWindow);
        }

        public void Close<TViewModel, TResult>(TViewModel viewModel, TResult result)
            where TViewModel : class
        {
            var window = (App.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?
                .Windows.FirstOrDefault(w => ReferenceEquals(w.DataContext, viewModel));

            window?.Close(result);
        }

        public async Task<string?> ShowFolderPickerAsync(FolderPickerOpenOptions options)
        {
            var topLevelVisual = _topLevelFunc();
            if (topLevelVisual == null) return null;

            var folders = await topLevelVisual.StorageProvider.OpenFolderPickerAsync(options);

            var path = folders.FirstOrDefault()?.Path;
            if (path == null) return null;
            return path.IsAbsoluteUri ? path.LocalPath : path.OriginalString;
        }


    }
}