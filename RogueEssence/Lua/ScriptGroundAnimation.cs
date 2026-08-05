using System;
using RogueEssence.Content;
using RogueEssence.Ground;
using RogueEssence.Dungeon;
using RogueEssence.Data;
using RogueElements;
using NLua;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace RogueEssence.Script
{
    /// <summary>
    /// Helper interface to regroup everything tied to ground mode under a single object
    /// </summary>
    public partial class ScriptGround : ILuaEngineComponent
    {

        //===================================
        //  Animation
        //===================================


        /// <summary>
        /// Make a character emote on the ground map.
        /// </summary>
        /// <param name="chara">Character to emote</param>
        /// <param name="emoteid">ID of the emote</param>
        /// <param name="cycles">The number of times to play the emote.</param>
        public void CharSetEmote(GroundChar chara, string emoteid, int cycles)
        {
            if (chara != null)
            {
                if (!String.IsNullOrEmpty(emoteid))
                {
                    EmoteData emote = DataManager.Instance.GetEmote(emoteid);
                    chara.StartEmote(new Emote(emote.Anim, emote.LocHeight, cycles));
                }
                else
                    chara.StartEmote(null);
            }
        }

        /// <summary>
        /// Sets the ground character's draw effect to become invisible, shaking, still, etc.
        /// </summary>
        /// <param name="chara">Target ground character.</param>
        /// <param name="effect">The draw effect.</param>
        public void CharSetDrawEffect(GroundChar chara, DrawEffect effect)
        {
            if (chara != null)
            {
                chara.SetDrawEffect(effect);
            }
        }

        /// <summary>
        /// Sets the ground character's draw effect to become invisible, shaking, still, etc.
        /// </summary>
        /// <param name="chara">Target ground character.</param>
        /// <param name="effect">The draw effect.</param>
        public void CharEndDrawEffect(GroundChar chara, DrawEffect effect)
        {
            if (chara != null)
            {
                chara.RemoveDrawEffect(effect);
            }
        }

        /// <summary>
        /// Gets the fallback animation for the character.
        /// </summary>
        /// <param name="chara"></param>
        /// <param name="anim">The anim to get the fallback anim of.</param>
        /// <returns>The fallback animation, as a string.  Blank if there is none.  Will return anim if anim already exists.</returns>
        public string CharGetAnimFallback(GroundChar chara, string anim)
        {
            int animIndex = GraphicsManager.GetAnimIndex(anim);
            CharSheet sheet = GraphicsManager.GetChara(chara.CurrentForm.ToCharID());
            int fallbackIndex = sheet.GetReferencedAnimIndex(animIndex);
            if (fallbackIndex < 0)
                return "";
            return GraphicsManager.Actions[fallbackIndex].Name;
        }


        /// <summary>
        /// Gets a character's current animation as a string.
        /// </summary>
        /// <param name="chara"></param>
        public string CharGetAnim(GroundChar chara)
        {
            GroundAction charaAction = chara.GetCurrentAction();
            int frameType = charaAction.AnimFrameType;
            if (frameType < 0)
                return "";
            return GraphicsManager.Actions[frameType].Name;
        }

        /// <summary>
        /// Gets the chosen action point of the character at this specific frame.
        /// </summary>
        /// <param name="chara"></param>
        /// <param name="actionPoint">The ype of action point to retrieve the coordinates for.</param>
        /// <returns>The location of the action point in absolute coordinates on the map.</returns>
        public Loc CharGetAnimPoint(GroundChar chara, ActionPointType actionPoint)
        {
            GroundAction charaAction = chara.GetCurrentAction();
            CharSheet sheet = GraphicsManager.GetChara(chara.CurrentForm.ToCharID());
            return charaAction.GetActionPoint(sheet, actionPoint);
        }

        /// <summary>
        /// Set a character's animation.
        /// </summary>
        /// <param name="chara">Character to animate</param>
        /// <param name="anim">Name of the animation</param>
        /// <param name="loop">Whether to loop the animation</param>
        public void CharSetAnim(GroundChar chara, string anim, bool loop)
        {
            int animIndex = GraphicsManager.GetAnimIndex(anim);
            chara.StartAction(new IdleAnimGroundAction(chara.Position, chara.LocHeight, chara.Direction, animIndex, loop));
        }

        /// <summary>
        /// Stops a character's current animation, reverting them to default idle.
        /// </summary>
        /// <param name="chara">Character to stop animating</param>
        public void CharEndAnim(GroundChar chara)
        {
            chara.StartAction(new IdleGroundAction(chara.Position, chara.LocHeight, chara.Direction));
        }

        /// <summary>
        /// Makes the character perform an animation and waits until it's over.
        /// </summary>
        /// <example>
        /// GROUND:CharWaitAnim(player, "Hurt")
        /// </example>
        public LuaFunction CharWaitAnim;

        /// <summary>
        /// [LuaFunction] CharWaitAnim
        /// </summary>
        /// <param name="ent">Character to animate</param>
        /// <param name="anim">Animation to play</param>
        /// <returns></returns>
        public YieldInstruction _CharWaitAnim(GroundEntity ent, string anim)
        {
            try
            {
                if (ent is GroundChar)
                {
                    GroundChar ch = (GroundChar)ent;
                    int animIndex = GraphicsManager.GetAnimIndex(anim);
                    IdleAnimGroundAction action = new IdleAnimGroundAction(ch.Position, ch.LocHeight, ch.Direction, animIndex, false);
                    ch.StartAction(action);
                    return new WaitUntil(() =>
                    {
                        return action.Complete || (ch.GetCurrentAction() != action);
                    });
                }
                throw new ArgumentException("Entity is not a valid type.");
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex, DiagManager.Instance.DevMode);
            }
            return null;
        }

        /// <summary>
        /// Set a character's action.
        /// </summary>
        /// <param name="chara">Character to perfom the action</param>
        /// <param name="action">The action to perform</param>
        public void CharSetAction(GroundChar chara, GroundAction action)
        {
            chara.StartAction(action);
        }


        /// <summary>
        /// Makes the character perform an action and waits until it's over.
        /// </summary>
        /// <example>
        /// GROUND:CharWaitAction(player, action)
        /// </example>
        public LuaFunction CharWaitAction;

        /// <summary>
        /// [LuaFunction] CharWaitAction
        /// </summary>
        /// <param name="ent">Character to animate</param>
        /// <param name="action">Action to perform</param>
        /// <returns></returns>
        public YieldInstruction _CharWaitAction(GroundEntity ent, GroundAction action)
        {
            try
            {
                if (ent is GroundChar)
                {
                    GroundChar ch = (GroundChar)ent;
                    ch.StartAction(action);
                    return new WaitUntil(() =>
                    {
                        return action.Complete || (ch.GetCurrentAction() != action);
                    });
                }
                throw new ArgumentException("Entity is not a valid type.");

            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex, DiagManager.Instance.DevMode);
            }
            return null;
        }

        /// <summary>
        /// Sets a ground object's animation.  After it finishes, it will return to the default animation.
        /// </summary>
        /// <param name="obj">The object to animate</param>
        /// <param name="frameTime">The duration of each frame of animation</param>
        /// <param name="startFrame">The start frame of animation</param>
        /// <param name="endFrame">The end frame of animation</param>
        /// <param name="dir">The direction of the animation</param>
        /// <param name="cycles">The number of times to repeat the animation</param>
        public void ObjectSetAnim(GroundObject obj, int frameTime, int startFrame, int endFrame, Dir8 dir, int cycles)
        {
            obj.StartAction(new ObjAnimData(obj.ObjectAnim.AnimIndex, frameTime, startFrame, endFrame, 255, dir), cycles);
        }

        /// <summary>
        /// Sets a ground object's default animation.
        /// </summary>
        /// <param name="obj">The object to animate</param>
        /// <param name="animName">The name of the animation</param>
        /// <param name="frameTime">The duration of each frame of animation</param>
        /// <param name="startFrame">The start frame of animation</param>
        /// <param name="endFrame">The end frame of animation</param>
        /// <param name="dir">The direction of the animation</param>
        public void ObjectSetDefaultAnim(GroundObject obj, string animName, int frameTime, int startFrame, int endFrame, Dir8 dir)
        {
            obj.ObjectAnim = new ObjAnimData(animName, frameTime, startFrame, endFrame, 255, dir);
        }


        /// <summary>
        /// Waits for the object to reach a specific frame before continuing.
        /// </summary>
        /// <example>
        /// GROUND:WaitObjectAnim(fountain, 3)
        /// </example>
        public LuaFunction ObjectWaitAnimFrame;

        /// <summary>
        /// [LuaFunction] ObjectWaitAnimFrame
        /// </summary>
        /// <param name="obj">The object to wait on</param>
        /// <param name="frame">The frame of animation to wait on.</param>
        /// <returns></returns>
        public YieldInstruction _ObjectWaitAnimFrame(GroundObject obj, int frame)
        {
            try
            {
                return new WaitUntil(() =>
                {
                    return obj.GetCurrentFrame() == frame;
                });
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex, DiagManager.Instance.DevMode);
            }
            return null;
        }


    }
}
