using System;
using System.Collections.Generic;
using System.Windows.Forms;
using RogueEssence.Data;
using RogueEssence.Content;
using RogueEssence.Dungeon;
using RogueEssence.Ground;
using RogueEssence.Script;
using Microsoft.Win32;
using RogueEssence.Menu;

namespace RogueEssence.Dev
{
    public partial class DevForm : Form, IRootEditor
    {
        public bool Loaded { get; private set; }

        private MapEditor mapEditor;
        private GroundEditor groundEditor;

        public IMapEditor MapEditor => mapEditor;
        public IGroundEditor GroundEditor => groundEditor;

        public DevForm()
        {
            floorIDs = new List<int>();

            InitializeComponent();
        }

        void IRootEditor.Load()
        {
            string[] item_names = DataManager.Instance.DataIndices[DataManager.DataType.Item].GetLocalStringArray();
            for (int ii = 0; ii < item_names.Length; ii++)
                cbSpawnItem.Items.Add(ii + " - " + item_names[ii]);
            object regVal = Registry.GetValue(DiagManager.REG_PATH, "ItemChoice", 0);
            cbSpawnItem.SelectedIndex = Math.Min(cbSpawnItem.Items.Count - 1, (regVal != null) ? (int)regVal : 0);

            string[] skill_names = DataManager.Instance.DataIndices[DataManager.DataType.Skill].GetLocalStringArray();
            for (int ii = 0; ii < skill_names.Length; ii++)
                cbSkills.Items.Add(ii + " - " + skill_names[ii]);
            regVal = Registry.GetValue(DiagManager.REG_PATH, "SkillChoice", 0);
            cbSkills.SelectedIndex = Math.Min(cbSkills.Items.Count - 1, (regVal != null) ? (int)regVal : 0);

            string[] intrinsic_names = DataManager.Instance.DataIndices[DataManager.DataType.Intrinsic].GetLocalStringArray();
            for (int ii = 0; ii < intrinsic_names.Length; ii++)
                cbIntrinsics.Items.Add(ii + " - " + intrinsic_names[ii]);
            regVal = Registry.GetValue(DiagManager.REG_PATH, "IntrinsicChoice", 0);
            cbIntrinsics.SelectedIndex = Math.Min(cbIntrinsics.Items.Count - 1, (regVal != null) ? (int)regVal : 0);

            string[] status_names = DataManager.Instance.DataIndices[DataManager.DataType.Status].GetLocalStringArray();
            for (int ii = 0; ii < status_names.Length; ii++)
                cbStatus.Items.Add(ii + " - " + status_names[ii]);
            regVal = Registry.GetValue(DiagManager.REG_PATH, "StatusChoice", 0);
            cbStatus.SelectedIndex = Math.Min(cbStatus.Items.Count - 1, (regVal != null) ? (int)regVal : 0);

            string[] monster_names = DataManager.Instance.DataIndices[DataManager.DataType.Monster].GetLocalStringArray();
            for (int ii = 0; ii < monster_names.Length; ii++)
                cbDexNum.Items.Add(ii + " - " + monster_names[ii]);
            cbDexNum.SelectedIndex = 0;


            string[] skin_names = DataManager.Instance.DataIndices[DataManager.DataType.Skin].GetLocalStringArray();
            for (int ii = 0; ii < DataManager.Instance.DataIndices[DataManager.DataType.Skin].Count; ii++)
                cbSkin.Items.Add(skin_names[ii]);

            cbSkin.SelectedIndex = 0;

            for (int ii = 0; ii < 3; ii++)
                cbGender.Items.Add(((Gender)ii).ToString());

            cbGender.SelectedIndex = 0;

            chkSprites.Checked = DataManager.Instance.HideChars;

            for (int ii = 0; ii < GraphicsManager.Actions.Count; ii++)
                cbAnim.Items.Add(GraphicsManager.Actions[ii].Name);

            cbAnim.SelectedIndex = 0;

            ZoneData zone = DataManager.Instance.GetZone(1);
            for (int ii = 0; ii < zone.GroundMaps.Count; ii++)
                cbMaps.Items.Add(zone.GroundMaps[ii]);

            regVal = Registry.GetValue(DiagManager.REG_PATH, "MapChoice", 0);
            cbMaps.SelectedIndex = Math.Min(Math.Max(0, (regVal != null) ? (int)regVal : 0), cbMaps.Items.Count - 1);

            string[] dungeon_names = DataManager.Instance.DataIndices[DataManager.DataType.Zone].GetLocalStringArray();
            for (int ii = 0; ii < dungeon_names.Length; ii++)
                cbZones.Items.Add(dungeon_names[ii]);
            regVal = Registry.GetValue(DiagManager.REG_PATH, "ZoneChoice", 0);
            cbZones.SelectedIndex = Math.Min(Math.Max(0, (regVal != null) ? (int)regVal : 0), cbZones.Items.Count - 1);

            regVal = Registry.GetValue(DiagManager.REG_PATH, "StructChoice", 0);
            cbStructure.SelectedIndex = Math.Min(Math.Max(0, (regVal != null) ? (int)regVal : 0), cbStructure.Items.Count - 1);

            regVal = Registry.GetValue(DiagManager.REG_PATH, "FloorChoice", 0);
            cbFloor.SelectedIndex = Math.Min(Math.Max(0, (regVal != null) ? (int)regVal : 0), cbFloor.Items.Count - 1);


            chkObject.Checked = DataManager.Instance.HideObjects;


            //Script tab
            m_cntDownArrow = 0;
            m_lastcommands = new Stack<string>();

            Show();
        }

        private void chkShowSprites_CheckedChanged(object sender, EventArgs e)
        {
            DataManager.Instance.HideChars = !chkSprites.Checked;
        }

        private void cbDexNum_SelectedIndexChanged(object sender, EventArgs e)
        {
            cbForm.Items.Clear();
            for (int ii = 0; ii < DataManager.Instance.GetMonster(cbDexNum.SelectedIndex).Forms.Count; ii++)
            {
                cbForm.Items.Add(ii + " - " + DataManager.Instance.GetMonster(cbDexNum.SelectedIndex).Forms[ii].FormName.ToLocal());
            }
            cbForm.SelectedIndex = 0;
            UpdateSprite();
        }

        private void cbForm_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateSprite();
        }

        private void cbSkin_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateSprite();
        }

        private void cbGender_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateSprite();
        }

        private void UpdateSprite()
        {
            if (IsInGame())
                DungeonScene.Instance.FocusedCharacter.Promote(new Dungeon.MonsterID(cbDexNum.SelectedIndex, cbForm.SelectedIndex, cbSkin.SelectedIndex, (Gender)cbGender.SelectedIndex));
        }

        private void btnEditSprites_Click(object sender, EventArgs e)
        {
            FormEditTree choices = new FormEditTree();
            choices.LoadFormDataEntries(true);
            choices.Show();
        }

        private void btnEditPortraits_Click(object sender, EventArgs e)
        {
            FormEditTree choices = new FormEditTree();
            choices.LoadFormDataEntries(false);
            choices.Show();
        }

        private void cbAnim_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (IsInGame())
            {
                CharAnimIdle action = new CharAnimIdle(Dungeon.DungeonScene.Instance.FocusedCharacter.CharLoc, Dungeon.DungeonScene.Instance.FocusedCharacter.CharDir);

                int index = cbAnim.SelectedIndex;

                GraphicsManager.GlobalIdle = index;
            }
        }

        private bool IsInGame()
        {
            return (ZoneManager.Instance.CurrentZone != null && Dungeon.DungeonScene.Instance.ActiveTeam.Players.Count > 0 && Dungeon.DungeonScene.Instance.FocusedCharacter != null);
        }

        private void btnMapEditor_Click(object sender, EventArgs e)
        {
            if (ZoneManager.Instance.CurrentMap != null && mapEditor == null)
            {
                mapEditor = new MapEditor();
                mapEditor.FormClosed += (sender, e) => mapEditor = null;
                mapEditor.Show();
            }
        }


        private void btnGroundEditor_Click(object sender, EventArgs e)
        {
            if (groundEditor == null)
            {
                MenuManager.Instance.ClearMenus();
                if (ZoneManager.Instance.CurrentGround != null)
                    GameManager.Instance.SceneOutcome = GameManager.Instance.MoveToEditor(true, ZoneManager.Instance.CurrentGround.AssetName);
                else
                    GameManager.Instance.SceneOutcome = GameManager.Instance.MoveToEditor(true, "");
            }

        }

        public void OpenGround()
        {
            groundEditor = new GroundEditor();
            groundEditor.FormClosed += groundEditorClosed;
            groundEditor.LoadFromCurrentGround();
            groundEditor.Show();
        }

        public void groundEditorClosed(object sender, EventArgs e)
        {
            GameManager.Instance.SceneOutcome = resetEditors();
        }


        private IEnumerator<YieldInstruction> resetEditors()
        {
            groundEditor = null;
            mapEditor = null;
            yield return CoroutineManager.Instance.StartCoroutine(GameManager.Instance.RestartToTitle());
        }


        public void CloseGround()
        {
            if (groundEditor != null)
                groundEditor.Close();
        }

        private void btnEditMonster_Click(object sender, EventArgs e)
        {
            OpenList(DataManager.DataType.Monster, DataManager.Instance.GetMonster, () => { return new MonsterData(); });
        }
        private void btnEditSkill_Click(object sender, EventArgs e)
        {
            OpenList(DataManager.DataType.Skill, DataManager.Instance.GetSkill, () => { return new SkillData(); });
        }
        private void btnEditIntrinsics_Click(object sender, EventArgs e)
        {
            OpenList(DataManager.DataType.Intrinsic, DataManager.Instance.GetIntrinsic, () => { return new IntrinsicData(); });
        }
        private void btnEditItem_Click(object sender, EventArgs e)
        {
            OpenList(DataManager.DataType.Item, DataManager.Instance.GetItem, () => { return new ItemData(); });
        }
        private void btnEditZone_Click(object sender, EventArgs e)
        {
            OpenList(DataManager.DataType.Zone, DataManager.Instance.GetZone, () => { return new ZoneData(); });
        }
        private void btnEditStatuses_Click(object sender, EventArgs e)
        {
            OpenList(DataManager.DataType.Status, DataManager.Instance.GetStatus, () => { return new StatusData(); });
        }
        private void btnEditMapStatuses_Click(object sender, EventArgs e)
        {
            OpenList(DataManager.DataType.MapStatus, DataManager.Instance.GetMapStatus, () => { return new MapStatusData(); });
        }
        private void btnEditTerrain_Click(object sender, EventArgs e)
        {
            OpenList(DataManager.DataType.Terrain, DataManager.Instance.GetTerrain, () => { return new TerrainData(); });
        }
        private void btnEditTiles_Click(object sender, EventArgs e)
        {
            OpenList(DataManager.DataType.Tile, DataManager.Instance.GetTile, () => { return new TileData(); });
        }
        private void btnEditAutoTile_Click(object sender, EventArgs e)
        {
            OpenList(DataManager.DataType.AutoTile, DataManager.Instance.GetAutoTile, () => { return new AutoTileData(); });
        }

        private delegate string[] GetEntryNames();
        private delegate IEntryData GetEntry(int entryNum);
        private delegate IEntryData CreateEntry();
        private void OpenList(DataManager.DataType dataType, GetEntry entryOp, CreateEntry createOp)
        {

            DataList choices = new DataList();
            choices.Text = dataType.ToString();
            string[] entries = DataManager.Instance.DataIndices[dataType].GetLocalStringArray(true);
            choices.AddEntries(entries);
            choices.SelectedOKEvent = () => {
                if (choices.ChosenEntry > -1)
                {
                    ElementForm editor = new ElementForm();
                    int entryNum = choices.ChosenEntry;
                    editor.Text = entries[entryNum];
                    IEntryData data = entryOp(entryNum);
                    editor.Text = data.ToString();//data.GetType().ToString() + "#" + entryNum;
                    DataEditor.LoadDataControls(data, editor.ControlPanel);

                    editor.OnOK += (object okSender, EventArgs okE) => {
                        object obj = data;
                        DataEditor.SaveDataControls(ref obj, editor.ControlPanel);
                        data = (IEntryData)obj;
                        DataManager.SaveData(entryNum, dataType.ToString(), data);
                        DataManager.Instance.ClearCache(dataType);
                        IEntryData entryData = ((IEntryData)data);
                        EntrySummary entrySummary = entryData.GenerateEntrySummary();
                        DataManager.Instance.DataIndices[dataType].Entries[entryNum] = entrySummary;
                        DataManager.Instance.SaveIndex(dataType);
                        choices.ModifyEntry(entryNum, entrySummary.GetLocalString(true));
                        editor.Close();
                    };
                    editor.OnCancel += (object okSender, EventArgs okE) => {
                        editor.Close();
                    };

                    editor.Show();
                }
            };
            choices.SelectedAddEvent = () => {
                ElementForm editor = new ElementForm();
                int entryNum = DataManager.Instance.DataIndices[dataType].Entries.Count;
                editor.Text = "New " + dataType.ToString();
                IEntryData data = createOp();
                editor.Text = data.ToString();//data.GetType().ToString() + "#" + entryNum;
                DataEditor.LoadDataControls(data, editor.ControlPanel);

                editor.OnOK += (object okSender, EventArgs okE) => {
                    object obj = data;
                    DataEditor.SaveDataControls(ref obj, editor.ControlPanel);
                    data = (IEntryData)obj;
                    DataManager.SaveData(entryNum, dataType.ToString(), data);
                    DataManager.Instance.ClearCache(dataType);
                    IEntryData entryData = ((IEntryData)data);
                    EntrySummary entrySummary = entryData.GenerateEntrySummary();
                    DataManager.Instance.DataIndices[dataType].Entries.Add(entrySummary);
                    DataManager.Instance.SaveIndex(dataType);
                    entries = DataManager.Instance.DataIndices[dataType].GetLocalStringArray(true);
                    choices.AddEntry(entrySummary.GetLocalString(true));
                    editor.Close();
                };
                editor.OnCancel += (object okSender, EventArgs okE) => {
                    editor.Close();
                };

                editor.Show();
            };
            choices.Show();

        }

        private void chkObject_CheckedChanged(object sender, EventArgs e)
        {
            DataManager.Instance.HideObjects = chkObject.Checked;
        }

        private void DevWindow_Load(object sender, EventArgs e)
        {
            Loaded = true;
        }

        private void DevWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
            DiagManager.Instance.LoadMsg = "Closing...";
            EnterLoadPhase(GameBase.LoadPhase.Unload);
        }

        public static void EnterLoadPhase(GameBase.LoadPhase loadState)
        {
            GameBase.CurrentPhase = loadState;
        }


        private void btnSpawn_Click(object sender, EventArgs e)
        {
            MonsterTeam team = new MonsterTeam();
            Character focusedChar = DungeonScene.Instance.FocusedCharacter;
            CharData character = new CharData();
            character.BaseForm = focusedChar.CurrentForm;
            character.Nickname = "Clone";
            character.Level = focusedChar.Level;

            for (int ii = 0; ii < CharData.MAX_SKILL_SLOTS; ii++)
                character.BaseSkills[ii] = new SlotSkill(focusedChar.BaseSkills[ii]);

            for (int ii = 0; ii < CharData.MAX_INTRINSIC_SLOTS; ii++)
                character.BaseIntrinsics[ii] = focusedChar.BaseIntrinsics[ii];

            Character new_mob = new Character(character, team);
            team.Players.Add(new_mob);
            new_mob.CharLoc = focusedChar.CharLoc;
            new_mob.CharDir = focusedChar.CharDir;
            new_mob.Tactic = new AITactic(focusedChar.Tactic);
            new_mob.EquippedItem = new InvItem(focusedChar.EquippedItem);
            ZoneManager.Instance.CurrentMap.MapTeams.Add(new_mob.MemberTeam);
            new_mob.RefreshTraits();
            DungeonScene.Instance.PendingDevEvent = new_mob.OnMapStart();
        }

        private void btnDespawn_Click(object sender, EventArgs e)
        {
            ZoneManager.Instance.CurrentMap.MapTeams.Clear();
        }

        private void btnSpawnItem_Click(object sender, EventArgs e)
        {
            Registry.SetValue(DiagManager.REG_PATH, "ItemChoice", cbSpawnItem.SelectedIndex);
            InvItem item = new InvItem(cbSpawnItem.SelectedIndex);
            ItemData entry = (ItemData)item.GetData();
            if (entry.MaxStack > 1)
                item.HiddenValue = entry.MaxStack;
            if (GameManager.Instance.CurrentScene == DungeonScene.Instance)
                DungeonScene.Instance.PendingDevEvent = DungeonScene.Instance.DropItem(item, DungeonScene.Instance.FocusedCharacter.CharLoc);
            else if (GameManager.Instance.CurrentScene == GroundScene.Instance)
            {
                if (DataManager.Instance.Save.ActiveTeam.GetInvCount() < DataManager.Instance.Save.ActiveTeam.GetMaxInvSlots(ZoneManager.Instance.CurrentZone))
                {
                    GameManager.Instance.SE("Menu/Sort");
                    DataManager.Instance.Save.ActiveTeam.AddToInv(item);
                }
                else
                    GameManager.Instance.SE("Menu/Cancel");
            }
            else
                GameManager.Instance.SE("Menu/Cancel");
        }

        private void btnRollSkill_Click(object sender, EventArgs e)
        {
            if (DungeonScene.Instance.ActiveTeam.Players.Count > 0 && Dungeon.DungeonScene.Instance.FocusedCharacter != null)
            {
                Character character = DungeonScene.Instance.FocusedCharacter;
                BaseMonsterForm form = DataManager.Instance.GetMonster(character.BaseForm.Species).Forms[character.BaseForm.Form];

                while (character.BaseSkills[0].SkillNum > -1)
                    character.DeleteSkill(0);
                List<int> final_skills = form.RollLatestSkills(character.Level, new List<int>());
                foreach (int skill in final_skills)
                    character.LearnSkill(skill, true);

                DungeonScene.Instance.LogMsg(String.Format("Skills reloaded"), false, true);
            }
        }

        private void nudLevel_ValueChanged(object sender, EventArgs e)
        {
            if (Dungeon.DungeonScene.Instance.ActiveTeam.Players.Count > 0 && Dungeon.DungeonScene.Instance.FocusedCharacter != null)
                Dungeon.DungeonScene.Instance.FocusedCharacter.Level = (int)nudLevel.Value;
        }

        private void btnToggleStatus_Click(object sender, EventArgs e)
        {
            Registry.SetValue(DiagManager.REG_PATH, "StatusChoice", cbStatus.SelectedIndex);
            int statusIndex = cbStatus.SelectedIndex;
            StatusData entry = DataManager.Instance.GetStatus(statusIndex);
            if (Dungeon.DungeonScene.Instance.ActiveTeam.Players.Count > 0 && Dungeon.DungeonScene.Instance.FocusedCharacter != null)
            {
                Character player = Dungeon.DungeonScene.Instance.FocusedCharacter;
                if (entry.Targeted)
                    Dungeon.DungeonScene.Instance.LogMsg(String.Format("This is a targeted status."), false, true);
                else
                {
                    if (player.StatusEffects.ContainsKey(statusIndex))
                        Dungeon.DungeonScene.Instance.PendingDevEvent = player.RemoveStatusEffect(statusIndex);
                    else
                    {
                        StatusEffect status = new StatusEffect(statusIndex);
                        status.LoadFromData();
                        Dungeon.DungeonScene.Instance.PendingDevEvent = player.AddStatusEffect(status);
                    }
                }
            }
        }

        private void btnLearnSkill_Click(object sender, EventArgs e)
        {
            Registry.SetValue(DiagManager.REG_PATH, "SkillChoice", cbSkills.SelectedIndex);
            if (DungeonScene.Instance.ActiveTeam.Players.Count > 0 && Dungeon.DungeonScene.Instance.FocusedCharacter != null)
            {
                Character player = Dungeon.DungeonScene.Instance.FocusedCharacter;
                if (player.BaseSkills[CharData.MAX_SKILL_SLOTS-1].SkillNum > -1)
                    player.DeleteSkill(0);
                player.LearnSkill(cbSkills.SelectedIndex, true);
                Dungeon.DungeonScene.Instance.LogMsg(String.Format("Taught {1} to {0}.", player.Name, DataManager.Instance.GetSkill(cbSkills.SelectedIndex).Name.ToLocal()), false, true);
            }
        }

        private void btnGiveSkill_Click(object sender, EventArgs e)
        {
            Registry.SetValue(DiagManager.REG_PATH, "SkillChoice", cbSkills.SelectedIndex);
            Character player = Dungeon.DungeonScene.Instance.FocusedCharacter;
            foreach (Character character in Dungeon.ZoneManager.Instance.CurrentMap.IterateCharacters())
            {
                if (Dungeon.DungeonScene.Instance.GetMatchup(player, character) == Alignment.Foe)
                {
                    if (character.BaseSkills[CharData.MAX_SKILL_SLOTS - 1].SkillNum > -1)
                        character.DeleteSkill(0);
                    character.LearnSkill(cbSkills.SelectedIndex, true);
                }
                        
            }
            Dungeon.DungeonScene.Instance.LogMsg(String.Format("Taught {0} to all foes.", DataManager.Instance.GetSkill(cbSkills.SelectedIndex).Name.ToLocal()), false, true);
        }

        private void btnSetIntrinsic_Click(object sender, EventArgs e)
        {
            Registry.SetValue(DiagManager.REG_PATH, "IntrinsicChoice", cbIntrinsics.SelectedIndex);
            if (Dungeon.DungeonScene.Instance.ActiveTeam.Players.Count > 0 && Dungeon.DungeonScene.Instance.FocusedCharacter != null)
            {
                Character player = Dungeon.DungeonScene.Instance.FocusedCharacter;
                player.LearnIntrinsic(cbIntrinsics.SelectedIndex, 0);
                Dungeon.DungeonScene.Instance.LogMsg(String.Format("Gave {1} to {0}.", player.Name, DataManager.Instance.GetIntrinsic(cbIntrinsics.SelectedIndex).Name.ToLocal()), false, true);
            }
        }

        private void btnGiveFoes_Click(object sender, EventArgs e)
        {
            Registry.SetValue(DiagManager.REG_PATH, "IntrinsicChoice", cbIntrinsics.SelectedIndex);
            Character player = Dungeon.DungeonScene.Instance.FocusedCharacter;
            foreach (Character character in Dungeon.ZoneManager.Instance.CurrentMap.IterateCharacters())
            {
                if (Dungeon.DungeonScene.Instance.GetMatchup(player, character) == Alignment.Foe)
                    character.LearnIntrinsic(cbIntrinsics.SelectedIndex, 0);
            }
            Dungeon.DungeonScene.Instance.LogMsg(String.Format("Gave {0} to all foes.", DataManager.Instance.GetIntrinsic(cbIntrinsics.SelectedIndex).Name.ToLocal()), false, true);
        }


        //
        // Script Tab
        //
        private Stack<string> m_lastcommands;
        private int          m_cntDownArrow;   //counts the ammount of times the down arrow has been pressed in a row, for looking through the last commands!
        private void btnReloadScripts_Click(object sender, EventArgs e)
        {
            //Reload everything
            LuaEngine.Instance.Reset();
            LuaEngine.Instance.ReInit();
        }

        private void txtScriptInput_KeyPress(object sender, KeyPressEventArgs e)
        {
            switch (e.KeyChar)
            {
                case (char)Keys.Enter:
                    {
                        if (Control.ModifierKeys == Keys.Shift || Control.ModifierKeys == Keys.Control)
                        {
                            //That's just a newline
                            txtScriptInput.AppendText("\n");
                        }
                        else
                        {
                            txtScriptOutput.AppendText("\n" + txtScriptInput.Text);
                            m_lastcommands.Push(txtScriptInput.Text);
                            //Send the text to the script engine
                            LuaEngine.Instance.RunString(txtScriptInput.Text);
                            txtScriptInput.Clear();
                        }
                        m_cntDownArrow = 0;
                        break;
                    }
                case (char)Keys.Tab:
                    {
                        string[] strs = m_lastcommands.ToArray();
                        if (m_cntDownArrow < strs.Length)
                        {
                            txtScriptInput.Clear();
                            txtScriptInput.Text = strs[m_cntDownArrow];
                            m_cntDownArrow++;
                        }
                        else if (m_cntDownArrow >= strs.Length)
                        {
                            txtScriptInput.Clear();
                            m_cntDownArrow = 0;
                        }
                        e.Handled = true;
                        break;
                    }
                default:
                    {
                        m_cntDownArrow = 0;
                        break;
                    }
            }
        }

        // Map Tab

        private void btnEnterMap_Click(object sender, EventArgs e)
        {
            Registry.SetValue(DiagManager.REG_PATH, "MapChoice", cbMaps.SelectedIndex);
            MenuManager.Instance.ClearMenus();
            GameManager.Instance.SceneOutcome = GameManager.Instance.DebugWarp(new ZoneLoc(1, new SegLoc(-1, cbMaps.SelectedIndex)), RogueElements.MathUtils.Rand.NextUInt64());
        }

        private void btnEnterDungeon_Click(object sender, EventArgs e)
        {
            Registry.SetValue(DiagManager.REG_PATH, "ZoneChoice", cbZones.SelectedIndex);
            Registry.SetValue(DiagManager.REG_PATH, "StructChoice", cbStructure.SelectedIndex);
            Registry.SetValue(DiagManager.REG_PATH, "FloorChoice", cbFloor.SelectedIndex);
            MenuManager.Instance.ClearMenus();
            GameManager.Instance.SceneOutcome = GameManager.Instance.DebugWarp(new ZoneLoc(cbZones.SelectedIndex, new SegLoc(cbStructure.SelectedIndex, floorIDs[cbFloor.SelectedIndex])), RogueElements.MathUtils.Rand.NextUInt64());
        }


        private void cbZones_SelectedIndexChanged(object sender, EventArgs e)
        {
            cbStructure.Items.Clear();
            ZoneData zone = DataManager.Instance.GetZone(cbZones.SelectedIndex);
            for (int ii = 0; ii < zone.Structures.Count; ii++)
            {
                cbStructure.Items.Add(ii/* + " - " + zone.Structures[ii].Name.ToLocal()*/);
            }
            cbStructure.SelectedIndex = 0;
        }

        private List<int> floorIDs;
        private void cbStructure_SelectedIndexChanged(object sender, EventArgs e)
        {
            floorIDs.Clear();
            cbFloor.Items.Clear();
            ZoneData zone = DataManager.Instance.GetZone(cbZones.SelectedIndex);
            foreach (int ii in zone.Structures[cbStructure.SelectedIndex].GetFloorIDs())
            {
                cbFloor.Items.Add(ii/* + " - " + zone.Structures[cbStructure.SelectedIndex].Floors[ii].Name.ToLocal()*/);
                floorIDs.Add(ii);
            }
            cbFloor.SelectedIndex = 0;
        }
    }
}
