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
            EnterFloor(ZoneManager.Instance.CurrentMap.EntryPoints[entryPointIndex]);
        }
        public void EnterFloor(LocRay8 entryPoint)
        {
            //put the carry-overs in the new map
            foreach (MapStatus status in ZoneManager.Instance.CurrentZone.CarryOver)
                ZoneManager.Instance.CurrentMap.Status.Add(status.ID, status);
            ZoneManager.Instance.CurrentZone.CarryOver.Clear();

            ZoneManager.Instance.CurrentMap.EnterMap(DataManager.Instance.Save.ActiveTeam, entryPoint);

            ResetFloor();
        }

        public override void Exit()
        {
            if (ZoneManager.Instance.CurrentMap != null)
            {
                //remove statuses
                foreach (Character character in ZoneManager.Instance.CurrentMap.ActiveTeam.Players)
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
                ResetAnims();

                //Notify script engine
                LuaEngine.Instance.OnDungeonFloorEnd();
            }
        }

        public void ResetFloor()
        {
            ZoneManager.Instance.CurrentMap.CurrentTurnMap = new TurnState();
            RegenerateTurnMap();

            focusedPlayerIndex = ZoneManager.Instance.CurrentMap.CurrentTurnMap.GetCurrentTurnChar().Char;

            //refresh everyone's traits
            foreach (Character character in ZoneManager.Instance.CurrentMap.IterateCharacters())
                character.RefreshTraits();

            GraphicsManager.GlobalIdle = GraphicsManager.IdleAction;

        }

        public IEnumerator<YieldInstruction> PrepareFloor()
        {
            DataManager.Instance.Save.Trail.Add(ZoneManager.Instance.CurrentMap.GetSingleLineName());
            LogMsg(Text.FormatKey("MSG_ENTER_MAP", ActiveTeam.GetReferenceName(), ZoneManager.Instance.CurrentMap.GetSingleLineName()), true, false);

            //start emitters for existing map status
            foreach (MapStatus mapStatus in ZoneManager.Instance.CurrentMap.Status.Values)
                mapStatus.StartEmitter(Anims);

            //process events before the map fades in
            foreach (SingleCharEvent effect in ZoneManager.Instance.CurrentMap.PrepareEvents)
                yield return CoroutineManager.Instance.StartCoroutine(effect.Apply(null, null, FocusedCharacter));

            //Notify script engine
             LuaEngine.Instance.OnDungeonFloorPrepare();
        }

        public IEnumerator<YieldInstruction> BeginFloor()
        {
            ZoneManager.Instance.CurrentMap.Begun = true;
            //process map-start events (dialogue, map condition announcement, etc)
            foreach (SingleCharEvent effect in ZoneManager.Instance.CurrentMap.StartEvents)
                yield return CoroutineManager.Instance.StartCoroutine(effect.Apply(null, null, FocusedCharacter));

            foreach (Character character in ActiveTeam.Players)
                yield return CoroutineManager.Instance.StartCoroutine(SpecialIntro(character));

            //process player happenings
            foreach (Character character in ZoneManager.Instance.CurrentMap.IterateCharacters())
            {
                character.Tactic.Initialize(character);
                if (!character.Dead)
                    yield return CoroutineManager.Instance.StartCoroutine(character.OnMapStart());
            }

            //map starts for map statuses
            EventEnqueueFunction<SingleCharEvent> function = (StablePriorityQueue<GameEventPriority, Tuple<GameEventOwner, Character, SingleCharEvent>> queue, int maxPriority, ref int nextPriority) =>
            {
                //start with universal
                DataManager.Instance.UniversalEvent.AddEventsToQueue(queue, maxPriority, ref nextPriority, DataManager.Instance.UniversalEvent.OnMapStarts);


                foreach (MapStatus mapStatus in ZoneManager.Instance.CurrentMap.Status.Values)
                {
                    MapStatusData entry = DataManager.Instance.GetMapStatus(mapStatus.ID);
                    mapStatus.AddEventsToQueue<SingleCharEvent>(queue, maxPriority, ref nextPriority, entry.OnMapStarts);
                }
            };
            foreach (Tuple<GameEventOwner, Character, SingleCharEvent> effect in IterateEvents<SingleCharEvent>(function))
                yield return CoroutineManager.Instance.StartCoroutine(effect.Item3.Apply(effect.Item1, effect.Item2, null));

            //Notify script engine
            LuaEngine.Instance.OnDungeonFloorBegin();
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
            }


            //heal players
            foreach (Character character in ActiveTeam.IterateByRank())
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

            ZoneManager.Instance.CurrentMap.CurrentTurnMap.CurrentOrder.TurnIndex = 0;
            ZoneManager.Instance.CurrentMap.CurrentTurnMap.CurrentOrder.EnemyFaction = false;
            ZoneManager.Instance.CurrentMap.CurrentTurnMap.CurrentOrder.TurnTier = 0;
            ZoneManager.Instance.CurrentMap.CurrentTurnMap.CurrentOrder.SkipAll = false;
            RegenerateTurnMap();

            RemoveDeadTeams();

            DataManager.Instance.Save.RescuesLeft--;
            //fade white back with music

            //remove reward item
            MapItem offeredItem = new MapItem((action[0] == 1), action[1]);
            offeredItem.HiddenValue = action[2];

            if (offeredItem.Value > -1)
            {
                if (offeredItem.IsMoney)
                    ActiveTeam.Bank -= offeredItem.Value;
                else
                {
                    ItemData entry = DataManager.Instance.GetItem(offeredItem.Value);
                    if (entry.MaxStack > 1)
                    {
                        List<int> itemsToTake = new List<int>();
                        for (int ii = 0; ii < offeredItem.HiddenValue; ii++)
                            itemsToTake.Add(offeredItem.Value);
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
                        List<int> itemsToTake = new List<int>();
                        itemsToTake.Add(DataManager.Instance.DataIndices[DataManager.DataType.Item].Count + chosenIndex);
                    }
                    else
                    {
                        List<int> itemsToTake = new List<int>();
                        itemsToTake.Add(offeredItem.Value);
                        ActiveTeam.TakeItems(itemsToTake);
                    }
                }
            }

            if (DataManager.Instance.CurrentReplay == null)
            {
                yield return CoroutineManager.Instance.StartCoroutine(ZoneManager.Instance.CurrentZone.OnRescued(mail));

                yield return CoroutineManager.Instance.StartCoroutine(MenuManager.Instance.SetDialogue(Text.FormatKey("MSG_RESCUES_LEFT", DataManager.Instance.Save.RescuesLeft)));
                yield return new WaitForFrames(1);
            }
            else
            {
                GameManager.Instance.SE(DataManager.Instance.ReviveSE);
                GameManager.Instance.SetFade(true, true);
                yield return CoroutineManager.Instance.StartCoroutine(GameManager.Instance.FadeIn());

                GameManager.Instance.BGM("Rescue.ogg", true);

                int nameLength = action[3];
                string name = "";
                for (int ii = 0; ii < nameLength; ii++)
                    name += (char)action[4+ii];
                LogMsg(Text.FormatKey("MSG_RESCUED_BY", name));
            }

            ZoneManager.Instance.CurrentMap.NoRescue = true;

            yield return CoroutineManager.Instance.StartCoroutine(ProcessTurnStart(CurrentCharacter));

        }

        public IEnumerator<YieldInstruction> ProcessPlayerInput(GameAction action)
        {
            if (action.Dir == Dir8.None)
                action.Dir = CurrentCharacter.CharDir;

            //hold on to the pending action (keep track of its direction)?
            //if a new action comes along, and it uses default direction, AND there was a previous pending action, have it inherit the old direction

            ActionResult result = new ActionResult();//denotes if a turn was taken
            yield return CoroutineManager.Instance.StartCoroutine(ProcessInput(action, CurrentCharacter, result));

            if (result.Success != ActionResult.ResultType.Fail)
            {
                //log the turn and reset inputs
                DataManager.Instance.LogPlay(action);
            }
        }

        //the intention, and its result to that frame
        //"choose the action to partake in"
        public IEnumerator<YieldInstruction> ProcessInput(GameAction action, Character character, ActionResult result)
        {
            //translates commands into actions
            if (character.AttackOnly && character.CantWalk && action.Type == GameAction.ActionType.Wait)
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
                            DungeonScene.Instance.LogMsg(Text.FormatKey("MSG_SKIP_TURN", character.BaseName), false, true);
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
                        yield return CoroutineManager.Instance.StartCoroutine(ProcessTileInteract(character, result));
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

                        yield return CoroutineManager.Instance.StartCoroutine(GameManager.Instance.EndSegment((GameProgress.ResultType)action[0]));
                        break;
                    }
                case GameAction.ActionType.Tactics:
                    {
                        result.Success = ActionResult.ResultType.Success;

                        //saves all the settings to the characters
                        for (int ii = 0; ii < ActiveTeam.Players.Count; ii++)
                        {
                            int choice = action[ii];
                            Character target = ActiveTeam.Players[ii];
                            AITactic tactic = DataManager.Instance.GetAITactic(choice);
                            if (tactic.ID != target.Tactic.ID)
                                target.Tactic = new AITactic(tactic);
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

                        ActiveTeam.Players[action[0]].SwitchSkills(action[1]);
                        break;
                    }
                case GameAction.ActionType.SortItems:
                    {
                        result.Success = ActionResult.ResultType.Success;

                        ActiveTeam.SortItems();
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
        public void AddTeam(Team team)
        {
            ZoneManager.Instance.CurrentMap.MapTeams.Add(team);

            int teamIndex = ZoneManager.Instance.CurrentMap.MapTeams.Count - 1;
        }
        public void RemoveTeam(int teamIndex)
        {
            CharIndex turnChar = ZoneManager.Instance.CurrentMap.CurrentTurnMap.GetCurrentTurnChar();
            //note: this cannot be called when it is the team's turn
            if (turnChar.Team == teamIndex)
                throw new Exception();

            ZoneManager.Instance.CurrentMap.MapTeams.RemoveAt(teamIndex);

            ZoneManager.Instance.CurrentMap.CurrentTurnMap.UpdateTeamRemoval(teamIndex);
        }

        public void AddCharToTeam(int teamIndex, Character character)
        {
            Team team = null;
            if (teamIndex == -1)
                team = ZoneManager.Instance.CurrentMap.ActiveTeam;
            else
                team = ZoneManager.Instance.CurrentMap.MapTeams[teamIndex];

            team.Players.Add(character);

            OnCharAdd(character);
        }

        public void OnCharAdd(Character newChar)
        {
            newChar.TurnWait = 0;
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

            Team team = null;
            if (charIndex.Team == -1)
                team = ZoneManager.Instance.CurrentMap.ActiveTeam;
            else
                team = ZoneManager.Instance.CurrentMap.MapTeams[charIndex.Team];

            Character character = team.Players[charIndex.Char];

            character.OnRemove();
            team.Players.RemoveAt(charIndex.Char);

            //update leader
            if (team is ExplorerTeam && charIndex.Char < team.GetLeaderIndex())
                ((ExplorerTeam)team).LeaderIndex--;

            ZoneManager.Instance.CurrentMap.CurrentTurnMap.UpdateCharRemoval(charIndex.Team, charIndex.Char);

            if (charIndex.Team == -1 && focusedPlayerIndex > charIndex.Char)
                focusedPlayerIndex--;

            //if the team is all empty (not all dead), definitely remove them
            if (team.Players.Count == 0 && charIndex.Team > -1)
                RemoveTeam(charIndex.Team);
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
            else if (ZoneManager.Instance.CurrentMap.NoSwitching)
            {
                GameManager.Instance.SE("Menu/Cancel");
                DungeonScene.Instance.LogMsg(Text.FormatKey("MSG_CANT_SWAP_LEADER", CurrentCharacter.BaseName), false, true);
                yield return new WaitForFrames(GameManager.Instance.ModifyBattleSpeed(10));
            }
            else
            {
                Character character = ActiveTeam.Players[charIndex];
                if (character.Dead)
                    GameManager.Instance.SE("Menu/Cancel");
                else if (!ZoneManager.Instance.CurrentMap.CurrentTurnMap.IsEligibleToMove(character))
                    GameManager.Instance.SE("Menu/Cancel");
                else
                {
                    result.Success = ActionResult.ResultType.Success;

                    //change the leader index
                    ActiveTeam.LeaderIndex = charIndex;

                    //re-order map order as well
                    ZoneManager.Instance.CurrentMap.CurrentTurnMap.AdjustPromotion(-1, ActiveTeam.LeaderIndex);

                    focusedPlayerIndex = ZoneManager.Instance.CurrentMap.CurrentTurnMap.GetCurrentTurnChar().Char;

                    DungeonScene.Instance.LogMsg(Text.FormatKey("MSG_LEADER_SWAP", ActiveTeam.Leader.BaseName));

                    GameManager.Instance.SE(DataManager.Instance.LeaderSE);
                    yield return new WaitForFrames(GameManager.Instance.ModifyBattleSpeed(10));
                    yield return CoroutineManager.Instance.StartCoroutine(SpecialIntro(ActiveTeam.Leader));
                }
            }
        }

        public IEnumerator<YieldInstruction> SpecialIntro(Character chara)
        {
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

                //update the leader indices
                if (ActiveTeam.LeaderIndex == charIndex)
                {
                    ActiveTeam.LeaderIndex++;
                    ZoneManager.Instance.CurrentMap.CurrentTurnMap.AdjustPromotion(-1, ActiveTeam.LeaderIndex);
                }
                else if (ActiveTeam.LeaderIndex == charIndex + 1)
                {
                    ActiveTeam.LeaderIndex--;
                    ZoneManager.Instance.CurrentMap.CurrentTurnMap.AdjustPromotion(-1, ActiveTeam.LeaderIndex);
                }


                focusedPlayerIndex = ZoneManager.Instance.CurrentMap.CurrentTurnMap.GetCurrentTurnChar().Char;
            }
        }

        public void ReorderDeadLeaders()
        {
            {
                int liveIndex = -1;
                for (int ii = 0; ii < ZoneManager.Instance.CurrentMap.ActiveTeam.Players.Count; ii++)
                {
                    if (!ZoneManager.Instance.CurrentMap.ActiveTeam.Players[ii].Dead)
                    {
                        liveIndex = ii;
                        break;
                    }
                }

                if (ZoneManager.Instance.CurrentMap.ActiveTeam.Leader.Dead && liveIndex != -1)
                {
                    //switch leader to this
                    ActiveTeam.LeaderIndex = liveIndex;

                    //re-order map order as well
                    ZoneManager.Instance.CurrentMap.CurrentTurnMap.AdjustPromotion(-1, liveIndex);

                    DungeonScene.Instance.LogMsg(Text.FormatKey("MSG_LEADER_SWAP", ZoneManager.Instance.CurrentMap.ActiveTeam.Leader.BaseName));
                }
            }

            for (int jj = 0; jj < ZoneManager.Instance.CurrentMap.MapTeams.Count; jj++)
            {
                Team team = ZoneManager.Instance.CurrentMap.MapTeams[jj];
                int liveIndex = -1;
                for (int ii = 0; ii < team.Players.Count; ii++)
                {
                    if (!team.Players[ii].Dead)
                    {
                        liveIndex = ii;
                        break;
                    }
                }
                //NOTE: THE ASSUMPTION IS THAT ALL OTHER TEAMS HAVE LEADERS AT INDEX 0
                if (liveIndex > 0)
                {
                    for (int ii = 0; ii < liveIndex; ii++)
                    {
                        //move this character to the end of the team
                        Character character = team.Players[0];
                        team.Players.RemoveAt(0);
                        team.Players.Add(character);

                        //re-order map order as well
                        ZoneManager.Instance.CurrentMap.CurrentTurnMap.AdjustDemotion(jj, team.Players.Count - 1);
                    }
                }
            }
        }

        public void RemoveDeadTeams()
        {
            for (int ii = ZoneManager.Instance.CurrentMap.MapTeams.Count - 1; ii >= 0; ii--)
            {
                bool allDead = true;
                foreach (Character character in ZoneManager.Instance.CurrentMap.MapTeams[ii].Players)
                {
                    if (!character.Dead)
                        allDead = false;
                }
                if (allDead)
                {
                    for (int jj = ZoneManager.Instance.CurrentMap.MapTeams[ii].Players.Count - 1; jj >= 0; jj--)
                        RemoveChar(new CharIndex(ii, jj));
                }
            }
        }

        public int GetSpeedMult(Character movingChar, Loc destTile)
        {
            if (RunMode)
                return 12;

            Loc seen = Character.GetSightDims();
            Loc startDiff = movingChar.CharLoc - FocusedCharacter.CharLoc;
            Loc endDiff = destTile - FocusedCharacter.CharLoc;
            if ((Math.Abs(startDiff.X) > seen.X || Math.Abs(startDiff.Y) > seen.Y) && (Math.Abs(endDiff.X) > seen.X || Math.Abs(endDiff.Y) > seen.Y))
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
            CurrentCharacter.TurnUsed = action;

            if (!IsGameOver())
            {

                ReorderDeadLeaders();

                do
                {
                    //move to next turn
                    ZoneManager.Instance.CurrentMap.CurrentTurnMap.CurrentOrder.TurnIndex++;

                    while (ZoneManager.Instance.CurrentMap.CurrentTurnMap.CurrentOrder.TurnIndex >= ZoneManager.Instance.CurrentMap.CurrentTurnMap.TurnToChar.Count)
                    {
                        ZoneManager.Instance.CurrentMap.CurrentTurnMap.CurrentOrder.EnemyFaction = !ZoneManager.Instance.CurrentMap.CurrentTurnMap.CurrentOrder.EnemyFaction;
                        ZoneManager.Instance.CurrentMap.CurrentTurnMap.CurrentOrder.TurnIndex = 0;

                        if (ZoneManager.Instance.CurrentMap.CurrentTurnMap.CurrentOrder.EnemyFaction == false)
                        {
                            ZoneManager.Instance.CurrentMap.CurrentTurnMap.CurrentOrder.TurnTier++;

                            if (ZoneManager.Instance.CurrentMap.CurrentTurnMap.CurrentOrder.TurnTier >= 6)
                            {
                                yield return CoroutineManager.Instance.StartCoroutine(ProcessMapTurnEnd());
                                ZoneManager.Instance.CurrentMap.CurrentTurnMap.CurrentOrder.TurnTier = 0;
                                ZoneManager.Instance.CurrentMap.CurrentTurnMap.CurrentOrder.SkipAll = false;
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

                if (!IsPlayerTurn())
                    OrganizeAIMovement(0);

                RemoveDeadTeams();

                yield return CoroutineManager.Instance.StartCoroutine(ProcessTurnStart(CurrentCharacter));
            }
        }

        public void ResetTurns()
        {
            //silently set team mode to false
            DataManager.Instance.Save.TeamMode = false;
            //silently reset turn map
            ZoneManager.Instance.CurrentMap.CurrentTurnMap = new TurnState();
            RegenerateTurnMap();
            focusedPlayerIndex = ZoneManager.Instance.CurrentMap.CurrentTurnMap.GetCurrentTurnChar().Char;
        }

        public void RegenerateTurnMap()
        {
            //beginning of a faction's turn; create new turn map
            ZoneManager.Instance.CurrentMap.CurrentTurnMap.TurnToChar.Clear();
            if (ZoneManager.Instance.CurrentMap.CurrentTurnMap.CurrentOrder.EnemyFaction)
            {
                for (int ii = 0; ii < ZoneManager.Instance.CurrentMap.MapTeams.Count; ii++)
                    ZoneManager.Instance.CurrentMap.CurrentTurnMap.LoadTeamTurnMap(ii, ZoneManager.Instance.CurrentMap.MapTeams[ii]);
            }
            else
                ZoneManager.Instance.CurrentMap.CurrentTurnMap.LoadTeamTurnMap(-1, ZoneManager.Instance.CurrentMap.ActiveTeam);
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
            //turn ends for all characters
            foreach (Character character in ZoneManager.Instance.CurrentMap.IterateCharacters())
            {
                if (!character.Dead)
                    yield return CoroutineManager.Instance.StartCoroutine(character.OnMapTurnEnd());
            }

            //turn ends for map statuses
            EventEnqueueFunction<SingleCharEvent> function = (StablePriorityQueue<GameEventPriority, Tuple<GameEventOwner, Character, SingleCharEvent>> queue, int maxPriority, ref int nextPriority) =>
            {
                //start with universal
                DataManager.Instance.UniversalEvent.AddEventsToQueue(queue, maxPriority, ref nextPriority, DataManager.Instance.UniversalEvent.OnMapTurnEnds);


                foreach (MapStatus mapStatus in ZoneManager.Instance.CurrentMap.Status.Values)
                {
                    MapStatusData entry = DataManager.Instance.GetMapStatus(mapStatus.ID);
                    mapStatus.AddEventsToQueue<SingleCharEvent>(queue, maxPriority, ref nextPriority, entry.OnMapTurnEnds);
                }
            };
            foreach (Tuple<GameEventOwner, Character, SingleCharEvent> effect in IterateEvents<SingleCharEvent>(function))
                yield return CoroutineManager.Instance.StartCoroutine(effect.Item3.Apply(effect.Item1, effect.Item2, null));


            ZoneManager.Instance.CurrentMap.MapTurns++;
            DataManager.Instance.Save.TotalTurns++;


            //Map Respawns
            if (ZoneManager.Instance.CurrentMap.RespawnTime > 0 && ZoneManager.Instance.CurrentMap.MapTurns % ZoneManager.Instance.CurrentMap.RespawnTime == 0)
            {
                int totalFoes = 0;
                foreach (Team team in ZoneManager.Instance.CurrentMap.MapTeams)
                {
                    foreach (Character character in team.Players)
                    {
                        if (!character.Dead)
                            totalFoes++;
                    }
                }
                if (totalFoes < ZoneManager.Instance.CurrentMap.MaxFoes)
                {
                    List<Character> respawns = ZoneManager.Instance.CurrentMap.RespawnMob();
                    foreach (Character respawn in respawns)
                    {
                        respawn.Tactic.Initialize(respawn);
                        if (!respawn.Dead)
                        {
                            yield return CoroutineManager.Instance.StartCoroutine(respawn.OnMapStart());
                            ZoneManager.Instance.CurrentMap.UpdateExploration(respawn);
                        }
                    }
                }
            }

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
                //process turn start (assume live characters)
                yield return CoroutineManager.Instance.StartCoroutine(character.OnTurnStart());
            }

        }

    }
}
