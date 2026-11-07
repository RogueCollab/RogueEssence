using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace RogueEssence.Dev.ViewModels;

public interface ISpriteRootOperationStrategy
{
    
    public event Action OnReload;
    Task MassExportAsync();
    Task MassImportAsync();
    Task ReImportAsync();
    Task DeleteAsync(string path);
    Task ExportAsync(string path);
    Task ImportAsync(ObservableCollection<string> items);
}