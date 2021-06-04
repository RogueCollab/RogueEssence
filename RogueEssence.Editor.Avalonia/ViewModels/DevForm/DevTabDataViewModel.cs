using System;
using System.Collections.Generic;
using System.Text;
using Avalonia.Controls;
using RogueElements;
using RogueEssence.Data;
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
            OpenList(DataManager.DataType.AutoTile, DataManager.Instance.GetAutoTile, () => { return new AutoTileData(); });
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

                            Views.DataEditForm editor = new Views.DataEditForm();
                            editor.Title = DataEditor.GetWindowTitle(String.Format("{0} #{1:D3}", dataType.ToString(), entryNum), data.Name.ToLocal(), data, data.GetType());
                            DataEditor.LoadDataControls(data, editor.ControlPanel);
                            editor.SelectedOKEvent += () =>
                            {
                                lock (GameBase.lockObj)
                                {
                                    object obj = data;
                                    DataEditor.SaveDataControls(ref obj, editor.ControlPanel);
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
                        int entryNum = DataManager.Instance.DataIndices[dataType].Entries.Count;
                        IEntryData data = createOp();

                        Views.DataEditForm editor = new Views.DataEditForm();
                        editor.Title = DataEditor.GetWindowTitle(String.Format("{0} #{1:D3}", dataType.ToString(), entryNum), data.Name.ToLocal(), data, data.GetType()); data.ToString();
                        DataEditor.LoadDataControls(data, editor.ControlPanel);
                        editor.SelectedOKEvent += () =>
                        {
                            lock (GameBase.lockObj)
                            {
                                object obj = data;
                                DataEditor.SaveDataControls(ref obj, editor.ControlPanel);
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

                choices.SelectedReindexEvent += () =>
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

                Views.DataListForm dataListForm = new Views.DataListForm
                {
                    DataContext = choices,
                };
                dataListForm.Show();
            }
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
