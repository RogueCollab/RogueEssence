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

namespace RogueEssence.Script
{
    class ScriptUI : ILuaEngineComponent
    {
        /// <summary>
        /// Used to store a list of choice and their return value for most multi-choice menus.
        /// </summary>
        struct ChoicePair
        {
            string Text { get; set; }
            int Value { get; set; }
        }

        //Variables for storing multi-step operations, like setting the speaker in a dialog
        private object                 m_choiceresult = -1;
        private MonsterID       m_curspeakerID = new MonsterID();
        private string              m_curspeakerName= "";
        private EmoteStyle  m_curspeakerEmo = new EmoteStyle(0);
        private bool                m_curspeakerSnd = true;
        private IEnumerator<YieldInstruction> m_curdialogue;

        private IInteractable m_curchoice;

        public ScriptUI()
        {
            ResetSpeaker();
        }

        public void Reset()
        {
            ResetSpeaker();
        }


        //Lua wrapper functions
        public LuaFunction WaitShowMonologue;
        public LuaFunction WaitForChoice;
        public LuaFunction WaitDialog;
        public LuaFunction WaitShowDialogue;
        public LuaFunction WaitShowVoiceOver;
        public LuaFunction WaitShowTitle;
        public LuaFunction WaitHideTitle;

        //================================================================
        // Dialogue
        //================================================================

        /// <summary>
        ///
        /// </summary>
        /// <param name="text"></param>
        public void TextMonologue(string text)
        {
            try
            {
                m_curdialogue = MenuManager.Instance.SetSign(text);
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogInfo(String.Format("ScriptUI.TextMonologue({0}): Encountered exception:\n{1}", text, e.Message));
            }
        }




        public void TextDialogue(string text)
        {
            try
            {
                m_curdialogue = MenuManager.Instance.SetDialogue(m_curspeakerID, m_curspeakerName, m_curspeakerEmo, m_curspeakerSnd, new string[] { text }); //!#NOTE : I really don't know why we should pass tables of strings?
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogInfo(String.Format("ScriptUI.TextDialogue({0}): Encountered exception:\n{1}", text, e.Message));
            }
        }


        public void TextVoiceOver(string text, int expireTime)
        {
            try
            {
                m_curdialogue = MenuManager.Instance.ProcessMenuCoroutine(new TitleDialog(text, true, expireTime, () => { }));
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogInfo(String.Format("ScriptUI.TextVoiceOver({0}, {1}): Encountered exception:\n{2}", text, expireTime, e.Message));
            }
        }


        public void TextShowTitle(string text, int time)
        {
            try
            {
                m_curdialogue = GameManager.Instance.FadeTitle(true, text, time);
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogInfo(String.Format("ScriptUI.TextShowTitle({0}, {1}): Encountered exception:\n{2}", text, time, e.Message));
            }
        }


        public void TextFadeTitle(int time)
        {
            try
            {
                m_curdialogue = GameManager.Instance.FadeTitle(false, "", time);
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogInfo(String.Format("ScriptUI.TextDialogue({0}): Encountered exception:\n{1}", time, e.Message));
            }
        }

        /// <summary>
        /// Clears the current speaker, so none is displayed the next time TextDialogue is called!
        /// </summary>
        public void ResetSpeaker(bool keysound = true)
        {
            m_curspeakerID = MonsterID.Invalid;
            m_curspeakerName = null;
            m_curspeakerEmo = new EmoteStyle(0);
            m_curspeakerSnd = keysound;
        }


        /// <summary>
        /// Sets the speaker to be displayed during the following calls to the TextDialogue functions.  It resets speaker emotion.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="keysound"></param>
        /// <param name="specie"></param>
        /// <param name="form"></param>
        /// <param name="skin"></param>
        /// <param name="gender"></param>
        public void SetSpeaker(string name, bool keysound, int specie, int form, int skin, Gender gender = Gender.Male)
        {
            m_curspeakerID = new MonsterID(specie, form, skin, gender) ;
            m_curspeakerName = name;
            m_curspeakerEmo.Emote = 0;
            m_curspeakerSnd = keysound;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="chara"></param>
        /// <param name="keysound"></param>
        public void SetSpeaker(Ground.GroundChar chara, bool keysound = true)
        {
            if (chara != null)
            {
                m_curspeakerID = chara.CurrentForm;
                m_curspeakerName = chara.GetDisplayName();
            }
            else
            {
                DiagManager.Instance.LogInfo("ScriptUI.SetSpeaker(): The speaker was null!!");
                m_curspeakerID = new MonsterID(0, 0, 0, Data.Gender.Unknown);
                m_curspeakerName = "NULL";
            }

            m_curspeakerEmo.Emote = 0;
            m_curspeakerSnd = keysound;
        }
        public void SetSpeaker(Character chara, bool keysound = true)
        {
            if (chara != null)
            {
                m_curspeakerID = chara.CurrentForm;
                m_curspeakerName = chara.Name;
            }
            else
            {
                DiagManager.Instance.LogInfo("ScriptUI.SetSpeaker(): The speaker was null!!");
                m_curspeakerID = new MonsterID(0, 0, 0, Data.Gender.Unknown);
                m_curspeakerName = "NULL";
            }

            m_curspeakerEmo.Emote = 0;
            m_curspeakerSnd = keysound;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="emo"></param>
        public void SetSpeakerEmotion(string emo)
        {
            int emoteIndex = GraphicsManager.Emotions.FindIndex((EmotionType element) => element.Name == emo);
            m_curspeakerEmo.Emote = emoteIndex;
        }



        /// <summary>
        /// Waits for the dialog window to be closed to return control to the script!
        /// </summary>
        public Coroutine _WaitDialog()
        {
            return new Coroutine(m_curdialogue);
        }

        /// <summary>
        /// Instantly break. Used as default/invalid value when returning a yieldable value.
        /// </summary>
        /// <returns></returns>
        private IEnumerator<YieldInstruction> _DummyWait()
        {
            yield break;
        }

        //================================================================
        // Menus
        //================================================================

        /// <summary>
        /// Ask a question answered by yes or no via character dialogue to the player.
        /// WaitForChoice() must be called afterwards for the menu to be actually displayed,
        /// and for execution to suspend until the choice is returned.
        /// Then to recover the integer value indicating the result of the menu, ChoiceResult() must be called.
        ///
        /// The Yes/No menu returns 1 for yes, and 0 for no.
        /// </summary>
        /// <param name="message">Question to be asked to the user.</param>
        /// <param name="bdefaultstono">Whether the cursor starts on no by default</param>
        public void ChoiceMenuYesNo( string message, bool bdefaultstono = false )
        {
            try
            {
                m_choiceresult = null;

                if (message == null)
                    message = "";

                if (m_curspeakerName != null)
                {
                    m_curchoice = MenuManager.Instance.CreateQuestion(m_curspeakerID,
                                                                      m_curspeakerName,
                                                                      m_curspeakerEmo,
                                                                      message,
                                                                      m_curspeakerSnd,
                                                                      () => { m_choiceresult = true; },
                                                                      () => { m_choiceresult = false; },
                                                                      bdefaultstono);
                }
                else
                {
                    m_curchoice = MenuManager.Instance.CreateQuestion(message,
                        m_curspeakerSnd,
                        () => { m_choiceresult = true; },
                        () => { m_choiceresult = false; });
                }
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogInfo(String.Format("ScriptUI.ChoiceMenuYesNo({0}): Encountered exception:\n{1}", message, e.Message));
            }
        }

        /// <summary>
        /// Displays the name input box.
        /// 
        /// WaitForChoice() must be called afterwards for the menu to be actually displayed,
        /// and for execution to suspend until the choice is returned.
        /// Then to recover the string value indicating the result of the menu, StringResult() must be called.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="desc"></param>
        public void NameMenu(string title, string desc)
        {
            try
            {
                m_choiceresult = "";
                m_curchoice = new TeamNameMenu(title, desc, (string name) => { m_choiceresult = name; });
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogInfo(String.Format("ScriptUI.NameMenu(): Encountered exception:\n{0}", e.Message));
            }
        }

        public void AssemblyMenu()
        {
            try
            {
                m_choiceresult = false;
                m_curchoice = new AssemblyMenu(0, () => { m_choiceresult = true; });
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogInfo(String.Format("ScriptUI.AssemblyMenu(): Encountered exception:\n{0}", e.Message));
            }
        }

        /// <summary>
        /// Displays the Shop menu.
        /// 
        /// WaitForChoice() must be called afterwards for the menu to be actually displayed,
        /// and for execution to suspend until the choice is returned.
        /// Then to recover the string value indicating the result of the menu, StringResult() must be called.
        /// </summary>
        /// <param name="goods"></param>
        public void ShopMenu(LuaTable goods)
        {
            try
            {
                m_choiceresult = LuaEngine.Instance.RunString("return {}").First() as LuaTable;
                List<Tuple<InvItem, int>> goodsList = new List<Tuple<InvItem, int>>();
                foreach (object key in goods.Keys)
                {
                    LuaTable entry = goods[key] as LuaTable;
                    InvItem item = entry["Item"] as InvItem;
                    Int64 price = (Int64)entry["Price"];
                    goodsList.Add(new Tuple<InvItem, int>(item, (int)price));
                }
                m_curchoice = new ShopMenu(goodsList, 0, (List<int> chosenGoods) =>
                {
                    LuaFunction addfn = LuaEngine.Instance.RunString("return function(tbl, val) table.insert(tbl, val) end").First() as LuaFunction;
                    foreach (int chosenGood in chosenGoods)
                        addfn.Call(m_choiceresult, chosenGood+1);
                });
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogInfo(String.Format("ScriptUI.ShopMenu(): Encountered exception:\n{0}", e.Message));
            }
        }

        /// <summary>
        /// Displays the Sell menu.
        /// 
        /// WaitForChoice() must be called afterwards for the menu to be actually displayed,
        /// and for execution to suspend until the choice is returned.
        /// Then to recover the string value indicating the result of the menu, StringResult() must be called.
        /// </summary>
        /// <param name="goods"></param>
        public void SellMenu()
        {
            try
            {
                m_choiceresult = LuaEngine.Instance.RunString("return {}").First() as LuaTable;
                m_curchoice = new SellMenu(0, (List<InvSlot> chosenGoods) =>
                {
                    LuaFunction addfn = LuaEngine.Instance.RunString("return function(tbl, val) table.insert(tbl, val) end").First() as LuaFunction;
                    foreach (InvSlot chosenGood in chosenGoods)
                        addfn.Call(m_choiceresult, chosenGood);
                });
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogInfo(String.Format("ScriptUI.SellMenu(): Encountered exception:\n{0}", e.Message));
            }
        }

        public void StorageMenu()
        {
            try
            {
                m_choiceresult = null;
                m_curchoice = new DepositMenu(0);
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogInfo(String.Format("ScriptUI.StorageMenu(): Encountered exception:\n{0}", e.Message));
            }
        }

        public void WithdrawMenu()
        {
            try
            {
                m_choiceresult = null;
                m_curchoice = new WithdrawMenu(0, true, onChooseSlot);
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogInfo(String.Format("ScriptUI.WithdrawMenu(): Encountered exception:\n{0}", e.Message));
            }
        }

        private void onChooseSlot(List<int> slots)
        {
            //store item
            List<InvItem> items = DataManager.Instance.Save.ActiveTeam.TakeItems(slots);

            foreach (InvItem item in items)
            {
                ItemData entry = DataManager.Instance.GetItem(item.ID);
                int existingStack = -1;
                if (entry.MaxStack > 1)
                {
                    for (int jj = 0; jj < DataManager.Instance.Save.ActiveTeam.GetInvCount(); jj++)
                    {
                        if (DataManager.Instance.Save.ActiveTeam.GetInv(jj).ID == item.ID && DataManager.Instance.Save.ActiveTeam.GetInv(jj).HiddenValue < entry.MaxStack)
                        {
                            existingStack = jj;
                            break;
                        }
                    }
                }
                if (existingStack > -1)
                {
                    DataManager.Instance.Save.ActiveTeam.GetInv(existingStack).HiddenValue += item.HiddenValue;
                    DataManager.Instance.Save.ActiveTeam.UpdateInv(DataManager.Instance.Save.ActiveTeam.GetInv(existingStack), DataManager.Instance.Save.ActiveTeam.GetInv(existingStack));
                }
                else
                    DataManager.Instance.Save.ActiveTeam.AddToInv(item);
            }
        }


        public void BankMenu()
        {
            try
            {
                m_choiceresult = null;
                m_curchoice = new BankMenu(DataManager.Instance.Save.ActiveTeam.Money, onChooseAmount);
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogInfo(String.Format("ScriptUI.BankMenu(): Encountered exception:\n{0}", e.Message));
            }
        }

        private void onChooseAmount(int amount)
        {
            long total = (long)DataManager.Instance.Save.ActiveTeam.Bank + DataManager.Instance.Save.ActiveTeam.Money;
            DataManager.Instance.Save.ActiveTeam.Bank = (int)(total - amount);
            DataManager.Instance.Save.ActiveTeam.Money = amount;
        }

        public void SpoilsMenu(LuaTable appraisalMap)
        {
            try
            {
                List<Tuple<InvItem, InvItem>> goodsList = new List<Tuple<InvItem, InvItem>>();
                foreach (object key in appraisalMap.Keys)
                {
                    LuaTable entry = appraisalMap[key] as LuaTable;
                    InvItem box = entry["Box"] as InvItem;
                    InvItem item = entry["Item"] as InvItem;
                    goodsList.Add(new Tuple<InvItem, InvItem>(box, item));
                }
                m_curchoice = new SpoilsMenu(goodsList);
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogInfo(String.Format("ScriptUI.SpoilsMenu(): Encountered exception:\n{0}", e.Message));
            }
        }


        public void AppraiseMenu()
        {
            try
            {
                m_choiceresult = LuaEngine.Instance.RunString("return {}").First() as LuaTable;
                m_curchoice = new AppraiseMenu(0, (List<InvSlot> chosenGoods) =>
                {
                    LuaFunction addfn = LuaEngine.Instance.RunString("return function(tbl, val) table.insert(tbl, val) end").First() as LuaFunction;
                    foreach (InvSlot chosenGood in chosenGoods)
                        addfn.Call(m_choiceresult, chosenGood);
                });
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogInfo(String.Format("ScriptUI.AppraiseMenu(): Encountered exception:\n{0}", e.Message));
            }
        }


        public void TutorTeamMenu()
        {
            try
            {
                m_choiceresult = -1;
                m_curchoice = new TutorTeamMenu(-1,
                    (int teamSlot) => { m_choiceresult = teamSlot; },
                    () => { });
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogInfo(String.Format("ScriptUI.TutorTeamMenu(): Encountered exception:\n{0}", e.Message));
            }
        }


        public void RelearnMenu(Character chara)
        {
            try
            {
                m_choiceresult = -1;
                List<int> forgottenSkills = chara.GetRelearnableSkills();
                m_curchoice = new SkillRecallMenu(chara, forgottenSkills.ToArray(),
                (int skillNum) => { m_choiceresult = skillNum; },
                () => { });
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogInfo(String.Format("ScriptUI.RelearnMenu(): Encountered exception:\n{0}", e.Message));
            }
        }


        public void LearnMenu(Character chara, int moveNum)
        {
            try
            {
                m_choiceresult = -1;
                m_curchoice = new SkillReplaceMenu(chara, moveNum,
                        (int slot) => { m_choiceresult = slot; },
                        () => { });
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogInfo(String.Format("ScriptUI.LearnMenu(): Encountered exception:\n{0}", e.Message));
            }
        }


        public void ForgetMenu(Character chara)
        {
            try
            {
                m_choiceresult = -1;
                m_curchoice = new SkillForgetMenu(chara,
                        (int slot) => { m_choiceresult = slot; },
                        () => { });
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogInfo(String.Format("ScriptUI.ForgetMenu(): Encountered exception:\n{0}", e.Message));
            }
        }


        public void ShowPromoteMenu()
        {
            try
            {
                m_choiceresult = -1;
                m_curchoice = new PromoteMenu(-1,
                    (int teamSlot) => { m_choiceresult = teamSlot; },
                    () => { });
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogInfo(String.Format("ScriptUI.ShowPromoteMenu(): Encountered exception:\n{0}", e.Message));
            }
        }


        public void SwapMenu(LuaTable goods, LuaTable prices)
        {
            try
            {
                m_choiceresult = -1;
                List<Tuple<int, int[]>> goodsList = new List<Tuple<int, int[]>>();
                foreach (object key in goods.Keys)
                {
                    LuaTable entry = goods[key] as LuaTable;
                    int item = (int)((Int64)entry["Item"]);
                    List<int> reqs = new List<int>();
                    LuaTable luaReqs = entry["ReqItem"] as LuaTable;
                    foreach (object tradeIn in luaReqs.Values)
                        reqs.Add((int)((Int64)tradeIn));
                    goodsList.Add(new Tuple<int, int[]>(item, reqs.ToArray()));
                }
                List<int> priceList = new List<int>();
                priceList.Add(0);
                foreach (object key in prices.Keys)
                {
                    int price = (int)((Int64)prices[key]);
                    priceList.Add(price);
                }
                m_curchoice = new SwapShopMenu(goodsList, priceList.ToArray(), 0, (chosenGood) =>
                {
                    m_choiceresult = chosenGood+1;
                });
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogInfo(String.Format("ScriptUI.SwapMenu(): Encountered exception:\n{0}", e.Message));
            }
        }

        public void TributeMenu(int spaces)
        {
            try
            {
                m_choiceresult = LuaEngine.Instance.RunString("return {}").First() as LuaTable;
                m_curchoice = new SwapGiveMenu(0, spaces, (List<int> chosenGoods) =>
                {
                    LuaFunction addfn = LuaEngine.Instance.RunString("return function(tbl, val) table.insert(tbl, val) end").First() as LuaFunction;
                    foreach (int chosenGood in chosenGoods)
                        addfn.Call(m_choiceresult, chosenGood);
                });
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogInfo(String.Format("ScriptUI.TributeMenu(): Encountered exception:\n{0}", e.Message));
            }
        }


        public void ShowMusicMenu()
        {
            try
            {
                m_choiceresult = null;
                m_curchoice = new MusicMenu((string dir) => { m_choiceresult = dir; });
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogInfo(String.Format("ScriptUI.ShowMusicMenu(): Encountered exception:\n{0}", e.Message));
            }
        }

        public void DungeonChoice(int dungeonid)
        {
            try
            {
                m_choiceresult = null;
                ZoneData zoneEntry = DataManager.Instance.GetZone(dungeonid);
                DialogueChoice[] choices = new DialogueChoice[2];
                choices[0] = new DialogueChoice(Text.FormatKey("DLG_CHOICE_YES"), () => { m_choiceresult = true; });
                choices[1] = new DialogueChoice(Text.FormatKey("DLG_CHOICE_NO"), () => { m_choiceresult = false; });
                m_curchoice = new DungeonEnterDialog(Text.FormatKey("DLG_ASK_ENTER_DUNGEON", zoneEntry.Name.ToLocal()), dungeonid, false, choices, 0, 1);
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogInfo(String.Format("ScriptUI.DungeonMenu(): Encountered exception:\n{0}", e.Message));
            }
        }

        public void DungeonMenu(LuaTable dungeons, LuaTable grounds)
        {
            try
            {
                List<int> availableDungeons = new List<int>();
                List<Tuple<int, int[]>> goodsList = new List<Tuple<int, int[]>>();
                foreach (object key in dungeons.Keys)
                {
                    int entry = (int)(Int64)dungeons[key];
                    availableDungeons.Add(entry);
                }

                List<ZoneLoc> availableGrounds = new List<ZoneLoc>();
                foreach (object key in grounds.Keys)
                {
                    LuaTable entry = grounds[key] as LuaTable;
                    int zone = (int)(Int64)entry["Zone"];
                    int id = (int)(Int64)entry["ID"];
                    int entryPoint = (int)(Int64)entry["Entry"];
                    availableGrounds.Add(new ZoneLoc(zone, new SegLoc(-1, id), entryPoint));
                }

                //give the player the choice between all the possible dungeons
                m_choiceresult = ZoneLoc.Invalid;
                m_curchoice = new DungeonsMenu(availableDungeons, availableGrounds,
                    (int choice) => { m_choiceresult = new ZoneLoc(availableDungeons[choice], new SegLoc(0, 0)); },
                    (int choice) => {
                        m_choiceresult = new ZoneLoc(availableGrounds[choice].ID,
          new SegLoc(-1, availableGrounds[choice].StructID.ID), availableGrounds[choice].EntryPoint);
                    });
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogInfo(String.Format("ScriptUI.DungeonMenu(): Encountered exception:\n{0}", e.Message));
            }
        }



        public void ServersMenu()
        {
            try
            {
                m_choiceresult = null;
                m_curchoice = new ServersMenu();
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogInfo(String.Format("ScriptUI.ServersMenu(): Encountered exception:\n{0}", e.Message));
            }
        }

        public void ContactsMenu()
        {
            try
            {
                m_choiceresult = 0;
                m_curchoice = new ContactsMenu(false, (OnlineActivity activity) => { m_choiceresult = 1; NetworkManager.Instance.PrepareActivity(activity); });
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogInfo(String.Format("ScriptUI.ContactsMenu(): Encountered exception:\n{0}", e.Message));
            }
        }

        public void SOSMenu()
        {
            try
            {
                m_choiceresult = null;
                m_curchoice = new MailMenu(true, (string fileName) => { m_choiceresult = fileName; });
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogInfo(String.Format("ScriptUI.SOSMenu(): Encountered exception:\n{0}", e.Message));
            }
        }

        public void AOKMenu()
        {
            try
            {
                m_choiceresult = null;
                m_curchoice = new MailMenu(false, (string fileName) => { });
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogInfo(String.Format("ScriptUI.AOKMenu(): Encountered exception:\n{0}", e.Message));
            }
        }

        public void PeersMenu()
        {
            try
            {
                m_choiceresult = 0;
                m_curchoice = new PeersMenu((OnlineActivity activity) => { m_choiceresult = 1; NetworkManager.Instance.PrepareActivity(activity/*, true*/); });
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogInfo(String.Format("ScriptUI.PeersMenu(): Encountered exception:\n{0}", e.Message));
            }
        }

        public void ShowConnectMenu()
        {
            try
            {
                m_choiceresult = 0;
                m_curchoice = new ConnectingMenu(() => { m_choiceresult = 1; });
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogInfo(String.Format("ScriptUI.ConnnectMenu(): Encountered exception:\n{0}", e.Message));
            }
        }

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
                DiagManager.Instance.LogInfo(String.Format("ScriptUI.CurrentActivityMenu(): Encountered exception:\n{0}", e.Message));
            }
        }

        /// <summary>
        /// Marks the start of a multi-choice menu.
        /// Choices must be added after calling this method using the AddChoice() method.
        /// WaitForChoice() must be called afterwards for the menu to be actually displayed,
        /// and for execution to suspend until the choice is returned.
        /// Then to recover the integer value indicating the result of the menu, ChoiceResult() must be called.
        /// </summary>
        /// <param name="message">The question to ask the user.</param>
        public void BeginChoiceMenu(string message, LuaTable choicespairs, object defaultchoice, object cancelchoice)
        {
            try
            {
                m_choiceresult = null;
                int mappedDefault = 0;
                int mappedCancel = 0;
                //Intepret the choices from lua
                List<DialogueChoice> choices = new List<DialogueChoice>();
                IDictionaryEnumerator dict = choicespairs.GetEnumerator();
                while (dict.MoveNext())
                {
                    string choicetext = "";
                    bool enabled = true;
                    if (dict.Value is string)
                        choicetext = dict.Value as string;
                    else if (dict.Value is LuaTable)
                    {
                        LuaTable tbl = dict.Value as LuaTable;
                        choicetext = (string)tbl[1];
                        enabled = (bool)tbl[2];
                    }
                    object choiceval = dict.Key;

                    if (defaultchoice.Equals(choiceval))
                        mappedDefault = choices.Count;
                    if (cancelchoice.Equals(choiceval))
                        mappedCancel = choices.Count;
                    choices.Add(new DialogueChoice(choicetext, () => { m_choiceresult = choiceval; }, enabled));
                }

                //Make a choice menu, and check if we display a speaker or not
                if (m_curspeakerName != null)
                {
                    m_curchoice = MenuManager.Instance.CreateMultiQuestion(m_curspeakerID, m_curspeakerName, m_curspeakerEmo,
                            message, m_curspeakerSnd, choices.ToArray(), mappedDefault, mappedCancel);
                }
                else
                {
                    m_curchoice = MenuManager.Instance.CreateMultiQuestion(message, m_curspeakerSnd, choices, mappedDefault, mappedCancel);
                }
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogInfo(String.Format("ScriptUI.BeginChoiceMenu({0}): Encountered exception:\n{1}", message, e.Message));
            }
        }

        /// <summary>
        /// Get the result of the last choice menu
        /// </summary>
        /// <returns></returns>
        public object ChoiceResult()
        {
            return m_choiceresult;
        }



        /// <summary>
        /// Let the scripts wait for the result of a choice
        /// </summary>
        /// <returns></returns>
        public Coroutine _WaitForChoice()
        {
            if (m_curchoice != null)
                return new Coroutine(MenuManager.Instance.ProcessMenuCoroutine(m_curchoice));
            else
                return new Coroutine(_DummyWait());

        }

        //================================================================
        // Menu Utilities
        //================================================================
        public Coroutine ProcessMenuCoroutine( object menu )
        {
            try
            {
                IInteractable imenu = (IInteractable)menu;
                return new Coroutine(Menu.MenuManager.Instance.ProcessMenuCoroutine(imenu));
            }
            catch(Exception ex)
            {
                DiagManager.Instance.LogInfo("ScriptUI.ProcessMenuCoroutine(): Exception caught: \n" + ex.Message + "\n" + ex.Source + "\n" + ex.StackTrace);
                return null;
            }
        }

        //
        // Function setup
        //
        public override void SetupLuaFunctions(LuaEngine state)
        {
            WaitDialog = state.RunString(@"
            return function(_)
                return coroutine.yield(UI:_WaitDialog())
            end", "WaitDialog").First() as LuaFunction;

            WaitForChoice = state.RunString(@"
            return function(_)
                return coroutine.yield(UI:_WaitForChoice())
            end", "WaitForChoice").First() as LuaFunction;

            WaitShowMonologue = state.RunString(@"
            return function(_, text)
                UI:TextMonologue(text)
                return coroutine.yield(UI:_WaitDialog())
            end", "WaitShowMonologue").First() as LuaFunction;

            WaitShowDialogue = state.RunString(@"
            return function(_, text)
                UI:TextDialogue(text)
                return coroutine.yield(UI:_WaitDialog())
            end", "WaitShowDialogue").First() as LuaFunction;

            WaitShowVoiceOver = state.RunString(@"
            return function(_, text, expiretime)
                UI:TextVoiceOver(text, expiretime)
                return coroutine.yield(UI:_WaitDialog())
            end", "WaitShowVoiceOver").First() as LuaFunction;

            WaitShowTitle = state.RunString(@"
            return function(_, text, time)
                UI:TextShowTitle(text, time)
                return coroutine.yield(UI:_WaitDialog())
            end", "WaitShowTitle").First() as LuaFunction;

            WaitHideTitle = state.RunString(@"
            return function(_, time)
                UI:TextFadeTitle(time)
                return coroutine.yield(UI:_WaitDialog())
            end", "WaitHideTitle").First() as LuaFunction;
        }
    }
}
