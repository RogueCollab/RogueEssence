using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using RogueElements;
using RogueEssence.Data;
using RogueEssence.Dev.Views;
using RogueEssence.Dungeon;
using RogueEssence.Menu;
using RogueEssence.Script;

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
                DataListFormViewModel choices = createChoices(dataType, entryOp, createOp);
                DataOpContainer reindexOp = createReindexOp(dataType, choices);

                DataOpContainer.TaskAction importAction = importDtef;
                DataOpContainer importOp = new DataOpContainer("Import DTEF", importAction);


                DataOpContainer.TaskAction exportAction = async () => { await exportDtef(choices.SearchList.InternalIndex); };
                
                DataOpContainer exportOp = new DataOpContainer("Export as DTEF", exportAction);
                choices.SetOps(reindexOp/*, importOp, exportOp*/);

                DataListForm dataListForm = new DataListForm
                {
                    DataContext = choices,
                };
                dataListForm.Show();
            }
        }

        private async Task importDtef()
        {
            //remember addresses in registry
            string folderName = DevForm.GetConfig("TilesetDir", Directory.GetCurrentDirectory());

            //open window to choose directory
            OpenFolderDialog openFileDialog = new OpenFolderDialog();
            openFileDialog.Directory = folderName;

            DevForm form = (DevForm)DiagManager.Instance.DevEditor;
            string folder = await openFileDialog.ShowAsync(form);

            if (folder != "")
            {
                string animName = Path.GetFileNameWithoutExtension(folder);

                bool conflict = false;
                foreach (string name in Content.GraphicsManager.TileIndex.Nodes.Keys)
                {
                    if (name.ToLower() == animName.ToLower())
                    {
                        conflict = true;
                        break;
                    }
                }

                if (conflict)
                {
                    MessageBox.MessageBoxResult result = await MessageBox.Show(form, "Are you sure you want to overwrite the existing sheet:\n" + animName, "Tileset already exists.",
                        MessageBox.MessageBoxButtons.YesNo);
                    if (result == MessageBox.MessageBoxResult.No)
                        return;
                }

                DevForm.SetConfig("TilesetDir", Path.GetDirectoryName(folder));

                try
                {
                    DevForm.ExecuteOrPend(() => { tryImportDtef(folder, animName); });
                }
                catch (Exception ex)
                {
                    DiagManager.Instance.LogError(ex, false);
                    await MessageBox.Show(form, "Error importing from\n" + folder + "\n\n" + ex.Message, "Import Failed", MessageBox.MessageBoxButtons.Ok);
                    return;
                }
            }
        }

        private void tryImportDtef(string folder, string animName)
        {
            lock (GameBase.lockObj)
            {
                string destFile = PathMod.HardMod(string.Format(Content.GraphicsManager.TILE_PATTERN, animName));
                DtefImportHelper.ImportDtef(folder, destFile);
            }
        }

        private async Task exportDtef(int entryIndex)
        {
            if (entryIndex > -1)
            {
                //remember addresses in registry
                string folderName = DevForm.GetConfig("TilesetDir", Directory.GetCurrentDirectory());

                //open window to choose directory
                OpenFolderDialog openFileDialog = new OpenFolderDialog();
                openFileDialog.Directory = folderName;

                DevForm form = (DevForm)DiagManager.Instance.DevEditor;
                string folder = await openFileDialog.ShowAsync(form);

                if (folder != "")
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

            foreach (string name in Content.GraphicsManager.TileIndex.Nodes.Keys)
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


        private delegate string[] GetEntryNames();
        private delegate IEntryData GetEntry(int entryNum);
        private delegate IEntryData CreateEntry();
        private void OpenList(DataManager.DataType dataType, GetEntry entryOp, CreateEntry createOp)
        {
            lock (GameBase.lockObj)
            {
                DataListFormViewModel choices = createChoices(dataType, entryOp, createOp);
                DataOpContainer reindexOp = createReindexOp(dataType, choices);
                choices.SetOps(reindexOp);

                Views.DataListForm dataListForm = new Views.DataListForm
                {
                    DataContext = choices,
                };
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
                    DataManager.Instance.LoadUniversalData();
                    DataManager.Instance.ClearCache(dataType);
                    DiagManager.Instance.DevEditor.ReloadData(dataType);
                    string[] entries = DataManager.Instance.DataIndices[dataType].GetLocalStringArray(true);
                    choices.SetEntries(entries);
                }
            };
            return new DataOpContainer("Re-Index", reindexAction);
        }

        private DataListFormViewModel createChoices(DataManager.DataType dataType, GetEntry entryOp, CreateEntry createOp)
        {
            DataListFormViewModel choices = new DataListFormViewModel();
            choices.Name = dataType.ToString();
            string[] entries = DataManager.Instance.DataIndices[dataType].GetLocalStringArray(true);
            choices.SetEntries(entries);

            choices.SelectedOKEvent += () =>
            {
                if (choices.SearchList.InternalIndex > -1)
                {
                    lock (GameBase.lockObj)
                    {
                        int entryNum = choices.SearchList.InternalIndex;
                        IEntryData data = entryOp(entryNum);

                        DataEditForm editor = new DataEditForm();
                        editor.Title = DataEditor.GetWindowTitle(String.Format("{0} #{1:D3}", dataType.ToString(), entryNum), data.Name.ToLocal(), data, data.GetType());
                        DataEditor.LoadDataControls(data, editor.ControlPanel);
                        editor.SelectedOKEvent += () =>
                        {
                            lock (GameBase.lockObj)
                            {
                                object obj = data;
                                DataEditor.SaveDataControls(ref obj, editor.ControlPanel, new Type[0]);
                                DataManager.Instance.ContentChanged(dataType, entryNum, (IEntryData)obj);

                                string newName = DataManager.Instance.DataIndices[dataType].Entries[entryNum].GetLocalString(true);
                                choices.ModifyEntry(entryNum, newName);
                                editor.Close();
                            }
                        };
                        editor.SelectedCancelEvent += () =>
                        {
                            editor.Close();
                        };

                        editor.Show();
                    }
                }
            };
            choices.SelectedAddEvent += () =>
            {
                lock (GameBase.lockObj)
                {
                    int entryNum = DataManager.Instance.DataIndices[dataType].Count;
                    IEntryData data = createOp();

                    DataEditForm editor = new DataEditForm();
                    editor.Title = DataEditor.GetWindowTitle(String.Format("{0} #{1:D3}", dataType.ToString(), entryNum), data.Name.ToLocal(), data, data.GetType()); data.ToString();
                    DataEditor.LoadDataControls(data, editor.ControlPanel);
                    editor.SelectedOKEvent += () =>
                    {
                        lock (GameBase.lockObj)
                        {
                            object obj = data;
                            DataEditor.SaveDataControls(ref obj, editor.ControlPanel, new Type[0]);
                            data = (IEntryData)obj;
                            DataManager.Instance.ContentChanged(dataType, entryNum, (IEntryData)obj);

                            string newName = DataManager.Instance.DataIndices[dataType].Entries[entryNum].GetLocalString(true);
                            choices.AddEntry(newName);
                            editor.Close();
                        }
                    };
                    editor.SelectedCancelEvent += () =>
                    {
                        editor.Close();
                    };

                    editor.Show();
                }
            };
            return choices;
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
