using RogueElements;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using RogueEssence.Content;
using RogueEssence.Data;
using RogueEssence.Dungeon;
using RogueEssence.Network;
using System.IO;
using System;

namespace RogueEssence.Menu
{
    public class SendHelpMenu : RescueCardMenu
    {
        SummaryMenu yourSummary;
        MenuText yourStatus;

        string aokPath;
        AOKMail aok;

        public ExchangeRescueState CurrentState;

        public SendHelpMenu()
            : base()
        {
            yourSummary = new SummaryMenu(Rect.FromPoints(new Loc(Bounds.Start.X, Bounds.End.Y),
                new Loc(Bounds.End.X, Bounds.End.Y + LINE_HEIGHT + GraphicsManager.MenuBG.TileHeight * 2)));
            yourStatus = new MenuText("",
                new Loc(yourSummary.Bounds.Width / 2, GraphicsManager.MenuBG.TileHeight), DirH.None);
            yourStatus.Color = TextTan;
            yourSummary.Elements.Add(yourStatus);

            CurrentState = ExchangeRescueState.Communicating;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!Visible)
                return;
            base.Draw(spriteBatch);

            //draw other windows
            yourSummary.Draw(spriteBatch);

        }

        public override void Update(InputManager input)
        {
            Visible = true;

            NetworkManager.Instance.Update();
            if (NetworkManager.Instance.Status == OnlineStatus.Offline)
            {
                //give offline message in a dialogue
                MenuManager.Instance.RemoveMenu();
                if (CurrentState != ExchangeRescueState.Completed)
                    MenuManager.Instance.AddMenu(MenuManager.Instance.CreateDialogue(NetworkManager.Instance.ExitMsg), false);
            }
            else
            {
                if (CurrentState == ExchangeRescueState.Communicating)
                {
                    //wait for an SOS
                    ActivitySendHelp sendHelp = NetworkManager.Instance.Activity as ActivitySendHelp;

                    if (sendHelp.CurrentState == ExchangeRescueState.SOSReady)
                    {
                        SOSMail sos = sendHelp.OfferedMail;

                        aokPath = DataManager.FindRescueMail(PathMod.FromApp(DataManager.RESCUE_OUT_PATH + DataManager.AOK_FOLDER), sos, DataManager.AOK_EXTENSION);

                        if (aokPath == null)
                        {
                            //no aok found; ask to receive SOS
                            //TODO: check to see if SOS has already been sent
                            //it needs to check if the reward is the same.
                            SetSOS(sendHelp.OfferedMail);

                            CurrentState = ExchangeRescueState.SOSReady;
                            sendHelp.SetReady(CurrentState);

                            DialogueBox dialog = MenuManager.Instance.CreateQuestion(Text.FormatKey("DLG_RESCUE_RECEIVE_SOS_ASK", sendHelp.TargetInfo.Data.TeamName), () =>
                            {
                                CurrentState = ExchangeRescueState.SOSTrading;
                                sendHelp.SetReady(CurrentState);
                            }, () =>
                            {
                                //just disconnect
                                MenuManager.Instance.RemoveMenu();
                                NetworkManager.Instance.Disconnect();
                            });
                            MenuManager.Instance.AddMenu(dialog, true);
                        }
                        else
                        {
                            //aok found; ask to send AOK
                            aok = (AOKMail)DataManager.LoadRescueMail(aokPath);
                            SetAOK(aok);

                            CurrentState = ExchangeRescueState.AOKReady;

                            sendHelp.OfferMail(aok);
                            sendHelp.SetReady(CurrentState);
                        }

                    }
                }
                else if (CurrentState == ExchangeRescueState.SOSTrading)
                {
                    //wait for the other party to also be SOSTrading or Completed
                    ActivitySendHelp sendHelp = NetworkManager.Instance.Activity as ActivitySendHelp;

                    if (sendHelp.CurrentState == ExchangeRescueState.SOSTrading || sendHelp.CurrentState == ExchangeRescueState.Completed)
                    {
                        //save the SOS mail
                        DataManager.SaveRescueMail(PathMod.FromApp(DataManager.RESCUE_IN_PATH + DataManager.SOS_FOLDER), sendHelp.OfferedMail, false);

                        MenuManager.Instance.AddMenu(MenuManager.Instance.CreateDialogue(Text.FormatKey("DLG_RESCUE_RECEIVE_SOS", sendHelp.TargetInfo.Data.TeamName)), false);
                        CurrentState = ExchangeRescueState.Completed;
                        sendHelp.SetReady(CurrentState);
                    }
                    //the other possible outcome is that they just disconnect
                }
                else if (CurrentState == ExchangeRescueState.AOKReady)
                {
                    //wait for other party's ready to receive SOS or ready to send AOK
                    ActivitySendHelp sendHelp = NetworkManager.Instance.Activity as ActivitySendHelp;

                    if (sendHelp.CurrentState == ExchangeRescueState.AOKReady)
                    {
                        //ready to receive SOS
                        string baseAskString = !String.IsNullOrEmpty(aok.OfferedItem.Value) ? "DLG_RESCUE_SEND_AOK_ASK_REWARD" : "DLG_RESCUE_SEND_AOK_ASK";
                        DialogueBox dialog = MenuManager.Instance.CreateQuestion(Text.FormatKey(baseAskString, sendHelp.TargetInfo.Data.TeamName), () =>
                        {
                            CurrentState = ExchangeRescueState.AOKTrading;
                            sendHelp.SetReady(CurrentState);
                        }, () =>
                        {
                            //just disconnect
                            MenuManager.Instance.RemoveMenu();
                            NetworkManager.Instance.Disconnect();
                        });
                        MenuManager.Instance.AddMenu(dialog, true);
                    }
                    //the other possible outcome is that they just disconnect
                }
                else if (CurrentState == ExchangeRescueState.AOKTrading)
                {
                    //wait for the other party to also be AOKTrading or Completed
                    ActivitySendHelp sendHelp = NetworkManager.Instance.Activity as ActivitySendHelp;

                    if (sendHelp.CurrentState == ExchangeRescueState.AOKTrading || sendHelp.CurrentState == ExchangeRescueState.Completed)
                    {
                        //delete the AOK file
                        File.Delete(aokPath);

                        if (!String.IsNullOrEmpty(aok.OfferedItem.Value))
                        {
                            if (aok.OfferedItem.IsMoney)
                                DataManager.Instance.Save.ActiveTeam.Bank += aok.OfferedItem.Amount;
                            else
                            {
                                List<InvItem> itemsToStore = new List<InvItem>();
                                itemsToStore.Add(aok.OfferedItem.MakeInvItem());
                                DataManager.Instance.Save.ActiveTeam.StoreItems(itemsToStore);
                            }
                            DataManager.Instance.SaveMainGameState();

                            MenuManager.Instance.AddMenu(MenuManager.Instance.CreateDialogue(Text.FormatKey("DLG_RESCUE_SEND_AOK", sendHelp.TargetInfo.Data.TeamName), Text.FormatKey("DLG_RESCUE_SEND_AOK_REWARD")), false);
                        }
                        else
                            MenuManager.Instance.AddMenu(MenuManager.Instance.CreateDialogue(Text.FormatKey("DLG_RESCUE_SEND_AOK", sendHelp.TargetInfo.Data.TeamName)), false);
                        CurrentState = ExchangeRescueState.Completed;
                        sendHelp.SetReady(CurrentState);
                    }
                    //the other possible outcome is that they just disconnect
                }
                else if (CurrentState == ExchangeRescueState.Completed)
                {
                    ActivitySendHelp sendHelp = NetworkManager.Instance.Activity as ActivitySendHelp;
                    //wait for the other party to also be Completed to leave the transaction
                    if (sendHelp.CurrentState == ExchangeRescueState.Completed)
                    {
                        MenuManager.Instance.RemoveMenu();
                        NetworkManager.Instance.Disconnect();
                    }
                }

                UpdateStatus();
            }
        }


        public void UpdateStatus()
        {
            ActivityGetHelp getHelp = NetworkManager.Instance.Activity as ActivityGetHelp;

            switch (CurrentState)
            {
                case ExchangeRescueState.Communicating:
                    yourStatus.SetText(Text.FormatKey("MENU_RESCUE_STATUS_COMMUNICATING"));
                    break;
                case ExchangeRescueState.SOSReady:
                    yourStatus.SetText(Text.FormatKey("MENU_RESCUE_STATUS_SOS_CONFIRMING"));
                    break;
                case ExchangeRescueState.SOSTrading:
                    yourStatus.SetText(Text.FormatKey("MENU_RESCUE_STATUS_SOS_RECEIVING"));
                    break;
                case ExchangeRescueState.AOKReady:
                    yourStatus.SetText(Text.FormatKey("MENU_RESCUE_STATUS_AOK_CONFIRMING"));
                    break;
                case ExchangeRescueState.AOKTrading:
                    yourStatus.SetText(Text.FormatKey("MENU_RESCUE_STATUS_AOK_SENDING"));
                    break;
                case ExchangeRescueState.Completed:
                    yourStatus.SetText(Text.FormatKey("MENU_RESCUE_STATUS_WAITING"));
                    break;
            }
        }
    }
}
