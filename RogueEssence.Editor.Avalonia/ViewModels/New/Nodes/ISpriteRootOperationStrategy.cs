using System.Threading.Tasks;

namespace RogueEssence.Dev.ViewModels;

public interface ISpriteRootOperationStrategy
{
    Task MassExportAsync();
    Task MassImportAsync();
    Task ExportAsync(DataItemNode node);
    Task ImportAsync();
    Task ReImportAsync();
}