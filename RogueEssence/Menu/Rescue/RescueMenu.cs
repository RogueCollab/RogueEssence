using RogueElements;
using System.Collections.Generic;
using RogueEssence.Data;
using RogueEssence.Dungeon;
using RogueEssence.Network;
using System.IO;
using RogueEssence.Script;
using System;

namespace RogueEssence.Menu
{
    public class RescueMenu : RescueCardMenu
    {
        public override bool IsCheckpoint { get { return true; } }

        private AOKMail testingMail;
        private string testingPath;

        public bool Verified;

        public RescueMenu()
            : base()
        {
            SetSOS(DataManager.Instance.Save.Rescue.SOS);
        }

        public override void Update(InputManager input)
        {
            Visible = true;

            if (testingPath != null)
            {
                if (Verified)
                    completeAOK();
                else
                {
                    if (File.Exists(testingPath))
                        File.Delete(testingPath);
                    MenuManager.Instance.AddMenu(MenuManager.Instance.CreateDialogue(MenuManager.Instance.RemoveMenu, Text.FormatKey("DLG_AWAIT_RESCUE_AOK_FAIL")), true);
                }
            }
            else
                awaitRescue();
        }


        private void awaitRescue()
        {
            //check for an AOK file from file rescue
            string aokPath = DataManager.FindRescueMail(PathMod.FromApp(DataManager.RESCUE_IN_PATH + DataManager.AOK_FOLDER), DataManager.Instance.Save.Rescue.SOS, DataManager.AOK_EXTENSION);
            if (aokPath != null)
            {
                AOKMail aok = (AOKMail)DataManager.LoadRescueMail(aokPath);
                if (aok != null)
                {
                    SetAOK(aok);
                    //an AOK mail has been found!
                    MenuManager.Instance.AddMenu(MenuManager.Instance.CreateDialogue(() => loadAOK(aokPath, aok), Text.FormatKey("DLG_AWAIT_RESCUE_AOK_FOUND")), true);
                }
            }
            else
            {
                //if there's no AOK, generate the SOS mail from the save data
                if (!File.Exists(PathMod.FromApp(DataManager.RESCUE_OUT_PATH + DataManager.SOS_FOLDER + DataManager.Instance.Save.UUID + DataManager.SOS_EXTENSION)))
                    DataManager.SaveRescueMail(PathMod.FromApp(DataManager.RESCUE_OUT_PATH + DataManager.SOS_FOLDER), DataManager.Instance.Save.Rescue.SOS, true);


                List<DialogueChoice> choices = new List<DialogueChoice>();
                choices.Add(new DialogueChoice(Text.FormatKey("MENU_RESCUE_GET_HELP"), contactForHelp));
                choices.Add(new DialogueChoice(Text.FormatKey("MENU_RESCUE_CONFIGURE_SERVER"), configureServer));
                choices.Add(new DialogueChoice(Text.FormatKey("MENU_RESCUE_OFFER_REWARD"), offerReward));
                choices.Add(new DialogueChoice(Text.FormatKey("MENU_REST_QUIT"), giveUp));
                choices.Add(new DialogueChoice(Text.FormatKey("MENU_CANCEL"), () => { MenuManager.Instance.RemoveMenu(); }));
                MenuManager.Instance.AddMenu(MenuManager.Instance.CreateMultiQuestion(Text.FormatKey("DLG_AWAIT_RESCUE_ASK"), false,
                    choices, 0, choices.Count - 1), true);
            }
        }

        private void contactForHelp()
        {
            MenuManager.Instance.AddMenu(new ContactsMenu(true, (OnlineActivity activity) =>
            {
                NetworkManager.Instance.PrepareActivity(activity);
                MenuManager.Instance.AddMenu(new ConnectingMenu(getHelp), false);
            }), false);
        }

        private void getHelp()
        {
            MenuManager.Instance.AddMenu(new GetHelpMenu(), false);
        }

        private void configureServer()
        {
            MenuManager.Instance.AddMenu(new ServersMenu(), false);
        }

        private void offerReward()
        {
            bool canOfferMoney = DataManager.Instance.Save.ActiveTeam.Bank > 0;
            bool canOfferItem = DataManager.Instance.Save.ActiveTeam.BoxStorage.Count > 0;
            if (!canOfferItem)
            {
                foreach(string key in DataManager.Instance.Save.ActiveTeam.Storage.Keys)
                {
                    if (DataManager.Instance.Save.ActiveTeam.Storage.GetValueOrDefault(key, 0) > 0)
                    {
                        canOfferItem = true;
                        break;
                    }
                }
            }
            bool canRemoveItem = !String.IsNullOrEmpty(DataManager.Instance.Save.Rescue.SOS.OfferedItem.Value);

            List <DialogueChoice> choices = new List<DialogueChoice>();
            choices.Add(new DialogueChoice(Text.FormatKey("MENU_REWARD_MONEY"), () => { MenuManager.Instance.AddMenu(new BankMenu(0, setRewardMoney), false); }, canOfferMoney));
            choices.Add(new DialogueChoice(Text.FormatKey("MENU_REWARD_ITEM"), () => { MenuManager.Instance.AddMenu(new WithdrawMenu(0, false, setRewardItem), false); }, canOfferItem));
            choices.Add(new DialogueChoice(Text.FormatKey("MENU_RESCUE_REMOVE_REWARD"), removeReward, canRemoveItem));
            choices.Add(new DialogueChoice(Text.FormatKey("MENU_CANCEL"), () => { }));
            MenuManager.Instance.AddMenu(MenuManager.Instance.CreateMultiQuestion(Text.FormatKey("DLG_RESCUE_ADD_REWARD_ASK"), false,
                choices, 0, choices.Count - 1), true);
        }

        private void setRewardMoney(int amount)
        {
            if (amount == 0)
                DataManager.Instance.Save.Rescue.SOS.OfferedItem = new MapItem();
            else
                DataManager.Instance.Save.Rescue.SOS.OfferedItem = MapItem.CreateMoney(amount);

            GameState state = DataManager.Instance.LoadMainGameState(false);
            state.Save.Rescue = DataManager.Instance.Save.Rescue;
            DataManager.Instance.SaveGameState(state);
            SetSOS(DataManager.Instance.Save.Rescue.SOS);
        }

        private void setRewardItem(List<WithdrawSlot> slots)
        {
            List<InvItem> items = DataManager.Instance.Save.ActiveTeam.TakeItems(slots, false);
            if (items.Count > 1)
                DataManager.Instance.Save.Rescue.SOS.OfferedItem = new MapItem(items[0].ID, items.Count);
            else
                DataManager.Instance.Save.Rescue.SOS.OfferedItem = new MapItem(items[0]);

            GameState state = DataManager.Instance.LoadMainGameState(false);
            state.Save.Rescue = DataManager.Instance.Save.Rescue;
            DataManager.Instance.SaveGameState(state);
            SetSOS(DataManager.Instance.Save.Rescue.SOS);
        }

        private void removeReward()
        {
            DataManager.Instance.Save.Rescue.SOS.OfferedItem = new MapItem();

            GameState state = DataManager.Instance.LoadMainGameState(false);
            state.Save.Rescue = DataManager.Instance.Save.Rescue;
            DataManager.Instance.SaveGameState(state);
            SetSOS(DataManager.Instance.Save.Rescue.SOS);
        }

        private void giveUp()
        {
            MenuManager.Instance.AddMenu(MenuManager.Instance.CreateQuestion(Text.FormatKey("DLG_RESCUE_QUIT_ASK"), true,
                        () => { TopMenu.Continue(null); },
                        () => { }, true), false);
        }

        private void loadAOK(string aokPath, AOKMail aok)
        {
            //cut to black and verify the mail behind the scenes
            //If the replay succeeds, set the rescue state to true and take them back to the title.

            //check the AOK mail
            ReRandom rerand = new ReRandom(aok.Seed);
            for (int ii = 0; ii < aok.RescueSeed; ii++)
                rerand.NextUInt64();
            ulong rolledSeed = rerand.NextUInt64();

            if (aok.RescueReplay.States[0].Save.Rand.FirstSeed == rolledSeed)
            {
                //when starting, we must persist the AOK data so we know what data to put into the SOS
                //and the path so we know which path to delete if we fail to verify.
                //let's store it in this menu
                //when the replay ends, it will look for this menu and inject the result: pass, or fail
                testingPath = aokPath;
                testingMail = aok;

                TitleScene.TitleMenuSaveState = MenuManager.Instance.SaveMenuState();
                MenuManager.Instance.ClearMenus();
                List<ModDiff> replayDiffs = aok.RescueReplay.States[0].Save.GetModDiffs();
                if (replayDiffs.Count > 0)
                    MenuManager.Instance.AddMenu(MenuManager.Instance.CreateDialogue(Text.FormatKey("DLG_FILE_VERSION_DIFF")), false);
                GameManager.Instance.SceneOutcome = validateReplay(aok.RescueReplay);
            }
            else
            {
                File.Delete(aokPath);
                MenuManager.Instance.AddMenu(MenuManager.Instance.CreateDialogue(MenuManager.Instance.RemoveMenu, Text.FormatKey("DLG_AWAIT_RESCUE_AOK_FAIL")), true);
            }
        }


        private IEnumerator<YieldInstruction> validateReplay(ReplayData replay)
        {
            GameManager.Instance.BGM("", true);
            yield return CoroutineManager.Instance.StartCoroutine(GameManager.Instance.FadeOut(false));

            DataManager.Instance.MsgLog.Clear();
            //load that up instead if found
            GameState state = replay.ReadState();
            DataManager.Instance.SetProgress(state.Save);
            LuaEngine.Instance.LoadSavedData(DataManager.Instance.Save); //notify script engine
            ZoneManager.LoadFromState(state.Zone);
            LuaEngine.Instance.UpdateZoneInstance();

            //NOTE: In order to preserve debug consistency, you SHOULD set the language to that of the quicksave.
            //HOWEVER, it would be too inconvenient for players sharing their quicksaves, thus this feature is LEFT OUT.

            DataManager.Instance.Loading = DataManager.LoadMode.Rescuing;
            DataManager.Instance.CurrentReplay = replay;

            yield return CoroutineManager.Instance.StartCoroutine(ZoneManager.Instance.CurrentZone.OnInit());
            yield return CoroutineManager.Instance.StartCoroutine(GameManager.Instance.MoveToZone(DataManager.Instance.Save.NextDest, true, false));
        }

        private void completeAOK()
        {
            //if the mail worked, mark the save file as complete
            //and then return to the top menu

            //notify if it is found and working
            //notify if it failed to work
            //delete the mail either way
            GameState state = DataManager.Instance.LoadMainGameState(false);
            state.Save.Rescue.SOS.RescuedBy = testingMail.RescuingTeam;
            state.Save.Rescue.SOS.RescuingNames = testingMail.RescuingNames;
            state.Save.Rescue.SOS.RescuingTeam = testingMail.RescuingProfile;
            state.Save.Rescue.SOS.RescuingPersonalities = testingMail.RescuingPersonalities;
            state.Save.Rescue.SOS.FinalStatement = testingMail.FinalStatement;
            DataManager.Instance.SaveGameState(state);

            //delete the SOS and AOK files if applicable
            if (File.Exists(testingPath))
                File.Delete(testingPath);

            if (File.Exists(PathMod.FromApp(DataManager.RESCUE_OUT_PATH + DataManager.SOS_FOLDER + state.Save.UUID + DataManager.SOS_EXTENSION)))
                File.Delete(PathMod.FromApp(DataManager.RESCUE_OUT_PATH + DataManager.SOS_FOLDER + state.Save.UUID + DataManager.SOS_EXTENSION));


            MenuManager.Instance.AddMenu(MenuManager.Instance.CreateDialogue(MenuManager.Instance.ClearMenus, Text.FormatKey("DLG_AWAIT_RESCUE_RESUME")), true);

        }
    }
}
