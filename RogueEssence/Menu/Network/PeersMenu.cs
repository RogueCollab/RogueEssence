using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RogueElements;
using RogueEssence.Data;
using RogueEssence.Network;
using RogueEssence.Content;
using RogueEssence.Dungeon;

namespace RogueEssence.Menu
{
    public class PeersMenu : MultiPageMenu
    {
        private const int SLOTS_PER_PAGE = 8;

        private ContactsMenu.OnChooseActivity action;

        ContactMiniSummary summaryMenu;

        private bool canTrade;
        private bool hasSwappable;

        //TODO: this class may not be needed since peer to peer communication can
        //be done by just setting the server to the target IP
        public PeersMenu(ContactsMenu.OnChooseActivity action)
        {
            this.action = action;

            canTrade = DataManager.Instance.Save.ActiveTeam.Assembly.Count > 0;
            hasSwappable = false;


            bool[] itemPresence = new bool[DataManager.Instance.DataIndices[DataManager.DataType.Item].Count];
            for (int ii = 0; ii < itemPresence.Length; ii++)
            {
                if (DataManager.Instance.Save.ActiveTeam.Storage[ii] > 0)
                {
                    if (updatePresence(ii, itemPresence))
                    {
                        hasSwappable = true;
                        break;
                    }
                }
            }

            List<MenuChoice> flatChoices = new List<MenuChoice>();
            for (int ii = 0; ii < DiagManager.Instance.CurSettings.PeerList.Count; ii++)
            {
                int index = ii;
                PeerInfo record = DiagManager.Instance.CurSettings.PeerList[ii];
                string entryText = null;
                if (record.Data.TeamName.Length > 0)
                    entryText = record.Data.TeamName;
                else
                    entryText = Text.FormatKey("MENU_NEW_CONTACT") + " ["+record.IP + "]";
                flatChoices.Add(new MenuTextChoice(entryText, () => { choose(index); }));
            }
            flatChoices.Add(new MenuTextChoice(Text.FormatKey("MENU_ADD_NEW"), startAddNew, true, Color.Yellow));
            List<MenuChoice[]> choices = SortIntoPages(flatChoices, SLOTS_PER_PAGE);

            summaryMenu = new ContactMiniSummary(Rect.FromPoints(new Loc(8,
                GraphicsManager.ScreenHeight - 8 - GraphicsManager.MenuBG.TileHeight * 2 - VERT_SPACE * 4),
                new Loc(GraphicsManager.ScreenWidth - 8, GraphicsManager.ScreenHeight - 8)));

            Initialize(new Loc(8, 8), 196, Text.FormatKey("MENU_CONTACTS_TITLE"), choices.ToArray(), 0, 0, SLOTS_PER_PAGE);
        }
        private bool updatePresence(int index, bool[] itemPresence)
        {
            if (!itemPresence[index])
            {
                //TODO: make this calculation not require item loading.
                itemPresence[index] = true;
                ItemData entry = DataManager.Instance.GetItem(index);
                if (entry.ItemStates.Contains<MaterialState>())
                    return true;
            }
            return false;
        }

        private void choose(int index)
        {
            PeerInfo peer = DiagManager.Instance.CurSettings.PeerList[index];
            MenuManager.Instance.AddMenu(new ContactChosenMenu(peer, new ServerInfo("", peer.IP, peer.Port), canTrade, hasSwappable, false, action, () => { DeleteAction(index); } ), true);
        }

        private void startAddNew()
        {
            MenuManager.Instance.AddMenu(new HostInputMenu(addNew, Text.FormatKey("INPUT_CONTACT_TITLE"), Text.FormatKey("INPUT_IP_SUB")), false);
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

            PeerInfo newInfo = new PeerInfo(serverPort[0], port);

            //bool alreadyExists = false;
            //foreach (ContactInfo info in DiagManager.Instance.CurSettings.ContactList)
            //{
            //    if (info.UUID == newID)
            //    {
            //        alreadyExists = true;
            //        break;
            //    }
            //}

            DiagManager.Instance.CurSettings.PeerList.Add(newInfo);
            DiagManager.Instance.SaveSettings(DiagManager.Instance.CurSettings);
            MenuManager.Instance.ReplaceMenu(new PeersMenu(action));
        }

        private void DeleteAction(int totalChoice)
        {
            DiagManager.Instance.CurSettings.PeerList.RemoveAt(totalChoice);
            DiagManager.Instance.SaveSettings(DiagManager.Instance.CurSettings);

            MenuManager.Instance.RemoveMenu();

            MenuManager.Instance.ReplaceMenu(new PeersMenu(action));
        }

        protected override void ChoiceChanged()
        {
            int totalChoice = CurrentChoice + CurrentPage * SLOTS_PER_PAGE;
            if (totalChoice < DiagManager.Instance.CurSettings.PeerList.Count)
            {
                summaryMenu.Visible = true;
                PeerInfo info = DiagManager.Instance.CurSettings.PeerList[totalChoice];
                summaryMenu.SetContact((info.Data.TeamName == "") ? "[" + Text.FormatKey("MENU_NEW_CONTACT") + "]" : info.Data.TeamName,
                    Text.FormatKey("MENU_CONTACT_RANK", info.Data.GetLocalRankStr()),
                    Text.FormatKey("MENU_CONTACT_LAST_SEEN", info.LastContact),
                    Text.FormatKey("MENU_CONTACT_IP", info.IP + ":" + info.Port),
                    info.Data.TeamProfile);
            }
            else
                summaryMenu.Visible = false;

            base.ChoiceChanged();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            //draw other windows
            summaryMenu.Draw(spriteBatch);
        }
    }
}
