using RogueElements;
using System;
using Microsoft.Xna.Framework.Graphics;
using RogueEssence.Content;
using RogueEssence.Data;
using RogueEssence.Network;

namespace RogueEssence.Menu
{
    public class GetHelpMenu : RescueCardMenu
    {
        SummaryMenu yourSummary;
        MenuText yourStatus;


        public ExchangeRescueState CurrentState;

        public GetHelpMenu()
            : base()
        {
            yourSummary = new SummaryMenu(Rect.FromPoints(new Loc(Bounds.Start.X, Bounds.End.Y),
                new Loc(Bounds.End.X, Bounds.End.Y + LINE_HEIGHT + GraphicsManager.MenuBG.TileHeight * 2)));
            yourStatus = new MenuText("",
                new Loc(yourSummary.Bounds.Width / 2, GraphicsManager.MenuBG.TileHeight), DirH.None);
            yourStatus.Color = TextTan;
            yourSummary.Elements.Add(yourStatus);

            SetSOS(DataManager.Instance.Save.Rescue.SOS);

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
                    //first send the SOS
                    SOSMail mail = DataManager.Instance.Save.Rescue.SOS;

                    CurrentState = ExchangeRescueState.SOSReady;

                    ActivityGetHelp getHelp = NetworkManager.Instance.Activity as ActivityGetHelp;
                    getHelp.OfferMail(mail);
                    getHelp.SetReady(CurrentState);
                }
                else if (CurrentState == ExchangeRescueState.SOSReady)
                {
                    //wait for other party's ready to receive SOS or ready to send AOK
                    ActivityGetHelp getHelp = NetworkManager.Instance.Activity as ActivityGetHelp;

                    if (getHelp.CurrentState == ExchangeRescueState.SOSReady)
                    {
                        //ready to receive SOS
                        DialogueBox dialog = MenuManager.Instance.CreateQuestion(Text.FormatKey("DLG_RESCUE_SEND_SOS_ASK", getHelp.TargetInfo.Data.TeamName), () =>
                        {
                            CurrentState = ExchangeRescueState.SOSTrading;
                            getHelp.SetReady(CurrentState);
                        }, () =>
                        {
                            //just disconnect
                            MenuManager.Instance.RemoveMenu();
                            NetworkManager.Instance.Disconnect();
                        });
                        MenuManager.Instance.AddMenu(dialog, true);
                    }
                    else if (getHelp.CurrentState == ExchangeRescueState.AOKReady)
                    {
                        //ready to receive AOK
                        SetAOK(getHelp.OfferedMail);

                        CurrentState = ExchangeRescueState.AOKReady;
                        getHelp.SetReady(CurrentState);

                        string baseAskString = !String.IsNullOrEmpty(getHelp.OfferedMail.OfferedItem.Value) ? "DLG_RESCUE_GET_AOK_ASK_REWARD" : "DLG_RESCUE_GET_AOK_ASK";

                        DialogueBox dialog = MenuManager.Instance.CreateQuestion(Text.FormatKey(baseAskString, getHelp.TargetInfo.Data.TeamName), () =>
                        {
                            CurrentState = ExchangeRescueState.AOKTrading;
                            getHelp.SetReady(CurrentState);
                        }, () =>
                        {
                            //just disconnect
                            MenuManager.Instance.RemoveMenu();
                            NetworkManager.Instance.Disconnect();
                        });
                        MenuManager.Instance.AddMenu(dialog, true);
                    }
                    else if (getHelp.CurrentState == ExchangeRescueState.Completed)
                    {
                        //TODO: signal that an SOS has already been sent
                        //MenuManager.Instance.AddMenu(MenuManager.Instance.CreateDialogue(Text.FormatKey("DLG_RESCUE_SEND_SOS_ALREADY", getHelp.TargetInfo.Data.TeamName)), false);
                        CurrentState = ExchangeRescueState.Completed;
                        getHelp.SetReady(CurrentState);
                    }
                }
                else if (CurrentState == ExchangeRescueState.SOSTrading)
                {
                    //wait for the other party to also be SOSTrading or Completed
                    ActivityGetHelp getHelp = NetworkManager.Instance.Activity as ActivityGetHelp;

                    if (getHelp.CurrentState == ExchangeRescueState.SOSTrading || getHelp.CurrentState == ExchangeRescueState.Completed)
                    {
                        MenuManager.Instance.AddMenu(MenuManager.Instance.CreateDialogue(Text.FormatKey("DLG_RESCUE_SEND_SOS", getHelp.TargetInfo.Data.TeamName)), false);
                        CurrentState = ExchangeRescueState.Completed;
                        getHelp.SetReady(CurrentState);
                    }
                    //the other possible outcome is that they just disconnect
                }
                else if (CurrentState == ExchangeRescueState.AOKTrading)
                {
                    //wait for the other party to also be AOKTrading or Completed
                    ActivityGetHelp getHelp = NetworkManager.Instance.Activity as ActivityGetHelp;

                    if (getHelp.CurrentState == ExchangeRescueState.AOKTrading || getHelp.CurrentState == ExchangeRescueState.Completed)
                    {
                        //save the AOK file
                        DataManager.SaveRescueMail(PathMod.FromApp(DataManager.RESCUE_IN_PATH + DataManager.AOK_FOLDER), getHelp.OfferedMail, false);
                        if (!String.IsNullOrEmpty(getHelp.OfferedMail.OfferedItem.Value))
                        {
                            //deduct your reward and save it to the base file
                            GameState state = DataManager.Instance.LoadMainGameState(false);
                            state.Save.Rescue.SOS.OfferedItem = getHelp.OfferedMail.OfferedItem;
                            DataManager.Instance.SaveGameState(state);

                            MenuManager.Instance.AddMenu(MenuManager.Instance.CreateDialogue(Text.FormatKey("DLG_RESCUE_GOT_AOK", getHelp.TargetInfo.Data.TeamName), Text.FormatKey("DLG_RESCUE_GOT_AOK_REWARD")), false);
                        }
                        else
                            MenuManager.Instance.AddMenu(MenuManager.Instance.CreateDialogue(Text.FormatKey("DLG_RESCUE_GOT_AOK", getHelp.TargetInfo.Data.TeamName)), false);

                        CurrentState = ExchangeRescueState.Completed;
                        getHelp.SetReady(CurrentState);
                    }
                    //the other possible outcome is that they just disconnect
                }
                else if (CurrentState == ExchangeRescueState.Completed)
                {
                    ActivityGetHelp getHelp = NetworkManager.Instance.Activity as ActivityGetHelp;
                    //wait for the other party to also be Completed to leave the transaction
                    if (getHelp.CurrentState == ExchangeRescueState.Completed)
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
                    yourStatus.SetText(Text.FormatKey("MENU_RESCUE_STATUS_SOS_SENDING"));
                    break;
                case ExchangeRescueState.AOKReady:
                    yourStatus.SetText(Text.FormatKey("MENU_RESCUE_STATUS_AOK_CONFIRMING"));
                    break;
                case ExchangeRescueState.AOKTrading:
                    yourStatus.SetText(Text.FormatKey("MENU_RESCUE_STATUS_AOK_RECEIVING"));
                    break;
                case ExchangeRescueState.Completed:
                    yourStatus.SetText(Text.FormatKey("MENU_RESCUE_STATUS_WAITING"));
                    break;
            }
        }
    }
}
