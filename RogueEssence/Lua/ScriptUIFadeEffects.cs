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
        /// Displays a voice over, waiting until the player completes it.
        /// </summary>
        /// <example>
        /// UI:WaitShowVoiceOver("Hello World!", 120)
        /// </example>
        public LuaFunction WaitShowVoiceOver;

        /// <summary>
        /// [LuaFunction] WaitShowVoiceOver
        /// Sets the current voice-over text to be shown.  Requires WaitDialog to actually display.
        /// </summary>
        /// <param name="text">The text to display.</param>
        /// <param name="expireTime">The time for the textbox to remain on screen. Pass -1 to wait for player input.</param>
        /// <param name="x">The starting X position of the box</param>
        /// <param name="y">The starting Y position of the box</param>
        /// <param name="width">The width of the box</param>
        /// <param name="height">The height of the box</param>
        /// <param name="callbacks">A Lua table of callbacks that the textbox can call using the [script] tag.</param>
        public void TextVoiceOver(string text, int expireTime, int x = -1, int y = -1, int width = -1, int height = -1, LuaTable callbacks = null)
        {
            try
            {
                object[] scripts = DialogueBox.CreateScripts(callbacks);
                Rect bounds = new Rect(x, y, width, height);
                if (DataManager.Instance.CurrentReplay == null)
                    m_curdialogue = MenuManager.Instance.SetTitleDialog(expireTime, m_curautoFinish, bounds, scripts, () => { }, text);
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.TextVoiceOver({0}, {1}): Encountered exception", text, expireTime), e), DiagManager.Instance.DevMode);
            }
        }

        /// <summary>
        /// Makes text pop up in the bottom-left corner by default. Displays concurrently with any other process.
        /// </summary>
        /// <param name="text">The text to display.</param>
        /// <param name="expireTime">The time for the textbox to remain on screen. Pass -1 to wait for player input.</param>
        /// <param name="x">The starting X position of the box</param>
        /// <param name="y">The starting Y position of the box</param>
        /// <param name="width">The width of the box</param>
        /// <param name="height">The height of the box</param>
        /// <param name="centerH">If true, centers the text horizontally.</param>
        /// <param name="centerV">If true, centers the text vertically.</param>
        public void TextPopUp(string text, int expireTime, int x = -1, int y = -1, int width = -1, int height = -1, bool centerH = false, bool centerV = false)
        {
            try
            {
                Rect bounds = new Rect(x, y, width, height);
                GameManager.Instance.TextPopUp.SetMessage(text, expireTime, bounds, centerH, centerV);
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.TextPopUp({0}, {1}): Encountered exception", text, expireTime), e), DiagManager.Instance.DevMode);
            }
        }

        /// <summary>
        /// Fades in text in the format of a title drop, then waits until the fade-in is complete.
        /// </summary>
        /// <example>
        /// UI:WaitShowTitle("Hello World!", 60)
        /// </example>
        public LuaFunction WaitShowTitle;

        /// <summary>
        /// [LuaFunction] WaitShowTitle
        /// Fades in text in the format of a title drop.  Requires WaitDialog to actually display.
        /// </summary>
        /// <param name="text">The text to display.</param>
        /// <param name="time">The time it takes for the text to fade in, in frames.</param>
        public void TextShowTitle(string text, int time)
        {
            try
            {
                m_curdialogue = GameManager.Instance.FadeTitle(true, text, time);
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.TextShowTitle({0}, {1}): Encountered exception", text, time), e), DiagManager.Instance.DevMode);
            }
        }

        /// <summary>
        /// Fades out the text set in a title drop, waiting until the fade-out is complete.
        /// </summary>
        /// <example>
        /// UI:WaitHideTitle(60)
        /// </example>
        public LuaFunction WaitHideTitle;

        /// <summary>
        /// [LuaFunction] WaitHideTitle
        /// Fades out the text set in a title drop.  Requires WaitDialog to actually fade.
        /// </summary>
        /// <param name="time">The time it takes for the text to fade out, in frames.</param>
        public void TextFadeTitle(int time)
        {
            try
            {
                m_curdialogue = GameManager.Instance.FadeTitle(false, "", time);
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.TextFadeTitle({0}, {1}): Encountered exception", time), e), DiagManager.Instance.DevMode);
            }
        }

        /// <summary>
        /// Fades in a chosen background image, with a chosen framerate, at a certain fade time, waiting until the fade-in is complete.
        /// </summary>
        /// <example>
        /// UI:WaitShowBG("TestBG", 3, 60)
        /// </example>
        public LuaFunction WaitShowBG;

        /// <summary>
        /// [LuaFunction] WaitShowBG
        /// Prepares to fade in a chosen background image, with a chosen framerate, at a certain fade time. Requires WaitDialog to actually display.
        /// </summary>
        /// <param name="bg">The background to show, from the list of BG textures.</param>
        /// <param name="frameTime">Framerate of the image animation.</param>
        /// <param name="fadeInTime">Time taken to fade in the image.</param>
        public void ShowBG(string bg, int frameTime, int fadeInTime)
        {
            try
            {
                m_curdialogue = GameManager.Instance.FadeBG(true, new BGAnimData(bg, frameTime), fadeInTime);
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.TextShowBG({0}, {1}, {2}): Encountered exception", bg, frameTime, fadeInTime), e), DiagManager.Instance.DevMode);
            }
        }

        /// <summary>
        /// Fades out the current background image, waiting until the fade-out is complete.
        /// </summary>
        /// <example>
        /// UI:WaitHideBG(60)
        /// </example>
        public LuaFunction WaitHideBG;

        /// <summary>
        /// [LuaFunction] WaitHideBG
        /// Prepares a fade-out of the current image. Requires WaitDialog to actually display.
        /// </summary>
        /// <param name="time">Time taken to fade out the image.</param>
        public void FadeBG(int time)
        {
            try
            {
                m_curdialogue = GameManager.Instance.FadeBG(false, new BGAnimData(), time);
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.ShowBG({0}): Encountered exception", time), e), DiagManager.Instance.DevMode);
            }
        }

    }
}
