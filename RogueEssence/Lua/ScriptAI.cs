using RogueEssence.Ground;
using System;

namespace RogueEssence.Script
{
    /// <summary>
    /// Handles script interactions with character AI
    /// </summary>
    public class ScriptAI : ILuaEngineComponent
    {

        /// <summary>
        /// Assign the given scripted AI class to the specified GroundChar.
        /// </summary>
        /// <param name="ch">The character to apply the AI to.</param>
        /// <param name="classpath">The Scripted AI to apply.</param>
        /// <param name="args"></param>
        public void SetCharacterAI(GroundChar ch, string classpath, params object[] args)
        {
            try
            {
                //!#TODO: Could add a check to see if the class can be loaded and return a boolean result?
                ch.SetAI(new GroundScriptedAI(classpath, args));
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex, DiagManager.Instance.DevMode);
            }
        }

        /// <summary>
        /// Disable a given groundchar's AI processing until its enabled again.
        /// </summary>
        /// <param name="ch">The character to disable the AI of. </param>
        public void DisableCharacterAI(GroundChar ch)
        {
            try
            {
                ch.AIEnabled = false;
            }
            catch(Exception ex)
            {
                DiagManager.Instance.LogError(ex, DiagManager.Instance.DevMode);
            }
        }

        /// <summary>
        /// Enable a given groundchar's AI processing if its currently disabled.
        /// </summary>
        /// <param name="ch">The character to enable the AI of. </param>
        public void EnableCharacterAI(GroundChar ch)
        {
            try
            {
                ch.AIEnabled = true;
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex, DiagManager.Instance.DevMode);
            }
        }

        /// <summary>
        /// Force the AI to change to the specified state if it exists
        /// </summary>
        /// <param name="ch">The character of which to force the AI state.</param>
        /// <param name="state">The state to force the AI to.</param>
        public void SetAIState(GroundChar ch, string state)
        {
            try
            {
                ch.SetAIState(state);
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex, DiagManager.Instance.DevMode);
            }
        }

        /// <summary>
        /// Initializes any LuaFunctions found in the class.
        /// Automatically on lua initialization.
        /// </summary>
        /// <param name="state">The lua engine to initialize with.</param>
        public override void SetupLuaFunctions(LuaEngine state)
        {
        }
    }
}
