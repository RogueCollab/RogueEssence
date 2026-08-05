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
        // Menus
        //================================================================

        /// <summary>
        /// Displays the name input box.
        /// 
        /// UI:WaitForChoice() must be called afterwards for the menu to be actually displayed,
        /// and for execution to suspend until the choice is returned.
        /// Then to retrieve the string value indicating the result of the menu, UI:ChoiceResult() must be called.
        /// </summary>
        /// <param name="title">The text to show above the input line.</param>
        /// <param name="desc">The text to show below the input line.</param>
        /// <param name="maxLength">The length limit of the text in pixels.</param>
        /// <param name="defaultName">Name to start the textbox with.</param>
        public void NameMenu(string title, string desc, int maxLength = 116, string defaultName = "")
        {
            try
            {
                m_choiceresult = "";
                //TODO: allow this to work in dungeon mode by skipping replays
                m_curchoice = new TeamNameMenu(title, desc, maxLength, defaultName, (string name) => { m_choiceresult = name; });
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.NameMenu(): Encountered exception."), e), DiagManager.Instance.DevMode);
            }
        }

        /// <summary>
        /// Displays a menu for replacing party members with the assembly.
        /// 
        /// UI:WaitForChoice() must be called afterwards for the menu to be actually displayed,
        /// and for execution to suspend until the choice is returned.
        /// Then to retrieve the boolean value indicating whether the team composition was changed or not, UI:ChoiceResult() must be called.
        /// </summary>
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
        /// UI:WaitForChoice() must be called afterwards for the menu to be actually displayed,
        /// and for execution to suspend until the choice is returned.
        /// Then to retrieve the table indicating the indices of items chosen, UI:ChoiceResult() must be called.
        /// </summary>
        /// <param name="goods">A table of items to be sold.  The format is { Item=InvItem, Price=int } for each item.</param>
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
                        addfn.Call(m_choiceresult, chosenGood + 1);
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
        /// UI:WaitForChoice() must be called afterwards for the menu to be actually displayed,
        /// and for execution to suspend until the choice is returned.
        /// Then to retrieve the table indicating the indices of items to sell, UI:ChoiceResult() must be called.
        /// </summary>
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

        /// <summary>
        /// Displays the Storage menu for which to exchange items in the inventory with.
        /// 
        /// UI:WaitForChoice() must be called afterwards for the menu to be actually displayed,
        /// and for execution to suspend until the menu is exited.
        /// </summary>
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

        /// <summary>
        /// Displays the Storage menu for which to withdraw from.
        /// 
        /// UI:WaitForChoice() must be called afterwards for the menu to be actually displayed,
        /// and for execution to suspend until the menu is exited.
        /// </summary>
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

        private void onChooseSlot(List<WithdrawSlot> slots)
        {
            //store item
            List<InvItem> items = DataManager.Instance.Save.ActiveTeam.TakeItems(slots);

            foreach (InvItem item in items)
            {
                ItemData entry = DataManager.Instance.GetItem(item.ID);
                if (entry.MaxStack > 1)
                {
                    foreach (InvItem inv in DataManager.Instance.Save.ActiveTeam.EnumerateInv())
                    {
                        if (inv.ID == item.ID && inv.Cursed == item.Cursed && inv.Amount < entry.MaxStack)
                        {
                            int addValue = Math.Min(entry.MaxStack - inv.Amount, item.Amount);
                            inv.Amount += addValue;
                            item.Amount -= addValue;
                            if (item.Amount <= 0)
                                break;
                        }
                    }
                    //after this point, may be still some stacks left to take care of
                    if (item.Amount <= 0)
                        continue;
                }
                DataManager.Instance.Save.ActiveTeam.AddToInv(item);
            }
        }


        /// <summary>
        /// Displays the Bank menu.
        /// 
        /// UI:WaitForChoice() must be called afterwards for the menu to be actually displayed,
        /// and for execution to suspend until the menu is exited.
        /// </summary>
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

        /// <summary>
        /// Displays the Spoils menu.
        /// 
        /// UI:WaitForChoice() must be called afterwards for the menu to be actually displayed,
        /// and for execution to suspend until the menu is exited.
        /// </summary>
        /// <param name="appraisalMap">A table of mappings from containers to items, in the format of { Box=InvItem , Item=InvItem }</param>
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

        /// <summary>
        /// Displays the Appraisal menu.
        /// 
        /// UI:WaitForChoice() must be called afterwards for the menu to be actually displayed,
        /// and for execution to suspend until the choice is returned.
        /// Then to retrieve the table indicating the indices of items chosen, UI:ChoiceResult() must be called.
        /// </summary>
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

        /// <summary>
        /// Displays the Tutor Team menu.
        /// 
        /// UI:WaitForChoice() must be called afterwards for the menu to be actually displayed,
        /// and for execution to suspend until the choice is returned.
        /// Then to retrieve the integer representing the chosen team member, UI:ChoiceResult() must be called.
        /// </summary>
        public void TutorTeamMenu(LuaFunction eligibleCheck = null)
        {
            if (DataManager.Instance.CurrentReplay != null)
            {
                m_choiceresult = DataManager.Instance.CurrentReplay.ReadUI();
                return;
            }

            try
            {
                bool isEligible(Character chara)
                {
                    if (eligibleCheck == null)
                        return true;
                    return (bool)eligibleCheck.Call(chara)[0];
                }
                ;

                //yields = LuaEngine.Instance.CallScriptFunction(luaFun);
                m_choiceresult = -1;
                m_curchoice = new TutorTeamMenu(-1, isEligible,
                    (int teamSlot) => { m_choiceresult = teamSlot; DataManager.Instance.LogUIPlay(teamSlot); },
                    () => { DataManager.Instance.LogUIPlay(-1); });
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.TutorTeamMenu(): Encountered exception."), e), DiagManager.Instance.DevMode);
            }
        }

        /// <summary>
        /// Displays the Relearn menu for a character.
        /// 
        /// UI:WaitForChoice() must be called afterwards for the menu to be actually displayed,
        /// and for execution to suspend until the choice is returned.
        /// Then to retrieve the integer representing the chosen skill, UI:ChoiceResult() must be called.
        /// </summary>
        /// <param name="chara">The character to relearn skills</param>
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

        /// <summary>
        /// Displays the Learn menu for a character to replace an existing skill with a new one.
        /// 
        /// UI:WaitForChoice() must be called afterwards for the menu to be actually displayed,
        /// and for execution to suspend until the choice is returned.
        /// Then to retrieve the integer representing the chosen skill, UI:ChoiceResult() must be called.
        /// </summary>
        /// <param name="chara">The character to relearn skills</param>
        /// <param name="skillNum">The new skill</param>
        public void LearnMenu(Character chara, string skillNum)
        {
            if (DataManager.Instance.CurrentReplay != null)
            {
                m_choiceresult = DataManager.Instance.CurrentReplay.ReadUI();
                return;
            }

            try
            {
                m_choiceresult = -1;
                m_curchoice = new SkillReplaceMenu(chara, skillNum,
                        (int slot) => { m_choiceresult = slot; DataManager.Instance.LogUIPlay(slot); },
                        () => { DataManager.Instance.LogUIPlay(-1); });
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.LearnMenu(): Encountered exception."), e), DiagManager.Instance.DevMode);
            }
        }

        /// <summary>
        /// Displays the Forget menu for a character to forget a skill.
        /// 
        /// UI:WaitForChoice() must be called afterwards for the menu to be actually displayed,
        /// and for execution to suspend until the choice is returned.
        /// Then to retrieve the integer representing the chosen skill, UI:ChoiceResult() must be called.
        /// </summary>
        /// <param name="chara">The character to relearn skills</param>
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

        /// <summary>
        /// Displays the Promote menu to choose a team member to promote.
        /// 
        /// UI:WaitForChoice() must be called afterwards for the menu to be actually displayed,
        /// and for execution to suspend until the choice is returned.
        /// Then to retrieve the integer representing the chosen team slot, UI:ChoiceResult() must be called.
        /// </summary>
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

        /// <summary>
        /// Determines if the swap menu has any valid items to display.
        /// </summary>
        /// <param name="goods">The list of goods to filter for valid items.</param>
        /// <returns>True if there is at least one item that can be displayed, false otherwise.</returns>
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

        /// <summary>
        /// Displays the swap menu with a table of goods and prices.
        /// </summary>
        /// <param name="goods"></param>
        /// <param name="prices"></param>
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
                    m_choiceresult = chosenGood + 1;
                });
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.SwapMenu(): Encountered exception."), e), DiagManager.Instance.DevMode);
            }
        }

        /// <summary>
        /// Displays the tribute menu.
        /// </summary>
        /// <param name="spaces"></param>
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

        /// <summary>
        /// Displays the Music menu to browse music for the game.
        /// 
        /// UI:WaitForChoice() must be called afterwards for the menu to be actually displayed,
        /// and for execution to suspend until the choice is returned.
        /// Then to retrieve the string representing the chosen song, UI:ChoiceResult() must be called.
        /// </summary>
        /// <param name="hardMod">Set to true if you want to only include music of the current quest mod.</param>
        /// <param name="spoilerUnlocks">A lua table of strings representing progression flags that have been completed.
        /// Any ogg file that uses this tag as a spoiler tag will display in the menu only if the flag has been passed.</param>
        public void ShowMusicMenu(bool hardMod, LuaTable spoilerUnlocks)
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
                m_curchoice = new MusicMenu(hardMod, unlockedTags, (string dir) => { m_choiceresult = dir; });
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.ShowMusicMenu(): Encountered exception."), e), DiagManager.Instance.DevMode);
            }
        }

        /// <summary>
        /// Ask to enter a destintion via character dialogue to the player.
        /// UI:WaitForChoice() must be called afterwards for the menu to be actually displayed,
        /// and for execution to suspend until the choice is returned.
        /// Then to retrieve the integer value indicating the result of the menu, UI:ChoiceResult() must be called.
        ///
        /// The Yes/No menu returns 1 for yes, and 0 for no.
        /// </summary>
        /// <param name="name">Name of the destination</param>
        /// <param name="dest">The ZoneLoc location of the destination.</param>
        public void DungeonChoice(string name, ZoneLoc dest)
        {
            try
            {
                m_choiceresult = null;
                ZoneData zoneEntry = DataManager.Instance.GetZone(dest.ID);
                DialogueChoice[] choices = new DialogueChoice[2];
                choices[0] = new DialogueChoice(Text.FormatKey("DLG_CHOICE_YES"), () => { m_choiceresult = true; });
                choices[1] = new DialogueChoice(Text.FormatKey("DLG_CHOICE_NO"), () => { m_choiceresult = false; });
                m_curchoice = new DungeonEnterDialog(Text.FormatKey("DLG_ASK_ENTER_DUNGEON", name), name, dest, false, choices, 0, 1);
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.DungeonMenu(): Encountered exception."), e), DiagManager.Instance.DevMode);
            }
        }

        /// <summary>
        /// Marks the start of a choice menu for choosing destinations, showing a preview of restrictions and requirements for dungeons.
        /// UI:WaitForChoice() must be called afterwards for the menu to be actually displayed,
        /// and for execution to suspend until the choice is returned.
        /// Then to retrieve the ZoneLoc indicating the chosen destination, UI:ChoiceResult() must be called.
        /// </summary>
        /// <param name="destinations">A lua table representing the list of destinations with each element in the format of { Name=string, Dest=ZoneLoc }</param>
        public void DestinationMenu(LuaTable destinations, object defaultChoice)
        {
            try
            {
                int? mappedDefault = null;

                List<string> names = new List<string>();
                List<string> titles = new List<string>();
                List<ZoneLoc> dests = new List<ZoneLoc>();
                List<object> keys = new List<object>();
                foreach (object key in destinations.Keys)
                {
                    LuaTable entry = destinations[key] as LuaTable;
                    string name = (string)entry["Name"];
                    string title = entry["Title"] != null ? (string)entry["Title"] : name;
                    ZoneLoc item = (ZoneLoc)entry["Dest"];

                    if (defaultChoice.Equals(key))
                        mappedDefault = names.Count;

                    names.Add(name);
                    titles.Add(title);
                    dests.Add(item);
                    keys.Add(key);
                }

                if (mappedDefault == null)
                    mappedDefault = 0;

                //give the player the choice between all the possible dungeons
                m_choiceresult = null;
                m_curchoice = new DungeonsMenu(names, titles, dests, mappedDefault.Value,
                    (int choice) => { m_choiceresult = keys[choice]; });
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(new Exception(String.Format("ScriptUI.DungeonMenu(): Encountered exception."), e), DiagManager.Instance.DevMode);
            }
        }


    }
}
