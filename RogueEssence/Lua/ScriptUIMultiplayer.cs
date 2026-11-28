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
        // Multiplayer Menus
        //================================================================

        /// <summary>
        /// Displays the Servers menu.
        /// </summary>
        public void ServersMenu()
        {
            try
            {
                m_choiceresult = null;
                m_curchoice = new ServersMenu();
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.ServersMenu(): Encountered exception."), e), DiagManager.Instance.DevMode);
            }
        }

        /// <summary>
        /// Displays the contacts menu.
        /// </summary>
        public void ContactsMenu()
        {
            try
            {
                m_choiceresult = 0;
                m_curchoice = new ContactsMenu(false, (OnlineActivity activity) => { m_choiceresult = 1; NetworkManager.Instance.PrepareActivity(activity); });
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.ContactsMenu(): Encountered exception."), e), DiagManager.Instance.DevMode);
            }
        }

        /// <summary>
        /// Displays the SOS menu.
        /// </summary>
        public void SOSMenu()
        {
            try
            {
                m_choiceresult = null;
                m_curchoice = new MailMenu(true, (string fileName) => { m_choiceresult = fileName; });
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.SOSMenu(): Encountered exception."), e), DiagManager.Instance.DevMode);
            }
        }

        /// <summary>
        /// Displays the AOK menu.
        /// </summary>
        public void AOKMenu()
        {
            try
            {
                m_choiceresult = null;
                m_curchoice = new MailMenu(false, (string fileName) => { });
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.AOKMenu(): Encountered exception."), e), DiagManager.Instance.DevMode);
            }
        }

        /// <summary>
        /// Displays the Peers menu.
        /// </summary>
        public void PeersMenu()
        {
            try
            {
                m_choiceresult = 0;
                m_curchoice = new PeersMenu((OnlineActivity activity) => { m_choiceresult = 1; NetworkManager.Instance.PrepareActivity(activity/*, true*/); });
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.PeersMenu(): Encountered exception."), e), DiagManager.Instance.DevMode);
            }
        }

        /// <summary>
        /// Displays the connection menu.
        /// </summary>
        public void ShowConnectMenu()
        {
            try
            {
                m_choiceresult = 0;
                m_curchoice = new ConnectingMenu(() => { m_choiceresult = 1; });
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.ConnnectMenu(): Encountered exception."), e), DiagManager.Instance.DevMode);
            }
        }

        /// <summary>
        /// Displays the current activity of the connected peer.
        /// </summary>
        public void CurrentActivityMenu()
        {
            try
            {
                m_choiceresult = "";
                switch (NetworkManager.Instance.Activity.Activity)
                {
                    case ActivityType.TradeTeam:
                        m_curchoice = new TradeTeamMenu(0);
                        break;
                    case ActivityType.TradeItem:
                        m_curchoice = new TradeItemMenu(0);
                        break;
                    case ActivityType.SendHelp:
                        m_curchoice = new SendHelpMenu();
                        break;
                }
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.CurrentActivityMenu(): Encountered exception."), e), DiagManager.Instance.DevMode);
            }
        }


        /// <summary>
        /// Marks the start of a choice menu for choosing monsters, showing a preview of their appearances via portrait.
        /// UI:WaitForChoice() must be called afterwards for the menu to be actually displayed,
        /// and for execution to suspend until the choice is returned.
        /// Then to retrieve the string indicating the chosen species, UI:ChoiceResult() must be called.
        /// </summary>
        /// <param name="title">The title of the menu</param>
        /// <param name="choices">A lua table of choices with each element being a MonsterID.</param>
        /// <param name="canMenu">If set to true, the Menu Button exits the menu if pressed.</param>
        /// <param name="canCancel">If set to true, the Cancel Button exits the menu if pressed.</param>
        /// <param name="slotsPerPage">Slots to display per page</param>
        public void ChooseMonsterMenu(string title, LuaTable choices, bool canMenu = false, bool canCancel = false, int slotsPerPage = 12)
        {
            try
            {
                m_choiceresult = null;

                List<StartChar> monsters = new List<StartChar>();

                for (int ii = 1; choices[ii] is not null; ii++)
                {
                    var choice = choices[ii];
                    if (choice is MonsterID monster)
                        monsters.Add(new StartChar(monster, ""));
                    else
                        throw new ArgumentException($"Table must be array of '{nameof(MonsterID)}'", nameof(choices));
                }

                if (monsters.Count == 0)
                    throw new ArgumentException($"Table must be array of one or more '{nameof(MonsterID)}'", nameof(choices));

                void chooseAction(int slot)
                {
                    m_choiceresult = monsters[slot].ID;
                    MenuManager.Instance.RemoveMenu();
                }

                void cancelAction() { }

                m_curchoice = new ChooseMonsterMenu(title, monsters, 0, chooseAction, canCancel ? cancelAction : null, canMenu, slotsPerPage);
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.ChooseMonsterMenu(): Encountered exception."), e), DiagManager.Instance.DevMode);
            }
        }


    }
}
