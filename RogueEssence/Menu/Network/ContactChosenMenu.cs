using System.Collections.Generic;
using Microsoft.Xna.Framework;
using RogueElements;
using RogueEssence.Data;
using RogueEssence.Network;
using System;
using RogueEssence.Content;

namespace RogueEssence.Menu
{
    public class ContactChosenMenu : SingleStripMenu
    {

        private ContactInfo targetContact;
        private ServerInfo targetServer;
        private bool rescueMode;
        private ContactsMenu.OnChooseActivity action;
        private Action deleteAction;

        public ContactChosenMenu(ContactInfo info, ServerInfo serverInfo, bool canTrade, bool hasSwappable, bool rescueMode, ContactsMenu.OnChooseActivity action, Action deleteAction)
        {
            this.targetContact = info;
            this.targetServer = serverInfo;
            this.rescueMode = rescueMode;
            this.action = action;
            this.deleteAction = deleteAction;

            List<MenuTextChoice> choices = new List<MenuTextChoice>();

            if (rescueMode)
                choices.Add(new MenuTextChoice(Text.FormatKey("MENU_RESCUE"), () => { ActivityAction(ActivityType.GetHelp); }));
            else
            {
                choices.Add(new MenuTextChoice(Text.FormatKey("MENU_RESCUE"), () => { ActivityAction(ActivityType.SendHelp); }));
                choices.Add(new MenuTextChoice(Text.FormatKey("MENU_TRADE_TEAM"), () => { ActivityAction(ActivityType.TradeTeam); }, canTrade, canTrade ? Color.White : Color.Red));
                choices.Add(new MenuTextChoice(Text.FormatKey("MENU_TRADE_ITEM"), () => { ActivityAction(ActivityType.TradeItem); }, hasSwappable, hasSwappable ? Color.White : Color.Red));
                //choices.Add(new MenuTextChoice(Text.FormatKey("MENU_TRADE_MAIL"), () => { ActivityAction(ActivityType.TradeMail); }));
                //choices.Add(new MenuTextChoice(Text.FormatKey("MENU_INFO"), InfoAction));
            }
            choices.Add(new MenuTextChoice(Text.FormatKey("MENU_DELETE"), deleteAction));
            choices.Add(new MenuTextChoice(Text.FormatKey("MENU_EXIT"), ExitAction));

            int choice_width = CalculateChoiceLength(choices, 72);
            Initialize(new Loc(Math.Min(204, GraphicsManager.ScreenWidth - choice_width), 8), choice_width, choices.ToArray(), 0);
        }


        private void ActivityAction(ActivityType activityType)
        {
            MenuManager.Instance.RemoveMenu();
            MenuManager.Instance.RemoveMenu();

            OnlineActivity activity = null;
            switch (activityType)
            {
                case ActivityType.SendHelp:
                    activity = new ActivitySendHelp(targetServer, DataManager.Instance.Save.CreateContactInfo(), targetContact);
                    break;
                case ActivityType.GetHelp:
                    activity = new ActivityGetHelp(targetServer, DataManager.Instance.Save.CreateContactInfo(), targetContact);
                    break;
                case ActivityType.TradeTeam:
                    activity = new ActivityTradeTeam(targetServer, DataManager.Instance.Save.CreateContactInfo(), targetContact);
                    break;
                case ActivityType.TradeItem:
                    activity = new ActivityTradeItem(targetServer, DataManager.Instance.Save.CreateContactInfo(), targetContact);
                    break;
                case ActivityType.TradeMail:
                    activity = new ActivityTradeMail(targetServer, DataManager.Instance.Save.CreateContactInfo(), targetContact);
                    break;
            }
            action(activity);
        }

        private void InfoAction()
        {

        }


        private void ExitAction()
        {
            MenuManager.Instance.RemoveMenu();
        }
    }
}
