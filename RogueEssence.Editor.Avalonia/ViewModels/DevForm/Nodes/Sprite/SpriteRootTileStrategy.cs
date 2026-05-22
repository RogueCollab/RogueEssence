using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using RogueEssence;
using RogueEssence.Content;
using RogueEssence.Dev;
using RogueEssence.Dev.Services;
using RogueEssence.Dev.ViewModels;
using RogueEssence.Dev.Views;

namespace RogueEssence.Dev.ViewModels;

public class SpriteRootTileStrategy : ISpriteRootOperationStrategy
{
    private int _cachedSize;
    private readonly IDialogService _dialogService;
    private readonly SpriteRootNode _spriteRootNode;

    public event Action OnReload;

    public SpriteRootTileStrategy(IDialogService dialogService, SpriteRootNode spriteRootNode)
    {
        _dialogService = dialogService;
        _spriteRootNode = spriteRootNode;
    }

    private void _export(string currentPath, string anim)
    {
        lock (GameBase.lockObj)
        {
            string animPath = PathMod.ModPath(String.Format(GraphicsManager.TILE_PATTERN, anim));
            ImportHelper.ExportTileSheet(animPath, currentPath);
            DiagManager.Instance.LogInfo("Frames from:\n" + anim + "\nhave been exported to:" + currentPath);
        }
    }

    private void _import(string currentPath, int tileSize)
    {
        DevForm.ExecuteOrPend(() => { _tryImport(currentPath, tileSize); });
        OnReload?.Invoke();
    }

    private void _tryImport(string currentPath, int tileSize)
    {
        lock (GameBase.lockObj)
        {
            string sheetName = Path.GetFileNameWithoutExtension(currentPath);
            string outputFile = PathMod.HardMod(String.Format(GraphicsManager.TILE_PATTERN, sheetName));

            if (!Directory.Exists(Path.GetDirectoryName(outputFile)))
                Directory.CreateDirectory(Path.GetDirectoryName(outputFile));

            using (BaseSheet tileset = BaseSheet.Import(currentPath))
            {
                List<BaseSheet[]> tileList = new List<BaseSheet[]>();
                tileList.Add(new BaseSheet[] { tileset });
                ImportHelper.SaveTileSheet(tileList, outputFile, tileSize);
            }

            GraphicsManager.RebuildIndices(GraphicsManager.AssetType.Tile);
            GraphicsManager.ClearCaches(GraphicsManager.AssetType.Tile);
            DevDataManager.ClearCaches();

            DiagManager.Instance.LogInfo("Tiles from:\n" + currentPath + "\nhave been imported.");
        }
    }

    private void _massImport(string currentPath, int tileSize)
    {
        DevForm.ExecuteOrPend(() => { _tryMassImport(currentPath, tileSize); });
    }

    private void _tryMassImport(string currentPath, int tileSize)
    {
        lock (GameBase.lockObj)
        {
            if (!Directory.Exists(Path.GetDirectoryName(PathMod.HardMod(GraphicsManager.TILE_PATTERN))))
                Directory.CreateDirectory(Path.GetDirectoryName(PathMod.HardMod(GraphicsManager.TILE_PATTERN)));

            ImportHelper.ImportAllTiles(currentPath, PathMod.HardMod(GraphicsManager.TILE_PATTERN), tileSize);

            GraphicsManager.RebuildIndices(GraphicsManager.AssetType.Tile);
            GraphicsManager.ClearCaches(GraphicsManager.AssetType.Tile);

            DiagManager.Instance.LogInfo("Mass import complete.");
        }
    }

    private bool _massExport(string currentPath)
    {
        bool success = true;
        string[] dirs = PathMod.GetModFiles(Path.GetDirectoryName(GraphicsManager.TILE_PATTERN),
            String.Format(Path.GetFileName(GraphicsManager.TILE_PATTERN), "*"));
        for (int ii = 0; ii < dirs.Length; ii++)
        {
            try
            {
                string filename = Path.GetFileNameWithoutExtension(dirs[ii]);
                DevForm.ExecuteOrPend(() => { _export(Path.Combine(currentPath, filename + ".png"), filename); });
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex, false);
                success = false;
            }
        }
        return success;
    }
    
    public async Task MassExportAsync()
    {
        string folderName = DevForm.GetConfig("TilesetDir", Directory.GetCurrentDirectory());

        var options = new FolderPickerOpenOptions
        {
            Title = "Select folder to mass export to",
            AllowMultiple = false,
        };

        await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            string folder = await _dialogService.ShowFolderPickerAsync(options, folderName);
            if (string.IsNullOrEmpty(folder)) return;

            DevForm.SetConfig("TilesetDir", folder);
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
            "Note: Importing a tileset to a slot that is already filled will automatically overwrite the old one.",
            "Mass Import", MessageBoxWindowView.MessageBoxButtons.Ok);

        string folderName = DevForm.GetConfig("TilesetDir", Directory.GetCurrentDirectory());

        var options = new FolderPickerOpenOptions
        {
            Title = "Select tileset folder to mass import",
            AllowMultiple = false,
        };

        await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            string folder = await _dialogService.ShowFolderPickerAsync(options, folderName);
            if (string.IsNullOrEmpty(folder)) return;

            MapRetileWindowViewModel viewModel = new MapRetileWindowViewModel(GraphicsManager.TileSize, "Tile size must be divisible by 8.");
            bool sizeResult = await _dialogService.ShowDialogAsync<MapRetileWindowViewModel, bool>(viewModel, "");

            int size = viewModel.TileSize;
            if (!sizeResult || size == 0) return;

            DevForm.SetConfig("TilesetDir", folder);
            _spriteRootNode.CachedPath = folder + "/";
            _cachedSize = size;

            try
            {
                _massImport(_spriteRootNode.CachedPath, _cachedSize);
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
        string folderName = DevForm.GetConfig("TilesetDir", Directory.GetCurrentDirectory());

        var options = new FilePickerOpenOptions
        {
            Title = "Open .png File",
            AllowMultiple = false,
            FileTypeFilter =
            [
                new FilePickerFileType("PNG Files") { Patterns = ["*.png"] }
            ]
        };

        string? result = null;
        await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            string filePath = await _dialogService.ShowFilePickerAsync(options, folderName);
            if (string.IsNullOrEmpty(filePath)) return;

            string animName = Path.GetFileNameWithoutExtension(filePath);

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

            MapRetileWindowViewModel viewModel = new MapRetileWindowViewModel(GraphicsManager.TileSize, "Tile size must be divisible by 8.");
            bool sizeResult = await _dialogService.ShowDialogAsync<MapRetileWindowViewModel, bool>(viewModel, "");

            int size = viewModel.TileSize;
            if (!sizeResult || size == 0) return;

            DevForm.SetConfig("TilesetDir", Path.GetDirectoryName(filePath));
            _cachedSize = size;

            try
            {
                _import(filePath, _cachedSize);
                result = filePath;
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex, false);
                await MessageBoxWindowView.Show(_dialogService,
                    "Error importing from\n" + filePath + "\n\n" + ex.Message,
                    "Import Failed", MessageBoxWindowView.MessageBoxButtons.Ok);
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
                string animPath = PathMod.ModPath(String.Format(GraphicsManager.TILE_PATTERN, key));
                if (File.Exists(animPath))
                    File.Delete(animPath);

                GraphicsManager.RebuildIndices(GraphicsManager.AssetType.Tile);
                GraphicsManager.ClearCaches(GraphicsManager.AssetType.Tile);

                DiagManager.Instance.LogInfo("Deleted frames for:" + key);
                OnReload?.Invoke();
            }
        }
    }

    public async Task ExportAsync(string key)
    {
        string folderName = DevForm.GetConfig("TilesetDir", Directory.GetCurrentDirectory());

        var options = new FilePickerSaveOptions
        {
            Title = "Export PNG",
            DefaultExtension = "png",
            FileTypeChoices =
            [
                new FilePickerFileType("PNG Files") { Patterns = ["*.png"] }
            ]
        };

        await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            string filePath = await _dialogService.TryGetSaveFileAsync(options, folderName);
            if (string.IsNullOrEmpty(filePath)) return;

            DevForm.SetConfig("TilesetDir", Path.GetDirectoryName(filePath));

            try
            {
                DevForm.ExecuteOrPend(() => { _export(filePath, key); });
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex, false);
                await MessageBoxWindowView.Show(_dialogService,
                    "Error exporting to\n" + filePath + "\n\n" + ex.Message,
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
                _import(_spriteRootNode.CachedPath, _cachedSize);
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