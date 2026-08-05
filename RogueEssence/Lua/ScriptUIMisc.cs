using RogueEssence.Menu;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System;
using NLua;
using RogueEssence.Dungeon;
using RogueEssence.Content;
using RogueEssence.Network;
using RogueEssence.Data;
using RogueEssence.Dev;
using Microsoft.Xna.Framework;
using RogueElements;

namespace RogueEssence.Script
{
    public partial class ScriptUI : ILuaEngineComponent
    {
        /// <summary>
        /// Used to store a list of choice and their return value for most multi-choice menus.
        /// </summary>
        struct ChoicePair
        {
            string Text { get; set; }
            int Value { get; set; }
        }

        //Variables for storing multi-step operations, like setting the speaker in a dialog
        private object                 m_choiceresult = -1;
        private IEnumerator<YieldInstruction> m_curdialogue;
        private IInteractable m_curchoice;

        private MonsterID       m_curspeakerID = MonsterID.Invalid;
        private string              m_curspeakerName= "";
        private bool m_curcenter_h = false;
        private bool m_curcenter_v = false;
        private bool m_curautoFinish = false;
        private EmoteStyle  m_curspeakerEmo = new EmoteStyle(0);
        private bool                m_curspeakerSnd = true;
        private string m_curspeakerSe = DialogueBox.SOUND_EFFECT;
        private int m_curspeakTime = DialogueBox.SPEAK_FRAMES;
        private Rect m_curbounds = DialogueBox.DefaultBounds;
        private Loc m_curspeakerLoc = SpeakerPortrait.DefaultLoc;
        private Loc m_curchoiceLoc = DialogueChoiceMenu.DefaultLoc;

        public ScriptUI()
        {
            ResetSpeaker();
        }

        /// <summary>
        /// Resets te entire UI instance.
        /// </summary>
        public void Reset()
        {
            ResetSpeaker();
        }



        /// <summary>
        /// Initializes any LuaFunctions found in the class.
        /// Automatically on lua initialization.
        /// </summary>
        /// <param name="state">The lua engine to initialize with.</param>
        public override void SetupLuaFunctions(LuaEngine state)
        {
            WaitDialog = state.RunString(@"
            return function(_)
                return coroutine.yield(UI:_WaitDialog())
            end", "WaitDialog").First() as LuaFunction;

            WaitForChoice = state.RunString(@"
            return function(_)
                return coroutine.yield(UI:_WaitForChoice())
            end", "WaitForChoice").First() as LuaFunction;

            WaitShowDialogue = state.RunString(@"
            return function(_, text, callbacks)
                UI:TextDialogue(text, -1, callbacks)
                return coroutine.yield(UI:_WaitDialog())
            end", "WaitShowDialogue").First() as LuaFunction;

            WaitShowTimedDialogue = state.RunString(@"
            return function(_, text, time, callbacks)
                UI:TextDialogue(text, time, callbacks)
                return coroutine.yield(UI:_WaitDialog())
            end", "WaitShowDialogue").First() as LuaFunction;

            WaitShowVoiceOver = state.RunString(@"
            return function(_, text, expiretime, x, y, width, height, callbacks)
                x = x == nil and -1 or x
                y = y == nil and -1 or y
                width = width == nil and -1 or width
                height = height == nil and -1 or height
                UI:TextVoiceOver(text, expiretime, x, y, width, height, callbacks)
                return coroutine.yield(UI:_WaitDialog())
            end", "WaitShowVoiceOver").First() as LuaFunction;

            WaitInput = state.RunString(@"
            return function(_, any)
                UI:EmptyWaitMenu(any)
                return coroutine.yield(UI:_WaitDialog())
            end", "WaitInput").First() as LuaFunction;

            WaitForPlayerInput = state.RunString(@"
            return function(_, inputs)
                UI:EmptyWaitInputMenu(inputs)
                return coroutine.yield(UI:_WaitDialog())
            end", "WaitInput").First() as LuaFunction;

            WaitShowTitle = state.RunString(@"
            return function(_, text, time)
                UI:TextShowTitle(text, time)
                return coroutine.yield(UI:_WaitDialog())
            end", "WaitShowTitle").First() as LuaFunction;

            WaitHideTitle = state.RunString(@"
            return function(_, time)
                UI:TextFadeTitle(time)
                return coroutine.yield(UI:_WaitDialog())
            end", "WaitHideTitle").First() as LuaFunction;

            WaitShowBG = state.RunString(@"
            return function(_, text, frameTime, fadeInTime)
                UI:ShowBG(text, frameTime, fadeInTime)
                return coroutine.yield(UI:_WaitDialog())
            end", "WaitShowBG").First() as LuaFunction;

            WaitHideBG = state.RunString(@"
            return function(_, time)
                UI:FadeBG(time)
                return coroutine.yield(UI:_WaitDialog())
            end", "WaitHideBG").First() as LuaFunction;
        }
    }
}
