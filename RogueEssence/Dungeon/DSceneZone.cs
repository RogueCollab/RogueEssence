using System;
using System.Collections.Generic;
using RogueEssence.Content;
using RogueElements;
using RogueEssence.Data;
using RogueEssence.Menu;
using RogueEssence.Script;

namespace RogueEssence.Dungeon
{
    public partial class DungeonScene
    {
        public void EnterFloor(int entryPointIndex)
        {
            LocRay8 entry = new LocRay8(Loc.Zero, Dir8.Down);
            if (entryPointIndex < ZoneManager.Instance.CurrentMap.EntryPoints.Count)
                entry = ZoneManager.Instance.CurrentMap.EntryPoints[entryPointIndex];
            EnterFloor(entry);
        }
        public void EnterFloor(LocRay8 entryPoint)
        {
            //put the carry-overs in the new map
            foreach (MapStatus status in ZoneManager.Instance.CurrentZone.CarryOver)
                ZoneManager.Instance.CurrentMap.Status[status.ID] = status;
            ZoneManager.Instance.CurrentZone.CarryOver.Clear();

            ZoneManager.Instance.CurrentMap.EnterMap(DataManager.Instance.Save.ActiveTeam, entryPoint);

            ResetFloor();
        }

        public override void Exit()
        {
            ResetAnims();
        }

        public IEnumerator<YieldInstruction> ExitFloor()
        {
            if (ZoneManager.Instance.CurrentMap != null)
            {
                //Notify script engine
                yield return CoroutineManager.Instance.StartCoroutine(ZoneManager.Instance.CurrentMap.OnExit());

                //remove statuses
                foreach (Character character in ZoneManager.Instance.CurrentMap.ActiveTeam.EnumerateChars())
                    character.OnRemove();

                ZoneManager.Instance.CurrentMap.ActiveTeam = null;

                //pull out all map statuses that are meant to carry over
                ZoneManager.Instance.CurrentZone.CarryOver.Clear();
                foreach (MapStatus status in ZoneManager.Instance.CurrentMap.Status.Values)
                {
                    MapStatusData data = (MapStatusData)status.GetData();
                    if (data.CarryOver)
                        ZoneManager.Instance.CurrentZone.CarryOver.Add(status);
                }

                ZoneManager.Instance.CurrentZone.SetCurrentMap(SegLoc.Invalid);

            }
        }

        public void ResetFloor()
        {
            ResetTurns();

            //refresh everyone's traits
            ZoneManager.Instance.CurrentMap.RefreshTraits();
            foreach (Character character in ZoneManager.Instance.CurrentMap.IterateCharacters())
                character.RefreshTraits();

        }

        public IEnumerator<YieldInstruction> InitFloor()
        {
            //start emitters for existing map status
            foreach (MapStatus mapStatus in ZoneManager.Instance.CurrentMap.Status.Values)
                mapStatus.StartEmitter(Anims);

            GraphicsManager.GlobalIdle = GraphicsManager.IdleAction;

            //Notify script engine
            yield return CoroutineManager.Instance.StartCoroutine(ZoneManager.Instance.CurrentMap.OnInit());
        }

        public IEnumerator<YieldInstruction> BeginFloor()
        {
            DataManager.Instance.Save.LocTrail.Add(new ZoneLoc(ZoneManager.Instance.CurrentZoneID, ZoneManager.Instance.CurrentZone.CurrentMapID));
            DataManager.Instance.Save.Trail.Add(ZoneManager.Instance.CurrentMap.GetColoredName());
            LogMsg(Text.FormatKey("MSG_ENTER_MAP", ActiveTeam.GetDisplayName(), ZoneManager.Instance.CurrentMap.GetColoredName()), true, false);

            ZoneManager.Instance.CurrentMap.Begun = true;

            //process player happenings
            foreach (Character character in ZoneManager.Instance.CurrentMap.IterateCharacters())
                character.Tactic.Initialize(character);

            SingleCharContext context = new SingleCharContext(null);
            //map starts for map statuses
            EventEnqueueFunction<SingleCharEvent> function = (StablePriorityQueue<GameEventPriority, EventQueueElement<SingleCharEvent>> queue, Priority maxPriority, ref Priority nextPriority) =>
            {
                //start with universal
                DataManager.Instance.UniversalEvent.AddEventsToQueue(queue, maxPriority, ref nextPriority, DataManager.Instance.UniversalEvent.OnMapStarts, null);
                ZoneManager.Instance.CurrentMap.MapEffect.AddEventsToQueue(queue, maxPriority, ref nextPriority, ZoneManager.Instance.CurrentMap.MapEffect.OnMapStarts, null);

                foreach (MapStatus mapStatus in ZoneManager.Instance.CurrentMap.Status.Values)
                {
                    MapStatusData entry = DataManager.Instance.GetMapStatus(mapStatus.ID);
                    mapStatus.AddEventsToQueue<SingleCharEvent>(queue, maxPriority, ref nextPriority, entry.OnMapStarts, null);
                }

                int portPriority = 0;
                foreach (Character character in ZoneManager.Instance.CurrentMap.IterateCharacters())
                {
                    if (!character.Dead)
                    {
                        DataManager.Instance.UniversalEvent.AddEventsToQueue(queue, maxPriority, ref nextPriority, DataManager.Instance.UniversalEvent.OnMapStarts, character);
                        ZoneManager.Instance.CurrentMap.MapEffect.AddEventsToQueue(queue, maxPriority, ref nextPriority, ZoneManager.Instance.CurrentMap.MapEffect.OnMapStarts, character);

                        foreach (PassiveContext effectContext in character.IteratePassives(new Priority(portPriority)))
                            effectContext.AddEventsToQueue(queue, maxPriority, ref nextPriority, effectContext.EventData.OnMapStarts, character);
                    }
                    portPriority++;
                }
            };
            foreach (EventQueueElement<SingleCharEvent> effect in IterateEvents<SingleCharEvent>(function))
            {
                context.User = effect.TargetChar;
                yield return CoroutineManager.Instance.StartCoroutine(effect.Event.Apply(effect.Owner, effect.OwnerChar, context));
                //normally, we would check for cancel, but I'm not sure if it's ideal to stop it for everyone here.
            }

            yield return CoroutineManager.Instance.StartCoroutine(ZoneManager.Instance.CurrentMap.OnEnter());

            LogMsg(Text.DIVIDER_STR);

            yield return CoroutineManager.Instance.StartCoroutine(CheckMobilityViolations());
        }


        public IEnumerator<YieldInstruction> ProcessAI()
        {
            GameAction action = CurrentCharacter.Tactic.GetAction(CurrentCharacter, DataManager.Instance.Save.Rand, false);
            ActionResult result = new ActionResult();
            yield return CoroutineManager.Instance.StartCoroutine(ProcessInput(action, CurrentCharacter, result));
            while (result.Success != ActionResult.ResultType.TurnTaken)
            {
                result = new ActionResult();
                yield return CoroutineManager.Instance.StartCoroutine(ProcessInput(new GameAction(GameAction.ActionType.Wait, Dir8.None), CurrentCharacter, result));
            }
        }

        public bool IsPlayerLeaderTurn()
        {
            return ZoneManager.Instance.CurrentMap.ActiveTeam.Leader == CurrentCharacter;
        }

        public bool IsPlayerTurn()
        {
            if (DataManager.Instance.Save.TeamMode && ActiveTeam.Players.Contains(CurrentCharacter))
                return true;

            return IsPlayerLeaderTurn();
        }

        public bool IsGameOver()
        {
            //leader is dead
            foreach (Character character in ZoneManager.Instance.CurrentMap.ActiveTeam.Players)
            {
                if (!character.Dead)
                    return false;
            }
            return true;
        }

        public IEnumerator<YieldInstruction> SuspendGame()
        {
            GameManager.Instance.BGM("", true);

            yield return CoroutineManager.Instance.StartCoroutine(GameManager.Instance.FadeOut(false));

            //compute and update the current session time.  the value in the gameprogress wont matter, but we are just using this function to get the result value.
            DataManager.Instance.Save.EndSession();
            DataManager.Instance.SaveSessionTime(DataManager.Instance.Save.SessionTime);
            //TODO: resolve all Nonserialized variables (such as in mapgen, AI) being inconsistent before enabling this.
            //DataManager.Instance.LogQuicksave();
            DataManager.Instance.SuspendPlay();

            MenuBase.Transparent = false;

            GameManager.Instance.SceneOutcome = GameManager.Instance.RestartToTitle();
        }

        public IEnumerator<YieldInstruction> ProcessRescue(GameAction action, SOSMail mail)
        {
            //delete all enemies
            for (int ii = ZoneManager.Instance.CurrentMap.MapTeams.Count - 1; ii >= 0; ii--)
            {
                for (int jj = ZoneManager.Instance.CurrentMap.MapTeams[ii].Players.Count - 1; jj >= 0; jj--)
                    yield return CoroutineManager.Instance.StartCoroutine(ZoneManager.Instance.CurrentMap.MapTeams[ii].Players[jj].DieSilent());
                for (int jj = ZoneManager.Instance.CurrentMap.MapTeams[ii].Guests.Count - 1; jj >= 0; jj--)
                    yield return CoroutineManager.Instance.StartCoroutine(ZoneManager.Instance.CurrentMap.MapTeams[ii].Guests[jj].DieSilent());
            }


            //heal players
            foreach (Character character in ActiveTeam.IterateMainByRank())
            {
                if (character.Dead)
                {
                    Loc? endLoc = ZoneManager.Instance.CurrentMap.GetClosestTileForChar(character, ActiveTeam.Leader.CharLoc);
                    if (endLoc == null)
                        endLoc = ActiveTeam.Leader.CharLoc;
                    character.CharLoc = endLoc.Value;

                    character.HP = character.MaxHP;
                    character.Dead = false;
                    character.DefeatAt = "";

                    ZoneManager.Instance.CurrentMap.UpdateExploration(character);
                }
            }

            ZoneManager.Instance.CurrentMap.CurrentTurnMap.CurrentOrder = new TurnOrder(0, Faction.Player, 0);
            ResetRound();
            RegenerateTurnMap();

            RemoveDeadTeams();

            DataManager.Instance.Save.RescuesLeft--;
            //fade white back with music

            //remove reward item
            MapItem offeredItem = new MapItem();
            int nn = 0;
            offeredItem.IsMoney = (action[nn] == 1);
            nn++;
            int itemLength = action[nn];
            nn++;
            string itemName = "";
            for (int ii = 0; ii < itemLength; ii++)
                itemName += (char)action[nn + ii];
            nn += itemLength;
            offeredItem.Value = itemName;

            itemLength = action[nn];
            nn++;
            itemName = "";
            for (int ii = 0; ii < itemLength; ii++)
                itemName += (char)action[nn + ii];
            nn += itemLength;
            offeredItem.HiddenValue = itemName;

            offeredItem.Amount = action[nn];
            nn++;


            if (offeredItem.IsMoney)
                ActiveTeam.Bank -= offeredItem.Amount;
            else if (!String.IsNullOrEmpty(offeredItem.Value))
            {
                ItemData entry = DataManager.Instance.GetItem(offeredItem.Value);
                if (entry.MaxStack > 1)
                {
                    List<WithdrawSlot> itemsToTake = new List<WithdrawSlot>();
                    for (int ii = 0; ii < offeredItem.Amount; ii++)
                        itemsToTake.Add(new WithdrawSlot(false, offeredItem.Value, 0));
                    ActiveTeam.TakeItems(itemsToTake);
                }
                else if (entry.UsageType == ItemData.UseType.Box)
                {
                    int chosenIndex = 0;
                    for (int ii = 0; ii < ActiveTeam.BoxStorage.Count; ii++)
                    {
                        if (ActiveTeam.BoxStorage[ii].ID == offeredItem.Value
                            && ActiveTeam.BoxStorage[ii].HiddenValue == offeredItem.HiddenValue)
                        {
                            chosenIndex = ii;
                            break;
                        }
                    }
                    List<WithdrawSlot> itemsToTake = new List<WithdrawSlot>();
                    itemsToTake.Add(new WithdrawSlot(true, "", chosenIndex));
                    ActiveTeam.TakeItems(itemsToTake);
                }
                else
                {
                    List<WithdrawSlot> itemsToTake = new List<WithdrawSlot>();
                    itemsToTake.Add(new WithdrawSlot(false, offeredItem.Value, 0));
                    ActiveTeam.TakeItems(itemsToTake);
                }
            }

            int nameLength = action[nn];
            nn++;
            string name = "";
            for (int ii = 0; ii < nameLength; ii++)
                name += (char)action[nn + ii];

            yield return CoroutineManager.Instance.StartCoroutine(ZoneManager.Instance.CurrentZone.OnRescued(name, mail));

            yield return CoroutineManager.Instance.StartCoroutine(ProcessTurnStart(CurrentCharacter));

        }

        public IEnumerator<YieldInstruction> ProcessPlayerInput(GameAction action)
        {
            if (action.Dir == Dir8.None)
                action.Dir = CurrentCharacter.CharDir;

            //extraneous directions are removed

            DataManager.Instance.QueueLogUI();
            ActionResult result = new ActionResult();//denotes if a turn was taken
            yield return CoroutineManager.Instance.StartCoroutine(ProcessInput(action, CurrentCharacter, result));

            if (result.Success != ActionResult.ResultType.Fail)
            {
                //log the turn and reset inputs
                DataManager.Instance.LogPlay(action);
                DataManager.Instance.DequeueLogUI();
            }
            else if (DataManager.Instance.CurrentReplay != null)
            {
                DiagManager.Instance.LogError(new InvalidOperationException(String.Format("Recorded action failed: {0}", action.ToString())));
            }
        }

        //the intention, and its result to that frame
        //"choose the action to partake in"
        public IEnumerator<YieldInstruction> ProcessInput(GameAction action, Character character, ActionResult result)
        {
            //translates commands into actions
            if (character.WaitToAttack && character.CantWalk && action.Type == GameAction.ActionType.Wait)
                action = new GameAction(GameAction.ActionType.Attack, action.Dir);

            ProcessDir(action.Dir, character);

            switch (action.Type)
            {
                case GameAction.ActionType.Dir:
                    {
                        //result.Success = ActionResult.ResultType.Success;
                        break;
                    }
                case GameAction.ActionType.Wait:
                    {
                        result.Success = ActionResult.ResultType.TurnTaken;

                        //if it's a team character and it's team mode, wait a little while
                        if (DataManager.Instance.Save.TeamMode && character.MemberTeam == ActiveTeam)
                        {
                            DungeonScene.Instance.LogMsg(Text.FormatKey("MSG_SKIP_TURN", character.GetDisplayName(false)), false, true);
                            yield return new WaitForFrames(GameManager.Instance.ModifyBattleSpeed(20));
                        }
                        else if (character == FocusedCharacter)//add just one little wait to slow down the turn-passing when no enemies are in view
                            yield return new WaitForFrames(1);//this will cause 1-frame breaks when waiting with walking characters in view, but it's barely noticable

                        yield return CoroutineManager.Instance.StartCoroutine(FinishTurn(character, true, false, false));
                        break;
                    }
                case GameAction.ActionType.Move:
                    {
                        yield return CoroutineManager.Instance.StartCoroutine(ProcessWalk(character, (action[0] == 1), result));
                        break;
                    }
                case GameAction.ActionType.Pickup:
                    {
                        yield return CoroutineManager.Instance.StartCoroutine(ProcessPickup(character, result));
                        break;
                    }
                case GameAction.ActionType.Drop:
                    {
                        //takes an index argument
                        //[0] = item slot to use (-1 for the held item)
                        yield return CoroutineManager.Instance.StartCoroutine(ProcessPlaceItem(character, action[0], result));
                        break;
                    }
                case GameAction.ActionType.Give:
                    {
                        //[0] = item slot to use (-1 for the ground item)
                        //[1] = who to give it to (-1 for the user)
                        yield return CoroutineManager.Instance.StartCoroutine(ProcessGiveItem(character, action[0], action[1], result));
                        break;
                    }
                case GameAction.ActionType.Take:
                    {
                        //[0] = team slot to take from
                        yield return CoroutineManager.Instance.StartCoroutine(ProcessTakeItem(character, action[0], result));
                        break;
                    }
                case GameAction.ActionType.Tile:
                    {
                        yield return CoroutineManager.Instance.StartCoroutine(ProcessTileInteract(character, (action[0] == 1), result));
                        break;
                    }
                case GameAction.ActionType.Attack:
                    {
                        yield return CoroutineManager.Instance.StartCoroutine(ProcessObjectInteract(character, result));
                        break;
                    }
                case GameAction.ActionType.UseSkill:
                    {
                        yield return CoroutineManager.Instance.StartCoroutine(ProcessUseSkill(character, action[0], result));
                        break;
                    }
                case GameAction.ActionType.UseItem:
                    {
                        //[0] = item slot to use (-1 for held item, -2 for the ground item)
                        //[1] = who to use it on (-1 for the user)
                        //others: which slot to delete,
                        //which intrinsic to have, which team member/item to send in, etc.
                        yield return CoroutineManager.Instance.StartCoroutine(ProcessUseItem(character, action[0], action[1], result));
                        break;
                    }
                case GameAction.ActionType.Throw:
                    {
                        //[0] = item slot to use (-1 for held item, -2 for the ground item)
                        yield return CoroutineManager.Instance.StartCoroutine(ProcessThrowItem(character, action[0], result));
                        break;
                    }
                case GameAction.ActionType.TeamMode:
                    {
                        yield return CoroutineManager.Instance.StartCoroutine(ToggleTeamMode(result));
                        break;
                    }
                case GameAction.ActionType.ShiftTeam:
                    {
                        result.Success = ActionResult.ResultType.Success;

                        SwitchTeam(action[0]);
                        break;
                    }
                case GameAction.ActionType.SetLeader:
                    {
                        yield return CoroutineManager.Instance.StartCoroutine(MakeLeader(action[0], result));
                        break;
                    }
                case GameAction.ActionType.SendHome:
                    {
                        result.Success = ActionResult.ResultType.Success;

                        yield return CoroutineManager.Instance.StartCoroutine(SendHome(action[0]));
                        break;
                    }
                case GameAction.ActionType.GiveUp:
                    {
                        result.Success = ActionResult.ResultType.Success;

                        GameManager.Instance.SceneOutcome = GameManager.Instance.EndSegment((GameProgress.ResultType)action[0]);
                        break;
                    }
                case GameAction.ActionType.Tactics:
                    {
                        result.Success = ActionResult.ResultType.Success;

                        List<string> eligibleAI = new List<string>();
                        foreach (string ai_asset in DataManager.Instance.DataIndices[DataManager.DataType.AI].GetOrderedKeys(true))
                        {
                            AIEntrySummary summary = DataManager.Instance.DataIndices[DataManager.DataType.AI].Get(ai_asset) as AIEntrySummary;
                            if (summary.Assignable)
                                eligibleAI.Add(ai_asset);
                        }

                        //saves all the settings to the characters
                        for (int ii = 0; ii < ActiveTeam.Players.Count; ii++)
                        {
                            int choice = action[ii];
                            Character target = ActiveTeam.Players[ii];
                            AITactic tactic = DataManager.Instance.GetAITactic(eligibleAI[choice]);
                            if (tactic.ID != target.Tactic.ID)
                                target.Tactic = new AITactic(tactic);
                            target.Tactic.Initialize(target);
                        }
                        break;
                    }
                case GameAction.ActionType.SetSkill:
                    {
                        result.Success = ActionResult.ResultType.Success;

                        Skill skill = ActiveTeam.Players[action[0]].Skills[action[1]].Element;
                        skill.Enabled = !skill.Enabled;
                        break;
                    }
                case GameAction.ActionType.ShiftSkill:
                    {
                        result.Success = ActionResult.ResultType.Success;
                        Character targetChar = ActiveTeam.Players[action[0]];
                        int slot = action[1];
                        targetChar.SwitchSkills(slot);
                        break;
                    }
                case GameAction.ActionType.SortItems:
                    {
                        result.Success = ActionResult.ResultType.Success;

                        ActiveTeam.SortItems();
                        break;
                    }
                case GameAction.ActionType.Option:
                    {
                        result.Success = ActionResult.ResultType.Success;

                        DataManager.Instance.Save.DefaultSkill = (Settings.SkillDefault)action[0];
                        break;
                    }
                default:
                    {
                        throw new Exception("Undefined Command: " + action.Type);
                    }
            }
        }

        //anyone can be added to or removed from the entity list at any time in a turn
        //when dealing with death disposals, it happens in a turn transition
        //when dealing with recruitment or summoning, it happens in mid-turn
        public void AddTeam(Faction faction, Team team)
        {
            if (faction == Faction.Player)
                throw new Exception("Can't add player team!");

            switch (faction)
            {
                case Faction.Foe:
                    ZoneManager.Instance.CurrentMap.MapTeams.Add(team);
                    break;
                case Faction.Friend:
                    ZoneManager.Instance.CurrentMap.AllyTeams.Add(team);
                    break;
            }
        }
        public void RemoveTeam(Faction faction, int teamIndex)
        {
            CharIndex turnChar = ZoneManager.Instance.CurrentMap.CurrentTurnMap.GetCurrentTurnChar();
            //note: this cannot be called when it is the team's turn
            if (turnChar.Faction == faction && turnChar.Team == teamIndex)
                throw new Exception("Can't remove a team whose turn it currently is!");
            if (faction == Faction.Player)
                throw new Exception("Can't remove player team!");

            switch (faction)
            {
                case Faction.Foe:
                    ZoneManager.Instance.CurrentMap.MapTeams.RemoveAt(teamIndex);
                    break;
                case Faction.Friend:
                    ZoneManager.Instance.CurrentMap.AllyTeams.RemoveAt(teamIndex);
                    break;
            }

            ZoneManager.Instance.CurrentMap.CurrentTurnMap.UpdateTeamRemoval(faction, teamIndex);
        }

        public void AddCharToTeam(Faction faction, int teamIndex, bool guest, Character character)
        {
            Team team = ZoneManager.Instance.CurrentMap.GetTeam(faction, teamIndex);

            if (guest)
                team.Guests.Add(character);
            else
                team.Players.Add(character);

            OnCharAdd(character);
        }

        public void OnCharAdd(Character newChar)
        {
            newChar.TurnWait = 0;
            newChar.TiersUsed = 0;
            newChar.TurnUsed = false;
        }

        public void RemoveChar(Character character)
        {
            RemoveChar(ZoneManager.Instance.CurrentMap.GetCharIndex(character));
        }

        public void RemoveChar(CharIndex charIndex)
        {
            CharIndex turnChar = ZoneManager.Instance.CurrentMap.CurrentTurnMap.GetCurrentTurnChar();
            //note: this cannot be called when it is the character's turn
            if (turnChar == charIndex)
                throw new Exception("Attempted to delete a character on their turn.");

            Team team = ZoneManager.Instance.CurrentMap.GetTeam(charIndex.Faction, charIndex.Team);

            EventedList<Character> playerList = (charIndex.Guest) ? team.Guests : team.Players;
            Character character = playerList[charIndex.Char];

            character.OnRemove();
            playerList.RemoveAt(charIndex.Char);

            //update leader
            if (!charIndex.Guest)
            {
                if (charIndex.Char < team.LeaderIndex)
                    team.LeaderIndex--;
                if (team.LeaderIndex >= playerList.Count)
                    team.LeaderIndex = playerList.Count - 1;
                if (team.LeaderIndex < 0)
                    team.LeaderIndex = 0;
            }

            ZoneManager.Instance.CurrentMap.CurrentTurnMap.UpdateCharRemoval(charIndex);

            if (charIndex.Faction == Faction.Player)
            {
                if (focusedPlayerIndex > charIndex.Char)
                    focusedPlayerIndex--;
                if (focusedPlayerIndex >= playerList.Count)
                    focusedPlayerIndex = team.LeaderIndex;
            }

            //if the team is all empty (not all dead), definitely remove them
            if (team.Players.Count == 0 && team.Guests.Count == 0 && charIndex.Faction != Faction.Player)
                RemoveTeam(charIndex.Faction, charIndex.Team);
        }

        public bool CanUseTeamMode()
        {
            int totalAlive = 0;
            for (int ii = 0; ii < ActiveTeam.Players.Count; ii++)
            {
                if (!ActiveTeam.Players[ii].Dead)
                {
                    totalAlive++;
                    if (totalAlive > 1)
                        return true;
                }
            }
            return false;
        }

        public IEnumerator<YieldInstruction> ToggleTeamMode(ActionResult result)
        {
            GameManager.Instance.SE("Menu/Sort");
            if (CanUseTeamMode())
            {
                result.Success = ActionResult.ResultType.Success;

                SetTeamMode(!DataManager.Instance.Save.TeamMode);
            }
            else
                DungeonScene.Instance.LogMsg(Text.FormatKey("MSG_TEAM_MODE_ALONE"), false, true);
            yield break;
        }

        public void SetTeamMode(bool mode)
        {
            if (DataManager.Instance.Save.TeamMode != mode)
            {
                DataManager.Instance.Save.TeamMode = mode;
                TeamModeNote.SetTeamMode(DataManager.Instance.Save.TeamMode);
            }
        }

        private bool canSwitchToChar(int charIndex)
        {
            Character character = ActiveTeam.Players[charIndex];
            if (character.Dead)
                return false;
            if (!ZoneManager.Instance.CurrentMap.CurrentTurnMap.IsEligibleToMove(character))
                return false;
            return true;
        }

        public IEnumerator<YieldInstruction> MakeLeader(int charIndex, ActionResult result)
        {
            if (CurrentCharacter != ActiveTeam.Leader)
            {
                GameManager.Instance.SE("Menu/Cancel");
                DungeonScene.Instance.LogMsg(Text.FormatKey("MSG_LEADER_SWAP_REQ"), false, true);
                yield return new WaitForFrames(GameManager.Instance.ModifyBattleSpeed(10));
            }
            else if (charIndex >= ActiveTeam.Players.Count || ActiveTeam.LeaderIndex == charIndex)
            {
                GameManager.Instance.SE("Menu/Cancel");
                yield return new WaitForFrames(GameManager.Instance.ModifyBattleSpeed(10));
            }
            else if (ZoneManager.Instance.CurrentMap.NoSwitching || DataManager.Instance.Save.NoSwitching)
            {
                GameManager.Instance.SE("Menu/Cancel");
                DungeonScene.Instance.LogMsg(Text.FormatKey("MSG_CANT_SWAP_LEADER", CurrentCharacter.GetDisplayName(true)), false, true);
                yield return new WaitForFrames(GameManager.Instance.ModifyBattleSpeed(10));
            }
            else
            {
                if (!canSwitchToChar(charIndex))
                    GameManager.Instance.SE("Menu/Cancel");
                else
                {
                    result.Success = ActionResult.ResultType.Success;

                    //change the leader index
                    int oldLeader = ActiveTeam.LeaderIndex;
                    ActiveTeam.LeaderIndex = charIndex;

                    //re-order map order as well
                    ZoneManager.Instance.CurrentMap.CurrentTurnMap.AdjustLeaderSwap(Faction.Player, 0, false, oldLeader, charIndex);
                    ReloadFocusedPlayer();

                    DungeonScene.Instance.LogMsg(Text.FormatKey("MSG_LEADER_SWAP", ActiveTeam.Leader.GetDisplayName(true)));

                    GameManager.Instance.SE(GraphicsManager.LeaderSE);
                    yield return new WaitForFrames(GameManager.Instance.ModifyBattleSpeed(10));
                    yield return CoroutineManager.Instance.StartCoroutine(SpecialIntro(ActiveTeam.Leader));
                }
            }
        }

        public IEnumerator<YieldInstruction> SpecialIntro(Character chara)
        {
            if (chara.Dead)
                yield break;

            SkinData skin = DataManager.Instance.GetSkin(chara.Appearance.Skin);
            yield return CoroutineManager.Instance.StartCoroutine(ProcessBattleFX(chara, chara, skin.LeaderFX));
        }

        public void SwitchTeam(int charIndex)
        {
            if (CurrentCharacter != ActiveTeam.Leader)
            {
                //this shouldn't be reached
                throw new Exception("Attempted to reorder team on incorrect turn.");
            }
            else
            {
                Character character = ActiveTeam.Players[charIndex];
                //move this character with the character in front
                ActiveTeam.Players.RemoveAt(charIndex);
                ActiveTeam.Players.Insert(charIndex+1, character);

                ZoneManager.Instance.CurrentMap.CurrentTurnMap.AdjustSlotSwap(Faction.Player, 0, false, charIndex, charIndex+1);

                //update the leader indices
                if (ActiveTeam.LeaderIndex == charIndex)
                {
                    ActiveTeam.LeaderIndex++;
                    ZoneManager.Instance.CurrentMap.CurrentTurnMap.AdjustLeaderSwap(Faction.Player, 0, false, charIndex, ActiveTeam.LeaderIndex);
                }
                else if (ActiveTeam.LeaderIndex == charIndex + 1)
                {
                    ActiveTeam.LeaderIndex--;
                    ZoneManager.Instance.CurrentMap.CurrentTurnMap.AdjustLeaderSwap(Faction.Player, 0, false, charIndex + 1, ActiveTeam.LeaderIndex);
                }

                ReloadFocusedPlayer();
            }
        }

        private int getLiveIndex(EventedList<Character> playerList)
        {
            for (int ii = 0; ii < playerList.Count; ii++)
            {
                if (!playerList[ii].Dead)
                    return ii;
            }
            return -1;
        }

        private void genericReorderDeadPlayerList(Faction faction, int teamIndex)
        {
            Team team = ZoneManager.Instance.CurrentMap.GetTeam(faction, teamIndex);

            int liveIndex = getLiveIndex(team.Players);

            if (liveIndex != -1 && team.Leader.Dead)
            {
                //switch leader to this
                int oldLeader = team.LeaderIndex;
                team.LeaderIndex = liveIndex;

                //re-order map order as well
                ZoneManager.Instance.CurrentMap.CurrentTurnMap.AdjustLeaderSwap(faction, teamIndex, false, oldLeader, liveIndex);
            }
        }

        public void ReorderDeadLeaders()
        {
            {
                Team team = ZoneManager.Instance.CurrentMap.ActiveTeam;
                int liveIndex = getLiveIndex(team.Players);

                if (liveIndex != -1 && team.Leader.Dead)
                {
                    //switch leader to this
                    int oldLeader = team.LeaderIndex;
                    team.LeaderIndex = liveIndex;

                    //re-order map order as well
                    ZoneManager.Instance.CurrentMap.CurrentTurnMap.AdjustLeaderSwap(Faction.Player, 0, false, oldLeader, liveIndex);

                    LogMsg(Text.FormatKey("MSG_LEADER_SWAP", team.Leader.GetDisplayName(true)));
                }
            }

            foreach(CharIndex deadTeamIdx in ZoneManager.Instance.CurrentMap.TeamsWithDead)
                genericReorderDeadPlayerList(deadTeamIdx.Faction, deadTeamIdx.Team);
            ZoneManager.Instance.CurrentMap.TeamsWithDead.Clear();
        }


        private int deadTeamIndexCompare(CharIndex key1, CharIndex key2)
        {
            int cmp = Math.Sign((int)key2.Faction - (int)key1.Faction);
            if (cmp != 0)
                return cmp;
            return Math.Sign(key2.Team - key1.Team);
        }

        public void RemoveDeadTeams()
        {
            List<CharIndex> sortedTeams = new List<CharIndex>();
            foreach (CharIndex deadTeamIdx in ZoneManager.Instance.CurrentMap.DeadTeams)
                sortedTeams.Add(deadTeamIdx);
            sortedTeams.Sort(deadTeamIndexCompare);
            foreach (CharIndex deadTeamIdx in sortedTeams)
            {
                Team deadTeam = ZoneManager.Instance.CurrentMap.GetTeam(deadTeamIdx.Faction, deadTeamIdx.Team);
                for (int jj = deadTeam.Guests.Count - 1; jj >= 0; jj--)
                    RemoveChar(new CharIndex(deadTeamIdx.Faction, deadTeamIdx.Team, true, jj));
                for (int jj = deadTeam.Players.Count - 1; jj >= 0; jj--)
                    RemoveChar(new CharIndex(deadTeamIdx.Faction, deadTeamIdx.Team, false, jj));
            }
            ZoneManager.Instance.CurrentMap.DeadTeams.Clear();
        }

        public int GetSpeedMult(Character movingChar, Loc destTile)
        {
            if (RunMode)
                return 12;

            Loc seen = Character.GetSightDims();
            Rect sightBounds = new Rect(FocusedCharacter.CharLoc - seen, seen * 2 + Loc.One);
            sightBounds = ZoneManager.Instance.CurrentMap.GetClampedSight(sightBounds);

            if (!ZoneManager.Instance.CurrentMap.InBounds(sightBounds, movingChar.CharLoc) && !ZoneManager.Instance.CurrentMap.InBounds(sightBounds, destTile))
                return 0;

            //character walks into vision
            //character walks out of vision
            //character and player walk into vision
            //character and player walk out of vision
            //character chasing player at vision border = keep visible
            //player chasing character at vision border = keep visible

            //Map.SightRange sight = FocusedCharacter.GetCharSight();
            //if (!FocusedCharacter.CanSeeLoc(movingChar.CharLoc, sight) && !FocusedCharacter.CanSeeLoc(destTile, sight))
            //    return 0;

            return 1;
        }

        public IEnumerator<YieldInstruction> MoveToUsableTurn(bool action, bool walked)
        {
            CurrentCharacter.TurnWait = (walked ? -CurrentCharacter.MovementSpeed : 0) + 1;
            CurrentCharacter.TiersUsed++;
            CurrentCharacter.TurnUsed = action;

            if (!IsGameOver())
            {

                ReorderDeadLeaders();

                do
                {
                    //move to next turn
                    ZoneManager.Instance.CurrentMap.CurrentTurnMap.CurrentOrder.TurnIndex++;

                    //if we're over he index of the current faction list, it means we must move to the next faction
                    while (ZoneManager.Instance.CurrentMap.CurrentTurnMap.CurrentOrder.TurnIndex >= ZoneManager.Instance.CurrentMap.CurrentTurnMap.TurnToChar.Count)
                    {
                        ZoneManager.Instance.CurrentMap.CurrentTurnMap.CurrentOrder.Faction = (Faction)(((int)ZoneManager.Instance.CurrentMap.CurrentTurnMap.CurrentOrder.Faction + 1) % 3);
                        ZoneManager.Instance.CurrentMap.CurrentTurnMap.CurrentOrder.TurnIndex = 0;

                        //if we looped back to the player faction, it means we must move on to the next turn tier
                        if (ZoneManager.Instance.CurrentMap.CurrentTurnMap.CurrentOrder.Faction == Faction.Player)
                        {
                            ZoneManager.Instance.CurrentMap.CurrentTurnMap.CurrentOrder.TurnTier++;

                            //if we're on the last turn tier, we loop back to the first one
                            if (ZoneManager.Instance.CurrentMap.CurrentTurnMap.CurrentOrder.TurnTier >= 6)
                            {
                                ResetRound();

                                yield return CoroutineManager.Instance.StartCoroutine(ProcessMapTurnEnd());
                            }
                        }

                        if (IsGameOver())
                            yield break;
                        //if we've switched to the other side, order the turns again
                        RegenerateTurnMap();

                        //if turn starts were to happen in a group, they would all occur in character-index sequence here
                        //check each character in the sequence to ensure they are acting in this turn
                        //what about sped-up characters?
                        //should they not be allowed to take their turn? (do not add them to list)

                        //what about revived characters?
                        //should they not be allowed to take their turn? (do not add them to list)

                    }

                }
                while (!ZoneManager.Instance.CurrentMap.CurrentTurnMap.IsEligibleToMove(CurrentCharacter));

                //reorder again in case something happened to deaths.
                ReorderDeadLeaders();

                RemoveDeadTeams();

                if (!IsPlayerTurn())
                    OrganizeAIMovement(0);

                yield return CoroutineManager.Instance.StartCoroutine(ProcessTurnStart(CurrentCharacter));
            }
        }

        public void ResetTurns()
        {
            //silently set team mode to false
            DataManager.Instance.Save.TeamMode = false;
            ResetRound();
            //silently reset turn map
            ZoneManager.Instance.CurrentMap.CurrentTurnMap = new TurnState();
            RegenerateTurnMap();
            ReloadFocusedPlayer();
        }

        public void ReloadFocusedPlayer()
        {
            focusedPlayerIndex = ZoneManager.Instance.CurrentMap.CurrentTurnMap.GetCurrentTurnChar().Char;
        }

        public void RegenerateTurnMap()
        {
            //beginning of a faction's turn; create new turn map
            ZoneManager.Instance.CurrentMap.CurrentTurnMap.TurnToChar.Clear();
            switch (ZoneManager.Instance.CurrentMap.CurrentTurnMap.CurrentOrder.Faction)
            {
                case Faction.Foe:
                    {
                        for (int ii = 0; ii < ZoneManager.Instance.CurrentMap.MapTeams.Count; ii++)
                            ZoneManager.Instance.CurrentMap.CurrentTurnMap.LoadTeamTurnMap(Faction.Foe, ii, ZoneManager.Instance.CurrentMap.MapTeams[ii]);
                    }
                    break;
                case Faction.Friend:
                    {
                        for (int ii = 0; ii < ZoneManager.Instance.CurrentMap.AllyTeams.Count; ii++)
                            ZoneManager.Instance.CurrentMap.CurrentTurnMap.LoadTeamTurnMap(Faction.Friend, ii, ZoneManager.Instance.CurrentMap.AllyTeams[ii]);
                    }
                    break;
                default:
                    ZoneManager.Instance.CurrentMap.CurrentTurnMap.LoadTeamTurnMap(Faction.Player, 0, ZoneManager.Instance.CurrentMap.ActiveTeam);
                    break;
            }
        }

        public void SkipRemainingTurns()
        {
            ZoneManager.Instance.CurrentMap.CurrentTurnMap.SetTeamRound(ZoneManager.Instance.CurrentMap.ActiveTeam, false);
            for (int ii = 0; ii < ZoneManager.Instance.CurrentMap.AllyTeams.Count; ii++)
                ZoneManager.Instance.CurrentMap.CurrentTurnMap.SetTeamRound(ZoneManager.Instance.CurrentMap.AllyTeams[ii], false);
            for (int ii = 0; ii < ZoneManager.Instance.CurrentMap.MapTeams.Count; ii++)
                ZoneManager.Instance.CurrentMap.CurrentTurnMap.SetTeamRound(ZoneManager.Instance.CurrentMap.MapTeams[ii], false);
        }

        public void ResetRound()
        {
            ZoneManager.Instance.CurrentMap.CurrentTurnMap.CurrentOrder.TurnTier = 0;

            ZoneManager.Instance.CurrentMap.CurrentTurnMap.SetTeamRound(ZoneManager.Instance.CurrentMap.ActiveTeam, true);
            for (int ii = 0; ii < ZoneManager.Instance.CurrentMap.AllyTeams.Count; ii++)
                ZoneManager.Instance.CurrentMap.CurrentTurnMap.SetTeamRound(ZoneManager.Instance.CurrentMap.AllyTeams[ii], true);
            for (int ii = 0; ii < ZoneManager.Instance.CurrentMap.MapTeams.Count; ii++)
                ZoneManager.Instance.CurrentMap.CurrentTurnMap.SetTeamRound(ZoneManager.Instance.CurrentMap.MapTeams[ii], true);
        }

        private void OrganizeAIMovement(int depth)
        {
            //here, we check to see if this AI wants to move, and if its ideal movement is blocked
            GameAction intendedInput = CurrentCharacter.Tactic.GetAction(CurrentCharacter, DataManager.Instance.Save.Rand, true);

            if (intendedInput.Type == GameAction.ActionType.Move)
            {
                Loc dest = CurrentCharacter.CharLoc + intendedInput.Dir.GetLoc();
                Character blockingChar = ZoneManager.Instance.CurrentMap.GetCharAtLoc(dest);
                if (blockingChar != null && ZoneManager.Instance.CurrentMap.CurrentTurnMap.IsEligibleToMove(blockingChar))
                {
                    //they need to be later in index than the current order + depth
                    int blockingTurn = ZoneManager.Instance.CurrentMap.CurrentTurnMap.CurrentOrder.TurnIndex;
                    for (int ii = ZoneManager.Instance.CurrentMap.CurrentTurnMap.CurrentOrder.TurnIndex + 1; ii < ZoneManager.Instance.CurrentMap.CurrentTurnMap.TurnToChar.Count; ii++)
                    {
                        CharIndex turnChar = ZoneManager.Instance.CurrentMap.CurrentTurnMap.TurnToChar[ii];
                        if (blockingChar == ZoneManager.Instance.CurrentMap.LookupCharIndex(turnChar))
                        {
                            blockingTurn = ii;
                            break;
                        }
                    }

                    //pull them forward in the turn queue.  note: this changes the current character
                    if (blockingTurn > ZoneManager.Instance.CurrentMap.CurrentTurnMap.CurrentOrder.TurnIndex + depth)
                    {
                        CharIndex newTurnChar = ZoneManager.Instance.CurrentMap.CurrentTurnMap.TurnToChar[blockingTurn];
                        ZoneManager.Instance.CurrentMap.CurrentTurnMap.TurnToChar.RemoveAt(blockingTurn);
                        ZoneManager.Instance.CurrentMap.CurrentTurnMap.TurnToChar.Insert(ZoneManager.Instance.CurrentMap.CurrentTurnMap.CurrentOrder.TurnIndex, newTurnChar);

                        //then, recursively check to see if they are blocked themselves
                        OrganizeAIMovement(depth + 1);
                    }
                }
            }
        }

        private IEnumerator<YieldInstruction> ProcessMapTurnEnd()
        {
            SingleCharContext context = new SingleCharContext(null);
            //turn ends for all
            EventEnqueueFunction<SingleCharEvent> function = (StablePriorityQueue<GameEventPriority, EventQueueElement<SingleCharEvent>> queue, Priority maxPriority, ref Priority nextPriority) =>
            {
                //start with universal
                DataManager.Instance.UniversalEvent.AddEventsToQueue(queue, maxPriority, ref nextPriority, DataManager.Instance.UniversalEvent.OnMapTurnEnds, null);
                ZoneManager.Instance.CurrentMap.MapEffect.AddEventsToQueue(queue, maxPriority, ref nextPriority, ZoneManager.Instance.CurrentMap.MapEffect.OnMapTurnEnds, null);


                foreach (MapStatus mapStatus in ZoneManager.Instance.CurrentMap.Status.Values)
                {
                    MapStatusData entry = DataManager.Instance.GetMapStatus(mapStatus.ID);
                    mapStatus.AddEventsToQueue<SingleCharEvent>(queue, maxPriority, ref nextPriority, entry.OnMapTurnEnds, null);
                }

                int portPriority = 0;
                foreach (Character character in ZoneManager.Instance.CurrentMap.IterateCharacters())
                {
                    if (!character.Dead)
                    {
                        foreach (PassiveContext effectContext in character.IteratePassives(new Priority(portPriority)))
                            effectContext.AddEventsToQueue(queue, maxPriority, ref nextPriority, effectContext.EventData.OnMapTurnEnds, character);
                    }
                    portPriority++;
                }
            };
            foreach (EventQueueElement<SingleCharEvent> effect in IterateEvents<SingleCharEvent>(function))
            {
                context.User = effect.TargetChar;
                yield return CoroutineManager.Instance.StartCoroutine(effect.Event.Apply(effect.Owner, effect.OwnerChar, context));
                //normally, we would check for cancel, but I'm not sure if it's ideal to stop it for everyone here.
            }


            ZoneManager.Instance.CurrentMap.MapTurns++;
            DataManager.Instance.Save.TotalTurns++;

            //check EXP because someone could've died
            yield return CoroutineManager.Instance.StartCoroutine(CheckEXP());

            ////increment time for zone manager
            //ZoneManager.TimeOfDay prevTime = DataManager.Instance.Save.Time;
            //ZoneManager.Instance.TimeCycle = (DataManager.Instance.Save.TimeCycle + 1) % (2 * (DataManager.MAJOR_TIME_DUR + DataManager.MINOR_TIME_DUR));
            //if (prevTime != DataManager.Instance.Save.Time)
            //DataManager.Instance.Save.LogMsg(Text.FormatKey("MENU_TIME_OF_DAY", DataManager.Instance.Save.Time));
        }


        private IEnumerator<YieldInstruction> ProcessTurnStart(Character character)
        {
            if (!character.Dead)
            {
                SingleCharContext context = new SingleCharContext(character);
                //process turn start (assume live characters)
                yield return CoroutineManager.Instance.StartCoroutine(character.OnTurnStart(context));
            }

        }

    }
}
