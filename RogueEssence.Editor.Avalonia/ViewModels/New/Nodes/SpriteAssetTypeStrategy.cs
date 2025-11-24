using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using RogueEssence.Content;
using RogueEssence.Dev.Services;
using RogueEssence.Dev.Views;

namespace RogueEssence.Dev.ViewModels;

public class SpriteAssetTypeStrategy : ISpriteOperationStrategy
{
    private readonly IDialogService _dialogService;
    private readonly NodeFactory _nodeFactory;
    private readonly SpriteRootNode _spriteRootNode;
    
    
    private void _export(string currentPath, string anim)
    {
        lock (GameBase.lockObj)
        {
            string animPath = PathMod.ModPath(String.Format(GraphicsManager.GetPattern(_spriteRootNode.AssetType), anim));
            if (File.Exists(animPath))
            {
                //read file and read binary data
                using (FileStream fileStream = File.OpenRead(animPath))
                {
                    using (BinaryReader reader = new BinaryReader(fileStream))
                    {
                        DirSheet sheet = DirSheet.Load(reader);

                        string filename = DirSheet.GetExportString(sheet, Path.GetFileNameWithoutExtension(currentPath));
                        string dirname = Path.GetDirectoryName(currentPath);
                        DirSheet.Export(sheet, Path.Combine(dirname, filename + ".png"));
                    }
                }
            }

            DiagManager.Instance.LogInfo("Frames from:\n" +
                                         anim +
                                         "\nhave been exported to:" + currentPath);
        }
    }
    
    private bool _massExport(string currentPath, GraphicsManager.AssetType assetType)
    {
        bool success = true;
        string assetPattern = GraphicsManager.GetPattern(assetType);
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

    public SpriteAssetTypeStrategy(IDialogService dialogService, NodeFactory nodeFactory, SpriteRootNode spriteRootNode)
    {
        _dialogService = dialogService;
        _nodeFactory = nodeFactory;
        _spriteRootNode = spriteRootNode;
    }

    public async Task<NodeBase> AddAsync()
    {
        
        Console.WriteLine("TODO: Resolve this later...");
        var vm = new RenameWindowViewModel();
        bool result = await _dialogService.ShowDialogAsync<RenameWindowViewModel, bool>(
            vm, "Add sprite ID");

        if (!result) return null;

        return _nodeFactory.CreateDataItemNode(
            vm.Name, "SpriteEditor", vm.Name + ":", "Icons.PaintBrushFill");
    }

    public async Task DeleteAsync(DataItemNode node)
    {
        var res = await MessageBoxWindowView.Show(_dialogService, $"Delete sprite '{node.Title}'?", "Deleting Sprite", MessageBoxWindowView.MessageBoxButtons.YesNo, true);

        var assetType = _spriteRootNode.AssetType;
        if (res == MessageBoxWindowView.MessageBoxResult.Yes)
        {
            lock (GameBase.lockObj)
            {
                string anim = node.ItemKey;
                string animPath = PathMod.ModPath(String.Format(GraphicsManager.GetPattern(assetType), anim));
                if (File.Exists(animPath))
                    File.Delete(animPath);

                GraphicsManager.RebuildIndices(assetType);
                GraphicsManager.ClearCaches(assetType);

                DiagManager.Instance.LogInfo("Deleted frames for:" + anim);
                _spriteRootNode.SubNodes.Remove(node);
            }
            
        }
    }


    public async Task MassExportAsync()
    {
        string name = _spriteRootNode.AssetType.ToString();
        
        //remember addresses in registry
        string folderName = DevForm.GetConfig(name+ "Dir", Directory.GetCurrentDirectory());

        
        //open window to choose directory
        await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var options = new FolderPickerOpenOptions
            {
                Title = "Select folder to mass export to",
                AllowMultiple = false,
            };
            var folder = await _dialogService.ShowFolderPickerAsync(options, folderName);
            if (folder == null) return;

            DevForm.SetConfig(name + "Dir", folder);
            _spriteRootNode.CachedPath = folder + "/";


            bool success = _massExport(_spriteRootNode.CachedPath, _spriteRootNode.AssetType);
            if (!success)
            {
                await MessageBoxWindowView.Show(_dialogService, "Errors found exporting to\n" + _spriteRootNode.CachedPath + "\n\nCheck logs for more info.", "Mass Export Failed", MessageBoxWindowView.MessageBoxButtons.Ok);
            }
        });
    }

    public async Task MassImportAsync()
    {
        await MessageBoxWindowView.Show(_dialogService, "Note: Importing a sprite to a slot that is already filled will automatically overwrite the old one.", "Mass Import", MessageBoxWindowView.MessageBoxButtons.Ok);
        string name = _spriteRootNode.AssetType.ToString();
        
        //remember addresses in registry
        string folderName = DevForm.GetConfig(name + "Dir", Directory.GetCurrentDirectory());

        var options = new FolderPickerOpenOptions
        {
            Title = "Select folder to mass import",
            AllowMultiple = false,
        };
        var result = await _dialogService.ShowFolderPickerAsync(options, folderName);

        if (result == null) return;
        
        DevForm.SetConfig(name + "Dir", result);
        _spriteRootNode.CachedPath = result + "/";
        
        try
        {
            MassImport(_spriteRootNode.CachedPath);
        }
        catch (Exception ex)
        {
            DiagManager.Instance.LogError(ex, false);
            
            await MessageBoxWindowView.Show(_dialogService, "Error importing from\n" + _spriteRootNode.CachedPath + "\n\n" + ex.Message, "Import Failed", MessageBoxWindowView.MessageBoxButtons.Ok);
        }
        
    }
    
    private void _recomputeAnimList()
    {
        lock (GameBase.lockObj)
        {
            _spriteRootNode.SubNodes.Clear();
            GraphicsManager.AssetType assetType = _spriteRootNode.AssetType;
            string assetPattern = GraphicsManager.GetPattern(assetType);
            string[] dirs = PathMod.GetModFiles(Path.GetDirectoryName(assetPattern), String.Format(Path.GetFileName(assetPattern), "*"));
            for (int ii = 0; ii < dirs.Length; ii++)
            {
                string filename = Path.GetFileNameWithoutExtension(dirs[ii]);
                _spriteRootNode.SubNodes.Add(_nodeFactory.CreateDataItemNode(filename, "", filename, "Icons.PaintBrushFill"));
            }
        }
    }
    
    private void _import(string currentPath)
    {
        DevForm.ExecuteOrPend(() => { _tryImport(currentPath); });

        //recompute
        _recomputeAnimList();
    }

    private void _tryImport(string currentPath)
    {
        lock (GameBase.lockObj)
        {
            GraphicsManager.AssetType assetType = _spriteRootNode.AssetType;
            string assetPattern = GraphicsManager.GetPattern(assetType);
            string destFile;
            string animName = Path.GetFileNameWithoutExtension(currentPath);
            if (Directory.Exists(currentPath))
                destFile = PathMod.HardMod(String.Format(assetPattern, animName));
            else
            {
                string[] components = animName.Split('.');
                if (components.Length != 2)
                    throw new ArgumentException(
                        "The input filename does not fit the convention of \"<Anim Name>.<Anim Type>.png\"!");
                destFile = PathMod.HardMod(String.Format(assetPattern, components[0]));
            }

            if (!Directory.Exists(Path.GetDirectoryName(destFile)))
                Directory.CreateDirectory(Path.GetDirectoryName(destFile));

            //write sprite data
            using (DirSheet sheet = DirSheet.Import(currentPath))
            {
                using (FileStream stream = File.OpenWrite(destFile))
                {
                    //save data
                    using (BinaryWriter writer = new BinaryWriter(stream))
                        sheet.Save(writer);
                }
            }

            GraphicsManager.RebuildIndices(assetType);
            GraphicsManager.ClearCaches(assetType);

            DiagManager.Instance.LogInfo("Frames from:\n" +
                                         currentPath + "\nhave been imported.");
        }
    }


    private void MassImport(string currentPath)
    {
        DevForm.ExecuteOrPend(() => { _tryMassImport(currentPath); });
        //recompute
        _recomputeAnimList();
    }
    private void _tryMassImport(string currentPath)
    {
        lock (GameBase.lockObj)
        {
            GraphicsManager.AssetType assetType = _spriteRootNode.AssetType;
            string assetPattern = GraphicsManager.GetPattern(assetType);
            if (!Directory.Exists(Path.GetDirectoryName(PathMod.HardMod(assetPattern))))
                Directory.CreateDirectory(Path.GetDirectoryName(PathMod.HardMod(assetPattern)));
            ImportHelper.ImportAllNameDirs(currentPath, PathMod.HardMod(assetPattern));

            GraphicsManager.RebuildIndices(assetType);
            GraphicsManager.ClearCaches(assetType);

            DiagManager.Instance.LogInfo("Mass import complete.");
        }
    }

    public async Task ExportAsync(DataItemNode node)
    {
        string name = _spriteRootNode.AssetType.ToString();
        string folderName = DevForm.GetConfig(name + "Dir", Directory.GetCurrentDirectory());
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

        await Dispatcher.UIThread.InvokeAsync(async () =>
        {

            var filePath = await _dialogService.TryGetSaveFileAsync(options, folderName);
            if (!String.IsNullOrEmpty(filePath))
            {
                DevForm.SetConfig(name + "Dir", Path.GetDirectoryName(filePath));

                try
                {
                    DevForm.ExecuteOrPend(() => { _export(filePath, node.ItemKey); });
                }
                catch (Exception ex)
                {
                    DiagManager.Instance.LogError(ex, false);


                    await MessageBoxWindowView.Show(_dialogService, $"Error exporting to\n{filePath}\n\n{ex.Message}", "Export Failed", MessageBoxWindowView.MessageBoxButtons.Ok);
                }
            }
        });

    }

    public async Task ImportAsync()
    {
        string name = _spriteRootNode.AssetType.ToString();
        string folderName = DevForm.GetConfig(name + "Dir", Directory.GetCurrentDirectory());
        var options = new FilePickerOpenOptions
        {
            Title = "Open .png or .xml File",
            AllowMultiple = false,
            FileTypeFilter =
            [
                new FilePickerFileType("PNG Files")
                {
                    Patterns = ["*.png"]
                },
                new FilePickerFileType("DirData XML")
                {
                    Patterns = ["*.xml"]
                }
            ]
        };
        await Dispatcher.UIThread.InvokeAsync(async () =>
        {

            var filePath = await _dialogService.ShowFilePickerAsync(options, folderName);

            if (filePath != null)
            {
                string animName = Path.GetFileNameWithoutExtension(filePath);
                if (_spriteRootNode.SubNodes.Any(n => n.Title == animName))
                {

                    MessageBoxWindowView.MessageBoxResult result = await MessageBoxWindowView.Show(_dialogService, "Are you sure you want to overwrite the existing sheet:\n" + animName, "Sprite Sheet already exists.", MessageBoxWindowView.MessageBoxButtons.YesNo);
                    if (result == MessageBoxWindowView.MessageBoxResult.No)
                        return;
                }

                DevForm.SetConfig(name + "Dir", Path.GetDirectoryName(filePath));
                if (Path.GetExtension(filePath) == ".xml")
                    _spriteRootNode.CachedPath = Path.GetDirectoryName(filePath);
                else
                    _spriteRootNode.CachedPath = filePath;

                try
                {
                    _import(_spriteRootNode.CachedPath);
                }
                catch (Exception ex)
                {
                    DiagManager.Instance.LogError(ex, false);
                    await MessageBoxWindowView.Show(_dialogService, "Error importing from\n" + _spriteRootNode.CachedPath + "\n\n" + ex.Message, "Import Failed", MessageBoxWindowView.MessageBoxButtons.Ok);
                    return;
                }

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
                await MessageBoxWindowView.Show(_dialogService, "Error importing from\n" + _spriteRootNode.CachedPath + "\n\n" + ex.Message, "Import Failed", MessageBoxWindowView.MessageBoxButtons.Ok);
                return;
            }

        });
    }
}