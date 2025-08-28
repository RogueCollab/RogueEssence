using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using RogueElements;
using RogueEssence.Content;
using RogueEssence.Data;
using RogueEssence.Dev.Views;
using RogueEssence.Dungeon;
using RogueEssence.Menu;
using RogueEssence.Script;
using Avalonia.Interactivity;

namespace RogueEssence.Dev.ViewModels
{
    public class DevTabDataViewModel : ViewModelBase
    {

        public void btnEditMonster_Click()
        {
            OpenList(DataManager.DataType.Monster, DataManager.Instance.GetMonster, () => { return new MonsterData(); });
        }
        public void btnEditSkill_Click()
        {
            OpenList(DataManager.DataType.Skill, DataManager.Instance.GetSkill, () => { return new SkillData(); });
        }
        public void btnEditIntrinsics_Click()
        {
            OpenList(DataManager.DataType.Intrinsic, DataManager.Instance.GetIntrinsic, () => { return new IntrinsicData(); });
        }
        public void btnEditItem_Click()
        {
            OpenList(DataManager.DataType.Item, DataManager.Instance.GetItem, () => { return new ItemData(); });
        }
        public void btnEditZone_Click()
        {
            OpenList(DataManager.DataType.Zone, DataManager.Instance.GetZone, () => { return new ZoneData(); });
        }
        public void btnEditStatuses_Click()
        {
            OpenList(DataManager.DataType.Status, DataManager.Instance.GetStatus, () => { return new StatusData(); });
        }
        public void btnEditMapStatuses_Click()
        {
            OpenList(DataManager.DataType.MapStatus, DataManager.Instance.GetMapStatus, () => { return new MapStatusData(); });
        }
        public void btnEditTerrain_Click()
        {
            OpenList(DataManager.DataType.Terrain, DataManager.Instance.GetTerrain, () => { return new TerrainData(); });
        }
        public void btnEditTiles_Click()
        {
            OpenList(DataManager.DataType.Tile, DataManager.Instance.GetTile, () => { return new TileData(); });
        }
        public void btnEditAutoTile_Click()
        {
            DataManager.DataType dataType = DataManager.DataType.AutoTile;
            GetEntry entryOp = DataManager.Instance.GetAutoTile;
            CreateEntry createOp = () => { return new AutoTileData(); };

            lock (GameBase.lockObj)
            {
                DataListForm dataListForm = new DataListForm();
                DataListFormViewModel choices = createChoices(dataListForm, dataType, entryOp, createOp);
                DataOpContainer reindexOp = createReindexOp(dataType, choices);

                DataOpContainer.TaskAction importAction = async () => { await importDtef(dataListForm, choices); }; ;
                DataOpContainer importOp = new DataOpContainer("Import DTEF", importAction);


                DataOpContainer.TaskAction exportAction = async () => { await exportDtef(dataListForm, choices.SearchList.InternalIndex); };
                
                DataOpContainer exportOp = new DataOpContainer("Export as DTEF", exportAction);
                choices.SetOps(reindexOp, importOp/*, exportOp*/);

                dataListForm.DataContext = choices;
                dataListForm.Show();
            }
        }

        private async Task importDtef(DataListForm listForm, DataListFormViewModel choices)
        {
            //remember addresses in registry
            string folderName = DevForm.GetConfig("TilesetDir", Directory.GetCurrentDirectory());

            //open window to choose directory
            OpenFolderDialog openFileDialog = new OpenFolderDialog();
            openFileDialog.Directory = folderName;

            string folder = await openFileDialog.ShowAsync(listForm);

            if (!String.IsNullOrEmpty(folder))
            {
                string animName = Path.GetFileNameWithoutExtension(folder);

                bool conflict = false;
                foreach (string name in GraphicsManager.TileIndex.Nodes.Keys)
                {
                    if (name.ToLower() == animName.ToLower())
                    {
                        conflict = true;
                        break;
                    }
                }

                if (conflict)
                {
                    MessageBox.MessageBoxResult result = await MessageBox.Show(listForm, "Are you sure you want to overwrite the existing sheet:\n" + animName, "Tileset already exists.",
                        MessageBox.MessageBoxButtons.YesNo);
                    if (result == MessageBox.MessageBoxResult.No)
                        return;
                }

                DevForm.SetConfig("TilesetDir", Path.GetDirectoryName(folder));

                try
                {
                    DevForm.ExecuteOrPend(() => { tryImportDtef(choices, folder, animName); });
                }
                catch (Exception ex)
                {
                    DiagManager.Instance.LogError(ex, false);
                    await MessageBox.Show(listForm, "Error importing from\n" + folder + "\n\n" + ex.Message, "Import Failed", MessageBox.MessageBoxButtons.Ok);
                    return;
                }
            }
        }

        private void tryImportDtef(DataListFormViewModel choices, string folder, string animName)
        {
            lock (GameBase.lockObj)
            {
                string destFile = PathMod.HardMod(string.Format(Content.GraphicsManager.TILE_PATTERN, animName));
                DtefImportHelper.ImportDtef(folder, destFile);

                //reindex graphics
                GraphicsManager.RebuildIndices(GraphicsManager.AssetType.Tile);
                GraphicsManager.ClearCaches(GraphicsManager.AssetType.Tile);
                DevDataManager.ClearCaches();

                //reindex data
                DevHelper.RunIndexing(DataManager.DataType.AutoTile);
                DevHelper.RunExtraIndexing(DataManager.DataType.AutoTile);
                DataManager.Instance.LoadIndex(DataManager.DataType.AutoTile);
                DataManager.Instance.LoadUniversalIndices();
                DataManager.Instance.ClearCache(DataManager.DataType.AutoTile);
                DiagManager.Instance.DevEditor.ReloadData(DataManager.DataType.AutoTile);
                Dictionary<string, string> entries = DataManager.Instance.DataIndices[DataManager.DataType.AutoTile].GetLocalStringArray(true);
                choices.SetEntries(entries);
            }
        }

        private async Task exportDtef(DataListForm listForm, int entryIndex)
        {
            if (entryIndex > -1)
            {
                //remember addresses in registry
                string folderName = DevForm.GetConfig("TilesetDir", Directory.GetCurrentDirectory());

                //open window to choose directory
                OpenFolderDialog openFileDialog = new OpenFolderDialog();
                openFileDialog.Directory = folderName;

                string folder = await openFileDialog.ShowAsync(listForm);

                if (!String.IsNullOrEmpty(folder))
                {
                    DevForm.SetConfig("TilesetDir", Path.GetDirectoryName(folder));

                    DevForm.ExecuteOrPend(() => { tryExportDtef(entryIndex, folder); });
                }
            }
        }
        private void tryExportDtef(int entryIndex, string folder)
        {
            lock (GameBase.lockObj)
            {
                DtefImportHelper.ExportDtefTile(entryIndex, folder);
            }
        }

        private List<string> getDtefConflictSheets(string animName)
        {
            List<string> conflicts = new List<string>();

            foreach (string name in GraphicsManager.TileIndex.Nodes.Keys)
            {
                foreach (string suffix in DtefImportHelper.TileTitles)
                {
                    string subName = animName + suffix;
                    if (name.ToLower() == subName.ToLower())
                        conflicts.Add(name);
                }
            }

            return conflicts;
        }

        public void btnEditEmote_Click()
        {
            OpenList(DataManager.DataType.Emote, DataManager.Instance.GetEmote, () => { return new EmoteData(); });
        }

        public void btnEditElement_Click()
        {
            OpenList(DataManager.DataType.Element, DataManager.Instance.GetElement, () => { return new ElementData(); });
        }

        public void btnEditGrowthGroup_Click()
        {
            OpenList(DataManager.DataType.GrowthGroup, DataManager.Instance.GetGrowth, () => { return new GrowthData(); });
        }

        public void btnEditSkillGroup_Click()
        {
            OpenList(DataManager.DataType.SkillGroup, DataManager.Instance.GetSkillGroup, () => { return new SkillGroupData(); });
        }

        public void btnEditRank_Click()
        {
            OpenList(DataManager.DataType.Rank, DataManager.Instance.GetRank, () => { return new RankData(); });
        }

        public void btnEditSkin_Click()
        {
            OpenList(DataManager.DataType.Skin, DataManager.Instance.GetSkin, () => { return new SkinData(); });
        }

        public void btnEditAI_Click()
        {
            OpenList(DataManager.DataType.AI, DataManager.Instance.GetAITactic, () => { return new AITactic(); });
        }


        private delegate string[] GetEntryNames();
        private delegate IEntryData GetEntry(string entryNum);
        private delegate IEntryData CreateEntry();
        private void OpenList(DataManager.DataType dataType, GetEntry entryOp, CreateEntry createOp)
        {
            lock (GameBase.lockObj)
            {
                DataListForm dataListForm = new DataListForm();
                DataListFormViewModel choices = createChoices(dataListForm, dataType, entryOp, createOp);
                DataOpContainer reindexOp = createReindexOp(dataType, choices);
                DataOpContainer resaveFileOp = createResaveFileDiffOp(dataType, choices, entryOp, false);
                DataOpContainer resaveDiffOp = createResaveFileDiffOp(dataType, choices, entryOp, true);
                choices.SetOps(reindexOp, resaveFileOp, resaveDiffOp);

                dataListForm.DataContext = choices;
                dataListForm.SetListContextMenu(CreateContextMenu(choices));
                dataListForm.Show();
            }
        }

        private DataOpContainer createReindexOp(DataManager.DataType dataType, DataListFormViewModel choices)
        {
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
            DataOpContainer.TaskAction reindexAction = async () =>
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
            {
                lock (GameBase.lockObj)
                {
                    DevHelper.RunIndexing(dataType);
                    DevHelper.RunExtraIndexing(dataType);
                    DataManager.Instance.LoadIndex(dataType);
                    DataManager.Instance.LoadUniversalIndices();
                    DataManager.Instance.ClearCache(dataType);
                    DiagManager.Instance.DevEditor.ReloadData(dataType);
                    Dictionary<string, string> entries = DataManager.Instance.DataIndices[dataType].GetLocalStringArray(true);
                    choices.SetEntries(entries);
                }
            };
            return new DataOpContainer("Re-Index", reindexAction);
        }


        private DataOpContainer createResaveFileDiffOp(DataManager.DataType dataType, DataListFormViewModel choices, GetEntry entryOp, bool asDiff)
        {
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
            DataOpContainer.TaskAction reindexAction = async () =>
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
            {
                lock (GameBase.lockObj)
                {
                    DevHelper.Resave(dataType, asDiff);

                    //then you have to reindex everything anyway
                    DevHelper.RunIndexing(dataType);
                    DevHelper.RunExtraIndexing(dataType);
                    DataManager.Instance.LoadIndex(dataType);
                    DataManager.Instance.LoadUniversalIndices();
                    DataManager.Instance.ClearCache(dataType);
                    DiagManager.Instance.DevEditor.ReloadData(dataType);
                    Dictionary<string, string> entries = DataManager.Instance.DataIndices[dataType].GetLocalStringArray(true);
                    choices.SetEntries(entries);
                }
            };
            if (asDiff)
                return new DataOpContainer("Resave all as Diff", reindexAction);
            else
                return new DataOpContainer("Resave all as File", reindexAction);
        }

        private DataListFormViewModel createChoices(DataListForm form, DataManager.DataType dataType, GetEntry entryOp, CreateEntry createOp)
        {
            DataListFormViewModel choices = new DataListFormViewModel();
            choices.Name = dataType.ToString();
            Dictionary<string, string> entries = DataManager.Instance.DataIndices[dataType].GetLocalStringArray(true);
            choices.SetEntries(entries);

            choices.SelectedOKEvent += async () =>
            {
                if (choices.ChosenAsset != null)
                {
                    lock (GameBase.lockObj)
                    {
                        string entryNum = choices.ChosenAsset;
                        IEntryData data = entryOp(entryNum);

                        DataEditForm editor = new DataEditRootForm();
                        editor.Title = DataEditor.GetWindowTitle(String.Format("{0} #{1}", dataType.ToString(), entryNum), data.Name.ToLocal(), data, data.GetType());
                        DataEditor.LoadDataControls(entryNum, data, editor);
                        editor.SelectedOKEvent += async () =>
                        {
                            lock (GameBase.lockObj)
                            {
                                object obj = data;
                                DataEditor.SaveDataControls(ref obj, editor.ControlPanel, new Type[0]);
                                DataManager.Instance.ContentChanged(dataType, entryNum, (IEntryData)obj);

                                string newName = DataManager.Instance.DataIndices[dataType].Get(entryNum).GetLocalString(true);
                                choices.ModifyEntry(entryNum, newName);
                                return true;
                            }
                        };

                        editor.Show();
                    }
                }
            };

            choices.SelectedAddEvent += async () =>
            {
                // Show a name entry window
                RenameWindow window = new RenameWindow();
                RenameViewModel vm = new RenameViewModel();
                window.DataContext = vm;

                bool result = await window.ShowDialog<bool>(form);
                if (!result)
                    return;

                lock (GameBase.lockObj)
                {
                    string assetName = Text.GetNonConflictingName(Text.Sanitize(vm.Name).ToLower(), DataManager.Instance.DataIndices[dataType].ContainsKey);
                    DataManager.Instance.ContentChanged(dataType, assetName, createOp());
                    string newName = DataManager.Instance.DataIndices[dataType].Get(assetName).GetLocalString(true);
                    choices.AddEntry(assetName, newName);

                    if (dataType == DataManager.DataType.Zone)
                        LuaEngine.Instance.CreateZoneScriptDir(assetName);
                }
            };


            choices.SelectedDeleteEvent += async () =>
            {
                string assetName = choices.ChosenAsset;

                DataManager.ModStatus modStatus = DataManager.GetEntryDataModStatus(assetName, dataType.ToString());

                if (PathMod.Quest.IsValid() && modStatus == DataManager.ModStatus.Base)
                {
                    await MessageBox.Show(form, String.Format("The {0} {1} is not a part of the currently edited mod cannot be deleted.", dataType.ToString(), assetName), "Delete " + dataType.ToString(),
                        MessageBox.MessageBoxButtons.Ok);
                    return;
                }

                MessageBox.MessageBoxResult result;
                if (modStatus == DataManager.ModStatus.Base || modStatus == DataManager.ModStatus.Added)
                    result = await MessageBox.Show(form, String.Format("Are you sure you want to delete the following {0}:\n{1}", dataType.ToString(), assetName), "Delete " + dataType.ToString(),
                        MessageBox.MessageBoxButtons.YesNo);
                else
                    result = await MessageBox.Show(form, String.Format("The following {0} will be reset back to the base game:\n{1}", dataType.ToString(), assetName), "Delete " + dataType.ToString(),
                        MessageBox.MessageBoxButtons.YesNo);

                if (result == MessageBox.MessageBoxResult.No)
                    return;

                lock (GameBase.lockObj)
                {
                    DataManager.Instance.ContentChanged(dataType, assetName, null);
                    choices.DeleteEntry(assetName);
                    if (DataManager.Instance.DataIndices[dataType].ContainsKey(assetName))
                    {
                        string newName = DataManager.Instance.DataIndices[dataType].Get(assetName).GetLocalString(true);
                        choices.AddEntry(assetName, newName);
                    }

                    if (dataType == DataManager.DataType.Zone)
                    {
                        string str = LuaEngine.MakeZoneScriptPath(assetName, "");
                        if (Directory.Exists(str))
                            Directory.Delete(str, true);
                    }
                }
            };

            choices.SelectedSaveFileEvent += async () =>
            {
                string entryNum = choices.ChosenAsset;
                if (DataManager.GetEntryDataModStatus(entryNum, dataType.ToString()) == DataManager.ModStatus.Base)
                {
                    await MessageBox.Show(form, String.Format("{0} must have saved edits first!", entryNum), "Error", MessageBox.MessageBoxButtons.Ok);
                    return;
                }

                lock (GameBase.lockObj)
                {
                    IEntryData data = entryOp(entryNum);
                    DataManager.Instance.ContentResaved(dataType, entryNum, data, false);

                    string newName = DataManager.Instance.DataIndices[dataType].Get(entryNum).GetLocalString(true);
                    choices.ModifyEntry(entryNum, newName);
                }

                await MessageBox.Show(form, String.Format("{0} is now saved as a file.", entryNum), "Complete", MessageBox.MessageBoxButtons.Ok);
            };

            choices.SelectedSaveDiffEvent += async () =>
            {
                string entryNum = choices.ChosenAsset;
                DataManager.ModStatus modStatus = DataManager.GetEntryDataModStatus(entryNum, dataType.ToString());
                if (modStatus == DataManager.ModStatus.Base)
                {
                    await MessageBox.Show(form, String.Format("{0} must have saved edits first!", entryNum), "Error", MessageBox.MessageBoxButtons.Ok);
                    return;
                }
                else if (modStatus == DataManager.ModStatus.Added)
                {
                    await MessageBox.Show(form, String.Format("{0} is newly added in this mod and cannot be saved as patch.", entryNum), "Error", MessageBox.MessageBoxButtons.Ok);
                    return;
                }

                lock (GameBase.lockObj)
                {
                    IEntryData data = entryOp(entryNum);
                    DataManager.Instance.ContentResaved(dataType, entryNum, data, true);

                    string newName = DataManager.Instance.DataIndices[dataType].Get(entryNum).GetLocalString(true);
                    choices.ModifyEntry(entryNum, newName);
                }

                if (DataManager.GetEntryDataModStatus(entryNum, dataType.ToString()) == DataManager.ModStatus.Base)
                    await MessageBox.Show(form, String.Format("Modded {0} was identical to base. Unneeded patch removed.", entryNum), "Complete", MessageBox.MessageBoxButtons.Ok);
                else
                    await MessageBox.Show(form, String.Format("{0} is now saved as a patch.", entryNum), "Complete", MessageBox.MessageBoxButtons.Ok);
            };

            return choices;
        }


        public static ContextMenu CreateContextMenu(DataListFormViewModel vm)
        {
            ContextMenu saveAsStrip = new ContextMenu();

            MenuItem saveAsFileMenuItem = new MenuItem();
            MenuItem saveAsDiffMenuItem = new MenuItem();

            Avalonia.Collections.AvaloniaList<object> list = (Avalonia.Collections.AvaloniaList<object>)saveAsStrip.Items;
            list.AddRange(new MenuItem[] {
                            saveAsFileMenuItem,
                            saveAsDiffMenuItem});

            saveAsFileMenuItem.Header = "Resave as File";
            saveAsDiffMenuItem.Header = "Resave as Patch";

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
            saveAsFileMenuItem.Click += async (object copySender, RoutedEventArgs copyE) =>
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
            {
                vm.mnuDataFile_Click();
            };
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
            saveAsDiffMenuItem.Click += async (object copySender, RoutedEventArgs copyE) =>
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
            {
                vm.mnuDataDiff_Click();
            };
            return saveAsStrip;
        }


        public void btnMapEditor_Click()
        {
            lock (GameBase.lockObj)
            {
                Views.DevForm form = (Views.DevForm)DiagManager.Instance.DevEditor;
                if (form.MapEditForm == null)
                {
                    LuaEngine.Instance.BreakScripts();
                    MenuManager.Instance.ClearMenus();
                    if (ZoneManager.Instance.CurrentMap != null)
                        GameManager.Instance.SceneOutcome = GameManager.Instance.MoveToEditor(false, ZoneManager.Instance.CurrentMap.AssetName);
                    else
                        GameManager.Instance.SceneOutcome = GameManager.Instance.MoveToEditor(false, "");
                }
            }
        }

        public void btnGroundEditor_Click()
        {
            lock (GameBase.lockObj)
            {
                Views.DevForm form = (Views.DevForm)DiagManager.Instance.DevEditor;
                if (form.GroundEditForm == null)
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

    }
}
