using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Avalonia.Platform;
using Microsoft.Xna.Framework;
using RogueEssence.Content;
using RogueEssence.Data;
using RogueEssence.Dev.ViewModels;
using RogueEssence.Dungeon;

namespace RogueEssence.Dev.Views;

public partial class DevForm : ChromelessWindow, IRootEditor
{
    public bool LoadComplete { get; private set; }

        public MapEditForm MapEditForm;
        public GroundEditForm GroundEditForm;

        private Action pendingEditorAction;
        private Exception pendingException;

        public IMapEditor MapEditor { get { return MapEditForm; } }
        public IGroundEditor GroundEditor { get { return GroundEditForm; } }
        public bool AteMouse { get { return false; } }
        public bool AteKeyboard { get { return false; } }

        private static Dictionary<string, string> devConfig;
        private static bool canSave;

    
        
        void IRootEditor.Load(GameBase game)
        {
            Console.WriteLine("Loading Dev Editor");
            ExecuteOrInvoke(load);
        }

        private void load()
        {
            lock (GameBase.lockObj)
            {
                DevDataManager.Init();

                loadDevConfig();

                reload(DataManager.DataType.All);

                LoadComplete = true;
            }
        }


        void IRootEditor.ReloadData(DataManager.DataType dataType)
        {
            ExecuteOrInvoke(() => { reload(dataType); });
        }

        private void reload(DataManager.DataType dataType)
        {
            lock (GameBase.lockObj)
            {
                if (dataType == DataManager.DataType.All)
                    DevDataManager.ClearCaches();

                ViewModels.DevFormViewModel devViewModel = (ViewModels.DevFormViewModel)this.DataContext;

                if (dataType == DataManager.DataType.All)
                {
                    devViewModel.Game.HideSprites = DataManager.Instance.HideChars;
                    devViewModel.Game.HideObjects = DataManager.Instance.HideObjects;
                    devViewModel.Travel.DebugGen = DiagManager.Instance.ListenGen;
                }

                if ((dataType & DataManager.DataType.Skill) != DataManager.DataType.None)
                    devViewModel.Game.ReloadSkills();

                if ((dataType & DataManager.DataType.Intrinsic) != DataManager.DataType.None)
                    devViewModel.Game.ReloadIntrinsics();

                if ((dataType & DataManager.DataType.Status) != DataManager.DataType.None)
                    devViewModel.Game.ReloadStatuses();

                if ((dataType & DataManager.DataType.Item) != DataManager.DataType.None)
                    devViewModel.Game.ReloadItems();


                if ((dataType & DataManager.DataType.Monster) != DataManager.DataType.None)
                {
                    devViewModel.Player.LoadMonstersNumeric();
                    devViewModel.Player.ReloadMonsters();
                }

                if (dataType == DataManager.DataType.All)
                {
                    int globalIdle = GraphicsManager.GlobalIdle;
                    devViewModel.Player.Anims.Clear();
                    for (int ii = 0; ii < GraphicsManager.Actions.Count; ii++)
                        devViewModel.Player.Anims.Add(GraphicsManager.Actions[ii].Name);
                    devViewModel.Player.ChosenAnim = -1;
                    devViewModel.Player.ChosenAnim = globalIdle;
                }

                if ((dataType & DataManager.DataType.Zone) != DataManager.DataType.None)
                    devViewModel.Travel.ReloadZones();

                if (dataType == DataManager.DataType.All)
                    devViewModel.UpdateMod();
                
                Console.WriteLine("Dev Editor Reloaded!");
                devViewModel.LoadDevTree();
                LoadComplete = true;
                
            }
        }


        public void Update(GameTime gameTime)
        {
            if (pendingEditorAction != null)
            {
                try
                {
                    pendingEditorAction();
                    pendingException = null;
                }
                catch (Exception ex)
                {
                    pendingException = ex;
                }
                pendingEditorAction = null;
            }

            ExecuteOrInvoke(update);
        }

        private void update()
        {
            lock (GameBase.lockObj)
            {
                ViewModels.DevFormViewModel devViewModel = (ViewModels.DevFormViewModel)this.DataContext;

                devViewModel.Player.UpdateLevel();
                if (GameManager.Instance.IsInGame())
                {
                    if (!devViewModel.Player.JustOnce)
                    {
                        if (devViewModel.Player.JustMe)
                        {
                            int currentIdle = Dungeon.DungeonScene.Instance.FocusedCharacter.IdleOverride;
                            if (currentIdle < 0)
                                currentIdle = GraphicsManager.GlobalIdle;
                            devViewModel.Player.ChosenAnim = currentIdle;
                        }
                        else
                            devViewModel.Player.ChosenAnim = GraphicsManager.GlobalIdle;
                    }
                    devViewModel.Player.UpdateSpecies(Dungeon.DungeonScene.Instance.FocusedCharacter.BaseForm);
                }
                if (GroundEditForm != null)
                {
                    ViewModels.GroundEditViewModel vm = (ViewModels.GroundEditViewModel)GroundEditForm.DataContext;
                    vm.Textures.TileBrowser.UpdateFrame();
                }
                if (MapEditForm != null)
                {
                    ViewModels.MapEditViewModel vm = (ViewModels.MapEditViewModel)MapEditForm.DataContext;
                    vm.Textures.TileBrowser.UpdateFrame();
                    vm.Terrain.TileBrowser.UpdateFrame();
                }

                if (canSave)
                    saveConfig();
            }
        }
        public void Draw() { }

        public void OpenGround()
        {
            ExecuteOrInvoke(openGround);
        }

        private void openGround()
        {
            GroundEditForm = new GroundEditForm();
            ViewModels.GroundEditViewModel vm = new ViewModels.GroundEditViewModel();
            GroundEditForm.DataContext = vm;
            vm.LoadFromCurrentGround();
            GroundEditForm.Show();
        }

        public void OpenMap()
        {
            ExecuteOrInvoke(openMap);
        }

        public void openMap()
        {
            MapEditForm = new MapEditForm();
            ViewModels.MapEditViewModel vm = new ViewModels.MapEditViewModel();
            MapEditForm.DataContext = vm;
            vm.LoadFromCurrentMap();
            MapEditForm.Show();
        }

        public void groundEditorClosed(object sender, EventArgs e)
        {
            GameManager.Instance.SceneOutcome = resetEditors();
        }

        public void mapEditorClosed(object sender, EventArgs e)
        {
            GameManager.Instance.SceneOutcome = resetEditors();
        }


        private IEnumerator<YieldInstruction> resetEditors()
        {
            GroundEditForm = null;
            MapEditForm = null;
            yield return CoroutineManager.Instance.StartCoroutine(GameManager.Instance.RestartToTitle());
        }


        public void CloseGround()
        {
            if (GroundEditForm != null)
                GroundEditForm.Close();
        }

        public void CloseMap()
        {
            if (MapEditForm != null)
                MapEditForm.Close();
        }


        void LoadGame()
        {
            // Windows - CAN run game in new thread, CAN run game in same thread via dispatch.
            // Mac - CANNOT run game in new thread, CAN run game in same thread via dispatch.
            // Linux - CAN run the game in new thread, CANNOT run game in same thread via dispatch.

            // When the game is started, it should run a continuous loop, blocking the UI
            // However, this is only happening on linux.  Why not windows and mac?
            // With Mac, cocoa can ONLY start the game window if it's on the main thread. Weird...

            if (!OperatingSystem.IsLinux())
                LoadGameDelegate();
            else
            {
                Thread thread = new Thread(LoadGameDelegate);
                thread.IsBackground = true;
                thread.Start();
            }
        }

        /// <summary>
        /// A method intended to be called from the editor thread, that sends a function pointer to the Game thread,
        /// waits for it to complete (blocking the thread), and then continues execution.
        /// This call cannot be performed within a lock!!
        /// </summary>
        /// <param name="action"></param>
        public static void ExecuteOrPend(Action action)
        {
            if (!OperatingSystem.IsLinux())
                action();
            else
            {
                DevForm editor = (DevForm)DiagManager.Instance.DevEditor;
                editor.pendingEditorAction = action;

                SpinWait.SpinUntil(() => editor.pendingEditorAction == null);

                if (editor.pendingException != null)
                {
                    Exception ex = editor.pendingException;
                    throw ex;
                }
            }
        }

        public static void ExecuteOrInvoke(Action action)
        {
            if (!OperatingSystem.IsLinux())
                action();
            else
                Dispatcher.UIThread.InvokeAsync(action, DispatcherPriority.Background);
        }

        void LoadGameDelegate()
        {
            try
            {
                DiagManager.Instance.DevEditor = this;
                using (GameBase game = new GameBase())
                    game.Run();
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex);
            }
            ExecuteOrInvoke(Close);
        }

        public void Window_Loaded(object sender, EventArgs e)
        {
            if (Design.IsDesignMode)
                return;
            //Thread thread = new Thread(LoadGame);
            //thread.IsBackground = true;
            //thread.Start();
            Dispatcher.UIThread.InvokeAsync(LoadGame, DispatcherPriority.Background);
            //LoadGame();
        }

        public void Window_Closed(object sender, EventArgs e)
        {
            DiagManager.Instance.LoadMsg = "Closing...";
            EnterLoadPhase(GameBase.LoadPhase.Unload);
        }

        public static void EnterLoadPhase(GameBase.LoadPhase loadState)
        {
            GameBase.CurrentPhase = loadState;
        }



        private static void loadDevConfig()
        {
            devConfig = new Dictionary<string, string>();

            try
            {
                string configPath = GetConfigPath();
                string folderPath = Path.GetDirectoryName(configPath);
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                if (File.Exists(configPath))
                {
                    using (FileStream stream = File.OpenRead(configPath))
                    {
                        using (BinaryReader reader = new BinaryReader(stream))
                        {
                            while (reader.BaseStream.Position < reader.BaseStream.Length)
                            {
                                string key = reader.ReadString();
                                string val = reader.ReadString();
                                devConfig[key] = val;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex);
            }
        }


        private static void saveConfig()
        {
            //save whole file
            try
            {
                using (var writer = new BinaryWriter(new FileStream(GetConfigPath(), FileMode.Create, FileAccess.Write, FileShare.None)))
                {
                    foreach (string curKey in devConfig.Keys)
                    {
                        writer.Write(curKey);
                        writer.Write(devConfig[curKey]);
                    }
                }
            }
            catch (IOException ioEx)
            {
                DiagManager.Instance.LogError(ioEx, false);
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex);
            }
        }

        public static string GetConfig(string key, string def)
        {
            string val;
            if (devConfig.TryGetValue(key, out val))
                return val;
            return def;
        }

        public static int GetConfig(string key, int def)
        {
            string val;
            if (devConfig.TryGetValue(key, out val))
            {
                int result;
                if (Int32.TryParse(val, out result))
                    return result;
            }
            return def;
        }

        public static void SetConfig(string key, int val)
        {
            SetConfig(key, val.ToString());
        }

        public static void SetConfig(string key, string val)
        {
            if (val == null && devConfig.ContainsKey(key))
                devConfig.Remove(key);
            else
                devConfig[key] = val;

            canSave = true;
        }

        public static string GetConfigPath()
        {
            //https://jimrich.sk/environment-specialfolder-on-windows-linux-and-os-x/
            //MacOS actually uses a different folder for config data, traditionally
            //I guess it's the odd one out...
            if (OperatingSystem.IsMacOS())
                return PathMod.FromApp("./devConfig");//Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "/Library/Application Support/RogueEssence/config");
            else
                return PathMod.FromApp("./devConfig");//Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RogueEssence /devConfig");
        }
        
        
    public static readonly StyledProperty<GridLength> CaptionHeightProperty =
        AvaloniaProperty.Register<DevForm, GridLength>(nameof(CaptionHeight));

    public GridLength CaptionHeight
    {
        get => GetValue(CaptionHeightProperty);
        set => SetValue(CaptionHeightProperty, value);
    }

    public static readonly StyledProperty<bool> HasLeftCaptionButtonProperty =
        AvaloniaProperty.Register<DevForm, bool>(nameof(HasLeftCaptionButton));

    public bool HasLeftCaptionButton
    {
        get => GetValue(HasLeftCaptionButtonProperty);
        set => SetValue(HasLeftCaptionButtonProperty, value);
    }

    public bool HasRightCaptionButton
    {
        get
        {
            if (OperatingSystem.IsLinux())
                return !Native.OS.UseSystemWindowFrame;

            return OperatingSystem.IsWindows();
        }
    }
    
    
    
    public DevForm()
    {
        
        if (OperatingSystem.IsMacOS())
        {
            HasLeftCaptionButton = true;
            CaptionHeight = new GridLength(34);
            ExtendClientAreaChromeHints =
                ExtendClientAreaChromeHints.SystemChrome | ExtendClientAreaChromeHints.OSXThickTitleBar;
        }
        else if (UseSystemWindowFrame)
        {
            CaptionHeight = new GridLength(30);
        }
        else
        {
            CaptionHeight = new GridLength(38);
        }
        
        InitializeComponent();
    }
    
    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        if (DataContext is DevFormViewModel vm)
        {
            vm.ModSwitcherClosed += () => ModSwitcherFlyoutButton.Flyout?.Hide();;
        }
    }
    
    private void ModSwitcherFlyout_OnOpened(object? sender, EventArgs e)
    {
        if (DataContext is DevFormViewModel vm)
        {
            vm.OnModSwitcherOpened();
        }
    }
    
    private void ModSwitcherFlyout_OnClosed(object? sender, EventArgs e)
    {
        if (DataContext is DevFormViewModel vm)
        {
            vm.OnModSwitcherClosed();
            // ModSwitcherFlyoutButton.Flyout?.Hide();
        }
    }
    
    protected override void OnClosing(WindowClosingEventArgs e)
    {
        base.OnClosing(e);

        if (!Design.IsDesignMode && DataContext is ViewModels.DevFormViewModel)
        {
            PreferencesWindowViewModel.Instance.Save();
        }
    }
    
    private void ShowDataItemNodeMenu(TreeDataGridRow current, DataItemNode node, DataRootNode parentNode,
        ContextRequestedEventArgs e)
    {
        var menu = new ContextMenu
        {
            Items =
            {
                new MenuItem { Header = "Resave as File", Command = parentNode.ResaveAsFile, CommandParameter = node, Icon = App.CreateMenuIcon("Icons.FileFill") },
                new MenuItem
                    { Header = "Resave as Patch", Command = parentNode.ResaveAsPatch, CommandParameter = node, Icon = App.CreateMenuIcon("Icons.FileTextFill") },
                new Separator(),
                new MenuItem { Header = "Edit", Icon = App.CreateMenuIcon("Icons.PencilFill") },
                new MenuItem { Header = "Delete", Command = parentNode.DeleteCommand, CommandParameter = node,  Icon = App.CreateMenuIcon("Icons.TrashFill") }
            }
        };

        AttachAndOpenMenu(current, menu, e);
    }

    private void ShowSpriteItemNodeMenu(TreeDataGridRow current, DataItemNode node, SpriteRootNode parentNode,
        ContextRequestedEventArgs e)
    {
        var menu = new ContextMenu
        {
            Items =
            {
                new MenuItem { Header = "Export", Command = parentNode.ExportCommand, CommandParameter = node, Icon = App.CreateMenuIcon("Icons.ExportFill") },
                new Separator(),
                new MenuItem { Header = "Delete", Command = parentNode.DeleteCommand, CommandParameter = node, Icon =  App.CreateMenuIcon("Icons.TrashFill") }
            }
        };

        AttachAndOpenMenu(current, menu, e);
    }

    private void ShowRootNodeMenu(TreeDataGridRow current, DataRootNode root, ContextRequestedEventArgs e)
    {
        var menu = new ContextMenu
        {
            Items =
            {
                new MenuItem { Header = "Re-Index", Command = root.ReIndexCommand, Icon = App.CreateMenuIcon("Icons.ListNumbersFill") },
                new MenuItem { Header = "Resave all as File", Command = root.ResaveAllAsFileCommand, Icon = App.CreateMenuIcon("Icons.FileFill") },
                new MenuItem { Header = "Resave all as Diff", Command = root.ResaveAllAsDiffCommand, Icon = App.CreateMenuIcon("Icons.PlusMinusFill") },
                new Separator(),
                new MenuItem { Header = "Add", Command = root.AddCommand, Icon = App.CreateMenuIcon("Icons.Plus") }
            }
        };

        AttachAndOpenMenu(current, menu, e);
    }
    
    private void ShowSpriteRootNodeMenu(TreeDataGridRow current, SpriteRootNode root, ContextRequestedEventArgs e)
    {
        var menu = new ContextMenu
        {
            Items =
            {
                new MenuItem { Header = "Mass Import", Command = root.MassImportCommand, Icon = App.CreateMenuIcon("Icons.DownloadSimpleFill") },
                new MenuItem { Header = "Mass Export", Command = root.MassExportCommand, Icon = App.CreateMenuIcon("Icons.ExportFill") },
                
                
                new Separator(),
                new MenuItem { Header = "Import", Command = root.ImportCommand, Icon = App.CreateMenuIcon("Icons.Plus") },
                new MenuItem { Header = "Re-Import", Command = root.ReImportCommand, Icon = App.CreateMenuIcon("Icons.RepeatFill") },
                // new MenuItem { Header = "Add", Command = root.AddCommand,  Icon = App.CreateMenuIcon("Icons.Plus") }
            }
        };

        if (root is SpriteTileRootNode node)
        {
            menu.Items.Insert(0, new MenuItem { Header = "Re-Index", Command = node.ReIndexCommand, Icon = App.CreateMenuIcon("Icons.ListNumbersFill") } );
        };

        AttachAndOpenMenu(current, menu, e);
    }

    private void AttachAndOpenMenu(TreeDataGridRow current, ContextMenu menu, ContextRequestedEventArgs e)
    {
        menu.Closed += (_, _) => LeftTreeDataGrid.ContextMenu = null;
        LeftTreeDataGrid.ContextMenu = menu;
        menu.Open(LeftTreeDataGrid);
        e.Handled = true;
    }
    
    private void LeftTreeDataGrid_OnDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (DataContext is DevFormViewModel vm && sender is TreeDataGrid treeView)
        {
            var selectedItem = (OpenEditorNode)treeView.RowSelection.SelectedItem;
            Console.WriteLine(selectedItem.ToString());
            vm.AddPageFromTreeNode(selectedItem);
        }
    }
    
    private void LeftTreeDataGrid_OnContextRequested(object? sender, ContextRequestedEventArgs e)
    {
        if (e.Source is not Visual visual)
            return;
    
        var row = visual.GetSelfAndVisualAncestors()
            .OfType<TreeDataGridRow>()
            .FirstOrDefault();
    
        if (row == null)
            return;
        
        if (row.DataContext is not NodeBase node)
            return;
        
        var parent = node.Parent;
   
    
        switch (node)
        {
            case DataItemNode itemNode when parent is DataRootNode root:
                ShowDataItemNodeMenu(row, itemNode, root, e);
                break;
    
            case DataItemNode itemNode when parent is SpriteRootNode spriteRoot:
                ShowSpriteItemNodeMenu(row, itemNode, spriteRoot, e);
                break;
    
            case DataRootNode rootNode:
                ShowRootNodeMenu(row, rootNode, e);
                break;
    
            case SpriteRootNode spriteRoot:
                ShowSpriteRootNodeMenu(row, spriteRoot, e);
                break;
        }
    }

    private async void LeftTreeDataGrid_OnLoaded(object sender, RoutedEventArgs e)
    {
    }

    private void LeftTreeDataGrid_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not TreeDataGrid treeDataGrid)
            return;
        
        var point = e.GetPosition(treeDataGrid);
        var visual = treeDataGrid.InputHitTest(point);
        
        var element = visual as Control;
        while (element != null && element is not TreeDataGridRow)
        {
            element = element.Parent as Control;
        }
    
        if (DungeonScene.Instance == null)
            return;

        GraphicsManager.AssetType debugAsset = GraphicsManager.AssetType.None;
        string? debugAnim = null;
        
        if (element is TreeDataGridRow row &&
            row.DataContext is DataItemNode node &&
            node.Parent is SpriteRootNode parent &&
            parent.AssetType.IsAnimEdit())
        {
            debugAsset = parent.AssetType;
            debugAnim = node.ItemKey;
        }

        lock (GameBase.lockObj)
        {
            DungeonScene.Instance.DebugAsset = debugAsset;
            DungeonScene.Instance.DebugAnim = debugAnim;
        }

    }

    private void LeftTreeDataGrid_OnLostFocus(object sender, RoutedEventArgs e)
    {
        
        if (!(sender is TreeDataGrid grid))
            return;
        
        
        if (grid.IsKeyboardFocusWithin)
            return;
        
        if (grid.ContextMenu?.IsOpen == true)
            return;
        
        var window = grid.GetVisualRoot() as Window;
        if (window is { IsActive: false })
            return; 
        // (grid.RowSelection.SelectedItem as DataItemNode)?.Parent?.ResaveAsFile(grid.RowSelection.SelectedItem as DataItemNode)
        // Not sure why it doesn't clear the grid...
        grid.RowSelection.Clear();
        
        lock (GameBase.lockObj)
        {
            if (DungeonScene.Instance != null)
            {
                DungeonScene.Instance.DebugAsset = GraphicsManager.AssetType.None;
                DungeonScene.Instance.DebugAnim = null;
            }
        }
    }

}