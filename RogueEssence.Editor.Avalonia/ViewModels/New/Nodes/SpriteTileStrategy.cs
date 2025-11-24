using System;
using System.Collections.Generic;
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

public class SpriteTileStrategy : ISpriteOperationStrategy
{
    private int _cachedSize;

    private readonly IDialogService _dialogService;
    private readonly NodeFactory _nodeFactory;
    private readonly SpriteRootNode _spriteRootNode;

    public SpriteTileStrategy(IDialogService dialogService, NodeFactory nodeFactory, SpriteRootNode spriteRootNode)
    {
        _dialogService = dialogService;
        _nodeFactory = nodeFactory;
        _spriteRootNode = spriteRootNode;
    }

    public async Task<NodeBase> AddAsync()
    {
        var vm = new RenameWindowViewModel();
        bool result = await _dialogService.ShowDialogAsync<RenameWindowViewModel, bool>(
            vm, "Add sprite ID");

        if (!result) return null;

        return _nodeFactory.CreateDataItemNode(
            vm.Name, "SpriteEditor", vm.Name + ":", "Icons.PaintBrushFill");
    }

    public async Task DeleteAsync(DataItemNode node)
    {
        var res = await MessageBoxWindowView.Show(_dialogService, $"Delete sprite '{node.Title}'?", "Deleting Sprite",
            MessageBoxWindowView.MessageBoxButtons.YesNo, true);

        if (res == MessageBoxWindowView.MessageBoxResult.Yes)
        {
            lock (GameBase.lockObj)
            {
                string anim = node.ItemKey;

                string animPath = PathMod.ModPath(String.Format(GraphicsManager.TILE_PATTERN, anim));
                if (File.Exists(animPath))
                    File.Delete(animPath);

                GraphicsManager.RebuildIndices(GraphicsManager.AssetType.Tile);
                GraphicsManager.ClearCaches(GraphicsManager.AssetType.Tile);

                DiagManager.Instance.LogInfo("Deleted frames for:" + anim);

                _spriteRootNode.SubNodes.Remove(node);
            }
        }
    }

    public async Task MassExportAsync()
    {
        //remember addresses in registry
        string folderName = DevForm.GetConfig("TilesetDir", Directory.GetCurrentDirectory());

   
        //open window to choose directory
        var options = new FolderPickerOpenOptions
        {
            Title = "Select folder to mass export to",
            AllowMultiple = false,
        };
        
        await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var folder = await _dialogService.ShowFolderPickerAsync(options, folderName);

            if (folder == null)
                return;
            
            DevForm.SetConfig("TilesetDir", folder);
            _spriteRootNode.CachedPath = folder + "/";

            bool success = _massExport(_spriteRootNode.CachedPath);
            if (!success)
                await MessageBoxWindowView.Show(_dialogService, "Errors found exporting to\n" + _spriteRootNode.CachedPath + "\n\nCheck logs for more info.", "Mass Export Failed", MessageBoxWindowView.MessageBoxButtons.Ok);
        });
    }

    public async Task MassImportAsync()
    {
        await MessageBoxWindowView.Show(_dialogService,
            "Note: Importing a tileset to a slot that is already filled will automatically overwrite the old one.",
            "Mass Import", MessageBoxWindowView.MessageBoxButtons.Ok);

        //remember addresses in registry
        string folderName = DevForm.GetConfig("TilesetDir", Directory.GetCurrentDirectory());

        //open window to choose directory
        var options = new FolderPickerOpenOptions
        {
            Title = "Select tileset folder to mass import",
            AllowMultiple = false,
        };

        // IStorageFolder directory = await parent.StorageProvider.TryGetFolderFromPathAsync(folderName);
        await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var folder = await _dialogService.ShowFolderPickerAsync(options, folderName);

            // string folder = results.First().Path.LocalPath;

            MapRetileViewModel viewModel =
                new MapRetileViewModel(GraphicsManager.TileSize, "Tile size must be divisible by 8.");
            bool sizeResult = await _dialogService.ShowDialogAsync<MapRetileViewModel, bool>(viewModel, "");

            int size = viewModel.TileSize;

            if (!sizeResult || size == 0)
                return;


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
                    "Error importing from\n" + _spriteRootNode.CachedPath + "\n\n" + ex.Message, "Import Failed",
                    MessageBoxWindowView.MessageBoxButtons.Ok);
                return;
            }
        });
    }

    public async Task ExportAsync(DataItemNode node)
    {
        //get current sprite
        string animData = node.ItemKey;

        //remember addresses in registry
        string folderName = DevForm.GetConfig("TilesetDir", Directory.GetCurrentDirectory());

        var options = new FilePickerSaveOptions
        {
            Title = "Export PNG",
            DefaultExtension = "png",
            FileTypeChoices =
            [
                new FilePickerFileType("PNG Files")
                {
                    Patterns = new[] { "*.png" }
                }
            ]
        };
        var folder = await _dialogService.TryGetSaveFileAsync(options, folderName);
        if (!String.IsNullOrEmpty(folder))
        {
            DevForm.SetConfig("TilesetDir", Path.GetDirectoryName(folder));

            try
            {
                DevForm.ExecuteOrPend(() => { _export(folder, animData); });
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex, false);
                await MessageBoxWindowView.Show(_dialogService,
                    "Error exporting to\n" + _spriteRootNode.CachedPath + "\n\n" + ex.Message, "Export Failed",
                    MessageBoxWindowView.MessageBoxButtons.Ok);
                return;
            }
        }
    }

    public async Task ImportAsync(DataItemNode node)
    {
        //remember addresses in registry
        string folderName = DevForm.GetConfig("TilesetDir", Directory.GetCurrentDirectory());

        var options = new FilePickerOpenOptions
        {
            Title = "Open .png File",
            AllowMultiple = false,
            FileTypeFilter =
            [
                new FilePickerFileType("PNG Files")
                {
                    Patterns = ["*.PNG"]
                }
            ]
        };

        var file = await _dialogService.ShowFilePickerAsync(options, folderName);

        if (file == null) return;

        string animName = Path.GetFileNameWithoutExtension(file);


        if (_spriteRootNode.SubNodes.Any(n => n.Title == animName))
        {
            MessageBoxWindowView.MessageBoxResult result = await MessageBoxWindowView.Show(_dialogService,
                "Are you sure you want to overwrite the existing sheet:\n" + animName,
                "Sprite Sheet already exists.",
                MessageBoxWindowView.MessageBoxButtons.YesNo);
            if (result == MessageBoxWindowView.MessageBoxResult.No)
                return;
        }

        MapRetileViewModel viewModel =
            new MapRetileViewModel(GraphicsManager.TileSize, "Tile size must be divisible by 8.");
        bool sizeResult = await _dialogService.ShowDialogAsync<MapRetileViewModel, bool>(viewModel, "");

        int size = viewModel.TileSize;

        if (!sizeResult || size == 0)
            return;

        DevForm.SetConfig("TilesetDir", Path.GetDirectoryName(file));
        _spriteRootNode.CachedPath = file;
        _cachedSize = size;

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
            return;
        }
    }

    public async Task ReImportAsync()
    {
        try
        {
            _import(_spriteRootNode.CachedPath, _cachedSize);
        }
        catch (Exception ex)
        {
            DiagManager.Instance.LogError(ex, false);
            await MessageBoxWindowView.Show(_dialogService,
                "Error importing from\n" + _spriteRootNode.CachedPath + "\n\n" + ex.Message, "Import Failed",
                MessageBoxWindowView.MessageBoxButtons.Ok);
            return;
        }
    }

    private void _reloadFullList()
    {
        lock (GameBase.lockObj)
        {
            _spriteRootNode.SubNodes.Clear();
            foreach (string name in GraphicsManager.TileIndex.Nodes.Keys)
            {
                _spriteRootNode.SubNodes.Add(
                    _nodeFactory.CreateDataItemNode(name, "", name + ":", _spriteRootNode.AssetType.GetIcon())
                );
            }
        }
    }


    private void _import(string currentPath, int tileSize)
    {
        DevForm.ExecuteOrPend(() => { _tryImport(currentPath, tileSize); });

        //recompute
        _reloadFullList();
    }

    private void _tryImport(string currentPath, int tileSize)
    {
        lock (GameBase.lockObj)
        {
            string sheetName = Path.GetFileNameWithoutExtension(currentPath);
            string outputFile = PathMod.HardMod(String.Format(GraphicsManager.TILE_PATTERN, sheetName));

            if (!Directory.Exists(Path.GetDirectoryName(outputFile)))
                Directory.CreateDirectory(Path.GetDirectoryName(outputFile));

            //load into tilesets
            using (BaseSheet tileset = BaseSheet.Import(currentPath))
            {
                List<BaseSheet[]> tileList = new List<BaseSheet[]>();
                tileList.Add(new BaseSheet[] { tileset });
                ImportHelper.SaveTileSheet(tileList, outputFile, tileSize);
            }

            GraphicsManager.RebuildIndices(GraphicsManager.AssetType.Tile);
            GraphicsManager.ClearCaches(GraphicsManager.AssetType.Tile);
            DevDataManager.ClearCaches();

            DiagManager.Instance.LogInfo("Tiles from:\n" +
                                         currentPath + "\nhave been imported.");
        }
    }

    private void _massImport(string currentPath, int tileSize)
    {
        DevForm.ExecuteOrPend(() => { _tryMassImport(currentPath, tileSize); });

        //recompute
        _reloadFullList();
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

    private void _export(string currentPath, string anim)
    {
        lock (GameBase.lockObj)
        {
            string animPath = PathMod.ModPath(String.Format(GraphicsManager.TILE_PATTERN, anim));
            ImportHelper.ExportTileSheet(animPath, currentPath);

            DiagManager.Instance.LogInfo("Frames from:\n" +
                                         anim + "\nhave been exported to:" + currentPath);
        }
    }
}