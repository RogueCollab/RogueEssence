using System.Threading.Tasks;

namespace RogueEssence.Dev.ViewModels;

public interface ISpriteOperationStrategy
{
    Task<NodeBase> AddAsync();
    Task DeleteAsync(DataItemNode node);
    Task MassExportAsync();
    Task MassImportAsync();
    Task ExportAsync(DataItemNode node);
    Task ImportAsync(DataItemNode node);
    Task ReImportAsync();
}