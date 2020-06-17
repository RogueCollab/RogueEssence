using RogueEssence.Ground;
using System;

namespace RogueEssence.Script
{
    /// <summary>
    /// Handles script interactions with character AI
    /// </summary>
    class ScriptAI : ILuaEngineComponent
    {

        /// <summary>
        /// Assign the given scripted AI class to the specified GroundChar.
        /// </summary>
        /// <param name="ch"></param>
        /// <param name="classpath"></param>
        public void SetCharacterAI(GroundChar ch, string classpath, params object[] args)
        {
            try
            {
                //!#TODO: Could add a check to see if the class can be loaded and return a boolean result?
                ch.SetAI(new GroundScriptedAI(classpath, args));
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogInfo(String.Format("ScriptAI.SetCharacterAI(): Got exception :\n{0}", ex.Message));
            }
        }

        /// <summary>
        /// Disable a given groundchar's AI processing until its enabled again.
        /// </summary>
        /// <param name="ch"></param>
        public void DisableCharacterAI(GroundChar ch)
        {
            try
            {
                ch.AIEnabled = false;
            }
            catch(Exception ex)
            {
                DiagManager.Instance.LogInfo(String.Format("ScriptAI.DisableCharacterAI(): Got exception :\n{0}", ex.Message));
            }
        }

        /// <summary>
        /// Enable a given groundchar's AI processing if its currently disabled
        /// </summary>
        /// <param name="ch"></param>
        public void EnableCharacterAI(GroundChar ch)
        {
            try
            {
                ch.AIEnabled = true;
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogInfo(String.Format("ScriptAI.DisableCharacterAI(): Got exception :\n{0}", ex.Message));
            }
        }


        public override void SetupLuaFunctions(LuaEngine state)
        {
        }
    }
}
