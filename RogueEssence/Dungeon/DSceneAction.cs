using System;
using System.Collections.Generic;
using RogueEssence.Content;
using RogueElements;
using RogueEssence.Data;

namespace RogueEssence.Dungeon
{
    public partial class DungeonScene
    {
        public IEnumerator<YieldInstruction> CancelWait(Loc loc)
        {
            if (AnimationsOver())
                yield return new WaitForFrames(GameManager.Instance.ModifyBattleSpeed(20, loc));
        }

        public IEnumerator<YieldInstruction> ProcessUseSkill(Character character, int skillSlot, ActionResult result)
        {
            BattleContext context = new BattleContext(BattleActionType.Skill);
            context.User = character;
            context.UsageSlot = skillSlot;

            yield return CoroutineManager.Instance.StartCoroutine(InitActionData(context));
            yield return CoroutineManager.Instance.StartCoroutine(context.User.BeforeTryAction(context));
            if (context.CancelState.Cancel) { yield return CoroutineManager.Instance.StartCoroutine(CancelWait(context.User.CharLoc)); yield break; }

            context.TurnCancel.Cancel = false;

            //move has been made; end-turn must be done from this point onwards
            yield return CoroutineManager.Instance.StartCoroutine(CheckExecuteAction(context, PreExecuteSkill));

            if (context.SkillUsedUp > -1 && !context.User.Dead)
            {
                SkillData entry = DataManager.Instance.GetSkill(context.SkillUsedUp);
                LogMsg(Text.FormatKey("MSG_OUT_OF_CHARGES", context.User.Name, entry.Name.ToLocal()));

                yield return CoroutineManager.Instance.StartCoroutine(DungeonScene.Instance.ProcessEmoteFX(context.User, DataManager.Instance.NoChargeFX));
            }

            if (!context.CancelState.Cancel)
                yield return new WaitForFrames(GameManager.Instance.ModifyBattleSpeed(20, context.User.CharLoc));

            yield return CoroutineManager.Instance.StartCoroutine(FinishTurn(context.User, !context.TurnCancel.Cancel));

            result.Success = context.TurnCancel.Cancel ? ActionResult.ResultType.Success : ActionResult.ResultType.TurnTaken;
        }


        public IEnumerator<YieldInstruction> ProcessUseItem(Character character, int invSlot, int teamSlot, ActionResult result)
        {
            if (character.AttackOnly)
            {
                LogMsg(Text.FormatKey("MSG_CANT_USE_ITEM", character.Name), false, true);
                yield break;
            }
            Character target = teamSlot == -1 ? character : character.MemberTeam.Players[teamSlot];
            if (target.AttackOnly)
            {
                LogMsg(Text.FormatKey("MSG_CANT_USE_ITEM", target.Name), false, true);
                yield break;
            }

            BattleContext context = new BattleContext(BattleActionType.Item);
            context.User = target;
            context.UsageSlot = invSlot;

            yield return CoroutineManager.Instance.StartCoroutine(InitActionData(context));
            yield return CoroutineManager.Instance.StartCoroutine(context.User.BeforeTryAction(context));
            if (context.CancelState.Cancel) { yield return CoroutineManager.Instance.StartCoroutine(CancelWait(context.User.CharLoc)); yield break; }

            context.TurnCancel.Cancel = false;

            //move has been made; end-turn must be done from this point onwards
            yield return CoroutineManager.Instance.StartCoroutine(CheckExecuteAction(context, PreExecuteItem));
            if (!context.CancelState.Cancel)
                yield return new WaitForFrames(GameManager.Instance.ModifyBattleSpeed(20, context.User.CharLoc));

            yield return CoroutineManager.Instance.StartCoroutine(FinishTurn(context.User, !context.TurnCancel.Cancel));

            result.Success = context.TurnCancel.Cancel ? ActionResult.ResultType.Success : ActionResult.ResultType.TurnTaken;
        }

        public IEnumerator<YieldInstruction> ProcessThrowItem(Character character, int invSlot, ActionResult result)
        {
            if (character.AttackOnly)
            {
                LogMsg(Text.FormatKey("MSG_CANT_USE_ITEM", character.Name), false, true);
                yield break;
            }

            BattleContext context = new BattleContext(BattleActionType.Throw);
            context.User = character;
            context.UsageSlot = invSlot;

            yield return CoroutineManager.Instance.StartCoroutine(InitActionData(context));
            yield return CoroutineManager.Instance.StartCoroutine(context.User.BeforeTryAction(context));
            if (context.CancelState.Cancel) { yield return CoroutineManager.Instance.StartCoroutine(CancelWait(context.User.CharLoc)); yield break; }

            context.TurnCancel.Cancel = false;

            //move has been made; end-turn must be done from this point onwards
            yield return CoroutineManager.Instance.StartCoroutine(CheckExecuteAction(context, PreExecuteItem));
            if (!context.CancelState.Cancel)
                yield return new WaitForFrames(GameManager.Instance.ModifyBattleSpeed(20, context.User.CharLoc));

            yield return CoroutineManager.Instance.StartCoroutine(FinishTurn(context.User, !context.TurnCancel.Cancel));

            result.Success = context.TurnCancel.Cancel ? ActionResult.ResultType.Success : ActionResult.ResultType.TurnTaken;
        }


        public IEnumerator<YieldInstruction> InitActionData(BattleContext context)
        {
            EventEnqueueFunction<BattleEvent> function = (StablePriorityQueue<GameEventPriority, Tuple<GameEventOwner, Character, BattleEvent>> queue, Priority maxPriority, ref Priority nextPriority) =>
            {
                DataManager.Instance.UniversalEvent.AddEventsToQueue(queue, maxPriority, ref nextPriority, DataManager.Instance.UniversalEvent.InitActionData);
            };
            foreach (Tuple<GameEventOwner, Character, BattleEvent> effect in IterateEvents(function))
            {
                yield return CoroutineManager.Instance.StartCoroutine(effect.Item3.Apply(effect.Item1, effect.Item2, context));
                if (context.CancelState.Cancel) yield break;
            }
        }

        public IEnumerator<YieldInstruction> PreExecuteSkill(BattleContext context)
        {
            if (context.UsageSlot > BattleContext.DEFAULT_ATTACK_SLOT && context.UsageSlot < CharData.MAX_SKILL_SLOTS)
            {
                yield return CoroutineManager.Instance.StartCoroutine(context.User.DeductCharges(context.UsageSlot, 1, false, false));
                if (context.User.Skills[context.UsageSlot].Element.Charges == 0)
                    context.SkillUsedUp = context.User.Skills[context.UsageSlot].Element.SkillNum;
            }
            yield return new WaitUntil(AnimationsOver);

            if (!String.IsNullOrEmpty(context.actionMsg))
                LogMsg(context.actionMsg);

            yield break;
        }

        public IEnumerator<YieldInstruction> PreExecuteItem(BattleContext context)
        {
            //remove the item from the inventory/ground/hold
            if (context.UsageSlot > BattleContext.EQUIP_ITEM_SLOT)
            {
                InvItem item = ((ExplorerTeam)context.User.MemberTeam).GetInv(context.UsageSlot);
                ItemData entry = (ItemData)item.GetData();
                if (entry.MaxStack > 1 && item.HiddenValue > 1)
                    item.HiddenValue--;
                else
                    ((ExplorerTeam)context.User.MemberTeam).RemoveFromInv(context.UsageSlot);
            }
            else if (context.UsageSlot == BattleContext.EQUIP_ITEM_SLOT)
            {
                InvItem item = context.User.EquippedItem;
                ItemData entry = (ItemData)item.GetData();
                if (entry.MaxStack > 1 && item.HiddenValue > 1)
                    item.HiddenValue--;
                else
                    context.User.DequipItem();
            }
            else if (context.UsageSlot == BattleContext.FLOOR_ITEM_SLOT)
            {
                int mapSlot = ZoneManager.Instance.CurrentMap.GetItem(context.User.CharLoc);
                MapItem item = ZoneManager.Instance.CurrentMap.Items[mapSlot];
                ItemData entry = DataManager.Instance.GetItem(item.Value);
                if (entry.MaxStack > 1 && item.HiddenValue > 1)
                    item.HiddenValue--;
                else
                    ZoneManager.Instance.CurrentMap.Items.RemoveAt(mapSlot);
            }

            yield return new WaitUntil(AnimationsOver);

            if (!String.IsNullOrEmpty(context.actionMsg))
                LogMsg(context.actionMsg);

            yield break;
        }



        public IEnumerator<YieldInstruction> PerformAction(BattleContext context)
        {
            //this is where the delays between target hits are managed
            context.HitboxAction.Distance += Math.Min(Math.Max(-3, context.RangeMod), 3);
            yield return CoroutineManager.Instance.StartCoroutine(context.User.PerformCharAction(context.HitboxAction.Clone(), context));
            //if (context.User.CharLoc == context.StrikeEndTile && context.StrikeEndTile != context.StrikeStartTile)
            //    yield return CoroutinesManager.Instance.StartCoroutine(ArriveOnTile(context.User, false, false, false));

            //TODO: test to make sure everything is consistent with the erasure of handling movement in here
            //first, hopping needs to work properly, because the it will call its own tile landing
            //dash attacks will also handle their own tile landing.
            //for now, since all dash moves hit tiles, that means they will all activate the tile

            //TODO: make NOTRAP true, and then make the attacks themselves activate traps
            //TODO: need to make sure that the user lands on their given tile EXACTLY ONCE
            //ex:
            //action does not move the user; move effect does not move the user //most moves
            //the action effect nor the main flow should not handle the update
            //action does not move the user; move effect DOES move the user //hopping moves
            //the action effect must handle the update; the main flow should not
            //action DOES move the user; move effect does not move the user //a dash attack
            //the action effect must not handle any update; the main flow should.
            //action DOES move the user; move effect DOES move the user //???
            //the action effect handles the update; the main flow doesn't need to
        }

        public delegate IEnumerator<YieldInstruction> ContextMethod(BattleContext context);

        public IEnumerator<YieldInstruction> CheckExecuteAction(BattleContext context, ContextMethod preExecute)
        {
            yield return CoroutineManager.Instance.StartCoroutine(PreProcessAction(context));
            yield return CoroutineManager.Instance.StartCoroutine(context.User.BeforeAction(context));
            if (context.CancelState.Cancel) yield break;
            yield return CoroutineManager.Instance.StartCoroutine(preExecute(context));
            yield return CoroutineManager.Instance.StartCoroutine(ExecuteAction(context));
            if (context.CancelState.Cancel) yield break;
            yield return CoroutineManager.Instance.StartCoroutine(RepeatActions(context));
        }



        public IEnumerator<YieldInstruction> PreProcessAction(BattleContext context)
        {
            context.StrikeStartTile = context.User.CharLoc;
            context.StrikeEndTile = context.User.CharLoc;

            List<Dir8> trialDirs = new List<Dir8>();
            trialDirs.Add(context.User.CharDir);

            if (context.UsageSlot != BattleContext.FORCED_SLOT && context.User.MovesScrambled)
            {
                trialDirs.Add(DirExt.AddAngles(context.User.CharDir, Dir8.DownRight));
                trialDirs.Add(DirExt.AddAngles(context.User.CharDir, Dir8.DownLeft));
            }
            ProcessDir(trialDirs[DataManager.Instance.Save.Rand.Next(trialDirs.Count)], context.User);

            yield break;
        }


        public IEnumerator<YieldInstruction> ExecuteAction(BattleContext baseContext)
        {
            BattleContext context = new BattleContext(baseContext, true);

            yield return CoroutineManager.Instance.StartCoroutine(context.User.OnAction(context));
            if (context.CancelState.Cancel) yield break;
            yield return CoroutineManager.Instance.StartCoroutine(PerformAction(context));
            if (context.CancelState.Cancel) yield break;
            yield return CoroutineManager.Instance.StartCoroutine(context.User.AfterActionTaken(context));
        }

        public IEnumerator<YieldInstruction> RepeatActions(BattleContext context)
        {
            //increment for multistrike
            context.StrikesMade++;
            while (context.StrikesMade < context.Strikes && !context.User.Dead)
            {
                yield return CoroutineManager.Instance.StartCoroutine(PreProcessAction(context));
                yield return CoroutineManager.Instance.StartCoroutine(context.User.BeforeAction(context));
                if (context.CancelState.Cancel) yield break;
                yield return CoroutineManager.Instance.StartCoroutine(ExecuteAction(context));

                context.StrikesMade++;
            }
        }



        public delegate IEnumerator<YieldInstruction> HitboxEffect(Loc target);


        public IEnumerator<YieldInstruction> MockHitLoc(Loc loc)
        {
            yield break;
            //TODO: draw error VFX on the location?
        }

        public IEnumerator<YieldInstruction> BeforeExplosion(BattleContext context)
        {
            EventEnqueueFunction<BattleEvent> function = (StablePriorityQueue<GameEventPriority, Tuple<GameEventOwner, Character, BattleEvent>> queue, Priority maxPriority, ref Priority nextPriority) =>
            {

                DataManager.Instance.UniversalEvent.AddEventsToQueue(queue, maxPriority, ref nextPriority, DataManager.Instance.UniversalEvent.BeforeExplosions);

                context.Data.AddEventsToQueue<BattleEvent>(queue, maxPriority, ref nextPriority, context.Data.BeforeExplosions);

                StablePriorityQueue<int, Character> charQueue = new StablePriorityQueue<int, Character>();
                foreach (Character character in ZoneManager.Instance.CurrentMap.IterateCharacters())
                {
                    if (!character.Dead)
                        charQueue.Enqueue(-character.Speed, character);
                }
                int portPriority = 0;

                while (charQueue.Count > 0)
                {
                    Character character = charQueue.Dequeue();
                    foreach (PassiveContext effectContext in character.IterateProximityPassives(context.User, context.ExplosionTile, portPriority))
                        effectContext.AddEventsToQueue(queue, maxPriority, ref nextPriority, ((ProximityData)effectContext.EventData).BeforeExplosions);

                    portPriority++;
                }

            };
            foreach (Tuple<GameEventOwner, Character, BattleEvent> effect in IterateEvents<BattleEvent>(function))
            {
                yield return CoroutineManager.Instance.StartCoroutine(effect.Item3.Apply(effect.Item1, effect.Item2, context));
                if (context.CancelState.Cancel) yield break;
            }

        }

        public IEnumerator<YieldInstruction> BeforeHit(BattleContext context)
        {
            EventEnqueueFunction<BattleEvent> function = (StablePriorityQueue<GameEventPriority, Tuple<GameEventOwner, Character, BattleEvent>> queue, Priority maxPriority, ref Priority nextPriority) =>
            {
                DataManager.Instance.UniversalEvent.AddEventsToQueue(queue, maxPriority, ref nextPriority, DataManager.Instance.UniversalEvent.BeforeHits);

                if (context.ActionType != BattleActionType.Trap)
                {
                    //check the action's effects
                    context.Data.AddEventsToQueue<BattleEvent>(queue, maxPriority, ref nextPriority, context.Data.BeforeHits);

                    foreach (PassiveContext effectContext in context.User.IteratePassives(GameEventPriority.USER_PORT_PRIORITY))
                        effectContext.AddEventsToQueue<BattleEvent>(queue, maxPriority, ref nextPriority, effectContext.EventData.BeforeHittings);

                }

                foreach (PassiveContext effectContext in context.Target.IteratePassives(GameEventPriority.TARGET_PORT_PRIORITY))
                    effectContext.AddEventsToQueue(queue, maxPriority, ref nextPriority, effectContext.EventData.BeforeBeingHits);

            };
            foreach (Tuple<GameEventOwner, Character, BattleEvent> effect in IterateEvents<BattleEvent>(function))
            {
                yield return CoroutineManager.Instance.StartCoroutine(effect.Item3.Apply(effect.Item1, effect.Item2, context));
                if (context.CancelState.Cancel) yield break;
            }

        }

        public IEnumerator<YieldInstruction> HitTarget(BattleContext context, Character target)
        {
            yield return CoroutineManager.Instance.StartCoroutine(BeforeHit(context));

            if (context.Hit)
                yield return CoroutineManager.Instance.StartCoroutine(ProcessHit(context));
        }


        public static int GetEffectiveness(Character attacker, Character target, BattleData action, int element)
        {
            int effectiveness = 0;

            EventEnqueueFunction<ElementEffectEvent> function = (StablePriorityQueue<GameEventPriority, Tuple<GameEventOwner, Character, ElementEffectEvent>> queue, Priority maxPriority, ref Priority nextPriority) =>
            {
                //start with universal
                DataManager.Instance.UniversalEvent.AddEventsToQueue(queue, maxPriority, ref nextPriority, DataManager.Instance.UniversalEvent.ElementEffects);

                //go through all statuses' element matchup methods
                if (attacker != null)
                {
                    foreach (PassiveContext effectContext in attacker.IteratePassives(GameEventPriority.USER_PORT_PRIORITY))
                        effectContext.AddEventsToQueue<ElementEffectEvent>(queue, maxPriority, ref nextPriority, effectContext.EventData.UserElementEffects);
                }
                if (target != null)
                {
                    foreach (PassiveContext effectContext in target.IteratePassives(GameEventPriority.TARGET_PORT_PRIORITY))
                        effectContext.AddEventsToQueue<ElementEffectEvent>(queue, maxPriority, ref nextPriority, effectContext.EventData.TargetElementEffects);
                }

                action.AddEventsToQueue<ElementEffectEvent>(queue, maxPriority, ref nextPriority, action.ElementEffects);
            };
            foreach (Tuple<GameEventOwner, Character, ElementEffectEvent> effect in IterateEvents<ElementEffectEvent>(function))
                effect.Item3.Apply(effect.Item1, effect.Item2, action.Element, element, ref effectiveness);

            return effectiveness;
        }

        public static int GetEffectiveness(Character attacker, Character target, int attacking, int defending)
        {
            int effectiveness = 0;

            EventEnqueueFunction<ElementEffectEvent> function = (StablePriorityQueue<GameEventPriority, Tuple<GameEventOwner, Character, ElementEffectEvent>> queue, Priority maxPriority, ref Priority nextPriority) =>
            {
                //start with universal
                DataManager.Instance.UniversalEvent.AddEventsToQueue(queue, maxPriority, ref nextPriority, DataManager.Instance.UniversalEvent.ElementEffects);

                //go through all statuses' element matchup methods
                if (attacker != null)
                {
                    foreach (PassiveContext effectContext in attacker.IteratePassives(GameEventPriority.USER_PORT_PRIORITY))
                        effectContext.AddEventsToQueue<ElementEffectEvent>(queue, maxPriority, ref nextPriority, effectContext.EventData.UserElementEffects);
                }
                if (target != null)
                {
                    foreach (PassiveContext effectContext in target.IteratePassives(GameEventPriority.TARGET_PORT_PRIORITY))
                        effectContext.AddEventsToQueue<ElementEffectEvent>(queue, maxPriority, ref nextPriority, effectContext.EventData.TargetElementEffects);
                }
            };
            foreach (Tuple<GameEventOwner, Character, ElementEffectEvent> effect in IterateEvents<ElementEffectEvent>(function))
                effect.Item3.Apply(effect.Item1, effect.Item2, attacking, defending, ref effectiveness);

            return effectiveness;
        }

        public IEnumerator<YieldInstruction> ProcessEndAnim(Character user, Character target, BattleData data)
        {

            //trigger animations of target
            StaticCharAnimation charAnim = data.HitCharAction.GetCharAnim();
            if (charAnim != null)
            {
                charAnim.AnimLoc = target.CharLoc;
                charAnim.CharDir = target.CharDir;
                yield return CoroutineManager.Instance.StartCoroutine(target.StartAnim(charAnim));
            }


            //play battle FX
            foreach (BattleFX fx in data.IntroFX)
                yield return CoroutineManager.Instance.StartCoroutine(ProcessBattleFX(user, target, fx));

            //play sound
            GameManager.Instance.BattleSE(data.HitFX.Sound);
            //the animation
            FiniteEmitter endEmitter = (FiniteEmitter)data.HitFX.Emitter.Clone();
            endEmitter.SetupEmit(target.MapLoc, user.MapLoc, target.CharDir);
            CreateAnim(endEmitter, DrawLayer.NoDraw);
            SetScreenShake(new ScreenMover(data.HitFX.ScreenMovement));
            yield return new WaitForFrames(GameManager.Instance.ModifyBattleSpeed(data.HitFX.Delay, target.CharLoc));
        }


        public IEnumerator<YieldInstruction> ProcessEmoteFX(Character user, EmoteFX fx)
        {
            //play sound
            GameManager.Instance.BattleSE(fx.Sound);
            //the animation
            user.StartEmote(new Emote(fx.Anim, fx.LocHeight, 1));
            yield return new WaitForFrames(GameManager.Instance.ModifyBattleSpeed(fx.Delay, user.CharLoc));
        }

        public IEnumerator<YieldInstruction> ProcessBattleFX(Character user, Character target, BattleFX fx)
        {
            yield return CoroutineManager.Instance.StartCoroutine(ProcessBattleFX(user.CharLoc, target.CharLoc, target.CharDir, fx));
        }
        public IEnumerator<YieldInstruction> ProcessBattleFX(Loc userLoc, Loc targetLoc, Dir8 userDir, BattleFX fx)
        {
            //play sound
            GameManager.Instance.BattleSE(fx.Sound);
            //the animation
            FiniteEmitter fxEmitter = (FiniteEmitter)fx.Emitter.Clone();
            fxEmitter.SetupEmit(targetLoc * GraphicsManager.TileSize, userLoc * GraphicsManager.TileSize, userDir);
            CreateAnim(fxEmitter, DrawLayer.NoDraw);
            SetScreenShake(new ScreenMover(fx.ScreenMovement));
            yield return new WaitForFrames(GameManager.Instance.ModifyBattleSpeed(fx.Delay, targetLoc));
        }


        public IEnumerator<YieldInstruction> ProcessHit(BattleContext context)
        {
            yield return CoroutineManager.Instance.StartCoroutine(context.Data.Hit(context));


            EventEnqueueFunction<BattleEvent> function = (StablePriorityQueue<GameEventPriority, Tuple<GameEventOwner, Character, BattleEvent>> queue, Priority maxPriority, ref Priority nextPriority) =>
            {
                if (context.ActionType != BattleActionType.Trap)
                {
                    foreach (PassiveContext effectContext in context.User.IteratePassives(GameEventPriority.USER_PORT_PRIORITY))
                        effectContext.AddEventsToQueue<BattleEvent>(queue, maxPriority, ref nextPriority, effectContext.EventData.AfterHittings);

                }

                foreach (PassiveContext effectContext in context.Target.IteratePassives(GameEventPriority.TARGET_PORT_PRIORITY))
                    effectContext.AddEventsToQueue(queue, maxPriority, ref nextPriority, effectContext.EventData.AfterBeingHits);

            };
            foreach (Tuple<GameEventOwner, Character, BattleEvent> effect in IterateEvents<BattleEvent>(function))
            {
                yield return CoroutineManager.Instance.StartCoroutine(effect.Item3.Apply(effect.Item1, effect.Item2, context));
                if (context.CancelState.Cancel) yield break;
            }
        }

        public IEnumerator<YieldInstruction> ReleaseHitboxes(Character user, Hitbox hitbox, HitboxEffect hitboxEffect, HitboxEffect tileEffect)
        {
            List<Hitbox> hitboxes = new List<Hitbox>();
            hitboxes.Add(hitbox);
            yield return CoroutineManager.Instance.StartCoroutine(ReleaseHitboxes(user, hitboxes, hitboxEffect, tileEffect));
        }
        public IEnumerator<YieldInstruction> ReleaseHitboxes(Character user, List<Hitbox> hitboxes, HitboxEffect hitboxEffect, HitboxEffect tileEffect)
        {
            yield return CoroutineManager.Instance.StartCoroutine(ReleaseHitboxes(user, hitboxes, Hitboxes, hitboxEffect, tileEffect));
        }

        public IEnumerator<YieldInstruction> ReleaseHitboxes(Character user, List<Hitbox> hitboxes, List<Hitbox> hitboxTo, HitboxEffect hitboxEffect, HitboxEffect tileEffect)
        {
            foreach (Hitbox hitbox in hitboxes)
            {
                //have all hitboxes pre-calculate their targets
                hitbox.PreCalculateAllTargets();
                hitbox.PreCalculateTileEmitters();
                //add the hitboxes to the screen
                hitboxTo.Add(hitbox);
            }

            yield return new WaitForFrames(GameManager.Instance.ModifyBattleSpeed(20, user.CharLoc, Settings.BattleSpeed.Slow));

            //set the NextStep to update the hit queue
            //this means that the hit queue will be checked at every frame if all events are clear
            //meanwhile, the update function can continue counting total time to keep subsequent hits consistent
            yield return CoroutineManager.Instance.StartCoroutine(ProcessHitQueue(hitboxes, hitboxEffect, tileEffect));
        }

        public IEnumerator<YieldInstruction> ProcessHitQueue(List<Hitbox> hitboxes, HitboxEffect burstEffect, HitboxEffect tileEffect)
        {
            //set the NextStep to update the hit queue; aka call this method again on next (available) update
            //stop doing it only if, for all hitboxes, everything has been hit and it's "done"
            //assume none of the hitboxes are blocking
            while (true)
            {
                StablePriorityQueue<int, HitboxHit> hitTargets = new StablePriorityQueue<int, HitboxHit>();

                foreach (Hitbox hitbox in hitboxes)
                    hitbox.UpdateHitQueue(hitTargets);

                //hit each target
                while (hitTargets.Count > 0)
                {
                    HitboxHit tile = hitTargets.Dequeue();
                    if (tile.Explode)
                        yield return CoroutineManager.Instance.StartCoroutine(burstEffect(tile.Loc));
                    else
                        yield return CoroutineManager.Instance.StartCoroutine(tileEffect(tile.Loc));
                }

                bool allDone = true;
                foreach (Hitbox hitbox in hitboxes)
                {
                    if (!hitbox.ProcessesDone())
                    {
                        allDone = false;
                        break;
                    }
                }
                if (allDone)
                    yield break;

                yield return new WaitForFrames(1);
            }
        }



        public delegate void EventEnqueueFunction<T>(StablePriorityQueue<GameEventPriority, Tuple<GameEventOwner, Character, T>> queue, Priority maxPriority, ref Priority nextPriority) where T : GameEvent;
        
        /// <summary>
        /// Iterates through all GameEvents gathered by the enqueue function, in order of priority.
        /// Each individual tier of priority is gathered and processed before searching for the next one.
        /// This is done so that GameEffects that affect other GameEffects at a further priority can take effect immediately.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enqueueFunction"></param>
        /// <returns></returns>
        public static IEnumerable<Tuple<GameEventOwner, Character, T>> IterateEvents<T>(EventEnqueueFunction<T> enqueueFunction) where T : GameEvent
        {
            Priority maxPriority = Priority.Invalid;
            while (true)
            {
                Priority nextPriority = Priority.Invalid;

                StablePriorityQueue<GameEventPriority, Tuple<GameEventOwner, Character, T>> queue = new StablePriorityQueue<GameEventPriority, Tuple<GameEventOwner, Character, T>>();

                enqueueFunction(queue, maxPriority, ref nextPriority);

                if (queue.Count == 0)
                    break;
                else
                {
                    while (queue.Count > 0)
                    {
                        Tuple<GameEventOwner, Character, T> effect = queue.Dequeue();
                        yield return effect;
                    }

                    maxPriority = nextPriority;
                }
            }
        }


        public Alignment GetMatchup(Character attacker, Character target)
        {
            return GetMatchup(attacker, target, true);
        }

        public Alignment GetMatchup(Character attacker, Character target, bool action)
        {
            if (attacker == null) return Alignment.Foe;
            if (target == null) return Alignment.Foe;
            if (attacker == target)
                return Alignment.Self;

            if (target.EnemyOfFriend && action)
                return Alignment.Foe;

            if (attacker.MemberTeam == target.MemberTeam)
                return Alignment.Friend;

            CharIndex attackerIndex = ZoneManager.Instance.CurrentMap.GetCharIndex(attacker);
            CharIndex targetIndex = ZoneManager.Instance.CurrentMap.GetCharIndex(target);
            if ((attackerIndex.Faction == Faction.Foe) == (targetIndex.Faction == Faction.Foe))
                return Alignment.Friend;
            
            return Alignment.Foe;
        }

        public bool IsTargeted(Character attacker, Character target, Alignment acceptedTargets)
        {
            if (attacker == null || target == null)
                return true;
            if (target.Dead)
                return false;
            Alignment alignment = GetMatchup(attacker, target);
            return (acceptedTargets & alignment) != 0;
        }

        public bool IsTargeted(Loc tile, TileAlignment tileAlignment)
        {
            if (!Collision.InBounds(ZoneManager.Instance.CurrentMap.Width, ZoneManager.Instance.CurrentMap.Height, tile))
                return false;

            if (tileAlignment == TileAlignment.None)
                return false;
            if (tileAlignment == TileAlignment.Any)
                return true;

            uint mobility = 0;
            mobility |= (1U << (int)TerrainData.Mobility.Lava);
            mobility |= (1U << (int)TerrainData.Mobility.Water);
            mobility |= (1U << (int)TerrainData.Mobility.Abyss);
            if (ZoneManager.Instance.CurrentMap.TileBlocked(tile, mobility))
                return true;
            else
                return false;
        }
    }
}

