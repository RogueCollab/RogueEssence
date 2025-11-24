using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Linq;
using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input.Platform;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;
using RogueEssence.Dev.Services;
using RogueEssence.Dev.ViewModels;
using RogueEssence.Dev.Views;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;

namespace RogueEssence.Dev
{

    public static class ServiceCollectionExtensions
    {
        public static void AddCommonServices(this IServiceCollection collection)
        {
            collection.AddSingleton<TabEvents>();
            collection.AddSingleton<PageFactory>();
            collection.AddSingleton<NodeFactory>();
            collection.AddSingleton<DevFormViewModel>();

       
            AddDevTabViewModels(collection);
            
            collection.AddTransient<TabSwitcherViewModel>(sp =>
            {
                var mainVm = sp.GetRequiredService<DevFormViewModel>();
                return new TabSwitcherViewModel(mainVm);
            });

            collection.AddTransient<ModSwitcherViewModel>();


            collection.AddTransient<NodeBase>();
            collection.AddTransient<DataRootNode>();
            collection.AddTransient<DataItemNode>();
            collection.AddTransient<OpenEditorNode>();
            collection.AddTransient<PageNode>();

            // TODO: remove?
            // collection.AddSingleton<Func<Type, NodeBase>>(x => type => type switch
            // {
            //     _ when type == typeof(NodeBase) => x.GetRequiredService<NodeBase>(),
            //     _ when type == typeof(DataRootNode) => x.GetRequiredService<DataRootNode>(),
            //     _ when type == typeof(DataItemNode) => x.GetRequiredService<DataItemNode>(),
            //     _ when type == typeof(OpenEditorNode) => x.GetRequiredService<OpenEditorNode>(),
            //     _ when type == typeof(PageNode) => x.GetRequiredService<PageNode>(),
            //     _ => throw new InvalidOperationException($"Page of type {type?.FullName} has no view model"),
            // });

            collection.AddSingleton<ViewLocator>();
            collection.AddSingleton<IDialogService, DialogService>();

            collection.AddTransient<DevControlViewModel>();
            collection.AddTransient<ZoneEditorPageViewModel>();
            collection.AddTransient<GroundEditorPageViewModel>();
            collection.AddTransient<RandomInfoPageViewModel>();
            collection.AddTransient<SpritePageViewModel>();
            collection.AddTransient<ModInfoEditorViewModel>();
            collection.AddTransient<DevEditPageViewModel>();
            collection.AddTransient<SpeciesEditPageViewModel>();
        }
        
        private static void AddDevTabViewModels(this IServiceCollection services)
        {
            services.AddSingleton<DevTabGameViewModel>();
            services.AddSingleton<DevTabPlayerViewModel>();
            services.AddSingleton<DevTabDataViewModel>();
            services.AddSingleton<DevTabTravelViewModel>();
            services.AddSingleton<DevTabSpritesViewModel>();
            services.AddSingleton<DevTabScriptViewModel>();
            services.AddSingleton<DevTabModsViewModel>();
            services.AddSingleton<DevTabConstantsViewModel>();
        }

        public static void RegisterPages(this IServiceProvider provider)
        {
            var pageFactory = provider.GetRequiredService<PageFactory>();

            pageFactory.Register<DevControlViewModel>("DevControl");
            pageFactory.Register<ZoneEditorPageViewModel>("ZoneEditor");
            pageFactory.Register<GroundEditorPageViewModel>("GroundEditor");
            pageFactory.Register<RandomInfoPageViewModel>("RandomInfo");
            pageFactory.Register<SpritePageViewModel>("SpritePage");
            pageFactory.Register<ModInfoEditorViewModel>("ModInfoEditor");
            pageFactory.Register<DevEditPageViewModel>("DevEditEditor");
            pageFactory.Register<SpeciesEditPageViewModel>("SpeciesSpriteEditor");
        }
    }

    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
            var pref = PreferencesWindowViewModel.Instance;

            // SetLocale(pref.Locale);

            pref.Changed
                .Where(_ => !pref.IsLoading)
                .Subscribe(_ => { pref.Save(); });
        }

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
            var collection = new ServiceCollection();
            collection.AddCommonServices();

            // TopLevel provider
            collection.AddSingleton<Func<TopLevel?>>(x => () =>
            {
                if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime topWindow)
                    return TopLevel.GetTopLevel(topWindow.MainWindow);

                if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
                    return TopLevel.GetTopLevel(singleViewPlatform.MainView);

                return null;
            });

            var services = collection.BuildServiceProvider();

            services.RegisterPages();


            var vm = services.GetRequiredService<DevFormViewModel>();

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new DevForm
                {
                    DataContext = vm
                };
            }
            else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
            {
                singleViewPlatform.MainView = new DevForm
                {
                    DataContext = vm
                };
            }

            base.OnFrameworkInitializationCompleted();
        }


        public static void SetLocale(string localeKey)
        {
            if (Current is not App app ||
                app.Resources[localeKey] is not ResourceDictionary targetLocale ||
                targetLocale == app._activeLocale)
                return;

            if (app._activeLocale != null)
                app.Resources.MergedDictionaries.Remove(app._activeLocale);

            app.Resources.MergedDictionaries.Add(targetLocale);
            app._activeLocale = targetLocale;
        }

        public static Avalonia.Controls.Shapes.Path CreateMenuIcon(string key)
        {
  
            var icon = new Avalonia.Controls.Shapes.Path();
            icon.Width = 12;
            icon.Height = 12;
            icon.Stretch = Stretch.Uniform;

            if (Current?.FindResource(key) is StreamGeometry geo)
                icon.Data = geo;

            return icon;
        }

        public static void SetTheme(string theme, string _)
        {
            if (Current is not App app)
                return;
            if (theme.Equals("Light", StringComparison.OrdinalIgnoreCase))
                app.RequestedThemeVariant = ThemeVariant.Light;
            else if (theme.Equals("Dark", StringComparison.OrdinalIgnoreCase))
                app.RequestedThemeVariant = ThemeVariant.Dark;
            else
                app.RequestedThemeVariant = ThemeVariant.Default;

            if (app._themeOverrides != null)
            {
                app.Resources.MergedDictionaries.Remove(app._themeOverrides);
                app._themeOverrides = null;
            }
            // if (app._themeOverrides != null)
            // {
            //     app.Resources.MergedDictionaries.Remove(app._themeOverrides);
            //     app._themeOverrides = null;
            // }
            //
            // if (!string.IsNullOrEmpty(themeOverridesFile) && File.Exists(themeOverridesFile))
            // {
            //     try
            //     {
            //         var resDic = new ResourceDictionary();
            //         using var stream = File.OpenRead(themeOverridesFile);
            //         var overrides = JsonSerializer.Deserialize(stream, JsonCodeGen.Default.ThemeOverrides);
            //         foreach (var kv in overrides.BasicColors)
            //         {
            //             if (kv.Key.Equals("SystemAccentColor", StringComparison.Ordinal))
            //                 resDic["SystemAccentColor"] = kv.Value;
            //             else
            //                 resDic[$"Color.{kv.Key}"] = kv.Value;
            //         }
            //
            //         app.Resources.MergedDictionaries.Add(resDic);
            //         app._themeOverrides = resDic;
            //     }
            //     catch
            //     {
            //         // ignore
            //     }
            // }
            //
        }

        private ResourceDictionary _activeLocale = null;
        private ResourceDictionary _themeOverrides = null;
    }
}