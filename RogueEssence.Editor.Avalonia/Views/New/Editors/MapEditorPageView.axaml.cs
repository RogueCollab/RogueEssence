using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace RogueEssence.Dev.Views;

public class ZoneMockEntry
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string LevelRange { get; set; }
    public int MapCount { get; set; }
    public string Music { get; set; }
}
public partial class MapEditorPageView : UserControl
{
    public MapEditorPageView()
    {
        InitializeComponent();
        // ZoneDataGrid.ItemsSource = GetMockData();
    }
    
    private List<ZoneMockEntry> GetMockData() => new List<ZoneMockEntry>
    {
        new ZoneMockEntry { Id = 0, Name = "Tiny Woods", LevelRange = "1-5", MapCount = 3, Music = "tiny_woods.ogg" },
        new ZoneMockEntry { Id = 1, Name = "Thunderwave Cave", LevelRange = "6-11", MapCount = 5, Music = "thunderwave.ogg" },
        new ZoneMockEntry { Id = 2, Name = "Mt. Steel", LevelRange = "11-19", MapCount = 7, Music = "mt_steel.ogg" },
        new ZoneMockEntry { Id = 3, Name = "Sinister Woods", LevelRange = "14-22", MapCount = 6, Music = "sinister_woods.ogg" },
        new ZoneMockEntry { Id = 4, Name = "Silent Chasm", LevelRange = "20-28", MapCount = 8, Music = "silent_chasm.ogg" },
    };


}