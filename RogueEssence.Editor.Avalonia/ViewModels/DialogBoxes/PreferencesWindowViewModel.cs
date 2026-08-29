using System;
using System.IO;
using System.Reactive.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using ReactiveUI;

namespace RogueEssence.Dev.ViewModels;


[JsonSerializable(typeof(PreferencesWindowViewModel))]
internal partial class JsonCodeGen : JsonSerializerContext { }

public class PreferencesWindowViewModel : ViewModelBase
{
    
    private static PreferencesWindowViewModel? _instance = null;
  
    [JsonIgnore]
    public static PreferencesWindowViewModel Instance
    {
        get
        {
            if (_instance != null) return _instance;
            _instance = Load();
            _instance.IsLoading = false;
            return _instance;
        }
    }
    
    public void Save()
    {
        if (_isLoading)
            return;
      
        var file = Path.Combine(Native.OS.DataDir, "preference.json");
        using var stream = File.Create(file);
        
        JsonSerializer.Serialize(stream, this, JsonCodeGen.Default.PreferencesWindowViewModel);
    }

    private static PreferencesWindowViewModel Load()
    {
        var path = Path.Combine(Native.OS.DataDir, "preference.json");
        if (!File.Exists(path))
        {
            return new PreferencesWindowViewModel();
        }

        try
        {
            using var stream = File.OpenRead(path);
            var prefs = JsonSerializer.Deserialize(stream, JsonCodeGen.Default.PreferencesWindowViewModel);
            return prefs;
        }
        catch
        {
            return new PreferencesWindowViewModel();
        }
    }
    
  
    
    private bool _isLoading = true;
    
    public bool IsLoading
    {
        get => _isLoading;
        set { this.RaiseAndSetIfChanged(ref _isLoading, value); }
    }
    
    private string _locale = "en_US";
    
    
    public string Locale
    {
        get => _locale;
        set { this.RaiseAndSetIfChanged(ref _locale, value); }
    }
    
    private string _theme = "Default";
    
    public string Theme
    {
        get => _theme;
        set { this.RaiseAndSetIfChanged(ref _theme, value); }
    }

    private string _themeOverrides = string.Empty;
    public string ThemeOverrides
    {
        get => _themeOverrides;
        set { this.RaiseAndSetIfChanged(ref _themeOverrides, value); }
    }
    
    public PreferencesWindowViewModel()
    {
        this.WhenAnyValue(x => x.Theme, x => x.ThemeOverrides)
            .Where(_ => !_isLoading)
            .Subscribe(x =>
            {
                var (theme, themeOverrides) = x;
                App.SetTheme(theme, themeOverrides);
            });
        
        this.WhenAnyValue(x => x.Locale)
            .Where(_ => !_isLoading)
            .Subscribe(App.SetLocale);
    }
}