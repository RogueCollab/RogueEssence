using System;
using Avalonia;
using Avalonia.ReactiveUI;

namespace RogueEssence.Dev
{
    public class Program
    {

        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args)
        {
            Native.OS.SetupDataDir();
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
        {
            var builder = AppBuilder.Configure<App>();
            builder.UsePlatformDetect();
            builder.LogToTrace();
            // builder.WithInterFont();
            builder.UseReactiveUI();
            Native.OS.SetupApp(builder);
            return builder;
        }
    }
}
