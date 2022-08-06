using RogueEssence.Ground;
using RogueEssence.Dungeon;
using RogueEssence.Data;
using System.Linq;

using NLua;
using System.Collections.Generic;
using RogueElements;
using System;

namespace RogueEssence.Script
{
    class ScriptGame : ILuaEngineComponent
    {
        public IRandom Rand { get { return MathUtils.Rand; } }

        public LuaFunction GroundSave;

        public Coroutine _GroundSave()
        {
            return new Coroutine(GroundScene.Instance.SaveGame());
        }

        public ModDiff GetModDiff(string uuidStr)
        {
            Guid uuid = new Guid(uuidStr);
            List<ModDiff> diffs = DataManager.Instance.Save.GetModDiffs();
            foreach (ModDiff diff in diffs)
            {
                if (diff.UUID == uuid)
                    return diff;
            }
            return new ModDiff("", uuid, null, null);
        }

        //===================================
        // Current Map
        //===================================
        public GroundMap GetCurrentGround()
        {
            return ZoneManager.Instance.CurrentGround;
        }

        public Map GetCurrentFloor()
        {
            return ZoneManager.Instance.CurrentMap;
        }

        public Zone GetCurrentDungeon()
        {
            return ZoneManager.Instance.CurrentZone;
        }


        //===================================
        // Enter/leave methods
        //===================================

        /// <summary>
        /// Leave current map, and enter specified dungeon
        /// </summary>
        /// <param name="mapname"></param>
        /// <param name="entrypoint"></param>
        public void EnterGroundMap(int id, int idxentrypoint, bool preserveMusic = false)
        {
            //Leave current map and enter specific groundmap at the specified entry point
            GameManager.Instance.SceneOutcome = GameManager.Instance.MoveToZone(new ZoneLoc(ZoneManager.Instance.CurrentZoneID, new SegLoc(ZoneManager.Instance.CurrentMapID.Segment, id), idxentrypoint), false, preserveMusic);
        }

        public void EnterGroundMap(string name, string entrypoint, bool preserveMusic = false)
        {
            GameManager.Instance.SceneOutcome = GameManager.Instance.MoveToGround(ZoneManager.Instance.CurrentZoneID, name, entrypoint, preserveMusic);
        }

        public void EnterGroundMap(string zone, string name, string entrypoint, bool preserveMusic = false)
        {
            GameManager.Instance.SceneOutcome = GameManager.Instance.MoveToGround(zone, name, entrypoint, preserveMusic);
        }


        public LuaFunction EnterDungeon;
        public Coroutine _EnterDungeon(string dungeonid, int structureid, int mapid, int entry, GameProgress.DungeonStakes stakes, bool recorded, bool silentRestrict)
        {
            return new Coroutine(GameManager.Instance.BeginGameInSegment(new ZoneLoc(dungeonid, new SegLoc(structureid, mapid), entry), stakes, recorded, silentRestrict));
        }

        public LuaFunction EnterRescue;
        public Coroutine _EnterRescue(string sosPath)
        {
            return new Coroutine(GameManager.Instance.BeginRescue(sosPath));
        }
        public void AddAOKRemark(int remarkIndex)
        {
            AOKMail aok = null;
            if (DataManager.Instance.Save.GeneratedAOK != null)
                aok = DataManager.LoadRescueMail(PathMod.NoMod(DataManager.RESCUE_OUT_PATH + DataManager.AOK_FOLDER + DataManager.Instance.Save.GeneratedAOK)) as AOKMail;
            if (aok != null)
            {
                aok.FinalStatement = remarkIndex;
                DataManager.SaveRescueMail(DataManager.Instance.Save.GeneratedAOK, aok);
            }
        }

        public LuaFunction ContinueDungeon;
        public Coroutine _ContinueDungeon(string dungeonid, int structureid, int mapid, int entry)
        {
            return new Coroutine(GameManager.Instance.BeginSegment(new ZoneLoc(dungeonid, new SegLoc(structureid, mapid), entry), false));
        }


        public LuaFunction EndDungeonRun;
        public Coroutine _EndDungeonRun(GameProgress.ResultType result, string destzoneid, int structureid, int mapid, int entryid, bool display, bool fanfare)
        {
            return new Coroutine(DataManager.Instance.Save.EndGame(result, new ZoneLoc(destzoneid, new SegLoc(structureid, mapid), entryid), display, fanfare));
        }

        /// <summary>
        /// Leave current map and load up the title screen
        /// </summary>
        public void RestartToTitle()
        {
            GameManager.Instance.SceneOutcome = GameManager.Instance.RestartToTitle();
        }


        /// <summary>
        ///
        /// </summary>
        /// <param name="zone"></param>
        /// <param name="structure"></param>
        /// <param name="id"></param>
        public void EnterZone(string zone, int structure, int id, int entry)
        {
            GameManager.Instance.SceneOutcome = GameManager.Instance.MoveToZone(new ZoneLoc(zone, new SegLoc(structure, id), entry));
        }


        /// <summary>
        /// Fade the screen
        /// </summary>
        /// <returns></returns>
        public LuaFunction FadeOut;
        public Coroutine _FadeOut(bool white, int duration)
        {
            return new Coroutine(GameManager.Instance.FadeOut(white, duration));
        }

        /// <summary>
        /// Fade the screen
        /// </summary>
        /// <returns></returns>
        public LuaFunction FadeIn;
        public Coroutine _FadeIn(int duration)
        {
            return new Coroutine(GameManager.Instance.FadeIn(duration));
        }


        /// <summary>
        /// Centers the camera on a position.
        /// </summary>
        public LuaFunction MoveCamera;
        public Coroutine _MoveCamera(int x, int y, int duration, bool toPlayer = false)
        {
            return new Coroutine(GroundScene.Instance.MoveCamera(new Loc(x, y), duration, toPlayer));
        }

        public Loc GetCameraCenter()
        {
            return GroundScene.Instance.GetFocusedLoc();
        }

        public bool IsCameraOnChar()
        {
            return !ZoneManager.Instance.CurrentGround.ViewCenter.HasValue;
        }

        //===================================
        // Mail
        //===================================
        public bool HasSOSMail()
        {
            string parentPath = PathMod.NoMod(DataManager.RESCUE_IN_PATH + DataManager.SOS_FOLDER);
            string[] files = System.IO.Directory.GetFiles(parentPath, "*" + DataManager.SOS_EXTENSION);
            return files.Length > 0;
        }
        public bool HasAOKMail()
        {
            string parentPath = PathMod.NoMod(DataManager.RESCUE_OUT_PATH + DataManager.AOK_FOLDER);
            string[] files = System.IO.Directory.GetFiles(parentPath, "*" + DataManager.AOK_EXTENSION);
            return files.Length > 0;
        }



        //===================================
        // Team Access
        //===================================
        /// <summary>
        /// Returns the index of the currently player controlled entity in the party.
        /// </summary>
        /// <returns>Index of the currently player controlled entity in the party.</returns>
        public int GetTeamLeaderIndex()
        {
            return DataManager.Instance.Save.ActiveTeam.LeaderIndex;
        }
        public void SetTeamLeaderIndex(int idx)
        {
            //make leader
            int oldIdx = DataManager.Instance.Save.ActiveTeam.LeaderIndex;
            DataManager.Instance.Save.ActiveTeam.LeaderIndex = idx;

            //update team
            if (GameManager.Instance.CurrentScene == GroundScene.Instance)
            {
                ZoneManager.Instance.CurrentGround.SetPlayerChar(new GroundChar(DataManager.Instance.Save.ActiveTeam.Leader,
                    ZoneManager.Instance.CurrentGround.ActiveChar.MapLoc,
                    ZoneManager.Instance.CurrentGround.ActiveChar.CharDir, "PLAYER"));
            }
            if (GameManager.Instance.CurrentScene == DungeonScene.Instance)
            {
                ZoneManager.Instance.CurrentMap.CurrentTurnMap.AdjustLeaderSwap(Faction.Player, 0, false, oldIdx, idx);
                DungeonScene.Instance.ReloadFocusedPlayer();
            }
        }

        public void SetCanSwitch(bool canSwitch)
        {
            DataManager.Instance.Save.NoSwitching = !canSwitch;
        }

        /// <summary>
        /// Returns the player party count
        /// </summary>
        /// <returns></returns>
        public int GetPlayerPartyCount()
        {
            return DataManager.Instance.Save.ActiveTeam.Players.Count;
        }

        /// <summary>
        /// Return the party as a LuaTable
        /// </summary>
        /// <returns></returns>
        public LuaTable GetPlayerPartyTable()
        {
            LuaTable tbl = LuaEngine.Instance.RunString("return {}").First() as LuaTable;
            LuaFunction addfn = LuaEngine.Instance.RunString("return function(tbl, chara) table.insert(tbl, chara) end").First() as LuaFunction;
            foreach (Character ent in DataManager.Instance.Save.ActiveTeam.Players)
                addfn.Call(tbl, ent);
            return tbl;
        }
        public Character GetPlayerPartyMember(int index)
        {
            return DataManager.Instance.Save.ActiveTeam.Players[index];
        }


        public int GetPlayerGuestCount()
        {
            return DataManager.Instance.Save.ActiveTeam.Guests.Count;
        }

        /// <summary>
        /// Return the guests as a LuaTable
        /// </summary>
        /// <returns></returns>
        public LuaTable GetPlayerGuestTable()
        {
            LuaTable tbl = LuaEngine.Instance.RunString("return {}").First() as LuaTable;
            LuaFunction addfn = LuaEngine.Instance.RunString("return function(tbl, chara) table.insert(tbl, chara) end").First() as LuaFunction;
            foreach (var ent in DataManager.Instance.Save.ActiveTeam.Guests)
                addfn.Call(tbl, ent);
            return tbl;
        }
        public Character GetPlayerGuestMember(int index)
        {
            return DataManager.Instance.Save.ActiveTeam.Guests[index];
        }

        public object GetPlayerAssemblyCount()
        {
            return DataManager.Instance.Save.ActiveTeam.Assembly.Count;
        }

        public LuaTable GetPlayerAssemblyTable()
        {
            LuaTable tbl = LuaEngine.Instance.RunString("return {}").First() as LuaTable;
            LuaFunction addfn = LuaEngine.Instance.RunString("return function(tbl, chara) table.insert(tbl, chara) end").First() as LuaFunction;
            foreach (var ent in DataManager.Instance.Save.ActiveTeam.Assembly)
                addfn.Call(tbl, ent);
            return tbl;
        }

        public Character GetPlayerAssemblyMember(int index)
        {
            return DataManager.Instance.Save.ActiveTeam.Assembly[index];
        }



        public void AddPlayerTeam(Character character)
        {
            if (GameManager.Instance.CurrentScene == DungeonScene.Instance)
            {
                DungeonScene.Instance.AddCharToTeam(Faction.Player, 0, false, character);
                character.RefreshTraits();
                character.Tactic.Initialize(character);
            }
            else
            {
                DataManager.Instance.Save.ActiveTeam.Players.Add(character);
            }
        }

        /// <summary>
        /// Removes the character from the team, placing its item back in the inventory.
        /// </summary>
        /// <param name="slot"></param>
        public void RemovePlayerTeam(int slot)
        {
            Character player = DataManager.Instance.Save.ActiveTeam.Players[slot];

            if (GameManager.Instance.CurrentScene == GroundScene.Instance)
            {
                if (!String.IsNullOrEmpty(player.EquippedItem.ID))
                {
                    InvItem heldItem = player.EquippedItem;
                    player.DequipItem();
                    DataManager.Instance.Save.ActiveTeam.AddToInv(heldItem);
                }

                GroundScene.Instance.RemoveChar(slot);
            }
            else if (GameManager.Instance.CurrentScene == DungeonScene.Instance)
            {
                if (!String.IsNullOrEmpty(player.EquippedItem.ID))
                {
                    InvItem heldItem = player.EquippedItem;
                    player.DequipItem();
                    if (DataManager.Instance.Save.ActiveTeam.GetInvCount() + 1 < DataManager.Instance.Save.ActiveTeam.GetMaxInvSlots(ZoneManager.Instance.CurrentZone))
                        DataManager.Instance.Save.ActiveTeam.AddToInv(heldItem);
                }

                DungeonScene.Instance.RemoveChar(new CharIndex(Faction.Player, 0, false, slot));
            }
            else
            {
                if (!String.IsNullOrEmpty(player.EquippedItem.ID))
                {
                    InvItem heldItem = player.EquippedItem;
                    player.DequipItem();
                    DataManager.Instance.Save.ActiveTeam.AddToInv(heldItem);
                }

                ExplorerTeam team = DataManager.Instance.Save.ActiveTeam;

                team.Players.RemoveAt(slot);

                //update leader
                if (slot < team.LeaderIndex)
                    team.LeaderIndex--;
            }
        }

        public void AddPlayerGuest(Character character)
        {
            if (GameManager.Instance.CurrentScene == DungeonScene.Instance)
            {
                DungeonScene.Instance.AddCharToTeam(Faction.Player, 0, true, character);
                character.RefreshTraits();
                character.Tactic.Initialize(character);
            }
            else
            {
                DataManager.Instance.Save.ActiveTeam.Guests.Add(character);
            }
        }

        /// <summary>
        /// Removes the character from the team, placing its item back in the inventory.
        /// </summary>
        /// <param name="slot"></param>
        public void RemovePlayerGuest(int slot)
        {
            Character player = DataManager.Instance.Save.ActiveTeam.Guests[slot];

            if (!String.IsNullOrEmpty(player.EquippedItem.ID))
                player.DequipItem();

            if (GameManager.Instance.CurrentScene == DungeonScene.Instance)
            {
                DungeonScene.Instance.RemoveChar(new CharIndex(Faction.Player, 0, true, slot));
            }
            else
            {
                DataManager.Instance.Save.ActiveTeam.Guests.RemoveAt(slot);
            }
        }


        public void AddPlayerAssembly(Character character)
        {
            DataManager.Instance.Save.ActiveTeam.AddToSortedAssembly(character);
        }

        public void RemovePlayerAssembly(int slot)
        {
            DataManager.Instance.Save.ActiveTeam.Assembly.RemoveAt(slot);
        }

        public void SetCharacterNickname(Character character, string nickname)
        {
            character.Nickname = nickname;
        }

        public string GetCharacterNickname(Character character)
        {
            return character.Nickname;
        }


        public void SetTeamName(string nickname)
        {
            DataManager.Instance.Save.ActiveTeam.Name = nickname;
            foreach (Character chara in DataManager.Instance.Save.ActiveTeam.Players)
                chara.OriginalTeam = DataManager.Instance.Save.ActiveTeam.Name;
            foreach(Character chara in DataManager.Instance.Save.ActiveTeam.Assembly)
                chara.OriginalTeam = DataManager.Instance.Save.ActiveTeam.Name;
        }

        public string GetTeamName()
        {
            return DataManager.Instance.Save.ActiveTeam.GetDisplayName();
        }

        /// <summary>
        /// Checks if the character can relearn any skills.
        /// </summary>
        /// <param name="character"></param>
        public bool CanRelearn(Character character)
        {
            return character.GetRelearnableSkills().Count > 0;
        }

        public bool CanForget(Character character)
        {
            foreach (SlotSkill skill in character.BaseSkills)
            {
                if (!String.IsNullOrEmpty(skill.SkillNum))
                    return true;
            }
            return false;
        }

        public bool CanLearn(Character character)
        {
            foreach (SlotSkill skill in character.BaseSkills)
            {
                if (String.IsNullOrEmpty(skill.SkillNum))
                    return true;
            }
            return false;
        }


        public LuaFunction CheckLevelSkills;
        public Coroutine _CheckLevelSkills(Character chara, int oldLevel)
        {
            return new Coroutine(__CheckLevelSkills(chara, oldLevel));
        }

        private IEnumerator<YieldInstruction> __CheckLevelSkills(Character chara, int oldLevel)
        {
            DungeonScene.GetLevelSkills(chara, oldLevel);

            foreach (string skill in DungeonScene.GetLevelSkills(chara, oldLevel))
            {
                int learn = -1;
                if (DataManager.Instance.CurrentReplay != null)
                    learn = DataManager.Instance.CurrentReplay.ReadUI();
                else
                {
                    yield return CoroutineManager.Instance.StartCoroutine(DungeonScene.TryLearnSkill(chara, skill, (int slot) => { learn = slot; }, () => { }));
                    DataManager.Instance.LogUIPlay(learn);
                }
                if (learn > -1)
                    yield return CoroutineManager.Instance.StartCoroutine(DungeonScene.LearnSkillWithFanfare(chara, skill, learn));
            }
        }


        public LuaFunction TryLearnSkill;
        public Coroutine _TryLearnSkill(Character chara, string skill)
        {
            return new Coroutine(__TryLearnSkill(chara, skill));
        }

        private IEnumerator<YieldInstruction> __TryLearnSkill(Character chara, string skill)
        {
            int learn = -1;
            if (DataManager.Instance.CurrentReplay != null)
                learn = DataManager.Instance.CurrentReplay.ReadUI();
            else
            {
                yield return CoroutineManager.Instance.StartCoroutine(DungeonScene.TryLearnSkill(chara, skill, (int slot) => { learn = slot; }, () => { }));
                DataManager.Instance.LogUIPlay(learn);
            }
            if (learn > -1)
                yield return CoroutineManager.Instance.StartCoroutine(DungeonScene.LearnSkillWithFanfare(chara, skill, learn));
        }

        public void LearnSkill(Character chara, string skillNum)
        {
            chara.LearnSkill(skillNum, true);
        }

        public void ForgetSkill(Character chara, int slot)
        {
            chara.DeleteSkill(slot);
        }

        public void SetCharacterSkill(Character character, string skillId, int slot)
        {
            character.ReplaceSkill(skillId, slot, true);
        }


        public string GetCharacterSkill(Character chara, int slot)
        {
            return chara.BaseSkills[slot].SkillNum;
        }



        public bool CanPromote(Character character)
        {
            MonsterData entry = DataManager.Instance.GetMonster(character.BaseForm.Species);
            for (int ii = 0; ii < entry.Promotions.Count; ii++)
            {
                if (!DataManager.Instance.DataIndices[DataManager.DataType.Monster].Entries[entry.Promotions[ii].Result.ToString()].Released)
                    continue;

                bool hardReq = false;
                foreach (PromoteDetail detail in entry.Promotions[ii].Details)
                {
                    if (detail.IsHardReq() && !detail.GetReq(character))
                    {
                        hardReq = true;
                        break;
                    }
                }
                if (!hardReq)
                    return true;
            }
            return false;
        }

        public LuaTable GetAvailablePromotions(Character character, string bypassItem)
        {
            MonsterData entry = DataManager.Instance.GetMonster(character.BaseForm.Species);
            bool bypass = character.EquippedItem.ID == bypassItem;

            LuaTable tbl = LuaEngine.Instance.RunString("return {}").First() as LuaTable;
            LuaFunction addfn = LuaEngine.Instance.RunString("return function(tbl, chara) table.insert(tbl, chara) end").First() as LuaFunction;

            for (int ii = 0; ii < entry.Promotions.Count; ii++)
            {
                if (!DataManager.Instance.DataIndices[DataManager.DataType.Monster].Entries[entry.Promotions[ii].Result.ToString()].Released)
                    continue;
                if (entry.Promotions[ii].IsQualified(character, false))
                    addfn.Call(tbl, entry.Promotions[ii]);
                else if (bypass)
                {
                    bool hardReq = false;
                    foreach (PromoteDetail detail in entry.Promotions[ii].Details)
                    {
                        if (detail.IsHardReq() && !detail.GetReq(character))
                        {
                            hardReq = true;
                            break;
                        }
                    }
                    if (!hardReq)
                        addfn.Call(tbl, entry.Promotions[ii]);
                }
            }

            return tbl;
        }

        public void PromoteCharacter(Character character, PromoteBranch branch, string bypassItem)
        {
            MonsterData entry = DataManager.Instance.GetMonster(branch.Result);
            //exception item bypass
            bool bypass = character.EquippedItem.ID == bypassItem;
            MonsterID newData = character.BaseForm;
            newData.Species = branch.Result;
            if (newData.Form >= entry.Forms.Count)
                newData.Form = 0;
            character.Promote(newData);
            character.FullRestore();
            branch.OnPromote(character, false, bypass);
            //remove exception item if there is one...
            if (bypass)
                character.DequipItem();
            DataManager.Instance.Save.RegisterMonster(character.BaseForm.Species);
            DataManager.Instance.Save.RogueUnlockMonster(character.BaseForm.Species);
        }

        //===================================
        // Inventory
        //===================================
        public object FindPlayerItem(string id, bool held, bool inv)
        {
            if (held)
            {
                for (int ii = 0; ii < DataManager.Instance.Save.ActiveTeam.Players.Count; ii++)
                {
                    Character activeChar = DataManager.Instance.Save.ActiveTeam.Players[ii];
                    if (activeChar.EquippedItem.ID == id)
                        return new InvSlot(true, ii);
                }
            }

            if (inv)
            {
                for (int ii = 0; ii < DataManager.Instance.Save.ActiveTeam.GetInvCount(); ii++)
                {
                    if (DataManager.Instance.Save.ActiveTeam.GetInv(ii).ID == id)
                        return new InvSlot(false, ii);
                }
            }

            return InvSlot.Invalid;
        }

        public int GetPlayerEquippedCount()
        {
            int nbitems = 0;
            foreach (Character player in DataManager.Instance.Save.ActiveTeam.Players)
            {
                if (!String.IsNullOrEmpty(player.EquippedItem.ID))
                    nbitems++;
            }

            return nbitems;
        }

        public int GetPlayerBagCount()
        {
            return DataManager.Instance.Save.ActiveTeam.GetInvCount();
        }

        public int GetPlayerBagLimit()
        {
            return DataManager.Instance.Save.ActiveTeam.MaxInv;
        }

        public object GetPlayerEquippedItem(int slot)
        {
            return DataManager.Instance.Save.ActiveTeam.Players[slot].EquippedItem;
        }

        public object GetGuestEquippedItem(int slot)
        {
            return DataManager.Instance.Save.ActiveTeam.Guests[slot].EquippedItem;
        }

        public void GivePlayerItem(InvItem item)
        {
            DataManager.Instance.Save.ActiveTeam.AddToInv(item);
        }

        public void GivePlayerItem(string id, int count = 1, bool cursed = false, string hiddenval = "")
        {
            InvItem item = new InvItem(id, cursed, count);
            item.HiddenValue = hiddenval;
            DataManager.Instance.Save.ActiveTeam.AddToInv(item);
        }


        public object GetPlayerBagItem(int slot)
        {
            return DataManager.Instance.Save.ActiveTeam.GetInv(slot);
        }

        /// <summary>
        /// Remove an item from player inventory
        /// </summary>
        public void TakePlayerBagItem(int slot)
        {
            DataManager.Instance.Save.ActiveTeam.RemoveFromInv(slot);
        }
        public void TakePlayerEquippedItem(int slot)
        {
            DataManager.Instance.Save.ActiveTeam.Players[slot].DequipItem();
        }
        public void TakeGuestEquippedItem(int slot)
        {
            DataManager.Instance.Save.ActiveTeam.Guests[slot].DequipItem();
        }

        public int GetPlayerStorageCount()
        {
            int count = DataManager.Instance.Save.ActiveTeam.BoxStorage.Count;
            foreach (string nb in DataManager.Instance.Save.ActiveTeam.Storage.Keys)
                count += DataManager.Instance.Save.ActiveTeam.Storage[nb];
            return count;
        }

        public int GetPlayerStorageItemCount(string id)
        {
            return DataManager.Instance.Save.ActiveTeam.Storage[id];
        }

        public void GivePlayerStorageItem(InvItem item)
        {
            DataManager.Instance.Save.ActiveTeam.StoreItems(new List<InvItem> { item });
        }

        public void GivePlayerStorageItem(string id, int count = 1, bool cursed = false, string hiddenval = "")
        {
            for (int ii = 0; ii < count; ii++)
            {
                InvItem item = new InvItem(id, cursed);
                item.HiddenValue = hiddenval;
                DataManager.Instance.Save.ActiveTeam.StoreItems(new List<InvItem> { item });
            }
        }
        public void TakePlayerStorageItem(string id)
        {
            DataManager.Instance.Save.ActiveTeam.TakeItems(new List<WithdrawSlot> { new WithdrawSlot(false, id, 0) });
        }

        //===================================
        // Money
        //===================================
        public int GetPlayerMoneyBank()
        {
            return DataManager.Instance.Save.ActiveTeam.Bank;
        }

        public void AddToPlayerMoneyBank(int toadd)
        {
            DataManager.Instance.Save.ActiveTeam.Bank += toadd;
        }

        public void RemoveFromPlayerMoneyBank(int toremove)
        {
            DataManager.Instance.Save.ActiveTeam.Bank -= toremove;
        }

        public int GetPlayerMoney()
        {
            return DataManager.Instance.Save.ActiveTeam.Money;
        }

        public void AddToPlayerMoney(int toadd)
        {
            DataManager.Instance.Save.ActiveTeam.AddMoney(null, toadd);
        }

        public void RemoveFromPlayerMoney(int toremove)
        {
            DataManager.Instance.Save.ActiveTeam.LoseMoney(null, toremove);
        }

        //===================================
        // Input
        //===================================
        public bool IsKeyDown(int keyid)
        {
            Microsoft.Xna.Framework.Input.Keys curkey = (Microsoft.Xna.Framework.Input.Keys)keyid;
            return GameManager.Instance.MetaInputManager.BaseKeyDown(curkey);
        }

        public void CutsceneMode(bool bon)
        {
            Content.GraphicsManager.GlobalIdle = bon ? 0 : Content.GraphicsManager.IdleAction;
            DataManager.Instance.Save.CutsceneMode = bon;
        }



        //
        // GameProgress
        //

        public ulong GetDailySeed()
        {
            return DataManager.Instance.Save.Rand.FirstSeed;
        }

        public void UnlockDungeon(string dungeonid)
        {
            DataManager.Instance.Save.UnlockDungeon(dungeonid);
        }

        public bool DungeonUnlocked(string dungeonid)
        {
            return DataManager.Instance.Save.GetDungeonUnlock(dungeonid) != GameProgress.UnlockState.None;
        }

        public bool InRogueMode()
        {
            return DataManager.Instance.Save is RogueProgress;
        }

        public bool HasServerSet()
        {
            return DiagManager.Instance.CurSettings.ServerList.Count > 0;
        }

        public bool GetRescueAllowed()
        {
            return DataManager.Instance.Save.AllowRescue;
        }

        public void SetRescueAllowed(bool allowed)
        {
            DataManager.Instance.Save.AllowRescue = allowed;
        }

        public void QueueLeaderEvent(object obj)
        {
            IEnumerator<YieldInstruction> yields = null;
            if (obj is Coroutine)
            {
                Coroutine coro = obj as Coroutine;
                yields = CoroutineManager.Instance.YieldCoroutine(coro);
            }
            else if (obj is LuaFunction)
            {
                LuaFunction luaFun = obj as LuaFunction;
                yields = LuaEngine.Instance.CallScriptFunction(luaFun);
            }

            if (GameManager.Instance.CurrentScene == GroundScene.Instance)
            {
                GroundScene.Instance.PendingLeaderAction = yields;
            }
            if (GameManager.Instance.CurrentScene == DungeonScene.Instance)
            {
                DungeonScene.Instance.PendingLeaderAction = yields;
            }
        }

        //===================================
        // Utils
        //===================================
        /// <summary>
        ///
        /// </summary>
        /// <param name="frames"></param>
        /// <returns></returns>
        public LuaFunction WaitFrames;
        public YieldInstruction _WaitFrames(int frames)
        {
            return new WaitForFrames(frames);
        }

        /// <summary>
        /// Turns a vector (preferably a unit vector) to a direction.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public Dir8 VectorToDirection(Loc v)
        {
            return VectorToDirection(v.X, v.Y);
        }

        /// <summary>
        /// Convenience function to get a vector's components from lua numbers(doubles)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public Dir8 VectorToDirection(double X, double Y)
        {
            try
            {
                if( X < 0 )
                {
                    if (Y > 0)
                        return Dir8.UpRight;
                    else if (Y < 0)
                        return Dir8.DownRight;
                    else
                        return Dir8.Right;
                }
                else if( X > 0)
                {
                    if (Y > 0)
                        return Dir8.UpLeft;
                    else if (Y < 0)
                        return Dir8.DownLeft;
                    else
                        return Dir8.Left;
                }
                else
                {
                    if (Y > 0)
                        return Dir8.Up;
                    else if (Y < 0)
                        return Dir8.Down;
                    else
                        return Dir8.None; //psy: If both X and Y are 0, well not much else fits ^^;
                }
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex, DiagManager.Instance.DevMode);
                return Dir8.None;
            }
        }

        /// <summary>
        /// Generates a random direction.
        /// </summary>
        /// <returns></returns>
        public Dir8 RandomDirection()
        {
            try
            {
                var rng = new Random();
                return (Dir8)rng.Next(0, 7);
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex, DiagManager.Instance.DevMode);
                return Dir8.None;
            }
        }

        /// <summary>
        /// Setups any extra functionalities for this object written on the lua side.
        /// </summary>
        public override void SetupLuaFunctions(LuaEngine state)
        {
            GroundSave = state.RunString("return function(_) return coroutine.yield(GAME:_GroundSave()) end").First() as LuaFunction;
            CheckLevelSkills = state.RunString("return function(_,chara, oldLevel) return coroutine.yield(GAME:_CheckLevelSkills(chara, oldLevel)) end").First() as LuaFunction;
            TryLearnSkill = state.RunString("return function(_,chara, skill) return coroutine.yield(GAME:_TryLearnSkill(chara, skill)) end").First() as LuaFunction;
            EnterRescue = state.RunString("return function(_, sosPath) return coroutine.yield(GAME:_EnterRescue(sosPath)) end").First() as LuaFunction;
            EnterDungeon = state.RunString("return function(_, dungeonid, structureid, mapid, entryid, stakes, recorded, silentRestrict) return coroutine.yield(GAME:_EnterDungeon(dungeonid, structureid, mapid, entryid, stakes, recorded, silentRestrict)) end").First() as LuaFunction;
            ContinueDungeon = state.RunString("return function(_, dungeonid, structureid, mapid, entryid) return coroutine.yield(GAME:_ContinueDungeon(dungeonid, structureid, mapid, entryid)) end").First() as LuaFunction;
            EndDungeonRun = state.RunString("return function(_, result, destzoneid, structureid, mapid, entryid, display, fanfare) return coroutine.yield(GAME:_EndDungeonRun(result, destzoneid, structureid, mapid, entryid, display, fanfare)) end").First() as LuaFunction;
            FadeOut = state.RunString("return function(_, bwhite, duration) return coroutine.yield(GAME:_FadeOut(bwhite, duration)) end").First() as LuaFunction;
            FadeIn = state.RunString("return function(_, duration) return coroutine.yield(GAME:_FadeIn(duration)) end").First() as LuaFunction;
            MoveCamera = state.RunString("return function(_, x, y, duration, toPlayer) return coroutine.yield(GAME:_MoveCamera(x, y, duration, toPlayer)) end").First() as LuaFunction;
            WaitFrames      = state.RunString("return function(_, frames) return coroutine.yield(GAME:_WaitFrames(frames)) end").First() as LuaFunction;
        }


    }
}
