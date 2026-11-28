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

        //================================================================
        // Dialogue
        //================================================================

        /// <summary>
        /// Waits for the player to press a button before continuing.
        /// </summary>
        /// <example>
        /// UI:WaitInput(true)
        /// </example>
        public LuaFunction WaitInput;

        /// <summary>
        /// [LuaFunction] WaitInput
        /// </summary>
        /// <param name="anyInput">If false, only the Continue button will let the game continue. If true, any button will work.</param>
        public void EmptyWaitMenu(bool anyInput)
        {
            try
            {
                if (DataManager.Instance.CurrentReplay == null)
                {
                    FrameInput.InputType[] input = anyInput ? [] : [FrameInput.InputType.Confirm]; 
                    m_curdialogue = MenuManager.Instance.SetWaitMenu(input);
                }
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.EmptyWaitMenu({0}): Encountered exception", anyInput), e), DiagManager.Instance.DevMode);
            }
        }

        /// <summary>
        /// Waits for the player to press at least one of the given inputs before continuing.
        /// If an empty table is supplied, then this function behaves like WaitInput.
        /// </summary>
        /// <example>
        /// local input = RogueEssence.FrameInput.InputType
        /// 
        /// UI:WaitForPlayerInput({}) --This will accept any input
        /// UI:WaitForPlayerInput({input.Menu, input.Cancel}) --This only accepts the menu or cancel button specifically
        /// </example>
        public LuaFunction WaitForPlayerInput;

        /// <summary>
        /// [LuaFunction] WaitForPlayerInput
        /// </summary>
        /// <param name="inputs">A table of inputs, either as InputTypes or as ints. The game will continue if any one of them are hit. If empty, any button will work.</param>
        public void EmptyWaitInputMenu(LuaTable inputs)
        {
            List<FrameInput.InputType> inputTypes = [];
            foreach(var input in inputs.Values)
            {
                if (input is FrameInput.InputType inputType)
                    inputTypes.Add(inputType);
                else if (input is long inputTypeId && inputTypeId < (int)FrameInput.InputType.Count)
                    inputTypes.Add((FrameInput.InputType)inputTypeId);
            }
            try
            {
                if (DataManager.Instance.CurrentReplay == null)
                    m_curdialogue = MenuManager.Instance.SetWaitMenu(inputTypes.ToArray());
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.EmptyWaitInputMenu({0}): Encountered exception", inputs), e), DiagManager.Instance.DevMode);
            }
        }

        /// <summary>
        /// Displays a dialogue box with text, waiting until the player completes it.
        /// Takes a string and an optional callback as an argument.
        /// </summary>
        /// <example>
        /// UI:WaitShowDialogue("Hello World!")
        /// UI:WaitShowDialogue("Hello World!", callback)
        /// </example>
        public LuaFunction WaitShowDialogue;

        /// <summary>
        /// Displays a dialogue box with text, waiting until the specified time has expired.
        /// Takes a string, integer, and an optional callback as an argument.
        /// </summary>
        /// <example>
        /// UI:WaitShowTimedDialogue("Hello World!", 120)
        /// UI:WaitShowTimedDialogue("Hello World!", 120, callback)
        /// </example>
        public LuaFunction WaitShowTimedDialogue;

        /// <summary>
        /// [LuaFunction] WaitShowTimedDialogue
        /// Sets the current dialogue text to be shown.  Requires WaitDialog to actually display.
        /// </summary>
        /// <param name="text">The text to display in the textbox.</param>
        /// <param name="waitTime">The time for the textbox to remain on screen. Pass -1 to wait for layer input.</param>
        /// <param name="callbacks">The Lua table of callbacks that the textbox can call using the [script] tag.</param>
        public void TextDialogue(string text, int waitTime = -1, LuaTable callbacks = null)
        {
            try
            {
                object[] scripts = DialogueBox.CreateScripts(callbacks);
                if (DataManager.Instance.CurrentReplay == null)
                    m_curdialogue = MenuManager.Instance.SetDialogue(m_curspeakerID, m_curspeakerName, m_curspeakerEmo, m_curspeakerLoc, m_curspeakerSnd, m_curspeakerSe, m_curspeakTime, () => { }, waitTime, m_curautoFinish, m_curcenter_h, m_curcenter_v, m_curbounds, scripts, new string[] { text });
                else
                {
                    if (!String.IsNullOrEmpty(m_curspeakerName))
                        DungeonScene.Instance.LogMsg(String.Format("{0}: {1}", m_curspeakerName, text));
                    else
                        DungeonScene.Instance.LogMsg(text);
                }
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.TextDialogue({0}): Encountered exception.", text), e), DiagManager.Instance.DevMode);
            }
        }


        /// <summary>
        /// Ask a question answered by yes or no via character dialogue to the player.
        /// UI:WaitForChoice() must be called afterwards for the menu to be actually displayed,
        /// and for execution to suspend until the choice is returned.
        /// Then to retrieve the integer value indicating the result of the menu, UI:ChoiceResult() must be called.
        ///
        /// The Yes/No menu returns 1 for yes, and 0 for no.
        /// </summary>
        /// <param name="message">Question to be asked to the user.</param>
        /// <param name="bdefaultstono">Whether the cursor starts on no by default</param>
        /// <param name="callbacks">The Lua table of callbacks for the textbox to call.</param>
        public void ChoiceMenuYesNo(string message, bool bdefaultstono = false, LuaTable callbacks = null)
        {
            if (DataManager.Instance.CurrentReplay != null)
            {
                m_choiceresult = DataManager.Instance.CurrentReplay.ReadUI() == 0 ? false : true;
                return;
            }

            try
            {
                object[] scripts = DialogueBox.CreateScripts(callbacks);
                m_choiceresult = null;

                if (message == null)
                    message = "";

                m_curchoice = MenuManager.Instance.CreateQuestion(
                    m_curspeakerID, m_curspeakerName, m_curspeakerEmo, m_curspeakerLoc, message,
                    m_curspeakerSnd, m_curspeakerSe, m_curspeakTime,
                    m_curautoFinish, m_curcenter_h, m_curcenter_v, m_curbounds, scripts, m_curchoiceLoc,
                    () => { m_choiceresult = true; DataManager.Instance.LogUIPlay(1); },
                    () => { m_choiceresult = false; DataManager.Instance.LogUIPlay(0); },
                    bdefaultstono);
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.ChoiceMenuYesNo({0}): Encountered exception.", message), e), DiagManager.Instance.DevMode);
            }
        }

        /// <summary>
        /// Displays a custom dialogue of the caller's choice.
        /// 
        /// UI:WaitDialog() must be called afterwards for the dialogue to be actually displayed,
        /// and for execution to suspend until the dialogue is completed.
        /// </summary>
        public void SetCustomDialogue(DialogueBox dialogue)
        {
            try
            {
                m_curdialogue = MenuManager.Instance.ProcessMenuCoroutine(dialogue);
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.SetCustomDialogue(): Encountered exception."), e), DiagManager.Instance.DevMode);
            }
        }



    }
}
