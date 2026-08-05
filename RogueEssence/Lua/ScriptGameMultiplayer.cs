using RogueEssence.Ground;
using RogueEssence.Dungeon;
using RogueEssence.Data;
using System.Linq;

using NLua;
using System.Collections.Generic;
using RogueElements;
using System;

namespace RogueEssence.Script
{
    public partial class ScriptGame : ILuaEngineComponent
    {


        //===================================
        // Multiplayer
        //===================================


        /// <summary>
        /// Enters a zone and begins a rescue adventure.
        /// </summary>
        /// <example>
        /// GAME:EnterRescue("RESCUE/INBOX/SOS/example.sosmail")
        /// </example>
        public LuaFunction EnterRescue;

        /// <summary>
        /// [LuaFunction] EnterRescue
        /// </summary>
        /// <param name="sosPath">The path of the sos mail.</param>
        /// <returns></returns>
        public Coroutine _EnterRescue(string sosPath)
        {
            return new Coroutine(GameManager.Instance.BeginRescue(sosPath));
        }

        /// <summary>
        /// TODO: WIP
        /// </summary>
        /// <param name="remarkIndex"></param>
        public void AddAOKRemark(int remarkIndex)
        {
            AOKMail aok = null;
            if (DataManager.Instance.Save.GeneratedAOK != null)
                aok = DataManager.LoadRescueMail(PathMod.FromApp(DataManager.RESCUE_OUT_PATH + DataManager.AOK_FOLDER + DataManager.Instance.Save.GeneratedAOK)) as AOKMail;
            if (aok != null)
            {
                aok.FinalStatement = remarkIndex;
                DataManager.SaveRescueMail(DataManager.Instance.Save.GeneratedAOK, aok);
            }
        }


        /// <summary>
        /// TODO
        /// </summary>
        /// <returns></returns>
        public bool HasSOSMail()
        {
            string parentPath = PathMod.FromApp(DataManager.RESCUE_IN_PATH + DataManager.SOS_FOLDER);
            string[] files = System.IO.Directory.GetFiles(parentPath, "*" + DataManager.SOS_EXTENSION);
            return files.Length > 0;
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <returns></returns>
        public bool HasAOKMail()
        {
            string parentPath = PathMod.FromApp(DataManager.RESCUE_OUT_PATH + DataManager.AOK_FOLDER);
            string[] files = System.IO.Directory.GetFiles(parentPath, "*" + DataManager.AOK_EXTENSION);
            return files.Length > 0;
        }


        /// <summary>
        /// Returns true if there is at least one server in the server list.
        /// </summary>
        /// <returns></returns>
        public bool HasServerSet()
        {
            return DiagManager.Instance.CurSettings.ServerList.Count > 0;
        }

        /// <summary>
        /// Checks to see if rescue is allowed.
        /// </summary>
        /// <returns>True if rescues are allowed, false otherwise.</returns>
        public bool GetRescueAllowed()
        {
            return DataManager.Instance.Save.AllowRescue;
        }

        /// <summary>
        /// Sets the value in the player's save file to determine if they can be rescued or not.
        /// If rescue is possible on the Save File level, it can still be prevented by the map.
        /// </summary>
        /// <param name="allowed">Set to true to allow the player to be rescued.  False otherwise.</param>
        public void SetRescueAllowed(bool allowed)
        {
            DataManager.Instance.Save.AllowRescue = allowed;
        }


    }
}
