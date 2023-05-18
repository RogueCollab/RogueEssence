using System;
using System.Collections.Generic;
using RogueEssence.Content;
using RogueElements;
using RogueEssence.Data;
using RogueEssence.Menu;

namespace RogueEssence.Dungeon
{
    public partial class DungeonScene
    {

        private void ProcessDir(Dir8 dir, Character character)
        {
            if (dir > Dir8.None)
                character.CharDir = dir;
        }

        public IEnumerator<YieldInstruction> ProcessWalk(Character character, bool wantItem, ActionResult result)
        {
            if (character.Dead)
                yield break;
            
            //immobilization check
            if (character.CantWalk)
                yield break;
                
            //check for blocking

            List<Dir8> trialDirs = new List<Dir8>();

            trialDirs.Add(character.CharDir);

            if (character.MovesScrambled)
            {
                trialDirs.Add(DirExt.AddAngles(character.CharDir, Dir8.DownRight));
                trialDirs.Add(DirExt.AddAngles(character.CharDir, Dir8.DownLeft));
            }


            for (int ii = trialDirs.Count - 1; ii >= 0; ii--)
            {
                Dir8 checkDir = trialDirs[ii];
                if (ZoneManager.Instance.CurrentMap.DirBlocked(checkDir, character.CharLoc, character.Mobility, 1, false, true))
                    trialDirs.RemoveAt(ii);
                else
                {
                    Loc checkLoc = character.CharLoc + checkDir.GetLoc();
                    Character checkChar = ZoneManager.Instance.CurrentMap.GetCharAtLoc(checkLoc);

                    if (checkChar != null)
                    {
                        if (checkChar.CantWalk || GetMatchup(character, checkChar, false) == Alignment.Foe)
                            trialDirs.RemoveAt(ii);
                    }
                }
            }


            if (trialDirs.Count == 0)//there is no direction to move
                yield break;

            while (trialDirs.Count >= 0)
            {
                if (trialDirs.Count == 0)
                    yield break;

                break;
            }

            result.Success = ActionResult.ResultType.TurnTaken;


            int dirIndex = DataManager.Instance.Save.Rand.Next(trialDirs.Count);
            Dir8 sampleDir = trialDirs[dirIndex];

            Loc fromLoc = character.CharLoc;
            Loc loc = character.CharLoc + sampleDir.GetLoc();
            Character switchedChar = ZoneManager.Instance.CurrentMap.GetCharAtLoc(loc);
            
            ProcessDir(sampleDir, character);

            int charSpeed = GetSpeedMult(character, loc);


            CharAnimWalk walkAnim = new CharAnimWalk(fromLoc, loc, character.CharDir, charSpeed);
            yield return CoroutineManager.Instance.StartCoroutine(character.StartAnim(walkAnim));

            if (switchedChar != null)
            {
                ProcessDir(character.CharDir.Reverse(), switchedChar);

                CharAnimWalk switchedWalkAnim = new CharAnimWalk(switchedChar.CharLoc, switchedChar.CharLoc + switchedChar.CharDir.GetLoc(), switchedChar.CharDir, charSpeed);
                yield return CoroutineManager.Instance.StartCoroutine(switchedChar.StartAnim(switchedWalkAnim));
            }

            SingleCharContext mainContext = new SingleCharContext(character);
            yield return CoroutineManager.Instance.StartCoroutine(character.OnWalk(mainContext));
            if (!character.Dead)
                yield return CoroutineManager.Instance.StartCoroutine(ArriveOnTile(character, true, wantItem));
            if (switchedChar != null)
            {
                SingleCharContext switchedContext = new SingleCharContext(switchedChar);
                yield return CoroutineManager.Instance.StartCoroutine(switchedChar.OnWalk(switchedContext));
                if (!switchedChar.Dead)
                    yield return CoroutineManager.Instance.StartCoroutine(ArriveOnTile(switchedChar));
            }


            //if it's a team character and it's team mode, wait until the walk is over, and wait a little while
            if (DataManager.Instance.Save.TeamMode && character.MemberTeam == ActiveTeam)
            {
                CharAnimation standAnim = new CharAnimIdle(character.CharLoc, character.CharDir);
                standAnim.MajorAnim = true;
                yield return CoroutineManager.Instance.StartCoroutine(character.StartAnim(standAnim));
                yield return new WaitForFrames(GameManager.Instance.ModifyBattleSpeed(20));
            }

            //NOTE: this is a one-time hack for the moment, where player walks will not take a frame of delay
            //so as to stay in sync with everyone else.
            if (charSpeed > 0)
                GameManager.Instance.FrameProcessed = true;

            //FrameProcessed must be set before FinishTurn because processes in FinishTurn, such as menus, may want to set FrameProcessed to false again
            yield return CoroutineManager.Instance.StartCoroutine(FinishTurn(character, true, false, true));
        }
        public IEnumerator<YieldInstruction> FinishTurn(Character character, bool advanceTurn = true)
        {
            return FinishTurn(character, advanceTurn, true, false);
        }

        public IEnumerator<YieldInstruction> FinishTurn(Character character, bool advanceTurn, bool action, bool walked)
        {
            yield return CoroutineManager.Instance.StartCoroutine(CheckEXP());

            LogMsg(Text.DIVIDER_STR);

            //check for mobility violation at the end of anyone's turn
            //only do this when someone has changed location, or when someone has changed mobility
            foreach (Character standChar in ZoneManager.Instance.CurrentMap.DisplacedChars)
            {
                if (!standChar.Dead)
                {
                    HashSet<Loc> iterWarpHistory = new HashSet<Loc>();
                    while (!iterWarpHistory.Contains(standChar.CharLoc) && ZoneManager.Instance.CurrentMap.TileBlocked(standChar.CharLoc, standChar.Mobility))
                    {
                        iterWarpHistory.Add(standChar.CharLoc);
                        yield return CoroutineManager.Instance.StartCoroutine(WarpNear(standChar, standChar.CharLoc, true));
                    }
                }
            }
            ZoneManager.Instance.CurrentMap.DisplacedChars.Clear();

            //end turn
            if (!character.Dead)
            {
                SingleCharContext turnContext = new SingleCharContext(character);
                yield return CoroutineManager.Instance.StartCoroutine(character.OnTurnEnd(turnContext));
            }


            if (!character.Dead) //add HP based on natural healing
                yield return CoroutineManager.Instance.StartCoroutine(character.UpdateFullness(action));

            //check for EXP gain again
            yield return CoroutineManager.Instance.StartCoroutine(CheckEXP());

            LogMsg(Text.DIVIDER_STR);

            //continue the walk phase
            if (advanceTurn)
                yield return CoroutineManager.Instance.StartCoroutine(MoveToUsableTurn(action, walked));
        }

        public IEnumerator<YieldInstruction> ArriveOnTile(Character character)
        {
            return ArriveOnTile(character, false, false);
        }

        public IEnumerator<YieldInstruction> ArriveOnTile(Character character, bool checkItem, bool wantItem)
        {
            //if void, add restart
            if (!Collision.InBounds(ZoneManager.Instance.CurrentMap.Width, ZoneManager.Instance.CurrentMap.Height, character.CharLoc))
                throw new Exception("Player out of bounds");
            else
            {
                //update exploration if the team is the player's team
                ZoneManager.Instance.CurrentMap.UpdateExploration(character);

                //check for item
                if (checkItem)
                {
                    int itemSlot = ZoneManager.Instance.CurrentMap.GetItem(character.CharLoc);
                    if (itemSlot > -1)
                    {

                        MapItem item = ZoneManager.Instance.CurrentMap.Items[itemSlot];
                        string itemName = item.GetDungeonName();

                        if (wantItem)
                        {
                            Team memberTeam = character.MemberTeam;
                            if (memberTeam is ExplorerTeam)
                            {
                                ExplorerTeam explorerTeam = (ExplorerTeam)memberTeam;
                                bool canGet = (explorerTeam.GetInvCount() < explorerTeam.GetMaxInvSlots(ZoneManager.Instance.CurrentZone));
                                if (item.Price > 0)
                                    canGet = false;
                                else
                                {
                                    canGet |= item.IsMoney;
                                    if (!canGet)
                                    {
                                        ItemData entry = DataManager.Instance.GetItem(item.Value);
                                        if (entry.MaxStack > 1)
                                        {
                                            //find an inventory slot that isn't full stack
                                            foreach (InvItem inv in explorerTeam.EnumerateInv())
                                            {
                                                if (inv.ID == item.Value && inv.Cursed == item.Cursed && inv.Amount < entry.MaxStack)
                                                {
                                                    canGet = true;
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                                if (canGet)
                                    yield return CoroutineManager.Instance.StartCoroutine(PickupItem(character, itemSlot));
                                else
                                {
                                    if (character == ActiveTeam.Leader)
                                        PendingLeaderAction = PromptFloorItem();
                                    wantItem = false;
                                }   
                            }
                            else if (memberTeam is MonsterTeam)
                            {
                                if (item.Price > 0 || item.IsMoney || !String.IsNullOrEmpty(character.EquippedItem.ID))
                                    wantItem = false;
                                else
                                    yield return CoroutineManager.Instance.StartCoroutine(PickupHoldItem(character));
                            }
                        }

                        if (!wantItem)
                            LogPickup(new PickupItem(Text.FormatKey("MSG_PASS_ITEM", character.GetDisplayName(false), itemName), "", "", character.CharLoc, character, true));
                    }
                }

                yield return CoroutineManager.Instance.StartCoroutine(RecurseDisplacements(character));
            }
        }

        public IEnumerator<YieldInstruction> RecurseDisplacements(Character character)
        {
            Tile tile = ZoneManager.Instance.CurrentMap.Tiles[character.CharLoc.X][character.CharLoc.Y];
            Loc landTile = character.CharLoc;
            if (!String.IsNullOrEmpty(tile.Effect.ID))
            {
                if (!character.WarpHistory.Contains(character.CharLoc))
                {
                    character.WarpHistory.Add(character.CharLoc);
                    //check for tile property
                    yield return CoroutineManager.Instance.StartCoroutine(tile.Effect.LandedOnTile(character));
                    yield return CoroutineManager.Instance.StartCoroutine(ActivateTraps(character));
                    character.WarpHistory.RemoveAt(character.WarpHistory.Count - 1);
                }
            }

            //check to make sure the character is on the same tile still, before moving on
            if (character.CharLoc == landTile)
            {
                if (!character.WarpHistory.Contains(character.CharLoc))
                {
                    character.WarpHistory.Add(character.CharLoc);
                    yield return CoroutineManager.Instance.StartCoroutine(tile.Data.LandedOnTile(character));
                    character.WarpHistory.RemoveAt(character.WarpHistory.Count - 1);
                }
            }
            else
                yield return CoroutineManager.Instance.StartCoroutine(RecurseDisplacements(character));
        }


        public void QueueTrap(Loc loc)
        {
            loc = ZoneManager.Instance.CurrentMap.WrapLoc(loc);
            //order matters
            if (!PendingTraps.Contains(loc))
                PendingTraps.Add(loc);
        }

        public IEnumerator<YieldInstruction> ActivateTraps(Character activator)
        {
            List<Loc> runningTraps = new List<Loc>();
            runningTraps.AddRange(PendingTraps);
            PendingTraps.Clear();
            foreach (Loc pendingLoc in runningTraps)
            {
                Tile tile = ZoneManager.Instance.CurrentMap.Tiles[pendingLoc.X][pendingLoc.Y];
                if (!String.IsNullOrEmpty(tile.Effect.ID))
                {
                    SingleCharContext singleContext = new SingleCharContext(activator);
                    yield return CoroutineManager.Instance.StartCoroutine(tile.Effect.InteractWithTile(singleContext));
                }
            }
            yield break;
        }

        public IEnumerator<YieldInstruction> PromptFloorItem()
        {
            if (DataManager.Instance.CurrentReplay != null)
                yield break;

            int itemSlot = ZoneManager.Instance.CurrentMap.GetItem(ActiveTeam.Leader.CharLoc);
            if (itemSlot > -1)
            {
                GameManager.Instance.SE("Menu/Confirm");
                yield return CoroutineManager.Instance.StartCoroutine(MenuManager.Instance.ProcessMenuCoroutine(new ItemUnderfootMenu(itemSlot)));
            }
        }

        public IEnumerator<YieldInstruction> ProcessPickup(Character character, ActionResult result)
        {
            Team memberTeam = character.MemberTeam;
            if (!(memberTeam is ExplorerTeam))
                yield break;

            if (character.CantInteract)
            {
                LogMsg(Text.FormatKey("MSG_CANT_PICKUP_ITEM", character.GetDisplayName(false)), false, true);
                yield break;
            }

            int itemSlot = ZoneManager.Instance.CurrentMap.GetItem(character.CharLoc);
            if (itemSlot > -1)
            {
                result.Success = ActionResult.ResultType.TurnTaken;

                yield return CoroutineManager.Instance.StartCoroutine(PickupItem(character, itemSlot));
                yield return new WaitForFrames(GameManager.Instance.ModifyBattleSpeed(20));

                yield return CoroutineManager.Instance.StartCoroutine(FinishTurn(character));
            }
            else
                LogMsg(Text.FormatKey("MSG_NOTHING_UNDERFOOT", character), false, true);
        }

        public IEnumerator<YieldInstruction> PickupItem(Character character, int itemSlot)
        {
            //assumes they have the space
            MapItem item = ZoneManager.Instance.CurrentMap.Items[itemSlot];

            ItemCheckContext context = new ItemCheckContext(character, item, new MapItem());

            Team memberTeam = character.MemberTeam;
            if (item.IsMoney)
            {
                ZoneManager.Instance.CurrentMap.Items.RemoveAt(itemSlot);
                LogPickup(new PickupItem(Text.FormatKey("MSG_PICKUP_MONEY", character.GetDisplayName(false), Text.FormatKey("MONEY_AMOUNT", item.Amount)), item.SpriteIndex, GraphicsManager.MoneySE, item.TileLoc, character, false));
                ((ExplorerTeam)memberTeam).AddMoney(character, item.Amount);
            }
            else
            {
                string msg = null;
                ItemData entry = DataManager.Instance.GetItem(item.Value);
                if (entry.MaxStack > 1)
                {
                    MapItem nameItem = new MapItem(item);
                    foreach (InvItem inv in ((ExplorerTeam)memberTeam).EnumerateInv())
                    {
                        if (inv.ID == item.Value && inv.Cursed == item.Cursed && inv.Amount < entry.MaxStack)
                        {
                            int addValue = Math.Min(entry.MaxStack - inv.Amount, item.Amount);
                            inv.Amount += addValue;
                            item.Amount -= addValue;
                            if (item.Amount <= 0)
                                break;
                        }
                    }
                    //still some stacks left to take care of
                    if (item.Amount > 0 && ((ExplorerTeam)memberTeam).GetInvCount() < ((ExplorerTeam)memberTeam).GetMaxInvSlots(ZoneManager.Instance.CurrentZone))
                    {
                        ((ExplorerTeam)memberTeam).AddToInv(item.MakeInvItem());
                        item.Amount = 0;
                    }
                    if (item.Amount == 0)
                    {
                        ZoneManager.Instance.CurrentMap.Items.RemoveAt(itemSlot);
                        msg = Text.FormatKey("MSG_PICKUP_ITEM", character.GetDisplayName(false), nameItem.GetDungeonName());
                    }
                    else
                        msg = Text.FormatKey("MSG_PICKUP_SOME_ITEM", character.GetDisplayName(false), nameItem.GetDungeonName());
                }
                else
                {
                    ZoneManager.Instance.CurrentMap.Items.RemoveAt(itemSlot);
                    ((ExplorerTeam)memberTeam).AddToInv(item.MakeInvItem());
                    msg = Text.FormatKey("MSG_PICKUP_ITEM", character.GetDisplayName(false), item.GetDungeonName());
                }
                bool teamCharacter = ActiveTeam.Players.Contains(character) || ActiveTeam.Guests.Contains(character);
                LogPickup(new PickupItem(msg, item.SpriteIndex, teamCharacter ? GraphicsManager.PickupSE : GraphicsManager.PickupFoeSE, item.TileLoc, character, false));
            }

            yield return CoroutineManager.Instance.StartCoroutine(character.OnPickup(context));
        }


        public IEnumerator<YieldInstruction> PickupHoldItem(Character character)
        {
            //TODO: Due to logging no pickups, the code in here happens instantaneously when it should not
            int mapSlot = ZoneManager.Instance.CurrentMap.GetItem(character.CharLoc);

            MapItem mapItem = ZoneManager.Instance.CurrentMap.Items[mapSlot];
            ZoneManager.Instance.CurrentMap.Items.RemoveAt(mapSlot);

            InvItem item = mapItem.MakeInvItem();

            bool teamCharacter = ActiveTeam.Players.Contains(character) || ActiveTeam.Guests.Contains(character);
            GameManager.Instance.SE(teamCharacter ? GraphicsManager.PickupSE : GraphicsManager.PickupFoeSE);
            if (!String.IsNullOrEmpty(character.EquippedItem.ID))
            {
                LogMsg(Text.FormatKey("MSG_REPLACE_HOLD_ITEM", character.GetDisplayName(false), item.GetDisplayName(), character.EquippedItem.GetDisplayName()));
                //spawn item on floor
                ZoneManager.Instance.CurrentMap.Items.Add(new MapItem(character.EquippedItem, character.CharLoc));
            }
            else
                LogMsg(Text.FormatKey("MSG_PICKUP_HOLD_ITEM", character.GetDisplayName(false), item.GetDisplayName()));

            yield return CoroutineManager.Instance.StartCoroutine(character.EquipItem(item));
        }


        private IEnumerator<YieldInstruction> ProcessPlaceItem(Character character, int invSlot, ActionResult result)
        {
            if (character.CantInteract)
            {
                LogMsg(Text.FormatKey("MSG_CANT_DROP_ITEM", character.GetDisplayName(false)), false, true);
                yield break;
            }

            if (invSlot == BattleContext.EQUIP_ITEM_SLOT && character.EquippedItem.Cursed && !character.CanRemoveStuck)
            {
                GameManager.Instance.SE(GraphicsManager.CursedSE);
                LogMsg(Text.FormatKey("MSG_DEQUIP_CURSED", character.GetDisplayName(false), character.EquippedItem.GetDisplayName()), false, true);
                yield break;
            }
            Loc loc = character.CharLoc;
            if (!ZoneManager.Instance.CurrentMap.CanItemLand(loc, true, true))
            {
                LogMsg(Text.FormatKey("MSG_CANT_DROP_HERE"), false, true);
                yield break;
            }
            
            int mapSlot = ZoneManager.Instance.CurrentMap.GetItem(character.CharLoc);
            if (mapSlot > -1)
            {
                //try and swap
                MapItem item = ZoneManager.Instance.CurrentMap.Items[mapSlot];
                if (item.IsMoney)
                {
                    LogMsg(Text.FormatKey("MSG_CANT_REPLACE_MONEY"), false, true);
                    yield break;
                }

                result.Success = ActionResult.ResultType.TurnTaken;

                ZoneManager.Instance.CurrentMap.Items.RemoveAt(mapSlot);
                if (invSlot == BattleContext.EQUIP_ITEM_SLOT)
                {
                    InvItem invItem = character.EquippedItem;
                    
                    //no need to call dequip since equip will be called
                    
                    ZoneManager.Instance.CurrentMap.Items.Add(new MapItem(invItem, loc));

                    bool teamCharacter = ActiveTeam.Players.Contains(character) || ActiveTeam.Guests.Contains(character);
                    GameManager.Instance.SE(teamCharacter ? GraphicsManager.PickupSE : GraphicsManager.PickupFoeSE);

                    LogMsg(Text.FormatKey("MSG_REPLACE_HOLD_ITEM", character.GetDisplayName(false), item.GetDungeonName(), invItem.GetDisplayName()));

                    yield return CoroutineManager.Instance.StartCoroutine(character.EquipItem(item.MakeInvItem()));
                }
                else
                {
                    ExplorerTeam memberTeam = (ExplorerTeam)character.MemberTeam;
                    InvItem invItem = memberTeam.GetInv(invSlot);

                    ItemCheckContext context = new ItemCheckContext(character, item, new MapItem(invItem));

                    memberTeam.RemoveFromInv(invSlot);
                    
                    ZoneManager.Instance.CurrentMap.Items.Add(new MapItem(invItem, loc));

                    GameManager.Instance.SE(GraphicsManager.ReplaceSE);

                    LogMsg(Text.FormatKey("MSG_REPLACE_ITEM", item.GetDungeonName(), invItem.GetDisplayName()));

                    memberTeam.AddToInv(item.MakeInvItem());

                    yield return CoroutineManager.Instance.StartCoroutine(character.OnPickup(context));
                }
            }
            else
            {
                result.Success = ActionResult.ResultType.TurnTaken;

                InvItem invItem;
                if (invSlot == -1)
                    invItem = character.EquippedItem;
                else
                    invItem = character.MemberTeam.GetInv(invSlot);

                LogMsg(Text.FormatKey("MSG_PLACE_ITEM", character.GetDisplayName(false), invItem.GetDisplayName()));

                ZoneManager.Instance.CurrentMap.Items.Add(new MapItem(invItem, loc));
                GameManager.Instance.SE(GraphicsManager.PlaceSE);

                if (invSlot == -1)
                    yield return CoroutineManager.Instance.StartCoroutine(character.DequipItem());
                else
                    character.MemberTeam.RemoveFromInv(invSlot);
            }
            yield return new WaitForFrames(GameManager.Instance.ModifyBattleSpeed(20));

            yield return CoroutineManager.Instance.StartCoroutine(FinishTurn(character));
        }

        public IEnumerator<YieldInstruction> ProcessObjectInteract(Character character, ActionResult result)
        {
            //past this point, the game state is changed

            if (character == FocusedCharacter && !character.CantInteract)
            {
                Loc frontLoc = character.CharLoc + character.CharDir.GetLoc();
                foreach(Character member in ActiveTeam.EnumerateChars())
                {
                    if (member.CharLoc == frontLoc && !member.Dead)
                    {
                        yield return CoroutineManager.Instance.StartCoroutine(ProcessAllyInteract(character, member, result));
                        yield break;
                    }
                }
                foreach (Team allyTeam in ZoneManager.Instance.CurrentMap.AllyTeams)
                {
                    foreach (Character member in allyTeam.EnumerateChars())
                    {
                        if (member.CharLoc == frontLoc && !member.Dead)
                        {
                            yield return CoroutineManager.Instance.StartCoroutine(ProcessAllyInteract(character, member, result));
                            yield break;
                        }
                    }
                }

                //read signs or objects
                Tile tile = ZoneManager.Instance.CurrentMap.GetTile(frontLoc);
                if (tile != null && !String.IsNullOrEmpty(tile.Effect.ID))
                {
                    TileData entry = DataManager.Instance.GetTile(tile.Effect.ID);
                    if (entry.StepType == TileData.TriggerType.Blocker || entry.StepType == TileData.TriggerType.Unlockable || entry.StepType == TileData.TriggerType.Object)
                    {
                        yield return CoroutineManager.Instance.StartCoroutine(tile.Effect.LandedOnTile(character));
                        yield return CoroutineManager.Instance.StartCoroutine(ActivateTraps(character));
                        yield break;
                    }
                }
            }

            //no talking, so just attack
            yield return CoroutineManager.Instance.StartCoroutine(ProcessUseSkill(character, BattleContext.DEFAULT_ATTACK_SLOT, result));
        }

        public IEnumerator<YieldInstruction> ProcessAllyInteract(Character character, Character target, ActionResult result)
        {
            BattleContext context = new BattleContext(BattleActionType.None);
            context.User = character;
            context.Target = target;
            foreach (BattleEvent effect in target.ActionEvents)
                yield return CoroutineManager.Instance.StartCoroutine(effect.Apply(null, target, context));

            if (!context.CancelState.Cancel)
            {
                yield return CoroutineManager.Instance.StartCoroutine(FinishTurn(context.User, !context.TurnCancel.Cancel));
                result.Success = context.TurnCancel.Cancel ? ActionResult.ResultType.Success : ActionResult.ResultType.TurnTaken;
            }
        }
        
        public IEnumerator<YieldInstruction> ProcessTileInteract(Character character, ActionResult result)
        {
            if (character.CantInteract)
            {
                LogMsg(Text.FormatKey("MSG_CANT_CHECK_TILE", character.GetDisplayName(false)), false, true);
                yield break;
            }

            Tile tile = ZoneManager.Instance.CurrentMap.Tiles[character.CharLoc.X][character.CharLoc.Y];
            if (String.IsNullOrEmpty(tile.Effect.ID))
                throw new Exception("Attempted to trigger a nonexistent tile effect.  This should never happen.");
            else
            {
                SingleCharContext singleContext = new SingleCharContext(character);
                yield return CoroutineManager.Instance.StartCoroutine(tile.Effect.InteractWithTile(singleContext));

                if (singleContext.CancelState.Cancel && singleContext.TurnCancel.Cancel) { yield return CoroutineManager.Instance.StartCoroutine(CancelWait(singleContext.User.CharLoc)); yield break; }

                if (!singleContext.TurnCancel.Cancel)
                {
                    yield return new WaitForFrames(GameManager.Instance.ModifyBattleSpeed(20));
                    yield return CoroutineManager.Instance.StartCoroutine(FinishTurn(character));
                }
                result.Success = singleContext.TurnCancel.Cancel ? ActionResult.ResultType.Success : ActionResult.ResultType.TurnTaken;
            }
        }

        public IEnumerator<YieldInstruction> ProcessGiveItem(Character character, int invSlot, int teamSlot, ActionResult result)
        {
            if (character.CantInteract)
            {
                LogMsg(Text.FormatKey("MSG_CANT_SWAP_ITEM", character.GetDisplayName(false)), false, true);
                yield break;
            }

            Character itemChar = character;
            Team memberTeam = character.MemberTeam;
            if (teamSlot > -1)
                itemChar = memberTeam.Players[teamSlot];
            if (itemChar.EquippedItem.Cursed && !itemChar.CanRemoveStuck)
            {
                GameManager.Instance.SE(GraphicsManager.CursedSE);
                LogMsg(Text.FormatKey("MSG_DEQUIP_CURSED", itemChar.GetDisplayName(false), itemChar.EquippedItem.GetDisplayName()), false, true);
                yield break;
            }

            result.Success = ActionResult.ResultType.TurnTaken;

            if (invSlot == BattleContext.FLOOR_ITEM_SLOT)
            {
                //character and itemChar are the same
                yield return CoroutineManager.Instance.StartCoroutine(PickupHoldItem(character));
            }
            else
            {
                InvItem item = ((ExplorerTeam)memberTeam).GetInv(invSlot);
                ((ExplorerTeam)memberTeam).RemoveFromInv(invSlot);

                GameManager.Instance.SE(GraphicsManager.EquipSE);

                if (!String.IsNullOrEmpty(itemChar.EquippedItem.ID))
                {
                    LogMsg(Text.FormatKey("MSG_ITEM_SWAP", itemChar.GetDisplayName(false), item.GetDisplayName(), itemChar.EquippedItem.GetDisplayName()));
                    //put item in inv
                    ((ExplorerTeam)memberTeam).AddToInv(new InvItem(itemChar.EquippedItem));
                }
                else
                    LogMsg(Text.FormatKey("MSG_ITEM_GIVE", itemChar.GetDisplayName(false), item.GetDisplayName()));

                yield return CoroutineManager.Instance.StartCoroutine(itemChar.EquipItem(item));
            }

            yield return new WaitForFrames(GameManager.Instance.ModifyBattleSpeed(20));
            yield return CoroutineManager.Instance.StartCoroutine(FinishTurn(character));
        }

        public IEnumerator<YieldInstruction> ProcessTakeItem(Character character, int teamSlot, ActionResult result)
        {
            if (character.CantInteract)
            {
                LogMsg(Text.FormatKey("MSG_CANT_DEQUIP", character.GetDisplayName(false)), false, true);
                yield break;
            }

            Team memberTeam = character.MemberTeam;
            Character itemChar = memberTeam.Players[teamSlot];
            if (itemChar.EquippedItem.Cursed && !itemChar.CanRemoveStuck)
            {
                GameManager.Instance.SE(GraphicsManager.CursedSE);
                LogMsg(Text.FormatKey("MSG_DEQUIP_CURSED", itemChar.GetDisplayName(false), itemChar.EquippedItem.GetDisplayName()), false, true);
                yield break;
            }

            result.Success = ActionResult.ResultType.TurnTaken;

            InvItem item = itemChar.EquippedItem;
            GameManager.Instance.SE(GraphicsManager.EquipSE);
            LogMsg(Text.FormatKey("MSG_ITEM_DEQUIP", character.GetDisplayName(false), item.GetDisplayName()));
            yield return CoroutineManager.Instance.StartCoroutine(itemChar.DequipItem());

            memberTeam.AddToInv(item);

            yield return new WaitForFrames(GameManager.Instance.ModifyBattleSpeed(20));
            yield return CoroutineManager.Instance.StartCoroutine(FinishTurn(character));
        }

        public bool CanCheckGround()
        {
            Character character = FocusedCharacter;

            //check for item
            int itemSlot = ZoneManager.Instance.CurrentMap.GetItem(character.CharLoc);
            if (itemSlot != -1)
                return true;
            Tile tile = ZoneManager.Instance.CurrentMap.Tiles[character.CharLoc.X][character.CharLoc.Y];
            if (!String.IsNullOrEmpty(tile.Effect.ID))
            {
                TileData tileData = DataManager.Instance.GetTile(tile.Effect.ID);
                if (tileData.StepType != TileData.TriggerType.None)
                    return true;
            }

            return false;
        }

        public ChoiceMenu GetGroundCheckMenu()
        {
            Character character = FocusedCharacter;
            
            //check for item
            int itemSlot = ZoneManager.Instance.CurrentMap.GetItem(character.CharLoc);
            if (itemSlot > -1)
                return new ItemUnderfootMenu(itemSlot);
            Tile tile = ZoneManager.Instance.CurrentMap.Tiles[character.CharLoc.X][character.CharLoc.Y];
            if (!String.IsNullOrEmpty(tile.Effect.ID))
                return new TileUnderfootMenu(tile.Effect.ID);
            
            return null;
        }

        public IEnumerator<YieldInstruction> CheckEXP()
        {
            if (GainedEXP.Count > 0)
                HandoutEXP();

            if (LevelGains.Count > 0)
            {
                yield return CoroutineManager.Instance.StartCoroutine(HandoutLevelUp());
            }
        }

        public void HandoutEXP()
        {
            if (ZoneManager.Instance.CurrentZone.NoEXP)
            {
                GainedEXP.Clear();
                return;
            }

            for (int ii = 0; ii < ActiveTeam.Players.Count; ii++)
            {
                if (ii >= GainedEXP.Count)
                    break;
                Character player = ActiveTeam.Players[ii];
                int levelDiff = 0;
                int totalExp = 0;

                if (!player.Dead && player.Level < DataManager.Instance.Start.MaxLevel)
                {
                    totalExp += GainedEXP[ii];

                    string growth = DataManager.Instance.GetMonster(player.BaseForm.Species).EXPTable;
                    GrowthData growthData = DataManager.Instance.GetGrowth(growth);
                    while (player.Level + levelDiff < DataManager.Instance.Start.MaxLevel && player.EXP + totalExp >= growthData.GetExpTo(player.Level, player.Level + levelDiff + 1))
                        levelDiff++;
                    while (player.Level + levelDiff > 1 && player.EXP + totalExp < growthData.GetExpTo(player.Level, player.Level + levelDiff))
                        levelDiff--;
                }

                player.EXP += totalExp;
                if (totalExp != 0)
                {
                    MeterChanged(player.CharLoc, totalExp, true);
                    LogMsg(Text.FormatKey("MSG_EXP_GAIN_MEMBER", player.GetDisplayName(true), totalExp), true, false);
                }
                if (levelDiff != 0)
                    LevelGains.Add(new CharIndex(Faction.Player, 0, false, ii));
            }

            GainedEXP.Clear();
        }

        public IEnumerator<YieldInstruction> HandoutLevelUp()
        {
            for (int ii = 0; ii < LevelGains.Count; ii++)
            {
                CharIndex index = LevelGains[ii];

                Team levelTeam = ZoneManager.Instance.CurrentMap.GetTeam(index.Faction, index.Team);
                Character player = levelTeam.Players[index.Char];
                string growth = DataManager.Instance.GetMonster(player.BaseForm.Species).EXPTable;
                GrowthData growthData = DataManager.Instance.GetGrowth(growth);
                int oldLevel = player.Level;
                int oldHP = player.MaxHP;
                int oldSpeed = player.BaseSpeed;
                int oldAtk = player.BaseAtk;
                int oldDef = player.BaseDef;
                int oldMAtk = player.BaseMAtk;
                int oldMDef = player.BaseMDef;

                if (player.Level < DataManager.Instance.Start.MaxLevel && player.EXP >= growthData.GetExpToNext(player.Level))
                {
                    while (player.EXP >= growthData.GetExpToNext(player.Level))
                    {
                        player.EXP -= growthData.GetExpToNext(player.Level);
                        player.Level++;

                        if (player.Level >= DataManager.Instance.Start.MaxLevel)
                        {
                            player.EXP = 0;
                            break;
                        }
                    }


                    GameManager.Instance.Fanfare("Fanfare/LevelUp");
                    yield return CoroutineManager.Instance.StartCoroutine(GameManager.Instance.LogSkippableMsg(Text.FormatKey("DLG_LEVEL_UP", player.GetDisplayName(true), player.Level), player.MemberTeam));
                    if (levelTeam == ActiveTeam && DataManager.Instance.CurrentReplay == null)
                    {
                        GameManager.Instance.SE("Menu/Confirm");
                        yield return CoroutineManager.Instance.StartCoroutine(MenuManager.Instance.ProcessMenuCoroutine(new LevelUpMenu(index.Char, oldLevel, oldHP, oldSpeed, oldAtk, oldDef, oldMAtk, oldMDef)));
                    }

                    yield return CoroutineManager.Instance.StartCoroutine(CheckLevelSkills(player, oldLevel));
                }
                else if (player.EXP < 0)
                {
                    while (player.EXP < 0)
                    {
                        if (player.Level <= 1)
                        {
                            player.EXP = 0;
                            break;
                        }
                        player.EXP += growthData.GetExpToNext(player.Level - 1);
                        player.Level--;
                    }

                    //bound out max HP
                    player.HP = Math.Min(player.MaxHP, player.HP);

                    GameManager.Instance.Fanfare("Fanfare/LevelDown");
                    yield return CoroutineManager.Instance.StartCoroutine(GameManager.Instance.LogSkippableMsg(Text.FormatKey("DLG_LEVEL_DOWN", player.GetDisplayName(true), player.Level), player.MemberTeam));
                }
            }
            LevelGains.Clear();
        }




        public static List<string> GetLevelSkills(Character player, int oldLevel)
        {
            List<string> skills = new List<string>();
            BaseMonsterForm entry = DataManager.Instance.GetMonster(player.BaseForm.Species).Forms[player.BaseForm.Form];
            int startLevel = 0;
            int endLevel = 0;
            if (oldLevel > 0)
            {
                startLevel = oldLevel + 1;
                endLevel = player.Level;
            }
            for (int ii = startLevel; ii <= endLevel; ii++)
            {
                foreach (string skill in entry.GetSkillsAtLevel(ii, false))
                    skills.Add(skill);
            }
            return skills;
        }

        public IEnumerator<YieldInstruction> CheckLevelSkills(Character player, int oldLevel)
        {
            if (!ActiveTeam.Players.Contains(player))
                yield break;

            foreach (string skill in GetLevelSkills(player, oldLevel))
            {
                int learn = -1;

                if (DataManager.Instance.CurrentReplay != null)
                    learn = DataManager.Instance.CurrentReplay.ReadUI();
                else
                {
                    yield return CoroutineManager.Instance.StartCoroutine(TryLearnSkill(player, skill, (int slot) => { learn = slot; }, () => { }));
                    DataManager.Instance.LogUIPlay(learn);
                }
                if (learn > -1)
                    yield return CoroutineManager.Instance.StartCoroutine(LearnSkillWithFanfare(player, skill, learn));
            }
        }

        public static IEnumerator<YieldInstruction> LearnSkillWithFanfare(Character player, string skill, int slot)
        {
            GameManager.Instance.SE("Fanfare/LearnSkill");
            string oldSkill = "";
            if (!String.IsNullOrEmpty(player.BaseSkills[slot].SkillNum))
            {
                SkillData oldEntry = DataManager.Instance.GetSkill(player.BaseSkills[slot].SkillNum);
                oldSkill = oldEntry.GetIconName();
            }
            player.ReplaceSkill(skill, slot, DataManager.Instance.Save.GetDefaultEnable(skill));

            if (oldSkill == "")
                yield return CoroutineManager.Instance.StartCoroutine(GameManager.Instance.LogSkippableMsg(Text.FormatKey("DLG_SKILL_LEARN", player.GetDisplayName(false), DataManager.Instance.GetSkill(skill).GetIconName()), player.MemberTeam));
            else
                yield return CoroutineManager.Instance.StartCoroutine(GameManager.Instance.LogSkippableMsg(Text.FormatKey("DLG_SKILL_REPLACE", player.GetDisplayName(false), DataManager.Instance.GetSkill(skill).GetIconName(), oldSkill), player.MemberTeam));

        }

        
        public static IEnumerator<YieldInstruction> TryLearnSkill(Character player, string skillIndex, VertChoiceMenu.OnChooseSlot learnAction, Action passAction)
        {
            int totalSkills = 0;
            for (int ii = 0; ii < player.BaseSkills.Count; ii++)
            {
                if (player.BaseSkills[ii].SkillNum == skillIndex)
                {
                    passAction();
                    yield break;
                }
                if (!String.IsNullOrEmpty(player.BaseSkills[ii].SkillNum))
                    totalSkills++;
            }
            if (totalSkills < CharData.MAX_SKILL_SLOTS)
                learnAction(totalSkills);
            else
            {
                //yield return CoroutinesManager.Instance.StartCoroutine(MenuManager.Instance.SetDialogue(Text.FormatKey("DLG_SKILL_FULL", player.Name, DataManager.Instance.GetSkill(skillIndex).Name.ToLocal())));
                //yield return CoroutinesManager.Instance.StartCoroutine(MenuManager.Instance.ProcessMenuCoroutine(createLearnQuestion(player, skillIndex, learnAction, passAction)));
                //NOTE: There is a very subtle problem that can happen if the code below were to be replaced with the commented code above.
                //this error can only happen when this function is called from an action already in free mode
                //it will appear as though the first line did not run, when in fact the first dialogue menu was buried due to the way free mode mechanics work
                //more research will be needed on how to solve this issue

                yield return CoroutineManager.Instance.StartCoroutine(MenuManager.Instance.SetDialogue(() => { MenuManager.Instance.AddMenu(createLearnQuestion(player, skillIndex, learnAction, passAction), false); },
                    Text.FormatKey("DLG_SKILL_FULL", player.GetDisplayName(false), DataManager.Instance.GetSkill(skillIndex).GetIconName())));
            }
        }

        private static DialogueBox createLearnQuestion(Character player, string skillIndex, VertChoiceMenu.OnChooseSlot learnAction, Action passAction)
        {
            return MenuManager.Instance.CreateQuestion(Text.FormatKey("DLG_SKILL_DELETE", DataManager.Instance.GetSkill(skillIndex).GetIconName()),
                () =>
                {
                    //show replace menu, pass on the passAction, feed in a skill-learn action
                    MenuManager.Instance.AddMenu(new SkillReplaceMenu(player, skillIndex, learnAction,
                        () => { MenuManager.Instance.AddMenu(createRefuseQuestion(player, skillIndex, learnAction, passAction), false); }), false);
                },
                () => { MenuManager.Instance.AddMenu(createRefuseQuestion(player, skillIndex, learnAction, passAction), false); });
        }

        private static DialogueBox createRefuseQuestion(Character player, string skillIndex, VertChoiceMenu.OnChooseSlot learnAction, Action passAction)
        {
            return MenuManager.Instance.CreateQuestion(Text.FormatKey("DLG_SKILL_STOP_LEARN", DataManager.Instance.GetSkill(skillIndex).GetIconName()),
                () =>
                {
                    MenuManager.Instance.ClearMenus();
                    MenuManager.Instance.AddMenu(MenuManager.Instance.CreateDialogue(passAction, Text.FormatKey("DLG_SKILL_NO_LEARN", player.GetDisplayName(false), DataManager.Instance.GetSkill(skillIndex).GetIconName())), false);
                },
                () => { MenuManager.Instance.AddMenu(createLearnQuestion(player, skillIndex, learnAction, passAction), false); });
        }


        public IEnumerator<YieldInstruction> AskToSendHome()
        {
            if (DataManager.Instance.CurrentReplay != null)
            {
                //automatically send home
                int index = DataManager.Instance.CurrentReplay.ReadUI();
                yield return CoroutineManager.Instance.StartCoroutine(ChooseSendHome(index));
            }
            else
            {
                yield return CoroutineManager.Instance.StartCoroutine(MenuManager.Instance.SetDialogue(Text.FormatKey("DLG_TEAM_FULL")));
                yield return CoroutineManager.Instance.StartCoroutine(MenuManager.Instance.ProcessMenuCoroutine(new TeamMenu(true)));
            }
        }
        
        public IEnumerator<YieldInstruction> ChooseSendHome(int index)
        {
            if (DataManager.Instance.CurrentReplay == null)
                DataManager.Instance.LogUIPlay(index);
            yield return CoroutineManager.Instance.StartCoroutine(SendHome(index));
        }

        public IEnumerator<YieldInstruction> SendHome(int index)
        {
            Character player = ActiveTeam.Players[index];
            if (!player.Dead)
                yield return CoroutineManager.Instance.StartCoroutine(DungeonScene.Instance.ProcessBattleFX(player, player, DataManager.Instance.SendHomeFX));

            if (!String.IsNullOrEmpty(player.EquippedItem.ID))
            {
                InvItem heldItem = player.EquippedItem;
                yield return CoroutineManager.Instance.StartCoroutine(player.DequipItem());
                if (ActiveTeam.GetInvCount() + 1 < ActiveTeam.GetMaxInvSlots(ZoneManager.Instance.CurrentZone))
                    ActiveTeam.AddToInv(heldItem);
                else if (player.Dead)
                    yield return CoroutineManager.Instance.StartCoroutine(DropItem(heldItem, FocusedCharacter.CharLoc));
                else
                    yield return CoroutineManager.Instance.StartCoroutine(DropItem(heldItem, player.CharLoc));
            }

            RemoveChar(new CharIndex(Faction.Player, 0, false, index));
            ActiveTeam.Assembly.Insert(0, player);

            ZoneManager.Instance.CurrentMap.UpdateExploration(player);
            yield return new WaitForFrames(30);

            yield return CoroutineManager.Instance.StartCoroutine(GameManager.Instance.LogSkippableMsg(Text.FormatKey("MSG_TEAM_SENT_HOME", player.GetDisplayName(false))));
        }

        public void SilentSendHome(int index)
        {
            Character player = ActiveTeam.Players[index];

            if (!String.IsNullOrEmpty(player.EquippedItem.ID))
            {
                InvItem heldItem = player.EquippedItem;
                player.SilentDequipItem();
                if (ActiveTeam.GetInvCount() + 1 < ActiveTeam.GetMaxInvSlots(ZoneManager.Instance.CurrentZone))
                    ActiveTeam.AddToInv(heldItem);
            }

            RemoveChar(new CharIndex(Faction.Player, 0, false, index));
            ActiveTeam.Assembly.Insert(0, player);
        }

        public IEnumerator<YieldInstruction> DropItem(InvItem item, Loc loc)
        {
            yield return CoroutineManager.Instance.StartCoroutine(DropItem(item, loc, loc));
        }

        public IEnumerator<YieldInstruction> DropItem(InvItem item, Loc loc, Loc start)
        {
            MapItem mapItem = new MapItem(item);
            yield return CoroutineManager.Instance.StartCoroutine(DropMapItem(mapItem, loc, start, false));
        }

        public IEnumerator<YieldInstruction> DropMoney(int amount, Loc loc, Loc start)
        {
            MapItem mapItem = MapItem.CreateMoney(amount);
            yield return CoroutineManager.Instance.StartCoroutine(DropMapItem(mapItem, loc, start, false));
        }

        public IEnumerator<YieldInstruction> DropMapItem(MapItem item, Loc loc, Loc start, bool silent)
        {
            MapItem mapItem = new MapItem(item);
            string itemName = mapItem.GetDungeonName();

            if (!ZoneManager.Instance.CurrentMap.CanItemLand(loc, false, false))
            {
                Loc? newLoc = ZoneManager.Instance.CurrentMap.FindItemlessTile(loc, 2);

                if (newLoc == null)
                {
                    if (!silent)
                    {
                        LogMsg(Text.FormatKey("MSG_MAP_ITEM_LOST", itemName));
                        yield return CoroutineManager.Instance.StartCoroutine(DungeonScene.Instance.ProcessBattleFX(loc, loc, Dir8.Down, DataManager.Instance.ItemLostFX));
                    }
                    yield break;
                }
                else
                    loc = newLoc.Value;
            }
            if (loc != start)
            {
                if (!silent)
                {
                    ItemAnim itemAnim = new ItemAnim(start * GraphicsManager.TileSize + new Loc(GraphicsManager.TileSize / 2), loc * GraphicsManager.TileSize + new Loc(GraphicsManager.TileSize / 2), item.IsMoney ? GraphicsManager.MoneySprite : DataManager.Instance.GetItem(item.Value).Sprite, GraphicsManager.TileSize / 2, 1);
                    CreateAnim(itemAnim, DrawLayer.Normal);
                    yield return new WaitForFrames(ItemAnim.ITEM_ACTION_TIME);
                }
            }
            Tile tile = ZoneManager.Instance.CurrentMap.GetTile(loc);
            TerrainData terrain = tile.Data.GetData();
            if (terrain.BlockType == TerrainData.Mobility.Lava)
            {
                if (!silent)
                {
                    LogMsg(Text.FormatKey("MSG_MAP_ITEM_LOST", itemName));
                    yield return CoroutineManager.Instance.StartCoroutine(DungeonScene.Instance.ProcessBattleFX(loc, loc, Dir8.Down, DataManager.Instance.ItemLostFX));
                }
            }
            else if (terrain.BlockType == TerrainData.Mobility.Abyss)
            {
                if (!silent)
                    LogMsg(Text.FormatKey("MSG_MAP_ITEM_FALL", itemName));
            }
            else
            {
                mapItem.TileLoc = ZoneManager.Instance.CurrentMap.WrapLoc(loc);
                ZoneManager.Instance.CurrentMap.Items.Add(mapItem);

                if (!silent)
                {
                    if (terrain.BlockType == TerrainData.Mobility.Water)
                        LogMsg(Text.FormatKey("MSG_MAP_ITEM_WATER", itemName));
                    else
                        LogMsg(Text.FormatKey("MSG_MAP_ITEM_GROUND", itemName));
                }
            }
            if (!silent)
                yield return new WaitForFrames(GameManager.Instance.ModifyBattleSpeed(10));
        }

        public IEnumerator<YieldInstruction> AddMapStatus(MapStatus status, bool msg = true)
        {
            MapStatus statusToCheck;
            if (ZoneManager.Instance.CurrentMap.Status.TryGetValue(status.ID, out statusToCheck))
                yield return CoroutineManager.Instance.StartCoroutine(((MapStatusData)statusToCheck.GetData()).RepeatMethod.Apply(statusToCheck, null, null, status, msg));
            else
            {
                ZoneManager.Instance.CurrentMap.Status.Add(status.ID, status);
                status.StartEmitter(Anims);
            }

            ZoneManager.Instance.CurrentMap.RefreshTraits();
            foreach (Character character in ZoneManager.Instance.CurrentMap.IterateCharacters())
                character.RefreshTraits();


            EventEnqueueFunction<MapStatusGivenEvent> function = (StablePriorityQueue<GameEventPriority, EventQueueElement<MapStatusGivenEvent>> queue, Priority maxPriority, ref Priority nextPriority) =>
            {
                //start with universal
                DataManager.Instance.UniversalEvent.AddEventsToQueue(queue, maxPriority, ref nextPriority, DataManager.Instance.UniversalEvent.OnMapStatusAdds, null);
                ZoneManager.Instance.CurrentMap.MapEffect.AddEventsToQueue(queue, maxPriority, ref nextPriority, ZoneManager.Instance.CurrentMap.MapEffect.OnMapStatusAdds, null);

                //call ALL status's on add for the add status
                foreach (MapStatus mapStatus in ZoneManager.Instance.CurrentMap.Status.Values)
                {
                    MapStatusData entry = DataManager.Instance.GetMapStatus(mapStatus.ID);
                    mapStatus.AddEventsToQueue<MapStatusGivenEvent>(queue, maxPriority, ref nextPriority, entry.OnMapStatusAdds, null);
                }

                int portPriority = 0;
                foreach (Character character in ZoneManager.Instance.CurrentMap.IterateCharacters())
                {
                    if (!character.Dead)
                    {
                        foreach (PassiveContext effectContext in character.IteratePassives(new Priority(portPriority)))
                            effectContext.AddEventsToQueue(queue, maxPriority, ref nextPriority, effectContext.EventData.OnMapStatusAdds, character);
                    }
                    portPriority++;
                }
            };
            foreach (EventQueueElement<MapStatusGivenEvent> effect in IterateEvents<MapStatusGivenEvent>(function))
                yield return CoroutineManager.Instance.StartCoroutine(effect.Event.Apply(effect.Owner, effect.OwnerChar, effect.TargetChar, status, msg));
        }
        
        public IEnumerator<YieldInstruction> RemoveMapStatus(string id, bool msg = true)
        {
            MapStatus statusToRemove;
            if (ZoneManager.Instance.CurrentMap.Status.TryGetValue(id, out statusToRemove))
            {
                ZoneManager.Instance.CurrentMap.Status.Remove(statusToRemove.ID);
                statusToRemove.EndEmitter();

                ZoneManager.Instance.CurrentMap.RefreshTraits();
                foreach (Character character in ZoneManager.Instance.CurrentMap.IterateCharacters())
                    character.RefreshTraits();


                EventEnqueueFunction<MapStatusGivenEvent> function = (StablePriorityQueue<GameEventPriority, EventQueueElement<MapStatusGivenEvent>> queue, Priority maxPriority, ref Priority nextPriority) =>
                {
                    //start with universal
                    DataManager.Instance.UniversalEvent.AddEventsToQueue(queue, maxPriority, ref nextPriority, DataManager.Instance.UniversalEvent.OnMapStatusRemoves, null);
                    ZoneManager.Instance.CurrentMap.MapEffect.AddEventsToQueue(queue, maxPriority, ref nextPriority, ZoneManager.Instance.CurrentMap.MapEffect.OnMapStatusRemoves, null);

                    //call ALL status's on add for the remove status, including the removed one
                    {
                        MapStatusData entry = DataManager.Instance.GetMapStatus(statusToRemove.ID);
                        statusToRemove.AddEventsToQueue<MapStatusGivenEvent>(queue, maxPriority, ref nextPriority, entry.OnMapStatusRemoves, null);
                    }
                    foreach (MapStatus mapStatus in ZoneManager.Instance.CurrentMap.Status.Values)
                    {
                        MapStatusData entry = DataManager.Instance.GetMapStatus(mapStatus.ID);
                        mapStatus.AddEventsToQueue<MapStatusGivenEvent>(queue, maxPriority, ref nextPriority, entry.OnMapStatusRemoves, null);
                    }


                    int portPriority = 0;
                    foreach (Character character in ZoneManager.Instance.CurrentMap.IterateCharacters())
                    {
                        if (!character.Dead)
                        {
                            foreach (PassiveContext effectContext in character.IteratePassives(new Priority(portPriority)))
                                effectContext.AddEventsToQueue(queue, maxPriority, ref nextPriority, effectContext.EventData.OnMapStatusRemoves, character);
                        }
                        portPriority++;
                    }
                };
                foreach (EventQueueElement<MapStatusGivenEvent> effect in IterateEvents<MapStatusGivenEvent>(function))
                    yield return CoroutineManager.Instance.StartCoroutine(effect.Event.Apply(effect.Owner, effect.OwnerChar, effect.TargetChar, statusToRemove, msg));
            }
        }


        public MapStatus GetMapStatus(string id)
        {
            MapStatus value;
            if (ZoneManager.Instance.CurrentMap.Status.TryGetValue(id, out value))
                return value;
            return null;
        }


        public IEnumerator<YieldInstruction> PointWarp(Character character, Loc loc, bool msg)
        {
            if (msg)
                LogMsg(Text.FormatKey("MSG_WARP", character.GetDisplayName(false)));

            yield return CoroutineManager.Instance.StartCoroutine(DungeonScene.Instance.ProcessBattleFX(character, character, DataManager.Instance.WarpFX));

            bool moved = (character.CharLoc != loc);

            CharAnimWarp warpAnim = new CharAnimWarp();
            warpAnim.FromLoc = character.CharLoc;
            warpAnim.CharDir = character.CharDir;
            warpAnim.ToLoc = loc;
            warpAnim.MajorAnim = true;

            yield return CoroutineManager.Instance.StartCoroutine(character.StartAnim(warpAnim));
            if (moved)
                yield return CoroutineManager.Instance.StartCoroutine(ArriveOnTile(character));

        }

        public IEnumerator<YieldInstruction> SyncActions(Character char1, CharAnimation anim1, Character char2, CharAnimation anim2)
        {
            if (char1.OccupiedwithAction() || char2.OccupiedwithAction())
                yield return new WaitWhile(() => { return char1.OccupiedwithAction() || char2.OccupiedwithAction(); });

            bool moved1 = (char1.CharLoc != anim1.CharLoc);
            bool moved2 = (char2.CharLoc != anim2.CharLoc);

            yield return CoroutineManager.Instance.StartCoroutine(char1.StartAnim(anim1));
            yield return CoroutineManager.Instance.StartCoroutine(char2.StartAnim(anim2));

            if (moved1)
                yield return CoroutineManager.Instance.StartCoroutine(ArriveOnTile(char1));
            if (moved2)
                yield return CoroutineManager.Instance.StartCoroutine(ArriveOnTile(char2));
        }

        public IEnumerator<YieldInstruction> RandomWarp(Character character, int radius, bool msg = true)
        {
            List<Loc> locs = ZoneManager.Instance.CurrentMap.FindNearLocs(character, character.CharLoc, radius);

            if (locs.Count == 0)
            {
                if (msg)
                    LogMsg(Text.FormatKey("MSG_WARP_FAIL", character.GetDisplayName(false)));
            }
            else
            {
                Loc loc = locs[DataManager.Instance.Save.Rand.Next(locs.Count)];
                yield return CoroutineManager.Instance.StartCoroutine(PointWarp(character, loc, msg));
            }
        }


        public IEnumerator<YieldInstruction> WarpNear(Character character, Loc loc, bool msg = true)
        {
            yield return CoroutineManager.Instance.StartCoroutine(WarpNear(character, loc, 0, msg));
        }

        public IEnumerator<YieldInstruction> WarpNear(Character character, Loc loc, int diffRange, bool msg = true)
        {

            Loc? dest = null;
            if (diffRange > 0)
            {
                List<Loc> candidates = ZoneManager.Instance.CurrentMap.FindNearLocs(character, loc, diffRange);
                if (candidates.Count > 0)
                    dest = candidates[DataManager.Instance.Save.Rand.Next(candidates.Count)];
            }

            if (dest == null)
                dest = ZoneManager.Instance.CurrentMap.GetClosestTileForChar(character, loc);

            if (dest == null || dest.Value == character.CharLoc)
            {
                if (msg)
                    LogMsg(Text.FormatKey("MSG_WARP_FAIL", character.GetDisplayName(false)));
            }
            else
                yield return CoroutineManager.Instance.StartCoroutine(PointWarp(character, dest.Value, msg));
        }

        public IEnumerator<YieldInstruction> Pounce(Character character, Dir8 dir, Loc startLoc, int range)
        {
            if (dir == Dir8.None)
                throw new ArgumentException(String.Format("Can't pounce in direction: {0}", dir));

            //face direction
            character.CharDir = dir;
            Loc charStart = character.CharLoc;

            Loc endLoc = startLoc;
            Loc checkLoc = endLoc;
            //move to the point at range.
            for (int ii = 0; ii < range; ii++)
            {
                //if the next location is blocked, stop.
                if (!ShotBlocked(character, checkLoc, dir, Alignment.Friend | Alignment.Foe, true, true))
                {
                    checkLoc = checkLoc + dir.GetLoc();
                    endLoc = checkLoc;
                }
                else
                    checkLoc = checkLoc + dir.GetLoc();
            }

            //animate with knockback for now
            CharAnimKnockback knockback = new CharAnimKnockback();
            knockback.FromLoc = character.CharLoc;
            knockback.CharDir = character.CharDir;
            knockback.ToLoc = endLoc;
            knockback.RecoilLoc = endLoc;

            bool moved = (charStart != endLoc);

            knockback.MajorAnim = true;
            yield return CoroutineManager.Instance.StartCoroutine(character.StartAnim(knockback));

            ZoneManager.Instance.CurrentMap.UpdateExploration(character);

            
            if (moved)
                yield return CoroutineManager.Instance.StartCoroutine(ArriveOnTile(character));
        }

        public IEnumerator<YieldInstruction> KnockBack(Character character, Dir8 dir, int range)
        {
            if (dir == Dir8.None)
                throw new ArgumentException(String.Format("Can't knock back in direction: {0}", dir));

            //face direction
            character.CharDir = dir.Reverse();

            Loc endLoc = character.CharLoc;

            //move to the point at range.
            for (int ii = 0; ii < range; ii++)
            {
                //if the next location is blocked, stop.
                if (ShotBlocked(character, endLoc, dir, Alignment.None, true, true))
                    break;
                endLoc = endLoc + dir.GetLoc();
            }

            yield return CoroutineManager.Instance.StartCoroutine(DungeonScene.Instance.ProcessBattleFX(character, character, DataManager.Instance.KnockbackFX));

            //animate knockback to that location
            CharAnimKnockback knockback = new CharAnimKnockback();
            knockback.FromLoc = character.CharLoc;
            knockback.CharDir = character.CharDir;
            knockback.ToLoc = endLoc;

            if (ZoneManager.Instance.CurrentMap.GetCharAtLoc(endLoc) != null)
            {
                Loc? dest = ZoneManager.Instance.CurrentMap.GetClosestTileForChar(character, endLoc);
                endLoc = (dest.HasValue ? dest.Value : character.CharLoc);
            }
            knockback.RecoilLoc = endLoc;

            bool moved = (character.CharLoc != endLoc);

            knockback.MajorAnim = true;
            yield return CoroutineManager.Instance.StartCoroutine(character.StartAnim(knockback));

            ZoneManager.Instance.CurrentMap.UpdateExploration(character);

            //don't do damage for now


            if (moved)
                yield return CoroutineManager.Instance.StartCoroutine(ArriveOnTile(character));
        }

        public IEnumerator<YieldInstruction> JumpTo(Character character, Dir8 dir, int range)
        {
            Loc endLoc = character.CharLoc;
                
            //move to the point at range.
            for (int ii = 0; ii < range; ii++)
            {
                //if the next location is blocked, stop.
                if (ShotBlocked(character, endLoc, dir, Alignment.None, true, true))
                    break;
                endLoc = endLoc + dir.GetLoc();
            }

            //if the location is occupied, keep moving
            while (ZoneManager.Instance.CurrentMap.GetCharAtLoc(endLoc) != null)
            {
                if (ShotBlocked(character, endLoc, dir, Alignment.None, true, true))
                    break;
                endLoc = endLoc + dir.GetLoc();
            }

            //find the closest free (accessible) space to that location
            Loc? dest = ZoneManager.Instance.CurrentMap.GetClosestTileForChar(character, endLoc);

            endLoc = (dest.HasValue ? dest.Value : character.CharLoc);

            //if it's the same place as the player, hop in place
            bool moved = (endLoc != character.CharLoc);
            if (moved)
                LogMsg(Text.FormatKey("MSG_JUMP", character.GetDisplayName(false)));
            else
                LogMsg(Text.FormatKey("MSG_JUMP_FAIL", character.GetDisplayName(false)));


            yield return CoroutineManager.Instance.StartCoroutine(DungeonScene.Instance.ProcessBattleFX(character, character, DataManager.Instance.JumpFX));

            //animate hop to that location
            CharAnimJump jumpTo = new CharAnimJump();
            jumpTo.FromLoc = character.CharLoc;
            jumpTo.CharDir = character.CharDir;
            jumpTo.ToLoc = endLoc;

            jumpTo.MajorAnim = true;
            yield return CoroutineManager.Instance.StartCoroutine(character.StartAnim(jumpTo));
            if (moved)
                yield return CoroutineManager.Instance.StartCoroutine(ArriveOnTile(character));
        }


        public delegate IEnumerator<YieldInstruction> ThrowHitEffect(Character target, Character attacker);
        public IEnumerator<YieldInstruction> ThrowTo(Character character, Character attacker, Dir8 dir, int range, Alignment targetAlignments, ThrowHitEffect targetHit)
        {
            //face direction
            character.CharDir = dir.Reverse();

            Loc endLoc = character.CharLoc;

            //find the closest target to land on
            Loc? target = ThrowAction.GetTarget(attacker, character.CharLoc, dir, true, range, targetAlignments, character);
            if (target != null)
                endLoc = target.Value;
            else //if impossible to find, use the default farthest landing spot
            {
                for (int ii = 0; ii < range; ii++)
                {
                    Loc nextLoc = endLoc + dir.GetLoc();
                    if (ZoneManager.Instance.CurrentMap.TileAttackBlocked(nextLoc, true))
                        break;
                    endLoc = nextLoc;
                }
            }

            //if the landing spot is occupied or nontraversible, find the closest unoccupied tile
            Loc? dest = ZoneManager.Instance.CurrentMap.GetClosestTileForChar(character, endLoc);

            yield return CoroutineManager.Instance.StartCoroutine(DungeonScene.Instance.ProcessBattleFX(character, character, DataManager.Instance.ThrowFX));

            //animate knockback to that location
            CharAnimThrown thrown = new CharAnimThrown();
            thrown.FromLoc = character.CharLoc;
            thrown.CharDir = character.CharDir;
            thrown.ToLoc = endLoc;

            endLoc = (dest.HasValue ? dest.Value : character.CharLoc);

            thrown.RecoilLoc = endLoc;

            bool moved = (character.CharLoc != endLoc);

            thrown.MajorAnim = true;
            yield return CoroutineManager.Instance.StartCoroutine(character.StartAnim(thrown));

            ZoneManager.Instance.CurrentMap.UpdateExploration(character);

            yield return new WaitForFrames(CharAnimThrown.ANIM_TIME);

            if (target != null) //if landed on enemy
                yield return CoroutineManager.Instance.StartCoroutine(targetHit(ZoneManager.Instance.CurrentMap.GetCharAtLoc(target.Value), attacker));
            else //if landed by self
                yield return CoroutineManager.Instance.StartCoroutine(targetHit(character, attacker));
            

            if (moved)
                yield return CoroutineManager.Instance.StartCoroutine(ArriveOnTile(character));
        }

        public bool CanTeamSeeChar(Team team, Character character)
        {
            foreach (Character player in team.Players)
            {
                if (!player.Dead)
                {
                    if (player.CanSeeCharacter(character))
                        return true;
                }
            }
            return false;
        }
        
        public bool CanTeamSeeLoc(Team team, Loc loc)
        {
            foreach (Character player in team.Players)
            {
                if (!player.Dead)
                {
                    if (player.CanSeeLoc(loc, player.GetTileSight()))
                        return true;
                }
            }
            return false;
        }
        public bool CanTeamSeeCharLoc(Team team, Loc loc)
        {
            foreach (Character player in team.Players)
            {
                if (!player.Dead)
                {
                    if (player.CanSeeLoc(loc, player.GetCharSight()))
                        return true;
                }
            }
            return false;
        }


        public bool ShotBlocked(Character character, Loc loc, Dir8 dir, Alignment blockedAlignments, bool useMobility, bool blockedByWall)
        {
            return ShotBlocked(character, loc, dir, blockedAlignments, useMobility, blockedByWall, false);
        }

        public bool ShotBlocked(Character character, Loc loc, Dir8 dir, Alignment blockedAlignments, bool useMobility, bool blockedByWall, bool blockedByDiagonal)
        {
            TerrainData.Mobility mobility = TerrainData.Mobility.Lava | TerrainData.Mobility.Water | TerrainData.Mobility.Abyss;

            if (!blockedByWall)
                mobility |= TerrainData.Mobility.Block;
            if (useMobility)
                mobility |= character.Mobility;

            if (ZoneManager.Instance.CurrentMap.DirBlocked(dir, loc, mobility, 1, false, blockedByDiagonal))
                return true;
            
            if (blockedAlignments != Alignment.None)
                return BlockedByCharacter(character, loc + dir.GetLoc(), blockedAlignments);

            return false;
        }


        public bool VisionBlocked(Loc loc)
        {
            if (!ZoneManager.Instance.CurrentMap.GetLocInMapBounds(ref loc))
                return true;

            Tile tile = ZoneManager.Instance.CurrentMap.Tiles[loc.X][loc.Y];
            TerrainData terrain = tile.Data.GetData();
            if (terrain.BlockLight)
                return true;
            //TileData effect = DataManager.Instance.GetTile(tile.Effect.ID);
            //if (effect.BlockLight)
            //    return true;

            return false;
        }

        public bool BlockedByCharacter(Character character, Loc loc, Alignment targetAlignments)
        {
            Character target = ZoneManager.Instance.CurrentMap.GetCharAtLoc(loc);
            if (target == null)
                return false;
            return IsTargeted(character, target, targetAlignments);
        }

        //used for run mode ending
        public bool AreTilesDistinct(Loc loc1, Loc loc2)
        {
            //differences are in either traversible terrain, traps, or items

            //if (ZoneManager.Instance.CurrentMap.TileBlocked(loc1, FocusedCharacter.Mobility) != ZoneManager.Instance.CurrentMap.TileBlocked(loc2, FocusedCharacter.Mobility))
            //    return true;
            
            bool hasItem1 = false;
            bool hasItem2 = false;
            foreach (MapItem item in ZoneManager.Instance.CurrentMap.Items)
            {
                if (item.TileLoc == loc1)
                    hasItem1 = true;
                if (item.TileLoc == loc2)
                    hasItem2 = true;
            }
            if (hasItem1 != hasItem2)
                return true;

            string effectId1 = "";
            string effectId2 = "";

            Tile tile1 = ZoneManager.Instance.CurrentMap.GetTile(loc1);
            Tile tile2 = ZoneManager.Instance.CurrentMap.GetTile(loc2);

            if (tile1 != null)
                effectId1 = tile1.Effect.ID;
            if (tile2 != null)
                effectId2 = tile2.Effect.ID;

            if (effectId1 != effectId2)
                return true;

            return false;
        }

        public bool IsRunningHall(Character character, Loc loc)
        {
            //checks to see if any diagonals are fully open
            for (int ii = 0; ii < DirExt.DIR4_COUNT; ii++)
            {
                if (!Grid.IsDirBlocked(loc, ((Dir4)ii).ToDir8().Rotate(1), ZoneManager.Instance.CurrentMap.TileBlocked, ZoneManager.Instance.CurrentMap.TileBlocked, 1))
                    return false;
            }

            return true;
        }

        public void GetSideBlocks(Character character, int forward, out bool blockedL, out bool blockedR)
        {
            Loc loc = character.CharLoc + character.CharDir.GetLoc() * forward;
            blockedL = ZoneManager.Instance.CurrentMap.TileBlocked(loc + DirExt.AddAngles(character.CharDir, Dir8.Left).GetLoc());
            blockedR = ZoneManager.Instance.CurrentMap.TileBlocked(loc + DirExt.AddAngles(character.CharDir, Dir8.Right).GetLoc());
        }

        public bool IsRunningHazard(Loc loc)
        {
            Tile tile = ZoneManager.Instance.CurrentMap.GetTile(loc);

            if (tile != null)//TODO: make this not so hardcoded??
            {
                TerrainData terrain = tile.Data.GetData();
                return (terrain.BlockType == TerrainData.Mobility.Lava || terrain.BlockType == TerrainData.Mobility.Block);
            }

            return false;
        }

        public Dir8 getTurnDir(bool ally, bool foe)
        {
            bool[] losTargets = new bool[DirExt.DIR8_COUNT];

            //First, get a list of all entities in line of sight
            //not empty?  pick the first one in a clockwise rotation
            foreach (Character target in ZoneManager.Instance.CurrentMap.IterateCharacters(ally, foe))
            {
                Loc unwrapped = ZoneManager.Instance.CurrentMap.GetClosestUnwrappedLoc(FocusedCharacter.CharLoc, target.CharLoc);
                Dir8 offDir = DirExt.GetDir(FocusedCharacter.CharLoc, unwrapped);
                if (offDir != Dir8.None)
                {
                    if (CanSeeCharOnScreen(target) && Collision.InFront(FocusedCharacter.CharLoc, unwrapped, offDir, -1))
                        losTargets[(int)offDir] = true;
                }
            }

            for (int ii = 1; ii < DirExt.DIR8_COUNT; ii++)
            {
                Dir8 testDir = DirExt.AddAngles(FocusedCharacter.CharDir, (Dir8)ii);
                if (losTargets[(int)testDir])
                    return testDir;
            }
            if (losTargets[(int)FocusedCharacter.CharDir])
                return FocusedCharacter.CharDir;
            return Dir8.None;
        }
    }
}
