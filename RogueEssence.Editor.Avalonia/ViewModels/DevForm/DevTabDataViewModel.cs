using System;
using System.Collections.Generic;
using System.Text;
using RogueEssence.Data;
using RogueEssence.Dungeon;
using RogueEssence.Menu;

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
                choices.AddEntries(entries);

                choices.SelectedOKEvent += () =>
                {
                    if (choices.SearchList.InternalIndex > -1)
                    {
                    //ElementForm editor = new ElementForm();
                    //int entryNum = choices.ChosenEntry;
                    //editor.Text = entries[entryNum];
                    //IEntryData data = entryOp(entryNum);
                    //editor.Text = data.ToString();//data.GetType().ToString() + "#" + entryNum;
                    //DataEditor.LoadDataControls(data, editor.ControlPanel);

                    //editor.OnOK += (object okSender, EventArgs okE) =>
                    //{
                    //    object obj = data;
                    //    DataEditor.SaveDataControls(ref obj, editor.ControlPanel);
                    //    data = (IEntryData)obj;
                    //    DataManager.SaveData(entryNum, dataType.ToString(), data);
                    //    DataManager.Instance.ClearCache(dataType);
                    //    IEntryData entryData = ((IEntryData)data);
                    //    EntrySummary entrySummary = entryData.GenerateEntrySummary();
                    //    DataManager.Instance.DataIndices[dataType].Entries[entryNum] = entrySummary;
                    //    DataManager.Instance.SaveIndex(dataType);
                    //    choices.ModifyEntry(entryNum, entrySummary.GetLocalString(true));
                    //    editor.Close();
                    //};
                    //editor.OnCancel += (object okSender, EventArgs okE) =>
                    //{
                    //    editor.Close();
                    //};

                    //editor.Show();
                }
                };
                choices.SelectedAddEvent += () =>
                {
                //ElementForm editor = new ElementForm();
                //int entryNum = DataManager.Instance.DataIndices[dataType].Entries.Count;
                //editor.Text = "New " + dataType.ToString();
                //IEntryData data = createOp();
                //editor.Text = data.ToString();//data.GetType().ToString() + "#" + entryNum;
                //DataEditor.LoadDataControls(data, editor.ControlPanel);

                //editor.OnOK += (object okSender, EventArgs okE) =>
                //{
                //    object obj = data;
                //    DataEditor.SaveDataControls(ref obj, editor.ControlPanel);
                //    data = (IEntryData)obj;
                //    DataManager.SaveData(entryNum, dataType.ToString(), data);
                //    DataManager.Instance.ClearCache(dataType);
                //    IEntryData entryData = ((IEntryData)data);
                //    EntrySummary entrySummary = entryData.GenerateEntrySummary();
                //    DataManager.Instance.DataIndices[dataType].Entries.Add(entrySummary);
                //    DataManager.Instance.SaveIndex(dataType);
                //    entries = DataManager.Instance.DataIndices[dataType].GetLocalStringArray(true);
                //    choices.AddEntry(entrySummary.GetLocalString(true));
                //    editor.Close();
                //};
                //editor.OnCancel += (object okSender, EventArgs okE) =>
                //{
                //    editor.Close();
                //};

                //editor.Show();
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

        }

        public void btnGroundEditor_Click()
        {
            lock (GameBase.lockObj)
            {
                Views.DevForm form = (Views.DevForm)DiagManager.Instance.DevEditor;
                if (form.GroundEditForm == null)
                {
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
