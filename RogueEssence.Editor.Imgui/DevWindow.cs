using Microsoft.Xna.Framework;
using ImGuiNET;
using ImGuiNET.XNA;
using Microsoft.Xna.Framework.Graphics;
using System;
using RogueEssence.Content;
using RogueEssence;
using RogueEssence.Data;
using RogueEssence.Dungeon;
using Num = System.Numerics;
using System.Collections.Generic;
using RogueEssence.Ground;
using RogueEssence.Menu;

namespace RogueEssence.Dev
{
    public class DevWindow : IRootEditor
    {
        public bool LoadComplete { get; private set; }
        public IGroundEditor GroundEditor => null;
        public IMapEditor MapEditor => null;

        public bool AteMouse { get { return ImGui.GetIO().WantCaptureMouse; } }
        public bool AteKeyboard { get { return ImGui.GetIO().WantCaptureKeyboard; } }

        private ImGuiRenderer _imGuiRenderer;


        public void Load(GameBase game)
        {
            _imGuiRenderer = new ImGuiRenderer(game);
            _imGuiRenderer.RebuildFontAtlas();

            //ImGui.StyleColorsDark();
            ImGui.StyleColorsClassic();

            LoadData();
            LoadComplete = true;
        }

        public void Update(GameTime gameTime)
        {
            if (!LoadComplete)
                return;
            _imGuiRenderer.BeforeLayout(gameTime);

            ;
        }
        public void Draw()
        {
            if (!LoadComplete)
                return;

            // Draw our UI
            ImGuiLayout();

            // Call AfterLayout now to finish up and draw all the things
            _imGuiRenderer.AfterLayout();
        }

        int currentSkill;
        int currentIntrinsic;
        int currentStatus;
        int currentItem;

        string[] skill_names;
        string[] intrinsic_names;
        string[] status_names;
        string[] item_names;


        string[] monster_names;
        string[] skin_names;
        string[] gender_names;
        string[] anim_names;


        int currentGround;
        int currentDungeon;
        int currentStructure;
        int currentFloor;

        string[] ground_names;
        string[] dungeon_names;
        string[] structure_names;
        string[] floor_names;
        List<int> floorIDs;

        protected void LoadData()
        {
            item_names = DataManager.Instance.DataIndices[DataManager.DataType.Item].GetLocalStringArray();
            for (int ii = 0; ii < item_names.Length; ii++)
                item_names[ii] = ii.ToString("D3") + ": " + item_names[ii];
            //object regVal = Registry.GetValue(DiagManager.REG_PATH, "ItemChoice", 0);
            //cbSpawnItem.SelectedIndex = Math.Min(cbSpawnItem.Items.Count - 1, (regVal != null) ? (int)regVal : 0);

            skill_names = DataManager.Instance.DataIndices[DataManager.DataType.Skill].GetLocalStringArray();
            for (int ii = 0; ii < skill_names.Length; ii++)
                skill_names[ii] = ii.ToString("D3") + ": " + skill_names[ii];
            //regVal = Registry.GetValue(DiagManager.REG_PATH, "SkillChoice", 0);
            //cbSkills.SelectedIndex = Math.Min(cbSkills.Items.Count - 1, (regVal != null) ? (int)regVal : 0);

            intrinsic_names = DataManager.Instance.DataIndices[DataManager.DataType.Intrinsic].GetLocalStringArray();
            for (int ii = 0; ii < intrinsic_names.Length; ii++)
                intrinsic_names[ii] = ii.ToString("D3") + ": " + intrinsic_names[ii];
            //regVal = Registry.GetValue(DiagManager.REG_PATH, "IntrinsicChoice", 0);
            //cbIntrinsics.SelectedIndex = Math.Min(cbIntrinsics.Items.Count - 1, (regVal != null) ? (int)regVal : 0);

            status_names = DataManager.Instance.DataIndices[DataManager.DataType.Status].GetLocalStringArray();
            for (int ii = 0; ii < status_names.Length; ii++)
                status_names[ii] = ii.ToString("D3") + ": " + status_names[ii];
            //regVal = Registry.GetValue(DiagManager.REG_PATH, "StatusChoice", 0);
            //cbStatus.SelectedIndex = Math.Min(cbStatus.Items.Count - 1, (regVal != null) ? (int)regVal : 0);

            monster_names = DataManager.Instance.DataIndices[DataManager.DataType.Monster].GetLocalStringArray();
            for (int ii = 0; ii < monster_names.Length; ii++)
                monster_names[ii] = ii.ToString("D3") + ": " + monster_names[ii];

            skin_names = DataManager.Instance.DataIndices[DataManager.DataType.Skin].GetLocalStringArray();

            List<string> names = new List<string>();
            for (int ii = 0; ii < 3; ii++)
                names.Add(((Gender)ii).ToString());
            gender_names = names.ToArray();


            names = new List<string>();
            for (int ii = 0; ii < GraphicsManager.Actions.Count; ii++)
                names.Add(GraphicsManager.Actions[ii].Name);
            anim_names = names.ToArray();


            ZoneData zone = DataManager.Instance.GetZone(1);
            ground_names = zone.GroundMaps.ToArray();

            //regVal = Registry.GetValue(DiagManager.REG_PATH, "MapChoice", 0);
            //cbMaps.SelectedIndex = Math.Min(Math.Max(0, (regVal != null) ? (int)regVal : 0), cbMaps.Items.Count - 1);

            dungeon_names = DataManager.Instance.DataIndices[DataManager.DataType.Zone].GetLocalStringArray();
            //regVal = Registry.GetValue(DiagManager.REG_PATH, "ZoneChoice", 0);
            //cbZones.SelectedIndex = Math.Min(Math.Max(0, (regVal != null) ? (int)regVal : 0), cbZones.Items.Count - 1);

            UpdateDungeon();


        }

        protected virtual void ImGuiLayout()
        {
            ImGui.Begin("Dev Form");
            ImGuiTabBarFlags tab_bar_flags = ImGuiTabBarFlags.None;
            if (ImGui.BeginTabBar("MyTabBar", tab_bar_flags))
            {
                GameTab();
                PlayerTab();
                TravelTab();
                ImGui.EndTabBar();
            }

            ImGui.End();
        }

        protected void GameTab()
        {
            if (ImGui.BeginTabItem("Game"))
            {
                ImGui.Checkbox("Hide Chars", ref DataManager.Instance.HideChars);
                ImGui.SameLine(ImGui.GetWindowWidth() * 0.5f);
                ImGui.Checkbox("Hide Objects", ref DataManager.Instance.HideObjects);
                if (ImGui.Button("Create Foe", new Num.Vector2(ImGui.GetWindowWidth() * 0.5f - 10, 20)))
                    SpawnClone();
                ImGui.SameLine(ImGui.GetWindowWidth() * 0.5f);
                if (ImGui.Button("Clear Foes", new Num.Vector2(ImGui.GetWindowWidth() * 0.5f - 10, 20)))
                    ClearFoes();
                ImGui.Combo("Skills", ref currentSkill, skill_names, skill_names.Length);
                if (ImGui.Button("Learn Skill", new Num.Vector2(ImGui.GetWindowWidth() * 0.5f - 10, 20)))
                    GiveSkill();
                ImGui.SameLine(ImGui.GetWindowWidth() * 0.5f);
                if (ImGui.Button("Teach Foes", new Num.Vector2(ImGui.GetWindowWidth() * 0.5f - 10, 20)))
                    GiveSkillFoe();
                ImGui.Combo("Intrinsic", ref currentIntrinsic, intrinsic_names, intrinsic_names.Length);
                if (ImGui.Button("Set Intrinsic", new Num.Vector2(ImGui.GetWindowWidth() * 0.5f - 10, 20)))
                    GiveIntrinsic();
                ImGui.SameLine(ImGui.GetWindowWidth() * 0.5f);
                if (ImGui.Button("Give Foes", new Num.Vector2(ImGui.GetWindowWidth() * 0.5f - 10, 20)))
                    GiveIntrinsicFoe();
                ImGui.Combo("Statuses", ref currentStatus, status_names, status_names.Length);
                if (ImGui.Button("Toggle Status", new Num.Vector2(ImGui.GetWindowWidth(), 20)))
                    ToggleStatus();
                ImGui.Combo("Items", ref currentItem, item_names, item_names.Length);
                if (ImGui.Button("Create Item", new Num.Vector2(ImGui.GetWindowWidth(), 20)))
                    CreateItem();
                ImGui.EndTabItem();
            }
        }

        protected void SpawnClone()
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

        protected void ClearFoes()
        {
            ZoneManager.Instance.CurrentMap.MapTeams.Clear();
        }

        protected void GiveSkill()
        {
            //Registry.SetValue(DiagManager.REG_PATH, "SkillChoice", cbSkills.SelectedIndex);
            if (DungeonScene.Instance.ActiveTeam.Players.Count > 0 && Dungeon.DungeonScene.Instance.FocusedCharacter != null)
            {
                Character player = DungeonScene.Instance.FocusedCharacter;
                if (player.BaseSkills[CharData.MAX_SKILL_SLOTS - 1].SkillNum > -1)
                    player.DeleteSkill(0);
                player.LearnSkill(currentSkill, true);
                DungeonScene.Instance.LogMsg(String.Format("Taught {1} to {0}.", player.Name, DataManager.Instance.GetSkill(currentSkill).Name.ToLocal()), false, true);
            }
        }

        protected void GiveSkillFoe()
        {
            //Registry.SetValue(DiagManager.REG_PATH, "SkillChoice", cbSkills.SelectedIndex);
            Character player = DungeonScene.Instance.FocusedCharacter;
            foreach (Character character in ZoneManager.Instance.CurrentMap.IterateCharacters())
            {
                if (DungeonScene.Instance.GetMatchup(player, character) == Alignment.Foe)
                {
                    if (character.BaseSkills[CharData.MAX_SKILL_SLOTS - 1].SkillNum > -1)
                        character.DeleteSkill(0);
                    character.LearnSkill(currentSkill, true);
                }

            }
            DungeonScene.Instance.LogMsg(String.Format("Taught {0} to all foes.", DataManager.Instance.GetSkill(currentSkill).Name.ToLocal()), false, true);
        }

        protected void GiveIntrinsic()
        {
            //Registry.SetValue(DiagManager.REG_PATH, "IntrinsicChoice", cbIntrinsics.SelectedIndex);
            if (DungeonScene.Instance.ActiveTeam.Players.Count > 0 && DungeonScene.Instance.FocusedCharacter != null)
            {
                Character player = DungeonScene.Instance.FocusedCharacter;
                player.LearnIntrinsic(currentIntrinsic, 0);
                DungeonScene.Instance.LogMsg(String.Format("Gave {1} to {0}.", player.Name, DataManager.Instance.GetIntrinsic(currentIntrinsic).Name.ToLocal()), false, true);
            }
        }

        protected void GiveIntrinsicFoe()
        {
            //Registry.SetValue(DiagManager.REG_PATH, "IntrinsicChoice", cbIntrinsics.SelectedIndex);
            Character player = DungeonScene.Instance.FocusedCharacter;
            foreach (Character character in Dungeon.ZoneManager.Instance.CurrentMap.IterateCharacters())
            {
                if (DungeonScene.Instance.GetMatchup(player, character) == Alignment.Foe)
                    character.LearnIntrinsic(currentIntrinsic, 0);
            }
            DungeonScene.Instance.LogMsg(String.Format("Gave {0} to all foes.", DataManager.Instance.GetIntrinsic(currentIntrinsic).Name.ToLocal()), false, true);
        }

        protected void ToggleStatus()
        {
            //Registry.SetValue(DiagManager.REG_PATH, "StatusChoice", cbStatus.SelectedIndex);
            StatusData entry = DataManager.Instance.GetStatus(currentStatus);
            if (DungeonScene.Instance.ActiveTeam.Players.Count > 0 && DungeonScene.Instance.FocusedCharacter != null)
            {
                Character player = DungeonScene.Instance.FocusedCharacter;
                if (entry.Targeted)
                    DungeonScene.Instance.LogMsg(String.Format("This is a targeted status."), false, true);
                else
                {
                    if (player.StatusEffects.ContainsKey(currentStatus))
                        DungeonScene.Instance.PendingDevEvent = player.RemoveStatusEffect(currentStatus);
                    else
                    {
                        StatusEffect status = new StatusEffect(currentStatus);
                        status.LoadFromData();
                        DungeonScene.Instance.PendingDevEvent = player.AddStatusEffect(status);
                    }
                }
            }
        }

        protected void CreateItem()
        {
            //Registry.SetValue(DiagManager.REG_PATH, "ItemChoice", cbSpawnItem.SelectedIndex);
            InvItem item = new InvItem(currentItem);
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

        protected void PlayerTab()
        {
            if (ImGui.BeginTabItem("Player"))
            {
                if (IsInGame())
                {
                    //ImGui.InputInt("input int", ref ,);
                    //ImGui.DragInt("Lv.", ref DataManager.Instance.Save.ActiveTeam.Leader.Level, 1, 1, 100);
                    ImGui.DragInt("Level", ref DungeonScene.Instance.FocusedCharacter.Level, 0.5f, 1, 100);
                    if (ImGui.Button("Reroll Skills", new Num.Vector2(ImGui.GetWindowWidth(), 20)))
                        RerollSkills();
                    MonsterID id = DataManager.Instance.Save.ActiveTeam.Leader.BaseForm;
                    int gender = (int)id.Gender;
                    bool changed = false;
                    changed |= ImGui.Combo("Species", ref id.Species, monster_names, monster_names.Length);

                    List<string> names = new List<string>();
                    MonsterData entry = DataManager.Instance.GetMonster(id.Species);
                    for (int ii = 0; ii < entry.Forms.Count; ii++)
                        names.Add(ii.ToString("D3") + ": " + entry.Forms[ii].FormName.ToLocal());
                    string[] form_names = names.ToArray();
                    id.Form = Math.Clamp(id.Form, 0, entry.Forms.Count - 1);

                    changed |= ImGui.Combo("Form", ref id.Form, form_names, form_names.Length);
                    changed |= ImGui.Combo("Skin", ref id.Skin, skin_names, skin_names.Length);
                    changed |= ImGui.Combo("Gender", ref gender, gender_names, gender_names.Length);
                    id.Gender = (Gender)gender;
                    if (changed)
                        UpdateSprite(id);
                    ImGui.Combo("Animation", ref GraphicsManager.GlobalIdle, anim_names, anim_names.Length);
                }
                ImGui.EndTabItem();
            }
        }

        protected void RerollSkills()
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

        protected void TravelTab()
        {
            if (ImGui.BeginTabItem("Travel"))
            {
                ImGui.Combo("Ground Map", ref currentGround, ground_names, ground_names.Length);
                if (ImGui.Button("Enter Ground Map", new Num.Vector2(ImGui.GetWindowWidth(), 20)))
                    EnterGround();

                if (ImGui.Combo("Dungeon", ref currentDungeon, dungeon_names, dungeon_names.Length))
                    UpdateDungeon();

                if (ImGui.Combo("Structure", ref currentStructure, structure_names, structure_names.Length))
                    UpdateStructure();

                ImGui.Combo("Floor", ref currentFloor, floor_names, floor_names.Length);
                if (ImGui.Button("Enter Dungeon", new Num.Vector2(ImGui.GetWindowWidth(), 20)))
                    EnterMap();
                ImGui.EndTabItem();
            }
        }

        protected void UpdateDungeon()
        {
            ZoneData zone = DataManager.Instance.GetZone(currentDungeon);
            List<string> names = new List<string>();
            for (int ii = 0; ii < zone.Structures.Count; ii++)
                names.Add(ii.ToString()/* + " - " + zone.Structures[ii].Name.ToLocal()*/);
            structure_names = names.ToArray();
            currentStructure = Math.Clamp(currentStructure, 0, Math.Max(0, structure_names.Length-1));
            UpdateStructure();
        }

        protected void UpdateStructure()
        {
            ZoneData zone = DataManager.Instance.GetZone(currentDungeon);
            List<string> names = new List<string>();
            floorIDs = new List<int>();
            if (zone.Structures.Count > 0)
            {
                foreach (int ii in zone.Structures[currentStructure].GetFloorIDs())
                {
                    names.Add(ii.ToString()/* + " - " + zone.Structures[cbStructure.SelectedIndex].Floors[ii].Name.ToLocal()*/);
                    floorIDs.Add(ii);
                }
            }
            floor_names = names.ToArray();
            currentFloor = Math.Clamp(currentFloor, 0, Math.Max(0, floor_names.Length - 1));
        }

        private void UpdateSprite(MonsterID id)
        {
            if (IsInGame())
                DungeonScene.Instance.FocusedCharacter.Promote(id);
        }

        protected void EnterGround()
        {
            //Registry.SetValue(DiagManager.REG_PATH, "MapChoice", cbMaps.SelectedIndex);
            MenuManager.Instance.ClearMenus();
            GameManager.Instance.SceneOutcome = GameManager.Instance.DebugWarp(new ZoneLoc(1, new SegLoc(-1, currentGround)), RogueElements.MathUtils.Rand.NextUInt64());
        }
        protected void EnterMap()
        {
            //Registry.SetValue(DiagManager.REG_PATH, "ZoneChoice", cbZones.SelectedIndex);
            //Registry.SetValue(DiagManager.REG_PATH, "StructChoice", cbStructure.SelectedIndex);
            //Registry.SetValue(DiagManager.REG_PATH, "FloorChoice", cbFloor.SelectedIndex);
            MenuManager.Instance.ClearMenus();
            GameManager.Instance.SceneOutcome = GameManager.Instance.DebugWarp(new ZoneLoc(currentDungeon, new SegLoc(currentStructure, floorIDs[currentFloor])), RogueElements.MathUtils.Rand.NextUInt64());
        }


        private bool IsInGame()
        {
            return (ZoneManager.Instance.CurrentZone != null && DungeonScene.Instance.ActiveTeam.Players.Count > 0 && DungeonScene.Instance.FocusedCharacter != null);
        }

        public void OpenGround() { }
        public void CloseGround() { }
    }
}