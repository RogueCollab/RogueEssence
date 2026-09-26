using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using RogueEssence;
using RogueEssence.Content;
using RogueEssence.Dev;
using RogueEssence.Dev.Services;
using RogueEssence.Dev.ViewModels;
using RogueEssence.Dev.Views;

public class SpriteRootBeamStrategy : ISpriteRootOperationStrategy
{
    private readonly IDialogService _dialogService;
    private readonly SpriteRootNode _spriteRootNode;

    public event Action OnReload;

    public SpriteRootBeamStrategy(IDialogService dialogService, SpriteRootNode spriteRootNode)
    {
        _dialogService = dialogService;
        _spriteRootNode = spriteRootNode;
    }

    private GraphicsManager.AssetType AssetType => _spriteRootNode.AssetType;
    private string Name => _spriteRootNode.AssetType.ToString();

    // ── Shared private helpers ──────────────────────────────────────────────

    private void _export(string currentPath, string anim)
    {
        lock (GameBase.lockObj)
        {
            if (!Directory.Exists(currentPath))
                Directory.CreateDirectory(currentPath);

            string animPath = PathMod.ModPath(String.Format(GraphicsManager.GetPattern(AssetType), anim));
            if (File.Exists(animPath))
            {
                using (FileStream fileStream = File.OpenRead(animPath))
                using (BinaryReader reader = new BinaryReader(fileStream))
                {
                    BeamSheet sheet = BeamSheet.Load(reader);
                    BeamSheet.Export(sheet, currentPath + "/");
                }
            }

            DiagManager.Instance.LogInfo("Frames from:\n" + anim + "\nhave been exported to:" + currentPath);
        }
    }

    private void _import(string currentPath)
    {
        DevForm.ExecuteOrPend(() => { _tryImport(currentPath); });
        OnReload?.Invoke();
    }

    private void _tryImport(string currentPath)
    {
        lock (GameBase.lockObj)
        {
            string animName = Path.GetFileNameWithoutExtension(currentPath);
            string destFile = PathMod.HardMod(String.Format(GraphicsManager.GetPattern(AssetType), animName));

            if (!Directory.Exists(Path.GetDirectoryName(destFile)))
                Directory.CreateDirectory(Path.GetDirectoryName(destFile));

            using (BeamSheet sheet = BeamSheet.Import(currentPath + "/"))
            using (FileStream stream = File.OpenWrite(destFile))
            using (BinaryWriter writer = new BinaryWriter(stream))
                sheet.Save(writer);

            GraphicsManager.RebuildIndices(AssetType);
            GraphicsManager.ClearCaches(AssetType);

            DiagManager.Instance.LogInfo("Frames from:\n" + currentPath + "\nhave been imported.");
        }
    }

    private bool _massExport(string currentPath)
    {
        bool success = true;
        string assetPattern = GraphicsManager.GetPattern(AssetType);
        string[] dirs = PathMod.GetModFiles(Path.GetDirectoryName(assetPattern), String.Format(Path.GetFileName(assetPattern), "*"));
        for (int ii = 0; ii < dirs.Length; ii++)
        {
            try
            {
                string filename = Path.GetFileNameWithoutExtension(dirs[ii]);
                DevForm.ExecuteOrPend(() => { _export(currentPath + filename, filename); });
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex, false);
                success = false;
            }
        }
        return success;
    }

    private void _massImport(string currentPath)
    {
        lock (GameBase.lockObj)
        {
            string assetPattern = GraphicsManager.GetPattern(AssetType);
            if (!Directory.Exists(Path.GetDirectoryName(PathMod.HardMod(assetPattern))))
                Directory.CreateDirectory(Path.GetDirectoryName(PathMod.HardMod(assetPattern)));

            ImportHelper.ImportAllBeams(currentPath, PathMod.HardMod(assetPattern));

            GraphicsManager.RebuildIndices(AssetType);
            GraphicsManager.ClearCaches(AssetType);

            DiagManager.Instance.LogInfo("Mass import complete.");
        }
    }

    // ── ISpriteRootOperationStrategy ───────────────────────────────────────

    public async Task MassExportAsync()
    {
        string folderName = DevForm.GetConfig(Name + "Dir", Directory.GetCurrentDirectory());

        var options = new FolderPickerOpenOptions
        {
            Title = "Select folder to mass export to",
            AllowMultiple = false,
        };

        await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            string folder = await _dialogService.ShowFolderPickerAsync(options, folderName);
            if (string.IsNullOrEmpty(folder)) return;

            DevForm.SetConfig(Name + "Dir", folder);
            _spriteRootNode.CachedPath = folder + "/";

            bool success = _massExport(_spriteRootNode.CachedPath);
            if (!success)
                await MessageBoxWindowView.Show(_dialogService,
                    "Errors found exporting to\n" + _spriteRootNode.CachedPath + "\n\nCheck logs for more info.",
                    "Mass Export Failed", MessageBoxWindowView.MessageBoxButtons.Ok);
        });
    }

    public async Task MassImportAsync()
    {
        await MessageBoxWindowView.Show(_dialogService,
            "Note: Importing a beam to a slot that is already filled will automatically overwrite the old one.",
            "Mass Import", MessageBoxWindowView.MessageBoxButtons.Ok);

        string folderName = DevForm.GetConfig(Name + "Dir", Directory.GetCurrentDirectory());

        var options = new FolderPickerOpenOptions
        {
            Title = "Select folder to mass import",
            AllowMultiple = false,
        };

        await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            string folder = await _dialogService.ShowFolderPickerAsync(options, folderName);
            if (string.IsNullOrEmpty(folder)) return;

            DevForm.SetConfig(Name + "Dir", folder);
            _spriteRootNode.CachedPath = folder + "/";

            try
            {
                _massImport(_spriteRootNode.CachedPath);
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex, false);
                await MessageBoxWindowView.Show(_dialogService,
                    "Error importing from\n" + _spriteRootNode.CachedPath + "\n\n" + ex.Message,
                    "Import Failed", MessageBoxWindowView.MessageBoxButtons.Ok);
            }
        });
    }
    

    public async Task ImportAsync(ObservableCollection<string> items)
    {
        string folderName = DevForm.GetConfig(Name + "Dir", Directory.GetCurrentDirectory());

        var options = new FolderPickerOpenOptions
        {
            Title = "Select folder to import",
            AllowMultiple = false,
        };

        string? result = null;
        await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            string folder = await _dialogService.ShowFolderPickerAsync(options, folderName);
            if (string.IsNullOrEmpty(folder)) return;

            string animName = Path.GetFileNameWithoutExtension(folder);

            if (items.Any(item => item == animName))
            {
                var res = await MessageBoxWindowView.Show(_dialogService,
                    "Are you sure you want to overwrite the existing sheet:\n" + animName,
                    "Sprite Sheet already exists.", MessageBoxWindowView.MessageBoxButtons.YesNo);
                if (res == MessageBoxWindowView.MessageBoxResult.No) return;
            }
            else
            {
                items.Add(animName);
            }

            DevForm.SetConfig(Name + "Dir", Path.GetDirectoryName(folder));
            result = folder;

            try
            {
                _import(result);
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex, false);
                await MessageBoxWindowView.Show(_dialogService,
                    "Error importing from\n" + result + "\n\n" + ex.Message,
                    "Import Failed", MessageBoxWindowView.MessageBoxButtons.Ok);
                result = null;
            }
        });

        _spriteRootNode.CachedPath = result;
    }

    public async Task DeleteAsync(string key)
    {
        var res = await MessageBoxWindowView.Show(_dialogService,
            $"Delete sprite '{key}'?", "Deleting Sprite",
            MessageBoxWindowView.MessageBoxButtons.YesNo, true);

        if (res == MessageBoxWindowView.MessageBoxResult.Yes)
        {
            lock (GameBase.lockObj)
            {
                string animPath = PathMod.ModPath(String.Format(GraphicsManager.GetPattern(AssetType), key));
                if (File.Exists(animPath))
                    File.Delete(animPath);

                GraphicsManager.RebuildIndices(AssetType);
                GraphicsManager.ClearCaches(AssetType);

                DiagManager.Instance.LogInfo("Deleted frames for:" + key);
                OnReload?.Invoke();
            }
        }
    }

    public async Task ExportAsync(string key)
    {
        string folderName = DevForm.GetConfig(Name + "Dir", Directory.GetCurrentDirectory());

        var options = new FolderPickerOpenOptions
        {
            Title = "Select folder to export to",
            AllowMultiple = false,
        };

        await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            string folder = await _dialogService.ShowFolderPickerAsync(options, folderName);
            if (string.IsNullOrEmpty(folder)) return;

            DevForm.SetConfig(Name + "Dir", Path.GetDirectoryName(folder));

            try
            {
                DevForm.ExecuteOrPend(() => { _export(folder, key); });
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex, false);
                await MessageBoxWindowView.Show(_dialogService,
                    $"Error exporting to\n{folder}\n\n{ex.Message}",
                    "Export Failed", MessageBoxWindowView.MessageBoxButtons.Ok);
            }
        });
    }

    public async Task ReImportAsync()
    {
        await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            try
            {
                _import(_spriteRootNode.CachedPath);
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex, false);
                await MessageBoxWindowView.Show(_dialogService,
                    "Error importing from\n" + _spriteRootNode.CachedPath + "\n\n" + ex.Message,
                    "Import Failed", MessageBoxWindowView.MessageBoxButtons.Ok);
            }
        });
    }
}