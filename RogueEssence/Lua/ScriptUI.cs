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
        private MonsterID       m_curspeakerID = MonsterID.Invalid;
        private string              m_curspeakerName= "";
        private bool m_curcenter_h = false;
        private bool m_curcenter_v = false;
        private bool m_curautoFinish = false;
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
        public LuaFunction WaitForChoice;
        public LuaFunction WaitDialog;
        public LuaFunction WaitShowDialogue;
        public LuaFunction WaitShowTimedDialogue;
        public LuaFunction WaitShowVoiceOver;
        public LuaFunction WaitInput;
        public LuaFunction WaitShowTitle;
        public LuaFunction WaitHideTitle;
        public LuaFunction WaitShowBG;
        public LuaFunction WaitHideBG;
        

        //================================================================
        // Dialogue
        //================================================================



        public void TextDialogue(string text, int waitTime = -1)
        {
            try
            {
                if (DataManager.Instance.CurrentReplay == null)
                    m_curdialogue = MenuManager.Instance.SetDialogue(m_curspeakerID, m_curspeakerName, m_curspeakerEmo, m_curspeakerSnd, () => { }, waitTime, m_curautoFinish, m_curcenter_h, m_curcenter_v, new string[] { text });
                else
                {
                    if (!String.IsNullOrEmpty(m_curspeakerName))
                        DungeonScene.Instance.LogMsg(String.Format("{0}: {1}", m_curspeakerName, text));
                    else
                        DungeonScene.Instance.LogMsg(text);
                }
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.TextDialogue({0}): Encountered exception.", text), e), DiagManager.Instance.DevMode);
            }
        }

        public void TextDialogue(string text, LuaTable callbacks, int waitTime)
        {
            //TODO: support mid-menu script callbacks
            try
            {
                if (DataManager.Instance.CurrentReplay == null)
                    m_curdialogue = MenuManager.Instance.SetDialogue(m_curspeakerID, m_curspeakerName, m_curspeakerEmo, m_curspeakerSnd, () => { }, waitTime, m_curautoFinish, m_curcenter_h, m_curcenter_v, new string[] { text });
                else
                {
                    if (!String.IsNullOrEmpty(m_curspeakerName))
                        DungeonScene.Instance.LogMsg(String.Format("{0}: {1}", m_curspeakerName, text));
                    else
                        DungeonScene.Instance.LogMsg(text);
                }
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.TextDialogue({0}): Encountered exception.", text), e), DiagManager.Instance.DevMode);
            }
        }


        public void TextVoiceOver(string text, int expireTime)
        {
            try
            {
                if (DataManager.Instance.CurrentReplay == null)
                    m_curdialogue = MenuManager.Instance.SetTitleDialog(expireTime, m_curautoFinish, () => { }, text);
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.TextVoiceOver({0}, {1}): Encountered exception", text, expireTime), e), DiagManager.Instance.DevMode);
            }
        }

        public void TextWaitMenu(bool anyInput)
        {
            try
            {
                if (DataManager.Instance.CurrentReplay == null)
                    m_curdialogue = MenuManager.Instance.SetWaitMenu(anyInput);
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.TextWaitMenu({0}): Encountered exception", anyInput), e), DiagManager.Instance.DevMode);
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
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.TextWaitMenu({0}, {1}): Encountered exception", text, time), e), DiagManager.Instance.DevMode);
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
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.TextFadeTitle({0}, {1}): Encountered exception", time), e), DiagManager.Instance.DevMode);
            }
        }



        public void ShowBG(string bg, int frameTime, int fadeInTime)
        {
            try
            {
                m_curdialogue = GameManager.Instance.FadeBG(true, new BGAnimData(bg, frameTime), fadeInTime);
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.TextShowBG({0}, {1}, {2}): Encountered exception", bg, frameTime, fadeInTime), e), DiagManager.Instance.DevMode);
            }
        }


        public void FadeBG(int time)
        {
            try
            {
                m_curdialogue = GameManager.Instance.FadeBG(false, new BGAnimData(), time);
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.ShowBG({0}): Encountered exception", time), e), DiagManager.Instance.DevMode);
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
            m_curautoFinish = false;
            m_curcenter_h = false;
            m_curcenter_v = false;
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
        public void SetSpeaker(string name, bool keysound, string specie, int form, string skin, Gender gender)
        {
            m_curspeakerID = new MonsterID(specie, form, skin, gender);
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
                m_curspeakerID = MonsterID.Invalid;
                m_curspeakerName = null;
            }

            m_curspeakerEmo.Emote = 0;
            m_curspeakerSnd = keysound;
        }
        public void SetSpeaker(Character chara, bool keysound = true)
        {
            if (chara != null)
            {
                m_curspeakerID = chara.CurrentForm;
                m_curspeakerName = chara.GetDisplayName(true);
            }
            else
            {
                m_curspeakerID = MonsterID.Invalid;
                m_curspeakerName = null;
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

        public void SetCenter(bool centerH, bool centerV = false)
        {
            m_curcenter_h = centerH;
            m_curcenter_v = centerV;
        }

        public void SetAutoFinish(bool autoFinish)
        {
            m_curautoFinish = autoFinish;
        }


        /// <summary>
        /// Waits for the dialog window to be closed to return control to the script!
        /// </summary>
        public Coroutine _WaitDialog()
        {
            if (DataManager.Instance.CurrentReplay != null)
                return new Coroutine(_DummyWait());

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
        public void ChoiceMenuYesNo( string message, bool bdefaultstono = false)
        {
            if (DataManager.Instance.CurrentReplay != null)
            {
                m_choiceresult = DataManager.Instance.CurrentReplay.ReadUI() == 0 ? false : true;
                return;
            }

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
                                                                      m_curspeakerSnd, m_curautoFinish, m_curcenter_h, m_curcenter_v,
                                                                      () => { m_choiceresult = true; DataManager.Instance.LogUIPlay(1); },
                                                                      () => { m_choiceresult = false; DataManager.Instance.LogUIPlay(0); },
                                                                      bdefaultstono);
                }
                else
                {
                    m_curchoice = MenuManager.Instance.CreateQuestion(MonsterID.Invalid, null, new EmoteStyle(0), message,
                        m_curspeakerSnd, false, m_curcenter_h, m_curcenter_v,
                        () => { m_choiceresult = true; DataManager.Instance.LogUIPlay(1); },
                        () => { m_choiceresult = false; DataManager.Instance.LogUIPlay(0); }, bdefaultstono);
                }
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.ChoiceMenuYesNo({0}): Encountered exception.", message), e), DiagManager.Instance.DevMode);
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
        public void NameMenu(string title, string desc, int maxLength = 116)
        {
            try
            {
                m_choiceresult = "";
                //TODO: allow this to work in dungeon mode by skipping replays
                m_curchoice = new TeamNameMenu(title, desc, maxLength, (string name) => { m_choiceresult = name; });
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.NameMenu(): Encountered exception."), e), DiagManager.Instance.DevMode);
            }
        }

        public void AssemblyMenu()
        {
            try
            {
                m_choiceresult = false;
                //TODO: allow this to work in dungeon mode by skipping replays
                m_curchoice = new AssemblyMenu(0, () => { m_choiceresult = true; });
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.AssemblyMenu(): Encountered exception."), e), DiagManager.Instance.DevMode);
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
                //TODO: allow this to work in dungeon mode by skipping replays
                m_curchoice = new ShopMenu(goodsList, 0, (List<int> chosenGoods) =>
                {
                    LuaFunction addfn = LuaEngine.Instance.RunString("return function(tbl, val) table.insert(tbl, val) end").First() as LuaFunction;
                    foreach (int chosenGood in chosenGoods)
                        addfn.Call(m_choiceresult, chosenGood+1);
                });
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.ShopMenu(): Encountered exception."), e), DiagManager.Instance.DevMode);
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
                //TODO: allow this to work in dungeon mode by skipping replays
                m_curchoice = new SellMenu(0, (List<InvSlot> chosenGoods) =>
                {
                    LuaFunction addfn = LuaEngine.Instance.RunString("return function(tbl, val) table.insert(tbl, val) end").First() as LuaFunction;
                    foreach (InvSlot chosenGood in chosenGoods)
                        addfn.Call(m_choiceresult, chosenGood);
                });
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.SellMenu(): Encountered exception."), e), DiagManager.Instance.DevMode);
            }
        }

        public void StorageMenu()
        {
            try
            {
                m_choiceresult = null;
                //TODO: allow this to work in dungeon mode by skipping replays
                m_curchoice = new DepositMenu(0);
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.StorageMenu(): Encountered exception."), e), DiagManager.Instance.DevMode);
            }
        }

        public void WithdrawMenu()
        {
            try
            {
                m_choiceresult = null;
                //TODO: allow this to work in dungeon mode by skipping replays
                m_curchoice = new WithdrawMenu(0, true, onChooseSlot);
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.WithdrawMenu(): Encountered exception."), e), DiagManager.Instance.DevMode);
            }
        }

        public void DepositAll() {
            List<InvItem> items = new List<InvItem>();
            int item_count = DataManager.Instance.Save.ActiveTeam.GetInvCount();

            // Get list from held items
            foreach (Character player in DataManager.Instance.Save.ActiveTeam.Players)
            {
                if (!String.IsNullOrEmpty(player.EquippedItem.ID))
                    items.Add(player.EquippedItem);
            }

            for (int ii = 0; ii < item_count; ii++) {
                // Get a list of inventory items.
                InvItem item = DataManager.Instance.Save.ActiveTeam.GetInv(ii);
                items.Add(item);
            };

            // Store all items in the inventory.
            DataManager.Instance.Save.ActiveTeam.StoreItems(items);

            // Remove held items
            foreach (Character player in DataManager.Instance.Save.ActiveTeam.Players)
            {
                if (!String.IsNullOrEmpty(player.EquippedItem.ID))
                    player.DequipItem();
            }

            // Remove the items back to front to prevent removing them in the wrong order.
            for (int ii = DataManager.Instance.Save.ActiveTeam.GetInvCount() - 1; ii >= 0; ii--) {
                DataManager.Instance.Save.ActiveTeam.RemoveFromInv(ii);
            }
        }

        private void onChooseSlot(List<WithdrawSlot> slots)
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
                        if (DataManager.Instance.Save.ActiveTeam.GetInv(jj).ID == item.ID && DataManager.Instance.Save.ActiveTeam.GetInv(jj).Amount < entry.MaxStack)
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
                //TODO: allow this to work in dungeon mode by skipping replays
                m_curchoice = new BankMenu(DataManager.Instance.Save.ActiveTeam.Money, onChooseAmount);
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.BankMenu(): Encountered exception."), e), DiagManager.Instance.DevMode);
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
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.SpoilsMenu(): Encountered exception."), e), DiagManager.Instance.DevMode);
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
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.AppraiseMenu(): Encountered exception."), e), DiagManager.Instance.DevMode);
            }
        }


        public void TutorTeamMenu()
        {
            if (DataManager.Instance.CurrentReplay != null)
            {
                m_choiceresult = DataManager.Instance.CurrentReplay.ReadUI();
                return;
            }

            try
            {
                m_choiceresult = -1;
                //TODO: allow this to work in dungeon mode by skipping replays
                m_curchoice = new TutorTeamMenu(-1,
                    (int teamSlot) => { m_choiceresult = teamSlot; DataManager.Instance.LogUIPlay(teamSlot); },
                    () => { DataManager.Instance.LogUIPlay(-1); });
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.TutorTeamMenu(): Encountered exception."), e), DiagManager.Instance.DevMode);
            }
        }


        public void RelearnMenu(Character chara)
        {

            try
            {
                List<string> forgottenSkills = chara.GetRelearnableSkills(true);

                if (DataManager.Instance.CurrentReplay != null)
                {
                    m_choiceresult = forgottenSkills[DataManager.Instance.CurrentReplay.ReadUI()];
                    return;
                }

                m_choiceresult = "";
                m_curchoice = new SkillRecallMenu(chara, forgottenSkills.ToArray(),
                (int skillSlot) => { m_choiceresult = forgottenSkills[skillSlot]; DataManager.Instance.LogUIPlay(skillSlot); },
                () => { DataManager.Instance.LogUIPlay(-1); });
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.RelearnMenu(): Encountered exception."), e), DiagManager.Instance.DevMode);
            }
        }


        public void LearnMenu(Character chara, string moveNum)
        {
            if (DataManager.Instance.CurrentReplay != null)
            {
                m_choiceresult = DataManager.Instance.CurrentReplay.ReadUI();
                return;
            }

            try
            {
                m_choiceresult = -1;
                m_curchoice = new SkillReplaceMenu(chara, moveNum,
                        (int slot) => { m_choiceresult = slot; DataManager.Instance.LogUIPlay(slot); },
                        () => { DataManager.Instance.LogUIPlay(-1); });
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.LearnMenu(): Encountered exception."), e), DiagManager.Instance.DevMode);
            }
        }


        public void ForgetMenu(Character chara)
        {
            if (DataManager.Instance.CurrentReplay != null)
            {
                m_choiceresult = DataManager.Instance.CurrentReplay.ReadUI();
                return;
            }

            try
            {
                m_choiceresult = -1;
                m_curchoice = new SkillForgetMenu(chara,
                        (int slot) => { m_choiceresult = slot; DataManager.Instance.LogUIPlay(slot); },
                        () => { DataManager.Instance.LogUIPlay(-1); });
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.ForgetMenu(): Encountered exception."), e), DiagManager.Instance.DevMode);
            }
        }


        public void ShowPromoteMenu()
        {
            try
            {
                m_choiceresult = -1;
                //TODO: allow this to work in dungeon mode by skipping replays
                m_curchoice = new PromoteMenu(-1,
                    (int teamSlot) => { m_choiceresult = teamSlot; },
                    () => { });
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.ShowPromoteMenu(): Encountered exception."), e), DiagManager.Instance.DevMode);
            }
        }


        public bool CanSwapMenu(LuaTable goods)
        {
            List<Tuple<string, string[]>> goodsList = new List<Tuple<string, string[]>>();
            foreach (object key in goods.Keys)
            {
                LuaTable entry = goods[key] as LuaTable;
                string item = (string)entry["Item"];
                List<string> reqs = new List<string>();
                LuaTable luaReqs = entry["ReqItem"] as LuaTable;
                foreach (object tradeIn in luaReqs.Values)
                    reqs.Add((string)tradeIn);
                goodsList.Add(new Tuple<string, string[]>(item, reqs.ToArray()));
            }
            return SwapShopMenu.CanView(goodsList);
        }

        public void SwapMenu(LuaTable goods, LuaTable prices)
        {
            try
            {
                m_choiceresult = -1;
                List<Tuple<string, string[]>> goodsList = new List<Tuple<string, string[]>>();
                foreach (object key in goods.Keys)
                {
                    LuaTable entry = goods[key] as LuaTable;
                    string item = (string)entry["Item"];
                    List<string> reqs = new List<string>();
                    LuaTable luaReqs = entry["ReqItem"] as LuaTable;
                    foreach (object tradeIn in luaReqs.Values)
                        reqs.Add((string)tradeIn);
                    goodsList.Add(new Tuple<string, string[]>(item, reqs.ToArray()));
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
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.SwapMenu(): Encountered exception."), e), DiagManager.Instance.DevMode);
            }
        }

        public void TributeMenu(int spaces)
        {
            try
            {
                m_choiceresult = LuaEngine.Instance.RunString("return {}").First() as LuaTable;

                SwapGiveMenu menu = null;
                menu = new SwapGiveMenu(0, spaces, (List<int> chosenGoods) =>
                {
                    LuaFunction addfn = LuaEngine.Instance.RunString("return function(tbl, val) table.insert(tbl, val) end").First() as LuaFunction;
                    foreach (int chosenGood in chosenGoods)
                        addfn.Call(m_choiceresult, menu.AllowedGoods[chosenGood]);
                });
                m_curchoice = menu;
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.TributeMenu(): Encountered exception."), e), DiagManager.Instance.DevMode);
            }
        }


        public void ShowMusicMenu(LuaTable spoilerUnlocks)
        {
            try
     {
                List<string> unlockedTags = new List<string>();
                foreach (object key in spoilerUnlocks.Keys)
                {
                    string entry = (string)spoilerUnlocks[key];
                    unlockedTags.Add(entry);
                }

                m_choiceresult = null;
                m_curchoice = new MusicMenu(unlockedTags, (string dir) => { m_choiceresult = dir; });
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.ShowMusicMenu(): Encountered exception."), e), DiagManager.Instance.DevMode);
            }
        }

        public void DungeonChoice(string name, ZoneLoc dest)
        {
            try
            {
                m_choiceresult = null;
                ZoneData zoneEntry = DataManager.Instance.GetZone(dest.ID);
                DialogueChoice[] choices = new DialogueChoice[2];
                choices[0] = new DialogueChoice(Text.FormatKey("DLG_CHOICE_YES"), () => { m_choiceresult = true; });
                choices[1] = new DialogueChoice(Text.FormatKey("DLG_CHOICE_NO"), () => { m_choiceresult = false; });
                m_curchoice = new DungeonEnterDialog(Text.FormatKey("DLG_ASK_ENTER_DUNGEON", name), dest, false, choices, 0, 1);
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.DungeonMenu(): Encountered exception."), e), DiagManager.Instance.DevMode);
            }
        }

        public void DestinationMenu(LuaTable destinations)
        {
            try
            {
                List<string> names = new List<string>();
                List<ZoneLoc> dests = new List<ZoneLoc>();
                foreach (object key in destinations.Keys)
                {
                    LuaTable entry = destinations[key] as LuaTable;
                    string name = (string)entry["Name"];
                    ZoneLoc item = (ZoneLoc)entry["Dest"];
                    names.Add(name);
                    dests.Add(item);
                }

                //give the player the choice between all the possible dungeons
                m_choiceresult = ZoneLoc.Invalid;
                m_curchoice = new DungeonsMenu(names, dests,
                    (int choice) => { m_choiceresult = dests[choice]; });
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.DungeonMenu(): Encountered exception."), e), DiagManager.Instance.DevMode);
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
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.ServersMenu(): Encountered exception."), e), DiagManager.Instance.DevMode);
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
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.ContactsMenu(): Encountered exception."), e), DiagManager.Instance.DevMode);
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
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.SOSMenu(): Encountered exception."), e), DiagManager.Instance.DevMode);
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
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.AOKMenu(): Encountered exception."), e), DiagManager.Instance.DevMode);
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
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.PeersMenu(): Encountered exception."), e), DiagManager.Instance.DevMode);
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
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.ConnnectMenu(): Encountered exception."), e), DiagManager.Instance.DevMode);
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
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.CurrentActivityMenu(): Encountered exception."), e), DiagManager.Instance.DevMode);
            }
        }

        public void ChooseMonsterMenu(string title, LuaTable choices, bool canMenu = false, bool canCancel = false)
        {
            try
            {
                m_choiceresult = null;

                List<(MonsterID mon, string name)> monsters = new();

                for (int ii = 1; choices[ii] is not null; ii++)
                {
                    var choice = choices[ii];
                    if (choice is MonsterID monster)
                        monsters.Add((monster, ""));
                    else
                        throw new ArgumentException($"Table must be array of '{nameof(MonsterID)}'", nameof(choices));
                }

                if (monsters.Count == 0)
                    throw new ArgumentException($"Table must be array of one or more '{nameof(MonsterID)}'", nameof(choices));

                void chooseAction(int slot)
                {
                    m_choiceresult = monsters[slot].mon;
                    MenuManager.Instance.RemoveMenu();
                }

                void cancelAction() { }

                m_curchoice = new ChooseMonsterMenu(title, monsters, 0, chooseAction, canCancel ? cancelAction : null, canMenu);
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.ChooseMonsterMenu(): Encountered exception."), e), DiagManager.Instance.DevMode);
            }
        }



        public void SetCustomMenu(InteractableMenu menu)
        {
            try
            {
                m_choiceresult = null;
                m_curchoice = menu;
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.SetCustomMenu(): Encountered exception."), e), DiagManager.Instance.DevMode);
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
        /// <param name="choicesPairs"></param>
        /// <param name="defaultChoice"></param>
        /// <param name="cancelChoice"></param>
        public void BeginChoiceMenu(string message, LuaTable choicesPairs, object defaultChoice, object cancelChoice)
        {
            if (DataManager.Instance.CurrentReplay != null)
            {
                m_choiceresult = DataManager.Instance.CurrentReplay.ReadUI();
                return;
            }

            try
            {
                m_choiceresult = null;
                int? mappedDefault = null;
                int? mappedCancel = null;
                //Intepret the choices from lua
                List<DialogueChoice> choices = new List<DialogueChoice>();
                IDictionaryEnumerator dict = choicesPairs.GetEnumerator();
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
                    long choiceval = (long)dict.Key;

                    if (defaultChoice.Equals(choiceval))
                        mappedDefault = choices.Count;
                    if (cancelChoice.Equals(choiceval))
                        mappedCancel = choices.Count;
                    choices.Add(new DialogueChoice(choicetext, () => { m_choiceresult = choiceval; DataManager.Instance.LogUIPlay((int)choiceval); }, enabled));
                }

                if (mappedDefault == null)
                    mappedDefault = 0;
                if (mappedCancel == null)
                    mappedCancel = -1;

                //Make a choice menu, and check if we display a speaker or not
                if (m_curspeakerName != null)
                {
                    m_curchoice = MenuManager.Instance.CreateMultiQuestion(m_curspeakerID, m_curspeakerName, m_curspeakerEmo,
                            message, m_curspeakerSnd, m_curautoFinish, m_curcenter_h, m_curcenter_v, choices.ToArray(), mappedDefault.Value, mappedCancel.Value);
                }
                else
                {
                    m_curchoice = MenuManager.Instance.CreateMultiQuestion(message, m_curspeakerSnd, choices, mappedDefault.Value, mappedCancel.Value);
                }
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.BeginChoiceMenu({0}): Encountered exception.", message), e), DiagManager.Instance.DevMode);
            }
        }


        public void BeginMultiPageMenu(int x, int y, int width, string title, LuaTable choicesPairs, int linesPerPage, object defaultChoice, object cancelChoice)
        {
            if (DataManager.Instance.CurrentReplay != null)
            {
                m_choiceresult = DataManager.Instance.CurrentReplay.ReadUI();
                return;
            }

            try
            {
                m_choiceresult = null;
                int? mappedDefault = null;
                int? mappedCancel = null;
                //Intepret the choices from lua
                List<MenuTextChoice> choices = new List<MenuTextChoice>();
                IDictionaryEnumerator dict = choicesPairs.GetEnumerator();
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
                    long choiceval = (long)dict.Key;

                    if (defaultChoice.Equals(choiceval))
                        mappedDefault = choices.Count;
                    if (cancelChoice.Equals(choiceval))
                        mappedCancel = choices.Count;

                    choices.Add(new MenuTextChoice(choicetext, () => { MenuManager.Instance.RemoveMenu(); m_choiceresult = choiceval; DataManager.Instance.LogUIPlay((int)choiceval); }, enabled, enabled ? Color.White : Color.Red));
                }

                if (mappedDefault == null)
                    mappedDefault = 0;

                Action cancelAction = null;
                if (mappedCancel != null)
                    cancelAction = () => { MenuManager.Instance.RemoveMenu(); m_choiceresult = (long)cancelChoice; DataManager.Instance.LogUIPlay((int)(long)cancelChoice); };

                //Make a choice menu, and check if we display a speaker or not
                m_curchoice = new CustomMultiPageMenu(new RogueElements.Loc(x, y), width, title, choices.ToArray(), mappedDefault.Value, linesPerPage, cancelAction, null);
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.BeginMultiPageMenu({0}): Encountered exception.", title), e), DiagManager.Instance.DevMode);
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

        public Action GetChoiceAction(object obj)
        {
            return () =>
            {
                MenuManager.Instance.RemoveMenu();
                m_choiceresult = obj;
            };
        }


        /// <summary>
        /// Let the scripts wait for the result of a choice
        /// </summary>
        /// <returns></returns>
        public Coroutine _WaitForChoice()
        {
            if (DataManager.Instance.CurrentReplay != null)
                return new Coroutine(_DummyWait());

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
                return new Coroutine(MenuManager.Instance.ProcessMenuCoroutine(imenu));
            }
            catch(Exception ex)
            {
                DiagManager.Instance.LogError(ex);
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

            WaitShowDialogue = state.RunString(@"
            return function(_, text)
                UI:TextDialogue(text)
                return coroutine.yield(UI:_WaitDialog())
            end", "WaitShowDialogue").First() as LuaFunction;

            WaitShowTimedDialogue = state.RunString(@"
            return function(_, text, time)
                UI:TextDialogue(text, time)
                return coroutine.yield(UI:_WaitDialog())
            end", "WaitShowDialogue").First() as LuaFunction;

            WaitShowVoiceOver = state.RunString(@"
            return function(_, text, expiretime)
                UI:TextVoiceOver(text, expiretime)
                return coroutine.yield(UI:_WaitDialog())
            end", "WaitShowVoiceOver").First() as LuaFunction;

            WaitInput = state.RunString(@"
            return function(_, any)
                UI:TextWaitMenu(any)
                return coroutine.yield(UI:_WaitDialog())
            end", "WaitInput").First() as LuaFunction;

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

            WaitShowBG = state.RunString(@"
            return function(_, text, frameTime, fadeInTime)
                UI:ShowBG(text, frameTime, fadeInTime)
                return coroutine.yield(UI:_WaitDialog())
            end", "WaitShowBG").First() as LuaFunction;

            WaitHideBG = state.RunString(@"
            return function(_, time)
                UI:FadeBG(time)
                return coroutine.yield(UI:_WaitDialog())
            end", "WaitHideBG").First() as LuaFunction;
        }
    }
}
