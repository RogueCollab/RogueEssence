using System.Threading.Tasks;

namespace RogueEssence.Dev.ViewModels;

public interface ISpriteRootOperationStrategy
{
    Task MassExportAsync();
    Task MassImportAsync();
    Task ImportAsync();
}