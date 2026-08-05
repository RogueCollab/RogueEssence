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
        /// Marks the start of a multi-choice menu.
        /// UI:WaitForChoice() must be called afterwards for the menu to be actually displayed,
        /// and for execution to suspend until the choice is returned.
        /// Then to retrieve the integer value indicating the result of the menu, UI:ChoiceResult() must be called.
        /// </summary>
        /// <param name="message">The question to ask the user.</param>
        /// <param name="choicesPairs">A table of choices.  Each choice can be either a string, or { string, bool } representing the text and enabled status.</param>
        /// <param name="defaultChoice">The cursor starts on this choice.</param>
        /// <param name="cancelChoice">This choice is chosen if the player presses the cancel button.</param>
        /// <param name="callbacks">The Lua table of callbacks for the textbox to call.</param>
        public void BeginChoiceMenu(string message, LuaTable choicesPairs, object defaultChoice, object cancelChoice, LuaTable callbacks = null)
        {
            if (DataManager.Instance.CurrentReplay != null)
            {
                m_choiceresult = DataManager.Instance.CurrentReplay.ReadUI();
                return;
            }

            try
            {
                object[] scripts = DialogueBox.CreateScripts(callbacks);
                m_choiceresult = null;
                int? mappedDefault = null;
                int? mappedCancel = null;
                //Intepret the choices from lua
                List<DialogueChoice> choices = new List<DialogueChoice>();
                IDictionaryEnumerator dict = choicesPairs.GetEnumerator();
                while (dict.MoveNext())
                {
                    string choicetext = "";
                    bool enabled = true;
                    if (dict.Value is string)
                        choicetext = dict.Value as string;
                    else if (dict.Value is LuaTable)
                    {
                        LuaTable tbl = dict.Value as LuaTable;
                        choicetext = (string)tbl[1];
                        enabled = (bool)tbl[2];
                    }
                    long choiceval = (long)dict.Key;

                    if (defaultChoice.Equals(choiceval))
                        mappedDefault = choices.Count;
                    if (cancelChoice.Equals(choiceval))
                        mappedCancel = choices.Count;
                    choices.Add(new DialogueChoice(choicetext, () => { m_choiceresult = choiceval; DataManager.Instance.LogUIPlay((int)choiceval); }, enabled));
                }

                if (mappedDefault == null)
                    mappedDefault = 0;
                if (mappedCancel == null)
                    mappedCancel = -1;

                m_curchoice = MenuManager.Instance.CreateMultiQuestion(m_curspeakerID, m_curspeakerName, m_curspeakerEmo, m_curspeakerLoc,
                            message, m_curspeakerSnd, m_curspeakerSe, m_curspeakTime, m_curautoFinish, m_curcenter_h, m_curcenter_v, m_curbounds, scripts, m_curchoiceLoc, choices.ToArray(), mappedDefault.Value, mappedCancel.Value);
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.BeginChoiceMenu({0}): Encountered exception.", message), e), DiagManager.Instance.DevMode);
            }
        }

        /// <summary>
        /// Marks the start of a multi-paged choice menu.
        /// UI:WaitForChoice() must be called afterwards for the menu to be actually displayed,
        /// and for execution to suspend until the choice is returned.
        /// Then to retrieve the integer value indicating the result of the menu, UI:ChoiceResult() must be called.
        /// </summary>
        /// <param name="x">X position of the menu</param>
        /// <param name="y">Y position of the menu</param>
        /// <param name="width">Width of the menu</param>
        /// <param name="title">Height of the menu</param>
        /// <param name="choicesPairs">A table of choices.  Each choice can be either a string, or { string, bool } representing the text and enabled status.</param>
        /// <param name="linesPerPage">Number of choices per page</param>
        /// <param name="defaultChoice">The cursor starts on this choice.</param>
        /// <param name="cancelChoice">This choice is chosen if the player presses the cancel button.</param>
        public void BeginMultiPageMenu(int x, int y, int width, string title, LuaTable choicesPairs, int linesPerPage, object defaultChoice, object cancelChoice)
        {
            if (DataManager.Instance.CurrentReplay != null)
            {
                m_choiceresult = DataManager.Instance.CurrentReplay.ReadUI();
                return;
            }

            try
            {
                m_choiceresult = null;
                int? mappedDefault = null;
                int? mappedCancel = null;
                //Intepret the choices from lua
                List<MenuTextChoice> choices = new List<MenuTextChoice>();
                IDictionaryEnumerator dict = choicesPairs.GetEnumerator();
                while (dict.MoveNext())
                {
                    string choicetext = "";
                    bool enabled = true;
                    if (dict.Value is string)
                        choicetext = dict.Value as string;
                    else if (dict.Value is LuaTable)
                    {
                        LuaTable tbl = dict.Value as LuaTable;
                        choicetext = (string)tbl[1];
                        enabled = (bool)tbl[2];
                    }
                    long choiceval = (long)dict.Key;

                    if (defaultChoice.Equals(choiceval))
                        mappedDefault = choices.Count;
                    if (cancelChoice.Equals(choiceval))
                        mappedCancel = choices.Count;

                    choices.Add(new MenuTextChoice(choicetext, () => { MenuManager.Instance.RemoveMenu(); m_choiceresult = choiceval; DataManager.Instance.LogUIPlay((int)choiceval); }, enabled, enabled ? Color.White : Color.Red));
                }

                if (mappedDefault == null)
                    mappedDefault = 0;

                Action cancelAction = null;
                if (mappedCancel != null)
                {
                    cancelAction = () => { MenuManager.Instance.RemoveMenu(); m_choiceresult = (int)(long)cancelChoice; DataManager.Instance.LogUIPlay((int)(long)cancelChoice); };
                }

                //Make a choice menu, and check if we display a speaker or not
                m_curchoice = new ScriptableMultiPageMenu(new Loc(x, y), width, title, choices.ToArray(), mappedDefault.Value, linesPerPage, cancelAction, cancelAction);
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.BeginMultiPageMenu({0}): Encountered exception.", title), e), DiagManager.Instance.DevMode);
            }
        }


        /// <summary>
        /// Displays a custom menu of the caller's choice.
        /// 
        /// UI:WaitForChoice() must be called afterwards for the menu to be actually displayed,
        /// and for execution to suspend until the menu is exited.
        /// </summary>
        public void SetCustomMenu(InteractableMenu menu)
        {
            try
            {
                m_choiceresult = null;
                m_curchoice = menu;
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.SetCustomMenu(): Encountered exception."), e), DiagManager.Instance.DevMode);
            }
        }

        /// <summary>
        /// Get the result of the last choice menu
        /// </summary>
        /// <returns>The result of the choice</returns>
        public object ChoiceResult()
        {
            return m_choiceresult;
        }

        /// <summary>
        /// Creates an action that sets the choice result to a specified object.
        /// </summary>
        /// <param name="obj">The object to set the choice result to, if the action was fired.</param>
        /// <returns>This action will set the choice result.</returns>
        public Action GetChoiceAction(object obj)
        {
            return () =>
            {
                MenuManager.Instance.RemoveMenu();
                m_choiceresult = obj;
            };
        }


        /// <summary>
        /// Displays the currently set choice menu and waits for the player's selection to complete.
        /// </summary>
        /// <example>
        /// UI:WaitForChoice()
        /// </example>
        public LuaFunction WaitForChoice;

        /// <summary>
        /// [LuaFunction] WaitForChoice
        /// </summary>
        /// <returns></returns>
        public Coroutine _WaitForChoice()
        {
            if (DataManager.Instance.CurrentReplay != null)
                return new Coroutine(_DummyWait());

            if (m_curchoice != null)
                return new Coroutine(_waitForChoice());
            else
                return new Coroutine(_DummyWait());

        }

        /// <summary>
        /// Wait for choice and then CLEAN UP m_curchoice
        /// </summary>
        /// <returns></returns>
        private IEnumerator<YieldInstruction> _waitForChoice()
        {
            yield return CoroutineManager.Instance.StartCoroutine(MenuManager.Instance.ProcessMenuCoroutine(m_curchoice));
        }

        /// <summary>
        /// Opens the provided menu and starts the menu coroutine.
        /// This should be called only from non-coroutine sources.
        /// </summary>
        /// <param name="menu"></param>
        /// <returns></returns>
        /// <example>
        /// TASK:WaitTask(UI:ProcessMenuCoroutine(MenuTools.MainMenu))
        /// </example>
        public Coroutine ProcessMenuCoroutine(object menu)
        {
            try
            {
                IInteractable imenu = (IInteractable)menu;
                return new Coroutine(MenuManager.Instance.ProcessMenuCoroutine(imenu));
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex);
                return null;
            }
        }

    }
}
