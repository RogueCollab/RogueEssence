using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Shapes;
using Avalonia.Platform.Storage;
using Avalonia.Threading;

namespace RogueEssence.Dev.Services
{
    public interface IDialogService
    {
        Task<TResult> ShowDialogAsync<TViewModel, TResult>(TViewModel viewModel, string title = "")
            where TViewModel : class;

        void Close<TViewModel, TResult>(TViewModel viewModel, TResult result)
            where TViewModel : class;

        Task<string?> ShowFolderPickerAsync(FolderPickerOpenOptions options, string folderName = "");
        Task<string?> ShowFilePickerAsync(FilePickerOpenOptions options, string folderName = "");
        
        Task<string?> TryGetSaveFileAsync(FilePickerSaveOptions options, string folderName = "");
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

        public async Task<TResult?> ShowDialogAsync<TViewModel, TResult>(TViewModel viewModel, string title = "")
            where TViewModel : class
        {
            var control = _viewLocator.Build(viewModel);
            if (control is not Window window)
                throw new InvalidOperationException($"View for {typeof(TViewModel).Name} must be a Window.");

            window.DataContext = viewModel;
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
        public async Task<string?> ShowFolderPickerAsync(FolderPickerOpenOptions options, string folderName)
        {

            var topLevel = _topLevelFunc();
            
            if (folderName != "")
            {
                options.SuggestedStartLocation = await topLevel.StorageProvider.TryGetFolderFromPathAsync(folderName);
            }

            var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(options);
            
            var path = folders.FirstOrDefault()?.Path;
            return path?.LocalPath;
        }
  
        public async Task<string?> ShowFilePickerAsync(FilePickerOpenOptions options, string folderName)
        {
            var topLevel = _topLevelFunc();
            
            if (folderName != "")
            {
                options.SuggestedStartLocation = await topLevel.StorageProvider.TryGetFolderFromPathAsync(folderName);
            }

            var files = await topLevel.StorageProvider.OpenFilePickerAsync(options);
            
            if (files.Count == 0)
                return null;

            return files[0].TryGetLocalPath();
        }
        
        public async Task<string?> TryGetSaveFileAsync(FilePickerSaveOptions options, string folderName)
        {
            var topLevel = _topLevelFunc();

            if (folderName != "")
            {
                options.SuggestedStartLocation = await topLevel.StorageProvider.TryGetFolderFromPathAsync(folderName);
            }
            
            var result = await topLevel.StorageProvider.SaveFilePickerAsync(options);
            return result?.Path.LocalPath;
        }
    }
}