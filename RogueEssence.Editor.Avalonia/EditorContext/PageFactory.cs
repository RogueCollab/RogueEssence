
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using RogueEssence.Data;
using RogueEssence.Dev.ViewModels;

namespace RogueEssence.Dev
{
    public class DesignServiceProvider : IServiceProvider
    {
        private readonly IServiceProvider _serviceProvider;

        public DesignServiceProvider()
        {
            var collection = new ServiceCollection();
            collection.AddCommonServices();
            _serviceProvider = collection.BuildServiceProvider();
            _serviceProvider.RegisterPages();
        }

        public object? GetService(Type serviceType)
        {
            return _serviceProvider.GetService(serviceType);
        }
    
        public T? GetService<T>()
        {
            return _serviceProvider.GetService<T>();
        }
    
        public T GetRequiredService<T>() where T : notnull
        {
            return _serviceProvider.GetRequiredService<T>();
        }

    }



    public class PageFactory
    {
        private readonly IServiceProvider _provider;
        private readonly HashSet<Type> _registeredTypes = new();

        public PageFactory(IServiceProvider provider)
        {
            _provider = provider;
        }

        public void Register<TPage>() where TPage : EditorPageViewModel
        {
            _registeredTypes.Add(typeof(TPage));
        }

        public TPage? CreatePage<TPage>() where TPage : EditorPageViewModel
        {
            var type = typeof(TPage);
        
            if (!_registeredTypes.Contains(type))
            {
                return null;
            }

            return (TPage)_provider.GetRequiredService(type);
        }
        
        public EditorPageViewModel? CreatePage(Type pageType, NodeBase? node = null, Action<EditorPageViewModel> onOpen = null)
        {
            
            // Make sure it's a page
            if (!typeof(EditorPageViewModel).IsAssignableFrom(pageType))
            {
                throw new ArgumentException(
                    $"Type {pageType.Name} must derive from EditorPageViewModel", 
                    nameof(pageType));
            }
            
            if (!_registeredTypes.Contains(pageType))
            {
                return null;
            }
            
            EditorPageViewModel page;

       
            if (node != null)
            {
                if (onOpen == null)
                {
                    onOpen = (vm) => { };
                }
                page = (EditorPageViewModel)ActivatorUtilities.CreateInstance(_provider, pageType, node, onOpen);
            }
            else
            {
                page = (EditorPageViewModel)_provider.GetRequiredService(pageType);
            }
            
            return page;
        }

        
        // public EditorPageViewModel? CreatePage(Type pageType)
        // {
        //     
        //     if (!typeof(EditorPageViewModel).IsAssignableFrom(pageType))
        //     {
        //         throw new ArgumentException(
        //             $"Type {pageType.Name} must derive from EditorPageViewModel", 
        //             nameof(pageType));
        //     }
        //     
        //     if (!_registeredTypes.Contains(pageType))
        //     {
        //         return null;
        //     }
        //     
        //     return (EditorPageViewModel)_provider.GetRequiredService(pageType);
        // }
        
        public TPage? CreatePage<TPage>(NodeBase? node, Action<EditorPageViewModel> onOpen = null) where TPage : EditorPageViewModel
        {

            if (onOpen == null)
            {
                onOpen = (vm) => { };
            }
            
            var type = typeof(TPage);
        
            if (!_registeredTypes.Contains(type))
            {
                return null;
            }

            if (node != null)
            {
                return (TPage)ActivatorUtilities.CreateInstance(_provider, type, node, onOpen);
            }
        
            return (TPage)_provider.GetRequiredService(type);
        }
        
        // // TODO: see if create page or the one above is better...
        // public T CreatePage<T>() where T : EditorPageViewModel
        // {
        //     Console.WriteLine("Don't use this...");
        //     return _provider.GetRequiredService<T>();
        // }
        //
        public void PrintRegisteredPages()
        {
            Console.WriteLine("Registered pages:");
            foreach (var entry in _registeredTypes)
            {
                Console.WriteLine($"Type: {entry}");
            }
        }
    
        public T GetRequiredService<T>() where T : notnull
        {
            return _provider.GetRequiredService<T>();
        }
    }
}



