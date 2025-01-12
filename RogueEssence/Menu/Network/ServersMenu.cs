using System.Collections.Generic;
using Microsoft.Xna.Framework;
using RogueElements;
using RogueEssence.Network;

namespace RogueEssence.Menu
{
    public class ServersMenu : MultiPageMenu
    {
        private const int SLOTS_PER_PAGE = 14;

        public ServersMenu() : this(MenuLabel.SERVERS_MENU) { }
        public ServersMenu(string label)
        {
            Label = label;
            List<MenuChoice> flatChoices = new List<MenuChoice>();
            for (int ii = 0; ii < DiagManager.Instance.CurSettings.ServerList.Count; ii++)
            {
                int index = ii;
                ServerInfo record = DiagManager.Instance.CurSettings.ServerList[ii];
                string entryText = null;
                if (record.ServerName.Length > 0)
                    entryText = record.ServerName + " ["+ record.IP + ":" + record.Port+"]";
                else
                    entryText = record.IP + ":" + record.Port;
                flatChoices.Add(new MenuTextChoice(entryText, () => { choose(index); }, true, ii > 0 ? Color.White : TextIndigo));
            }
            flatChoices.Add(new MenuTextChoice(Text.FormatKey("MENU_ADD_NEW"), startAddNew, true, Color.Yellow));
            IChoosable[][] choices = SortIntoPages(flatChoices.ToArray(), SLOTS_PER_PAGE);

            Initialize(new Loc(8, 8), 224, Text.FormatKey("MENU_SERVERS_TITLE"), choices, 0, 0, SLOTS_PER_PAGE);
        }

        private void choose(int index)
        {
            MenuManager.Instance.AddMenu(new ServerChosenMenu(index), true);
        }

        private void startAddNew()
        {
            MenuManager.Instance.AddMenu(new HostInputMenu(addNew, Text.FormatKey("INPUT_IP_TITLE"), Text.FormatKey("INPUT_IP_SUB")), false);
        }

        private void addNew(string newIP)
        {
            int port = NetworkManager.DEFAULT_PORT;
            string[] serverPort = newIP.Split(':');
            if (serverPort.Length > 1)
            {
                ushort newPort = 0;
                if (ushort.TryParse(serverPort[1], out newPort))
                    port = newPort;
            }

            ServerInfo newInfo = new ServerInfo("", serverPort[0], port);

            DiagManager.Instance.CurSettings.ServerList.Add(newInfo);
            DiagManager.Instance.SaveSettings(DiagManager.Instance.CurSettings);

            MenuManager.Instance.ReplaceMenu(new ServersMenu());
        }
    }
}
