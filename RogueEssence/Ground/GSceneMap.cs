using System;
using System.Collections.Generic;
using RogueElements;
using RogueEssence.Data;
using RogueEssence.Menu;
using RogueEssence.Dungeon;
using RogueEssence.Content;
using RogueEssence.Script;

namespace RogueEssence.Ground
{
    public partial class GroundScene
    {

        private void ProcessDir(Dir8 dir, GroundChar character)
        {
            if (dir > Dir8.None)
                character.CharDir = dir;
        }


        //public IEnumerator<YieldInstruction> ArriveOnTile(Character character)

        //public IEnumerator<YieldInstruction> ArriveOnTile(Character character, bool checkItem, bool wantItem, bool noTrap)



        private IEnumerator<YieldInstruction> ProcessUseItem(GroundChar character, int invSlot, int teamSlot, int commandIdx)
        {
            InvItem invItem = null;
            if (invSlot < 0)
            {
                Character activeChar = DataManager.Instance.Save.ActiveTeam.Leader;
                invItem = activeChar.EquippedItem;
            }
            else
                invItem = DataManager.Instance.Save.ActiveTeam.GetInv(invSlot);
            Character target = teamSlot == -1 ? DataManager.Instance.Save.ActiveTeam.Leader : DataManager.Instance.Save.ActiveTeam.Players[teamSlot];

            ItemData itemEntry = (ItemData)invItem.GetData();
            if (commandIdx > -1)
            {
                GroundItemEvent groundEvent = itemEntry.GroundUseActions[commandIdx];
                GroundContext context = new GroundContext(invItem, character, target);
                yield return CoroutineManager.Instance.StartCoroutine(groundEvent.Apply(context));
                if (context.CancelState.Cancel) yield break;
            }
            
 
            if (itemEntry.MaxStack > 1 && invItem.Amount > 1)
                invItem.Amount--;
            else if (itemEntry.MaxStack < 0)
            {
                //reusable, -1 do nothing.
            }
            else
            {
                if (invSlot < 0)
                {
                    Character activeChar = DataManager.Instance.Save.ActiveTeam.Leader;
                    activeChar.SilentDequipItem();
                } else 
                    DataManager.Instance.Save.ActiveTeam.RemoveFromInv(invSlot);
            }
        }

        private IEnumerator<YieldInstruction> ProcessTrashItem(GroundChar character, int invSlot, bool held)
        {
            InvItem invItem = null;
            if (held)
            {
                Character activeChar = DataManager.Instance.Save.ActiveTeam.Players[invSlot];

                //no curse check in ground mode

                invItem = activeChar.EquippedItem;
                activeChar.SilentDequipItem();
            }
            else
            {
                ExplorerTeam memberTeam = DataManager.Instance.Save.ActiveTeam;
                invItem = memberTeam.GetInv(invSlot);
                memberTeam.RemoveFromInv(invSlot);
            }

            yield return CoroutineManager.Instance.StartCoroutine(MenuManager.Instance.SetDialogue(false, String.Format("Threw away the {0}.", invItem.GetDisplayName())));
        }

        public IEnumerator<YieldInstruction> ProcessObjectInteract(GroundChar character)
        {
            //check to see if we're colliding with anyone
            TriggerResult result = new TriggerResult();
            if (character == FocusedCharacter)
            {
                //Loc front = character.GetFront() + character.CharDir.GetLoc();
                Loc start = character.MapLoc + character.CharDir.GetLoc();
                foreach (AABB.IObstacle obstacle in ZoneManager.Instance.CurrentGround.Find(new Rect(start, character.Bounds.Size)))
                {
                    if (obstacle == FocusedCharacter)
                    {
                        //do nothing
                    }
                    else if (obstacle is GroundChar)
                    {
                        GroundChar talkTo = (GroundChar)obstacle;
                        if (talkTo.EntEnabled)
                        {
                            character.CurrentCommand = new GameAction(GameAction.ActionType.None, Dir8.None);
                            yield return CoroutineManager.Instance.StartCoroutine(talkTo.Interact(character, result));
                            if (result.Success)
                                yield break;
                        }
                    }
                    else if (obstacle is GroundObject)
                    {
                        GroundObject groundObj = (GroundObject)obstacle;
                        if (groundObj.EntEnabled && groundObj.GetTriggerType() == GroundObject.EEntityTriggerTypes.Action)
                        {
                            character.CurrentCommand = new GameAction(GameAction.ActionType.None, Dir8.None);
                            yield return CoroutineManager.Instance.StartCoroutine(groundObj.Interact(character, result));
                            if (result.Success)
                                yield break;
                        }
                    }
                }
            }

        }

        public IEnumerator<YieldInstruction> ProcessGiveItem(GroundChar character, int invSlot, int teamSlot)
        {
            ExplorerTeam memberTeam = DataManager.Instance.Save.ActiveTeam;
            Character itemChar = memberTeam.Leader;
            if (teamSlot > -1)
                itemChar = memberTeam.Players[teamSlot];

            //no curse check in ground mode

            InvItem item = memberTeam.GetInv(invSlot);
            memberTeam.RemoveFromInv(invSlot);

            GameManager.Instance.SE(GraphicsManager.EquipSE);

            if (!String.IsNullOrEmpty(itemChar.EquippedItem.ID))
            {
                yield return CoroutineManager.Instance.StartCoroutine(MenuManager.Instance.SetDialogue(false, Text.FormatKey("MSG_ITEM_SWAP", itemChar.GetDisplayName(false), item.GetDisplayName(), itemChar.EquippedItem.GetDisplayName())));
                //put item in inv
                memberTeam.AddToInv(new InvItem(itemChar.EquippedItem));
            }
            else
                yield return CoroutineManager.Instance.StartCoroutine(MenuManager.Instance.SetDialogue(false, Text.FormatKey("MSG_ITEM_GIVE", itemChar.GetDisplayName(false), item.GetDisplayName())));


            itemChar.SilentEquipItem(item);
        }

        public IEnumerator<YieldInstruction> ProcessTakeItem(GroundChar character, int teamSlot)
        {
            ExplorerTeam memberTeam = DataManager.Instance.Save.ActiveTeam;
            Character itemChar = memberTeam.Players[teamSlot];

            //no curse check in ground mode

            InvItem item = itemChar.EquippedItem;
            memberTeam.AddToInv(item);
            yield return CoroutineManager.Instance.StartCoroutine(itemChar.DequipItem());
            GameManager.Instance.SE(GraphicsManager.EquipSE);
            yield return CoroutineManager.Instance.StartCoroutine(MenuManager.Instance.SetDialogue(false, Text.FormatKey("MSG_ITEM_DEQUIP", itemChar.GetDisplayName(false), item.GetDisplayName())));

        }

        //public bool CanCheckGround()

        //public ChoiceMenu GetGroundCheckMenu()

        //public IEnumerator<YieldInstruction> CheckEXP()

        public IEnumerator<YieldInstruction> HandoutEXP(Character character, int expToGain)
        {
            character.EXP += expToGain;
            yield return CoroutineManager.Instance.StartCoroutine(HandoutLevelUp());
        }

        public IEnumerator<YieldInstruction> LevelUpChar(Character character, int numLevels)
        {
            character.EXP = 0;
            string growth = DataManager.Instance.GetMonster(character.BaseForm.Species).EXPTable;
            GrowthData growthData = DataManager.Instance.GetGrowth(growth);
            if (numLevels < 0)
            {
                int levelsChanged = 0;
                while (levelsChanged > numLevels && character.Level + levelsChanged > 1)
                {
                    character.EXP -= growthData.GetExpToNext(character.Level + levelsChanged - 1);
                    levelsChanged--;
                }
            }
            else if (numLevels > 0)
            {
                int levelsChanged = 0;
                while (levelsChanged < numLevels && character.Level + levelsChanged < DataManager.Instance.Start.MaxLevel)
                {
                    character.EXP += growthData.GetExpToNext(character.Level + levelsChanged);
                    levelsChanged++;
                }
            }
            
            yield return CoroutineManager.Instance.StartCoroutine(HandoutLevelUp());
        }
        public IEnumerator<YieldInstruction> HandoutLevelUp()
        {
            for (int ii = 0; ii < DataManager.Instance.Save.ActiveTeam.Players.Count; ii++)
            {
                Character character = DataManager.Instance.Save.ActiveTeam.Players[ii];
                string growth = DataManager.Instance.GetMonster(character.BaseForm.Species).EXPTable;
                GrowthData growthData = DataManager.Instance.GetGrowth(growth);
                int oldLevel = character.Level;
                int oldHP = character.MaxHP;
                int oldSpeed = character.BaseSpeed;
                int oldAtk = character.BaseAtk;
                int oldDef = character.BaseDef;
                int oldMAtk = character.BaseMAtk;
                int oldMDef = character.BaseMDef;

                if (character.Level < DataManager.Instance.Start.MaxLevel &&
                    character.EXP >= growthData.GetExpToNext(character.Level))
                {
                    while (character.EXP >= growthData.GetExpToNext(character.Level))
                    {
                        character.EXP -= growthData.GetExpToNext(character.Level);
                        character.Level++;

                        if (character.Level >= DataManager.Instance.Start.MaxLevel)
                        {
                            character.EXP = 0;
                            break;
                        }
                    }


                    GameManager.Instance.Fanfare("Fanfare/LevelUp");
                    yield return CoroutineManager.Instance.StartCoroutine(
                        GameManager.Instance.LogSkippableMsg(
                            Text.FormatKey("DLG_LEVEL_UP", character.GetDisplayName(true), character.Level),
                            character.MemberTeam));

                    GameManager.Instance.SE("Menu/Confirm");
                    yield return CoroutineManager.Instance.StartCoroutine(
                        MenuManager.Instance.ProcessMenuCoroutine(new LevelUpMenu(character, oldLevel, oldHP, oldSpeed,
                            oldAtk, oldDef, oldMAtk, oldMDef)));

                    yield return CoroutineManager.Instance.StartCoroutine(CheckLevelSkills(character, oldLevel));
                }
                else if (character.EXP < 0)
                {
                    while (character.EXP < 0)
                    {
                        if (character.Level <= 1)
                        {
                            character.EXP = 0;
                            break;
                        }

                        character.EXP += growthData.GetExpToNext(character.Level - 1);
                        character.Level--;
                    }

                    //bound out max HP
                    character.HP = Math.Min(character.MaxHP, character.HP);

                    GameManager.Instance.Fanfare("Fanfare/LevelDown");
                    yield return CoroutineManager.Instance.StartCoroutine(
                        GameManager.Instance.LogSkippableMsg(
                            Text.FormatKey("DLG_LEVEL_DOWN", character.GetDisplayName(true), character.Level),
                            character.MemberTeam));
                }
            }
        }
        
        public IEnumerator<YieldInstruction> CheckLevelSkills(Character player, int oldLevel)
        {
            foreach (string skill in DungeonScene.GetLevelSkills(player, oldLevel))
            {
                int learn = -1;

                if (DataManager.Instance.CurrentReplay != null)
                    learn = DataManager.Instance.CurrentReplay.ReadUI();
                else
                {
                    yield return CoroutineManager.Instance.StartCoroutine(DungeonScene.TryLearnSkill(player, skill, (int slot) => { learn = slot; }, () => { }));
                    DataManager.Instance.LogUIPlay(learn);
                }
                if (learn > -1)
                    yield return CoroutineManager.Instance.StartCoroutine(DungeonScene.LearnSkillWithFanfare(player, skill, learn));
            }
        }



        //public IEnumerator<YieldInstruction> AskToSendHome()

        public IEnumerator<YieldInstruction> ChooseSendHome(int index)
        {
            yield return CoroutineManager.Instance.StartCoroutine(SendHome(index));
        }

        public IEnumerator<YieldInstruction> SendHome(int index)
        {
            yield return new WaitForFrames(30);
        }


        /// <summary>
        /// Sends a team member home without affecting the ground map.
        /// </summary>
        /// <param name="index">Index of team member.</param>
        /// <param name="front">Whether to put the team member in front or in back of the assembly list.</param>
        public void SilentSendHome(int index)
        {
            Character player = DataManager.Instance.Save.ActiveTeam.Players[index];

            if (!String.IsNullOrEmpty(player.EquippedItem.ID))
            {
                InvItem heldItem = player.EquippedItem;
                player.SilentDequipItem();
                DataManager.Instance.Save.ActiveTeam.AddToInv(heldItem);
            }

            RemoveChar(index);
            DataManager.Instance.Save.ActiveTeam.Assembly.Insert(0, player);

        }

        /// <summary>
        /// Adds a team member from the assembly to the team without affecting the ground map.
        /// </summary>
        /// <param name="index">Index of assembly member.</param>
        public void SilentAddToTeam(int index)
        {
            Character member = DataManager.Instance.Save.ActiveTeam.Assembly[index];
            DataManager.Instance.Save.ActiveTeam.Assembly.RemoveAt(index);

            DataManager.Instance.Save.ActiveTeam.Players.Add(member);
        }

        //public IEnumerator<YieldInstruction> DropItem(InvItem item, Loc loc)

        //public IEnumerator<YieldInstruction> DropItem(InvItem item, Loc loc, Loc start)

        //public IEnumerator<YieldInstruction> DropMoney(int amount, Loc loc, Loc start)

        //public IEnumerator<YieldInstruction> DropMapItem(MapItem item, Loc loc, Loc start)


        public void AddMapStatus(MapStatus status)
        {
            MapStatus statusToCheck;
            if (ZoneManager.Instance.CurrentGround.Status.TryGetValue(status.ID, out statusToCheck))
            {

            }
            else
            {
                ZoneManager.Instance.CurrentGround.Status.Add(status.ID, status);
                status.StartEmitter(Anims);
            }
        }

        public void RemoveMapStatus(string id)
        {
            MapStatus statusToRemove;
            if (ZoneManager.Instance.CurrentGround.Status.TryGetValue(id, out statusToRemove))
            {
                ZoneManager.Instance.CurrentGround.Status.Remove(statusToRemove.ID);
                statusToRemove.EndEmitter();
            }
        }

        //public IEnumerator<YieldInstruction> PointWarp(Character character, Loc loc, bool msg)

        //public IEnumerator<YieldInstruction> SyncActions(Character char1, CharAnimation anim1, Character char2, CharAnimation anim2)

        //public IEnumerator<YieldInstruction> RandomWarp(Character character, int radius, bool msg = true)

        //public IEnumerator<YieldInstruction> WarpNear(Character character, Loc loc, bool msg = true)

        //public IEnumerator<YieldInstruction> WarpNear(Character character, Loc loc, int diffRange, bool msg = true)

        //public IEnumerator<YieldInstruction> Pounce(Character character, Dir8 dir, Loc startLoc, int range)

        //public IEnumerator<YieldInstruction> KnockBack(Character character, Dir8 dir, int range)

        //public IEnumerator<YieldInstruction> JumpTo(Character character, Dir8 dir, int range)

        //public IEnumerator<YieldInstruction> ThrowTo(Character character, Character attacker, Dir8 dir, int range, Alignment targetAlignments)


        //public bool ShotBlocked(Character character, Loc loc, Dir8 dir, Alignment blockedAlignments, bool useMobility, bool blockedByWall)

        //public bool ShotBlocked(Character character, Loc loc, Dir8 dir, Alignment blockedAlignments, bool useMobility, bool blockedByWall, bool blockedByDiagonal)


        //public bool VisionBlocked(int x, int y)

        //public bool BlockedByCharacter(Character character, Loc loc, Alignment targetAlignments)
    }
}
