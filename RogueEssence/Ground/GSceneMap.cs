using System;
using System.Collections.Generic;
using RogueElements;
using RogueEssence.Data;
using RogueEssence.Menu;
using RogueEssence.Dungeon;

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



        private IEnumerator<YieldInstruction> ProcessTrashItem(GroundChar character, int invSlot, bool held)
        {
            InvItem invItem = null;
            if (held)
            {
                Character activeChar = DataManager.Instance.Save.ActiveTeam.Players[invSlot];

                //no curse check in ground mode

                invItem = activeChar.EquippedItem;
                activeChar.EquippedItem = new InvItem();
            }
            else
            {
                ExplorerTeam memberTeam = DataManager.Instance.Save.ActiveTeam;
                invItem = memberTeam.Inventory[invSlot];
                memberTeam.Inventory.RemoveAt(invSlot);
            }

            yield return CoroutineManager.Instance.StartCoroutine(MenuManager.Instance.SetDialogue(false, String.Format("Threw away the {0}.", invItem.GetName())));

        }

        public IEnumerator<YieldInstruction> ProcessObjectInteract(GroundChar character)
        {
            //check to see if we're colliding with anyone

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
                            yield return CoroutineManager.Instance.StartCoroutine(talkTo.Interact(character));
                            yield break;
                        }
                    }
                    else if (obstacle is GroundObject)
                    {
                        GroundObject groundObj = (GroundObject)obstacle;
                        if (groundObj.EntEnabled && groundObj.GetTriggerType() == GroundObject.EEntityTriggerTypes.Action)
                        {
                            character.CurrentCommand = new GameAction(GameAction.ActionType.None, Dir8.None);
                            yield return CoroutineManager.Instance.StartCoroutine(groundObj.Interact(character));
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

            InvItem item = memberTeam.Inventory[invSlot];
            memberTeam.Inventory.RemoveAt(invSlot);

            GameManager.Instance.SE(DataManager.Instance.EquipSE);

            if (itemChar.EquippedItem.ID > -1)
            {
                yield return CoroutineManager.Instance.StartCoroutine(MenuManager.Instance.SetDialogue(false, Text.FormatKey("MSG_ITEM_SWAP", itemChar.Name, item.GetName(), itemChar.EquippedItem.GetName())));
                //put item in inv
                memberTeam.Inventory.Add(new InvItem(itemChar.EquippedItem));
            }
            else
                yield return CoroutineManager.Instance.StartCoroutine(MenuManager.Instance.SetDialogue(false, Text.FormatKey("MSG_ITEM_GIVE", itemChar.Name, item.GetName())));


            itemChar.EquippedItem = item;
            ItemData entry = DataManager.Instance.GetItem(itemChar.EquippedItem.ID);

            if (item.Cursed)
            {
                GameManager.Instance.SE(DataManager.Instance.CursedSE);
                yield return CoroutineManager.Instance.StartCoroutine(MenuManager.Instance.SetDialogue(false, Text.FormatKey("MSG_EQUIP_CURSED", item.GetName(), itemChar.Name)));
            }
            else if (entry.Cursed)
            {
                GameManager.Instance.SE(DataManager.Instance.CursedSE);
                yield return CoroutineManager.Instance.StartCoroutine(MenuManager.Instance.SetDialogue(false, Text.FormatKey("MSG_EQUIP_AUTOCURSE", item.GetName(), itemChar.Name)));
                item.Cursed = true;
            }
        }

        public IEnumerator<YieldInstruction> ProcessTakeItem(GroundChar character, int teamSlot)
        {
            ExplorerTeam memberTeam = DataManager.Instance.Save.ActiveTeam;
            Character itemChar = memberTeam.Players[teamSlot];

            //no curse check in ground mode

            InvItem item = itemChar.EquippedItem;
            memberTeam.Inventory.Add(item);
            itemChar.EquippedItem = new InvItem();
            GameManager.Instance.SE(DataManager.Instance.EquipSE);
            yield return CoroutineManager.Instance.StartCoroutine(MenuManager.Instance.SetDialogue(false, Text.FormatKey("MSG_ITEM_DEQUIP", itemChar.Name, item.GetName())));

        }

        //public bool CanCheckGround()

        //public ChoiceMenu GetGroundCheckMenu()

        //public IEnumerator<YieldInstruction> CheckEXP()

        //public void HandoutEXP()

        //public IEnumerator<YieldInstruction> HandoutLevelUp()



        //public IEnumerator<YieldInstruction> CheckLevelSkills(Character player, int oldLevel)

        //public IEnumerator<YieldInstruction> LearnSkillWithFanfare(Character player, int skill, int slot)

        //public IEnumerator<YieldInstruction> TrySkillLearn(Character player, int skillIndex, VertChoiceMenu.OnChooseSlot learnAction, Action passAction)

        //public IEnumerator<YieldInstruction> TrySkillDelete(Character player, int skillIndex, VertChoiceMenu.OnChooseSlot learnAction, Action passAction)


        //public void TryStopLearn(Character player, int skillIndex, VertChoiceMenu.OnChooseSlot learnAction, Action passAction)


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
            DataManager.Instance.Save.ActiveTeam.AddToSortedAssembly(player);
            RemoveChar(index);

            if (player.EquippedItem.ID > -1)
            {
                InvItem heldItem = player.EquippedItem;
                player.EquippedItem = new InvItem();
                DataManager.Instance.Save.ActiveTeam.Inventory.Add(heldItem);
            }
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

        //public IEnumerator<YieldInstruction> AddMapStatus(MapStatus status, bool msg = true)

        //public IEnumerator<YieldInstruction> RemoveMapStatus(int id, bool msg = true)

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
