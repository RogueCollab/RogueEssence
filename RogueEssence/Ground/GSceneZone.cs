using System;
using System.Collections.Generic;
using RogueEssence.Content;
using RogueElements;
using RogueEssence.Data;
using RogueEssence.Menu;
using RogueEssence.Dungeon;
using RogueEssence.Script;

namespace RogueEssence.Ground
{
    public partial class GroundScene
    {

        public void EnterGround(int entryPoint)
        {
            LocRay8 entry = ZoneManager.Instance.CurrentGround.GetEntryPoint(entryPoint);
            EnterGround(entry.Loc, entry.Dir);
        }

        public void EnterGround(string entryPoint)
        {
            LocRay8 entry = ZoneManager.Instance.CurrentGround.GetEntryPoint(entryPoint);

            if (entry.Dir == Dir8.None)
                entry.Dir = DataManager.Instance.Save.ActiveTeam.Leader.CharDir;

            EnterGround(entry.Loc, entry.Dir);
        }


        public void EnterGround(Loc entrypos, Dir8 entrydir)
        {
            ZoneManager.Instance.CurrentGround.SetPlayerChar(new GroundChar(DataManager.Instance.Save.ActiveTeam.Leader, entrypos,
                (entrydir != Dir8.None) ? entrydir : DataManager.Instance.Save.ActiveTeam.Leader.CharDir, "PLAYER"));

            ResetGround();
        }



        public override void Exit()
        {
            ResetAnims();
        }


        public IEnumerator<YieldInstruction> ExitGround()
        {
            if (ZoneManager.Instance.CurrentGround != null)
            {
                //Notify script engine
                yield return CoroutineManager.Instance.StartCoroutine(ZoneManager.Instance.CurrentGround.OnExit());
                
                ZoneManager.Instance.CurrentGround.SetPlayerChar(null);
                ZoneManager.Instance.CurrentZone.SetCurrentMap(SegLoc.Invalid);
            }
            yield break;
        }

        public void ResetGround()
        {
            ZoneManager.Instance.CurrentGround.ViewCenter = null;
            ZoneManager.Instance.CurrentGround.ViewOffset = new Loc();
        }

        public IEnumerator<YieldInstruction> InitGround(bool saveLoad)
        {
            //start emitters for existing map status
            foreach (MapStatus mapStatus in ZoneManager.Instance.CurrentGround.Status.Values)
                mapStatus.StartEmitter(Anims);

            GraphicsManager.GlobalIdle = GraphicsManager.IdleAction;

            yield return CoroutineManager.Instance.StartCoroutine(ZoneManager.Instance.CurrentGround.OnInit());
            if (saveLoad)
                yield return CoroutineManager.Instance.StartCoroutine(ZoneManager.Instance.CurrentGround.OnGameLoad());
        }

        public IEnumerator<YieldInstruction> BeginGround()
        {
            DataManager.Instance.Save.Trail.Add(ZoneManager.Instance.CurrentGround.GetColoredName());
            LogMsg(Text.FormatKey("MSG_ENTER_MAP", DataManager.Instance.Save.ActiveTeam.GetDisplayName(), ZoneManager.Instance.CurrentGround.GetColoredName()));
            //psy's note: might as well help encapsulate map stuff
            yield return CoroutineManager.Instance.StartCoroutine(ZoneManager.Instance.CurrentGround.OnEnter());
        }

        public IEnumerator<YieldInstruction> SuspendGame()
        {
            GameManager.Instance.BGM("", true);

            yield return CoroutineManager.Instance.StartCoroutine(GameManager.Instance.FadeOut(false));

            //TODO: call OnGameSave?
            //where does it load the game in a suspend scenario?
            DataManager.Instance.LogGroundSave();
            DataManager.Instance.SuspendPlay();

            MenuBase.Transparent = false;

            GameManager.Instance.SceneOutcome = GameManager.Instance.RestartToTitle();
        }

        public IEnumerator<YieldInstruction> SaveGame()
        {
            yield return CoroutineManager.Instance.StartCoroutine(ZoneManager.Instance.CurrentGround.OnGameSave());
            DataManager.Instance.SaveMainGameState();
        }

        public IEnumerator<YieldInstruction> ProcessInput(GameAction action)
        {
            GroundChar character = FocusedCharacter;

            switch (action.Type)
            {
                case GameAction.ActionType.Dir:
                    {
                        //result.Success = ActionResult.ResultType.Success;
                        break;
                    }
                case GameAction.ActionType.Move:
                    {
                        character.CurrentCommand = action;
                        break;
                    }
                case GameAction.ActionType.Drop:
                    {
                        yield return CoroutineManager.Instance.StartCoroutine(ProcessTrashItem(character, action[0], action[1] != 0));
                        break;
                    }
                case GameAction.ActionType.Give:
                    {
                        //[0] = item slot to use (-1 for the ground item)
                        //[1] = who to give it to (-1 for the user)
                        yield return CoroutineManager.Instance.StartCoroutine(ProcessGiveItem(character, action[0], action[1]));
                        break;
                    }
                case GameAction.ActionType.Take:
                    {
                        //[0] = team slot to take from
                        yield return CoroutineManager.Instance.StartCoroutine(ProcessTakeItem(character, action[0]));
                        break;
                    }
                case GameAction.ActionType.Attack:
                    {
                        character.CurrentCommand = action;
                        yield return CoroutineManager.Instance.StartCoroutine(ProcessObjectInteract(character));
                        break;
                    }
                case GameAction.ActionType.UseItem:
                    {
                        //[0] = item slot to use (-1 for held item, -2 for the ground item)
                        //[1] = who to use it on (-1 for the user)
                        //others: which slot to delete,
                        //which intrinsic to have, which team member/item to send in, etc.
                        yield return CoroutineManager.Instance.StartCoroutine(ProcessUseItem(character, action[0], action[1] != 0));
                        break;
                    }
                case GameAction.ActionType.ShiftTeam:
                    {
                        int charIndex = action[0];
                        Character targetChar = DataManager.Instance.Save.ActiveTeam.Players[charIndex];
                        DataManager.Instance.Save.ActiveTeam.Players.RemoveAt(charIndex);
                        DataManager.Instance.Save.ActiveTeam.Players.Insert(charIndex + 1, targetChar);

                        //update the leader indices
                        if (DataManager.Instance.Save.ActiveTeam.LeaderIndex == charIndex)
                            DataManager.Instance.Save.ActiveTeam.LeaderIndex++;
                        else if (DataManager.Instance.Save.ActiveTeam.LeaderIndex == charIndex + 1)
                            DataManager.Instance.Save.ActiveTeam.LeaderIndex--;
                        break;
                    }
                case GameAction.ActionType.SetLeader:
                    {
                        yield return CoroutineManager.Instance.StartCoroutine(MakeLeader(action[0], action[1] != 0));
                        break;
                    }
                case GameAction.ActionType.SendHome:
                    {
                        yield return CoroutineManager.Instance.StartCoroutine(SendHome(action[0]));
                        break;
                    }
                case GameAction.ActionType.GiveUp:
                    {
                        GameManager.Instance.SceneOutcome = GameManager.Instance.EndSegment((GameProgress.ResultType)action[0]);
                        break;
                    }
                case GameAction.ActionType.Tactics:
                    {
                        //saves all the settings to the characters
                        for (int ii = 0; ii < DataManager.Instance.Save.ActiveTeam.Players.Count; ii++)
                        {
                            int choice = action[ii];
                            Character target = DataManager.Instance.Save.ActiveTeam.Players[ii];
                            AITactic tactic = DataManager.Instance.GetAITactic(choice);
                            if (tactic.ID != target.Tactic.ID)
                                target.Tactic = new AITactic(tactic);
                            target.Tactic.Initialize(target);
                        }
                        break;
                    }
                case GameAction.ActionType.SetSkill:
                    {
                        Skill skill = DataManager.Instance.Save.ActiveTeam.Players[action[0]].Skills[action[1]].Element;
                        skill.Enabled = !skill.Enabled;
                        break;
                    }
                case GameAction.ActionType.ShiftSkill:
                    {
                        int slot = action[1];
                        Character targetChar = DataManager.Instance.Save.ActiveTeam.Players[action[0]];
                        BackReference<Skill> upState = targetChar.Skills[slot];
                        BackReference<Skill> downState = targetChar.Skills[slot + 1];
                        targetChar.Skills[slot] = downState;
                        targetChar.Skills[slot + 1] = upState;

                        if (upState.BackRef > -1 && downState.BackRef > -1)
                        {
                            SlotSkill skill = targetChar.BaseSkills[slot];
                            targetChar.BaseSkills.RemoveAt(slot);
                            targetChar.BaseSkills.Insert(slot + 1, skill);
                        }
                        break;
                    }
                case GameAction.ActionType.SortItems:
                    {
                        DataManager.Instance.Save.ActiveTeam.SortItems();
                        break;
                    }
                default:
                    {
                        throw new Exception("Undefined Command: " + action.Type);
                    }
            }
        }


        private bool canSwitchToChar(int charIndex)
        {
            Character character = DataManager.Instance.Save.ActiveTeam.Players[charIndex];
            if (character.Dead)
                return false;
            return true;
        }

        public IEnumerator<YieldInstruction> MakeLeader(int charIndex, bool silent)
        {
            if (charIndex >= DataManager.Instance.Save.ActiveTeam.Players.Count || charIndex == DataManager.Instance.Save.ActiveTeam.LeaderIndex)
                GameManager.Instance.SE("Menu/Cancel");
            else if (ZoneManager.Instance.CurrentGround.NoSwitching || DataManager.Instance.Save.NoSwitching)
                GameManager.Instance.SE("Menu/Cancel");
            else
            {
                if (!canSwitchToChar(charIndex))
                    GameManager.Instance.SE("Menu/Cancel");
                else
                {
                    DataManager.Instance.Save.ActiveTeam.LeaderIndex = charIndex;
                    GameManager.Instance.SE(GraphicsManager.LeaderSE);

                    yield return CoroutineManager.Instance.StartCoroutine(GameManager.Instance.FadeOut(false));

                    yield return CoroutineManager.Instance.StartCoroutine(ZoneManager.Instance.CurrentGround.OnInit());

                    yield return CoroutineManager.Instance.StartCoroutine(GameManager.Instance.FadeIn());

                    if (!silent)
                        yield return CoroutineManager.Instance.StartCoroutine(MenuManager.Instance.SetDialogue(Text.FormatKey("MSG_LEADER_SWAP", DataManager.Instance.Save.ActiveTeam.Leader.GetDisplayName(true))));
                }
            }
            yield return new WaitForFrames(GameManager.Instance.ModifyBattleSpeed(10));
        }


        public void RemoveChar(int charIndex)
        {
            ExplorerTeam team = DataManager.Instance.Save.ActiveTeam;

            team.Players.RemoveAt(charIndex);

            //update leader
            if (charIndex < team.LeaderIndex)
                team.LeaderIndex--;

        }

    }
}
