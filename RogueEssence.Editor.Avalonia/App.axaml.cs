using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input.Platform;
using Avalonia.Markup.Xaml;
using RogueEssence.Dev.ViewModels;
using RogueEssence.Dev.Views;

namespace RogueEssence.Dev
{
    public class App : Application
    {

        
        public static async void CopyText(string data)
        {
            if (Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                if (desktop.MainWindow.Clipboard is IClipboard clipbord)
                {
                    await clipbord.SetTextAsync(data);
                }
            }
        }

        
        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new DevForm
                {
                    DataContext = new DevFormViewModel(),
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
