
using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
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
        private readonly Dictionary<string, Type> _map = new();

        public PageFactory(IServiceProvider provider)
        {
            _provider = provider;
        }

        public void Register<TPage>(string key) where TPage : EditorPageViewModel
        {
            _map[key] = typeof(TPage);
        }
    
        public EditorPageViewModel? CreatePage(string key)
        {
            if (_map.TryGetValue(key, out var type))
            {
                return (EditorPageViewModel)_provider.GetRequiredService(type);
            }
    
            return null;
        }
    
        // TODO: see if create page or the one above is better...
        public T CreatePage<T>() where T : EditorPageViewModel
        {
            Console.WriteLine("Don't use this...");
            return _provider.GetRequiredService<T>();
        }
    
        public void PrintRegisteredPages()
        {
            Console.WriteLine("Registered pages:");
            foreach (var entry in _map)
            {
                Console.WriteLine($"Key: {entry.Key}, Type: {entry.Value.FullName}");
            }
        }
    
        public T GetRequiredService<T>() where T : notnull
        {
            return _provider.GetRequiredService<T>();
        }
    }
}



