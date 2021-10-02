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
    public class ContactsMenu : MultiPageMenu
    {
        private const int SLOTS_PER_PAGE = 8;

        public delegate void OnChooseActivity(OnlineActivity activity);
        private OnChooseActivity action;

        ContactMiniSummary summaryMenu;

        private bool rescueMode;
        private bool canTrade;
        private bool hasSwappable;

        public ContactsMenu(bool rescueMode, OnChooseActivity action)
        {
            this.rescueMode = rescueMode;
            this.action = action;


            if (!rescueMode)
            {
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
            }

            List<MenuChoice> flatChoices = new List<MenuChoice>();
            flatChoices.Add(new MenuTextChoice(DataManager.Instance.Save.ActiveTeam.GetDisplayName(), () => { chooseSelf(DataManager.Instance.Save.UUID); }, true, TextIndigo));
            for (int ii = 0; ii < DiagManager.Instance.CurSettings.ContactList.Count; ii++)
            {
                int index = ii;
                ContactInfo record = DiagManager.Instance.CurSettings.ContactList[ii];
                string entryText = null;
                if (record.Data.TeamName.Length > 0)
                    entryText = record.Data.TeamName;
                else
                    entryText = Text.FormatKey("MENU_NEW_CONTACT") + " ["+record.UUID.Substring(0, 8) + "]";
                flatChoices.Add(new MenuTextChoice(entryText, () => { choose(index); }));
            }
            flatChoices.Add(new MenuTextChoice(Text.FormatKey("MENU_ADD_NEW"), startAddNew, true, Color.Yellow));
            IChoosable[][] choices = SortIntoPages(flatChoices.ToArray(), SLOTS_PER_PAGE);


            summaryMenu = new ContactMiniSummary(Rect.FromPoints(new Loc(8,
                GraphicsManager.ScreenHeight - 8 - GraphicsManager.MenuBG.TileHeight * 2 - VERT_SPACE * 4),
                new Loc(GraphicsManager.ScreenWidth - 8, GraphicsManager.ScreenHeight - 8)));

            Initialize(new Loc(8, 8), 196, Text.FormatKey("MENU_CONTACTS_TITLE"), choices, 0, 0, SLOTS_PER_PAGE);

        }


        private bool updatePresence(int index, bool[] itemPresence)
        {
            if (!itemPresence[index])
            {
                itemPresence[index] = true;
                ItemEntrySummary itemEntry = DataManager.Instance.DataIndices[DataManager.DataType.Item].Entries[index] as ItemEntrySummary;
                if (itemEntry.ContainsState<MaterialState>())
                    return true;
            }
            return false;
        }

        private void chooseSelf(string uuid)
        {
            MenuManager.Instance.AddMenu(new SelfChosenMenu(uuid, rescueMode, action), true);
        }

        private void choose(int index)
        {
            MenuManager.Instance.AddMenu(new ContactChosenMenu(DiagManager.Instance.CurSettings.ContactList[index], DiagManager.Instance.CurSettings.ServerList[0], canTrade, hasSwappable, rescueMode, action, () => { DeleteAction(index); }), true);
        }

        private void startAddNew()
        {
            MenuManager.Instance.AddMenu(new ContactInputMenu(addNew), false);
        }

        private void addNew(string newID)
        {
            ContactInfo newInfo = new ContactInfo(newID);

            //bool alreadyExists = false;
            //foreach (ContactInfo info in DiagManager.Instance.CurSettings.ContactList)
            //{
            //    if (info.UUID == newID)
            //    {
            //        alreadyExists = true;
            //        break;
            //    }
            //}

            DiagManager.Instance.CurSettings.ContactList.Add(newInfo);
            DiagManager.Instance.SaveSettings(DiagManager.Instance.CurSettings);
            MenuManager.Instance.ReplaceMenu(new ContactsMenu(rescueMode, action));
        }

        private void DeleteAction(int totalChoice)
        {
            DiagManager.Instance.CurSettings.ContactList.RemoveAt(totalChoice);
            DiagManager.Instance.SaveSettings(DiagManager.Instance.CurSettings);

            MenuManager.Instance.RemoveMenu();

            MenuManager.Instance.ReplaceMenu(new ContactsMenu(rescueMode, action));
        }

        protected override void ChoiceChanged()
        {
            int totalChoice = CurrentChoice + CurrentPage * SLOTS_PER_PAGE;
            if (totalChoice == 0)
            {
                summaryMenu.Visible = true;
                ContactInfo info = DataManager.Instance.Save.CreateContactInfo();
                summaryMenu.SetContact(info.Data.TeamName,
                    Text.FormatKey("MENU_CONTACT_RANK", info.Data.GetLocalRankStr()),
                    Text.FormatKey("MENU_CONTACT_LAST_SEEN", info.LastContact),
                    Text.FormatKey("MENU_CONTACT_ID", info.UUID),
                    info.Data.TeamProfile);
            }
            else if (totalChoice < DiagManager.Instance.CurSettings.ContactList.Count+1)
            {
                summaryMenu.Visible = true;
                ContactInfo info = DiagManager.Instance.CurSettings.ContactList[totalChoice - 1];
                summaryMenu.SetContact((info.Data.TeamName == "") ? "[" + Text.FormatKey("MENU_NEW_CONTACT") + "]" : info.Data.TeamName,
                    Text.FormatKey("MENU_CONTACT_RANK", info.Data.GetLocalRankStr()),
                    Text.FormatKey("MENU_CONTACT_LAST_SEEN", info.LastContact),
                    Text.FormatKey("MENU_CONTACT_ID", info.UUID),
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
