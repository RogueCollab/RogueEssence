using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using ReactiveUI;
using RogueElements;
using RogueEssence.Content;
using RogueEssence.Data;
using RogueEssence.Dev.Services;
using RogueEssence.Dev.Views;
using RogueEssence.Dungeon;
using RogueEssence.Ground;
using RogueEssence.Menu;
using RogueEssence.Script;

namespace RogueEssence.Dev.ViewModels;

public class GroundEditorPageViewModel : EditorPageViewModel, IGroundEditor, IPreCreatePage, IPageReloadable
{
    // Keeps track of whether the map editor while testing in-game
    private bool _allowEdit;
    public bool AllowEdit
    {
        get { return _allowEdit; }
        set
        {
            this.RaiseAndSetIfChanged(ref _allowEdit, value);
        }
    }
    

    // This is probably not the ideal way of doing this...
    public static void OnPreCreate()
    {
        lock (GameBase.lockObj)
        {
            Views.DevForm form = (Views.DevForm)DiagManager.Instance.DevEditor;
            if (form.GroundEditorPage == null)
            {
                LuaEngine.Instance.BreakScripts();
                MenuManager.Instance.ClearMenus();
                if (ZoneManager.Instance.CurrentGround != null)
                    GameManager.Instance.SceneOutcome = GameManager.Instance.MoveToEditor(true, ZoneManager.Instance.CurrentGround.AssetName);
                else
                    GameManager.Instance.SceneOutcome = GameManager.Instance.MoveToEditor(true, "");
            }
        }
    }
    public bool Active { get; private set; }

    public UndoStack Edits { get; }
    
    public GroundEditorPageViewModel(EditorContext context, NodeBase node,
        Action<EditorPageViewModel> onPageOpen = null) : base(context, node, onPageOpen)
    {
        Edits = new UndoStack();
    }

    public GroundTabTexturesViewModel Textures { get; set; }
    public GroundTabDecorationsViewModel Decorations { get; set; }
    public GroundTabWallsViewModel Walls { get; set; }
    public GroundTabEntitiesViewModel Entities { get; set; }
    public GroundTabPropertiesViewModel Properties { get; set; }
    public GroundTabStringsViewModel Strings { get; set; }
    public GroundTabScriptViewModel Script { get; set; }

    public void Reload()
    {
        _reload();
    }

    public override void OnPageLoad()
    {
        _reload();
        this.WhenAnyValue(x => x.CurrentFile)
            .Select(file => string.IsNullOrEmpty(file) ? "New File" : Path.GetFileNameWithoutExtension(file))
            .Subscribe(SetTitle);
    }
    
    public void Close()
    {
        DiagManager.Instance.DevEditor.GroundEditor = null;
        _context.TabEvents.RemoveTab(this);
    }
    
    private void _reload()
    {
        AllowEdit = true;
        DiagManager.Instance.DevEditor.GroundEditor = this;

        DevForm.ExecuteOrInvoke(() =>
        {
            Textures = new GroundTabTexturesViewModel(_context);
            Decorations = new GroundTabDecorationsViewModel();
            Walls = new GroundTabWallsViewModel();
            Entities = new GroundTabEntitiesViewModel();
            Properties = new GroundTabPropertiesViewModel(_context, this);
            Strings = new GroundTabStringsViewModel();
            Script = new GroundTabScriptViewModel(_context);
            CurrentFile = "";
            LoadFromCurrentGround();
            Active = true;
        });
        
        
       
        base.OnPageLoad();
        
    }
    
    private string currentFile;

    public string CurrentFile
    {
        get => currentFile;
        set => this.RaiseAndSetIfChanged(ref currentFile, value);
    }

    private int selectedTabIndex;

    public int SelectedTabIndex
    {
        get => selectedTabIndex;
        set
        {
            this.SetIfChanged(ref selectedTabIndex, value);
            TabChanged();
        }
    }


    public void mnuNew_Click()
    {
        CurrentFile = "";

        lock (GameBase.lockObj) //Schedule the map creation
            DoNew();
    }

    public async void mnuOpen_Click()
    {
        string mapDir = Path.GetFullPath(PathMod.ModPath(DataManager.GROUND_PATH));

        var options = new FilePickerOpenOptions
        {
            Title = "Open .rsground File",
            AllowMultiple = false,
            FileTypeFilter =
            [
                new FilePickerFileType("Ground Files")
                {
                    Patterns = ["*." + DataManager.GROUND_EXT.Substring(1)]
                }
            ]
        };

        await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            string result = await _context.DialogService.ShowFilePickerAsync(options, mapDir);

      
            if (!string.IsNullOrEmpty(result))
            {
                bool legalPath = false;
                foreach (string proposedPath in PathMod.FallbackPaths(DataManager.GROUND_PATH))
                {
                    if (comparePaths(proposedPath, Path.GetDirectoryName(result)))
                        legalPath = true;
                }

                if (!legalPath)
                    await MessageBoxWindowView.Show(_context.DialogService,
                        String.Format("Map can only be loaded from:\n{0}\nOr one of its parents.",
                            PathMod.ModPath(DataManager.GROUND_PATH)), "Error",
                        MessageBoxWindowView.MessageBoxButtons.Ok);
                else
                {
                    lock (GameBase.lockObj)
                        DoLoad(Path.GetFileNameWithoutExtension(result));
                }
            }
        });
    }

    public async Task<bool> mnuSave_Click()
    {
        if (CurrentFile == "")
            return
                await mnuSaveAs_Click(); //Since its the same thing, might as well re-use the function! It makes everyone's lives easier!
        else
        {
            string reqDir = PathMod.HardMod(DataManager.GROUND_PATH);
            string result = Path.Join(reqDir, Path.GetFileName(CurrentFile));
            lock (GameBase.lockObj)
            {
                string oldFilename = CurrentFile;
                DoSave(ZoneManager.Instance.CurrentGround, result, oldFilename);
            }

            return true;
        }
    }

    public async Task<bool> mnuSaveAs_Click()
    {
        string mapDir = Path.GetFullPath(PathMod.ModPath(DataManager.GROUND_PATH));

        var options = new FilePickerSaveOptions
        {
            FileTypeChoices =
            [
                new FilePickerFileType("Ground Files")
                {
                    Patterns = [$"*{DataManager.GROUND_EXT}"]
                }
            ]
        };

        await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            string result = await _context.DialogService.TryGetSaveFileAsync(options, mapDir);

            if (!string.IsNullOrEmpty(result))
            {
                string reqDir = PathMod.HardMod(DataManager.GROUND_PATH);
                if (!comparePaths(reqDir, Path.GetDirectoryName(result)))
                    await MessageBoxWindowView.Show(_context.DialogService,
                        string.Format("Map can only be saved to:\n{0}", reqDir),
                        "Error", MessageBoxWindowView.MessageBoxButtons.Ok);
                else if (Path.GetFileName(result).Contains(" "))
                    await MessageBoxWindowView.Show(_context.DialogService,
                        string.Format("Save file should not contain white space:\n{0}", Path.GetFileName(result)),
                        "Error", MessageBoxWindowView.MessageBoxButtons.Ok);
                else
                {
                    lock (GameBase.lockObj)
                    {
                        string oldFilename = CurrentFile;
                        DoSave(ZoneManager.Instance.CurrentGround, result, oldFilename);
                    }

                    return true;
                }
            }
            return false;
        });
        return false;
    }
    
    private bool silentClose;
    public void SilentClose()
    {
        silentClose = true;
        // Close();
    }


    public async void mnuTest_Click()
    {
        bool saved = await mnuSave_Click();
        if (saved)
        {
            lock (GameBase.lockObj)
            {
                DevForm form = (DevForm)DiagManager.Instance.DevEditor;
                form.GroundEditorPage.SilentClose();
                form.GroundEditorPage = null;
                GameManager.Instance.SceneOutcome =
                    GameManager.Instance.TestWarp(ZoneManager.Instance.CurrentGround.AssetName, true,
                        MathUtils.Rand.NextUInt64());
                AllowEdit = false;
            }
        }
    }

    public async void mnuImportFromPng_Click()
    {
        string mapDir = Path.GetFullPath(PathMod.ModPath(DataManager.GROUND_PATH));

        var options = new FilePickerOpenOptions
        {
            Title = "Open .png File",
            AllowMultiple = false,
            FileTypeFilter =
            [
                new FilePickerFileType("PNG Files")
                {
                    Patterns = ["*.png"]
                }
            ]
        };

        await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            string result = await _context.DialogService.ShowFilePickerAsync(options, mapDir);

            if (!string.IsNullOrEmpty(result))
                DoImportPng(result);
        });
    }


    public void mnuClearLayer_Click()
    {
        lock (GameBase.lockObj)
            DoClearLayer();
    }


    public async void mnuImportFromTileset_Click()
    {
        if (Textures.TileBrowser.CurrentTileset == "")
            await MessageBoxWindowView.Show(_context.DialogService, String.Format("No tileset to import!"), "Error",
                MessageBoxWindowView.MessageBoxButtons.Ok);
        else
        {
            lock (GameBase.lockObj)
                DoImportTileset(Textures.TileBrowser.CurrentTileset);
        }
    }
    
    


    public async void mnuReSize_Click()
    {
        MapResizeWindowViewModel vm = new MapResizeWindowViewModel(ZoneManager.Instance.CurrentGround.Width, ZoneManager.Instance.CurrentGround.Height);
        bool result = await _context.DialogService.ShowDialogAsync<MapResizeWindowViewModel, bool>(vm, "Resize Map");
        lock (GameBase.lockObj)
        {
            if (result)
            {
                //TODO: support undo for this
                DiagManager.Instance.DevEditor.GroundEditor.Edits.Clear();
        
                DiagManager.Instance.LoadMsg = "Resizing Map...";
                DevForm.EnterLoadPhase(GameBase.LoadPhase.Content);
        
                ZoneManager.Instance.CurrentGround.ResizeJustified(vm.MapWidth, vm.MapHeight,
                    vm.ResizeDir);
        
                DevForm.EnterLoadPhase(GameBase.LoadPhase.Ready);
            }
        }
    }

    public async void mnuReTile_Click()
    {
        
        MapRetileWindowViewModel vm = new MapRetileWindowViewModel(ZoneManager.Instance.CurrentGround.TileSize, "Tile size must be divisible by 8. All textures will be erased from all layers upon completing this operation.");

        bool result = await _context.DialogService.ShowDialogAsync<MapRetileWindowViewModel, bool>(vm, "Tileset Size");

        if (result)
        {
            lock (GameBase.lockObj)
            {
                bool sizeChanged = vm.TileSize != ZoneManager.Instance.CurrentGround.TileSize;
                if (result && sizeChanged)
                {
                    //TODO: support undo for this
                    DiagManager.Instance.DevEditor.GroundEditor.Edits.Clear();

                    DiagManager.Instance.LoadMsg = "Retiling Map...";
                    DevForm.EnterLoadPhase(GameBase.LoadPhase.Content);

                    ZoneManager.Instance.CurrentGround.Retile(vm.TileSize / GraphicsManager.TEX_SIZE);

                    Textures.TileBrowser.TileSize = ZoneManager.Instance.CurrentGround.TileSize;
                    Textures.AutotileBrowser.TileSize = ZoneManager.Instance.CurrentGround.TileSize;
                    ZoneManager.Instance.CurrentGround.BlankBG = new AutoTile();
                    Properties.BlankBG.LoadFromSource(ZoneManager.Instance.CurrentGround.BlankBG);

                    DevForm.EnterLoadPhase(GameBase.LoadPhase.Ready);
                }
            }
        }
    }
    
    public override void OnPageRemoved()
    {
        base.OnPageRemoved();
        Active = false;
        if (!silentClose)
            GameManager.Instance.SceneOutcome = exitGroundEdit();
        Node.SubNodes.Clear();
    }

    private IEnumerator<YieldInstruction> exitGroundEdit()
    {
        DevForm form = (DevForm)DiagManager.Instance.DevEditor;
        form.GroundEditorPage = null;

        //move to the previous scene or the title, if there was none
        if (DataManager.Instance.Save != null && DataManager.Instance.Save.NextDest.IsValid())
            yield return CoroutineManager.Instance.StartCoroutine(GameManager.Instance.MoveToZone(DataManager.Instance.Save.NextDest, true, false));
        else
            yield return CoroutineManager.Instance.StartCoroutine(GameManager.Instance.RestartToTitle());
    }
    
    public void mnuUndo_Click()
    {
        if (DiagManager.Instance.DevEditor.GroundEditor.Edits.CanUndo)
        {
            DiagManager.Instance.DevEditor.GroundEditor.Edits.Undo();
            ProcessUndo();
        }
    }

    public void mnuRedo_Click()
    {
        if (DiagManager.Instance.DevEditor.GroundEditor.Edits.CanRedo)
            DiagManager.Instance.DevEditor.GroundEditor.Edits.Redo();
    }

    private void DoNew()
    {
        DiagManager.Instance.DevEditor.GroundEditor.Edits.Clear();
        //take all the necessary steps before and after moving to the map

        DiagManager.Instance.LoadMsg = "Loading Map...";
        DevForm.EnterLoadPhase(GameBase.LoadPhase.Content);
        GameManager.Instance.ForceReady();

        ZoneManager.Instance.CurrentZone.DevNewGround();
        ZoneManager.Instance.CurrentGround.OnEditorInit();

        loadEditorSettings();
        DevForm.EnterLoadPhase(GameBase.LoadPhase.Ready);
    }

    private void DoLoad(string mapName)
    {
        DiagManager.Instance.DevEditor.GroundEditor.Edits.Clear();
        //take all the necessary steps before and after moving to the map

        DiagManager.Instance.LoadMsg = "Loading Map...";
        DevForm.EnterLoadPhase(GameBase.LoadPhase.Content);
        GameManager.Instance.ForceReady();

        ZoneManager.Instance.CurrentZone.DevLoadGround(mapName);
        ZoneManager.Instance.CurrentGround.OnEditorInit();

        CurrentFile = PathMod.ModPath(Path.Combine(DataManager.GROUND_PATH, mapName + DataManager.GROUND_EXT));
        loadEditorSettings();
        DevForm.EnterLoadPhase(GameBase.LoadPhase.Ready);
    }

    public void LoadFromCurrentGround()
    {
        if (ZoneManager.Instance.CurrentGround.AssetName != "")
            CurrentFile = PathMod.ModPath(Path.Combine(DataManager.GROUND_PATH,
                ZoneManager.Instance.CurrentGround.AssetName + DataManager.GROUND_EXT));
        else
            CurrentFile = "";

        loadEditorSettings();
    }

    private void loadEditorSettings()
    {
        Textures.Layers.LoadLayers();
        Textures.TileBrowser.TileSize = ZoneManager.Instance.CurrentGround.TileSize;
        Textures.AutotileBrowser.TileSize = ZoneManager.Instance.CurrentGround.TileSize;

        Decorations.SelectEntity(null);
        Decorations.Layers.LoadLayers();

        Entities.SelectEntity(null);
        Entities.Layers.LoadLayers();

        Walls.SetupLayerVisibility();
        Properties.LoadMapProperties();
        Script.LoadScripts();
        Strings.LoadStrings();
    }

    private void DoImportPng(string filePath)
    {
        DevForm.ExecuteOrPend(() => { tryImportPng(filePath); });

        string sheetName = Path.GetFileNameWithoutExtension(filePath);
        lock (GameBase.lockObj)
        {
            Textures.TileBrowser.UpdateTilesList();
        }

        Textures.TileBrowser.SelectTileset(sheetName);
    }

    private void tryImportPng(string filePath)
    {
        lock (GameBase.lockObj)
        {
            string sheetName = Path.GetFileNameWithoutExtension(filePath);
            string outputFile = PathMod.HardMod(String.Format(GraphicsManager.TILE_PATTERN, sheetName));

            //load into tilesets
            using (BaseSheet tileset = BaseSheet.Import(filePath))
            {
                List<BaseSheet[]> tileList = new List<BaseSheet[]>();
                tileList.Add(new BaseSheet[] { tileset });
                ImportHelper.SaveTileSheet(tileList, outputFile, ZoneManager.Instance.CurrentGround.TileSize);
            }

            GraphicsManager.RebuildIndices(GraphicsManager.AssetType.Tile);
            GraphicsManager.ClearCaches(GraphicsManager.AssetType.Tile);
            DevDataManager.ClearCaches();
        }
    }

    private void DoClearLayer()
    {
        //TODO: support undo for this
        DiagManager.Instance.DevEditor.GroundEditor.Edits.Clear();

        DiagManager.Instance.LoadMsg = "Loading Map...";
        DevForm.EnterLoadPhase(GameBase.LoadPhase.Content);

        //set tilesets
        for (int yy = 0; yy < ZoneManager.Instance.CurrentGround.Height; yy++)
        {
            for (int xx = 0; xx < ZoneManager.Instance.CurrentGround.Width; xx++)
                ZoneManager.Instance.CurrentGround.Layers[Textures.Layers.ChosenLayer].Tiles[xx][yy] = new AutoTile();
        }

        DevForm.EnterLoadPhase(GameBase.LoadPhase.Ready);
    }

    private void DoImportTileset(string sheetName)
    {
        //TODO: support undo for this
        DiagManager.Instance.DevEditor.GroundEditor.Edits.Clear();

        Loc newSize = GraphicsManager.TileIndex.GetTileDims(sheetName);

        DiagManager.Instance.LoadMsg = "Loading Map...";
        DevForm.EnterLoadPhase(GameBase.LoadPhase.Content);

        ZoneManager.Instance.CurrentGround.ResizeJustified(
            Math.Max(newSize.X, ZoneManager.Instance.CurrentGround.Width),
            Math.Max(newSize.Y, ZoneManager.Instance.CurrentGround.Height), Dir8.UpLeft);

        //set tilesets
        for (int yy = 0; yy < newSize.Y; yy++)
        {
            for (int xx = 0; xx < newSize.X; xx++)
            {
                AutoTile tile = new AutoTile();
                TileFrame newFrame = new TileFrame(new Loc(xx, yy), sheetName);
                //check for emptiness
                long tilePos = GraphicsManager.TileIndex.GetPosition(newFrame.Sheet, newFrame.TexLoc);
                if (tilePos > 0)
                    tile.Layers.Add(new TileLayer(newFrame));

                ZoneManager.Instance.CurrentGround.Layers[Textures.Layers.ChosenLayer].Tiles[xx][yy] = tile;
            }
        }

        DevForm.EnterLoadPhase(GameBase.LoadPhase.Ready);
    }

    //TODO: standardize adding of frames
    private void DoImportTilesetToFrames(string sheetName)
    {
        //TODO: support undo for this
        DiagManager.Instance.DevEditor.GroundEditor.Edits.Clear();

        Loc newSize = GraphicsManager.TileIndex.GetTileDims(sheetName);

        DiagManager.Instance.LoadMsg = "Loading Map...";
        DevForm.EnterLoadPhase(GameBase.LoadPhase.Content);

        ZoneManager.Instance.CurrentGround.ResizeJustified(
            Math.Max(newSize.X, ZoneManager.Instance.CurrentGround.Width),
            Math.Max(newSize.Y, ZoneManager.Instance.CurrentGround.Height), Dir8.UpLeft);

        //count highest frames
        int maxFrames = 0;
        for (int yy = 0; yy < newSize.Y; yy++)
        {
            for (int xx = 0; xx < newSize.X; xx++)
            {
                AutoTile tile = ZoneManager.Instance.CurrentGround.Layers[Textures.Layers.ChosenLayer].Tiles[xx][yy];
                if (tile.Layers.Count > 0)
                    maxFrames = Math.Max(tile.Layers[0].Frames.Count, maxFrames);
            }
        }
        

        //set tilesets
        for (int yy = 0; yy < newSize.Y; yy++)
        {
            for (int xx = 0; xx < newSize.X; xx++)
            {
                AutoTile tile = ZoneManager.Instance.CurrentGround.Layers[Textures.Layers.ChosenLayer].Tiles[xx][yy];
                TileFrame newFrame = new TileFrame(new Loc(xx, yy), sheetName);
                //check for emptiness
                long tilePos = GraphicsManager.TileIndex.GetPosition(newFrame.Sheet, newFrame.TexLoc);
                if (tilePos > 0)
                {
                    if (tile.Layers.Count == 0)
                        tile.Layers.Add(new TileLayer(60));
                    while (tile.Layers[0].Frames.Count < maxFrames)
                        tile.Layers[0].Frames.Add(TileFrame.Empty);
                    tile.Layers[0].Frames.Add(newFrame);
                }
            }
        }

        DevForm.EnterLoadPhase(GameBase.LoadPhase.Ready);
    }

    private void DoSave(GroundMap curgrnd, string filepath, string oldfname)
    {
        curgrnd.AssetName = Path.GetFileNameWithoutExtension(filepath); //Set the assetname to the file name!
        DataManager.SaveObject(curgrnd, filepath);

        //Actually create the script folder, and default script file.
        CreateOrCopyScriptData(oldfname, filepath);
        //create or update the strings
        Strings.SaveStrings();

        CurrentFile = filepath;
    }


    private static bool comparePaths(string path1, string path2)
    {
        return String.Compare(Path.GetFullPath(path1).TrimEnd('\\').TrimEnd('/'),
            Path.GetFullPath(path2).TrimEnd('\\').TrimEnd('/'),
            StringComparison.InvariantCultureIgnoreCase) == 0;
    }

    /// <summary>
    /// Call this when saving as, so that if the previous map name has a script folder with data in it,
    /// we can copy it. And if it doesn't, we create a "blank slate" one!
    /// </summary>
    /// <param name="oldfilepath"></param>
    /// <param name="newfilepath"></param>
    public static void CreateOrCopyScriptData(string oldfilepath, string newfilepath)
    {
        //If we are not resaving an old map, create a new script file
        if (String.IsNullOrEmpty(oldfilepath))
        {
            //We just create a new one straight away!
            LuaEngine.Instance.CreateGroundMapScriptDir(Path.GetFileNameWithoutExtension(newfilepath));
        }
        else
        {
            //just make the dir path
            string mappath = LuaEngine.MakeGroundMapScriptPath(Path.GetFileNameWithoutExtension(newfilepath), "");

            //Check if files exists already
            if (!Directory.Exists(mappath))
                Directory.CreateDirectory(mappath);
        }
    }

    public void TabChanged()
    {
        switch (selectedTabIndex)
        {
            case 0: //Textures
                GroundEditScene.Instance.EditMode = GroundEditScene.EditorMode.Texture;
                break;
            case 1: //Decorations
                GroundEditScene.Instance.EditMode = GroundEditScene.EditorMode.Decoration;
                Decorations.TabbedIn();
                break;
            case 2: //Walls
                GroundEditScene.Instance.EditMode = GroundEditScene.EditorMode.Wall;
                break;
            case 3: //Entities
                GroundEditScene.Instance.EditMode = GroundEditScene.EditorMode.Entity;
                Entities.TabbedIn();
                break;
            default:
                GroundEditScene.Instance.EditMode = GroundEditScene.EditorMode.Other;
                break;
        }

        if (selectedTabIndex != 1)
            Decorations.TabbedOut();
        if (selectedTabIndex != 3)
            Entities.TabbedOut();
    }

    public void ProcessInput(InputManager input)
    {
        lock (GameBase.lockObj)
        {
            if (input.BaseKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftControl))
            {
                if (input.BaseKeyPressed(Microsoft.Xna.Framework.Input.Keys.Z) &&
                    input.BaseKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftShift))
                {
                    mnuRedo_Click();
                    return;
                }
                else if (input.BaseKeyPressed(Microsoft.Xna.Framework.Input.Keys.Z))
                {
                    mnuUndo_Click();
                    return;
                }
            }

            switch (selectedTabIndex)
            {
                case 0: //Textures
                    Textures.ProcessInput(input);
                    break;
                case 1: //Decorations
                    Decorations.ProcessInput(input);
                    break;
                case 2: //Walls
                    Walls.ProcessInput(input);
                    break;
                case 3: //Entities
                    Entities.ProcessInput(input);
                    break;
            }
        }
    }

    public void ProcessUndo()
    {
        lock (GameBase.lockObj)
        {
            switch (selectedTabIndex)
            {
                case 3: //Entities
                    Entities.ProcessUndo();
                    break;
            }
        }
    }
}